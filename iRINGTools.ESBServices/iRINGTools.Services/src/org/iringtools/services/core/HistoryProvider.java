package org.iringtools.services.core;

import java.io.IOException;
import java.util.ArrayList;
import java.util.Hashtable;
import java.util.List;
import javax.xml.bind.JAXBException;

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

    for (int i = 0; i < filesInFolder.size(); i++)
    {
      ExchangeResponse response = JaxbUtil.read(ExchangeResponse.class, path + "\\" + filesInFolder.get(i));
      responses.add(response);
    }
    
    return history;
  }
}
