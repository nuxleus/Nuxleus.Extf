# -*- coding: utf-8 -*-

import os.path
import selector
from amplee.handler.store.wsgi import Service, Store

# Local imports
from core.atompub import setup_store

cur_dir = os.path.dirname(os.path.abspath(__file__))

# Our main WSGI application is the selector middleware
#�which will dispatch to the amplee WSGI applications
# based on the request URI
s = selector.Selector()

def create_store(dispatcher):
    print "Creating APP store"
    service, conf = setup_store()
    workspace = service.workspaces[0]
    collections = service.get_collections()
    
    print "Creating the service WSGI application"
    service = Service(service)
    # Uri to which the app:service document is reachable
    dispatcher.add('/service', GET=service.get_service)
    
    print "Creating the collection WSGI application"
    music_store = Store(workspace.get_collection('music'), strict=True)
    dispatcher.add('[/]', POST=music_store.create_member,
          GET=music_store.get_collection, HEAD=music_store.head_collection)
    dispatcher.add('/{rid:any}', GET=music_store.get_member,
          PUT=music_store.update_member,
          DELETE=music_store.delete_member, HEAD=music_store.head_member)
    print "All good!"

create_store(s)

app = s
