# -*- coding: utf-8 -*-

__doc__ = """Really simple indexing implementation for amplee

I assume something bigger such as Lucene would be much more efficient.

For example:

>>> import time
>>> from amplee.indexer import *
>>> ind = Indexer()
>>> container = ShelveContainer('/tmp/cache.p')
>>> pi = PublishedIndex('pi', container=container, granularity=DateIndex.month)
>>> container2 = ShelveContainer('/tmp/cache2.p')
>>> ai = AuthorIndex('ai', container=container2)
>>> ind.register(pi)
>>> ind.register(ai)
>>> ind.start_daemon(interval=1.0)
>>> try:
>>>     index(a)
>>>     time.sleep(3)
>>> finally:
>>>     ind.stop_daemon()

>>> from datetime import datetime
>>> start = datetime(2006, 10, 8, 14, 00, 12)
>>> end = datetime(2008, 10, 8, 16, 00, 12)
>>> r0 = pi.between(start, end)
# r0 is either None or a dictionary which keys are the name of collections
# and the values are the list of members within those collections that
# match the pattern
>>> print r0 
>>> r1 = ai.lookup('Jon Doe')
>>> print r1

# You can even concatenate both set of results into one
>>> print r0 + r1

# Or substract (assuming r2)
>>> print r0 + r2 - r1

Of course since the results returned by the indexes are
dictionnaries you can use the sets stdlib module as follow:

>>> from sets import Set
>>> s0 = Set(r0[key])
>>> s1 = Set(r1[key])
>>> s0 & s1

Once you get a result dictionnary you can do something like this:

>>> collection_name = r.keys()[0]
>>> c0 = service.get_collection(collection_name)
>>> members = c0.reload_members_from_list(r[collection_name])

And get a list of member instances.

This indexer only pushed new data and never removes nor updates values.
This means that after a while it can return a lot of false positive.

You can rebuild it with something along:

>>> members = collection.reload_members()
>>> for member in members:
        index(member)
        ind.apply_all()

If you do this on a seperate process (as this can be a long process)
you will be able to rebuild the index.

"""

from Queue import Queue, Empty
import sha
import threading
from datetime import datetime
import re
import shelve

from amplee.utils import parse_isodate

__all__ = ['Indexer', 'BaseIndex', 'PublishedIndex', 'AuthorIndex',
           'CategoryIndex', 'UpdatedIndex', 'DateIndex', 'MemoryContainer',
           'index', 'ShelveContainer', 'KeywordIndex']

_queue = Queue()

def index(member):
    """Add the member to queue of elements to be indexed.

    The ``member`` is an instance of any of the member classes
    provided by amplee.

    Note that qe-queuing will be asynchronized and thus this
    function will return before the member is actually indexed.
    """
    _queue.put(member)

class MemoryContainer(object):
    """Dummy memory container based on the dictionnary.
    This does not do thread locking so be careful
    of what you do with it :)
    """
    def __init__(self):
        self._container = {}

    def start(self):
        return self._container

    def shutdown(self, container):
        pass

class ShelveContainer(object):
    def __init__(self, abs_path, protocol=2):
        """Simple container around the built-in shelve module.
        This provides a simple and more-or-less efficient
        persistence system.

        The ``abs_path`` is the absolute path of the shelve to
        open (and create if it does not exist).

        The ``protocol`` is the protocol value used by the open()
        function of the shelve module.

        If you plan to create your own container, they must
        implement the start and shutdown methods as-is.
        """
        self.abs_path = abs_path
        self.protocol = protocol

    def start(self):
        """Initiates and returns the shelve container
        """
        return shelve.open(self.abs_path, protocol=self.protocol)

    def shutdown(self, shelf):
        """Closes the shelve container when not needed anymore.
        """
        if not shelve: return
        try:
            shelf.close()
        except:
            pass

# Stolen from CherryPy
class CyclicTimer(threading._Timer):
    """A thread timer that runs untl it is explicitely stopped.
    """
    def run(self):
        while True:
            self.finished.wait(self.interval)
            if self.finished.isSet():
                break
            self.function(*self.args, **self.kwargs)

class Indexer(object):
    def __init__(self, batch=5):
        """This class is the manager of your index handlers.
        You register your index handlers and then you start the
        indexer daemon which will run at regular interval, polling
        from the global queue members to index.

        If you have your own even mechanism you don't need to
        start the daemon and can simply call `apply_all` to
        perform the same job at will.

        The ``batch`` parameter indicates how many members
        to dequeue during one iteration of indexing.
        """
        self.indexes = {}
        self.index_timer = None
        self.batch = batch

    def register(self, index):
        """Registers a new index handler."""
        self.indexes[index.name] = index

    def unregister(self, index):
        """Removes an index handler"""
        if index.name in self.indexes:
            del self.indexes[index.name]

    def start_daemon(self, interval=60.0):
        """Convenient way to start a daemon timer that will
        process the queue of indexable members.

        Note that if you try to start it before it was stopped
        an error will be raised.
        """
        if self.index_timer is not None:
            raise RuntimeError, "Index daemon already started"
        
        t = CyclicTimer(interval, self.apply_all)
        t.setName('Amplee indexer')
        self.index_timer = t
        self.index_timer.start()

    def stop_daemon(self):
        """Stops the daemon timer"""
        if self.index_timer is not None:
            self.index_timer.cancel()
            self.index_timer.join()
            self.index_timer = None
            
    def apply_all(self):
        """Unqueue up to `self.batch` number
        of members from the global queue and apply
        all the index handlers.

        A handler only needs to be a class that implements
        a method named `update` and taking the `member` as its
        unique parameter.
        """
        for unused in xrange(0, self.batch):
            try:
                member = _queue.get_nowait()
            except Empty:
                return
            for name in self.indexes:
                self.indexes[name].update(member)

class Result(dict):
    """This class allows to perform some very trivial
    operations on results returned by the indexes.

    This is certainly suboptimal but again the indexer module
    is meant to provide local indexing rather than deal with
    millions of values.
    """
    def __add__(self, other):
        if not other:
            return self
        r = Result()
        r.update(self)
        for key in other.iterkeys():
            if key in r:
                other_values = other[key]
                for other_key in other_values:
                    if other_key not in r[key]:
                        r[key].append(other_key)
            else:
                r[key] = other[key][:] 
        return r

    def __sub__(self, other):
        if not other:
            return self
        r = Result()
        r.update(self)
        for key in other.iterkeys():
            if key in r:
                del r[key]

        return r
    
    def __radd__(self, other):
        if not other:
            return self
        r = Result()
        r.update(self)
        for key in other.iterkeys():
            if key in r:
                other_values = other[key]
                for other_key in other_values:
                    if other_key not in r[key]:
                        r[key].append(other_key)
            else:
                r[key] = other[key][:] 
        return r

    def __rsub__(self, other):
        if not other:
            return self
        r = Result()
        r.update(self)
        for key in other.iterkeys():
            if key in r:
                del r[key]

        return r
            
class BaseIndex(object):
    def __init__(self, name, container=None):
        """
        Base class of your index handler.

        Built-in index handlers will always inherit from
        this class but your own handlers don't need to
        as long as they implement a method named `update`
        and taking the `member` as its unique parameter.

        The ``name`` parameter is an internal identifier.

        The ``container`` is an instance of object
        implementing the dictionnary interface.
        """
        self.name = name
        self.container = container

    def update(self, member):
        raise NotImplemented

    def _cleanup_result(self, result):        
        # Cleanup for any duplicates...
        final_result = Result()
        if result:
            for (collection, member_id) in result:
                if collection not in final_result:
                    final_result[collection] = [member_id]
                elif member_id not in final_result[collection]:
                    final_result[collection].append(member_id)

            return final_result

    def store(self, key, value):
        """Stores a value within the index container

        The ``key`` and ``value`` can be any object
        as long as the container can cope with it.
        """
        container = None
        try:
            container = self.container.start()
            if not key in container:
                container[key] = [value]
            else:
                container[key].append(value)
        finally:
            self.container.shutdown(container)

    def load(self, key):
        value = container = None
        try:
            container = self.container.start()
            if key in container:
                value = container[key]
        finally:
            self.container.shutdown(container)
        return value

    def iterindex(self, func):
        """Iterates through the keys of the
        container and apply for each one the provided ``func``
        with the key and the data associated.

        If you want to stop the iteration you can
        simply raise StopIteration from the function.

        The method returns a list of results provided
        by the function calls.
        """
        container = None
        result = []
        try:
            container = self.container.start()
            for key in container.iterkeys():
                value = func(key, container[key])
                if isinstance(value, list):
                    result.extend(value)
                elif value is not None:
                    result.append(value)
        except StopIteration:
            pass
        
        self.container.shutdown(container)

        return result

class DateIndex(BaseIndex):
    def __init__(self, name, target, container=None, granularity=None):
        """Base class for date based inde handlers.

        The ``target`` is the name of the element within the atom entry
        to search for.

        The ``granularity`` is one of the classmethods of this class
        that indicates what will be the granularity of the key of the index.

        The lower the finer but also the more keys you will have. It
        defaults to ``DateIndex.hour``.
        """
        BaseIndex.__init__(self, name, container)
        if not granularity:
            granularity = DateIndex.hour
        self.granularity = granularity
        self.target = target
        
    def update(self, member):
        entry = member.atom
        published = entry.get_child(self.target, entry.xml_ns)
        if published:
            pub = self.granularity(parse_isodate(published.xml_text))
            self.store(str(pub),  (member.collection.name_or_id, member.member_id))
            
    def between(self, start, end):
        """Returns a dictionnary of the forum {collection_name: [member_id]}
        which have been indexed between the two provided dates.
        """
        def _between(key, data):
            dt = parse_isodate(key)
            if start <= dt <= end:
                return data

        result = self.iterindex(_between)

        return self._cleanup_result(result)

    def year(cls, dt):
        return datetime(dt.year, 1, 1, 0, 0)
    year = classmethod(year)
    
    def month(cls, dt):
        return datetime(dt.year, dt.month, 1, 0, 0)
    month = classmethod(month)
    
    def day(cls, dt):
        return datetime(dt.year, dt.month, dt.day, 0, 0)
    year = classmethod(day)
    
    def hour(cls, dt):
        return datetime(dt.year, dt.month, dt.day, dt.hour, 0)
    hour = classmethod(hour)
    
    def minute(cls, dt):
        return datetime(dt.year, dt.month, dt.day, dt.hour, dt.minute)
    minute = classmethod(minute)
            
class PublishedIndex(DateIndex):
    def __init__(self, name, container=None, granularity=None):
        DateIndex.__init__(self, name, 'published', container, granularity)
    
class UpdatedIndex(DateIndex):
    def __init__(self, name, container=None, granularity=None):
        DateIndex.__init__(self, name, 'updated', container, granularity)
    
class AuthorIndex(BaseIndex):
    def __init__(self, name, container=None, index_name=True, index_uri=False, index_email=False):
        """Simple author indexing.

        This class will create a sha hash value of the concatenation of
        the name, uri and email if the three have been enabled.
        """
        BaseIndex.__init__(self, name, container)
        self.index_name = index_name
        self.index_uri = index_uri
        self.index_email = index_email

    def update(self, member):
        entry = member.atom
        authors = entry.get_children('author', entry.xml_ns)
        if authors:
            for author in authors:
                name = uri = email = ''
                if self.index_name:
                    name = author.get_child('name', author.xml_ns)
                if self.index_uri:
                    uri = author.get_child('uri', author.xml_ns)
                if self.index_email:
                    email = author.get_child('email', author.xml_ns)

                hashed_key = sha.new('%s%s%s' % (name, uri, email)).hexdigest()
                self.store(hashed_key,  (member.collection.name_or_id, member.member_id))
            
    def lookup(self, name='', uri='', email=''):
        hashed_key = sha.new('%s%s%s' % (name, uri, email)).hexdigest()
        return self._cleanup_result(self.load(hashed_key))

class CategoryIndex(BaseIndex):
    def __init__(self, name, container=None, index_term=True, index_scheme=False):
        """Simple category indexing.
        """
        BaseIndex.__init__(self, name, container)
        self.index_term = index_term
        self.index_scheme = index_scheme

    def update(self, member):
        entry = member.atom
        categories = entry.get_children('category', entry.xml_ns)
        if categories:
            for category in categories:
                term = scheme = ''
                if self.index_term:
                    term = category.get_attribute('term')
                if self.index_scheme:
                    scheme = category.get_attribute('scheme')

                hashed_key = sha.new('%s%s' % (term, scheme)).hexdigest()
                self.store(hashed_key, (member.collection.name_or_id, member.member_id))
            
    def lookup(self, term='', scheme=''):
        hashed_key = sha.new('%s%s' % (term, scheme)).hexdigest()
        return self._cleanup_result(self.load(hashed_key))

class KeywordIndex(BaseIndex):
    def __init__(self, name, container=None, keywords=None):
        """Simple keyword indexing on the content element
        """
        BaseIndex.__init__(self, name, container)
        self.keywords = keywords
        self._r = re.compile('|'.join(self.keywords))

    def update(self, member):
        entry = member.atom
        content = entry.get_child('content', entry.xml_ns)
        if content:
            content_type = content.get_attribute('type')
            if unicode(content_type) == u'text':
                match = self._r.search(content.xml_text)
                if match is not None:
                    for keyword in self.keywords:
                        self.store(keyword, (member.collection.name_or_id, member.member_id))
            
    def contains(self, keyword):
        return self._cleanup_result(self.load(keyword))
