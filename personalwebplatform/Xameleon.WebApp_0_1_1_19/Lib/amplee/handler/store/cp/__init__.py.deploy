# -*- coding: utf-8 -*-

__doc__ = """
CherryPy interface to amplee.handler and to be used in your
CherryPy 3 application (http://www.cherrypy.org).
"""
import urllib

import cherrypy

from amplee.error import *
from amplee.atompub.member import EntryMember
from amplee.handler import MemberHandler, MemberType
from amplee.http_helper import get_best_mimetype
from amplee.utils import decode_slug
from bridge.filter.atom import lookup_links

class Service(object):
    exposed = True
    
    def __init__(self, service):
        self.service = service

    @cherrypy.tools.etags(autotags=True)
    def GET(self):
        cherrypy.response.headers['Content-Type'] = 'application/atomserv+xml'
        return self.service.service.xml()

class Store(object):
    exposed = True
    
    def __init__(self, collection, strict=True):
        self.collection = collection
        self.strict = strict

    def HEAD(self, *args, **kwargs):
        content = self.GET(*args, **kwargs)
        cherrypy.response.headers['Content-Length'] = len(content)

    def GET(self, *args, **kwargs):
        if len(args) == 0:
            cherrypy.response.headers['Content-Type'] = 'application/atom+xml;type=feed'
            return self.collection.feed.xml()

        rid = args[-1]

        member_id, media_id = self.collection.convert_id(rid)
        member = self.collection.get_member(member_id)
        if not member:
            raise cherrypy.NotFound

        accept = None
        if self.strict:
            check_params = False
            if rid == member_id:
                # When dealing with the Atom media-type we additionally check
                # for the parameter type
                check_params = True
            accept_header = cherrypy.request.headers.get('Accept', '')
            accept = get_best_mimetype(accept_header, self.collection.accept_media_types,
                                       check_params=check_params)
            if not accept:
                raise cherrypy.HTTPError(406, 'Not Acceptable')

        try:
            if rid == member_id:
                content, content_type = MemberHandler.get_atom(self.collection, member)
            else:
                content, content_type = MemberHandler.get_content(self.collection, member)
        except UnknownResource:
            raise cherrypy.NotFound
        except ResourceOperationException, roe:
            raise cherrypy.HTTPError(roe.code, roe.msg)

        cherrypy.response.headers['Content-Type'] = content_type
        return content

    def DELETE(self, rid):
        member_id, media_id = self.collection.convert_id(rid)
        member = self.collection.get_member(member_id)
        if member:
            try:
                MemberHandler.delete(self.collection, member)
            except ResourceOperationException, roe:
                raise cherrypy.HTTPError(roe.code, roe.msg)
            
            self.collection.store.commit(message="Deleting %s and %s" % (member_id, media_id))

    def POST(self):
        if self.collection.is_read_only:
            raise cherrypy.HTTPError(400, 'Creation of resources not accepted to this collection')
        
        ct = cherrypy.request.headers.get('Content-Type')
        if not ct:
            raise cherrypy.HTTPError(400, 'Missing content type')

        length = cherrypy.request.headers.get('Content-Length')
        if not length:
            raise cherrypy.HTTPError(411, 'Length Required')
            
        check_params = False
        if self.strict:
            # In this case we also validate all the parameters
            check_params = True
        accept = get_best_mimetype(ct, self.collection.accept_media_types,
                                   check_params=check_params, return_full=self.strict)
        if not accept:
            raise cherrypy.HTTPError(415, 'Unsupported Media Type')

        handler, mt = self.collection.get_handler(accept)

        try:
            length = int(length)
            fileobj = cherrypy.request.body
            mt.params['media_type'] = unicode(accept)
            mt.params['slug'] = decode_slug(cherrypy.request.headers.get('slug', None))
            member = MemberHandler.create_from_stream(self.collection, mt, fileobj, length, accept)
        except UnsupportedMediaType:
            raise cherrypy.HTTPError(415, 'Unsupported Media Type')
        except FixedCategoriesError:
            raise cherrypy.HTTPError(400, 'The member categories do not meet the collection requirement')
        except ResourceOperationException, roe:
            raise cherrypy.HTTPError(roe.code, roe.msg)
        
        if accept == self.collection.member_media_type:
            message = 'Adding %s' % member.member_id
        else:
            message='Adding %s and %s' % (member.member_id, member.media_id)
            
        self.collection.store.commit(message=message)

        cherrypy.response.status = '201 Created'
        entry = member.atom
        edit_link = entry.filtrate(lookup_links, rel=u'edit')
        if edit_link:
            edit_link = edit_link[0]
            location = edit_link.get_attribute('href')
            if location:
                location = '%s%s' % (self.collection.xml_base or '', str(location))
                cherrypy.response.headers['Location'] = location
                cherrypy.response.headers['Content-Location'] = location
            
        cherrypy.response.headers['Content-Type'] = self.collection.member_media_type
        return entry.xml()

    def PUT(self, *args, **kwargs):
        if len(args) == 0:
            raise cherrypy.HTTPError(400, "Missing identifier")

        ct = cherrypy.request.headers.get('Content-Type')

        if not ct:
            raise cherrypy.HTTPError(400, "Missing content type")

        check_params = False
        if self.strict:
            check_params = True
        accept = get_best_mimetype(ct, self.collection.accept_media_types,
                                   check_params=check_params, return_full=self.strict)
        if not accept:
            raise cherrypy.HTTPError(415, 'Unsupported Media Type')

        rid = args[-1]

        member_id, media_id = self.collection.convert_id(rid)
        member = self.collection.get_member(member_id)
        if not member:
            raise cherrypy.NotFound
  
        handler, mt = self.collection.get_handler(accept)

        length = cherrypy.request.headers.get('Content-Length')
        if not length:
            raise cherrypy.HTTPError(411, 'Length Required')

        length = int(length)
        fileobj = cherrypy.request.body

        try:
            mt.params['media_type'] = unicode(accept)
            member = MemberHandler.update_content_from_stream(self.collection, mt, member, fileobj, length)
            message = 'Updating %s' % member.member_id
            if member.media_id:
                message = '%s and %s' % (message, member.media_id)
            self.collection.store.commit(message=message)
        except UnknownResource:
            raise cherrypy.HTTPError(404, "Could not find '%s'" % member.member_id)
        except UnsupportedMediaType:
            raise cherrypy.HTTPError(415, 'Unsupported Media Type')
        except FixedCategoriesError, e:
            raise cherrypy.HTTPError(400, 'The member categories do not meet the collection requirement')
        except ResourceOperationException, roe:
            raise cherrypy.HTTPError(roe.code, roe.msg)
            
        cherrypy.response.headers['Content-Type'] = self.collection.member_media_type
        return member.atom.xml()
