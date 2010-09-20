package org.iringtools.services.diffsvc;

import java.io.IOException;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Hashtable;
import java.util.List;

import javax.servlet.ServletContext;
import javax.ws.rs.ConsumeMime;
import javax.ws.rs.GET;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.core.Context;
import javax.xml.bind.JAXBException;
import org.iringtools.adapter.library.dto.ClassObject;
import org.iringtools.adapter.library.dto.DataTransferIndices;
import org.iringtools.adapter.library.dto.DataTransferObject;
import org.iringtools.adapter.library.dto.TransferType;
import org.iringtools.adapter.library.dto.DataTransferIndices.DataTransferIndex;
import org.iringtools.adapter.library.dto.DataTransferObjects;
import org.iringtools.adapter.library.manifest.Manifest;
import org.iringtools.exchange.library.directory.ExchangeDefinition;
import org.iringtools.library.DxRequest;
import org.iringtools.library.Identifiers;
import org.iringtools.utility.NetUtil;

@Path("/")
@Produces("application/xml")
@ConsumeMime("application/xml")
public class DiffService
{
  @Context 
  private ServletContext context;
  private Hashtable<String, String> settings;
  
  public DiffService()
  {
    settings = new Hashtable<String, String>();
  }
  
  //////////////////////////////////////////////////////////
  // Temporary Services
  //////////////////////////////////////////////////////////
  
  @GET
  @Path("/dxi/{exchangeId}")
  public DataTransferIndices dxi(@PathParam("exchangeId") String exchangeId) throws JAXBException, IOException
  {   
    // get exchange definition
    ExchangeDefinition xdef = getExchangeDefinition(exchangeId);    
    
    // get target manifest
    String targetManifestUrl = xdef.getTargetUri() + "/" + xdef.getTargetAppScope() + "/" + xdef.getTargetAppName() + "/manifest";
    Manifest targetManifest = NetUtil.get(Manifest.class, targetManifestUrl);
    
    // create data exchange request
    DxRequest dxRequest = new DxRequest();
    dxRequest.setManifest(targetManifest);
    dxRequest.setHashAlgorithm(xdef.getHashAlgorithm());
    
    // get source DXIs
    String sourceUrl = xdef.getSourceUri() + "/" + xdef.getSourceAppScope() + "/" + xdef.getSourceAppName() + "/" + xdef.getSourceGraphName() + "/dxi";
    DataTransferIndices sourceDxis = NetUtil.post(DataTransferIndices.class, sourceUrl, dxRequest);
      
    // get target DXIs
    String targetUrl = xdef.getTargetUri() + "/" + xdef.getTargetAppScope() + "/" + xdef.getTargetAppName() + "/" + xdef.getTargetGraphName() + "/dxi";    
    DataTransferIndices targetDxis = NetUtil.post(DataTransferIndices.class, targetUrl, dxRequest);
    
    return diff(sourceDxis, targetDxis);
  }
  
  @POST
  @Path("/dxo/{exchangeId}")
  public DataTransferObjects dxo(@PathParam("exchangeId") String exchangeId, DataTransferIndices dxis) throws JAXBException, IOException
  {
    DataTransferObjects resultDtos = new DataTransferObjects();
    List<DataTransferObject> resultDtoList = resultDtos.getDataTransferObject();
    
    // get exchange definition
    ExchangeDefinition xdef = getExchangeDefinition(exchangeId);
    
    // store add/change-sync/delete identifiers in different lists
    List<String> addIdentifierList = new ArrayList<String>();
    List<String> changeIdentifierList = new ArrayList<String>();
    List<String> deleteIdentifierList = new ArrayList<String>();
    
    for (DataTransferIndex dxi : dxis.getDataTransferIndex())
    {
      String identifier = dxi.getIdentifier();
      
      switch (dxi.getTransferType())
      {
      case ADD:
        addIdentifierList.add(identifier);
        break;
      case DELETE:
        deleteIdentifierList.add(identifier);
        break;
      default: // treat sync DTO as change DTO in case it has changed since DXI differencing occurred        
        changeIdentifierList.add(identifier);  
        break;
      }
    }
    
    // get target manifest
    String targetManifestUrl = xdef.getTargetUri() + "/" + xdef.getTargetAppScope() + "/" + xdef.getTargetAppName() + "/manifest";
    Manifest targetManifest = NetUtil.get(Manifest.class, targetManifestUrl);
    
    // create source DTO request
    DxRequest sourceDtosRequest = new DxRequest();
    sourceDtosRequest.setManifest(targetManifest);
    Identifiers sourceIdentifiers = new Identifiers();
    sourceIdentifiers.getIdentifier().addAll(addIdentifierList);
    sourceIdentifiers.getIdentifier().addAll(changeIdentifierList);
    
    // create target DTO request
    DxRequest targetDtosRequest = new DxRequest();
    targetDtosRequest.setManifest(targetManifest);
    Identifiers targetIdentifiers = new Identifiers();
    targetIdentifiers.getIdentifier().addAll(changeIdentifierList);
    targetIdentifiers.getIdentifier().addAll(deleteIdentifierList);
    
    // get source add/change-sync DTOs
    String sourceUrl = xdef.getSourceUri() + "/" + xdef.getSourceAppScope() + "/" + xdef.getSourceAppName() + "/" + xdef.getSourceGraphName() + "/dto";
    DataTransferObjects sourceDtos = NetUtil.post(DataTransferObjects.class, sourceUrl, sourceDtosRequest);
    List<DataTransferObject> sourceDtoList = sourceDtos.getDataTransferObject();
      
    // add new DTOs to resultDtoList and remove them from the list so only change-sync ones stay in the source list
    for (int i = 0; i < sourceDtoList.size(); i++)
    {
      DataTransferObject sourceDto = sourceDtoList.get(i);
      List<ClassObject> sourceClassObjectList = sourceDto.getClassObjects().getClassObject();
      
      if (sourceClassObjectList.size() > 0)
      {
        for (String addIdentifier : addIdentifierList)
        {
          if (sourceClassObjectList.get(0).getIdentifier().equalsIgnoreCase(addIdentifier))
          {
            sourceDto.setTransferType(TransferType.ADD);
            DataTransferObject newDto = (DataTransferObject)sourceDto.clone();
            resultDtoList.add(newDto);
            
            sourceDtoList.remove(i--);
            addIdentifierList.remove(addIdentifier);
            break;
          }
        }
      }
    }
    
    // get target change-sync/delete DTOs
    String targetUrl = xdef.getTargetUri() + "/" + xdef.getTargetAppScope() + "/" + xdef.getTargetAppName() + "/" + xdef.getTargetGraphName() + "/dto";    
    DataTransferObjects targetDtos = NetUtil.post(DataTransferObjects.class, targetUrl, targetDtosRequest);
    List<DataTransferObject> targetDtoList = targetDtos.getDataTransferObject();
    
    // add deleted DTOs to resultDtoList and remove them from the list so only change-sync ones stay in the target list
    for (int i = 0; i < targetDtoList.size(); i++)
    {
      DataTransferObject targetDto = targetDtoList.get(i);
      List<ClassObject> targetClassObjectList = targetDto.getClassObjects().getClassObject();
      
      if (targetClassObjectList.size() > 0)
      {
        for (String deleteIdentifier : deleteIdentifierList)
        {
          if (targetClassObjectList.get(0).getIdentifier().equalsIgnoreCase(deleteIdentifier))
          {
            targetDto.setTransferType(TransferType.DELETE);
            DataTransferObject newDto = (DataTransferObject)targetDto.clone();
            resultDtoList.add(newDto);
            
            targetDtoList.remove(i--);
            deleteIdentifierList.remove(deleteIdentifier);
            break;
          }
        }
      }
    }
    
    // add change-sync DTOs to resultDtoList
    DataTransferObjects diffDtos = diff(sourceDtos, targetDtos);    
    resultDtoList.addAll(diffDtos.getDataTransferObject());
    Collections.sort(resultDtoList);
    
    return resultDtos;
  }
  
  private ExchangeDefinition getExchangeDefinition(String exchangeId) throws JAXBException, IOException
  {
    String directoryServiceUri = context.getInitParameter("directoryServiceUri");
    String url = directoryServiceUri + "/exchanges/" + exchangeId;
    return NetUtil.get(ExchangeDefinition.class, url);
  }
  
  //////////////////////////////////////////////////////////
    
  @POST
  @Path("/dxi")
  public DataTransferIndices diff(DataTransferIndices sourceDxis, DataTransferIndices targetDxis)
  {
    DiffProvider diffProvider = new DiffProvider(settings);
    return diffProvider.diff(sourceDxis, targetDxis);
  }
  
  @POST
  @Path("/dxo")
  public DataTransferObjects diff(DataTransferObjects sourceDtos, DataTransferObjects targetDtos)
  {
    DiffProvider diffProvider = new DiffProvider(settings);
    return diffProvider.diff(sourceDtos, targetDtos);
  }
  
  //TODO: add another service that receives DXIs + DXOs + filter from PHP(client). It
  //fills missing DXOs from DXI list, apply filter, then send it to exchange service
}