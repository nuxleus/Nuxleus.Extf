# -*- coding: utf-8 -*-
import os
import cherrypy
from cpappsite import create_store, cur_dir

def filtered_basic_auth(realm, users, allow):
    if cherrypy.request.method.upper() not in allow:
        cherrypy.lib.auth.basic_auth(realm, users)

cherrypy.tools.filtered_auth = cherrypy.Tool('on_start_resource', filtered_basic_auth, priority=20)

def get_user():
    return {'test': '098f6bcd4621d373cade4e832627b4f6'}

def run(blocking=False):
    cur_dir = os.getcwd()
    CA = os.path.join(cur_dir, 'server.crt')
    KEY = os.path.join(cur_dir, 'server.key')

    cherrypy.config.update({'checker.on': False,
                            'engine.autoreload_on': False,
                            'server.socket_port': 9999,
                            'server.thread_pool': 10, 
			    'tools.proxy.on': True,
			    'log.screen': False,
			    'log.access_file': os.path.join(cur_dir, 'cp_access.log'),
		            'log.error_file': os.path.join(cur_dir, 'cp_error.log'),
			    #'server.ssl_certificate': CA,
			    #'server.ssl_private_key': KEY
			  })

    method_dispatcher = cherrypy.dispatch.MethodDispatcher()
    conf = {'/service': {'request.dispatch': method_dispatcher},
            '/collection': {'request.dispatch': method_dispatcher,
                            'tools.etags.on': True,
                            'tools.etags.autotags': False,
			    'tools.filtered_auth.on': True,
			    'tools.filtered_auth.realm': 'codemerge.sonicradar.com',
			    'tools.filtered_auth.users': get_user,
		            'tools.filtered_auth.allow': ['GET', 'HEAD']},
            '/static': {'tools.staticdir.on': True,
                        'tools.staticdir.dir': 'public_web',
                        'tools.staticdir.root:': cur_dir}}
    cherrypy.tree.mount(create_store(), '/', config=conf)
    cherrypy.server.quickstart()
    cherrypy.engine.start(blocking=blocking)

def shutdown():
    cherrypy.engine.stop()

if __name__ == '__main__':
    run(blocking=True)
