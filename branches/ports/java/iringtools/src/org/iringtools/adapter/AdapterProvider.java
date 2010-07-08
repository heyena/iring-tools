package org.iringtools.adapter;

import java.util.List;
import javax.servlet.ServletContext;
import org.apache.axiom.om.OMElement;
import org.iringtools.adapter.library.DiffEngine;
import org.iringtools.adapter.library.dto.DataTransferObjects;
import org.iringtools.adapter.library.manifest.GraphMap;
import org.iringtools.adapter.library.manifest.Manifest;
import org.iringtools.utility.IOUtil;
import org.iringtools.utility.JaxbUtil;
import org.apache.log4j.Logger;

public class AdapterProvider
{
  private static final Logger logger = Logger.getLogger(AdapterProvider.class);
  
  private ServletContext context;
  private Manifest manifest;
  
  public AdapterProvider(ServletContext context)
  {
    this.context = context;
  }
  
  private GraphMap getGraphMap(String graphName)
  {
    List<GraphMap> graphMaps = manifest.getGraphMaps().getGraphMap();
    
    for (GraphMap graphMap : graphMaps)
    {
      if (graphMap.getName().equalsIgnoreCase(graphName))
      {
        return graphMap;
      }
    }
    
    return null;
  }
  
  public String diff(String graphName)
  {
    String result = null;
        
    String path = context.getRealPath("/WEB-INF/data/"); 
    String projName = context.getAttribute("projName").toString();
    String appName = context.getAttribute("appName").toString();
    String scope = projName + "." + appName;;
    
    String manifestPath = path + "/Manifest." + scope + ".xml";    
    String sendingRdfPath = path + "/Rdf." + scope + ".Send.xml";    
    String receivingRdfPath = path + "/Rdf." + scope + ".Receive.xml";
    
    try
    {
      manifest = JaxbUtil.read(Manifest.class, manifestPath);
      GraphMap graphMap = getGraphMap(graphName);
      
      if (graphMap != null)
      {
        OMElement sendingRdf = IOUtil.readXml(sendingRdfPath);
        OMElement receivingRdf = IOUtil.readXml(receivingRdfPath);    
        
        DiffEngine diffEngine = new DiffEngine(graphMap);
        DataTransferObjects resultDtoList = diffEngine.diff(sendingRdf, receivingRdf);
        result = JaxbUtil.serialize(resultDtoList);
      }
    }
    catch (Exception ex)
    {
      logger.error(ex);
      return "<error>" + ex + "</error>";
    }
    
    return result;
  }
}
