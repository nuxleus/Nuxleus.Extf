# -*- coding: utf-8 -*-

import os.path
import cherrypy
from core.application import create_application

def parse_commandline():
    from optparse import OptionParser
    parser = OptionParser()
    parser.add_option("-c", "--configuration", dest="configuration",
                      help="configuration file path")
    parser.add_option("-p", "--port", dest="port",
                      help="port to listen to", default="8080")
    parser.add_option("-s", "--log-to-screen", dest="log_to_screen",
                      help="log server to screen", action="store_true")
    parser.add_option("-a", "--aws", dest="aws",
                      help="path to the file containing the AWS keys")
    (options, args) = parser.parse_args()

    return options

def run_application():
    cur_dir = os.path.dirname(os.path.abspath(__file__))
    
    options = parse_commandline()
    cherrypy.config.update({'checker.on': False,
                            'server.thread_pool': 15,
                            'server.socket_port': int(options.port),
                            'engine.autoreload_on': False,
                            'log.screen': options.log_to_screen,
                            'log.error_file': os.path.join(cur_dir, "error.log"),
                            'log.access_file': os.path.join(cur_dir, "access.log")})
    
    create_application(options, cur_dir)

    cherrypy.server.quickstart() 
    cherrypy.engine.start()

if __name__ == '__main__':
    run_application()
