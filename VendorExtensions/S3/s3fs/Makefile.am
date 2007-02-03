## Process this file with automake to produce Makefile.in

LIBS = -lcurl -lssl -lcrypto -ldl -lz
SUBDIRS = @subdirs@ @subdirs2@

EXTRA_DIST =			\
	fuse.pc.in		\
	README*			\
	Filesystems		\
	FAQ			\
	doc/how-fuse-works	\
	doc/kernel.txt	\
	go	\
	done	\
	s3cmd.py	\
	s3.py

pkgconfigdir = @pkgconfigdir@
pkgconfig_DATA = fuse.pc

$(pkgconfig_DATA): config.status
