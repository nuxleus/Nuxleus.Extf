import clr
clr.AddReference('Xameleon')
from Xameleon import WebServer
w = WebServer()
w.SetPort(9999)
w.Start()