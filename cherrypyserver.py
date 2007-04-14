# -*- coding: utf-8 -*-
from static import Cling
import clr
clr.AddReference('Xameleon')

import os
import cherrypy
from cpappsite import app

def run(blocking=False):
    cur_dir = os.getcwd()
    cherrypy.config.update({'checker.on': False,
                            'engine.autoreload_on': False,
                            'server.socket_port': 9999,
                            'server.thread_pool': 10,})
	
    cherrypy.tree.graft(app)
    cherrypy.server.quickstart()
    cherrypy.engine.start(blocking=blocking)


def shutdown():
    cherrypy.engine.stop()

if __name__ == '__main__':
    run(blocking=True)
