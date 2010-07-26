package org.iringtools.adapter;

import java.util.Date;
import java.util.Hashtable;
import java.util.List;
import java.util.Properties;
import org.iringtools.adapter.library.DiffEngine;
import org.iringtools.adapter.library.dto.DataTransferObjects;
import org.iringtools.adapter.library.manifest.Graph;
import org.iringtools.adapter.library.manifest.Manifest;
import org.iringtools.utility.IOUtil;
import org.iringtools.utility.JaxbUtil;
import org.apache.log4j.Logger;
import com.google.inject.Guice;
import com.google.inject.Injector;
import com.google.inject.Key;
import com.google.inject.name.Names;

public class ExchangeProvider
{
  private static final Logger logger = Logger.getLogger(ExchangeProvider.class);
  private Hashtable<String, String> settings;
  private Manifest manifest;

  public ExchangeProvider(Hashtable<String, String> settings)
  {
    this.settings = settings;
  }

  private Graph getGraphMap(String graphName)
  {
    List<Graph> graphMaps = manifest.getGraphs().getGraph();

    for (Graph graphMap : graphMaps)
    {
      if (graphMap.getName().equalsIgnoreCase(graphName))
      {
        return graphMap;
      }
    }

    return null;
  }

  public String diff(String graphName, String format)
  {
    String result = null;

    String dataPath = settings.get("baseDirectory") + "/WEB-INF/data/";
    String projName = settings.get("projName");
    String appName = settings.get("appName");
    String scope = projName + "." + appName;

    String manifestPath = dataPath + "/Manifest." + scope + ".xml";
    String sendingXmlPath = null;
    String receivingXmlPath = null;
    
    if (format.equalsIgnoreCase("dto"))
    {
      sendingXmlPath = dataPath + "/Dto." + scope + ".Send-3.xml";
      receivingXmlPath = dataPath + "/Dto." + scope + ".Receive-3.xml";
    }
    else
    {
      sendingXmlPath = dataPath + "/Rdf." + scope + ".Send-3.xml";
      receivingXmlPath = dataPath + "/Rdf." + scope + ".Receive-3.xml";
    }
    
    try
    {
      manifest = JaxbUtil.read(Manifest.class, manifestPath);
      Graph graphMap = getGraphMap(graphName);

      if (graphMap != null)
      {
        String sendingXml = IOUtil.readString(sendingXmlPath);
        String receivingXml = IOUtil.readString(receivingXmlPath);

        Date start = new Date();
        Properties properties = IOUtil.loadProperties(dataPath + "binding.properties");    
        ExchangeModule exchangeModule = new ExchangeModule(properties);
        Injector injector = Guice.createInjector(exchangeModule);
        DiffEngine diffEngine = injector.getInstance(Key.get(DiffEngine.class, Names.named(format)));
        DataTransferObjects resultDtos = diffEngine.diff(graphMap, sendingXml, receivingXml);
        result = JaxbUtil.serialize(resultDtos);
        
        Date end = new Date();
        long duration = end.getTime() - start.getTime();
        long remainingSeconds = duration % (60 * 1000);
        logger.info(String.format("Execution time [%d:%d.%d] minutes.", duration / (60 * 1000),
            remainingSeconds / 1000, remainingSeconds % 1000));
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
