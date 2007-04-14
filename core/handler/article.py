# -*- coding: utf-8 -*-

import time
from httplib import HTTPException
from datetime import datetime, timedelta

#from bucker.provider.sqs import Messenger
from bucker.message import *
from amplee.utils import parse_isodate, get_isodate

__all__ = ['AtomHandler']

class AtomHandler(object):
    def __init__(self, member_type):
        # The media-type that this class will handle
        self.member_type = member_type

    def on_error(self, exception, member):
        pass

    def create_standing_blip_from_entry(self, entry):
        blip = create_standing_blip(origin=u"unknown", id=entry.id.xml_text)

        start = parse_isodate(entry.published.xml_text)
        end = start + timedelta(days=1)
        llup_time_span(get_isodate(start), end=get_isodate(end), parent=blip)

        return blip

    def on_create(self, member, content):
        return member, content

    def on_created(self, member):
        blip = self.create_standing_blip_from_entry(member.atom)

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

