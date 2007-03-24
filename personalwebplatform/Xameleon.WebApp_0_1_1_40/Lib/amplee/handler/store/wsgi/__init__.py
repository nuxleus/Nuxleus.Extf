# -*- coding: utf-8 -*-

__doc__ = """
WSGI interface to amplee.handler and to be used in your
WSGI application (http://wsgi.org).
"""

import sys
import urllib

from amplee.atompub.member import EntryMember
from amplee.handler import MemberHandler, MemberType
from amplee.error import *
from amplee.http_helper import get_best_mimetype
from amplee.utils import decode_slug
from bridge.filter.atom import lookup_links

class Service(object):
    def __init__(self, service):
        self.service = service

    def get_service(self, environ, start_response):
        body = self.service.service.xml()
        headers = [('Content-Type', 'application/atomserv+xml'),
                   ('Content-Type', str(len(body)))]
        start_response("200 OK", headers)
        return [body]

class Store(object):
    def __init__(self, collection, strict=True):
        self.collection = collection
        self.strict = strict

    def create_member(self, environ, start_response):
        ct = environ.get('CONTENT_TYPE', None)

        if not ct:
            start_response("400 Bad Request", [('Content-Type','text/plain')])
            return ["Missing content type"] 

        length = environ.get('CONTENT_LENGTH', None)
        if not length:
            start_response("411 Length Required", [('Content-Type','text/plain')])
            return ["Length Required"]

        check_params = False
        if self.strict:
            # In this case we also validate all the parameters
            check_params = True
        best = get_best_mimetype(ct, self.collection.accept_media_types,
                                 check_params=check_params, return_full=self.strict)
        if not best:
            start_response("415 Unsupported Media Type", [('Content-Type','text/plain')])
            return ["Unsupported Media Type"]

        handler, mt = self.collection.get_handler(best)

        try:
            fileobj = environ['wsgi.input']
            length = int(length)
            mt.params['media_type'] = unicode(best)
            mt.params['slug'] = decode_slug(environ.get('HTTP_SLUG', None))
            member = MemberHandler.create_from_stream(self.collection, mt, fileobj, length, best)
        except UnsupportedMediaType:
            start_response("415 Unsupported Media Type", [('Content-Type','text/plain')], sys.exc_info())
            return ["Unsupported Content Type"] 
        except FixedCategoriesError:
            start_response("400 The member categories do not meet the collection requirement",
                           [('Content-Type','text/plain')], sys.exc_info())
            return ["The member categories do not meet the collection requirement"] 
        except ResourceOperationException, roe:
            start_response("%s %s" % (roe.code, roe.msg), [('Content-Type','text/plain')], sys.exc_info())
            return [roe.msg]
        except:
            start_response('500 Internal Server Error', [('Content-Type','text/plain')], sys.exc_info())
            return ['500 Internal Server Error']

        if best == self.collection.member_media_type:
            message = 'Adding %s' % member.member_id
        else:
            message='Adding %s and %s' % (member.member_id, member.media_id)

        self.collection.store.commit(message=message)

        entry = member.atom
        headers = [('Content-Type', self.collection.member_media_type.encode('utf-8'))]
        edit_link = entry.filtrate(lookup_links, rel=u'edit')
        if edit_link:
            edit_link = edit_link[0]
            location = edit_link.get_attribute('href')
            if location:
                location = str(location)
                location = '%s%s' % (self.collection.xml_base or '', location)
                location = location.encode('utf-8')
                headers.append(('Location', location))
                headers.append(('Content-Location', location))

        body = entry.xml()
        headers.append(('Content-Length', str(len(body))))
        start_response("201 Created", headers)
        
        return [body]

    def head_collection(self, environ, start_response):
        content = self.collection.feed.xml()
        start_response("200 OK", [('Content-Type', 'application/atom+xml;type=feed'),
                                  ('Content-Length', str(len(content)))])
        return []
    
    def get_collection(self, environ, start_response):
        headers = [('Content-Type', 'application/atom+xml;type=feed')]
        body = self.collection.feed.xml()
        headers.append(('Content-Length', str(len(body))))
        start_response("200 OK", headers)
        return [body]
            
    def head_member(self, environ, start_response):
        rid = environ['wsgiorg.routing_args'][1]['rid']

        member_id, media_id = self.collection.convert_id(rid)
        member = self.collection.get_member(member_id)
        if not member:
            start_response("404 Not Found", [('Content-Type','text/plain')])
            return ["Could not find '%s'" % rid]

        accept = None
        if self.strict:
            accept = environ.get('HTTP_ACCEPT', None)
            accept = get_best_mimetype(accept, self.collection.accept_media_types)
            if not accept:
                start_response("406 Not Acceptable", [('Content-Type','text/plain')])
                return []
        
        try:
            if rid == member_id:
                content, content_type = MemberHandler.get_atom(self.collection, member)
            else:
                content, content_type = MemberHandler.get_content(self.collection, member)
        except UnknownResource:
            start_response("404 Not Found", [('Content-Type','text/plain')], sys.exc_info())
            return ["Could not find '%s'" % rid]
        except ResourceOperationException, roe:
            start_response("%s %s" % (roe.code, roe.msg), [('Content-Type','text/plain')], sys.exc_info())
            return [roe.msg] 
        
        start_response("200 OK", [('Content-Type', content_type.encode('utf-8')),
                                  ('Content-Length', str(len(content)))])
        
        return []

    def get_member(self, environ, start_response):
        rid = environ['wsgiorg.routing_args'][1]['rid']

        member_id, media_id = self.collection.convert_id(rid)
        member = self.collection.get_member(member_id)
        if not member:
            start_response("404 Not Found", [('Content-Type','text/plain')])
            return ["Could not find '%s'" % rid]

        accept = None
        if self.strict:
            check_params = False
            if rid == member_id:
                # When dealing with the Atom media-type we additionally check
                # for the parameter type
                check_params = ['type']
            accept = environ.get('HTTP_ACCEPT', None)
            accept = get_best_mimetype(accept, self.collection.accept_media_types,
                                       check_params=check_params)
            if not accept:
                start_response("406 Not Acceptable", [('Content-Type','text/plain')])
                return []

        try:
            if rid == member_id:
                content, content_type = MemberHandler.get_atom(self.collection, member)
            else:
                content, content_type = MemberHandler.get_content(self.collection, member)
        except UnknownResource:
            start_response("404 Not Found", [('Content-Type','text/plain')], sys.exc_info())
            return ["Could not find '%s'" % rid]
        except ResourceOperationException, roe:
            start_response("%s %s" % (roe.code, roe.msg), [('Content-Type','text/plain')], sys.exc_info())
            return [roe.msg] 

        headers = [('Content-Type', content_type.encode('utf-8')),
                   ('Content-Length', str(len(content)))]
        start_response("200 OK", headers)
        return [content]

    def delete_member(self, environ, start_response):
        rid = environ['wsgiorg.routing_args'][1]['rid']
        member_id, media_id = self.collection.convert_id(rid)
        
        member = self.collection.get_member(member_id)
        if member:
            try:
                MemberHandler.delete(self.collection, member)
            except ResourceOperationException, roe:
                start_response("%s %s" % (roe.code, roe.msg), [('Content-Type','text/plain')], sys.exc_info())
                return [roe.msg] 
            self.collection.store.commit(message="Deleting %s and %s" % (member_id, media_id))

        headers = [('Content-Type','text/plain'), ('Content-Length', '0')]
        start_response("200 OK", headers)
        return []

    def update_member(self, environ, start_response):
        if 'rid' not in environ['wsgiorg.routing_args'][1]:
            start_response("400 Bad Request", [('Content-Type','text/plain')])
            return ["Missing identifier"]
        
        ct = environ.get('CONTENT_TYPE', None)

        if not ct:
            start_response("400 Bad Request", [('Content-Type','text/plain')])
            return ["Missing content type"] 

        check_params = False
        if self.strict:
            # In this case we also validate all the parameters
            check_params = True
            
        best = get_best_mimetype(ct, self.collection.accept_media_types,
                                   check_params=check_params, return_full=self.strict)
        if not best:
            start_response("415 Unsupported Media Type", [('Content-Type','text/plain')])
            return ["Unsupported Media Type"]

        rid = environ['wsgiorg.routing_args'][1]['rid']

        member_id, media_id = self.collection.convert_id(rid)
        member = self.collection.get_member(member_id)
        if not member:
            start_response("404 Not Found", [('Content-Type','text/plain')])
            return ["Could not find '%s'" % rid]
        
        handler, mt = self.collection.get_handler(best)

        length = environ.get('CONTENT_LENGTH', None)
        if not length:
            start_response("411 Length Required", [('Content-Type','text/plain')])
            return ["Length Required"]
        
        length = int(length)
        fileobj = environ['wsgi.input']

        try:
            mt.params['media_type'] = unicode(best)
            member = MemberHandler.update_content_from_stream(self.collection, mt, member, fileobj, length)
            if member.media_id:
                self.collection.store.commit(message='Updating %s and %s' % (member.member_id,
                                                                             member.media_id))
            else:
                self.collection.store.commit(message='Updating %s' % member.member_id)
        except UnsupportedMediaType:
            start_response("415 Unsupported Media Type", [('Content-Type','text/plain')], sys.exc_info())
            return ["Unsupported Content Type"] 
        except FixedCategoriesError:
            start_response("400 The member categories do not meet the collection requirement",
                           [('Content-Type','text/plain')], sys.exc_info())
            return ["The member categories do not meet the collection requirement"] 
        except ResourceOperationException, roe:
            start_response("%s %s" % (roe.code, roe.msg), [('Content-Type','text/plain')], sys.exc_info())
            return [roe.msg]
        except:
            start_response('500 Internal Server Error', [('Content-Type','text/plain')], sys.exc_info())
            return ['500 Internal Server Error'] 

        body = member.atom.xml()
        ct = self.collection.member_media_type.encode('utf-8')
        headers = [('Content-Type', ct), ('Content-Length', str(len(body)))]
        start_response("200 OK", headers)
        return [body]

       
