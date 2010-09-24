package org.iringtools.exchange;

import java.io.FileNotFoundException;
import java.util.Hashtable;
import javax.xml.bind.JAXBException;
import org.iringtools.directory.Directory;
import org.iringtools.directory.ExchangeDefinition;
import org.iringtools.utility.JaxbUtil;

public class DirectoryProvider
{
  private Hashtable<String, String> settings;

  public DirectoryProvider(Hashtable<String, String> settings)
  {
    this.settings = settings;
  }

  public Directory getExchangeList() throws FileNotFoundException, JAXBException 
  {
    String path = settings.get("baseDirectory") + "/WEB-INF/data/directory.xml";
    return JaxbUtil.read(Directory.class, path);    
  }

  public ExchangeDefinition getExchangeDefinition(String id) throws FileNotFoundException, JAXBException
  {
    String path = settings.get("baseDirectory") + "/WEB-INF/data/exchange-definition-" + id + ".xml";
    return JaxbUtil.read(ExchangeDefinition.class, path);
  }
}
