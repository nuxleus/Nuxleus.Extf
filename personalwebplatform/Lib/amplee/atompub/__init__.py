# -*- coding: utf-8 -*-

__doc__ = """

APP is defined as:

'''
The Atom Publishing Protocol is an application-level protocol for
publishing and editing Web resources using HTTP [RFC2616] and XML 1.0.
The protocol supports the creation of arbitrary web resources and
provides facilities for:

 * Collections: Sets of resources, which can be retrieved in whole or in part.
 * Service: Discovering and describing Collections.
 * Editing: Creating, updating and deleting resources.
'''

The amplee.atompub package contains an implementation
of the different entities of the Atom Publishing Protocol.

The way to use this package is to:

- create a storage for member resources
[optionally also create a storage for media resources]
- create a store and pass the created storage(s)
- create a service instance and pass the created store
- create one or more workspace using the service instance
- create one or more collections per workspace

Then create members, attach them to their collections
and finally commit the modification into the storage
via the store instance.
"""

__all__ = ['member', 'collection', 'store',
           'service', 'workspace']
