# -*- coding: utf-8 -*-

__doc__ = """
Generic media resources handler
"""

__all__ = ['GenericMediaMember']

import os

from amplee.utils import generate_uuid
from amplee.atompub.member import MediaMember
from amplee.atompub.member.helper import MemberHelper

class GenericMediaMember(MediaMember):
    def __init__(self, collection, source, slug=None, 
                 media_type=None, entry_id_creator=None,
                 existing_member=None, name_creator=None, **kwargs):
        """

        Keyword arguments:
        collection -- AtomPubCollection instance carrying this member
        source -- resource string to handle
        slug -- hint on how to generate the name of the resource and
        used as the last part of the edit and edit-media IRIs (default:None)
        media_type -- mime-type for this member (default:None)
        name_creator -- callable which must return an unicode object used
        as the name of the resource and as the last segment of it IRI (default:None)
        existing_member -- if provided it must be an entry as a bridge.Element instance.
        It should be the existing member on the server which is going to be updated.
        entry_id_creator -- callable which must return an unicode object used
        for the id element of the entry (default:None)

        The 'entry_id_creator' callable, if provided, must take one unique
        parameter which will be the content of the resource.

        If no 'name_creator' provided, the name will default to an UUID value.
        If provided it must take one parameter, the 'slug' value provided.
        """
        MediaMember.__init__(self, collection, media_type=media_type)

        if not callable(name_creator):
            name_creator = generate_uuid

        id = self.generate_id(existing_member, entry_id_creator, True, slug)
            
        if not existing_member:
            if callable(name_creator):
                media_id = member_id = name_creator(self.collection, slug, None)
            else:
                media_id = member_id = id
            member_id = u'%s.%s' % (member_id, self.collection.member_extension)
            self.member_id = member_id
            self.media_id = media_id
        else:
            self.member_id = existing_member.member_id
            self.media_id = existing_member.media_id

        mh = MemberHelper(self.collection, existing_member) 
        mh.initiate(id=id)
        mh.add_edit_link(self.member_id)
        mh.add_edit_media_link(self.media_id, self.media_type)
        mh.add_remote_content(self.media_id, self.media_type)
        mh.validate()
        
        self.entry = mh.entry
        del mh
