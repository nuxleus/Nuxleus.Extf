# -*- coding: utf-8 -*-

__doc__ = """
Member classes have a common set of functionnalities that this module try
to centralize to make the code easier to ead and maintain
"""

from xml.sax.saxutils import escape

from bridge import Element, Attribute
from bridge.common import ATOM10_PREFIX, ATOMPUB_PREFIX, XML_PREFIX, XML_NS, \
     ATOM10_NS, ATOMPUB_NS, XHTML1_NS, XHTML1_PREFIX, THR_NS, atom_as_attr, \
     atom_as_list, atom_attribute_of_element

from amplee.utils import generate_uuid_uri, get_isodate
from bridge.filter.atom import lookup_links, requires_summary, requires_author
from amplee.atompub.member import EntryMember

class MemberHelper(object):
    def __init__(self, collection, existing_member=None):
        self.collection = collection
        self.existing_member = existing_member
        self.entry = None
        
    def initiate(self, id):
        """
        Creates a default Atom entry document with its id, published, updated and
        edited children set. If the collection has a xml_base attribute set it will
        also set it.

        Keyword argument:
        id -- the identifier of the entry (unicode)
        """
        entry = Element(u'entry', prefix=ATOM10_PREFIX, namespace=ATOM10_NS)
        # He we force atom elements to be set as attributes of their parents
        # so that they are more easily accessible
        entry.as_attribute.update(atom_as_attr)
        entry.as_list.update(atom_as_list)
        entry.as_attribute_of_element.update(atom_attribute_of_element)
        
        Element(u'id', content=id, prefix=ATOM10_PREFIX, namespace=ATOM10_NS, parent=entry)
        isodate = get_isodate()

        if self.existing_member is None:
            Element(u'published', content=isodate, prefix=ATOM10_PREFIX,
                    namespace=ATOM10_NS, parent=entry)
            Element(u'updated', content=isodate, prefix=ATOM10_PREFIX,
                    namespace=ATOM10_NS, parent=entry)
        else:
            # when we already have an existing member we make sure
            # we preserve the atom:published and atom:updated elements
            pub = self.existing_member.atom.get_child('published', ATOM10_NS)
            if pub:
                pub_date = unicode(pub)
            else:
                pub_date = isodate
            Element(u'published', content=pub_date, prefix=ATOM10_PREFIX,
                    namespace=ATOM10_NS, parent=entry)
            
            updated = self.existing_member.atom.get_child('updated', ATOM10_NS)
            if pub:
                updated_date = unicode(updated)
            else:
                updated_date = isodate
            Element(u'updated', content=updated_date, prefix=ATOM10_PREFIX,
                    namespace=ATOM10_NS, parent=entry)
            
        Element(u'edited', content=isodate, prefix=ATOMPUB_PREFIX,
                namespace=ATOMPUB_NS, parent=entry)

        if self.collection.xml_base:
            Attribute(u'base', self.collection.xml_base, prefix=XML_PREFIX,
                      namespace=XML_NS, parent=entry)

        self.entry = entry

    def add_edit_link(self, member_id):
        """
        Add an atom:link rel='edit' to the entry

        Keyword argument:
        member_id -- id of the member resource
        """
        base_edit_uri = self.collection.base_edit_uri.rstrip('/')
        attr = {u'rel': u'edit', u'type': self.collection.member_media_type,
                u'href': unicode(escape("%s/%s" % (base_edit_uri, member_id))),}
        self.add_element('link', attributes=attr)

    def add_edit_media_link(self, media_id, media_type, length=None):
        """
        Add an atom:link rel='edit-media' to the entry

        Keyword argument:
        media_id -- id of the member resource
        media_type -- mime type of the resource
        length -- if provided an unicode object to fill the length attribute of the link
        """
        base_media_edit_uri = self.collection.base_media_edit_uri.rstrip('/')
        attr = {u'rel': u'edit-media', u'type': media_type, 
                u'href': unicode(escape("%s/%s" % (base_media_edit_uri, media_id)))}
        link = self.add_element('link', attributes=attr)
        if length:
            Attribute(u'length', length, prefix=XML_PREFIX,
                      namespace=XML_NS, parent=link)

    def add_remote_content(self, media_id, media_type):
        """
        Add an atom:content element with 'src' and 'type' attributes.
        The 'src' value will be computed based on internal the collection.base_uri
        attribute and the 'media_id'

        Keyword argument:
        media_id -- value to be used for the last segment of the IRI
        media_type -- the mime type of the remote resource
        """
        src_uri = self.collection.base_uri.rstrip('/')
        attr = {u'src': unicode(escape("%s/%s" % (src_uri, media_id))).lstrip('/'),
                u'type': media_type}
        self.add_element('content', attributes=attr)

    def clean_up_element_name(self, name):
        return name.replace('-', '_').replace('.', '_')

    def add_element(self, name, content=None, attributes=None,
                    prefix=None, ns=None, parent=None):
        """
        Add a child to an element

        Keyword arguments:
        name -- name of the element
        content -- content of the element
        attributes -- dictionnary of attributes to set
        prefix -- XML prefix (if not provided defaults to self.entry.xml_prefix)
        ns -- XML namespace (if not provided defaults to self.entry.xml_ns)
        parent -- if provided the child will be attached to parent,
        otherwiseparent will default to self.entry
        """
        if not parent:
            parent = self.entry
        if not prefix:
            prefix = parent.xml_prefix
        if not ns:
            ns = parent.xml_ns
        return Element(name, content=content, attributes=attributes,
                       prefix=prefix, namespace=ns, parent=parent)

    def copy_element(self, name, source=None, destination=None, ns=None):
        """
        Copy an element into another element

        Keyword arguments:
        name -- name of the element to copy
        source -- element containing a child 'name'
        destination -- element to which attached the copy
        ns -- XML namespace to match (if not provided defaults to self.entry.xml_ns)
        """
        if not destination:
            destination = self.entry
        if not ns:
            ns = destination.xml_ns
        if source.has_child(name, ns):
            copy = source.get_child(name, ns).clone().xml_root
            copy.xml_parent = destination
            destination.xml_children.append(copy)
            #setattr(destination, self.clean_up_element_name(name), copy)
            return copy
        
        return None

    def copy_elements(self, name, source=None, destination=None, ns=None):
        """
        Copy a list of elements into another element

        Keyword arguments:
        name -- name of the element to copy
        source -- element containing a child 'name'
        destination -- element to which attached the copy
        ns -- XML namespace to match (if not provided defaults to self.entry.xml_ns)
        """
        if not destination:
            destination = self.entry
        if not ns:
            ns = destination.xml_ns
        handle = getattr(destination, name, [])
        children = source.get_children(name, ns)
        for child in children:
            copy = child.clone().xml_root
            copy.xml_parent = destination
            destination.xml_children.append(copy)
            handle.append(copy)
        #setattr(destination, self.clean_up_element_name(name), handle)

        return handle

    def validate(self):
        """
        Validates this entry by following section 4.1.2 of RFC 4287
        """
        if not self.entry.has_child('title', ATOM10_NS):
            self.add_element('title', content=u'', attributes={u'type': u'text'})

        needs_summary = self.entry.filtrate(requires_summary)
        if needs_summary:
            self.add_element('summary', content=u'', attributes={u'type': u'text'})
            
        needs_author = self.entry.filtrate(requires_author)
        if needs_author:
            author = self.add_element('author')
            self.add_element('name', content=u'', parent=author)

        if not self.entry.has_child('content', ATOM10_NS):
            # atom:entry elements that contain no child atom:content element
            # MUST contain at least one atom:link element with a rel attribute value of "alternate".
            links = self.entry.filtrate(lookup_links, rel=u'alternate')
            if not links:
                self.add_element('content', content=u'', attributes={u'type': u'text'})
                
    def copy_from(self, entry):
        """
        Copies elements from the 'entry' into 'self' and ensure
        this copy keeps 'self' valid as per RFC 4287.

        Keyword argument:
        entry -- source entry bridge.Element
        """
        # the source document had no title to copy from
        # we must add one
        title = self.copy_element('title', source=entry)
        if not title:
            self.add_element('title', content=u'', attributes={u'type': u'text'})

        summary = self.copy_element('summary', source=entry)
        # the source document had no summary to copy from
        # let's see if we need one
        if not summary:
            needs_summary = entry.filtrate(requires_summary)
            if needs_summary:
                self.add_element('summary', content=u'', attributes={u'type': u'text'})

        authors = self.copy_elements('author', source=entry)
        # the source document had no authors to copy from
        # let's see if we need one
        if not authors:
            needs_author = entry.filtrate(requires_author)
            if needs_author:
                author = self.add_element('author')
                self.add_element('name', content=u'', parent=author)   

        # let's copy the atom:link elements from the source
        # except the rel="edit" and rem="edit-media" ones
        links = entry.filtrate(lookup_links, rel=u'edit')
        for link in links:
            del link
        links = entry.filtrate(lookup_links, rel=u'edit-media')
        for link in links:
            del link
        self.copy_elements('link', source=entry)
        self.copy_element('in-reply-to', source=entry, ns=THR_NS)
        self.copy_elements('category', source=entry)
        
        content = self.copy_element('content', source=entry)

        if not content:
            # atom:entry elements that contain no child atom:content element
            # MUST contain at least one atom:link element with a rel attribute value of "alternate".
            links = entry.filtrate(lookup_links, rel=u'alternate')
            if not links:
                self.add_element('content', content=u'', attributes={u'type': u'text'})
            
