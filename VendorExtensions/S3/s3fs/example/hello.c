/* TODO */
/* much more error checking */

/*
    FUSE: Filesystem in Userspace
    Copyright (C) 2001-2006  Miklos Szeredi <miklos@szeredi.hu>

    This program can be distributed under the terms of the GNU GPL.
    See the file COPYING.
*/

#include <fuse.h>
#include <stdlib.h>
#include <unistd.h>
#include <stdio.h>
#include <string.h>
#include <errno.h>
#include <fcntl.h>
#include <assert.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <utime.h>
#include <sys/wait.h>
#include <curl/curl.h>
#include <zlib.h>
#include <openssl/bio.h>
#include <openssl/evp.h>
#include <openssl/hmac.h>


// define size of the s3 diskdrive
//#define EXT3SIZE 0xFFFFFF		/* 16Mb */
#define EXT3SIZE 0xFFFFFFF		/* 256Mb */
//#define EXT3SIZE 0xFFFFFFFF		/* 4Gb */
//#define EXT3SIZE 0xFFFFFFFFF		/* 64Gb */
//#define EXT3SIZE 0x4FFFFFFFFF		/* 320Gb */
//#define EXT3SIZE 0xFFFFFFFFFF		/* 1Tb */
//#define EXT3SIZE 0xFFFFFFFFFFF	/* 16Tb */
//#define EXT3SIZE 0xFFFFFFFFFFFF	/* 256Tb */
//#define EXT3SIZE 0xFFFFFFFFFFFFF	/* 4Pb */
//#define EXT3SIZE 0xFFFFFFFFFFFFFF	/*64Pb */
//#define EXT3SIZE 0xFFFFFFFFFFFFFFF	/*1Eb */

//#define BLKSIZE	4096		/* max size of read/write in fuse */
#define BLKSIZE 1048576		/* 1mb block size */
#define CHUNK BLKSIZE+1000	/* for decompression */
#define LOOKAHEAD 3		/* blocks to read ahead sequentially */
#define YES 1
#define NO 0
#define SLEEPTRY 1	/* seconds to sleep between tries */
#define MAXTRIES 20

static const char *hello_path = "/S3DISK";
static const char *tmpdir = "/tmp2/.s3/";
static const char *dwnload = "downloading/";
static const char *prefix = "/s3disk/";
static const char *s3www = "http://s3.amazonaws.com/";
static const char *xmlheader= "<?xml";

//static const char *bucket="??????"; /* read from secret.h */
#include "secret.h"

extern int close();
extern int pwrite();
extern int pread();
extern int utime();
extern int stat();
extern int system();
extern int unlink();

typedef struct block_t {
	unsigned char buf[CHUNK];
	size_t bytes;
} block_t;


static char junk[BLKSIZE];

static void makejunk()
{
    /* do this so it compresses nicely */
    memset(junk, 0, sizeof(junk) );
}

static int hello_getattr(const char *path, struct stat *stbuf)
{
    int res = 0;

    memset(stbuf, 0, sizeof(struct stat));

    if(strcmp(path, "/") == 0) {
        stbuf->st_mode = S_IFDIR | 0777;
        stbuf->st_nlink = 2;
    }
    else if(strcmp(path, hello_path) == 0) {
        stbuf->st_mode = S_IFREG | 0777;
        stbuf->st_nlink = 1;
        stbuf->st_size = EXT3SIZE;
    }
    else {
        res = -ENOENT;
    }
    return res;
}

static int hello_readdir(const char *path, void *buf, fuse_fill_dir_t filler,
                         off_t offset, struct fuse_file_info *fi)
{
    (void) offset;
    (void) fi;

    if(strcmp(path, "/") != 0)
        return -ENOENT;

    filler(buf, ".", NULL, 0);
    filler(buf, "..", NULL, 0);
    filler(buf, hello_path + 1, NULL, 0);

    return 0;
}

static int hello_open(const char *path, struct fuse_file_info *fi)
{
    (void)fi;

    if(strcmp(path, hello_path) != 0) {
        return -ENOENT;
    }
    return 0;
}

/* taken from zlib example home pages:
   Decompress from file source to file dest until stream ends or EOF.
   inf() returns Z_OK on success, Z_MEM_ERROR if memory could not be
   allocated for processing, Z_DATA_ERROR if the deflate data is
   invalid or incomplete, Z_VERSION_ERROR if the version of zlib.h and
   the version of the library linked do not match, or Z_ERRNO if there
   is an error reading or writing the files. */
int inf(block_t *source, FILE *dest)
{
    int ret;
    unsigned have;
    z_stream strm;
    unsigned char out[CHUNK];

    /* allocate inflate state */
    strm.zalloc = Z_NULL;
    strm.zfree = Z_NULL;
    strm.opaque = Z_NULL;
    strm.avail_in = 0;
    strm.next_in = Z_NULL;
    ret = inflateInit(&strm);
    if (ret != Z_OK)
        return ret;

    /* decompress until deflate stream ends or end of file */

    strm.avail_in = source->bytes;
    strm.next_in = source->buf;

    /* run inflate() on input until output buffer not full */
    do {
       strm.avail_out = CHUNK;
       strm.next_out = out;
       ret = inflate(&strm, Z_NO_FLUSH);
       assert(ret != Z_STREAM_ERROR);  /* state not clobbered */
       switch (ret) {
       case Z_NEED_DICT:
           ret = Z_DATA_ERROR;     /* and fall through */
       case Z_DATA_ERROR:
       case Z_MEM_ERROR:
           (void)inflateEnd(&strm);
           return ret;
       }
       have = CHUNK - strm.avail_out;
       if (fwrite(out, 1, have, dest) != have || ferror(dest)) {
            (void)inflateEnd(&strm);
            return Z_ERRNO;
       }
    } while (strm.avail_out == 0);

    /* clean up and return */
    (void)inflateEnd(&strm);
    return ret == Z_STREAM_END ? Z_OK : Z_DATA_ERROR;
}

/* return current time since jan 1 1960 */
static time_t hoelaat()
{   time_t now;
    time(&now);
    return (now);
}

size_t curl_write_func(void *ptr, size_t size, size_t nmemb, block_t *blk) {
	size_t sz = size * nmemb;
	if (sz) {
		if (blk->bytes + sz > sizeof(blk->buf))
			return 0;
		memcpy(blk->buf+blk->bytes,ptr,sz);
		blk->bytes += sz;
	}

	return sz;
}

static struct curl_slist *get_amz_authhdrs(char *key) {
	char buf[BUFSIZ],datestr[32],*b64str;
	struct curl_slist *hdrs=NULL;
	unsigned int len = EVP_MAX_MD_SIZE;
	unsigned char md[len];
	time_t tm;
	BIO *bin,*bout;

	tm = time(NULL);
	strftime(datestr,sizeof(datestr)-1,"%a, %d %b %Y %H:%M:%S %Z",gmtime(&tm));
	strcpy(buf,"GET\n\n\n");
	strcat(buf,datestr);
	strcat(buf,"\n/");
	strcat(buf,key);

	HMAC(EVP_sha1(),access_key,strlen(access_key),(unsigned char *)buf,strlen(buf),md,&len);

	bin = BIO_new(BIO_f_base64());
	bout = BIO_new(BIO_s_mem());
	bout = BIO_push(bin,bout);
	BIO_write(bout,md,len);
	BIO_flush(bout);
	len = BIO_get_mem_data(bout,&b64str);
	b64str[len-1] = '\0'; // chop newline

	strcpy(buf,"Authorization: AWS ");
	strcat(buf,key_id);
	strcat(buf,":");
	strcat(buf,b64str);
	BIO_free_all(bout);
	hdrs = curl_slist_append(hdrs,buf);

	strcpy(buf,"Date: ");
	strcat(buf,datestr);
	hdrs = curl_slist_append(hdrs,buf);

	return hdrs;
}

static int wgetfile(const char *host,char *key, char *file)
{
	CURL *curl;
	CURLcode res;
	FILE *fpout;
	char buf[BUFSIZ];
	struct curl_slist *hdrs=NULL;
	block_t block;
	int fileo;

	curl = curl_easy_init();
	if (curl) {
		hdrs = get_amz_authhdrs(key);

		strcpy(buf,host);
		strcat(buf,key);

		block.bytes = 0;

//		curl_easy_setopt(curl,CURLOPT_VERBOSE,1);
		curl_easy_setopt(curl,CURLOPT_HTTPHEADER,hdrs);
		curl_easy_setopt(curl,CURLOPT_URL,buf);
		curl_easy_setopt(curl,CURLOPT_WRITEFUNCTION,curl_write_func);
		curl_easy_setopt(curl,CURLOPT_WRITEDATA,&block);

		res = curl_easy_perform(curl);
		if (res) {
			perror("curl perform");
		}
		curl_slist_free_all(hdrs);
		curl_easy_cleanup(curl);

		if (memcmp(xmlheader,block.buf,strlen(xmlheader)) == 0)
			return 0;

		/*  Here we make sure that we do not overwrite a file if it exists */
		fileo = open(file, O_CREAT | O_EXCL | O_WRONLY, S_IFREG | 0777 );
		if (fileo == -1) { /* then we assume it exists and return */
			return 0;
		}

		fpout=fdopen(fileo,"w"); /* reopen as stream */
		inf(&block,fpout);
		fclose(fpout);
		close(fileo);
		return 0;
	} else {
		perror("curl init");
		return -1;
	}
}

/* wait for download to finish if any in progress */
/* return 0 if ok, -1 if not ok */
int waitfordownload(char *blknr)
{
	char download[BUFSIZ];
	int cnt;
        struct stat buf;
	int fd;

    strcpy(download,tmpdir);
    strcat(download,dwnload);
    strcat(download,blknr);

    for (cnt=1; cnt <= MAXTRIES ; cnt++) {
       	fd = stat(download,&buf);
       	if ( fd != 0 ) {
         	return 0; /* not downloading so return */
       	}
        sleep(SLEEPTRY);
    }/*end for */
    /* download taking too long so remove lock file */
    unlink(download);
    return -1; /* it failed */
}

static int downloadblock(char *blknr, char *blockfile)
{
    char cmd[BUFSIZ];
    char download[BUFSIZ];
    int fd,res;
    struct stat buf;

    /* create downloading entry */
    strcpy(download,tmpdir);
    strcat(download,dwnload);
    strcat(download,blknr);

 /* create the file if it exists then we are already downloading */
    fd = open(download, O_CREAT | O_EXCL | O_WRONLY, S_IFREG | 0777);
    if (fd == -1) {
       /* it is being downloaded already */
       return(waitfordownload(blknr));
/* TODO */
/* we really should try harder here */
/* instead of returning */
    }
    close(fd); /* so we made the downloading file */

/* using CURLlib */
    strcpy(cmd,bucket);
    strcat(cmd,"/");
    strcat(cmd,prefix);
    strcat(cmd,blknr);
    wgetfile(s3www,cmd,blockfile);

    /* now see if we can read it if so return */
    fd = stat(blockfile,&buf);
    if (fd == 0) {
	/* ok it worked so return after cleanup */
       unlink(download);
       return 0;
    }

   /* so file not online so make it */
   /* but only make it if it doesn't exist O_EXCL */
    fd = open(blockfile, O_CREAT | O_EXCL | O_WRONLY, S_IFREG | 0777);
    if (fd == -1) {
        perror(blockfile);
        return -errno;
    }
    res = pwrite(fd, junk, BLKSIZE, 0);
    if (res == -1) {
        perror(blockfile);
        res = -errno;
    }
    close(fd);
    unlink(download); /* cleanup */
    return 0;
}

static int setatime(char *filename)
/* change atime but not modtime */
{
	int res;
	struct utimbuf timbuf;
	struct stat buf;
	time_t now;

	res = stat(filename,&buf);
	if (res == 0){
		time(&now);
		timbuf.actime = now;
		timbuf.modtime = buf.st_mtime;
		res= utime(filename,&timbuf);
	}
	return res;
}

int ultoa(unsigned long value, char *string, int radix)
{
  char tmp[33];
  char *tp = tmp;
  long i;
  unsigned long v = value;
  char *sp;

  while (v || tp == tmp)
  {
    i = v % radix;
    v = v / radix;
    if (i < 10)
      *tp++ = i+'0';
    else
      *tp++ = i + 'a' - 10;
  }

  sp = string;
 
  while (tp > tmp)
    *sp++ = *--tp;
  *sp = 0;
  return 0;
}

/* convert number into ascii representation */
int blknr2asc(char *blocknr, size_t blknr)
{   
    return(ultoa((unsigned long)blknr,blocknr,36));
}

/* return YES if blockfile exists or is being downloaded */
int blockfile_exists(int blknr)
{
    char blockfile[BUFSIZ];
    char blocknr[BUFSIZ];
    int fd;
    struct stat buf;


/* check if file is here already */
    strcpy(blockfile,tmpdir);
    blknr2asc(blocknr,blknr);
    strcat(blockfile,blocknr);

    fd = stat(blockfile,&buf);
    if (fd == 0) {
    	return (YES);
    }

/* check if downloading */
    strcpy(blockfile,tmpdir);
    strcat(blockfile,dwnload);
    strcat(blockfile,blocknr);

    fd = stat(blockfile,&buf);
    if (fd == 0) {
    	return (YES);
    }

    return (NO);
}

/* open the blockfile after downloading it if needed */
int openblockfile(size_t blknr)
{
    pid_t pid;
    int fd;
    int i;
    int res;
    char blockfile[BUFSIZ];
    char blocknr[BUFSIZ];
    static size_t lastblock = 0;

/* first check if downloading if so wait for it to finish */
    blknr2asc(blocknr,blknr);
    waitfordownload(blocknr);

    /* try to open the blockfile */
    strcpy(blockfile,tmpdir);
    strcat(blockfile,blocknr);
    fd = open(blockfile, O_RDONLY);
    if (fd == -1) {
	/* it doesn't exist so get it */
	res = downloadblock(blocknr,blockfile);
	/* and try again */
        fd = open(blockfile, O_RDONLY);
	if (fd == -1) {
		/* ok it didn't work give I/O error */
 	        return -1;
	}
    }
    setatime(blockfile); /* so we know we read it */
   
   /* if same one as last time then return */
   if (lastblock == blknr ) {
	return(fd);
   } else {
	lastblock = blknr;
   }

/* now lets fork off getting other blocks, while we return result */
/* dont fork if no LOOKAHEAD */
    if (LOOKAHEAD <= 0) {
	return(fd);
    }

/* so we have to fork and do some bkgrnd stuff */
    signal(SIGCLD, SIG_IGN);
    pid = fork();
/* ok we forked or we tried */
    if (pid == -1) {
	perror("forking"); /* something went wrong in fork() */
        return(fd);
    }

   if (pid == 0) {
/* the child loop where we can get some extra blocks if we want to */
	for (i=1; i <= LOOKAHEAD ; i++) {
		if ( blockfile_exists(blknr+i) == NO ) {
    			strcpy(blockfile,tmpdir);
    			blknr2asc(blocknr,blknr+i);
    			strcat(blockfile,blocknr);
			res = downloadblock(blocknr,blockfile);
		}
	} /*end for */
	exit (0); /* end the child process */
   }

/* the parent loop so just return */
    return (fd);
}


static int hello_read(const char *path, char *buf, size_t size, off_t offset,
                      struct fuse_file_info *fi)
{
    int res,tres;
    int fd;
    size_t blknr,bytesread,maxbytes,bytes2read;
    off_t offsetinblk,myoffset;
    (void) fi;
    (void) path;

    tres=0;
/* read 13 bytes from offset=58 blksize=10 */
    bytesread=(size_t)0;
    while (bytesread < size ) { /* bytesread=0,2,12,13 */
       myoffset= offset+bytesread; /* 58,60,70 */
       blknr=myoffset/BLKSIZE; /* 5=58/10, 6=60/10, 7=70/10 */
       offsetinblk = myoffset - (BLKSIZE*blknr); /* 8=58-(10*5), 0=60-(10*6), 0=70-(10*7) */
       maxbytes= BLKSIZE - offsetinblk; /* 2=10-8, 10=10-0, 10=10-0 */
       bytes2read = size - bytesread; /* 13=13-0, 11=13-2, 1=13-12 */
       if (bytes2read > maxbytes) {
		bytes2read = maxbytes; /* 2, 10, 1 */
       }
       /* now read the bytes */
       fd = openblockfile(blknr);
       if (fd == -1) {
    	  return -ENOENT;
       }
       res = pread(fd, &(buf[bytesread]), bytes2read, offsetinblk);
       if (res == -1) {
          res = -errno;
          close(fd);
	  return res;
       }
       tres=tres+res;
       close(fd);
       bytesread = bytesread +  bytes2read;
    } /* keep going in while until all bytes read */
    return tres;
}

static int hello_write(const char *path, const char *buf, size_t size,
                     off_t offset, struct fuse_file_info *fi)
{
    int fd;
    int res,tres;
    size_t blknr,byteswritten,maxbytes,bytes2write;
    char blocknr[BUFSIZ];
    off_t offsetinblk,myoffset;
    char blockfile[BUFSIZ];
    char dummy[2];

    tres=0;
/* write 13 bytes from offset=58 blksize=10 */
    byteswritten=(size_t)0;
    while (byteswritten < size ) { /* byteswritten=0,2,12,13 */
       myoffset= offset+byteswritten; /* 58,60,70 */
       blknr=myoffset/BLKSIZE; /* 5=58/10, 6=60/10, 7=70/10 */
       offsetinblk = myoffset - (BLKSIZE*blknr); /* 8=58-(10*5), 0=60-(10*6), 0=70-(10*7) */
       maxbytes= BLKSIZE - offsetinblk; /* 2=10-8, 10=10-0, 10=10-0 */
       bytes2write = size - byteswritten; /* 13=13-0, 11=13-2, 1=13-12 */
       if (bytes2write > maxbytes) {
		bytes2write = maxbytes; /* 2, 10, 1 */
       }

    /* we first need to make sure the blockfile exist before writing to it */
    /* so  we do a dummy 1 byte read on it */
    /* which will create it and do a look ahead */
     hello_read(path, dummy, (size_t)1, myoffset, fi);

    /* ok now get ready to write to it */
     strcpy(blockfile,tmpdir);
     blknr2asc(blocknr,blknr);
     strcat(blockfile,blocknr);

     /* just write to the blockfile since it will exist  */
     fd = open(blockfile,  O_CREAT | O_WRONLY, S_IFREG | 0777);
     if (fd == -1) {
         return -errno;
     }
     res = pwrite(fd, &(buf[byteswritten]), bytes2write, offsetinblk);
     if (res == -1) {
        res = -errno;
        close(fd);
	return res;
     }
     tres=tres+res;
     close(fd);
     byteswritten=byteswritten + bytes2write;
    }/* end while*/
    return tres;
}

static struct fuse_operations hello_oper = {
    .getattr	= hello_getattr,
    .readdir	= hello_readdir,
    .open	= hello_open,
    .read	= hello_read,
    .write	= hello_write,
};

int main(int argc, char *argv[])
{ 
    /* do the init stuff */
    makejunk();
//    signal(SIGCLD, SIG_IGN);  /* now I don't have to wait() for children */
    curl_global_init(CURL_GLOBAL_ALL);


    return fuse_main(argc, argv, &hello_oper);


   /* cleanup */
   curl_global_cleanup();	

}
