using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;

namespace dotnetRdfDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            using (MicrosoftSqlStoreManager storeManager = new MicrosoftSqlStoreManager("AUS8482", "dotnetRDFAuto", "rdladmin", "Lm!1N2w"))
            {
                //update / insert graph
                DateTime s = DateTime.Now;
                string project = "12345_000";
                string application = "PSPID";
                string[] graphName = { "Instrument", "Lines"};
                string[] rdfPath = {@"..\..\resources\RDF.12345_000.PSPID.Instrument.xml",
                 @"..\..\resources\RDF.12345_000.PSPID.Lines.xml"};
                for (int i = 0; i <= graphName.Length - 1; i++)
                {
                    string graphURI = "http://hatch.iringtools.org/" + project + "/" + application + "/" + graphName[i];
                    Uri graph = new Uri(graphURI);
                    storeManager.Open(true);
                    Graph workGraph = new Graph();
                    //storeManager.LoadGraph(workGraph, graph);
                    string graphId = storeManager.GetGraphID(graph);
                    storeManager.ClearGraph(graphId);
                    storeManager.RemoveGraph(graphId);

                    Graph g = new Graph();
                    //FileLoader.Load(g, instrumentPath);
                    FileLoader.Load(g, rdfPath[i]);
                    g.BaseUri = graph;
                    storeManager.LoadGraph(g, graphURI);
                    storeManager.SaveGraph(g);
                }
                DateTime e = DateTime.Now;
                TimeSpan d = e.Subtract(s);
                Console.WriteLine("Elapsed time = {0}m {1}s {2}ms", d.Minutes, d.Seconds, d.Milliseconds);
                Console.ReadKey();
            }
        }

    }
}
