# -*- coding: utf-8 -*-

import os.path
import selector
from static import Cling
from amplee.handler.store.wsgi import Service, Store

# Local imports
from core.atompub import setup_store

cur_dir = os.path.dirname(os.path.abspath(__file__))

# Our main WSGI application is the selector middleware
# which will dispatch to the amplee WSGI applications
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
    dispatcher.add('/service/pub[/]', GET=service.get_service)
    
    print "Creating the collection WSGI application"
    music_store = Store(workspace.get_collection('music'), strict=True)
    dispatcher.add('/music[/]', POST=music_store.create_member,
          GET=music_store.get_collection, HEAD=music_store.head_collection)
    dispatcher.add('/music/{rid:any}[/]', GET=music_store.get_member,
          PUT=music_store.update_member,
          DELETE=music_store.delete_member, HEAD=music_store.head_member)
    print "All good!"

def create_static(dispatcher):
    print "Setting up the static servicing"
    static = Cling(os.path.join(cur_dir, 'public_web'))
    s.add('[/]{:segment}[/]', GET=static)

def dummy_xameleon_handler(dispatcher):
    from core.xameleonhandler import xamdler
    s.add('[/]', GET=xamdler)

create_static(s)
create_store(s)
# Just uncomment the following to enable the
# Xameleon handler out of amplee
# You may not need this
#dummy_xameleon_handler(s)

from httplogger import HTTPLogger
s = HTTPLogger(s, propagate_exc=False)
s.create_access_logger(access_path=os.path.join(cur_dir, 'access.log'))
s.create_error_logger(error_path=os.path.join(cur_dir, 'error.log'))
app = s
