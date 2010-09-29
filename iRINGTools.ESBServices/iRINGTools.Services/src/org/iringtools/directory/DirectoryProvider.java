package org.iringtools.directory;

import java.io.FileNotFoundException;
import java.util.Hashtable;
import javax.xml.bind.JAXBException;
import org.iringtools.utility.JaxbUtil;

public class DirectoryProvider
{
  private Hashtable<String, String> settings;

  public DirectoryProvider(Hashtable<String, String> settings)
  {
    this.settings = settings;
  }

  public Directory getExchanges() throws FileNotFoundException, JAXBException 
  {
    String path = settings.get("baseDirectory") + "/WEB-INF/data/directory.xml";
    return JaxbUtil.read(Directory.class, path);    
  }

  public Exchange getExchange(String id) throws FileNotFoundException, JAXBException
  {
    String path = settings.get("baseDirectory") + "/WEB-INF/data/exchange-" + id + ".xml";
    return JaxbUtil.read(Exchange.class, path);
  }
}
