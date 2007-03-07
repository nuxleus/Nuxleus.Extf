# -*- coding: utf-8 -*-

import clr
import sys
import os.path
cur_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'bin')
sys.path.append(cur_dir)
clr.AddReferenceToFile('Xameleon.dll')

## from cherrypy import wsgiserver
## wsgi_apps = [('', app)]

## server = wsgiserver.CherryPyWSGIServer(('localhost', 9999), wsgi_apps,
##                                        server_name='localhost')

if __name__ == '__main__':
##     try:
##         print "Serving HTTP on http://localhost:9999"
##         server.start()
##     except KeyboardInterrupt:
##         server.stop()

    import os
    import cherrypy

    cur_dir = os.getcwd()
    cherrypy.config.update({'checker.on': False,
                            'engine.autoreload_on': False,
                            'server.socket_port': 9999})

    from appsite import app
    cherrypy.tree.graft(app)
    cherrypy.server.quickstart()
    cherrypy.engine.start()
