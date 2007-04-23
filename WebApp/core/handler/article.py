# -*- coding: utf-8 -*-

import time
from httplib import HTTPException
from datetime import datetime, timedelta

#from bucker.provider.sqs import Messenger
from bucker.message.llup import Notification
from amplee.utils import parse_isodate, get_isodate
from bridge.filter.atom import lookup_links

__all__ = ['AtomHandler']

class AtomHandler(object):
    def __init__(self, member_type):
        # The media-type that this class will handle
        self.member_type = member_type

    def on_error(self, exception, member):
        pass

    def on_create(self, member, content):
        return member, content

    def on_created(self, member):
        entry = member.atom
        start = parse_isodate(entry.published.xml_text)
        end = start + timedelta(days=1)

        href = entry.filtrate(lookup_links, rel=u'edit')[0].get_attribute('href')
        n = Notification(u'sylvain', get_isodate(end), unicode(href))
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
                
    def on_update(self, existing_member, new_member, new_content):
        return new_member, new_content

    def on_delete(self, member):
        pass

    def on_get_content(self, member, content, content_type):

        return member, content, content_type

    def on_get_atom(self, member):
        return member, member.atom.xml(), 'application/atom+xml'

