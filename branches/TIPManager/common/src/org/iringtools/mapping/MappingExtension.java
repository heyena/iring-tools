package org.iringtools.mapping;

public class MappingExtension extends Mapping {
	
	Mapping mapping = null;
	
	public MappingExtension(Mapping mapping){
		this.mapping = mapping;
	}
	
    public GraphMap findGraphMap(String graphName)
    {
      GraphMap graphMap = null;

      for (GraphMap graph : mapping.getGraphMaps().getItems())
      {
        if (graph.getName().equalsIgnoreCase(graphName))
        {
          if (graph.getClassTemplateMaps().getItems().size() == 0)
            throw new RuntimeException("Graph [" + graphName + "] is empty.");

          graphMap = graph;
        }
      }

      return graphMap;
    }

}
