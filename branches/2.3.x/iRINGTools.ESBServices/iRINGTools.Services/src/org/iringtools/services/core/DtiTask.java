package org.iringtools.services.core;

import java.util.Map;

import org.apache.log4j.Logger;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpUtils;

public class DtiTask implements Runnable
{
  private static final Logger logger = Logger.getLogger(DtiTask.class);
  private HttpClient httpClient;
  private Manifest manifest;
  private DataTransferIndices indices;
  
  public DtiTask(final Map<String, Object> settings, final String url, final Manifest manifest)
  {
    this.httpClient = new HttpClient(url);
    HttpUtils.addOAuthHeaders(settings, httpClient); 
    
    this.manifest = manifest;
  }
  
  @Override
  public void run()
  {
    try 
    {
      indices = httpClient.post(DataTransferIndices.class, manifest);
    }
    catch (Exception e) 
    {
      logger.error("Error getting dxi: " + e.getMessage());
    }
  }
  
  public DataTransferIndices getDataTransferIndices()
  {
    return indices;
  }  
}
