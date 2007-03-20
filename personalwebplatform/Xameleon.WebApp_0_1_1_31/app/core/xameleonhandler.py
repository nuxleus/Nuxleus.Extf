# -*- coding: utf-8 -*-

# import Xameleon
# from Xameleon import entry_point

__all__ = ['xamdler']

# The simplest WSGI application you can have
def xamdler(environ, start_response, exc_info=None):
    status = '200 OK'
    response_headers = [('Content-type', 'text/html'),
                        ('Content-Language', 'en-US')]

    # This calls initializes the process of the response
    # by sending the headers for example
    start_response(status, response_headers)

    body = ''
    # Then we return the actual content of the application
    # If you need to get anything from the request,
    # uncomment the next line, restart and look at the log:
    ## print environ
    # For example:
    ## body = entry_point(environ['HTTP_METHOD'])
    return [body]
