package org.iringtools.services.core;

import java.io.IOException;
import java.util.Hashtable;
import javax.xml.bind.JAXBException;
import org.iringtools.directory.Directory;
import org.iringtools.directory.ExchangeDefinition;
import org.iringtools.utility.JaxbUtils;

public class DirectoryProvider
{
  private Hashtable<String, String> settings;

  public DirectoryProvider(Hashtable<String, String> settings)
  {
    this.settings = settings;
  }

  public Directory getExchanges() throws JAXBException, IOException 
  {
    String path = settings.get("baseDirectory") + "/WEB-INF/data/directory.xml";
    return JaxbUtils.read(Directory.class, path);    
  }

  public ExchangeDefinition getExchangeDefinition(String scope, String id) throws JAXBException, IOException
  {
    String path = settings.get("baseDirectory") + "/WEB-INF/data/exchange-" + scope + "-" + id + ".xml";
    return JaxbUtils.read(ExchangeDefinition.class, path);
  }
}
