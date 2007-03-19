# -*- coding: utf-8 -*-

__doc__ = """
This module defines a set of member classes aiming at
dealing with media formats supported by the Hachoir library:
http://hachoir.org/wiki/hachoir-parser

Therefore this module requires the Hachoir library (core and meta_data):
http://hachoir.org/
"""

__all__ = ['HachoirMember']

from datetime import datetime
import os

from bridge import Element, Attribute
from bridge.common import ATOM10_PREFIX, ATOMPUB_PREFIX, XML_PREFIX, XML_NS, \
     ATOM10_NS, ATOMPUB_NS

from amplee.utils import generate_uuid_uri, get_isodate
from amplee.error import MemberMediaError
from amplee.atompub.member import MediaMember
from amplee.atompub.member.helper import MemberHelper

from hachoir.error import HachoirError
from hachoir.stream import InputStreamError, InputSubStream
from hachoir_parser import guessParser
from hachoir_metadata import extractMetadata

class HachoirMember(MediaMember):
    def __init__(self, collection, source, slug=None, title=None,
                 description=None, media_type=None, ext=None,
                 entry_id_creator=None, name_creator=None, **kwargs): 
        """
        Creates a member based Hachoir capabilities.

        Keyword arguments
        collection -- parent collection
        source -- fileobj of data to handle
        title -- title to use for the Atom entry. If not provided it will be extracted
        from the resource metadat if any. 
        description -- summary to use for the Atom entry. If not provided it will
        extracted from the metadata if any.
        name_creator -- callable to generate the last segment of URIs used for
        this resource
        ext -- extension to use for the media resource
        media_type -- mime type of the media resource
        entry_id_creator -- callable which will return the id
        to use in the atom:id element (as an unicode object)

        The name_creator and the entry_id_creator function must takes the following parameters:
        collection, abs_path, metadata (a hachoir_metadata.metadata instance), slug, ext
        """       
        MediaMember.__init__(self, collection, media_type=media_type)

        metadata = self.__get_meta_data(source)
        
        if not existing_member:
            name = slug
            if callable(name_creator):
                name = name_creator(collection, abs_path, metadata, slug, ext)

            media_id = member_id = name
            if self.collection.member_extension:
                member_id = u'%s.%s' % (name, self.collection.member_extension)
        else:
            member_id = existing_member.member_id
            media_id = existing_member.media_id
            
        self.media_id = media_id
        self.member_id = member_id

        id = self.generate_id(existing_member, entry_id_creator, True, abs_path, metadata, slug, ext)
            
        mh = MemberHelper(self.collection, existing_member)
        mh.initiate(id=id)
        mh.add_element('title', content=name, attributes={u'type': u'text'})
        mh.add_element('summary', unicode(description), attributes={u'type': u'text'})
        author_name = self.__clean_value(metadata, 'author')
        author = mh.add_element('author')
        mh.add_element('name', content=author_name, parent=author)
        mh.add_edit_link(self.member_id)
        mh.add_edit_media_link(self.media_id, self.media_type)
        mh.add_remote_content(self.media_id, self.media_type)

        mh.validate()
        self.entry = mh.entry
        del mh
        
    def __clean_value(self, metadata, name):
        value = getattr(metadata, name, None)
        
        if not value:
            value = u''
            
        elif isinstance(value, list):
            value = unicode(', '.join(value))
        elif isinstance(value, basetring):
            value =  unicode(value)

        return value
    
    def __get_meta_data(self, source):
        parser = None
        try:
            parser = guessParser(input=StringInputStream(source))
        except InputStreamError, err:
            raise unicode(err)
        if not parser:
            raise "Could not parse '%s'" % filepath

        try:
            metadata = extractMetadata(parser)
        except HachoirError, err:
            raise unicode(err)

        return metadata
