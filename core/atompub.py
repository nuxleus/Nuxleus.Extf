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

#########################################################################
# Utils
#########################################################################
def atom_id_generator(collection, seed, slug, *args, **kwargs):
    """
    Through this function you can provide the precise atom:id value
    you want for your member.
    
    For the purpose of this example this function will be fairly dumb
    and will simply construct the absolute URI to the media member
    resource.
    """
    title = seed.get_child('title', seed.xml_ns)
    if title:
        title = title.xml_text
    else:
        title = slug
    id = unicode('%s/%s%s' % (collection.xml_base or '',
                              collection.base_uri, title.replace(' ', '-')))
    # When xml_base is not set the previous call leaves a leading '/'
    # we just get rid of it here
    return id.lstrip('/')


def resource_storage_name_generator(base_uri, slug, title):
    """
    This method returns the value that will be used to identify
    the resource within your storage.

    So if your Slug header looks like 'my great blog post' this
    will return 'my-great-blog-post' which will be used to
    store the resource.

    If a slug is not provided we try with the title of the
    member resource if any.
    """
    if slug:
        return unicode('%s' % slug.replace(' ', '-'))
    if title:
        return unicode('%s' % title.replace(' ', '-'))
    raise ValueError, 'Missing slug or title to generate the resource name'
