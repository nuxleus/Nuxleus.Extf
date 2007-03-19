# -*- coding: utf-8 -*-

__doc__ = """
The amplee.atompub package is the heart of amplee has it carries
the mapping of APP entities. However that package does not
mention HTTP anywhere. The amplee.handler package is meant to deal with
that aspect of APP.

Basically amplee.handler defines:

 - a class MemberHandler that can be called to perform operations
   on an amplee.atompub.store, collections and members.
 - a class MemberType to inform how the MemberHandler class should
   handle resources based on their mime-types
 - Two sub-packages allowing an easy integration of a store within
   a CherryPy or WSGI application.

Most of the time MemberHandler will not need to be handled directly
by the developer who will prefer the higher level interface provided
by the CherryPy and WSGI handlers.
"""

import os

from bridge import Element, Attribute
from bridge.common import ATOM10_PREFIX, ATOMPUB_PREFIX, \
     ATOM10_NS, ATOMPUB_NS, atom_as_attr, atom_as_list

from amplee.utils import get_isodate
from amplee.atompub.store import *
from amplee.atompub.service import *
from amplee.atompub.workspace import *
from amplee.atompub.collection import *
from amplee.atompub.member import atom, generic
from amplee.error import AmpleeError, UnknownResource

__all__ = ['MemberHandler', 'MemberType']

class MemberType(object):
    def __init__(self, media_type, member_class, params=None):
        """
        Represents meta data about a media-type.

        Keyword arguments
        media_type -- mime type as a string object
        member_class -- class which will be used to process resource
        params -- dictionary of extra parameters to provide to the member_class
        when is is instanciated
        """
        self.media_type = media_type
        self.member_class = member_class
        self.params = params or {}

    def deepcopy(self):
        """
        Mimic a deepcopy of a MemberType instance
        """
        mt = MemberType(self.media_type, self.member_class)
        mt.params = {}
        for key in self.params:
            mt.params[key] = self.params[key]

        return mt

class MemberHandler(object):
    """
    Main member hander that HTTP method handler can call to
    perform CRUD operations on the member and resource.
    """
    def create_from_stream(cls, collection, member_type, fileobj, length, media_type=None):
        """
        Create a new a collection and attach it.
        Returns the new member.

        If member_type.on_create is set to a callable and that
        callable raises an error or an exception, the member will not
        be attached to the collection and not returned by the class method.
        
        Keyword arguments
        collection -- collection which will host the member
        member_type -- a MemberType instance for the mime type of the resource
        fileobj -- a file object containg the content. It only needs a read(length) method
        length -- how much content to read from the file object
        media_type -- media type of the request
        """
        content = fileobj.read(length)
        member = member_type.member_class(collection, source=content, **member_type.params)

        result = collection.dispatch('on_create', media_type, member, content)
        if result is not None:
            member, content = result

        entry = member.atom.xml()

        try:
            if collection.member_media_type == media_type:
                collection.attach(member, member_content=entry)
            else:
                collection.attach(member, member_content=entry, media_content=content)
        except AmpleeError, ae:
            collection.dispatch('on_error', media_type, ae, member)
            raise ae

        return member
    create_from_stream = classmethod(create_from_stream)

    def get_content(cls, collection, member):
        """
        Get the content of a resource which identifier is 'rid'

        Raise a amplee.error.UnknownResource error if it could not
        be found.

        Keyword arguments
        collection -- collection which will host the member
        member -- member instance
        """
        content_type = member.media_type
        path = collection.get_content_path(member.media_id)
        content = collection.get_content(path)

        result = collection.dispatch('on_get_content', content_type,
                                     member, content, content_type)
        if result is not None:
            member, content, content_type = result

        return content, content_type.encode('utf-8')
    get_content = classmethod(get_content)

    def get_atom(cls, collection, member):
        """
        Get the content of an atom entry which identifier is 'rid'

        Raise a amplee.error.UnknownResource error if it could not
        be found.
        
        Keyword arguments
        collection -- collection which will host the member
        member -- member instance
        """
        content_type = collection.member_media_type
        content = member.atom.xml()

        result = collection.dispatch('on_get_atom', content_type, member)
        if result is not None:
            member, content, content_type = result
            
        return content, content_type.encode('utf-8')
    get_atom = classmethod(get_atom)
    
    def delete(cls, collection, member):
        """
        Delete the media resource and its meta data from the collection.

        Keyword arguments
        collection -- collection which will host the member
        member -- member instance
        """
        try: 
            collection.dispatch('on_delete', member.media_type, member)
            collection.prune(member.member_id, member.media_id)
        except StandardError, se:
            collection.dispatch('on_error', member.media_type, se, member)
            raise se
    delete = classmethod(delete)

    def update_content_from_stream(cls, collection, member_type, member, fileobj, length):
        """
        Update a resource identified by 'rid'
        Returns the updated member.

        If member_type.on_update is set to a callable and that
        callable raises an error or an exception, the member will not
        be attached to the collection and not returned by the class method.
        
        Keyword arguments
        collection -- collection which will host the member
        member_type -- a MemberType instance for the mime type of the resource
        member -- member instance
        fileobj -- a file object containg the content. It only needs a read(length) method
        length -- how much content to read from the file object
        """
        content = fileobj.read(length)

        entry = member
        member_type.params['slug'] = entry.media_id
        member_type.params['existing_member'] = entry
        member = member_type.member_class(collection, source=content, **member_type.params)

        result = collection.dispatch('on_update', entry.media_type, entry, member, content)
        if result is not None:
            member, content = result
        entry = member.atom.xml()

        try:
            collection.attach(member, member_content=entry, media_content=content, check_media_type=False)
        except AmpleeError, ae:
            collection.dispatch('on_error', member.media_type, ae, member)
            raise ae
        
        return member
    update_content_from_stream = classmethod(update_content_from_stream)
        
