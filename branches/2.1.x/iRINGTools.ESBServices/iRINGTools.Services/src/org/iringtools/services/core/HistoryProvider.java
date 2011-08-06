package org.iringtools.services.core;

import java.io.IOException;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Map;

import javax.xml.bind.JAXBException;

import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.history.History;
import org.iringtools.utility.IOUtils;
import org.iringtools.utility.JaxbUtils;

public class HistoryProvider
{
  private Map<String, Object> settings;

  public HistoryProvider(Map<String, Object> settings)
  {
    this.settings = settings;
  }

  public History getExchangeHistory(String scope, String id) throws JAXBException, IOException
  {
    History history = new History();
    List<ExchangeResponse> responses = new ArrayList<ExchangeResponse>();
    history.setResponses(responses);
    
    String path = settings.get("baseDirectory") + "/WEB-INF/exchanges/" + scope + "/" + id;
    List<String> filesInFolder = IOUtils.getFiles(path);
    Collections.sort(filesInFolder);

    // show most recent first
    for (int i = filesInFolder.size() - 1; i >= 0 ; i--)
    {
      ExchangeResponse response = JaxbUtils.read(ExchangeResponse.class, path + "/" + filesInFolder.get(i));
      response.setStatusList(null);  // only interest in summary status
      responses.add(response);
    }
    
    return history;
  }
  
  public ExchangeResponse getExchangeResponse(String scope, String id, String timestamp) throws JAXBException, IOException 
  {
    String path = settings.get("baseDirectory") + "/WEB-INF/exchanges/" + scope + "/" + id;
    String exchangeFile = path + "/" + timestamp.replace(":", ".") + ".xml";
    
    return JaxbUtils.read(ExchangeResponse.class, exchangeFile);
  }
}
