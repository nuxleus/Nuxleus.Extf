# -*- coding: utf-8 -*-

__doc__ = """Represents an Atom Publisging Protocol service entity.
"""

from bridge import Element, Attribute
from bridge.common import ATOM10_PREFIX, ATOMPUB_PREFIX, ATOMPUB_NS

__all__ = ['AtomPubService']

class AtomPubService(object):
    def __init__(self, store):
        """Atom Publisging Protocol service entity.

        A ``store`` is a ``amplee.atompub.AtomPubStore`` instance
        """
        self.store = store

        # List of workspace instances belonging to
        # this service instance
        self.workspaces = []

    def get_workspace(self, name_or_id):
        """Returns a workspace per its identifier.

        The ``name_or_id`` matches the Workspace attribute of the
        same name.
        """
        for workspace in self.workspaces:
            if workspace.name_or_id == name_or_id:
                return workspace

    def get_collections(self):
        """Returns all the collections belonging to workspaces
        of this service.
        """
        collections = []
        for workspace in self.workspaces:
            collections.extend(workspace.collections)
        return collections
        
    def to_service(self, prefix=ATOMPUB_PREFIX, namespace=ATOMPUB_NS):
        """Generates and returns a ``bridge.Element`` instance of the service
        document and its workspaces.

        The ``prefix`` is the XML prefix to use 
        The ``namespace`` is the amespace associated with the service document
        
        """
        service = Element(u'service', prefix=prefix, namespace=namespace)
        service.workspace = []
        for workspace in self.workspaces:
            ws = workspace.workspace
            ws.parent = service
            service.xml_children.append(ws)
            service.workspace.append(ws)

        return service
    service = property(to_service)
