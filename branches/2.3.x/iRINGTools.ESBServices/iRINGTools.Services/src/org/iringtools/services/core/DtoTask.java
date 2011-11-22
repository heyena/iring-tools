package org.iringtools.services.core;

import java.util.List;
import java.util.Map;

import org.apache.log4j.Logger;
import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.dxfr.dti.DataTransferIndexList;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.dxfr.request.DxoRequest;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpUtils;

public class DtoTask implements Runnable
{
  private static final Logger logger = Logger.getLogger(DtoTask.class);
  private HttpClient httpClient;
  private DxoRequest dxoRequest;
  private DataTransferObjects dtos;
  
  public DtoTask(final Map<String, Object> settings, final String url, final Manifest manifest,
      final List<DataTransferIndex> dtiItems)
  {
    httpClient = new HttpClient(url);
    HttpUtils.addOAuthHeaders(settings, httpClient); 
    
    DataTransferIndices indices = new DataTransferIndices();
    DataTransferIndexList dtiList = new DataTransferIndexList();
    dtiList.setItems(dtiItems);
    indices.setDataTransferIndexList(dtiList);
    
    dxoRequest = new DxoRequest();
    dxoRequest.setManifest(manifest);    
    dxoRequest.setDataTransferIndices(indices);
  }
  
  @Override
  public void run()
  {
    try 
    {
      dtos = httpClient.post(DataTransferObjects.class, dxoRequest);
    }
    catch (Exception e) 
    {
      logger.error("Error getting dxo: " + e.getMessage());
    }
  }
  
  public DataTransferObjects getDataTransferObjects()
  {
    return dtos;
  }  
}
