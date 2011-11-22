package org.iringtools.services.core;

import java.util.Map;

import org.apache.log4j.Logger;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpUtils;

public class ManifestTask implements Runnable
{
  private static final Logger logger = Logger.getLogger(ManifestTask.class);
  private HttpClient httpClient;
  private Manifest manifest;
  
  public ManifestTask(final Map<String, Object> settings, final String url)
  {
    this.httpClient = new HttpClient(url);
    HttpUtils.addOAuthHeaders(settings, httpClient);
  }
  
  @Override
  public void run()
  {
    try 
    {
      manifest = httpClient.get(Manifest.class);
    }
    catch (Exception e) 
    {
      logger.error("Error getting manifest: " + e.getMessage());
    }
  }
  
  public Manifest getManifest()
  {
    return manifest;
  }  
}
