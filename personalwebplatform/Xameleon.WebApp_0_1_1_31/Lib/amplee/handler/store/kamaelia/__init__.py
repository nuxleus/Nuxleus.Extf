# -*- coding: utf-8 -*-

__doc__ = """
Kamaelia interface handler
http://kamaelia.sourceforge.net
"""

__all__ = ['Service', 'Store', 'request_handlers']

# EXPERIMENTAL

import socket
import urllib

try:
    from cStringIO import StringIO
except ImportError:
    from StringIO import StringIO
    
from amplee.error import *
from amplee.atompub.member import EntryMember
from amplee.handler import MemberHandler, MemberType
from amplee.http_helper import get_best_mimetype
from amplee.utils import decode_slug
from bridge.filter.atom import lookup_links

import Axon
from Axon.Ipc import producerFinished
from Kamaelia.Chassis.ConnectedServer import SimpleServer
from Kamaelia.Protocol.HTTP.HTTPServer import HTTPServer
from Kamaelia.Protocol.HTTP.Handlers.Minimal import Minimal
import Kamaelia.Protocol.HTTP.ErrorPages as ErrorPages

def request_handlers(URLHandlers):
    def createRequestHandler(request):
        if request.get("bad"):
            return ErrorPages.websiteErrorPage(400, request.get("errormsg",""))
        else:
            for ((prefix, amplee_handler), handler) in URLHandlers:
                if request["raw-uri"][:len(prefix)] == prefix:
                    request["uri-prefix-trigger"] = prefix
                    request["uri-suffix"] = request["raw-uri"][len(prefix):]
                    return handler(request, amplee_handler)

        return ErrorPages.websiteErrorPage(404, "No resource handlers could be found for the requested URL")

    return createRequestHandler

class Service(Axon.Component.component):
    def __init__(self, request, service, *args, **kwargs):
        super(Service, self).__init__()
        self.request = request
        self.service = service

    def main(self):
        method = self.request.get('method')
        if method not in ['GET', 'HEAD']:
            resource = {'statuscode': '405', 'data': 'Method Not Allowed'}
        else:
            data = self.service.service.xml()
            resource = {
                "type"           : 'application/atomserv+xml',
                "statuscode"     : '200',
                "length"         : len(data),
                }
            if method == 'GET':
                resource['data'] = data

        self.send(resource, "outbox")
        yield 1
        self.send(Axon.Ipc.producerFinished(self), "signal")
        yield 1 
        
class Store(Axon.Component.component):
    def __init__(self, request, collection, strict=True):
        super(Store, self).__init__()
        self.request = request
        self.collection = collection
        self.strict = strict

    def handle_get_feed(self):
        data = self.collection.feed.xml()
        resource = {
           "type"           : 'application/atom+xml;type=feed',
           "statuscode"     : '200',
           "length"         : len(data),
           "data"           : data,
        }
        return resource

    def handle_head_member(self):
        self.handle_get_member(send_content=False)

    def handle_get_member(self, send_content=True):
        rid = urllib.unquote(self.request.get('uri-suffix')[1:])
        member_id, media_id = self.collection.convert_id(rid)
        
        member = self.collection.get_member(member_id)
        if not member:
            return {'statuscode': '404', 'data': 'Could not find %s' % rid}

        accept = None
        if self.strict:
            check_params = False
            if rid == member_id:
                # When dealing with the Atom media-type we additionally check
                # for the parameter type
                check_params = True
            accept_header = self.request['headers'].get('accept', None)
            accept = get_best_mimetype(accept_header, self.collection.accept_media_types,
                                       check_params=check_params)
            if not accept:
                return {'statuscode': '406', 'data': 'Not Acceptable'}

        try:
            if rid == member_id:
                content, content_type = MemberHandler.get_atom(self.collection, member)
            else:
                content, content_type = MemberHandler.get_content(self.collection, member)
        except UnknownResource:
            return {'statuscode': '404', 'data': 'Could not find %s' % rid}
        except ResourceOperationException, roe:
            raise {'statuscode': str(roe.code), 'data': roe.msg}

        resource = {
           "type"           : content_type,
           "statuscode"     : "200",
           "length"         : len(content),
        }
        # GET needs a body
        if send_content:
            resource['data'] = content
            
        return resource

    def verify_post(self):
        if self.collection.is_read_only:
            return {'statuscode': '400', 'data': 'Creation of resources not accepted to this collection'}
        
        ct = self.request['headers'].get('content-type')
        if not ct:
            return {'statuscode': '400', 'data': 'Missing content type'}
            
        length = self.request['headers'].get('content-length')
        if not length:
            return {'statuscode': '411', 'data': 'Length Required'}
            
        if self.strict:
            # In this case we also validate all the parameters
            check_params = True
        accept = get_best_mimetype(ct, self.collection.accept_media_types,
                                   check_params=check_params, return_full=self.strict)
        if not accept:
            return {'statuscode': '415', 'data': 'Unsupported Media Type'}

        handler, mt = self.collection.get_handler(accept)
        
        return (length, accept)
        
    def handle_post(self, accept, fileobj, length):
        handler, mt = self.collection.get_handler(accept)

        try:
            length = int(length)
            mt.params['media_type'] = unicode(accept)
            mt.params['slug'] = decode_slug(self.request['headers'].get('slug', None))
            member = MemberHandler.create_from_stream(self.collection, mt, fileobj, length, accept)
        except UnsupportedMediaType:
            return {'statuscode': '415', 'data': 'Unsupported Media Type'}
        except FixedCategoriesError:
            return {'statuscode': '400', 'data': 'The member categories do not meet the collection requirement'}
        except ResourceOperationException, roe:
            return {'statuscode': str(roe.code), 'data': roe.msg}
        
        if accept == self.collection.member_media_type:
            message = 'Adding %s' % member.member_id
        else:
            message='Adding %s and %s' % (member.member_id, member.media_id)
            
        self.collection.store.commit(message=message)

        entry = member.atom
        edit_link = entry.filtrate(lookup_links, rel=u'edit')
        if edit_link:
            edit_link = edit_link[0]
            location = edit_link.get_attribute('href')
            if location:
                location = '%s%s' % (self.collection.xml_base or '', str(location))
              
        #"location"         : location,
        #"content-location" : location,
        data = entry.xml()
        return {
           "type"             : self.collection.member_media_type,
           "statuscode"       : '201',
           "data"             : data,
           "length"           : len(data),
        }

    def handle_put(self):
        if not self.request.get('uri-suffix'):
            return {'statuscode': '400', 'data': "Missing identifier"}

        ct = self.request['headers'].get('content-type')
        if not ct:
            return {'statuscode': '400', 'data': 'Missing content type'}

        check_params = False
        if self.strict:
            check_params = True
        accept = get_best_mimetype(ct, self.collection.accept_media_types,
                                   check_params=check_params, return_full=self.strict)
        if not accept:
            return {'statuscode': '415', 'data': 'Unsupported Media Type'}

        rid = urllib.unquote(self.request.get('uri-suffix')[1:])
        member_id, media_id = self.collection.convert_id(rid)
        member = self.collection.get_member(member_id)
        if not member:
            return {'statuscode': '404', 'data': 'Could not find %s' % rid}
  
        handler, mt = self.collection.get_handler(accept)
        
        length = self.request['headers'].get('content-length')
        if not length:
            return {'statuscode': '411', 'data': 'Length Required'}

        try:
            length = int(length)
            fileobj = StringIO(self.request.get('body', ''))
            mt.params['media_type'] = unicode(accept)
            member = MemberHandler.update_content_from_stream(self.collection, mt, rid, fileobj, length)
            message = 'Updating %s' % member.member_id
            if member.media_id:
                message = '%s and %s' % (message, member.media_id)
            self.collection.store.commit(message=message)
        except UnknownResource:
            return {'statuscode': '404', 'data': 'Could not find %s' % rid}
        except UnsupportedMediaType:
            return {'statuscode': '415', 'data': 'Unsupported Media Type'}
        except FixedCategoriesError:
            return {'statuscode': '400', 'data': 'The member categories do not meet the collection requirement'}
        except ResourceOperationException, roe:
            return {'statuscode': str(roe.code), 'data': roe.msg}

        data = member.atom.xml()
        resource = {
           "type"           : self.collection.member_media_type,
           "statuscode"     : '200',
           "data"           : data,
           "length"         : len(data),
        }

        return resource

    def handle_delete(self):
        if self.request.get('uri-suffix'):
            rid = urllib.unquote(self.request.get('uri-suffix')[1:])
            member_id, media_id = self.collection.convert_id(rid)
            member = self.collection.get_member(member_id)
            if member:
                try:
                    MemberHandler.delete(self.collection, member)
                except ResourceOperationException, roe:
                    return {'statuscode': str(roe.code), 'data': roe.msg}

                self.collection.store.commit(message="Deleting %s and %s" % (member_id, media_id))

        return {
           "type"           : "text/plain",
           "statuscode"     : '200',
           "length"         : 0,
        }
            
    def main(self):
        method = self.request.get('method')

        if method == 'GET':
            if not self.request.get('uri-suffix') :
                resource = self.handle_get_feed()
            else:
                resource = self.handle_get_member()
        elif method == 'POST':
            resource = self.verify_post()

            if isinstance(resource, tuple):
                while 1:
                    while self.dataReady("inbox"):
                        data = self.recv("inbox")
                        print data

                    if not self.anyReady():
                        self.pause()
                        yield 1
                        
                fileobj = StringIO('')
                while self.dataReady("inbox"):
                    data = self.recv("inbox")
                    print data
                    fileobj.write(data)
##                 receivingpost = True
##                 while receivingpost:
##                     while self.dataReady("inbox"):
##                         data = self.recv("inbox")
##                         print data
##                         fileobj.write(data)
                    
##                     while self.dataReady("control"):
##                         msg = self.recv("control")
##                         if isinstance(msg, producerFinished):
##                             receivingpost = False
                  
##                     if receivingpost:
##                         print "h"
##                         yield 1
##                         self.pause()
                
                fileobj.seek(0)
                length, accept = resource
                resource = self.handle_post(accept, fileobj, length)
        elif method == 'PUT':
            resource = self.handle_put()
        elif method == 'DELETE':
            resource = self.handle_delete()
        elif method == 'HEAD':
            if not self.request.get('uri-suffix'):
                resource = self.handle_head_member()
        else:
            resource = {'statuscode': '405', 'data': 'Method Not Allowed'}

        if "type" not in resource:
            resource['type'] = 'text/plain'
            
        self.send(resource, "outbox")
        yield 1
        self.send(Axon.Ipc.producerFinished(self), "signal")
        yield 1

if __name__ == '__main__':
    def servePage(request):
        return Minimal(request=request,
                       homedirectory=homedirectory,
                       indexfilename=indexfilename)

    def HTTPProtocol():
        return HTTPServer(requestHandlers([
            ["/hello", HelloHandler ],
            ["/", servePage ],
            ]))

    SimpleServer(protocol=HTTPProtocol,
                 port=8082,
                 socketOptions=(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)  ).run()
