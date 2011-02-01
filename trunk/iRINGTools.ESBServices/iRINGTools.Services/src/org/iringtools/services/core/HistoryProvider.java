package org.iringtools.services.core;

import java.io.IOException;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Hashtable;
import java.util.List;
import javax.xml.bind.JAXBException;
import javax.xml.datatype.XMLGregorianCalendar;

import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.history.History;
import org.iringtools.utility.IOUtil;
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
    History history = new History();
    List<ExchangeResponse> responses = new ArrayList<ExchangeResponse>();
    history.setResponses(responses);
    
    String path = settings.get("baseDirectory") + "/WEB-INF/logs/exchanges/" + scope + "/" + id;
    List<String> filesInFolder = IOUtil.getFiles(path);
    Collections.sort(filesInFolder);

    // show most recent first
    for (int i = filesInFolder.size() - 1; i >= 0 ; i--)
    {
      ExchangeResponse response = JaxbUtil.read(ExchangeResponse.class, path + "/" + filesInFolder.get(i));
      response.setStatusList(null);  // only interest in summary status
      responses.add(response);
    }
    
    return history;
  }
  
  public ExchangeResponse getExchangeResponse(String scope, String id, XMLGregorianCalendar timestamp) throws JAXBException, IOException 
  {
    String path = settings.get("baseDirectory") + "/WEB-INF/logs/exchanges/" + scope + "/" + id;
    
    // timestamp is the start time with ":" being replaced with "." to be able to save the file to windows file system,
    // thus reverse the process to get the file name
    String exchangeFile = path + "/" + timestamp.toString().replace(".", ":") + ".xml";
    
    return JaxbUtil.read(ExchangeResponse.class, exchangeFile);
  }
}
