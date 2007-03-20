# -*- coding: utf-8 -*-

__doc__ = """Helper to find the best matching media-types.

Synopsis
--------

The Atom Publishing Protocol is very sensitive to the media-type
of resources it treats. Doing a simple string comparison is
certainly not enough in this case since media-types sent by
user-agents can be of different forms. This module provides only
one method called `get_best_mimetype` which does all the hard work
of finding the best matching media-type given a seed and a list
of acceptable media-types.

"""

__all__ = ['get_best_mimetype']

###################################################################
# stolen from CherryPy :)
import re

class HeaderElement(object):
    """An element (with parameters) from an HTTP header's element list."""
    
    def __init__(self, value, params=None):
        self.value = value
        if params is None:
            params = {}
        self.params = params
    
    def __unicode__(self):
        p = [";%s=%s" % (k, v) for k, v in self.params.iteritems()]
        return u"%s%s" % (self.value, "".join(p))
    
    def __str__(self):
        return str(self.__unicode__())
    
    def parse(elementstr):
        """Transform 'token;key=val' to ('token', {'key': 'val'})."""
        # Split the element into a value and parameters. The 'value' may
        # be of the form, "token=token", but we don't split that here.
        atoms = [x.strip() for x in elementstr.split(";")]
        initial_value = atoms.pop(0).strip()
        params = {}
        for atom in atoms:
            atom = [x.strip() for x in atom.split("=", 1) if x.strip()]
            key = atom.pop(0)
            if atom:
                val = atom[0]
            else:
                val = ""
            params[key] = val
        return initial_value, params
    parse = staticmethod(parse)
    
    def from_str(cls, elementstr):
        """Construct an instance from a string of the form 'token;key=val'."""
        ival, params = cls.parse(elementstr)
        return cls(ival, params)
    from_str = classmethod(from_str)


q_separator = re.compile(r'; *q *=')

class AcceptElement(HeaderElement):
    """An element (with parameters) from an Accept-* header's element list."""
    
    def from_str(cls, elementstr):
        qvalue = None
        # The first "q" parameter (if any) separates the initial
        # parameter(s) (if any) from the accept-params.
        atoms = q_separator.split(elementstr, 1)
        initial_value = atoms.pop(0).strip()
        if atoms:
            # The qvalue for an Accept header can have extensions. The other
            # headers cannot, but it's easier to parse them as if they did.
            qvalue = HeaderElement.from_str(atoms[0].strip())
        
        ival, params = cls.parse(initial_value)
        if qvalue is not None:
            params["q"] = qvalue
        return cls(ival, params)
    from_str = classmethod(from_str)
    
    def qvalue(self):
        val = self.params.get("q", "1")
        if isinstance(val, HeaderElement):
            val = val.value
        return float(val)
    qvalue = property(qvalue, doc="The qvalue, or priority, of this value.")
    
    def __cmp__(self, other):
        # If you sort a list of AcceptElement objects, they will be listed
        # in priority order; the most preferred value will be first.
        diff = cmp(other.qvalue, self.qvalue)
        if diff == 0:
            diff = cmp(str(other), str(self))
        return diff


###################################################################

def get_best_mimetype(header_value, within, default=None, check_params=False, return_full=False):
    """Iterates through 'header_value' and checks if it finds any match in 'within'.

    When */* is part of header_value and no candidate was found this
    function returns the first media-type of 'within'

    Consider the following examples:

    >>> from amplee.http_helper import get_best_mimetype
    >>> l = ['application/rdf+xml', 'application/atom+xml']
    >>> a = 'text/xml,application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5'
    >>> get_best_mimetype(a, l)
    >>> 'application/rdf+xml'
    >>> get_best_mimetype(a, l, 'application/atom+xml')
    'application/atom+xml'
    >>> a = 'text/xml,application/xml,application/atom+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5'
    >>> get_best_mimetype(a, l)
    'application/atom+xml'
    >>> l = ['application/rdf+xml', 'entry']
    >>> get_best_mimetype(a, l)
    'application/atom+xml'
    >>> a = 'text/xml,application/xml,text/html;q=0.9,text/plain;q=0.8,image/png'
    >>> get_best_mimetype(a, l) # returns None
    >>> a = 'application/xml;q=0.9,application/rdf+xml;q=0.8,application/atom+xml'
    >>> l = ['application/rdf+xml', 'entry']
    >>> get_best_mimetype(a, l)
    'application/atom+xml'
    >>> a = 'application/xml;q=0.9,application/rdf+xml;q=0.8,application/atom+xml;q=0.1'
    >>> get_best_mimetype(a, l)
    'application/rdf+xml'
    >>> a = 'application/xml,application/rdf+xml;q=0.8,application/atom+xml'
    >>> get_best_mimetype(a, l)
    'application/atom+xml'
    >>> l = ['application/rdf+xml', 'application/atom+xml', 'application/xhtml+xml'] 
    >>> a = 'application/*'
    >>> get_best_mimetype(a, l)
    >>> 'application/xhtml+xml'
    >>> l = [u'application/atom+xml;type=entry', u'application/x-www-form-urlencoded']
    >>> a = u'application/atom+xml;type=entry;some=yu'
    >>> get_best_mimetype(a, l, check_params=True)
    >>> a = u'application/atom+xml;type=entry'
    >>> get_best_mimetype(a, l, check_params=True)
    u'application/atom+xml'
    >>> get_best_mimetype(a, l, check_params=True, return_full=True)
    u'application/atom+xml;type=entry'
    >>> a = u'application/atom+xml;type=entry;some=yu'
    >>> get_best_mimetype(a, l, check_params=['type'])
    u'application/atom+xml'
    >>> get_best_mimetype(a, l, check_params=['type'], return_full=True)
    u'application/atom+xml;some=yu;type=entry'
    >>> get_best_mimetype(a, l)
    u'application/atom+xml'

    The ``header_value`` is a string respecting the HTTP Accept header format
    as defined in section 14.1 of RFC 2616.
    
    The ``within`` argument is a list of acceptable media-type strings.
    
    The ``default`` value is returned when no match was found.
    
    The ``check_params``, if provided, may be a list of keys (string) that
    should be matched between ``header_value`` and headers ``within`` or it
    can be a boolean. If ``True`` then every parameters will be tested, if
    ``False`` (default) the test won't occur. Setting it to ``True`` ensures
    that if a match is found it will be exactly the one wanted but this is
    a more restrictive matching scheme.
    
    If ``return_full`` is ``True`` it returns the media-type along with its
    parameters. Otherwise it returns only the ``media-type``.
    """
    if not header_value:
        return default
    
    tokens = [token.strip() for token in header_value.split(',')]

    candidate = None
    match_any = False
    for token in tokens:
        header = AcceptElement.from_str(token)
        if header.value == '*/*':
            match_any = True
        
        header_media_type = header_sub_type = None
        if '/' in header.value:
            header_media_type, header_sub_type = header.value.split('/')

        header_left_token = header_right_token = None
        if header_sub_type and '+' in header_sub_type:
            header_left_token, header_right_token = header_sub_type.split('+')

        for item in within:
            mimetype = None
            # specific case for APP
            if item == 'entry':
                item = AcceptElement.from_str('application/atom+xml')
            else:
                item = AcceptElement.from_str(item)

            if '/' in item.value:
                media_type, sub_type = item.value.split('/')
                if header_media_type == media_type:
                    if sub_type == '*':
                        mimetype = header
                    elif '+' in sub_type:
                        left_token, right_token = sub_type.split('+')
                        if right_token == header_right_token:
                            if left_token == header_left_token:
                                mimetype = header
                            elif header_left_token == '*':
                                mimetype = item
                    elif header_sub_type == sub_type:
                        mimetype = header

                    if not mimetype and header_sub_type == '*':
                        mimetype = item
            elif header.value == item.value:
                mimetype = header

            if not mimetype:
                continue

            if check_params != False:
                with_param = False
                if check_params == True:
                    header_params = header.params.keys()[:]
                    item_params = item.params.keys()[:]
                    with_param = True
                    if header_params and item_params:
                        with_param = False
                        header_params.sort()
                        item_params.sort()
                        if header_params == item_params:
                            for key in item_params:
                                if header.params[key] == item.params[key]:
                                    with_param = True
                                else:
                                    with_param = False
                                    break
                elif isinstance(check_params, list):
                    if not header.params and not item.params:
                        with_param = True
                    else:
                        parameters = check_params[:]
                        for key in parameters:
                            if key in header.params and key in item.params:
                                if header.params[key] == item.params[key]:
                                    with_param = True
                                else:
                                    with_param = False
                                    break
                if with_param and not candidate:
                    candidate = mimetype
                elif with_param and mimetype.qvalue >= candidate.qvalue:
                    candidate = mimetype
            elif not candidate:
                candidate = mimetype
            elif mimetype and mimetype.qvalue >= candidate.qvalue:
                candidate = mimetype

    if not candidate and match_any and within:
        if not default:
            candidate = AcceptElement.from_str(within[0])
        else:
            candidate = AcceptElement.from_str(default)
    elif not candidate and default:
        candidate = AcceptElement.from_str(default)

    if not candidate:
        return None
    
    if return_full:
        return unicode(candidate)
    
    return candidate.value
