# -*- coding: utf-8 -*-

#
# dejavu ORM can be downloaded from:
# http://projects.amor.org/dejavu/
#

__all__ = ['DejavuStorage', 'DJMember']

import new
import copy
import os, os.path

import dejavu
from dejavu import logic, Unit, UnitProperty
arena = dejavu.Arena()

from amplee.storage import Storage

class DJMember(Unit):
    member_name = UnitProperty(unicode)
    media_name = UnitProperty(unicode)
    member_data = UnitProperty(unicode, hints={u'bytes': 0})
    media_data = UnitProperty(str, hints={u'bytes': 0})
    
class DejavuStorage(Storage):
    def __init__(self, storage_type, config, base_unit=DJMember):
        """
        Dejavu storage for amplee.

        Keyword arguments
        storage_type -- string indicating what underlying engine to use by dejavu
        config -- a dictionnary of parameters to provide to dejavu for that storage_type
        base_unit -- base dejavu.Unit class to use when creating collection units

        The base_unit must provide at least the same properties as DJMember and more
        if needed.
        """
        arena.add_store("main", "%s" % storage_type, config)
        self.base_unit = base_unit
        
    def shutdown(self):
        """
        Shutdown the main dejavu arena object
        """
        arena.shutdown()

    def create_container(self, collection_name):
        """
        Register a new unit within the arena

        This will create a table in your database
        with the name 'collection_name' and following the
        same schema as defined by the base_unit interface.

        If it is already registered it will not create it.

        Returns the newly created unit class.

        Keyword argument
        collection_name -- name of the unit to create
        """
        try:
            cls = arena.class_by_name(collection_name)
        except KeyError:
            cls = new.classobj(collection_name, (self.base_unit,), {})
            arena.register(cls)
            if not arena.has_storage(cls):
                arena.create_storage(cls)
        return cls
    
    def path(self, *args, **kwargs):
        """
        Returns a unit instance or None if it could not be found
        in the storage.

        Keyword arguments
        args[0] -- collection name
        args[1] -- resource name to search for or None
        """
        sandbox = arena.new_sandbox()
        collection_name, resource_name = args[0], args[1] or ''
        expr = logic.Expression(lambda m: ((m.member_name == resource_name) or \
                                           (m.media_name == resource_name)))
        member_cls = self.create_container(collection_name)
        mb = sandbox.unit(member_cls, expr)
        return mb

    def get_content(self, mb):
        """
        Returns the content of the resource

        Keyword argument
        mb -- as returned by get_path
        """
        return mb.media_data
        
    def get_meta_data(self, mb):
        """
        Returns the content (UTF-8 encoded) of the resource

        Keyword argument
        mb -- as returned by get_path
        """
        return mb.member_data.encode('utf-8')

    def put_content(self, mb, content, member_id, media_id,
                    collection_name, **kwargs):
        """
        Set the resource content of the unit instance

        Automatically flushes modification to the storage.
        
        Keyword arguments
        mb -- Unit instance as returned by get_path or None.
        If None, mb is created.
        content -- string object to dump into mb.media_content
        member_id -- as provided by member.member_id
        media_id -- as provided by member.media_id
        collection_name -- collection storing the resource
        """
        if not mb:
            sandbox = arena.new_sandbox()
            member_cls = self.create_container(collection_name)
            mb = member_cls()
            mb.member_name = member_id
            mb.media_name = media_id
            sandbox.memorize(mb)
            
        mb.adjust(media_data=content)
        mb.sandbox.flush_all()
        
    def put_meta_data(self, mb, content, member_id, media_id,
                      collection_name, **kwargs):
        """
        Set the resource content of the unit instance

        Automatically flushes modification to the storage.
        
        Keyword arguments
        mb -- Unit instance as returned by get_path or None.
        If None, mb is created.
        content -- string object to dump into mb.member_content
        member_id -- as provided by member.member_id
        media_id -- as provided by member.media_id
        collection_name -- collection storing the resource
        """
        if not mb:
            sandbox = arena.new_sandbox()
            member_cls = self.create_container(collection_name)
            mb = member_cls()
            mb.member_name = member_id
            mb.media_name = media_id
            sandbox.memorize(mb)

        mb.adjust(member_data=content)
        mb.sandbox.flush_all()
 
    def remove_content(self, mb):
        """
        Removes the resource from the storage

        Keyword argument
        mb -- Unit instance as returned by get_path
        """
        mb.forget()
            
    def remove_meta_data(self, mb):
        """
        Removes the resource from the storage

        Keyword argument
        mb -- Unit instance as returned by get_path
        """
        self.remove_content(mb)

    def persist(self, *args, **kwargs):
        """
        Does nothing in this storage.
        """
        pass
        
    def exists(self, mb):
        """
        True if mb is valid
        False otherwise
        
        Keyword argument
        mb -- Unit instance as returned by get_path
        """
        if mb is None:
            return False
        return True
    
    def ls(self, collection_name, ext, distinct=False):
        """
        List resources with the provided extension in a collection
        Returns a dictionary like this:

        members[basename(abs_path)] = {'path': abs_path}
        
        Keyword arguments
        collection_name -- name of the storage unit 
        containing all the members of a collection. Created if it does
        not exists.
        ext -- unused
        distinct -- if true returns all resources with an extension
        different from 'ext'
        """
        member_cls = self.create_container(collection_name)
        sandbox = arena.new_sandbox()
        results = sandbox.recall(member_cls)
        members = {}
        if ext and ext[0] != '.':
            ext = '.%s' % ext
        for result in results:
            if ext:
                if not distinct and result.media_name.endswith(ext):
                    members[result.media_name] = {'path': result}
                elif distinct and not result.media_name.endswith(ext):
                    members[result.media_name] = {'path': result}
                elif not distinct and result.member_name.endswith(ext):
                    members[result.member_name] = {'path': result}
            else:
                members[result.member_name] = {'path': result}

        return members

