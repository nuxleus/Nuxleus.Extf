# -*- coding: utf-8 -*-

__doc__ = """Atom Publising Store loader

Synopsis
--------

The process of creating an Atom Publishing store is
a repetitive task that is common to all projects.

The loader module offers the possibility to automate
this task by putting required information in a
configuration file and let the `loader`function
generates the store from the configuration settings.

Consider the following:

>>> from amplee.loader import loader
>>> service, config = loader('/my/config.cfg')

The ``service`` object returned is an instance of
``amplee.atompub.service.AtomPubService`` and is your
reference to all the related objects.

The ``config`` object is an instance of
``amplee.loader.Config`` and is the representation
of your configuration file.

"""

import os.path
import imp
import re
import ConfigParser

from amplee.atompub.store import *
from amplee.atompub.service import *
from amplee.atompub.workspace import *
from amplee.atompub.collection import *
from amplee.handler import MemberType

from bridge import Element, set_bridge_parser
from bridge.common import ATOM10_PREFIX, ATOM10_NS

__all__ = ['loader', 'Config']

class Config(object):
    """Represents an INI file as a tree of Config instances

    Say you have ``test.conf``:
    
    [general]
    verbose = 0

    >>> from amplee.loader import Config
    >>> config = Config()
    >>> config.from_ini('test.conf')
    >>> print config.general.verbose
    >>> 0

    Each loaded value is an unicode object.
    """
    def from_ini(self, filepath, encoding='ISO-8859-1'):
        """Loads the configuration file into the current instance
        of `Config`.

        The ``filepath`` is the path to the configuration file.

        The ``encoding`` indicates how to decode each value.
        """
        config = ConfigParser.ConfigParser()
        config.readfp(file(filepath, 'rb'))

        for section in config.sections():
            section_prop = Config()
            section_prop.keys = []
            setattr(self, section, section_prop)
            for option in config.options(section):
                section_prop.keys.append(option)
                value = config.get(section, option).decode(encoding)
                setattr(section_prop, option, value)

    def get(self, section, option, default=None, raise_error=False):
        """Retrieve the value for a particular node in the configuration tree.

        >>> print config.get('general', 'verbose', default=1)

        but also:
        
        >>> try:
        >>>    config.get('general', 'verbose', raise_error=True)
        >>> except AttributeError:
        >>>    print 'Could not find it'

        The ``section`` is the name of the parent node of which ``option``
        is a child.

        The ``option`` is the name of the node to lookup.

        You can provide a ``default`` value when the node was not found.
        This applies when ``raise_error`` is ``False``, which is the default.

        If you set ``raise_error`` to ``True`` an ``AttributeError``
        exception will be raised if the node is not found.
        
        """
        if hasattr(self, section):
            obj = getattr(self, section, None)
            if obj and hasattr(obj, option):
                return getattr(obj, option, default)

        if raise_error:
            raise AttributeError, "%s %s" % (section, option)

        return default

    def get_section(self, section):
        return getattr(self, section, None)

    def get_all_values(self, section):
        """Returns a dictionnary of all nodes which parent is the section.

        The ``section`` is the name of the parent node to look for.
        """
        values = {}
        if hasattr(self, section):
            obj = getattr(self, section, None)
            for key in obj.keys:
                if hasattr(obj, key):
                    values[key] = getattr(obj, key, None)

        return values
    
def init_storage(config, storage, base_path=None):
    """Initializes a storage as described in the configuration object.

    The ``storage`` string value is looked up within the ``config``
    object and then based upon which a storage object is constructed.

    The optional ``base_path`` is only required when your configuration
    settings use relative path in their values. 
    """
    if storage.startswith('fs_storage'):
        from amplee.storage import dummyfs
        root_dir = config.get(storage, 'base_path')
        if base_path:
            root_dir = os.path.join(base_path, root_dir)
        enable_lock = bool(int(config.get(storage, 'enable_lock', 0)))
        return dummyfs.DummyStorageFS(root_dir, enable_lock)
    elif storage.startswith('svn_storage'):
        from amplee.storage import storesvn
        repository_uri = config.get(storage, 'repository_uri')
        working_copy_path = config.get(storage, 'working_copy_path')
        if base_path:
            working_copy_path = os.path.join(base_path, working_copy_path)
        username = config.get(storage, 'username', '')
        password = config.get(storage, 'password', '')
        if username == '': username = None
        if password == '': password = None
        return storesvn.SubversionStorage(repository_uri, working_copy_path, username, password)
    elif storage.startswith('zodb_storage'):
        from ZODB import DB
        from amplee.storage import storezodb
        fs_type = config.get(storage, 'fs_type', 'filestorage')
        if fs_type == 'filestorage':
            from ZODB import FileStorage
            storage_path = config.get(storage, 'fs_path')
            if base_path:
                storage_path = os.path.join(base_path, storage_path)
            db = DB(FileStorage.FileStorage(storage_path))
        elif fs_type == 'clientstorage':
            from ZEO import ClientStorage
            db = DB(ClientStorage.ClientStorage(config.get(storage, 'address')))
 	return storezodb.ZODBStorage(db, config.get(storage, 'top_level_node_name'))
    elif storage.startswith('dejavu_storage'):
        from amplee.storage import storedejavu
        conf = {'Connect': config.get(storage, 'connect_string')}
        return storedejavu.DejavuStorage(config.get(storage, 'db_type'), conf)
    elif storage.startswith('s3_storage'):
        from amplee.storage import stores3
        aws_access_key_id = config.get(storage, 'access_key')
        aws_secret_access_key = config.get(storage, 'private_key')
        unique_prefix = config.get(storage, 'bucket_unique_prefix')
        separator = config.get(storage, 'separator', '_')
        return stores3.S3Storage(aws_access_key_id, aws_secret_access_key,
                                 unique_prefix, separator)
    elif storage.startswith('nonbuiltin_storage'):
        storage_module = config.get(storage, 'storage_module')
        storage_class = config.get(storage, 'storage_class')
        directory, name = os.path.split(storage_module)
        if base_path:
            directory = os.path.join(base_path, directory)
            
        file, filename, description = imp.find_module(name, [directory])
        mod = imp.load_module(name, file, filename, description)
        mod_class = getattr(mod_class, storage_class, None)
        if not mod_class:
            raise RuntimeError, "could not load non built-in storage %s" % storage

        return mod_class(config=getattr(config, storage, None))
    
def init_workspaces(config, service, base_path=None):
    """Initializes the Atom Publishing Protocol workspaces from the
    provided service.
    """
    workspaces = config.get('store', 'workspaces', '').split(',')
    for workspace_name in workspaces:
        name = config.get(workspace_name, 'name')
        title = config.get(workspace_name, 'title')
        workspace = AtomPubWorkspace(service, name, title=title)
        init_collections(config, workspace_name, workspace, base_path)

def init_collections(config, workspace_name, workspace, base_path=None):
    """Initializes the Atom Publishing Protocol collections from the
    provided workspace.
    """
    collections = config.get(workspace_name, 'collections', '').split(',')
    for collection_name in collections:
        base_uri = config.get(collection_name, 'base_uri', u'')
        id = config.get(collection_name, 'name', raise_error=True)
        title = config.get(collection_name, 'title', raise_error=True)
        base_edit_uri = config.get(collection_name, 'base_edit_uri')
        base_media_edit_uri = config.get(collection_name, 'base_media_edit_uri')
        accept_media_types = config.get(collection_name, 'accept_media_types', [])
        member_media_type = config.get(collection_name, 'member_media_type', u'application/atom+xml')
        member_extension = config.get(collection_name, 'member_media_extension', u'atom')
        if accept_media_types:
            accept_media_types = accept_media_types.split(',')
        favorite = bool(int(config.get(collection_name, 'favorite', 0)))
        read_only = bool(int(config.get(collection_name, 'read_only', 0)))
        fixed_categories = bool(int(config.get(collection_name, 'fixed_categories', 0)))
        xml_attrs = config.get(collection_name, 'xml_attrs', None)
        if xml_attrs:
            attrs = xml_attrs.split(';')
            xml_attrs = {}
            for attr in attrs:
                key, value = attr.split(',')
                xml_attrs[key] = value

        categories = config.get(collection_name, 'categories', [])
        if categories:
            categories = []
            for category in categories.split(','):
                category = Element(u'category', attributes={u'term': category},
                                   prefix=ATOM10_PREFIX, namespace=ATOM10_NS)
                categories.append(category)
                
        collection = AtomPubCollection(workspace, name_or_id=id, title=title,
                                       xml_attrs=xml_attrs, favorite=favorite,
                                       base_uri=base_uri, base_edit_uri=base_edit_uri,
                                       base_media_edit_uri=base_media_edit_uri,
                                       accept_media_types=accept_media_types,
                                       fixed_categories=fixed_categories,
                                       categories=categories,
                                       member_media_type=member_media_type,
                                       member_extension=member_extension)
        collection.is_read_only = read_only
        init_handlers(config, collection_name, collection, base_path)
        autoreload = bool(int(config.get(collection_name, 'reload_members', 0)))
        if autoreload:
            collection.reload_members()
        
_member_type_regex = re.compile('module:(.*),callable:(.*)')

def handle_member_type(config, handler_name, base_path=None):
    """Initializes the member type instances.
    """
    member_type = config.get(handler_name, 'member_type', None)
    mt = {}
    if member_type:
        values = config.get_all_values(member_type)
        for key in values:
            value = values[key]
            if not value.startswith('module:'):
                mt[key] = value
            else:
               match = _member_type_regex.match(value) 
               path, cb = match.groups()
               directory, name = os.path.split(path)
               if base_path:
                   directory = os.path.join(base_path, directory)
               file, filename, description = imp.find_module(name, [directory])
               mod = imp.load_module(name, file, filename, description)
               if hasattr(mod, cb):
                   mt[key] = getattr(mod, cb)
               else:
                   raise ImportError, "cannot import name %s from %s" % (cb, name)

    return mt

def init_handlers(config, collection_name, collection, base_path=None):
    """Initializes the handlers that will take handle one media-type
    for the collection they are registered with.
    """
    handlers = config.get(collection_name, 'handlers', '').split(',')
    for handler_name in handlers:
        # Load the member module
        member_module = config.get(handler_name, 'member_module', raise_error=True)
        member_class = config.get(handler_name, 'member_class', raise_error=True)
        if member_module.startswith('builtin:'):
            name = member_module[8:]
            mod_path = "amplee/atompub/member/%s" % member_module[8:]
            file, filename, description = imp.find_module(mod_path)
        else:
            directory, name = os.path.split(member_module)
            if base_path:
                directory = os.path.join(base_path, directory)
            file, filename, description = imp.find_module(name, [directory])

        mod = imp.load_module(name, file, filename, description)
        
        class_obj = getattr(mod, member_class, None)
        if not class_obj:
            raise ImportError, "cannot import %s from %s" % (member_class, member_module)
        
        params = handle_member_type(config, handler_name, base_path)
        media_type = config.get(handler_name, 'media_type', raise_error=True)
        member_type = MemberType(media_type, class_obj, params=params)
        
        # Load the handler module
        handler_module = config.get(handler_name, 'handler_module', raise_error=True)
        handler_class = config.get(handler_name, 'handler_class', raise_error=True)
        directory, name = os.path.split(handler_module)
        if base_path:
            directory = os.path.join(base_path, directory)
        file, filename, description = imp.find_module(name, [directory])
        mod = imp.load_module(name, file, filename, description)

        class_obj = getattr(mod, handler_class, None)
        if not class_obj:
            raise ImportError, "cannot import %s from %s" % (handler_class, handler_module)

        class_inst = class_obj(member_type)
        
        # Now let's register this handler to the collection
        collection.register_handler(class_inst)
            
def loader(conf_path, encoding='ISO-8859-1', base_path=None):
    """
    Creates the structure of an APP store following settings
    provided by the configuration file. It returns the created
    service instance as well as the configuration instance.

    The ``conf_path`` is the path to the configuration settings
    The ``encoding`` indicates how to encode each value of the settings
    The ``base_path``, if provided, will be prepended to all the
      values which require a path
    """
    config = Config()
    config.from_ini(conf_path, encoding=encoding)

    parser_name = config.get('general', 'bridge_parser', 'default')
    set_bridge_parser(parser_name)

    member_storage = media_storage = None
    # Initiating the storage
    storage = config.get('store', 'member_storage')
    if storage:
        member_storage = init_storage(config, storage, base_path)
    
    media_storage = config.get('store', 'media_storage')
    if media_storage:
        if storage != media_storage:
            media_storage = init_storage('media_storage')
        else:
            media_storage = member_storage

    # Initiate the store
    enable_lock = bool(int(config.get('store', 'enable_lock', 0)))
    store = AtomPubStore(member_storage, media_storage=media_storage,
                         enable_lock=enable_lock)

    # Initiate the service
    service = AtomPubService(store)
    
    # Initiate the workspaces and their collections
    init_workspaces(config, service, base_path)

    return service, config
