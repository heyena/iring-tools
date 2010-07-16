package org.iringtools.adapter;

import java.util.Date;
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

public class ExchangeProvider
{
  private static final Logger logger = Logger.getLogger(ExchangeProvider.class);
  
  private ServletContext context;
  private Manifest manifest;
  
  public ExchangeProvider(ServletContext context)
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
  
  public String diffRdf(String graphName)
  {
    String result = null;
        
    String path = context.getRealPath("/WEB-INF/data/");
    String projName = context.getAttribute("projName").toString();
    String appName = context.getAttribute("appName").toString();
    String scope = projName + "." + appName;
    
    String manifestPath = path + "/Manifest." + scope + ".xml";
    String sendingRdfPath = path + "/Rdf." + scope + ".Send-3.xml";
    String receivingRdfPath = path + "/Rdf." + scope + ".Receive-3.xml";
    
    try
    {
      manifest = JaxbUtil.read(Manifest.class, manifestPath);
      GraphMap graphMap = getGraphMap(graphName);
      
      if (graphMap != null)
      {
        OMElement sendingRdf = IOUtil.readXml(sendingRdfPath);
        OMElement receivingRdf = IOUtil.readXml(receivingRdfPath); 
        
        Date start = new Date();
        DiffEngine diffEngine = new DiffEngine(graphMap);
        DataTransferObjects resultDtoList = diffEngine.diffRdf(sendingRdf, receivingRdf);
        result = JaxbUtil.serialize(resultDtoList); 
        Date end = new Date();
        long duration = end.getTime() - start.getTime();
        long remainingSeconds = duration % (60 * 1000);
        logger.info(String.format("Execution time [%d:%d.%d] minutes.",
            duration/(60 * 1000), remainingSeconds/1000, remainingSeconds % 1000));
      }
    }
    catch (Exception ex)
    {
      logger.error(ex);
      return "<error>" + ex + "</error>";
    }
    
    return result;
  }
  
  public String diffDto(String graphName)
  {
    String result = null;
    
    try
    {
      String path = context.getRealPath("/WEB-INF/data/");
      String projName = context.getAttribute("projName").toString();
      String appName = context.getAttribute("appName").toString();
      String scope = projName + "." + appName;
      
      String manifestPath = path + "/Manifest." + scope + ".xml";
      String sendingDtoPath = path + "/Dto." + scope + ".Send-3.xml";
      String receivingDtoPath = path + "/Dto." + scope + ".Receive-3.xml";
      
      manifest = JaxbUtil.read(Manifest.class, manifestPath);
      GraphMap graphMap = getGraphMap(graphName);
      
      if (graphMap != null)
      {
        DataTransferObjects sendingDtos = JaxbUtil.read(DataTransferObjects.class, sendingDtoPath);
        DataTransferObjects receivingDtos = JaxbUtil.read(DataTransferObjects.class, receivingDtoPath);
        
        Date start = new Date();
        DiffEngine diffEngine = new DiffEngine(graphMap);
        DataTransferObjects resultDtos = diffEngine.diffDto(sendingDtos, receivingDtos);
        result = JaxbUtil.serialize(resultDtos); 
        Date end = new Date();
        long duration = end.getTime() - start.getTime();
        long remainingSeconds = duration % (60 * 1000);
        logger.info(String.format("Execution time [%d:%d.%d] minutes.",
            duration/(60 * 1000), remainingSeconds/1000, remainingSeconds % 1000));
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
