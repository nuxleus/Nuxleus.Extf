# -*- coding: utf-8 -*-

# Simplest handler you can have
# A handler is a class that defines the following
# methods. If one is not implemented it won't
# raise an exception, amplee will simply
# understand you don't have extra work to do
# for that particular type of request.
#
# Each member is the instance of its particular
# Member class as you can find in
# amplee.atompub.member.*
#
# To access the actual Atom entry you have to do the
# following call:
#
# member.atom
#
# Such as: member.atom.id.xml_text
#
# You can also raise a amplee.error.ResourceOperationException
# exception to stop the process and issue an HTTP error
# to the client. In that case the resource will not
# be persisted into the storage.
#
# Implement this handler as you see fit to add extra
# functionnalities to amplee for your specific
# requirements.
#
# For an example see:
# http://trac.defuze.org/browser/oss/amplee/amplee/examples/blog/core/handler/article.py

import System.Xml as sx
import clr

try:
    clr.AddReference('Xameleon')
except IOError:
    pass
    
import Xameleon

class AtomHandler(object):
    def __init__(self, member_type):
        # The media-type that this class will handle
        self.member_type = member_type

    # This is called in some cases by amplee
    # when an error happened and gives you a chance
    # to process the exception yourself
    def on_error(self, exception, member):
        pass

    # Called upon creation of a new member
    # It provides the automatically generated member
    # (see amplee.atompub.member.*) and the
    # content sent within the request.
    # This should return the member and the content
    # again but in between you can modify both of
    # them before they get stored in the storage
    def on_create(self, member, content):
        print "on_create called"
        # I will create a friendlier approach to
        # the two next lines directly from brige
        # something like: member.atom.native() -> XmlDocument
        doc = sx.XmlDocument()
        doc.LoadXml(member.atom.xml())
        print Xameleon.Atom.AtomEntry.ToXhtml(doc)
        return member, content

    # Same as above except that it provides
    # the member as it exists within the storage
    # and the member generated from the PUT content
    # This allows you to compare the two or do whatever
    # is useful to your needs.
    def on_update(self, existing_member, new_member, new_content):
        return new_member, new_content

    # Called before the member is actually removed from
    # the storage
    def on_delete(self, member):
        pass

    # Called when the media resource is being requested
    def on_get_content(self, member, content, content_type):
        return member, content, content_type

    # Called when the entry member is being requested
    def on_get_atom(self, member):
        return member, member.atom.xml(), 'application/atom+xml'

