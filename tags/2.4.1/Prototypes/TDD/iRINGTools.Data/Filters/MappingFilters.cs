using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRINGTools.Data
{
  public static class MappingFilters
  {
    public static GraphMap GraphMapByName(this Mapping mapping, string graphName)
    {
      GraphMap graphMap = null;

      if (mapping.GraphMaps != null)
      {
        foreach (GraphMap graph in mapping.GraphMaps)
        {
          if (graph.Name.ToLower() == graphName.ToLower())
          {
            if (graph.ClassTemplateMaps.Count == 0)
              throw new Exception("Graph [" + graphName + "] is empty.");

            graphMap = graph;
          }
        }
      }

      return graphMap;
    }
  }
}
