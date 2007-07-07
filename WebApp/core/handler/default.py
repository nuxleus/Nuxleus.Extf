# -*- coding: utf-8 -*-

from datetime import timedelta
from amplee.utils import parse_isodate
from amplee.atompub.member.atom import EntryResource

from lib.notification import send_sqs_blip_notification
from lib.aws import lookup_keys

__all__ = ['DefaultEntryMember', 'DefaultHandler']

class DefaultEntryMember(EntryResource):
    def __init__(self, collection, **kwargs):
        EntryResource.__init__(self, collection, **kwargs)

    def generate_atom_id(self, entry, slug=None):
        if not slug:
            slug = entry.get_child('title', entry.xml_ns).xml_text

        return u'%s%s' % (collection.get_base_uri(), slug.replace(' ', '-'))

    def generate_resource_id(self, entry, slug=None):
        if slug:
            return slug.replace(' ','_').decode('utf-8')
    
        return entry.get_child('title', entry.xml_ns).xml_text.replace(' ','_')
    
class DefaultHandler(object):
    def __init__(self, member_type):
        # The media-type that this class will handle
        self.member_type = member_type
        self.aws_key, self.aws_private_key = lookup_keys(self.member_type.params['aws_path'])

    def on_update_feed(self, member):
        member.collection.feed_handler.set(member.collection.feed)
        
    def on_created(self, member):
        entry = member.atom
        start = parse_isodate(entry.published.xml_text)
        end = start + timedelta(days=7)

        send_blip_notification(u'create', member, start, end,
                               self.aws_key, self.aws_private_key)
              
    def on_updated(self, member):
        entry = member.atom
        start = parse_isodate(entry.updated.xml_text)
        end = start + timedelta(days=7)

        send_blip_notification(u'update', member, start, end,
                               self.aws_key, self.aws_private_key)

    def on_deleted(self, member):
        entry = member.atom
        start = parse_isodate(entry.updated.xml_text)
        end = start + timedelta(days=7)

        send_blip_notification(u'delete', member, start, end,
                               self.aws_key, self.aws_private_key)
