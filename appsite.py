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
    dispatcher.add('/collection/music[/]', POST=music_store.create_member,
          GET=music_store.get_collection, HEAD=music_store.head_collection)
    dispatcher.add('/collection/music/{rid:any}[/]', GET=music_store.get_member,
          PUT=music_store.update_member,
          DELETE=music_store.delete_member, HEAD=music_store.head_member)
          
    blog_store = Store(workspace.get_collection('blog'), strict=True)
    dispatcher.add('/collection/blog[/]', POST=blog_store.create_member,
          GET=blog_store.get_collection, HEAD=blog_store.head_collection)
    dispatcher.add('/collection/blog/{rid:any}[/]', GET=blog_store.get_member,
          PUT=blog_store.update_member,
          DELETE=blog_store.delete_member, HEAD=blog_store.head_member)
          
    photos_store = Store(workspace.get_collection('photos'), strict=True)
    dispatcher.add('/collection/photos[/]', POST=photos_store.create_member,
          GET=photos_store.get_collection, HEAD=photos_store.head_collection)
    dispatcher.add('/collection/photos/{rid:any}[/]', GET=photos_store.get_member,
          PUT=photos_store.update_member,
          DELETE=photos_store.delete_member, HEAD=photos_store.head_member)
          
    bookmarks_store = Store(workspace.get_collection('bookmarks'), strict=True)
    dispatcher.add('/collection/bookmarks[/]', POST=bookmarks_store.create_member,
          GET=bookmarks_store.get_collection, HEAD=bookmarks_store.head_collection)
    dispatcher.add('/collection/bookmarks/{rid:any}[/]', GET=bookmarks_store.get_member,
          PUT=bookmarks_store.update_member,
          DELETE=bookmarks_store.delete_member, HEAD=music_store.head_member)
          
    subscriptions_store = Store(workspace.get_collection('subscriptions'), strict=True)
    dispatcher.add('/collection/subscriptions[/]', POST=subscriptions_store.create_member,
          GET=subscriptions_store.get_collection, HEAD=subscriptions_store.head_collection)
    dispatcher.add('/collection/subscriptions/{rid:any}[/]', GET=subscriptions_store.get_member,
          PUT=subscriptions_store.update_member,
          DELETE=subscriptions_store.delete_member, HEAD=subscriptions_store.head_member)
          
    print "All good!"

def create_repository(dispatcher):
    print "Setting up the repository WSGI application"
    Cling.index_file = "index.xml"
    base = Cling(os.path.join(cur_dir, '../public_web'))
    s.add('/[{:segment}[/{:segment}]]', GET=base)

def dummy_xameleon_handler(dispatcher):
    from core.xameleonhandler import xamdler
    s.add('[/]', GET=xamdler)

create_repository(s)
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
