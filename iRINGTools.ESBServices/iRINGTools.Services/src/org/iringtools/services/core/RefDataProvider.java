package org.iringtools.services.core;

import java.io.FileNotFoundException;
import java.io.IOException;
import java.util.Hashtable;
import javax.xml.bind.JAXBException;

import org.iringtools.directory.ExchangeDefinition;
import org.iringtools.refdata.queries.Queries;
import org.iringtools.refdata.queries.Query;
import org.iringtools.utility.JaxbUtil;

public class RefDataProvider
{
  private Hashtable<String, String> settings;

  public RefDataProvider(Hashtable<String, String> settings)
  {
    this.settings = settings;
  }

  public Queries getQueryFileName() throws JAXBException, IOException, FileNotFoundException
  {
    //String path = settings.get("baseDirectory") + "/WEB-INF/data/Queries.xml";
	  String path = "Queries.xml";
	  System.out.println(path);
    return JaxbUtil.read(Queries.class, path);    
  }

  public ExchangeDefinition getRepository() throws JAXBException, IOException, FileNotFoundException
  {
    String path = settings.get("baseDirectory") + "/WEB-INF/data/Repositories.xml";
    return JaxbUtil.read(ExchangeDefinition.class, path);
  }
}
