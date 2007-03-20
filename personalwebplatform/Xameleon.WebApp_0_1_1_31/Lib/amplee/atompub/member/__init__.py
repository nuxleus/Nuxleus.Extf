# -*- coding: utf-8 -*-

__doc__ = """
APP establishes uses the term of members to describe
resources within an APP service context.

APP says:
'''
A resource whose IRI is listed in a Collection by a link
element with a relation of "edit" or "edit-media". 
'''

The base member is the what amplee calls the EntryMember
which is the resource whose IRI is contained in the
link defined by rel="edit".

The EntryMember should be rarerly used directly has it is
only meaningful if your resource is an Atom entry document.

On the other hand the MediaMember describes the resource
whose IRI is contained in the link defined by rel="edit-media".

The Mediamember will certainly be the most common class to
inherit for your own member implementations.
"""

__all__ = ['EntryMember', 'MediaMember', 'audio', 'odf', 'atom', 'generic', 'helper', 'media']

from bridge import Element as E
from bridge.common import atom_as_attr, atom_as_list, atom_attribute_of_element
from amplee.utils import generate_uuid_uri

class EntryMember(object):
    def __init__(self, collection, id=None, atom=None):
        """

        
        Keyword Arguments:
        collection -- AtomPubCollection instance holding this member
        id -- identifier for this member
        atom -- bridge.Element instance
        """
        self.collection = collection
        self.entry = atom
        self.member_id = id
        self.media_id = None
        self.media_type = u'application/atom+xml'
        
    def _getentry(self):
        if self.entry is None:
            path = self.collection.get_meta_data_path(self.member_id)
            source = self.collection.get_meta_data(path)
            self.entry = E.load(source, as_attribute=atom_as_attr, as_list=atom_as_list,
                                as_attribute_of_element=atom_attribute_of_element).xml_root
        return self.entry

    def _setentry(self, entry):
        raise AttributeError, "Cannot overwrite the underlying Atom entry"

    def _delentry(self):
        raise AttributeError, "Cannot delete the underlying Atom entry"
    
    atom = property(_getentry, _setentry, _delentry)

    def generate_id(self, existing_member, id_generator, create_default, *args, **kwargs):
        id = None
        if not existing_member:
             if callable(id_generator):
                 id = id_generator(self.collection, *args, **kwargs)
        else:
            id = existing_member.atom.get_child('id', existing_member.atom.xml_ns)
            if id:
                id = unicode(id)
                
        if not id and create_default:
            id = generate_uuid_uri()

        return id

    # For the pickling of this class
    def __getstate__(self):
        return {'member_id': self.member_id,
                'media_id': self.media_id,
                'media_type': self.media_type}
                #'entry': self.atom.xml(indent=False)}

    def __setstate__(self, state):
        self.entry = None
        self.member_id = state['member_id']
        self.media_id = state['media_id']
        self.media_type = state['media_type']
        #self.entry = E.load(state['entry'], as_attribute=atom_as_attr, as_list=atom_as_list,
        #                    as_attribute_of_element=atom_attribute_of_element).xml_root

class MediaMember(EntryMember):
    def __init__(self, collection, media_type):
        """

        Keyword arguments:
        collection -- AtomPubCollection instance holding this member
        media_type -- resource mime-type
        """
        EntryMember.__init__(self, collection)
        self.collection = collection
        self.media_type = media_type
        
    def _getmediacontent(self):
        return self.collection.get_content(self.collection.get_path(self.media_id))

    def _setmediacontent(self, entry):
        raise AttributeError, "Cannot overwrite the underlying content"

    def _delmediacontent(self):
        raise AttributeError, "Cannot delete the media content"
    
    content = property(_getmediacontent, _setmediacontent, _delmediacontent)
