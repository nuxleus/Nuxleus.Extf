# -*- coding: utf-8 -*-

import os.path
import time
import base64
import time
import hmac
import sha
import urllib
from urlparse import urlparse
from httplib import HTTPException
from datetime import datetime, timedelta

#from bucker.provider.sqs import Messenger
from bucker.message.llup import Link, Notification
from amplee.utils import parse_isodate, get_isodate
from bridge.filter.atom import lookup_links
from core.aws import lookup_keys

from boto.utils import canonical_string as s3_canonical_string

__all__ = ['AtomHandler']

class AtomHandler(object):
    def __init__(self, member_type):
        # The media-type that this class will handle
        self.member_type = member_type
        self.aws_key, self.aws_private_key = lookup_keys()

    def on_error(self, exception, member):
        pass

    def on_create(self, member, content):
        return member, content

    def _send_blip_notification(self, blip_type, member, start, end):
        entry = member.atom
        href = entry.filtrate(lookup_links, rel=u'edit')[0].get_attribute('href')
        n = Notification(u'create', unicode(member.collection.get_base_edit_uri()))
        n.links.append(Link(unicode(href), u'self'))
        if blip_type == 'create':
           n.published = entry.published.xml_text
        elif blip_type == 'update':
           n.updated = entry.updated.xml_text
        n.expires = get_isodate(end)
        tokens = urlparse(str(href))
        host = tokens[1]
        s3_href = unicode(href).replace(host, 's3.amazonaws.com')
        path = '/%s_%s/%s' % (member.collection.store.storage.unique_prefix, member.collection.name_or_id, os.path.split(tokens[2])[-1])

        expires = int(time.mktime(end.timetuple()))
        s3_qs = hmac.new(self.aws_private_key, s3_canonical_string('GET', path, {}, expires=str(expires)), sha)
        s3_qs = urllib.quote_plus(base64.encodestring(s3_qs.digest()).strip())
        s3_href = unicode(s3_href).replace(tokens[2], '%s?AWSAccessKeyId=%s&Expires=%d&Signature=%s' % (path, self.aws_key, expires, s3_qs))

        n.links.append(Link(s3_href, u's3'))
        n.from_entry(entry)
        blip = n.to_entry()

        retry = 5
        while True:
            try:
                member.collection.m.push(blip)
                break
            except HTTPException:
                if retry > 5: break
                retry = retry + 1
                time.sleep(0.0001)


    def on_created(self, member):
        entry = member.atom
        start = parse_isodate(entry.published.xml_text)
        end = start + timedelta(days=7)

        self._send_blip_notification(u'create', member, start, end)
                    
    def on_update(self, existing_member, new_member, new_content):
        return new_member, new_content

    def on_updated(self, member):
        entry = member.atom
        start = parse_isodate(entry.updated.xml_text)
        end = start + timedelta(days=7)

        self._send_blip_notification(u'update', member, start, end)

    def on_deleted(self, member):
        entry = member.atom
        start = parse_isodate(entry.updated.xml_text)
        end = start + timedelta(days=7)

        self._send_blip_notification(u'delete', member, start, end)

    def on_delete(self, member):
        pass

    def on_get_content(self, member, content, content_type):

        return member, content, content_type

    def on_get_atom(self, member):
        return member, member.atom.xml(), 'application/atom+xml'

