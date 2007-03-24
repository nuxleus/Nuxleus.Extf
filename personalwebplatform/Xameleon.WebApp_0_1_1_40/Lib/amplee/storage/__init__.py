# -*- coding: utf-8 -*-

class Storage(object):
    """Base storage class. Should not be instanciated"""
    
    def shutdown(self):
        """Shutdown the undrelying storage connections"""
        raise NotImplemented

    def create_container(self, name):
        """Creates a container within a storage.
        A container means whatever is meaningful for the subclass storage.
        A directory in the filesystem storage, a OBTree in the ZODB
        storage, a table in a database storage.
        """
        raise NotImplemented

    def path(self, *args, **kwargs):
        """Returns a path usable by the storage
        Do not expect this method to return a string or unicode.
        It can return whatever type makes sense to the storage
        """
        pass
    
    def get_content(self, *args, **kwargs):
        """Returns the content of a resource"""
        raise NotImplemented
    
    def get_meta_data(self, *args, **kwargs):
        """Returns the meta data of a resource"""
        raise NotImplemented
    
    def put_content(self, *args, **kwargs):
        """Puts the content of a resource"""
        raise NotImplemented
    
    def put_meta_data(self, *args, **kwargs):
        """Sets the meta-data of a resource"""
        raise NotImplemented
    
    def remove_content(self, *args, **kwargs):
        """Delete a resource"""
        raise NotImplemented
    
    def remove_meta_data(self, *args, **kwargs):
        """Delete meta-data"""
        raise NotImplemented

    def persist(self, *args, **kwargs):
        """Persist the operations made"""
        raise NotImplemented

    def exists(self, path):
        """Checks whether or not a path is valid for the storage"""
        raise NotImplemented
    
    def ls(self, collection_name_or_id, **kwargs):
        """Returns existing members"""
        raise NotImplemented
