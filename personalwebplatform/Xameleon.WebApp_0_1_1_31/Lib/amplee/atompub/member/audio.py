# -*- coding: utf-8 -*-

__doc__ = """
This module defines a set of member classes aiming at
dealing with audio formats such as Vorbis, Flac, MP3, WMA, etc.

This classes will generate an entry member based on the id3 tags
of media resources. This means that you should be careful on
how those are set before using these classes.

This package will require the mutagen library that your can find at:
http://cheeseshop.python.org/pypi/mutagen/

And wmainfo for WMA support:
http://cheeseshop.python.org/pypi/wmainfo-py
"""

__all__ = ['ID3BasedMember', 'MP3Member', 'OGGMember', 'FlacMember',
           'M4AMember', 'WavPackMember', 'WMAMember']

from datetime import datetime
import os

from bridge import Element, Attribute
from bridge.lib import isodate
from bridge.common import ATOM10_PREFIX, ATOMPUB_PREFIX, XML_PREFIX, XML_NS, \
     ATOM10_NS, ATOMPUB_NS

from amplee.utils import generate_uuid_uri, parse_isodate, get_isodate
from amplee.utils import create_temporary_resource, delete_temporary_resource
from amplee.error import MemberMediaError
from amplee.atompub.member import MediaMember
from amplee.atompub.member.helper import MemberHelper
 
from mutagen.easyid3 import EasyID3
from mutagen.mp3 import MP3
from mutagen.oggvorbis import OggVorbis
from mutagen.oggflac import OggFLAC
from mutagen.flac import FLAC, FLACNoHeaderError
from mutagen.wavpack import WavPack
from mutagen.apev2 import APEv2File
from mutagen.m4a import M4A

try:           
     from wmainfo import WmaInfo
except ImportError:
     from warnings import warn
     warn("Couldn't import wmainfo.WmaInfo. WMA files will not be supported")
     WmaInfo = None
 
class ID3BasedMember(MediaMember):
    def __init__(self, collection, source, audio, 
                 ext, media_type, title=u'', description=u'', slug=None, existing_member=None, 
                 name_format='%A %d %n', entry_id_creator=None, name_creator=None, **kwargs):
        """
        Creates a member based on ID3 tags, typically audio files such a MP3 or Vorbis

        Keyword arguments
        collection -- parent collection
        source -- fileobject of content to handle
        length -- amount of data to read from source
        title -- title to use for the Atom entry. If not provided
        will use 'title' ID3 tag. 
        description -- summary to use for the Atom entry
        slug -- hint on how to generate the name of the resource and
        used as the last part of the edit and edit-media IRIs (default:None)
        name_format -- template to generate the member and media ids
        ext -- extension to use fo the media resource
        media_type -- mime type of the media resource
        existing_member -- if provided it must be an entry as a bridge.Element instance.
        It should be the existing member on the server which is going to be updated.
        entry_id_creator -- function object which will return the id
        to use in the atom:id element (as an unicode object)
        name_creator -- callable which must return an unicode object used
        as the name of the resource and as the last segment of it IRI (default:None)
        
        The name format defaults takes the following values:
        %A -- artist name
        %d -- album name
        %t -- tracknumber
        %n -- track title
        %Y -- year
        %M -- month
        %D -- day
        It defaults to %A %d %n

        The entry_id_creator function must takes the following parameters:
        base_uri, source, artist, album, tracknumber, title, date, genres
        
        If no 'name_creator' provided, the name will default the processing
        of 'name_format'.
        If provided it must take two parameters, the 'name_format' processed
        and the 'slug' value provided.
        """
        MediaMember.__init__(self, collection, media_type=media_type)

        if not name_format:
            name_format = '%A %d %n'
                
        artist, album, tracknumber, title, date, genres = self._get_infos(audio)
        name_format = name_format.replace('%A', artist)
        name_format = name_format.replace('%d', album)
        name_format = name_format.replace('%t', tracknumber)
        name_format = name_format.replace('%n', title)
        name_format = name_format.replace('%Y', str(date.year))
        name_format = name_format.replace('%M', "%02d" % date.month)
        name_format = name_format.replace('%D', "%02d" % date.day)

        if not existing_member:
             if callable(name_creator):
                  media_id = member_id = name_creator(self.collection, slug, name_format)
             else:
                  media_id = member_id = name_format
             if ext:
                  media_id = u'%s.%s' % (media_id, ext)
             if self.collection.member_extension:
                  member_id = u'%s.%s' % (media_id, self.collection.member_extension)
        else:
            member_id = existing_member.member_id
            media_id = existing_member.media_id
            
        self.media_id = media_id
        self.member_id = member_id

        id = self.generate_id(existing_member, entry_id_creator, True, self.collection.base_uri, source,
                              artist, album, tracknumber, title, date, genres)

        mh = MemberHelper(self.collection, existing_member)
        mh.initiate(id=id)
        mh.add_element('title', content=title, attributes={u'type': u'text'})
        mh.add_element('summary', unicode(description), attributes={u'type': u'text'})
        author = mh.add_element('author')
        mh.add_element('name', content=unicode(artist), parent=author)
        mh.add_edit_link(self.member_id)
        mh.add_edit_media_link(self.media_id, self.media_type)

        for genre in genres:
             mh.add_element(u'category', attributes={u'term': unicode(genre)})

        mh.add_remote_content(self.media_id, self.media_type)
        mh.validate()
        self.entry = mh.entry
        del mh
        
    def _get_infos(self, audio):
        artist = u''
        if 'artist' in audio:
            artist = audio['artist'].pop()
            
        album = u''
        if 'album' in audio:
            album = audio['album'].pop()
            
        title = u''
        if 'title' in audio:
            title = audio['title'].pop()
                
        tracknumber = u''
        if 'tracknumber' in audio:
            tracknumber = audio['tracknumber'].pop()

        date = None
        if 'date' in audio:
            date = parse_isodate(audio['date'].pop())
        if not date:
            date = get_isodate()

        genres = []
        if 'genre' in audio:
            genres = audio['genre']

        return artist, album, tracknumber, title, date, genres
                
class MP3Member(ID3BasedMember):
    def __init__(self, collection, source, 
                 title=u'', description=u'',
                 name_format=None, ext='mp3', 
                 media_type=u'audio/mpeg',
                 entry_id_creator=None, **kwargs):
         fd, path, content = create_temporary_resource(source)
         audio = MP3(path, ID3=EasyID3)
         ID3BasedMember.__init__(self, collection, path, audio, 
                                 title=title, description=description,
                                 name_format=name_format, ext=ext, media_type=media_type,
                                 entry_id_creator=entry_id_creator, **kwargs)
         delete_temporary_resource(path)
        
       
class OGGMember(ID3BasedMember):
    def __init__(self, collection, source, 
                 title=u'', description=u'',
                 name_format=None, ext='ogg', 
                 media_type=u'application/ogg',
                 entry_id_creator=None, **kwargs):
         fd, path, content = create_temporary_resource(source)
         audio = OggVorbis(path)
         ID3BasedMember.__init__(self, collection, path, audio, 
                                 title=title, description=description,
                                 name_format=name_format, ext=ext, media_type=media_type,
                                 entry_id_creator=entry_id_creator, **kwargs)
         delete_temporary_resource(path)
        
class FlacMember(ID3BasedMember):
    def __init__(self, collection, source, 
                 title=u'', description=u'',
                 name_format=None, ext='flac', 
                 media_type=u'audio/x-flac',
                 entry_id_creator=None, **kwargs):
         fd, path, content = create_temporary_resource(source)
         try:
              audio = FLAC(path)
         except FLACNoHeaderError:
              try:
                   audio = OggFLAC(path)
              except:
                   raise
         ID3BasedMember.__init__(self, collection, path, audio, 
                                 title=title, description=description,
                                 name_format=name_format, ext=ext, media_type=media_type,
                                 entry_id_creator=entry_id_creator, **kwargs)
         delete_temporary_resource(path)
        
class WavPackMember(ID3BasedMember):
    def __init__(self, collection, source, 
                 title=u'', description=u'',
                 name_format=None, ext='wv', 
                 media_type=u'audio/x-wav',
                 entry_id_creator=None, **kwargs):
         fd, path, content = create_temporary_resource(source)
         audio = APEv2File(path)
         ID3BasedMember.__init__(self, collection, path, audio, 
                                 title=title, description=description,
                                 name_format=name_format, ext=ext, media_type=media_type,
                                 entry_id_creator=entry_id_creator, **kwargs)
         delete_temporary_resource(path)

    def _get_infos(self, audio):
        artist = u''
        if 'Artist' in audio:
            artist = unicode(audio['Artist'].value)
            
        album = u''
        if 'Album' in audio:
            album = unicode(audio['Album'].value)
            
        title = u''
        if 'Title' in audio:
            title = unicode(audio['Title'].value)
               
        date = None
        if 'Date' in audio:
            date = parse_isodate(audio['Date'].value)
        if not date:
            date = datetime.now()
            
        tracknumber = u''
        if 'Track' in audio:
            tracknumber = unicode(audio['Track'].value)

        return artist, album, tracknumber, title, date, []
          

class M4AMember(ID3BasedMember):
    def __init__(self, collection, source, 
                 title=u'', description=u'',
                 name_format=None, ext='m4a', 
                 media_type=u'audio/mp4a',
                 entry_id_creator=None, **kwargs):
         fd, path, content = create_temporary_resource(source)
         audio = M4A(path)
         ID3BasedMember.__init__(self, collection, path, audio, 
                                 title=title, description=description,
                                 name_format=name_format, ext=ext, media_type=media_type,
                                 entry_id_creator=entry_id_creator, **kwargs)
         delete_temporary_resource(path)

    def _get_infos(self, audio):
        artist = u''
        if '\xa9ART' in audio:
            artist = audio['\xa9ART']

        album = u''
        if '\xa9alb' in audio:
            album = audio['\xa9alb']
            
        title = u''
        if '\xa9nam' in audio:
            title = audio['\xa9nam']
                
        tracknumber = u''
        if 'tracknumber' in audio:
            tracknumber = [str(digit) for digit in audio['trkn']]
            tracknumber.reverse()
            tracknumber = ''.join(tracknumber)

        genres = []
        if '\xa9gen' in audio:
            genres = audio['\xa9gen']

        date = datetime.now()

        return artist, album, tracknumber, title, date, genres
          

if WmaInfo:
     class WMAMember(ID3BasedMember):
          def __init__(self, collection, source, 
                       title=u'', description=u'',
                       name_format=None, ext='wma', 
                       media_type=u'audio/x-ms-wma',
                       entry_id_creator=None, **kwargs):
               fd, path, content = create_temporary_resource(source)
               audio = WmaInfo(path)
               ID3BasedMember.__init__(self, collection, path, audio, 
                                       title=title, description=description,
                                       name_format=name_format, ext=ext, media_type=media_type,
                                       entry_id_creator=entry_id_creator, **kwargs)
               delete_temporary_resource(path)
         
          def _get_infos(self, audio):
               artist = unicode(audio.tags.get('Author', ''))
               title = unicode(audio.tags.get('Title', ''))
               tracknumber = unicode(audio.tags.get('TrackNumber', ''))
               album = unicode(audio.tags.get('AlbumTitle', ''))
               genres = [unicode(audio.tags.get('Genre', ''))]
               date = audio.tags.get('Year', '')
               date = parse_isodate(date)
               if not date:
                    date = datetime.now()

               return artist, album, tracknumber, title, date, genres

