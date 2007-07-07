# -*- coding: utf-8 -*-

import os.path
import cherrypy
from amplee.loader import loader, Config
from amplee.handler.store.cp import Service, Store

__all__ = ['create_application']

class Root:
    pass

class WebService:
    pass

class Collection:
    pass

def create_application(options, base_dir):
    """Create and setup the application from the amplee configuration file"""
    cherrypy.log("Creating APP store")
    service, conf = loader(os.path.join(base_dir, options.configuration),
                           encoding='ISO-8859-1', base_path=base_dir)

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
    
    # For SQS
    from bucker.provider.sqs import Messenger
    from lib.aws import lookup_keys
    s3_access_key, s3_private_key = lookup_keys(options.aws)
    m = Messenger(s3_access_key, s3_private_key)
    cherrypy.engine.on_stop_engine_list.append(m.shutdown)
    cherrypy.log("Setting up SQS queue")
    m.bind(conf.general.sqs_queue_name)
    app.collection.frontpage.collection.m = m
    app.collection.music.collection.m = m
    app.collection.blog.collection.m = m
    app.collection.calendar.collection.m = m
    app.collection.photos.collection.m = m
    app.collection.subscriptions.collection.m = m
    app.collection.bookmarks.collection.m = m

    cherrypy.log("All good!")

    method_dispatcher = cherrypy.dispatch.MethodDispatcher()
    conf = {'/pub': {'request.dispatch': method_dispatcher,
                     'tools.etags.on': True,
                     'tools.etags.autotags': False},
            '/collection': {'request.dispatch': method_dispatcher,
                            'tools.etags.on': True,
                            'tools.etags.autotags': False}}
    
    cherrypy.tree.mount(app, '/', config=conf)

