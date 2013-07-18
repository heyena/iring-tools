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
//using org.iringtools.library;
//using org.iringtools.utility;

namespace RdfImportExport
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
    private static bool clearBeforeImport = false;
    static void Main(string[] args)
    {
      try
      {
                    
        if (args.Length >= 1)
        {
          method = args[0];
          {
            if (args.Length >= 8)
            {
              dbServer = args[1];
              dbName = args[2];
              dbUser = args[3];
              dbPassword = args[4];
              rdfFullFilename = args[5];
              graphUri = args[6];
              clearBeforeImport = Convert.ToBoolean(args[7]);
            }
            if (method.ToUpper() == "IMPORT")
            {
              DoImport();
            }
            else if (method.ToUpper() == "EXPORT")
            {
              DoExport();
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
            DoImport();
          }
          else
          {
            GetAppSettings();
            DoExport();
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
      clearBeforeImport = Convert.ToBoolean(ConfigurationManager.AppSettings["ClearBeforeImport"]);
      if (method.ToUpper() == "IMPORT")
      {
          rdfFullFilename = ConfigurationManager.AppSettings["RdfImportFullFilename"];
      }
      else
      {
          rdfFullFilename = ConfigurationManager.AppSettings["RdfExportFullFilename"];
      }

      graphUri = ConfigurationManager.AppSettings["GraphUri"];
      if (String.IsNullOrEmpty(graphUri))
      {
        graphUri = "dotnetrdf:default-graph";
      }
    }

    private static void DoExport()
    {
      MicrosoftSqlStoreManager msStore = new MicrosoftSqlStoreManager(dbServer, dbName, dbUser, dbPassword);
      SqlGraph sqlGraph = null;
      List<Uri> graphUris = msStore.GetGraphUris();
      FastRdfXmlWriter rdfXmlWriter = new FastRdfXmlWriter();

      if (graphUri == null || graphUri == string.Empty)
      {
          sqlGraph = new SqlGraph(graphUris[0], msStore);
      }
      else
      {
          bool exist = msStore.Exists(new Uri(graphUri));
          if (exist) 
              sqlGraph = new SqlGraph(new Uri(graphUri), msStore);
          else
              throw new Exception(graphUri + " does not exist in Sql store ...");
      }

      rdfXmlWriter.Save(sqlGraph, rdfFullFilename);

      if (graphUri == string.Empty) graphUri = "dotnetrf:default-graph";
      Console.WriteLine("Graph[" + graphUri + "] written to " + rdfFullFilename);
      Console.WriteLine("Press any key to continue....");
      Console.ReadKey();

    }

    private static void DoImport()
    {
      MicrosoftSqlStoreManager msStore = new MicrosoftSqlStoreManager(dbServer, dbName, dbUser, dbPassword);
      
      List<Uri> graphUris = msStore.GetGraphUris();
      SqlGraph sqlGraph = null;
        // let's load the existing graph
      if (graphUri == null || graphUri == string.Empty)
      {
         sqlGraph  = new SqlGraph(graphUris[0], msStore);
      }
      else
      {
          bool uriExist = msStore.Exists(new Uri(graphUri));
          if (uriExist)
          sqlGraph = new SqlGraph(new Uri(graphUri), msStore);
          else 
              throw new Exception(graphUri + " does not exists in Sql Store ... ");
      }
        // do we need to clear it first?
      if (clearBeforeImport)
      {
          sqlGraph.Clear();
      }
      Graph g = new Graph();
      FileLoader.Load(g, rdfFullFilename);
      foreach (Triple t in g.Triples)
      {
          sqlGraph.Assert(t);
      }

      sqlGraph.Manager.Flush();

      if (graphUri == string.Empty) graphUri = "dotnetrf:default-graph";
      Console.WriteLine("Graph[" + graphUri + "] imported from " + rdfFullFilename);
      Console.ReadKey();
    }

    static void PrintUsage()
    {
      Console.WriteLine("Usage:");
      Console.WriteLine("\\n\\RdfImportImport.exe Import DbServer DbName DbUser DbPassword RdfImportFullFilename GraphUri ClearBeforeImport\\n");
      Console.WriteLine("\\n\\RdfImportExport.exe Export DbServer DbName DbUser DbPassword RdfExportFullFilename GraphUri\\n");
      Console.ReadKey();
    }
  }
}
