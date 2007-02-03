#!/usr/bin/env python

import s3
import sys
import secret	# where password and keys are stored
import os.path
import md5
import random
import zlib

AWS_KEY = secret.key_id
AWS_SECRET_KEY = secret.access_key
PASSWD = secret.passwd


#----------------------------------------------------
def crypt(key, text, encrypting):
#----------------------------------------------------
#	return text	# if not encrypting just return

# just compressing is expensive
	if encrypting:
		text = zlib.compress(text)

# this part is VERY expensive time and CPU wise
#	rand = random.Random(key).randrange
#	text = ''.join([chr(ord(elem)^rand(256)) for elem in text])
	if not encrypting:
		text = zlib.decompress(text)
	return text

#----------------------------------------------------
def encrypt(text):
#----------------------------------------------------
	return(crypt(PASSWD, text, True))

#----------------------------------------------------
def decrypt(text):
#----------------------------------------------------
	return(crypt(PASSWD, text, False))

#----------------------------------------------------
def error(msg):
#----------------------------------------------------
	print 'ERROR:',msg
	return(1)

#----------------------------------------------------
def connect():
#----------------------------------------------------
	try:
#		conn = s3.AWSAuthConnection(AWS_KEY, AWS_SECRET_KEY, is_secure=True)
		conn = s3.AWSAuthConnection(AWS_KEY, AWS_SECRET_KEY, is_secure=False)
		return conn, True
	except:
		return conn, False

#----------------------------------------------------
def download(key,file):
#----------------------------------------------------
	try:
		response = conn.get( bucket, key )
		data = response.object.data
		data = decrypt(data)
		fp=open(file,'wb')
		fp.write(data)
		fp.close()
		return True
	except:
		return False

#----------------------------------------------------
def upload(key,file):
#----------------------------------------------------
	try:
		fp=open(file,'rb')
		data = fp.read()
		data = encrypt(data)
#		response = conn.put( bucket, key, s3.S3Object(data) )
# lets do public for now to make curlib easier and faster
		response = conn.put( bucket, key, s3.S3Object(data) , { "x-amz-acl":"public-read" } )
		fp.close()
		print "Uploaded",file,"into bucket=",bucket,"key=",key
		return True
	except:
		return False

#----------------------------------------------------
def listbucket():
#----------------------------------------------------
	response = conn.list_bucket(bucket)
	truncated = response.is_truncated
	while truncated == 'true':
		nr = len(response.entries)
		response2=conn.list_bucket(bucket,{"marker":response.entries[nr-1].key} )
		for entry in response2.entries:
			response.entries.append(entry)
		truncated = response2.is_truncated

	return response

#----------------------------------------------------
def dir():
#----------------------------------------------------
	response = listbucket()
	for entry in response.entries:
#		print entry.etag[1:-1], entry.key
		print entry.key
	return True

#----------------------------------------------------
def prbytes(size):
#----------------------------------------------------
        if size < 1024:
                return "%d bytes" % size
        if size < 1024*1024:
                return "%d Kbytes" % (size/1024)
        if size < 1024*1024*1024:
                return "%d Mbytes" % (size/(1024*1024))
        return "%d Gbytes" % (size/(1024*1024*1024))


#----------------------------------------------------
def du():
#----------------------------------------------------
	response = listbucket()
	nrbytes = 0
	for entry in response.entries:
#		print prbytes(entry.size),entry.key
		nrbytes= nrbytes + entry.size
	print "Total",prbytes(nrbytes)
	return True

#----------------------------------------------------
def rmdir():
#----------------------------------------------------
	response = listbucket()
	for entry in response.entries:
		response = conn.delete(bucket,entry.key)
		print "deleted",entry.key
	return True

#----------------------------------------------------
def close(fp):
#----------------------------------------------------
	return fp.close()

#----------------------------------------------------
def md5sum(file):
#----------------------------------------------------
	try:
		fp=open(file,'rb')
		data = fp.read()
		fp.close()
	except:
		return "0"

	data = encrypt(data)
	return md5.new(data).hexdigest()

#----------------------------------------------------
def rmkey(key):
#----------------------------------------------------
	response = conn.delete(bucket,key)
	if response.http_response.status == 204:
		return True
	else:
		return False

def syncdir(localdir,remotedir):
	# get remote file md5 chcksums
	response = listbucket()

	# take out stuff in other directories
	for entry in response.entries:
		if os.path.dirname(entry.key) != remotedir:
			response.entries.remove(entry)

	# now fill the dictionary
	remotefile = {}
	for entry in response.entries:
		remotefile[os.path.basename(entry.key)]=entry.etag[1:-1] # take off "

	for root, dirs, files in os.walk(localdir):
		for file in files:
			# for every file chk chksum with remote
			fullpath = os.path.join(root,file)
			if os.path.dirname(fullpath) == localdir:
				md5 = md5sum(fullpath)
				try:
					remotemd5 = remotefile[file]
				except:
					remotemd5 = "NEW"
				if remotemd5 != md5:
					print "Changed",fullpath,"=",md5,file,"=",remotemd5
					upload(remotedir+'/'+file,fullpath)
				else:
					pass
			else:
				pass

# list all buckets
#----------------------------------------------------
def lsb():
#----------------------------------------------------
	response = conn.list_all_my_buckets()
        for bucket in response.entries:
        	print bucket.name
	return 0

# make new bucket
#----------------------------------------------------
def mkb():
#----------------------------------------------------
	response = conn.create_bucket( bucket )
	if response.http_response.status == 200:
		return 0
	else:
		return 1

# remove bucket
#----------------------------------------------------
def rmb():
#----------------------------------------------------
	response = conn.delete_bucket( bucket )
	if response.http_response.status == 204:
		return 0
	else:
		return 1

#----------------------------------------------------
# MAIN
#----------------------------------------------------
if len(sys.argv) > 1:
	conn,ok = connect()
	if ok:
		pass
	else:
		sys.exit(error('unable to connect to S3'))

	# s3cmd lsb # list buckets
	if sys.argv[1] == 'lsb':
		sys.exit(lsb())

	bucket = sys.argv[2]

	# s3cmd mkb bucket # make bucket
	if sys.argv[1] == 'mkb':
		sys.exit(  mkb() )

	# s3cmd rmb bucket # remove bucket
	if sys.argv[1] == 'rmb':
		sys.exit(  rmb() )

	# s3cmd get bucket key file
	if sys.argv[1] == 'get':
		sys.exit(download(sys.argv[3], sys.argv[4]))

	# s3cmd put bucket key file
	if sys.argv[1] == 'put':
		sys.exit(  upload(sys.argv[3], sys.argv[4]))

	# s3cmd dir bucket
	if sys.argv[1] == 'dir':
		sys.exit(  dir() )

	# s3cmd du bucket
	if sys.argv[1] == 'du':
		sys.exit(  du() )

	# s3cmd del bucket key
	if sys.argv[1] == 'del':
		sys.exit(  rmkey(sys.argv[3]) )

	# s3cmd rmdir bucket
	if sys.argv[1] == 'rmdir':
		sys.exit(  rmdir() )

	# s3cmd sync bucket localdir remotedir
	if sys.argv[1] == 'sync':
		sys.exit(  syncdir(sys.argv[3], sys.argv[4]) )

else:
	pass
