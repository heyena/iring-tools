package org.iringtools.services.dirsvc;

import java.util.Hashtable;
import org.apache.log4j.Logger;
import org.iringtools.exchange.library.directory.Directory;
import org.iringtools.exchange.library.directory.ExchangeDefinition;
import org.iringtools.utility.JaxbUtil;

public class DirectoryProvider
{
  private static final Logger logger = Logger.getLogger(DirectoryProvider.class);
  private Hashtable<String, String> settings;

  public DirectoryProvider(Hashtable<String, String> settings)
  {
    this.settings = settings;
  }

  public Directory getExchangeList() 
  {
    try 
    {
      String path = settings.get("baseDir") + "/WEB-INF/data/directory.xml";
      return JaxbUtil.read(Directory.class, path);
    }
    catch (Exception ex)
    {
      logger.error(ex);
      return null;
    }
  }

  public ExchangeDefinition getExchangeDefinition(String id)
  {
    try
    {
      String path = settings.get("baseDir") + "/WEB-INF/data/exchange-definition-" + id + ".xml";
      return JaxbUtil.read(ExchangeDefinition.class, path);
    }
    catch (Exception ex)
    {
      logger.error(ex);
      return null;
    }
  }
}
