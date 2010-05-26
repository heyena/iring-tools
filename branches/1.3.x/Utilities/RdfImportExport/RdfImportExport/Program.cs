using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Writing;

namespace RdfConverter
{
    class Program
    {
        private static string method = string.Empty;
        private static string dbServer = string.Empty;
        private static string dbName = string.Empty;
        private static string dbUser = string.Empty;
        private static string dbPassword = string.Empty;
        private static string rdfFullFilename = string.Empty;
        private static string graphUri = string.Empty;

        static void Main(string[] args)
        {
            try
            {

                if (args.Length >= 1)
                {
                    method = args[0];
                    {
                        if (args.Length >= 7)
                        {
                            dbServer = args[1];
                            dbName = args[2];
                            dbUser = args[3];
                            dbPassword = args[4];
                            rdfFullFilename = args[5];
                            graphUri = args[6];
                        }
                        if (method.ToUpper() == "IMPORT")
                        {
                            DoImport(dbServer, dbName, dbUser, dbPassword, rdfFullFilename, graphUri);
                        }
                        else if (method.ToUpper() == "EXPORT")
                        {
                            DoExport(dbServer, dbName, dbUser, dbPassword, rdfFullFilename, graphUri);
                        }
                        else
                        {
                            PrintUsage();
                        }
                    }
                }
                else
                {
                    method = ConfigurationManager.AppSettings["Method"];

                    if (method.ToUpper() == "IMPORT")
                    {
                        GetAppSettings();
                        DoImport(dbServer, dbName, dbUser, dbPassword, rdfFullFilename, graphUri);
                    }
                    else
                    {
                        GetAppSettings();
                        DoExport(dbServer, dbName, dbUser, dbPassword, rdfFullFilename, graphUri);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*** ERROR ***\n " + ex);
                Console.ReadKey();
            }
        }

         static void GetAppSettings()
        {
             dbServer = ConfigurationManager.AppSettings["DBServer"];
             dbName = ConfigurationManager.AppSettings["DBName"];
             dbUser = ConfigurationManager.AppSettings["DBUser"];
             dbPassword = ConfigurationManager.AppSettings["DBPassword"];
             rdfFullFilename = ConfigurationManager.AppSettings["RdfImportFullFilename"];
             graphUri = ConfigurationManager.AppSettings["GraphUri"];
        }

        private static void DoExport(string dbServer, string dbName, string dbUser, string dbPassword, string rdfFullFilename, string graphUri)
        {
            MicrosoftSqlStoreManager msStore = new MicrosoftSqlStoreManager(dbServer, dbName, dbUser, dbPassword);
            msStore.Open(false);
            RdfXmlTreeWriter rdfXmlWriter = new RdfXmlTreeWriter();
            
            Graph workGraph = new Graph();
            msStore.LoadGraph(workGraph, graphUri);

            rdfXmlWriter.Save(workGraph, rdfFullFilename);
            msStore.Dispose();

            Console.WriteLine("Graph[" + graphUri + "] written to " + rdfFullFilename);
            Console.ReadKey();

        }

        private static void DoImport(string dbServer, string dbName, string dbUser, string dbPassword, string rdfFullFilename, string graphUri)
        {
            MicrosoftSqlStoreManager msStore = new MicrosoftSqlStoreManager(dbServer, dbName, dbUser, dbPassword);
            msStore.Open(false);

            Graph workGraph = new Graph();
            FileLoader.Load(workGraph, rdfFullFilename);
            workGraph.BaseUri = new Uri(graphUri);

            msStore.SaveGraph(workGraph);
            msStore.Dispose();

            Console.WriteLine("Graph[" + graphUri + "] imported from " + rdfFullFilename);
            Console.ReadKey();
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("\\n\\RdfImportExport.exe Import DbServer DbName DbUser DbPassword RdfImportFullFilename GraphUri\\n");
            Console.WriteLine("\\n\\RdfImportExport.exe Export DbServer DbName DbUser DbPassword RdfExportFullFilename GraphUri\\n");
            Console.ReadKey();
        }
    }
}
