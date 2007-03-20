# -*- coding: utf-8 -*-

import os

__all__ = ['XHTMLMember']

from bridge import Element, Attribute
from bridge.filter.xhtml import extract_meta
from bridge.common import ATOM10_PREFIX, ATOMPUB_PREFIX, XML_PREFIX, XML_NS, \
     ATOM10_NS, ATOMPUB_NS, XHTML1_NS, XHTML1_PREFIX, xhtml_as_attr

from amplee.error import MemberMediaError
from amplee.atompub.member import MediaMember
from amplee.atompub.member.helper import MemberHelper

class XHTMLMember(MediaMember):
    def __init__(self, collection, source=None, 
                 resource_id_generator=None, slug=None, ext=u'xhtml',
                 media_type=u'application/xhtml+xml', inline_content=True,
                 entry_id_creator=None, existing_member=None, name_creator=None, **kwargs):
        MediaMember.__init__(self, collection, media_type=media_type)

        meta_only = not inline_content
        doc = Element.load(source, as_attribute=xhtml_as_attr).xml_root

        id = self.generate_id(existing_member, entry_id_creator, True, html_doc=doc, ext=ext)

        meta = doc.head.filtrate(extract_meta)

        mh = MemberHelper(self.collection, existing_member)
        mh.initiate(id=id)
        title = doc.head.get_child('title', XHTML1_NS) or u''
        title = mh.add_element('title', content=title.xml_text, attributes={u'type': u'text'})
        description = meta.get('description', u'')
        mh.add_element('summary', description, attributes={u'type': u'text'})

        if not existing_member:
            if callable(name_creator):
                media_id = member_id = name_creator(self.collection, slug, title.xml_text)
            else:
                media_id = member_id = title.xml_text or slug
            if ext:
                media_id = '%s.%s' % (media_id, ext)
            member_id = u'%s.%s' % (media_id, self.collection.member_extension)
            self.member_id = member_id
            self.media_id = media_id
        else:
            self.member_id = existing_member.member_id
            self.media_id = existing_member.media_id

        creator = meta.get('author', u'')
        author = mh.add_element('author')
        mh.add_element('name', content=creator, parent=author)

        mh.add_edit_link(self.member_id)
        
        if 'keywords' in meta:
            keywords = meta['keywords'].split(' ')
            for keyword in keywords:
                attr = {u'term': keyword}
                mh.add_element('category', attributes=attr)
           
        if inline_content:
            content = doc.get_child('body', XHTML1_NS)
            if content:
                ct = mh.add_element('content', attributes={u'type': u'xhtml'})
                div = Element('div', prefix=content.xml_prefix, namespace=XHTML1_NS, parent=ct)
                mh.copy_element('body', source=doc, destination=div, ns=XHTML1_NS)
            else:
                mh.add_element('content', attributes={u'type': u'text'})
        else:
            mh.add_edit_media_link(self.media_id, self.media_type)
            mh.add_remote_content(self.media_id, self.media_type)

        mh.validate()
        self.entry =mh. entry
        del mh
