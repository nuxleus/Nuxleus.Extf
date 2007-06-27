using System;
using System.Xml;

namespace Xameleon.Atom
{
    public class AtomEntry
    {
        public AtomEntry() {}

        public static String ToXhtml(XmlDocument atomentry) {

            // Do transformation and return string location of document
            // Need to rework the transformation process to be more efficient
            // in the way it handles the requested return type
            // So for now this is just a place holder such as the put the API/method
            // into commission such that Sylvain can test its functionality with Amplee/IronPython

            string fooPath = "/foo/bar/path/location/entry.html";

            return fooPath;

        }
    }
}
