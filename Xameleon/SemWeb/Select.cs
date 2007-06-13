using System;
using SemWeb;
using System.Text;
using System.IO;
using System.Xml;
using System.Web;

namespace Xameleon.SemWeb {
    public class Select {
        const string RDF = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        const string FOAF = "http://xmlns.com/foaf/0.1/";

        static readonly Entity rdftype = RDF + "type";
        static readonly Entity foafPerson = FOAF + "Person";
        static readonly Entity foafknows = FOAF + "knows";
        static readonly Entity foafname = FOAF + "name";

        public Select() { }

        public static void Process(HttpResponse response, String foaf) {

            TextWriter output = response.Output;
            Uri uri = new Uri(foaf);

            Store store = new MemoryStore();
            RdfReader reader = RdfReader.LoadFromUri(uri);
            reader.BaseUri = uri.OriginalString;
            store.Import(reader);


            output.Write("These are the people in the file:");
            foreach (Statement s in store.Select(new Statement(null, rdftype, foafPerson))) {
                foreach (Resource r in store.SelectObjects(s.Subject, foafname))
                    output.WriteLine(r.ToString());
            }
            output.WriteLine();

            output.Write("And here's RDF/XML just for some of the file:");

            using (RdfWriter w = new RdfXmlWriter(output)) {
                
                store.Select(new Statement(null, foafname, null), w);
                store.Select(new Statement(null, foafknows, null), w);
            }
            output.WriteLine();
        }

    }

}
