# -*- coding: utf-8 -*-

__all__ = ['DummyStorageFS']

try:
    from glob import iglob as glob
except ImportError:
    from glob import glob
    
import os, os.path
import threading

from amplee.storage import Storage

class DummyStorageFS(Storage):
    def __init__(self, storage_path, enable_lock=False):
        """
        Simple filesystem storage for amplee.

        If storage path does not exist, it is created.

        When the locking is enabled it creates a threading.RLock instance
        which is used for writing operations (put and remove).

        Keyword arguments
        storage_path -- absolute path to the top level directory which will contain
        collections and resources
        enable_lock -- if True, thread locking will be enabled during write operations
        """
        self.storage_path = storage_path
        if not os.path.exists(self.storage_path):
            os.mkdir(self.storage_path)
        self.enable_lock = enable_lock
        if enable_lock:
            self.lock = threading.RLock()
        
    def shutdown(self):
        """
        Shutdown the subversion storage.
        Does nothing effectively.
        """
        pass 

    def create_container(self, collection_name):
        """
        Creates a subdirectory within the storage directory
        If it already exists does nothing. 

        Keyword argument
        collection_name -- name of the directory to create
        """
        path = self.path(collection_name)
        if not os.path.exists(path):
            os.mkdir(path)
    
    def path(self, *args, **kwargs):
        """
        Returns the full path as a string of the resource
        relative to the storage directory
        """
        segments = []
        for segment in args:
            segments.append(segment.strip('/').encode('utf-8'))
        return os.path.join(self.storage_path, *segments)
    
    def get_content(self, path):
        """
        Returns the content as a string of the resource found at 'path'.
        If no resource could be found, an IOError is raised.

        Keyword arguments
        path -- Path to the resource as returned by get_path
        """
        if os.path.exists(path):
            resource = file(path, 'rb')
            content = resource.read()
            resource.close()
            return content
        raise IOError, "Could not find %s" % path
    
    def get_meta_data(self, path):
        """
        Returns the content as a string of the resource found at 'path'.
        If no resource could be found, an IOError is raised.

        Keyword arguments
        path -- Path to the resource as returned by get_path
        """
        return self.get_content(path)
    
    def put_content(self, path, content, **kwargs):
        """
        Set the content at 'path' of the resource.

        Keyword arguments
        path -- Path to the resource as returned by get_path
        content -- data as a string object
        """
        try:
            if self.enable_lock:
                self.lock.acquire()
            resource = file(path, 'wb')
            resource.write(content)
            resource.close()
        finally:
            if self.enable_lock:
                self.lock.release()
                
    def put_meta_data(self, path, content, **kwargs):
        """
        Set the content at 'path' of the resource.

        Keyword arguments
        path -- Path to the resource as returned by get_path
        content -- data as a string object
        """
        self.put_content(path, content)
        
    def remove_content(self, path):
        """
        Remove the resource at 'path'

        Keyword arguments
        path -- Path to the resource as returned by get_path
        """
        try:
            if self.enable_lock:
                self.lock.acquire()
            if os.path.exists(path):
                os.unlink(path)
        finally:
            if self.enable_lock:
                self.lock.release()

    def remove_meta_data(self, path):
        """
        Remove the resource at 'path'

        Keyword arguments
        path -- Path to the resource as returned by get_path
        """
        self.remove_content(path)

    def persist(self, *args, **kwargs):
        """
        Does nothing in the filesystem storage
        """
        pass
        
    def exists(self, path):
        """
        Returns True if the resource at 'path' exists. False otherwise.
        
        Keyword arguments
        path -- Path to the resource as returned by get_path
        """
        return os.path.exists(path)

    def ls(self, collection_name, ext, distinct=False):
        """
        List resources with the provided extension in a collection
        Returns a dictionary like this:

        members[basename(abs_path)] = {'path': abs_path}
        
        Keyword arguments
        collection_name -- name of the directory in the working copy
        containing all the members of a collection. Created if it does
        not exists.
        ext -- extension of resources to return
        distinct -- if true returns all resources with an extension
        different from 'ext'
        """
        self.create_container(collection_name)
        path = self.path(collection_name)
        if ext and ext[0] != '.':
            ext = '.%s' % ext
        if distinct:
            results = glob('%s/*' % path)
        else:
            results = glob('%s/*%s' % (path, ext or ''))
        members = {}
        for result in results:
            basename = os.path.basename(result)
            if ext:
                if distinct and not basename.endswith(ext):
                    members[basename] = {'path': result}
                elif not distinct and basename.endswith(ext):
                    members[basename] = {'path': result}
            else:
                members[basename] = {'path': result}
                
        return members
