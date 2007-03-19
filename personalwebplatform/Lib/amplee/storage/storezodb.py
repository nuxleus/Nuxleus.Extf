 # -*- coding: utf-8 -*-

__all__ = ['ZODBStorage']

# requires ZODB 3.6 or above
from BTrees.OOBTree import OOBTree
import transaction
import ZODB

from amplee.storage import Storage

class ZODBFSPath(list):
    def __init__(self):
        list.__init__(self)

    def __hash__(self):
        return id(repr(self))

class ZODBStorage(object):
    def __init__(self, db, name):
        """
        ZODB Storage for amplee.

        Keyword argument
        db -- a ZODB database (FileStorage for instance)
        name -- name of the top level node for this storage
        """
        self.database = db
        self.name = name
        conn = self.database.open()

        try:
            root = conn.root()
            if not root.has_key(self.name):
                root[self.name] = OOBTree()
                transaction.commit()
            conn.close()
        except:
            transaction.abort()
            conn.close()
            raise
        
    def create_container(self, collection_name):
        """
        Creates and returns a new new node named 'collection_name'
        """
        node = None
        conn = self.database.open()
        try:
            root = conn.root()
            if collection_name in root[self.name]:
                node = root[self.name][collection_name]
            else:
                root[self.name][collection_name] = OOBTree()
                node = root[self.name][collection_name]
                transaction.commit()
        except:
            transaction.abort()
            conn.close()
            raise IOError, "Could not create or retrieve %s" % collection_name

        return node
        
    def shutdown(self):
        """
        Shutdown the ZODB storage.
        Does nothing effectively.
        """
        pass

    def path(self, *args, **kwargs):
        """
        Returns the full path as a string of the resource
        as an instance of ZODBFSPath.
        """
        path = ZODBFSPath()
        path.extend(args)
        return path
    
    def get_content(self, path):
        """
        Returns the content as a string of the resource found at 'path'.
        If no resource could be found, an IOError is raised.

        Keyword arguments
        path -- Path to the resource as returned by get_path
        """
        conn = self.database.open()
        try:
            node = conn.root()[self.name]
            content = None
            resource = path.pop()
            if path:
                for part in path:
                    node = node[part]
            content = node[resource]
            conn.close()
        except:
            conn.close()
            raise IOError, "Could not find %s" % '.'.join(path)
        return content
    
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
        path = path[:]
        conn = self.database.open()
        node = conn.root()[self.name]
        resource = path.pop()
        if path:
            for part in path:
                node = node[part]
        node[resource] = content
        transaction.commit()
        conn.close()
        
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
        path = path[:]
        conn = self.database.open()
        try:
            node = conn.root()[self.name]
            resource = path.pop()
            if path:
                for part in path:
                    node = node[part]
            if node.has_key(resource):
                del node[resource]
                transaction.commit()
            conn.close()
        except:
            transaction.abort()
            conn.close()
            raise

    def remove_meta_data(self, path):
        """
        Remove the resource at 'path'

        Keyword arguments
        path -- Path to the resource as returned by get_path
        """
        self.remove_content(path)

    def persist(self, *args, **kwargs):
        """
        Does nothing in the ZODB storage
        """
        pass
        
    def exists(self, path):
        """
        Returns True if the resource at 'path' exists. False otherwise.
        
        Keyword arguments
        path -- Path to the resource as returned by get_path
        """
        path = path[:]
        conn = self.database.open()
        try:
            node = conn.root()[self.name]
            resource = path.pop()
            if path:
                for part in path:
                    node = node[part]
            res = node.has_key(resource)
            conn.close()
        except:
            conn.close()
            raise
            
        if res in (0, False, None):
            return False
        return True
    
    def ls(self, collection_name, ext, distinct=False):
        """
        List resources with the provided extension in a collection
        Returns a dictionary like this:

        members[basename(abs_path)] = {'path': abs_path}
        
        Keyword arguments
        collection_name -- name of the node in the ZODB database
        containing all the members of a collection. Created if it does
        not exists.
        ext -- extension of resources to return
        distinct -- if true returns all resources with an extension
        different from 'ext'
        """
        self.create_container(collection_name)
        path = self.path(collection_name)
        path = path[:]
        if ext and ext[0] != '.':
            ext = '.%s' % ext
        conn = self.database.open()
        node = conn.root()[self.name]
        for part in path:
            node = node[part]
        members = {}
        resources = node.keys()
        for resource in resources:
            abs_path = path[:]
            abs_path.append(resource)
            if ext:
                if distinct and not resource.endswith(ext):
                    members[resource] = {'path': abs_path}
                elif not distinct and resource.endswith(ext):
                    members[resource] = {'path': abs_path}
            else:
                members[resource] = {'path': abs_path}
        return members
