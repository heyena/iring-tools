package org.iringtools.services.core;

import java.util.Map;

import org.apache.log4j.Logger;
import org.iringtools.directory.Directory;
import org.iringtools.directory.ExchangeDefinition;
import org.iringtools.utility.JaxbUtils;

public class DirectoryProvider
{
  private static final Logger logger = Logger.getLogger(DirectoryProvider.class);
  private Map<String, Object> settings;

  public DirectoryProvider(Map<String, Object> settings)
  {
    this.settings = settings;
  }

  public Directory getExchanges() throws ServiceProviderException
  {
    String path = settings.get("baseDirectory") + "/WEB-INF/data/directory.xml";
    
    try
    {
      return JaxbUtils.read(Directory.class, path);
    }
    catch (Exception e)
    {
      String message = "Error getting exchange definitions: " + e;
      logger.error(message);
      throw new ServiceProviderException(message);
    }
  }

  public ExchangeDefinition getExchangeDefinition(String scope, String id) throws ServiceProviderException
  {
    String path = settings.get("baseDirectory") + "/WEB-INF/data/exchange-" + scope + "-" + id + ".xml";
    try
    {
      return JaxbUtils.read(ExchangeDefinition.class, path);
    }
    catch (Exception e)
    {
      String message = "Error exchange definition of [" + scope + "." + id + "]: " + e;
      logger.error(message);
      throw new ServiceProviderException(message);
    }
  }
}
