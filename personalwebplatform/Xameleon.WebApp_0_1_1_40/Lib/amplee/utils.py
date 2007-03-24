# -*- coding: utf-8 -*-

__doc__ = """Utility functions.

Synopsis
--------

Just a list of utility functions that help you for common tasks.

"""
__all__ = ['generate_uuid', 'generate_uri', 'generate_uuid_uri',
           'get_isodate', 'parse_isodate', 'decode_slug', 'encode_slug']

import os
import os.path
import md5
import uuid
import datetime
import tempfile
try:
    # Python 2.5
    from email.header import Header, decode_header
except ImportError:
    # Python 2.4
    from email.Header import Header, decode_header
    
from bridge.lib import isodate

def generate_uuid(seed=None):
    """Returns a uuid value as specified in RFC 4122.

    The ``seed``, if provided, allows this function to be idempotent.
    When not provided a random value is returned each time.
    """
    if not seed:
        return unicode(uuid.uuid4())
    else:
        return unicode(uuid.uuid5(uuid.NAMESPACE_URL, seed))
    
def generate_uri(scheme, authority, trail):
    """Builds an URI as an unicode object
    
    The ``scheme`` is URI scheme (e.g. http)
    The ``authority`` is the hostname
    The ``trail`` is the URI path
    """
    uri = u"%s:%s:%s" % (scheme, authority, trail)
    return unicode(uri)

def generate_uuid_uri(seed=None):
    """Returns an URN from uuid
    
    The ``seed``, if provided, allows this function to be idempotent.
    When not provided a random value is returned each time.
    """
    return generate_uri('urn', 'uuid', generate_uuid(seed))

def compute_etag(seed):
    """ Returns a MD5 hash value of seed
    """
    return '"%s"' % md5.new(seed).hexdigest()

def get_isodate(dt=None):
    """Returns a date respecting the ISO 8601 format

    If ``dt`` is not provided the current UTC time is used.
    """
    if not dt:
        dt = datetime.datetime.utcnow()
    return unicode(dt.isoformat() + 'Z')

def parse_isodate(value):
    """Attempts to parse value as an ISO date and if succeeding returns
    a datetime object, ``None`` otherwise.

    The ``value`` is a date in a string format.
    """
    if not value:
        return None
        
    dt = None
    try:
        dt = isodate.parse(value)
    except ValueError:
        value = value.replace(' ', 'T')
        if value[-1] != 'Z':
            value = value + 'Z'
        try:
            dt = isodate.parse(value)
        except ValueError:
            pass
    if dt:
        return datetime.datetime.utcfromtimestamp(dt)
    
    return None

def create_temporary_resource(content):
    """Dumps the content into a temp file and returns
    the file handle, the absolute path of the temp file
    as well as the content as a string.
    """
    fd, path = tempfile.mkstemp()
    f = file(path, 'wb')
    f.write(content)
    f.close()

    return fd, path, content

def delete_temporary_resource(path):
    """Safely removes path.
    """
    if not path:
        return

    try:
        os.unlink(path)
    except OSError, IOError:
        pass

def decode_slug(slug):
    """Decodes and returns the provided slug value

    For example:
    =?iso-8859-1?q?The_Beach?= will return The_Beach

    The ``slug`` is a string value as described by the
    Atom Publishing Protocol.
    """
    if slug is None:
        return
    tokens = decode_header(slug)
    return tokens[0][0]

def encode_slug(unicode_text, encoding='iso-8859-1'):
    """Returns a valid slug header as a string.

    The ``unicode_text`` is a unicode object of the text to
    encode.
    The ``encoding`` is encoding used on the ``unicode_text`` object
    """
    h = Header(unicode_text, encoding)
    return str(h)
