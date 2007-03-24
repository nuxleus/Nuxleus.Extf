# -*- coding: utf-8 -*-

__all__ = ['SubversionStorage']

import os, os.path
import thread

# http://pysvn.tigris.org
import pysvn

from amplee.storage import Storage

class SubversionStorage(Storage):
    def __init__(self, repository_uri, working_copy_path, username=None, password=None):
        """
        Subversion storage for amplee.

        If the working copy does not exist, the storage will automatically check the
        repository out.

        Keyword arguments
        repository_uri -- URI of the subversion repository. It can be a local or remote URL.
        working_copy_path -- absoulte path to the local working copy
        username -- if needed by the repository
        password -- if needed by the repository
        """
        self.repository_uri = repository_uri
        self.working_copy_path = working_copy_path
        self.lock = thread.allocate_lock()
        self.username = username
        self.password = password
        
        try:
            client = pysvn.Client()
            self._set_credentials(client)
            client.info(self.working_copy_path)
        except pysvn.ClientError, ce:
            try:
                client.checkout(self.repository_uri,
                                self.working_copy_path)
            except pysvn.ClientError, e:
                raise IOError, "Could not create working copy at %s: %s" % (self.working_copy_path, e)

    def _set_credentials(self, client):
        if self.username:
            client.set_default_username(self.username)
            if self.password:
                client.set_default_password(self.password)
        
    def shutdown(self):
        """
        Shutdown the subversion storage.
        Does nothing effectively.
        """
        pass

    def create_container(self, collection_name):
        """
        Creates a subdirectory within the the svn working copy
        If it already exists does nothing. The newly created
        directory is immediatly checked in.

        Keyword argument
        collection_name -- name of the directory to create
        """
        client = pysvn.Client()
        self._set_credentials(client)
        path = self.path(collection_name)
        resource = client.info(path)
        if not resource:
            client.mkdir(path, "Create %s" % path)
            client.checkin(path, "Create %s" % path)

    def path(self, *args, **kwargs):
        """
        Returns the full path as a string of the resource to the working copy.
        """
        tokens = [arg.strip('/') for arg in args if arg]
        return os.path.join(self.working_copy_path, *tokens)
    
    def get_content(self, path):
        """
        Returns the content as a string of the resource found at 'path'.
        If no resource could be found, an IOError is raised.

        Keyword arguments
        path -- Path to the resource as returned by get_path
        """
        path = os.path.join(self.working_copy_path, path)
        client = pysvn.Client()
        self._set_credentials(client)
        resource = client.info(path)
        if resource:
            content = client.cat(path)
            return content
        raise IOError, "Could not find %s" % path

    def get_meta_data(self, path):
        """
        Returns the content as a string of the meta data found at 'path'.
        If no resource could be found, an IOError is raised.

        Keyword arguments
        path -- Path to the resource as returned by get_path
        """
        return self.get_content(path)
                         
    def put_content(self, path, content, media_type=None, *args, **kwargs):
        """
        Set the content at 'path' of the resource.

        Keyword arguments
        path -- Path to the resource as returned by get_path
        content -- data as a string object
        media_type -- if provided, mime type of the resource as a string object
        It uses the svn:mime_type property
        """
        path = os.path.join(self.working_copy_path, path)
        target = file(path, 'wb')
        target.write(content)
        target.close()
        client = pysvn.Client()
        self._set_credentials(client)
        resource = client.info(path)
        if not resource:
            client.add(path)
        else:
            client.update(path)
        if media_type:
            client.propset('svn:mime-type', media_type, path)

    def put_meta_data(self, path, content, media_type=None, *args, **kwargs):
        """
        Set the content at 'path' of the resource.

        Keyword arguments
        path -- Path to the resource as returned by get_path
        content -- data as a string object
        media_type -- if provided, mime type of the resource as a string object
        It uses the svn:mime_type property
        """
        self.put_content(path, content, media_type=media_type)
        
    def remove_content(self, path):
        """
        Remove the resource at 'path'

        Keyword arguments
        path -- Path to the resource as returned by get_path
        """
        path = os.path.join(self.working_copy_path, path)
        client = pysvn.Client()
        self._set_credentials(client)
        resource = client.info(path)
        if resource:
            client.remove(path)
            
    def remove_meta_data(self, path):
        """
        Remove the resource at 'path'

        Keyword arguments
        path -- Path to the resource as returned by get_path
        """
        self.remove_content(path)

    def persist(self, path_list, msg=None):
        """
        Finalize the svn checkin
        
        Keyword arguments
        path_list -- list of paths as returned by get_path to
        checkin
        msg -- string message to provide for the checking
        It defaults to a simple 'Persist'
        """
        if not msg:
            msg = "Persist"
        client = pysvn.Client()
        self._set_credentials(client)
        client.checkin(path_list, msg)

    def exists(self, path):
        """
        Returns True if the resource at 'path' exists. False otherwise.
        
        Keyword arguments
        path -- Path to the resource as returned by get_path
        """
        client = pysvn.Client()
        self._set_credentials(client)
        entry = client.info(path)
        if entry:
            return True
        return False

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
        client = pysvn.Client()
        self._set_credentials(client)
        results = client.ls(path)
        members = {}
        if ext and ext[0] != '.':
            ext = '.%s' % ext
        for result in results:
            if ext:
                if not distinct and result['name'].endswith(ext):
                    members[os.path.basename(result['name'])] = {'path': result['name']}
                elif distinct and not result['name'].endswith(ext):
                    members[os.path.basename(result['name'])] = {'path': result['name']}
            else:
                members[os.path.basename(result['name'])] = {'path': result['name']}

        return members
