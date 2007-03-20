# -*- coding: utf-8 -*-

__doc__ = """
Base member class meant to handle Atom documents.

Use the AtomMember when your store has a collection
dealing with Atom documents. This measn that by
default every collections should use this class
to handle member resources which are simply
Atom entries.
"""

__all__ = ['AtomMember']

import os
from xml.sax.saxutils import escape

from bridge import Element, Attribute
from bridge.filter.atom import lookup_links
from bridge.common import ATOM10_PREFIX, ATOMPUB_PREFIX, XML_PREFIX, XML_NS, \
     ATOM10_NS, ATOMPUB_NS, XHTML1_NS, XHTML1_PREFIX, atom_as_attr, atom_as_list, \
     atom_attribute_of_element

from amplee.utils import generate_uuid_uri, get_isodate
from amplee.atompub.member import EntryMember
from amplee.atompub.member.helper import MemberHelper
from amplee.error import ResourceOperationException

class AtomMember(EntryMember):
    def __init__(self, collection, source, slug=None,
                 media_type=u'application/atom+xml;type=entry', existing_member=None,
                 entry_id_creator=None, name_creator=None, **kwargs):
        """

        Keyword arguments:
        collection -- AtomPubCollection instance carrying this member
        source -- resource string to handle
        slug -- hint on how to generate the name of the resource and
        used as the last part of the edit and edit-media IRIs (default:None)
        media_type -- mime-type for this member (default:u'application/atom+xml')
        existing_member -- if provided it must be an entry as a bridge.Element instance.
        It should be the existing member on the server which is going to be updated.
        entry_id_creator -- callable which must return an unicode object used
        for the id element of the entry (default:None)
        name_creator -- callable which must return an unicode object used
        as the name of the resource and as the last segment of it IRI (default:None)
        
        The 'entry_id_creator' callable, if provided, must take a bridge.Element
        as unique parameter. This instance containing the newly created atom
        entry or the one provided via the 'member' parameter.

        If no 'name_creator' provided, the name will default to an UUID value.
        If provided it must take two parameters, the 'slug' value provided and
        the 'title' of the entry.
        """
        EntryMember.__init__(self, collection)
        self.media_type = media_type

        doc = Element.load(source, as_attribute=atom_as_attr, as_list=atom_as_list,
                           as_attribute_of_element=atom_attribute_of_element).xml_root
        doc.update_prefix(ATOM10_PREFIX, doc.xml_ns, ATOM10_NS, update_attributes=False)
            
        if not existing_member:
            self.__create(doc, slug, entry_id_creator, name_creator, **kwargs)
        else:
            self.__update(doc, existing_member, **kwargs)
        
    def __create(self, doc, slug=None, entry_id_creator=None,
                 name_creator=None, **kwargs):

        id = self.generate_id(None, entry_id_creator, True, seed=doc, slug=slug)

        mh = MemberHelper(self.collection)
        mh.initiate(id=id)
        mh.copy_from(doc)

        if callable(name_creator):
            title = mh.entry.get_child('title', ATOM10_NS)
            if title:
                title = title.xml_text
            media_id = member_id = name_creator(self.collection, slug, title)
        else:
            media_id = member_id = id

        member_id = u'%s.%s' % (media_id, self.collection.member_extension)
        self.member_id = member_id
        self.media_id = media_id
        mh.add_edit_link(self.member_id)
        
        self.entry = mh.entry
        del mh

    def __update(self, doc, existing_member, **kwargs):        
        mh = MemberHelper(self.collection, existing_member)
        mh.initiate(id=existing_member.atom.get_child('id', ATOM10_NS).xml_text)
        mh.copy_from(doc)

        self.member_id = existing_member.member_id
        self.media_id = existing_member.media_id
        
        edit_links = mh.entry.filtrate(lookup_links, rel=u'edit')
        if not edit_links:
            mh.add_edit_link(self.member_id)
      
        self.entry = mh.entry
        del mh
