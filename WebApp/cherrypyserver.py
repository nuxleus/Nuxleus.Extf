# -*- coding: utf-8 -*-
import os
import cherrypy
from cpappsite import create_store, cur_dir

def run(blocking=False):
    cur_dir = os.getcwd()
    cherrypy.config.update({'checker.on': False,
                            'engine.autoreload_on': False,
                            'server.socket_port': 9999,
                            'server.thread_pool': 10,})

    method_dispatcher = cherrypy.dispatch.MethodDispatcher()
    conf = {'/service': {'request.dispatch': method_dispatcher},
            '/collection': {'request.dispatch': method_dispatcher,
                            'tools.etags.on': True,
                            'tools.etags.autotags': False},
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
