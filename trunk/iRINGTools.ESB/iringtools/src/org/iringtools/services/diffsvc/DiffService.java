package org.iringtools.services.diffsvc;

import java.io.IOException;
import java.util.Hashtable;
import javax.servlet.ServletContext;
import javax.ws.rs.ConsumeMime;
import javax.ws.rs.GET;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.core.Context;
import javax.xml.bind.JAXBException;
import org.iringtools.adapter.library.dto.DataTransferIndices;
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
    
    // get receiver manifest
    String receiverManifestUrl = xdef.getReceiverUri() + "/" + xdef.getReceiverAppScope() + "/" + xdef.getReceiverAppName() + "/manifest";
    Manifest receiverManifest = NetUtil.get(Manifest.class, receiverManifestUrl);
    
    // create data exchange request
    DxRequest dxRequest = new DxRequest();
    dxRequest.setManifest(receiverManifest);
    dxRequest.setHashAlgorithm(xdef.getHashAlgorithm());
    
    // get sending DXIs
    String senderUrl = xdef.getSenderUri() + "/" + xdef.getSenderAppScope() + "/" + xdef.getSenderAppName() + "/" + xdef.getSenderGraphName() + "/dxi";
    DataTransferIndices sendingDxis = NetUtil.post(DataTransferIndices.class, senderUrl, dxRequest);
      
    // get receiving DXIs
    String receiverUrl = xdef.getReceiverUri() + "/" + xdef.getReceiverAppScope() + "/" + xdef.getReceiverAppName() + "/" + xdef.getReceiverGraphName() + "/dxi";    
    DataTransferIndices receivingDxis = NetUtil.post(DataTransferIndices.class, receiverUrl, dxRequest);
    
    return diff(sendingDxis, receivingDxis);
  }
  
  @POST
  @Path("/dxo/{exchangeId}")
  public DataTransferObjects dxo(@PathParam("exchangeId") String exchangeId, Identifiers identifiers) throws JAXBException, IOException
  { 
    // get exchange definition
    ExchangeDefinition xdef = getExchangeDefinition(exchangeId);
    
    // get receiver manifest
    String receiverManifestUrl = xdef.getReceiverUri() + "/" + xdef.getReceiverAppScope() + "/" + xdef.getReceiverAppName() + "/manifest";
    Manifest receiverManifest = NetUtil.get(Manifest.class, receiverManifestUrl);
    
    // create data exchange request
    DxRequest dxRequest = new DxRequest();
    dxRequest.setManifest(receiverManifest);
    dxRequest.setIdentifiers(identifiers);
    
    // get sending DTOs
    String senderUrl = xdef.getSenderUri() + "/" + xdef.getSenderAppScope() + "/" + xdef.getSenderAppName() + "/" + xdef.getSenderGraphName() + "/dto";
    DataTransferObjects sendingDtos = NetUtil.post(DataTransferObjects.class, senderUrl, dxRequest);
      
    // get receiving DTOs
    String receiverUrl = xdef.getReceiverUri() + "/" + xdef.getReceiverAppScope() + "/" + xdef.getReceiverAppName() + "/" + xdef.getReceiverGraphName() + "/dto";    
    DataTransferObjects receivingDtos = NetUtil.post(DataTransferObjects.class, receiverUrl, dxRequest);
    
    return diff(sendingDtos, receivingDtos);
  }
  
  //////////////////////////////////////////////////////////
    
  @POST
  @Path("/dxi")
  public DataTransferIndices diff(DataTransferIndices sendingDxis, DataTransferIndices receivingDxis)
  {
    DiffProvider diffProvider = new DiffProvider(settings);
    return diffProvider.diff(sendingDxis, receivingDxis);
  }
  
  @POST
  @Path("/dxo")
  public DataTransferObjects diff(DataTransferObjects sendingDtos, DataTransferObjects receivingDtos)
  {
    DiffProvider diffProvider = new DiffProvider(settings);
    return diffProvider.diff(sendingDtos, receivingDtos);
  }
  
  private ExchangeDefinition getExchangeDefinition(String exchangeId) throws JAXBException, IOException
  {
    String directoryServiceUri = context.getInitParameter("directoryServiceUri");
    String url = directoryServiceUri + "/exchanges/" + exchangeId;
    return NetUtil.get(ExchangeDefinition.class, url);
  }
}