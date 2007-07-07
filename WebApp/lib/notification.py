# -*- coding: utf-8 -*-

import time
import base64
import hmac
import urllib
from urlparse import urlparse
from httplib import HTTPException
from datetime import datetime, timedelta
from boto.utils import canonical_string as s3_canonical_string

from bucker.message.llup import Link, Notification
from amplee.utils import parse_isodate, get_isodate

__all__ = ['send_sqs_blip_notification']

def send_sqs_blip_notification(blip_type, member, start, end, aws_key, aws_private_key):
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
    path = '/%s_%s/%s' % (member.collection.store.storage.unique_prefix,
                          member.collection.name_or_id, os.path.split(tokens[2])[-1])

    expires = int(time.mktime(end.timetuple()))
    s3_qs = hmac.new(aws_private_key, s3_canonical_string('GET', path, {}, expires=str(expires)), sha)
    s3_qs = urllib.quote_plus(base64.encodestring(s3_qs.digest()).strip())
    s3_href = unicode(s3_href).replace(tokens[2], '%s?AWSAccessKeyId=%s&Expires=%d&Signature=%s' % (path,
                                                                                                    aws_key,
                                                                                                    expires,
                                                                                                    s3_qs))

    n.links.append(Link(s3_href, u's3'))
    n.from_entry(entry)
    blip = n.to_entry()

    retry = 5
    while True:
        try:
            member.collection.m.push(blip)
            break
        except HTTPException:
            if retry > 5:
                break
            retry = retry + 1
            time.sleep(0.0001)
