#!/usr/bin/env python

import atexit
import signal
import sys
import threading
import time

_trace = {}


def stacktrace_line(frame):
    return '%s:%s (in %s)' % (frame.f_code.co_name, frame.f_lineno, frame.f_code.co_filename)

def dump_trace(frame):
    out = []
    while frame is not None:
        out.append('    ' + stacktrace_line(frame))
        frame = frame.f_back
    return '\n'.join(out)

def tracer(frame, kind, obj):
    global _trace
    _trace[threading.currentThread()] = frame
    if (kind != 'exception') and (kind != 'c_exception') and kind != 'c_call' and kind != 'c_return':
        return tracer

def dump_thread(thread):
    global _trace
    out = [ 'Stack trace for thread %r:' % (thread,) ]
    frame = _trace.get(thread, None)
    if frame is None:
        out.append('    <unable to find stack trace>')
    else:
        out.append(dump_trace(frame))
    return '\n'.join(out)

def sigquit(signal, frame):
    sys.stderr.write('\nSIGQUIT - THREAD STACKS:\n')
    for thread in threading.enumerate():
        sys.stderr.write('\n')
        sys.stderr.write(dump_thread(thread))
    sys.stderr.write('\n')


#sys.settrace(tracer)
#threading.settrace(tracer)
sys.setprofile(tracer)
threading.setprofile(tracer)
#atexit.register(lambda: sys.settrace(None))
atexit.register(lambda: sys.setprofile(None))
signal.signal(signal.SIGQUIT, sigquit)
