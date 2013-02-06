package org.iringtools.services.core;

import java.io.File;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Map;

import org.apache.log4j.Logger;
import org.iringtools.common.response.Response;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.history.History;
import org.iringtools.utility.IOUtils;
import org.iringtools.utility.JaxbUtils;

public class HistoryProvider
{
  private static final Logger logger = Logger.getLogger(HistoryProvider.class);
  private Map<String, Object> settings;

  public HistoryProvider(Map<String, Object> settings)
  {
    this.settings = settings;
  }

  public History getExchangeHistory(String scope, String id) throws ServiceProviderException
  {
	  try
	  {
		  return getExchangeHistory(scope, id, 0);
	  }
	  catch (Exception e)
	  {
		  String message = "Error getting exchange history: " + e;
		  logger.error(message);
		  throw new ServiceProviderException(message);
	  }
  }

  public History getExchangeHistory(String scope, String id, int limit) throws ServiceProviderException
  {
    History history = new History();
    List<ExchangeResponse> responses = new ArrayList<ExchangeResponse>();
    history.setExchangeResponses(responses);

    String path = settings.get("baseDirectory") + "/WEB-INF/exchanges/" + scope + "/" + id;

    try
    {
      File file = new File(path);
      
      if (file.exists())
      {      
        List<String> exchangeLogs = IOUtils.getFiles(path);
        
        // filter out pool logs
        for (int i = 0; i < exchangeLogs.size(); i++)
        {
          if (exchangeLogs.get(i).contains(ExchangeProvider.POOL_PREFIX))
          {
            exchangeLogs.remove(i--);
          }
        }
        
        // show most recent first
        Collections.sort(exchangeLogs);

        // may just want the x most recent history, where x=limit
        int actualLimit = limit;
        if (actualLimit > 0)
        {
        	// note we loop from biggest to smallest so actualLimit is an "inverted" value, also the array is zero based
        	// so if there were 10 logs and we only wanted the most recent one we'd want to loop while i >= 9
        	actualLimit = (limit > exchangeLogs.size() ? 0 : exchangeLogs.size() - limit);
        }
  
        for (int i = exchangeLogs.size() - 1; i >= actualLimit; i--)
        {
          ExchangeResponse response = JaxbUtils.read(ExchangeResponse.class, path + "/" + exchangeLogs.get(i));          
          responses.add(response);
        }
      }
      else
      {
        throw new Exception("No exchange history found.");
      }
    }
    catch (Exception e)
    {
      String message = "Error getting exchange history: " + e;
      logger.error(message);
      throw new ServiceProviderException(message);
    }

    return history;
  }

  public Response getExchangeResponse(String scope, String id, String timestamp, int start, int limit)
      throws ServiceProviderException
  {
    String path = settings.get("baseDirectory") + "/WEB-INF/exchanges/" + scope + "/" + id;
    String exchangeFile = path + "/" + timestamp.replace(":", ".") + ExchangeProvider.POOL_PREFIX + (start+1) + "-" + (start+limit) + ".xml";

    try
    {
      return JaxbUtils.read(Response.class, exchangeFile);
    }
    catch (Exception e)
    {
      String message = "Error getting exchange history of [" + scope + "." + id + "]: " + e;
      logger.error(message);
      throw new ServiceProviderException(message);
    }
  }
}
