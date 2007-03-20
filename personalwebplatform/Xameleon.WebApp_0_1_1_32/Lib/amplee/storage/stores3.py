# -*- coding: utf-8 -*-

__all__ = ['S3Storage']

try:
    from cStringIO import StringIO
except ImportError:
    from StringIO import StringIO

# http://code.google.com/p/boto/
import boto
try:
    from boto.key import Key
except ImportError:
    # 0.8 and above
    from boto.s3.key import Key
    
from amplee.storage import Storage
from amplee.utils import generate_uuid

class KeyWrapper(Key):
    # The boto key class does not allow the mime-type
    # to be passed to the method this storage uses most
    # of the time so we add it :)
    def set_content(self, fp, media_type):
        if self.bucket != None:
            self._compute_md5(fp)
            self.content_type = media_type
            self.send_file(fp)

class S3Storage(Storage):
    def __init__(self, aws_access_key_id, aws_secret_access_key,
                 unique_prefix, encoding='utf-8', separator='_'):
        """
        Amazon S3 storage for amplee.
        http://www.amazon.com/gp/browse.html?node=16427261

        If the buck does not exist this storage will create it transparently.

        Keyword arguments
        aws_access_key_id -- Amazon S3 publick key
        aws_secret_access_key -- Amazon S3 private key
        unique_prefix -- Unique prefix used for the creation of buckets
        encoding -- when your content is unicode it has to be encoded before being sent
        to the Amazon servers (default: utf-8)
        separator -- character used to join the prefix and the bucket

        Set it to something really unique and not None!
        """
        self.unique_prefix = unique_prefix
        self.s3conn = boto.connect_s3(aws_access_key_id=aws_access_key_id,
                                      aws_secret_access_key=aws_secret_access_key)
        self.encoding = encoding
        self.separator = separator
        
    def shutdown(self):
        """
        Does nothing effectively.
        """

    def __bn(self, value):
        """
        Constructs the bucket name 
        """
        return "%s%s%s" % (self.unique_prefix, self.separator, value)

    def create_container(self, collection_name):
        """
        Creates and returns a boto bucket instance for that collection name
        prefixed by self.unique_prefix
        """
        return self.s3conn.create_bucket(self.__bn(collection_name))

    def path(self, *args, **kwargs):
        """
        Returns a KeyWrapper instance.

        You must provided at least two non keyword arguments:
        the collection name and the resource name
        """
        collection = args[0]
        resource = args[1]
        bucket = self.create_container(collection)
        key = KeyWrapper(bucket)
        key.key = resource
        return key

    def get_content(self, path):
        """
        Returns the content of the resource at 'path' as a string

        Keyword argument:
        path -- a Key or KeyWrapper instance
        """
        if path:
            return path.get_contents_as_string().decode(self.encoding)

    def get_meta_data(self, path):
        """
        Returns the content of the resource at 'path' as a string

        Keyword argument:
        path -- a Key or KeyWrapper instance
        """
        if path:
            return path.get_contents_as_string().decode(self.encoding)

    def put_content(self, path, content, media_type=None, *args, **kwargs):
        """
        Updates the content of the resource at 'path'

        Keyword argument:
        path -- a KeyWrapper instance
        content -- content as a string to be persisted into the bucket
        media_type -- mime type of the resource (default:None)
        """
        if path:
            fp = StringIO(content.encode(self.encoding))
            path.set_content(fp, media_type)
            fp.close()

    def put_meta_data(self, path, content, media_type=None, *args, **kwargs):
        """
        Updates the content of the resource at 'path'

        Keyword argument:
        path -- a KeyWrapper instance
        content -- content as a string to be persisted into the bucket
        media_type -- mime type of the resource (default:None)
        """
        if path:
            fp = StringIO(content.encode(self.encoding))
            path.set_content(fp, media_type)
            fp.close()

    def remove_content(self, path):
        """
        Removes the resource at 'path'
        
        Keyword argument:
        path -- a Key or KeyWrapper instance
        """
        if path:
            path.bucket.delete_key(path)

    def remove_meta_data(self, path):
        """
        Removes the resource at 'path'
        
        Keyword argument:
        path -- a Key or KeyWrapper instance
        """
        if path:
            path.bucket.delete_key(path)

    def persist(self, path_list, msg=None):
        """
        Does nothing
        """
        pass
    
    def exists(self, path):
        """
        Returns True if the resource 'path' exists in the bucket
        """
        if path:
            result = path.bucket.lookup(path.key)
            if result:
                return True
        return False

    def ls(self, collection_name, ext, distinct=False):
        """
        Returns a dictionary of the form {resource_name: {'path': KeyWrapper}}
        
        Keyword argument:
        collection_name -- the name of the collection to browse
        ext -- extension to filter through
        distinct -- if true returns all resources with an extension
        different from 'ext'
        """
        bucket = self.create_container(collection_name)
        results = bucket.get_all_keys()
        if ext and ext[0] != '.':
            ext = '.%s' % ext
        items = {}
        for result in results:
            if ext:
                if distinct and not result.key.endswith(ext):
                    items[result.key] = {'path': result}
                elif not distinct and result.key.endswith(ext):
                    items[result.key] = {'path': result}
            else:
                items[result.key] = {'path': result}
                
        return items
