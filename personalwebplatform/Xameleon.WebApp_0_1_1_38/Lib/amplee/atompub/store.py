# -*- coding: utf-8 -*-

__doc__ = """Representation of a store.

Synopsis
--------

A store is a conceptual representation of your APP instance.
It is the outter envelop of the underlying entities involved.
APP does not define the concept of store per se but it is
a common way of naming and representing an APP instance.

A store is not really concerned about the meaning of members and
media resources however. All it cares about is 'meta-data' and 'content'.

A store needs at least one meta-data storage which will be used
to persist the representation of APP members.

A store can accept a second store which will be used to persist
media resources. If not provided the meta-data storage is then used.
"""

__all__ = ['AtomPubStore']

class AtomPubStore(object):
    def __init__(self, storage, media_storage=None,
                 autocommit=False, enable_lock=False):
        """Store acting has a container of APP entities.

        Operations on the store are only commited to the storage
        automatically when the ``autocommit`` parameter is ``True``. This is
        however not advised for performance issue.


        In any case operations put each resource in distinct internal
        lists which are processed when ``commit()`` is called.

        The ``storage`` is a ``mplee.storage.*`` instance to persist member resources.
        
        The ``media_storage`` is a ``amplee.storage.*`` instance to persist media resources.
        If not provided ``storage`` will be used.
        
        If ``autocommit`` is ``True`` the store should automatically call the
        persist method of the storage.
        This is disabled by default and should be rarely enabled.
        
        If ``enable_lock`` is ``True``, a thread lock will be used when performing
        operations against the store. This is disabled by default but will often be
        enabled.
        """
        self.storage = storage
        if not media_storage:
            media_storage = self.storage
        self.media_storage = media_storage
        self.dirty = False
        self.autocommit = autocommit
        self.locking_enabled = enable_lock
        if enable_lock:
            import threading
            self.lock = threading.RLock()

        # Non public attributes.
        self._pending_meta_data_to_add = {}
        self._pending_resource_to_add = {}
        self._pending_meta_data_to_remove = []
        self._pending_resource_to_remove = []
            
    def commit(self, message=None):
        """Persists modification to the store into the storages.
        
        A ``message``, if the storage accepts it
        (like subversion), to pass to the storage.
        """
        try:
            if self.locking_enabled:
                self.lock.acquire()

            for path in self._pending_meta_data_to_remove:
                self.storage.remove_meta_data(path)
                
            for path in self._pending_resource_to_remove:
                self.media_storage.remove_content(path)
                
            paths = []
            media_paths = []

            for path, infos in self._pending_meta_data_to_add.items():
                obj = infos['__obj__']
                if hasattr(obj, 'read'):
                    content = obj.read()
                else:
                    content = obj
                del infos['__obj__']
                self.storage.put_meta_data(path, content, **infos)
                
            paths.extend(self._pending_meta_data_to_remove)
            paths.extend(self._pending_meta_data_to_add.keys())

            for path, infos in self._pending_resource_to_add.items():
                obj = infos['__obj__']
                if hasattr(obj, 'read'):
                    content = obj.read()
                else:
                    content = obj
                del infos['__obj__']
                self.media_storage.put_content(path, content, **infos)

            media_paths.extend(self._pending_resource_to_remove)
            media_paths.extend(self._pending_resource_to_add.keys())

            if paths:
                self.storage.persist(paths, msg=message)
            if media_paths:
                self.media_storage.persist(media_paths, msg=message)

            self._pending_meta_data_to_remove = []
            self._pending_resource_to_remove = []
            self._pending_meta_data_to_add.clear()
            self._pending_resource_to_add.clear()
        finally:
            if self.locking_enabled:
                self.lock.release()

    def get_meta_data_path(self, *args):
        """Returns the path of resource used by the meta-data storage.
        
        The returned value should only be seen as read-only.

        Arguments depend on the underlying storage.
        """
        return self.storage.path(*args)

    def get_content_path(self, *args):
        """Returns the path of resource used by the content storage.
        
        The returned value should only be seen as read-only.

        Arguments depend on the underlying storage.
        """
        return self.media_storage.path(*args)

    def add_content(self, path, obj, **kwargs):
        """Adds content to the content storage.
        If ``self.autocommit`` is ``True`` the content is immediatly persisted.

        The ``path`` is a value returned by ``get_content_path()``
        The ``obj`` is whatever Python object that should be persisted
        ``**kwargs`` is any additional parameters to provide to the storage
        """
        try:
            if self.locking_enabled:
                self.lock.acquire()
            infos = {'__obj__': obj}
            if kwargs:
                infos.update(kwargs)
            self._pending_resource_to_add[path] = infos
            if self.autocommit:
                self.commit()
        finally:
            if self.locking_enabled:
                self.lock.release()

    def add_meta_data(self, path, obj, **kwargs):
        """Adds meta-data to the meta-data storage.
        If ``self.autocommit`` is ``True`` the content is immediatly persisted.

        The ``path`` is a value returned by ``get_meta_data_path()``
        The ``obj`` is whatever Python object that should be persisted
        ``**kwargs`` is any additional parameters to provide to the storage
        """
        try:
            if self.locking_enabled:
                self.lock.acquire()
            infos = {'__obj__': obj}
            if kwargs:
                infos.update(kwargs)
            self._pending_meta_data_to_add[path] = infos
            if self.autocommit:
                self.commit()
        finally:
            if self.locking_enabled:
                self.lock.release()

    def remove_meta_data(self, path):
        """Removes the resource at ``path`` from the meta-data storage.
        If ``self.autocommit`` is ``True`` the meta-data object is immediatly persisted.

        The ``path`` is a value returned by ``get_meta_data_path()``
        """
        try:
            if self.locking_enabled:
                self.lock.acquire()
            if path in self._pending_meta_data_to_add:
                del self._pending_meta_data_to_add[path]
            self._pending_meta_data_to_remove.append(path)
            if self.autocommit:
                self.commit()
        finally:
            if self.locking_enabled:
                self.lock.release()

    def remove_content(self, path):
        """Removes the resource at ``path`` from the content storage.
        If ``self.autocommit`` is ``True`` the content object is immediatly persisted.

        The ``path`` is a value returned by ``get_meta_data_path()``
        """
        try:
            if self.locking_enabled:
                self.lock.acquire()
            if path in self._pending_resource_to_add:
                del self._pending_resource_to_add[path]
            self._pending_resource_to_remove.append(path)
            if self.autocommit:
                self.commit()
        finally:
            if self.locking_enabled:
                self.lock.release()

    def exists(self, path, as_media=False):
        """Returns ``True`` if the resource ``path`` does exist.

        Keyword arguments:
        The ``path`` is a value returned by ``get_meta_data_path()``
        or ``get_content_path()``
        
        If ``as_media`` is ``False`` the method checks in the member storage
        otherwise it checks in the media storage. When both are the same it does
        not matter but if both are different you should set this value
        accordingly.
        """
        if as_media:
            return self.media_storage.exists(path)
        return self.storage.exists(path)

    def fetch_content(self, path):
        """Returns the content object at ``path``.

        The ``path`` is a value returned by ``get_content_path()``
        """
        return self.media_storage.get_content(path)
    
    def fetch_meta_data(self, path):
        """Returns the meta-data object at ``path``.

        The ``path`` is a value returned by ``get_meta_data_path()``
        """
        return self.storage.get_meta_data(path)

    def list_members(self, path, ext=None, distinct=False, in_media=False):
        """Returns a dictionary of the form ``{resource-name: {'path': resource-path}}`` filtered
        by the given parameters.

        The ``path`` is the base path where to perform the lookup
        
        The ``ext`` allows to filter by extension of the resource
        
        If ``distinct`` is ``True`` returns all resources with an extension
        different from ``ext``
        
        If ``in_media`` is ``False`` it will list members within the member storage
        otherwise it will list members within the media storage
        """
        if in_media:
            return self.media_storage.ls(path, ext=ext, distinct=distinct)
        return self.storage.ls(path, ext=ext, distinct=distinct)
