package org.iringtools.services.core;

import java.io.IOException;
import java.util.Hashtable;
import javax.xml.bind.JAXBException;
import org.iringtools.directory.Directory;
import org.iringtools.history.History;
import org.iringtools.utility.JaxbUtil;

public class HistoryProvider
{
  private Hashtable<String, String> settings;

  public HistoryProvider(Hashtable<String, String> settings)
  {
    this.settings = settings;
  }
  
  public History getExchangeHistory(String scope, String id) throws JAXBException, IOException
  {
    //Special Directory: /WEB-INF/logs/{scope}/exchanges/{id}
    //Go into ESBService and save Responses as {datetimestamp}.xml in special directory 
    
    //go to special directory and get a list of all of the xml files
    //load xml files into Hisotry xml
    //return History xml
      
    return null;
  }
}
