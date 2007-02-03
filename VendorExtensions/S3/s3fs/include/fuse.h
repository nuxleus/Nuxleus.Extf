/*
    FUSE: Filesystem in Userspace
    Copyright (C) 2001-2006  Miklos Szeredi <miklos@szeredi.hu>

    This program can be distributed under the terms of the GNU LGPL.
    See the file COPYING.LIB.
*/

#ifndef _FUSE_H_
#define _FUSE_H_

/* This file defines the library interface of FUSE */

/* IMPORTANT: you should define FUSE_USE_VERSION before including this
   header.  To use the newest API define it to 25 (recommended for any
   new application), to use the old API define it to 21 (default) or
   22, to use the even older 1.X API define it to 11. */

#ifndef FUSE_USE_VERSION
#define FUSE_USE_VERSION 21
#endif

#include "fuse_common.h"

#include <sys/types.h>
#include <sys/stat.h>
#include <sys/statvfs.h>
#include <utime.h>

#ifdef __cplusplus
extern "C" {
#endif

/* ----------------------------------------------------------- *
 * Basic FUSE API                                              *
 * ----------------------------------------------------------- */

/** Handle for a FUSE filesystem */
struct fuse;

/** Structure containing a raw command */
struct fuse_cmd;

/** Function to add an entry in a readdir() operation
 *
 * @param buf the buffer passed to the readdir() operation
 * @param name the file name of the directory entry
 * @param stat file attributes, can be NULL
 * @param off offset of the next entry or zero
 * @return 1 if buffer is full, zero otherwise
 */
typedef int (*fuse_fill_dir_t) (void *buf, const char *name,
                                const struct stat *stbuf, off_t off);

/* Used by deprecated getdir() method */
typedef struct fuse_dirhandle *fuse_dirh_t;
typedef int (*fuse_dirfil_t) (fuse_dirh_t h, const char *name, int type,
                              ino_t ino);

/**
 * The file system operations:
 *
 * Most of these should work very similarly to the well known UNIX
 * file system operations.  A major exception is that instead of
 * returning an error in 'errno', the operation should return the
 * negated error value (-errno) directly.
 *
 * All methods are optional, but some are essential for a useful
 * filesystem (e.g. getattr).  Open, flush, release, fsync, opendir,
 * releasedir, fsyncdir, access, create, ftruncate, fgetattr, init and
 * destroy are special purpose methods, without which a full featured
 * filesystem can still be implemented.
 */
struct fuse_operations {
    /** Get file attributes.
     *
     * Similar to stat().  The 'st_dev' and 'st_blksize' fields are
     * ignored.  The 'st_ino' field is ignored except if the 'use_ino'
     * mount option is given.
     */
    int (*getattr) (const char *, struct stat *);

    /** Read the target of a symbolic link
     *
     * The buffer should be filled with a null terminated string.  The
     * buffer size argument includes the space for the terminating
     * null character.  If the linkname is too long to fit in the
     * buffer, it should be truncated.  The return value should be 0
     * for success.
     */
    int (*readlink) (const char *, char *, size_t);

    /* Deprecated, use readdir() instead */
    int (*getdir) (const char *, fuse_dirh_t, fuse_dirfil_t);

    /** Create a file node
     *
     * There is no create() operation, mknod() will be called for
     * creation of all non-directory, non-symlink nodes.
     */
    int (*mknod) (const char *, mode_t, dev_t);

    /** Create a directory */
    int (*mkdir) (const char *, mode_t);

    /** Remove a file */
    int (*unlink) (const char *);

    /** Remove a directory */
    int (*rmdir) (const char *);

    /** Create a symbolic link */
    int (*symlink) (const char *, const char *);

    /** Rename a file */
    int (*rename) (const char *, const char *);

    /** Create a hard link to a file */
    int (*link) (const char *, const char *);

    /** Change the permission bits of a file */
    int (*chmod) (const char *, mode_t);

    /** Change the owner and group of a file */
    int (*chown) (const char *, uid_t, gid_t);

    /** Change the size of a file */
    int (*truncate) (const char *, off_t);

    /** Change the access and/or modification times of a file */
    int (*utime) (const char *, struct utimbuf *);

    /** File open operation
     *
     * No creation, or truncation flags (O_CREAT, O_EXCL, O_TRUNC)
     * will be passed to open().  Open should check if the operation
     * is permitted for the given flags.  Optionally open may also
     * return an arbitrary filehandle in the fuse_file_info structure,
     * which will be passed to all file operations.
     *
     * Changed in version 2.2
     */
    int (*open) (const char *, struct fuse_file_info *);

    /** Read data from an open file
     *
     * Read should return exactly the number of bytes requested except
     * on EOF or error, otherwise the rest of the data will be
     * substituted with zeroes.  An exception to this is when the
     * 'direct_io' mount option is specified, in which case the return
     * value of the read system call will reflect the return value of
     * this operation.
     *
     * Changed in version 2.2
     */
    int (*read) (const char *, char *, size_t, off_t, struct fuse_file_info *);

    /** Write data to an open file
     *
     * Write should return exactly the number of bytes requested
     * except on error.  An exception to this is when the 'direct_io'
     * mount option is specified (see read operation).
     *
     * Changed in version 2.2
     */
    int (*write) (const char *, const char *, size_t, off_t,
                  struct fuse_file_info *);

    /** Just a placeholder, don't set */
    /** Get file system statistics
     *
     * The 'f_frsize', 'f_favail', 'f_fsid' and 'f_flag' fields are ignored
     *
     * Replaced 'struct statfs' parameter with 'struct statvfs' in
     * version 2.5
     */
    int (*statfs) (const char *, struct statvfs *);

    /** Possibly flush cached data
     *
     * BIG NOTE: This is not equivalent to fsync().  It's not a
     * request to sync dirty data.
     *
     * Flush is called on each close() of a file descriptor.  So if a
     * filesystem wants to return write errors in close() and the file
     * has cached dirty data, this is a good place to write back data
     * and return any errors.  Since many applications ignore close()
     * errors this is not always useful.
     *
     * NOTE: The flush() method may be called more than once for each
     * open().  This happens if more than one file descriptor refers
     * to an opened file due to dup(), dup2() or fork() calls.  It is
     * not possible to determine if a flush is final, so each flush
     * should be treated equally.  Multiple write-flush sequences are
     * relatively rare, so this shouldn't be a problem.
     *
     * Filesystems shouldn't assume that flush will always be called
     * after some writes, or that if will be called at all.
     *
     * Changed in version 2.2
     */
    int (*flush) (const char *, struct fuse_file_info *);

    /** Release an open file
     *
     * Release is called when there are no more references to an open
     * file: all file descriptors are closed and all memory mappings
     * are unmapped.
     *
     * For every open() call there will be exactly one release() call
     * with the same flags and file descriptor.  It is possible to
     * have a file opened more than once, in which case only the last
     * release will mean, that no more reads/writes will happen on the
     * file.  The return value of release is ignored.
     *
     * Changed in version 2.2
     */
    int (*release) (const char *, struct fuse_file_info *);

    /** Synchronize file contents
     *
     * If the datasync parameter is non-zero, then only the user data
     * should be flushed, not the meta data.
     *
     * Changed in version 2.2
     */
    int (*fsync) (const char *, int, struct fuse_file_info *);

    /** Set extended attributes */
    int (*setxattr) (const char *, const char *, const char *, size_t, int);

    /** Get extended attributes */
    int (*getxattr) (const char *, const char *, char *, size_t);

    /** List extended attributes */
    int (*listxattr) (const char *, char *, size_t);

    /** Remove extended attributes */
    int (*removexattr) (const char *, const char *);

    /** Open directory
     *
     * This method should check if the open operation is permitted for
     * this  directory
     *
     * Introduced in version 2.3
     */
    int (*opendir) (const char *, struct fuse_file_info *);

    /** Read directory
     *
     * This supersedes the old getdir() interface.  New applications
     * should use this.
     *
     * The filesystem may choose between two modes of operation:
     *
     * 1) The readdir implementation ignores the offset parameter, and
     * passes zero to the filler function's offset.  The filler
     * function will not return '1' (unless an error happens), so the
     * whole directory is read in a single readdir operation.  This
     * works just like the old getdir() method.
     *
     * 2) The readdir implementation keeps track of the offsets of the
     * directory entries.  It uses the offset parameter and always
     * passes non-zero offset to the filler function.  When the buffer
     * is full (or an error happens) the filler function will return
     * '1'.
     *
     * Introduced in version 2.3
     */
    int (*readdir) (const char *, void *, fuse_fill_dir_t, off_t,
                    struct fuse_file_info *);

    /** Release directory
     *
     * Introduced in version 2.3
     */
    int (*releasedir) (const char *, struct fuse_file_info *);

    /** Synchronize directory contents
     *
     * If the datasync parameter is non-zero, then only the user data
     * should be flushed, not the meta data
     *
     * Introduced in version 2.3
     */
    int (*fsyncdir) (const char *, int, struct fuse_file_info *);

    /**
     * Initialize filesystem
     *
     * The return value will passed in the private_data field of
     * fuse_context to all file operations and as a parameter to the
     * destroy() method.
     *
     * Introduced in version 2.3
     */
    void *(*init) (void);

    /**
     * Clean up filesystem
     *
     * Called on filesystem exit.
     *
     * Introduced in version 2.3
     */
    void (*destroy) (void *);

    /**
     * Check file access permissions
     *
     * This will be called for the access() system call.  If the
     * 'default_permissions' mount option is given, this method is not
     * called.
     *
     * This method is not called under Linux kernel versions 2.4.x
     *
     * Introduced in version 2.5
     */
    int (*access) (const char *, int);

    /**
     * Create and open a file
     *
     * If the file does not exist, first create it with the specified
     * mode, and then open it.
     *
     * If this method is not implemented or under Linux kernel
     * versions earlier than 2.6.15, the mknod() and open() methods
     * will be called instead.
     *
     * Introduced in version 2.5
     */
    int (*create) (const char *, mode_t, struct fuse_file_info *);

    /**
     * Change the size of an open file
     *
     * This method is called instead of the truncate() method if the
     * truncation was invoked from an ftruncate() system call.
     *
     * If this method is not implemented or under Linux kernel
     * versions earlier than 2.6.15, the truncate() method will be
     * called instead.
     *
     * Introduced in version 2.5
     */
    int (*ftruncate) (const char *, off_t, struct fuse_file_info *);

    /**
     * Get attributes from an open file
     *
     * This method is called instead of the getattr() method if the
     * file information is available.
     *
     * Currently this is only called after the create() method if that
     * is implemented (see above).  Later it may be called for
     * invocations of fstat() too.
     *
     * Introduced in version 2.5
     */
    int (*fgetattr) (const char *, struct stat *, struct fuse_file_info *);
};

/** Extra context that may be needed by some filesystems
 *
 * The uid, gid and pid fields are not filled in case of a writepage
 * operation.
 */
struct fuse_context {
    /** Pointer to the fuse object */
    struct fuse *fuse;

    /** User ID of the calling process */
    uid_t uid;

    /** Group ID of the calling process */
    gid_t gid;

    /** Thread ID of the calling process */
    pid_t pid;

    /** Private filesystem data */
    void *private_data;
};

/*
 * Main function of FUSE.
 *
 * This is for the lazy.  This is all that has to be called from the
 * main() function.
 *
 * This function does the following:
 *   - parses command line options (-d -s and -h)
 *   - passes relevant mount options to the fuse_mount()
 *   - installs signal handlers for INT, HUP, TERM and PIPE
 *   - registers an exit handler to unmount the filesystem on program exit
 *   - creates a fuse handle
 *   - registers the operations
 *   - calls either the single-threaded or the multi-threaded event loop
 *
 * Note: this is currently implemented as a macro.
 *
 * @param argc the argument counter passed to the main() function
 * @param argv the argument vector passed to the main() function
 * @param op the file system operation
 * @return 0 on success, nonzero on failure
 */
/*
int fuse_main(int argc, char *argv[], const struct fuse_operations *op);
*/
#define fuse_main(argc, argv, op) \
            fuse_main_real(argc, argv, op, sizeof(*(op)))

/* ----------------------------------------------------------- *
 * More detailed API                                           *
 * ----------------------------------------------------------- */

/**
 * Create a new FUSE filesystem.
 *
 * @param fd the control file descriptor
 * @param args argument vector
 * @param op the operations
 * @param op_size the size of the fuse_operations structure
 * @return the created FUSE handle
 */
struct fuse *fuse_new(int fd, struct fuse_args *args,
                      const struct fuse_operations *op, size_t op_size);

/**
 * Destroy the FUSE handle.
 *
 * The filesystem is not unmounted.
 *
 * @param f the FUSE handle
 */
void fuse_destroy(struct fuse *f);

/**
 * FUSE event loop.
 *
 * Requests from the kernel are processed, and the appropriate
 * operations are called.
 *
 * @param f the FUSE handle
 * @return 0 if no error occurred, -1 otherwise
 */
int fuse_loop(struct fuse *f);

/**
 * Exit from event loop
 *
 * @param f the FUSE handle
 */
void fuse_exit(struct fuse *f);

/**
 * FUSE event loop with multiple threads
 *
 * Requests from the kernel are processed, and the appropriate
 * operations are called.  Request are processed in parallel by
 * distributing them between multiple threads.
 *
 * Calling this function requires the pthreads library to be linked to
 * the application.
 *
 * @param f the FUSE handle
 * @return 0 if no error occurred, -1 otherwise
 */
int fuse_loop_mt(struct fuse *f);

/**
 * Get the current context
 *
 * The context is only valid for the duration of a filesystem
 * operation, and thus must not be stored and used later.
 *
 * @param f the FUSE handle
 * @return the context
 */
struct fuse_context *fuse_get_context(void);

/**
 * Obsolete, doesn't do anything
 *
 * @return -EINVAL
 */
int fuse_invalidate(struct fuse *f, const char *path);

/* Deprecated, don't use */
int fuse_is_lib_option(const char *opt);

/**
 * The real main function
 *
 * Do not call this directly, use fuse_main()
 */
int fuse_main_real(int argc, char *argv[], const struct fuse_operations *op,
                   size_t op_size);

/* ----------------------------------------------------------- *
 * Advanced API for event handling, don't worry about this...  *
 * ----------------------------------------------------------- */

/** Function type used to process commands */
typedef void (*fuse_processor_t)(struct fuse *, struct fuse_cmd *, void *);

/** This is the part of fuse_main() before the event loop */
struct fuse *fuse_setup(int argc, char *argv[],
                        const struct fuse_operations *op, size_t op_size,
                        char **mountpoint, int *multithreaded, int *fd);

/** This is the part of fuse_main() after the event loop */
void fuse_teardown(struct fuse *fuse, int fd, char *mountpoint);

/** Read a single command.  If none are read, return NULL */
struct fuse_cmd *fuse_read_cmd(struct fuse *f);

/** Process a single command */
void fuse_process_cmd(struct fuse *f, struct fuse_cmd *cmd);

/** Multi threaded event loop, which calls the custom command
    processor function */
int fuse_loop_mt_proc(struct fuse *f, fuse_processor_t proc, void *data);

/** Return the exited flag, which indicates if fuse_exit() has been
    called */
int fuse_exited(struct fuse *f);

/** Set function which can be used to get the current context */
void fuse_set_getcontext_func(struct fuse_context *(*func)(void));

/* ----------------------------------------------------------- *
 * Compatibility stuff                                         *
 * ----------------------------------------------------------- */

#ifndef __FreeBSD__

#if FUSE_USE_VERSION == 22 || FUSE_USE_VERSION == 21 || FUSE_USE_VERSION == 11
#  include "fuse_compat.h"
#  undef FUSE_MINOR_VERSION
#  undef fuse_main
#  if FUSE_USE_VERSION == 22
#    define FUSE_MINOR_VERSION 4
#    define fuse_main(argc, argv, op) \
            fuse_main_real_compat22(argc, argv, op, sizeof(*(op)))
#    define fuse_new fuse_new_compat22
#    define fuse_setup fuse_setup_compat22
#    define fuse_operations fuse_operations_compat22
#    define fuse_file_info fuse_file_info_compat22
#    define fuse_mount fuse_mount_compat22
#  else
#    define fuse_dirfil_t fuse_dirfil_t_compat
#    define __fuse_read_cmd fuse_read_cmd
#    define __fuse_process_cmd fuse_process_cmd
#    define __fuse_loop_mt fuse_loop_mt_proc
#    if FUSE_USE_VERSION == 21
#      define FUSE_MINOR_VERSION 1
#      define fuse_operations fuse_operations_compat2
#      define fuse_main fuse_main_compat2
#      define fuse_new fuse_new_compat2
#      define __fuse_setup fuse_setup_compat2
#      define __fuse_teardown fuse_teardown
#      define __fuse_exited fuse_exited
#      define __fuse_set_getcontext_func fuse_set_getcontext_func
#      define fuse_mount fuse_mount_compat22
#    else
#      warning Compatibility with API version 11 is deprecated
#      undef FUSE_MAJOR_VERSION
#      define FUSE_MAJOR_VERSION 1
#      define FUSE_MINOR_VERSION 1
#      define fuse_statfs fuse_statfs_compat1
#      define fuse_operations fuse_operations_compat1
#      define fuse_main fuse_main_compat1
#      define fuse_new fuse_new_compat1
#      define fuse_mount fuse_mount_compat1
#      define FUSE_DEBUG FUSE_DEBUG_COMPAT1
#    endif
#  endif
#elif FUSE_USE_VERSION < 25
#  error Compatibility with API version other than 21, 22 and 11 not supported
#endif

#else /* __FreeBSD__ */

#if FUSE_USE_VERSION < 25
#  error On FreeBSD API version 25 or greater must be used
#endif

#endif /* __FreeBSD__ */

#ifdef __cplusplus
}
#endif

#endif /* _FUSE_H_ */
