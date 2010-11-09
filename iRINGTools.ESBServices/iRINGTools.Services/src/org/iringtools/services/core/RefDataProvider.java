package org.iringtools.services.core;

import java.io.FileNotFoundException;
import java.io.IOException;
import java.util.Hashtable;
import javax.xml.bind.JAXBException;

import org.iringtools.directory.ExchangeDefinition;
import org.iringtools.sparql.Query;
import org.iringtools.utility.JaxbUtil;

public class RefDataProvider
{
  private Hashtable<String, String> settings;

  public RefDataProvider(Hashtable<String, String> settings)
  {
    this.settings = settings;
  }

  public Query getQueryFileName() throws JAXBException, IOException, FileNotFoundException
  {
    String path = settings.get("baseDirectory") + "/WEB-INF/data/Queries.xml";
    return JaxbUtil.read(Query.class, path);    
  }

  public ExchangeDefinition getRepository() throws JAXBException, IOException, FileNotFoundException
  {
    String path = settings.get("baseDirectory") + "/WEB-INF/data/Repositories.xml";
    return JaxbUtil.read(ExchangeDefinition.class, path);
  }
}
