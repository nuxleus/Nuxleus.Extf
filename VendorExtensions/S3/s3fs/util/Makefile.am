## Process this file with automake to produce Makefile.in

bin_PROGRAMS = fusermount

fusermount_SOURCES = fusermount.c

install-exec-hook:
	-chown root $(DESTDIR)$(bindir)/fusermount
	-chmod u+s $(DESTDIR)$(bindir)/fusermount
	@if test ! -e $(DESTDIR)/dev/fuse; then \
		$(mkdir_p) $(DESTDIR)/dev; \
		echo "mknod $(DESTDIR)/dev/fuse -m 0666 c 10 229"; \
		mknod $(DESTDIR)/dev/fuse -m 0666 c 10 229; \
	fi

EXTRA_DIST = mount.fuse udev.rules

MOUNT_FUSE_PATH = @MOUNT_FUSE_PATH@
UDEV_RULES_PATH = @UDEV_RULES_PATH@

install-exec-local:
	$(mkdir_p) $(DESTDIR)$(MOUNT_FUSE_PATH)
	$(INSTALL_PROGRAM) $(srcdir)/mount.fuse $(DESTDIR)$(MOUNT_FUSE_PATH)/mount.fuse

install-data-local:
	$(mkdir_p) $(DESTDIR)$(UDEV_RULES_PATH)
	$(INSTALL_DATA) $(srcdir)/udev.rules $(DESTDIR)$(UDEV_RULES_PATH)/40-fuse.rules

uninstall-local:
	rm -f $(DESTDIR)$(MOUNT_FUSE_PATH)/mount.fuse
	rm -f $(DESTDIR)$(UDEV_RULES_PATH)/40-fuse.rules
