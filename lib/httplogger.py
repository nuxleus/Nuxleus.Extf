# -*- coding: utf-8 -*-

# 2007/02/24 - sylvain
# A mix between paste.TransLogger and CherryPy logging module
# but without having to import any of those packages
# Yeap reinventing the wheel but only to make it fit my bike

__all__ = ['HTTPLogger']
__version__ = "0.1"
__authors__ = ['Sylvain Hellegouarch']
__doc__ = """
app = HTTPLogger(wsgiapp)
app.create_access_logger('/some/path/access.log', stdout=True)
app.create_error_logger('/some/path/error.log', stderr=True)
"""

import sys
import traceback
import urllib
import logging
logfmt = logging.Formatter("%(message)s")
from logging import FileHandler, StreamHandler
from datetime import datetime
import rfc822

_pattern = '%(h)s %(l)s %(u)s %(t)s "%(r)s" %(s)s %(b)s "%(f)s" "%(a)s"'

class HTTPLogger(object):
    def __init__(self, wsgiapp, access_logger=None, error_logger=None, propagate_exc=True):
        self.wsgiapp = wsgiapp
        self.access_logger = access_logger
        self.error_logger = error_logger
        self.propagate_exc = propagate_exc

    def create_access_logger(self, access_path, stdout=True):
        logger = logging.getLogger('access.logger')
        logger.setLevel(logging.INFO)
        
        h = FileHandler(access_path)
        h.setFormatter(logfmt)
        logger.addHandler(h)

        if stdout:
            h = StreamHandler(sys.stdout)
            h.setFormatter(logfmt)
            logger.addHandler(h)
        
        self.access_logger = logger
        
    def create_error_logger(self, error_path, stderr=True):
        logger = logging.getLogger('error.logger')
        logger.setLevel(logging.DEBUG)
        
        h = FileHandler(error_path)
        h.setFormatter(logfmt)
        logger.addHandler(h)

        if stderr:
            h = StreamHandler(sys.stderr)
            h.setFormatter(logfmt)
            logger.addHandler(h)
        
        self.error_logger = logger

    def __call__(self, environ, start_response):
        def start_response_logged(status, headers, exc_info=None):
            if self.propagate_exc and exc_info:
                raise exc_info[0],exc_info[1],exc_info[2]
            
            try:
                content = start_response(status, headers)
                self.log_access(status, headers, environ)
                return content
            except:
                if self.error_logger:
                    self.log_access('500 Internal Server Error', headers, environ)
                    self.log_error()
                if self.propagate_exc:
                    raise

        return self.wsgiapp(environ, start_response_logged)

    def log_access(self, status, headers, environ):
        log = _pattern % {'h': environ['REMOTE_ADDR'],
                          'l': '-',
                          'u': '-',
                          't': self.format_now(),
                          'r': self.format_request_uri(environ),
                          's': status.split(" ", 1)[0],
                          'b': '-',
                          'f': environ.get('HTTP_REFERER', ''),
                          'a': environ.get('HTTP_USER_AGENT', '')}
        self.access_logger.log(logging.INFO, log)

    def log_error(self):
        self.error_logger.log(logging.DEBUG, self.format_exc())

    def format_now(self):
        # from CherryPy
        now = datetime.now()
        month = rfc822._monthnames[now.month - 1].capitalize()
        return ('[%02d/%s/%04d:%02d:%02d:%02d]' %
                (now.day, month, now.year, now.hour, now.minute, now.second))

    def format_request_uri(self, environ):
        # from paste.TransLogger
        req_uri = urllib.quote(environ.get('SCRIPT_NAME', '')
                               + environ.get('PATH_INFO', ''))
        if environ.get('QUERY_STRING'):
            req_uri += '?'+environ['QUERY_STRING']

        req_uri = "%s %s %s" % (environ['REQUEST_METHOD'] or '', req_uri,
                                environ['SERVER_PROTOCOL'] or '')

        return req_uri

    def format_exc(self, exc=None):
        # tolen frm CherryPy
        """Return exc (or sys.exc_info if None), formatted."""
        if exc is None:
            exc = sys.exc_info()
        if exc == (None, None, None):
            return ""
        return "".join(traceback.format_exception(*exc))
