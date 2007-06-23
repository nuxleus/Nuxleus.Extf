# -*- coding: utf-8 -*-

import os.path
from amplee.loader import loader, Config

#########################################################################
# A couple of global but handy variables
#########################################################################
base_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..')

def setup_store():
    """Loads into memory our APP store from the configuration file"""
    service, conf = loader(os.path.join(base_dir, 'appstore.conf'),
                           encoding='ISO-8859-1', base_path=base_dir)

    return service, conf

def validate_collection_feed(collection, feed):
    """Allows some tuning of the collection feed before it is returned"""
    return feed
