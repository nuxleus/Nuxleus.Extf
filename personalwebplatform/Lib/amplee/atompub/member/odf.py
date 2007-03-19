# -*- coding: utf-8 -*-

__all__ = ['ODTMember']

import os
from xml.sax.saxutils import escape
from zipfile import ZipFile

from bridge import Element, Attribute
from bridge.common import ATOM10_PREFIX, ATOMPUB_PREFIX, XML_PREFIX, XML_NS, \
     ATOM10_NS, ATOMPUB_NS, XHTML1_NS, XHTML1_PREFIX, ODF_OFFICE_NS,\
     ODF_META_NS, DC_NS, odf_office_as_attr, odf_meta_as_list

from amplee.utils import generate_uuid_uri
from amplee.utils import create_temporary_resource, delete_temporary_resource
from amplee.error import MemberMediaError
from amplee.atompub.member import MediaMember
from amplee.atompub.member.helper import MemberHelper

class ODTMember(MediaMember):
    def __init__(self, collection, source, 
                 media_type=u'application/vnd.oasis.opendocument.text',
                 inline_content=True, entry_id_creator=None, slug=None,
                 name_creator=None, existing_member=None, ext='odt', **kwargs):
        MediaMember.__init__(self, collection, media_type=media_type)

        size = content = None
        try:
            fd, path, content = create_temporary_resource(source)
            zp = ZipFile(path)
            self.media_type = unicode(zp.read('mimetype'))
            meta = zp.read('meta.xml')
            if inline_content:
                content = zp.read('content.xml')
            zp.close()
            if not inline_content:
                try:
                    size = unicode(os.stat(path).st_size)
                except:
                    pass
        finally:
            delete_temporary_resource(path)
            
        doc = Element.load(meta, as_attribute=odf_office_as_attr,
                           as_list=odf_meta_as_list).xml_root

        id = self.generate_id(existing_member, entry_id_creator, True, seed=source, slug=slug)
                
        mh = MemberHelper(self.collection, existing_member)
        mh.initiate(id=id)
        title = doc.meta.get_child('title', DC_NS)
        if title:
            title = mh.add_element('title', content=title.xml_text, attributes={u'type': u'text'})
        else:
            title = mh.add_element('title', content=u"", attributes={u'type': u'text'})
        description = doc.meta.get_child('description', DC_NS)
        if description:
            mh.add_element('summary', content=description.xml_text, attributes={u'type': u'text'})
        else:
            mh.add_element('summary', content=u"", attributes={u'type': u'text'})

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
                
        if doc.meta.has_child('creator', DC_NS):
            author = mh.add_element('author')
            mh.add_element('name', content=unicode(doc.meta.creator), parent=author)
           
        mh.add_edit_link(self.member_id)

        if doc.meta.has_child('keyword', ODF_META_NS):
            for keyword in  doc.meta.keyword:
                mh.add_element('category', attributes={u'term': keyword.xml_text})

        if inline_content:
            ct = mh.add_element(u'content', attributes={u'type': u'xhtml'})
            div = mh.add_element(u'div', prefix=XHTML1_PREFIX, ns=XHTML1_NS, parent=ct)
            if content:
                content = Element.load(content).xml_root
                content.xml_parent = div
                div.xml_children.append(content)
        else:
            mh.add_edit_media_link(self.media_id, self.media_type, length=size)
            mh.add_remote_content(self.media_id, self.media_type)
            
        mh.validate()
        self.entry = mh.entry

        del mh
