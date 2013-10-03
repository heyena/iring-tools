﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using org.iringtools.mapping;
using org.iringtools.utility;
using System.Configuration;

namespace org.iringtools.utility.FillClassPaths
{
  public class Program
  {
    static void Main(string[] args)
    {
      string directory = ConfigurationManager.AppSettings["MappingDirectory"];

      if (string.IsNullOrWhiteSpace(directory))
      {
        if (args.Length <= 0)
        {
          Console.Write("Enter mapping directory: ");
          directory = Console.ReadLine();
        }
        else
        {
          directory = args[0];
        }
      }

      if (!Directory.Exists(directory))
      {
        Console.WriteLine("Directory [" + directory + "] not found.");
      }

      if (!directory.EndsWith("\\"))
      {
        directory += "\\";
      }

      string[] files = Directory.GetFiles(directory);

      foreach (string file in files)
      {
        if (file.StartsWith(directory + "Mapping."))
        {
          Console.Write("Processing [" + file.Substring(file.LastIndexOf("\\") + 1) + "]... ");
          
          Mapping mapping = null;
          try
          {
            mapping = Utility.Read<Mapping>(file);
          }
          catch (Exception e)
          {
            Console.WriteLine("invalid mapping: " + e.Message);
          }

          if (mapping != null && mapping.graphMaps != null)
          {
            foreach (GraphMap graphMap in mapping.graphMaps)
            {
              if (graphMap != null && graphMap.classTemplateMaps != null)
              {
                foreach (ClassTemplateMap ctm in graphMap.classTemplateMaps)
                {
                  if (ctm != null && ctm.classMap != null && ctm.templateMaps != null)
                  {
                    foreach (TemplateMap templateMap in ctm.templateMaps)
                    {
                      if (templateMap != null && templateMap.roleMaps != null)
                      {
                        foreach (RoleMap roleMap in templateMap.roleMaps)
                        {
                          if (roleMap.classMap != null)
                          {
                            string path = graphMap.BuildClassPath(ctm.classMap, templateMap, roleMap);
                            roleMap.classMap.path = path;

                            ClassMap relatedClassMap = GetClassMap(graphMap, roleMap.classMap.id, roleMap.classMap.index);

                            if (relatedClassMap != null)
                            {
                              relatedClassMap.path = path;
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            }

            Utility.Write<Mapping>(mapping, file);
            Console.WriteLine("succeeded.");
          }
        }
      }

      Console.Write("Press any key to continue...");
      Console.ReadKey();
    }

    static ClassMap GetClassMap(GraphMap graphMap, string classId, int classIndex)
    {
      foreach (ClassTemplateMap ctm in graphMap.classTemplateMaps)
      {
        if (ctm.classMap.id == classId && ctm.classMap.index == classIndex)
          return ctm.classMap;
      }

      return null;
    }
  }
}
