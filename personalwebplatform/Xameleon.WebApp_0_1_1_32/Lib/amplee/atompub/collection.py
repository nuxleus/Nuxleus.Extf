# -*- coding: utf-8 -*-

__doc__ = """

Synopsis
--------

Collections are the container of member and media resources.

Collections are important to amplee as they will map eventually
to a physical container in the storage.

For instance a collection named ``notes`` would map to:

- a table named ``notes`` in a database
- a directory names ``notes`` on the filesystem
- a bucket named ``notes`` on Amazon S3

etc.

So it is very important to carefully choose the ``name_or_id`` parameter
in the class ``AtomPubCollection``.
"""

__all__ = ['AtomPubCollection']

from urlparse import urlparse

from bridge import Element, Attribute
from bridge.filter.atom import lookup_links
from bridge.validator.atom import respect_fixed_categories
from bridge.validator import BridgeValidatorException
from bridge.common import ATOM10_PREFIX, ATOMPUB_PREFIX, XML_PREFIX, XML_NS, \
     ATOM10_NS, ATOMPUB_NS, atom_as_attr, atom_as_list, atom_attribute_of_element

from amplee.utils import generate_uuid_uri, get_isodate
from amplee.atompub.member import EntryMember
from amplee.http_helper import get_best_mimetype
from amplee.error import UnknownResource, UnsupportedMediaType, FixedCategoriesError

class AtomPubCollection(object):
    def __init__(self, workspace, name_or_id, title, base_uri, 
                 base_edit_uri, base_media_edit_uri=None, xml_attrs=None,
                 member_extension=u'atom', member_media_type=u'application/atom+xml',
                 accept_media_types=None, categories=None,
                 fixed_categories=None, favorite=False):
        """
        Atom Publishing Protocol collection handler.

        Keyword arguments:
        workspace -- AtomPubWorkspace instance carrying this collection
        name_or_id -- Name or identifier by which you reference this
        collection within the store [eg: 'audio']
        base_uri -- Base URI of the content used in links and external content
        base_edit_uri -- Base URI used for the edit links
        base_media_edit_uri -- Base URI used for the edit-media links. If None will
        use base_edit_uri.
        xml_attrs -- dictionary of XML attributes belonging to the
        xml namespace (http://www.w3.org/XML/1998/namespace). They will be passed
        to the top level Atom elements (feed, entry, etc.)
        member_extension -- Extension used for the Atom entries representing
        the member resource (default to 'atom')
        member_media_type -- Media type for the Atom entries representing the
        member resource default to 'application/atom+xml')
        accept_media_types -- List of a strings of acceptable media-types for
        this collection
        categories -- List of bridge.Element instances
        fixed_categories -- True => 'yes', False => 'no', None => undefined
        favorite -- if True means thiscollection is the preferred one in the workspace
        """
        self.workspace = workspace
        self.workspace.collections.append(self)
        self.name_or_id = name_or_id
        self.base_uri = base_uri
        xml_attrs = xml_attrs or {}
        self.xml_base = xml_attrs.get('base', None)
        self.xml_id = xml_attrs.get('id', None)
        self.xml_lang = xml_attrs.get('lang', None)
        self.title = title
        self.categories = categories
        self.favorite = favorite
        self.fixed_categories = fixed_categories
        self.member_extension = member_extension
        self.member_media_type = member_media_type
        self.base_edit_uri = base_edit_uri
        if not base_media_edit_uri:
            base_media_edit_uri = base_edit_uri
        self.base_media_edit_uri = base_media_edit_uri

        if not accept_media_types:
            accept_media_types = []
        if isinstance(accept_media_types, basestring):
            accept_media_types = [accept_media_types]
        self.accept_media_types = accept_media_types

        self.handlers_mapping = {}

        # If the accept element exists but is empty,
        # clients SHOULD assume that the Collection
        # does not support the creation of new Entries. 
        self.is_read_only = False

        # By default we don't use the cache
        self.has_cache_enabled = False
        self.cache = None
        
    def store_container(self):
        """
        Returns the store carrying this collection
        """
        return self.workspace.service.store
    store = property(store_container)

    def attach(self, member, member_content,
               member_id=None, member_path=None,
               media_id=None, media_path=None,
               media_content=None, check_media_type=True):
        """
        Add a member to this collection by
         * adding it to the store
         * adding it to the internal cache if it is enabled

        You are not forced to pass the resource content through this
        method if you prefer storing it in a different location
        without using amplee.

        This method will throw a amplee.error.FixedCategoriesError
        is self.fixed_categories is True and none of the member
        categories matches self.categories.

        This method will throw a amplee.error.UnsupportedMediaType
        exception if check_media_type is True and the media type of
        the media resource does not fall into the allowed list.
        
        Keyword arguments:
        member -- amplee.atompub.member.* instance
        member_content -- Content to be persisted into the storage.
        Usually an XML string of the Atom entry
        member_id -- Internal id used to reference this member.
        Usually equals to the resource_name + collection.member_extension
        member_path -- Path under this member is stored
        media_id -- Internal id used to reference the resource associated
        to this member.
        media_path -- Path under the media resource is stored
        media_content -- Resource content
        check_media_type -- If False the resource media type will not be
        checked against collection.accept_media_types
        """
        if not member_id:
            member_id = member.member_id
        if not media_id:
            media_id = member.media_id

        if self.fixed_categories and self.categories:
            try:
                member.atom.validate(respect_fixed_categories, test_set=self.categories)
            except BridgeValidatorException:
                raise FixedCategoriesError
        
        if media_content and check_media_type:
            # This is will be very strict and will check the media-type
            # and its parameters. Use with care.
            if not get_best_mimetype(member.media_type, self.accept_media_types,
                                     check_params=True):
                raise UnsupportedMediaType

        if not member_path:
            member_path = self.get_meta_data_path(member_id)
            
        self.store.add_meta_data(member_path, member_content,
                                 media_type=self.member_media_type,
                                 member_id=member_id,
                                 media_id=media_id,
                                 collection_name=self.name_or_id)
        if media_content:
            if not media_path:
                media_path = self.get_content_path(media_id)
            
            self.store.add_content(media_path, media_content,
                                   media_type=member.media_type,
                                   member_id=member_id,
                                   media_id=media_id,
                                   collection_name=self.name_or_id)
        
        self.cache_data(member_id, member)
        
    def prune(self, member_id=None, media_id=None):
        """
        Removes a member from a collection and the underlying stor.
        Removes only objects passed in the id parameters.
        
        Keyword arguments:
        member_id -- Identifier of the member
        media_id -- Identifier of the media resource
        """
        if member_id:
            self.prune_data(member_id)
            path = self.get_meta_data_path(member_id)
            if self.store.exists(path):
                self.store.remove_meta_data(path)

        if media_id:
            path = self.get_content_path(media_id)
            if self.store.exists(path, as_media=True):
                self.store.remove_content(path)

    def convert_id(self, id):
        """
        Take the parameter and returns a tuple such as (member_id, media_id).

        Keyword arguments:
        id -- Can be either member_id or media_id
        
        """
        if self.member_extension:
            ext = '.%s' % self.member_extension
            pos_ext = 0 - len(ext)
            if id[pos_ext:] == ext:
                return (id, id[:pos_ext])
            return ('%s.%s' % (id, self.member_extension), id)
        return (id, id)
    
    def contains(self, path, as_media=False):
        """
        Does a path belong to the store

        keyword arguments:
        path -- a path object as returned by get_meta_data_path or get_content_path
        as_media -- True when the path is to be tested on the media storage
        """
        return self.store.exists(path, as_media=as_media)

    def get_meta_data_path(self, id):
        """
        Constructs and returns the path to the member
        pointed by the id parameter.
        """
        return self.store.get_meta_data_path(self.name_or_id, id)

    def get_content_path(self, id):
        """
        Constructs and returns the path to the resource
        pointed by the id parameter.
        """
        return self.store.get_content_path(self.name_or_id, id)

    def get_meta_data(self, path):
        """
        Returns an object stored as meta-data information in the store
        for a resource. 

        Does not check the existence of the resource.
        """
        return self.store.fetch_meta_data(path)
        
    def get_content(self, path):
        """
        Returns the content of the media resource or None.
        
        Does not check the existence of the resource.
        """
        return self.store.fetch_content(path)

    def get_base_edit_uri(self):
        if self.xml_base:
            return u'%s/%s' % (self.xml_base, self.base_edit_uri)
        return self.base_edit_uri

    def get_base_media_edit_uri(self):
        if self.xml_base:
            return u'%s/%s' % (self.xml_base, self.base_media_edit_uri)
        return self.base_media_edit_uri

    def to_feed(self, prefix=ATOM10_PREFIX, namespace=ATOM10_NS, limit=0):
        """
        Transforms and returns the collection into an bridge.Element instance

        Careful as this can be quite an expansive call.
        """
        feed = Element(u'feed', prefix=prefix, namespace=namespace)
        base_uri = self.base_uri
        if self.xml_base:
            base_uri = "%s%s" % (self.xml_base, base_uri)
        base_uri = base_uri.encode('utf-8')
        uuid = generate_uuid_uri(seed=base_uri)
        Element(u'id', content=uuid, prefix=prefix, namespace=namespace, parent=feed)
        Element(u'updated', content=get_isodate(),
                prefix=prefix, namespace=namespace, parent=feed)
        title = Element(u'title', content=unicode(self.title), attributes={u'type': u'text'},
                        prefix=prefix, namespace=namespace, parent=feed)
        if self.xml_base:
            Attribute(u'base', self.xml_base, prefix=XML_PREFIX,
                      namespace=XML_NS, parent=feed)
        if self.xml_lang:
            Attribute(u'lang', self.xml_lang, prefix=XML_PREFIX,
                      namespace=XML_NS, parent=feed)
        feed.entry = []
        members = self.store.list_members(self.name_or_id, self.member_extension)
        count = 0
        for member_id in members:
            member_path = members[member_id]['path']
            member = self.load_member(member_id, member_path)
            member.atom.parent = feed
            feed.xml_children.append(member.atom)
            feed.entry.append(member.atom)

            if limit:
                count = count + 1
                if count > limit:
                    break

        return feed
    feed = property(to_feed)

    def to_collection(self, prefix=ATOMPUB_PREFIX, namespace=ATOMPUB_NS):
        """
        Tranforms and returns the collection into a bridge.Element instance
        """
        collection = Element(u'collection', attributes={u'href': self.base_edit_uri},
                             prefix=prefix, namespace=namespace)
        if self.xml_base:
            Attribute(u'base', self.xml_base, prefix=XML_PREFIX,
                      namespace=XML_NS, parent=collection)
        if self.xml_lang:
            Attribute(u'lang', self.xml_lang, prefix=XML_PREFIX,
                      namespace=XML_NS, parent=collection)
        Element(u'title', content=self.title, attributes={u'type': u'text'},
                prefix=ATOM10_PREFIX, namespace=ATOM10_NS, parent=collection)
        accept = Element(u'accept', prefix=prefix, namespace=namespace, parent=collection)
        if self.is_read_only is False:
            accept.xml_text = u','.join(self.accept_media_types)
            
        categories = self.categories or []
        if categories:
            attr = {}
            fixed = None
            if self.fixed_categories == True: fixed = u'yes'
            elif self.fixed_categories == False: fixed = u'no'
            if fixed is not None:
                attr[u'fixed'] = fixed
            cats = Element(u'categories', attributes=attr, prefix=prefix,
                           namespace=namespace, parent=collection)
            for category in categories:
                Element(u'category', attributes={u'term': unicode(category.term)},
                        prefix=ATOM10_PREFIX, namespace=ATOM10_NS, parent=cats) 

        return collection
    collection = property(to_collection)

    def get_member(self, member_id):
        """
        Returns the requested member or None if not found.
        If the member is not in the collection cache, amplee
        attempts to load it from the storage.

        Keyword argument:
        member_id -- identifier of the member as returned by convert_id()
        """
        member = None
        if self.has_cache_enabled:
            try:
                member = self.get_cached(member_id)
            except KeyError:
                pass
            
        if member is None:
            try:
                member = self.load_member(member_id)
                if member:
                    self.cache_data(member_id, member)
            except UnknownResource:
                pass
            
        return member
    
    def load_member(self, member_id, member_path=None):
        """
        Loads a member from its storage and returns it or None
        if not found. The member is not added to the collection
        cache at this stage.

        Keyword argument:
        member_id -- identifier of the member as returned by convert_id()
        member_path -- path as returned by get_meta_data_path(), if not
        provided it will be computed automatically
        """
        if not member_path:
            member_path = self.get_meta_data_path(member_id)
            
        if not self.contains(member_path):
            raise UnknownResource, "Could not find '%s'" % member_id
        
        data = self.get_meta_data(member_path)
        
        member_ext = None
        if self.member_extension:
            member_ext = '.%s' % self.member_extension
            
        if data:
            entry = Element.load(data, as_attribute=atom_as_attr, as_list=atom_as_list,
                                 as_attribute_of_element=atom_attribute_of_element).xml_root
            member = EntryMember(self, id=member_id, atom=entry)
                
            # we guess the media-type of the media resource from
            # the edit-media link if any
            edit_media_links = entry.filtrate(lookup_links, rel=u'edit-media')
            if edit_media_links:
                link = edit_media_links[0].get_attribute('type')
                if link:
                    member.media_type = unicode(link)
                
            if member_ext:
                pos = member.member_id.rfind(member_ext)
                if pos == -1:
                    member.media_id = member.member_id
                else:
                    member.media_id = member.member_id[:pos]
            else:
                member.media_id = member.member_id

            return member

    def reload_members(self, limit=0):
        """
        Reloads all or part of existing members.
        Call this at server startup to refresh the collection.
        Careful as this could be a fairly long process.

        Returns the list of loaded members.
        
        Keyword argument
        limit -- indicate how many members you want to load
        By default 0 means all of them.
        """
        results = []
        members = self.store.list_members(self.name_or_id, self.member_extension)
        count = 0
        for member_id in members:
            member_path = members[member_id]['path']
            member = self.load_member(member_id, member_path)

            if member:
                results.append(member)
                self.cache_data(member_id, member)

            if limit:
                count = count + 1
                if count > limit:
                    break
        return results
    
    def reload_members_from_feed(self, source):
        """
        Reloads members from a an atom feed. This can be useful if you
        want to keep in the collection cache some given entries. You can construct
        a feed of those member entries and provide it to this method.

        Returns the list of loaded members.
        
        Keyword argument:
        source -- can be a string, a path or a file object representing the feed
        """
        feed = Element.load(source, as_attribute=atom_as_attr, as_list=atom_as_list,
                            as_attribute_of_element=atom_attribute_of_element).xml_root

        results = []
        for entry in feed.entry:
            edit_link = entry.filtrate(lookup_links, rel=u'edit')
            if edit_link:
                edit_link = edit_link[0]
                member_id = urlparse(edit_link.href)[2].rsplit('/', 1)
                member = self.get_member(member_id)
                
                if member:
                    results.append(member)
                    self.cache_data(member_id, member)

        return results
    
    def reload_members_from_list(self, member_ids):
        """Given a list of member_ids this will return a list of loaded members.
        """
        results = []
        for member_id in member_ids:
            member = self.load_member(member_id)
            
            if member:
                results.append(member)
                self.cache_data(member_id, member)

        return results
    
    def register_handler(self, handler):
        """
        Registers the provided handler to be available to this
        collection.

        Keyword arguments:
        handler -- class instance which must have an attribute
        'member_type' which is an instance of the
        amplee.handler.MemberType class

        A handler is the instance of a class that may have the following
        callable attributes:

        on_error(exception, member) -> None
        on_create(member, content) -> member, content
        on_update(existing_member, new_member, new_content) -> new_member, new_content
        on_delete(member) -> None
        on_get_content(member, content, content_type) -> member, content, content_type
        on_get_atom(member) -> member, content, content_type

        If present those callbacks will be automatically applied.
        Not being present doesn't constitute a fault.
        """
        self.handlers_mapping[handler.member_type.media_type] = handler

    def dispatch(self, operation, media_type, *args, **kwargs):
        """
        Dispatches the operation based for that media-type
        to the matching handler

        Keyword arguments:
        operation -- name of the callback handler
        media_type -- media-type string

        The *args and **kwargs dicts will be passed to the callback.

        Returns the result of the callback.
        """
        if media_type in self.handlers_mapping:
            handler = self.handlers_mapping[media_type]
            handler = getattr(handler, operation, None)
            if handler is not None:
                return handler(*args, **kwargs)
            
    def get_handler(self, media_type):
        """
        Returns a tuple (handler, memember_type) based on the
        provided media_type

        Raises a RuntimError if it cannot be found
        
        Keyword argument:
        media_type -- media-type string
        """
        if media_type in self.handlers_mapping:
            handler = self.handlers_mapping[media_type]
            mt = handler.member_type.deepcopy()
            return handler, mt

        raise RuntimeError, "Missing handler for %s" % media_type

    # Some helpers to access the cache

    def set_cache(self, cache):
        """
        Set the cache object which will be used internally.
        The only requirement is to implement the __Xet_item__
        interface so that it can be accessed as a mapping.

        Keyword argument:
        cache -- instance
        """
        self.cache = cache
        self.has_cache_enabled = True

    # The following methods needn't to be called from your code
    # as you should not have to access the cache
    # This is more an internal API

    # Note that amplee caches only a few information regarding
    # a member.

    def cache_data(self, key, data):
        if self.has_cache_enabled and self.cache:
            self.cache[key] = data

    def prune_data(self, key):
        if self.has_cache_enabled and self.cache:
            del self.cache[key]

    def get_cached(self, key):
        if self.has_cache_enabled and self.cache:
            obj = self.cache[key]
            if obj is EntryMember:
                obj.collection = self
                return obj
            
        raise KeyError
        
