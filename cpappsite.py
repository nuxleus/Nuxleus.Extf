# -*- coding: utf-8 -*-

import cherrypy
import os.path
from amplee.handler.store.cp import Service, Store

# Local imports
from core.atompub import setup_store

cur_dir = os.path.dirname(os.path.abspath(__file__))

__all__ = ['create_store']

class Root:
    pass

class WebService:
    pass

class Collection:
    pass

def create_store():
    cherrypy.log("Creating APP store")
    service, conf = setup_store()
    workspace = service.workspaces[0]
    collections = service.get_collections()

    app = Root()
    app.service = WebService()
    app.collection = Collection()
    
    cherrypy.log("Creating the service WSGI application")
    app.service.pub = Service(service)
    
    cherrypy.log("Creating the collection WSGI application")
    app.collection.frontpage = Store(workspace.get_collection('frontpage'), strict=True)
    app.collection.music = Store(workspace.get_collection('music'), strict=True)
    app.collection.blog = Store(workspace.get_collection('blog'), strict=True)
    app.collection.calendar = Store(workspace.get_collection('calendar'), strict=True)
    app.collection.photos = Store(workspace.get_collection('photos'), strict=True)
    app.collection.bookmarks = Store(workspace.get_collection('bookmarks'), strict=True)
    app.collection.subscriptions = Store(workspace.get_collection('subscriptions'), strict=True)

    # For XMPP 
    #from bucker.provider.xmpp import Messenger
    #m = Messenger('localhost', 5222)
    #cherrypy.engine.on_stop_engine_list.append(m.shutdown)
    #cherrypy.log("Setting up SQS queue")
    #m.bind(u'localhost', u'test', u'test', u'webapp')
    #app.collection.blog.collection.m = m

    # For SQS
    from bucker.provider.sqs import Messenger
    m = Messenger(conf.general.s3_access_key, conf.general.s3_private_key)
    cherrypy.engine.on_stop_engine_list.append(m.shutdown)
    cherrypy.log("Setting up SQS queue")
    m.bind(conf.general.sqs_queue_name)
    app.collection.blog.collection.m = m
    
    cherrypy.log("All good!")

    return app
