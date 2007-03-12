# -*- coding: utf-8 -*-

import clr
import sys
import os.path
cur_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'bin')
sys.path.append(cur_dir)
clr.AddReferenceToFile('Xameleon.dll')

import os
import cherrypy
from appsite import app

def run(blocking=False):
    cur_dir = os.getcwd()
    cherrypy.config.update({'checker.on': False,
                            'engine.autoreload_on': False,
                            'server.socket_port': 9999,
                            'server.thread_pool': 2,})

    cherrypy.tree.graft(app)
    cherrypy.server.quickstart()
    cherrypy.engine.start(blocking=blocking)

def shutdown():
    cherrypy.engine.stop()

if __name__ == '__main__':
    run(blocking=True)
