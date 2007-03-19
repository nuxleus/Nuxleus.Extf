# -*- coding: utf-8 -*-

__doc__ == """
Synopsis
--------

A workspace is a conceptual container holding a set of collections.
The organization brought by workspaces is mainly for clarity.
Consider workspaces being a drawer and collections folders inside.
"""

__all__ = ['AtomPubWorkspace']

from bridge import Element, Attribute
from bridge.common import ATOM10_PREFIX, ATOMPUB_PREFIX, \
     ATOM10_NS, ATOMPUB_NS

class AtomPubWorkspace(object):
    def __init__(self, service, name_or_id, title):
        """Atom Publishing Protocol workspace entity

        The ``service`` object is a ``amplee.atompub.service.AtomPubService``
        instance holding this workspace instance
        
        The ``name_or_id`` is the workspace internale identifier.
        
        The ``title`` is the human readable label of this workspace.
        """
        self.service = service
        self.name_or_id = name_or_id
        self.title = title
        self.service.workspaces.append(self)

        # list of collections held by this workspace
        self.collections = []

    def get_collection(self, name_or_id):
        """Returns a collection identified by ``name_or_id``
        """
        for collection in self.collections:
            if collection.name_or_id == name_or_id:
                return collection
            
    def to_workspace(self, prefix=ATOMPUB_PREFIX, namespace=ATOMPUB_NS):
        """Generates and returns a ``bridge.Element`` instance of the workspace
        document and its collections.

        The first collection shown will be the one having the ``favorite``
        attribute set to ``True``.

        The ``prefix`` is the XML prefix to use 
        The ``namespace`` is the namespace associated with the workspace element
        """
        workspace = Element(u'workspace', prefix=prefix, namespace=namespace)
        workspace.collection = []
        Element(u'title', content=self.title, attributes={u'type': u'text'},
                prefix=ATOM10_PREFIX, namespace=ATOM10_NS, parent=workspace)
                                  
        for collection in self.collections:
            ct = collection.collection
            ct.parent = workspace
            if collection.favorite:
                workspace.xml_children.insert(0, ct)
                workspace.collection.insert(0, ct)
            else:
                workspace.xml_children.append(ct)
                workspace.collection.append(ct)

        return workspace
    workspace = property(to_workspace)
        
