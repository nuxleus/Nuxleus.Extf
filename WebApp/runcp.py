# -*- coding: utf-8 -*-

import os.path

def parse_commandline():
    from optparse import OptionParser
    parser = OptionParser()
    parser.add_option("-c", "--configuration", dest="configuration",
                      help="configuration file path")
    parser.add_option("-a", "--aws", dest="aws",
                      help="path to the file containing the AWS keys")
    (options, args) = parser.parse_args()

    return options

def run_application():
    from core.application import create_application
    cur_dir = os.path.dirname(os.path.abspath(__file__))
    options = parse_commandline()
    create_application(options, cur_dir)

    import cherrypy
    cherrypy.config.update({'server.thread_pool': 15,
                            'server.socket_port': 8080,
                            'engine.autoreload_on': False,
                            'log.screen': True,
                            'log.error_file': os.path.join(cur_dir, "error.log"),
                            'log.access_file': os.path.join(cur_dir, "access.log")})
    cherrypy.server.quickstart() 
    cherrypy.engine.start()

if __name__ == '__main__':
    run_application()
