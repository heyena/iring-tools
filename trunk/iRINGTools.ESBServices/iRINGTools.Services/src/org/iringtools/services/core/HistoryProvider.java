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
    //Special Directory: /WEB-INF/logs/{scope}/exchanges/{id}
    //Go into ESBService and save Responses as {datetimestamp}.xml in special directory 
    
    //go to special directory and get a list of all of the xml files
    //load xml files into Hisotry xml
    //return History xml
	String path = settings.get("baseDirectory") + "/WEB-INF/logs/" + scope + "/exchanges/" + id;
	
	History history = new History();
	List<ExchangeResponse> responses = new ArrayList<ExchangeResponse>();
	ExchangeResponse response = null;
	
	List<String> filesInFolder = IOUtil.getFiles(path);
	
	for (int i = 0; i < filesInFolder.size(); i++) 
    {
    	response = JaxbUtil.read(ExchangeResponse.class, path+ "\\" +filesInFolder.get(i));
    	responses.add(response);
    }
    
    history.setResponses(responses);
    
	return history;
  }
}
