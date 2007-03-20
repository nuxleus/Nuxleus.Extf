# -*- coding: utf-8 -*-

__doc__ = """
Handler based on IMDB informations
Use IMDBpy (http://imdbpy.sourceforge.net/)
"""

__all__ = ['MovieMember']

#################
# DO NOT USE YET
#################

import os
import imdb

from bridge import Element as E
from bridge import Attribute as A
from bridge import ENCODING
from bridge.common import ATOM10_NS, ATOM10_PREFIX, XMLNS_PREFIX, XMLNS_NS,\
     XML_NS, XML_PREFIX, RDF_PREFIX, RDF_NS, RDF_IMDB_PREFIX, RDF_IMDB_NS

from amplee.utils import generate_uuid_uri, get_isodate
from amplee.atompub.member import MediaMember
from amplee.atompub.member.helper import MemberHelper

class MovieMember(MediaMember):
    def __init__(self, collection, source, slug=None, 
                 media_type=None, entry_id_creator=None,
                 existing_member=None, name_creator=None, **kwargs):
        """

        Keyword arguments:
        collection -- AtomPubCollection instance carrying this member
        source -- resource string to handle
        slug -- hint on how to generate the name of the resource and
        used as the last part of the edit and edit-media IRIs (default:None)
        media_type -- mime-type for this member (default:None)
        name_creator -- callable which must return an unicode object used
        as the name of the resource and as the last segment of it IRI (default:None)
        existing_member -- if provided it must be an entry as a bridge.Element instance.
        It should be the existing member on the server which is going to be updated.
        entry_id_creator -- callable which must return an unicode object used
        for the id element of the entry (default:None)

        The 'entry_id_creator' callable, if provided, must take one unique
        parameter which will be the content of the resource.

        If no 'name_creator' provided, the name will default to an UUID value.
        If provided it must take one parameter, the 'slug' value provided.

        You may provide also a 'proxy' value if needed.
        """
        MediaMember.__init__(self, collection, media_type=media_type)

        self.proxy = kwargs.get('proxy', None)
        
        id = self.generate_id(existing_member, entry_id_creator, True, slug or source)
            
        if not existing_member:
            if callable(name_creator):
                media_id = member_id = name_creator(self.collection, slug, source)
            else:
                media_id = member_id = id
            member_id = u'%s.%s' % (member_id, self.collection.member_extension)
            self.member_id = member_id
            self.media_id = media_id
        else:
            self.member_id = existing_member.member_id
            self.media_id = existing_member.media_id

        mh = MemberHelper(self.collection, existing_member) 
        mh.initiate(id=id)
        mh.add_edit_link(self.member_id)
        mh.add_edit_media_link(self.media_id, self.media_type)
        mh.add_remote_content(self.media_id, self.media_type)
        mh.validate()
        
        self.entry = mh.entry
        del mh

    def movie_to_rdf(self, movie_title, limit=-1, access_system='http', *args, **kwargs):
        """
        Generates an Atom feed of all results returned by IMDB
        Each entry matching one result.

        Call this method from the on_create, on_update handlers
        of the collection.

        Keyword arguments:
        movie_title -- the movie to look for within IMDB
        limit -- how many entries we will include within the feed
        (default: -1 means all)
        access_system -- access method to pass to IMDBpy (default: http)
        """
        rdf = E(u'rdf')
        A(u'imdb', value=RDF_IMDB_NS,
          prefix=XMLNS_PREFIX, namespace=XMLNS_NS, parent=rdf)

        print rdf.xml()

        return rdf
        

    def _make_movie_id(self, movieid):
        return u"http://www.imdb.com/title/tt%s/" % str(movieid)

    def _make_person_id(self, personnid):
        return u"http://www.imdb.com/name/nm%s/" % str(personnid)

    def fetch_movie_data(self, movie_title, limit=-1, access_system='http', *args, **kwargs):
        """
        Generates an Atom feed of all results returned by IMDB
        Each entry matching one result.

        Call this method from the on_create, on_update handlers
        of the collection.

        Keyword arguments:
        movie_title -- the movie to look for within IMDB
        limit -- how many entries we will include within the feed
        (default: -1 means all)
        access_system -- access method to pass to IMDBpy (default: http)
        """
        mdb = imdb.IMDb(accessSystem=access_system, *args, **kwargs)
        if self.proxy is not None:
            mdb.set_proxy(self.proxy)

        isodate = get_isodate()
        feed = E(u'feed')
        A(u'base', value=u'http://www.imdb.com/',
          prefix=XML_PREFIX, namespace=XML_NS, parent=feed)
        E(u'id', content=generate_uuid_uri(), parent=feed)
        E(u'published', content=isodate, parent=feed)
        E(u'updated', content=isodate, parent=feed)
        E(u'title', content=movie_title.decode(ENCODING),
          attributes={u'type': u'text'}, parent=feed)
        
        results = mdb.search_movie(movie_title)
        index = 0
        for item in results:
            mdb.update(item)
            entry = E(u'entry', parent=feed)
            E(u'id', content=self._make_movie_id(item.movieID), parent=entry)
            E(u'title', content=item['long imdb title'], 
              attributes={u'type': u'text'}, parent=entry)
            E(u'published', content=isodate, parent=entry)
            E(u'updated', content=isodate, parent=entry)
            E(u'link', attributes={u'rel': u'alternate',
                                   u'href': u'title/tt%s/' % str(item.movieID),
                                   u'type': u'text/html'}, parent=entry)
            if item.has_key('director'):
                for director in item['director']:
                    mdb.update(director)
                    author = E(u'author', parent=entry)
                    E(u'name', content=director['long imdb canonical name'], parent=author)
                    E(u'uri', content=self._make_person_id(director.personID), parent=author)
            summary = E(u'summary', content=u'', attributes={u'type': u'text'}, parent=entry)
            if item.has_key('plot outline'):
                summary.xml_text = item['plot outline']
            if item.has_key('genres'):
                for genre in item['genres']:
                    E(u'category', attributes={u'term': genre}, parent=entry)
            index = index + 1
            if limit > 0 and index == limit:
                break

        # let's update the atom prefix/namespace
        feed.update_prefix(u'atom', None, ATOM10_NS, update_attributes=False)
        return feed
