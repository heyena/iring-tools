package org.iringtools.services.diffsvc;

import java.io.IOException;
import java.util.Hashtable;
import javax.servlet.ServletContext;
import javax.ws.rs.ConsumeMime;
import javax.ws.rs.GET;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
//import javax.ws.rs.PathParam;
import javax.ws.rs.QueryParam;
import javax.ws.rs.Produces;
import javax.ws.rs.core.Context;
import javax.xml.bind.JAXBException;
import org.iringtools.adapter.library.dto.DataTransferIndices;
import org.iringtools.adapter.library.dto.DataTransferObjects;
import org.iringtools.adapter.library.manifest.Manifest;
import org.iringtools.library.DxRequest;
import org.iringtools.library.Identifiers;
import org.iringtools.utility.JaxbUtil;
import org.iringtools.utility.NetUtil;

@Path("/")
@Produces("application/xml")
@ConsumeMime("application/xml")
//TODO: check null list and handle error conditions
//TODO: read settings from web.xml
public class DiffService
{
  @Context 
  private ServletContext context;
  private Hashtable<String, String> settings;
  
  public DiffService()
  {
    settings = new Hashtable<String, String>();
  }
    
  //////////////////////
  // temporary service
  /////////////////////
  @GET
  @Path("/dxi")
  public String diffDxi(@QueryParam("senderUri") String senderUri,
                     @QueryParam("senderProj") String senderProj,
                     @QueryParam("senderApp") String senderApp,
                     @QueryParam("senderGraph") String senderGraph,
                     @QueryParam("receiverUri") String receiverUri,                        
                     @QueryParam("receiverProj") String receiverProj,
                     @QueryParam("receiverApp") String receiverApp,
                     @QueryParam("receiverGraph") String receiverGraph,
                     @QueryParam("hashAlgorithm") String hashAlgorithm) throws JAXBException, IOException
  {
    // get receiver manifest
    String receiverManifestUrl = receiverUri + "/AdapterService/" + receiverProj + "/" + receiverApp + "/manifest";
    Manifest receiverManifest = NetUtil.get(Manifest.class, receiverManifestUrl);
    
    // create data exchange request
    DxRequest dxRequest = new DxRequest();
    dxRequest.setManifest(receiverManifest);
    dxRequest.setHashAlgorithm(hashAlgorithm);
    
    // get sending DXIs
    String senderUrl = senderUri + "/ExchangeService/" + senderProj + "/" + senderApp + "/" + senderGraph + "/dxi";
    DataTransferIndices sendingDxis = NetUtil.post(DataTransferIndices.class, senderUrl, dxRequest);
      
    // get receiving DXIs
    String receiverUrl = receiverUri + "/ExchangeService/" + receiverProj + "/" + receiverApp + "/" + receiverGraph + "/dxi";    
    DataTransferIndices receivingDxis = NetUtil.post(DataTransferIndices.class, receiverUrl, dxRequest);
    
    return diff(sendingDxis, receivingDxis);
  }
  
  //////////////////////
  // temporary service
  /////////////////////
  @GET
  @Path("/dxo")
  //TODO: change to @POST due to # of characters limit of url request
  //TODO: accept exchange URI and adapter URI param
  public String diffDxo(@QueryParam("senderUri") String senderUri,
                     @QueryParam("senderProj") String senderProj,
                     @QueryParam("senderApp") String senderApp,
                     @QueryParam("senderGraph") String senderGraph,
                     @QueryParam("receiverUri") String receiverUri,                        
                     @QueryParam("receiverProj") String receiverProj,
                     @QueryParam("receiverApp") String receiverApp,
                     @QueryParam("receiverGraph") String receiverGraph,
                     @QueryParam("identifiers") String identifiers) throws JAXBException, IOException
  {   
    Identifiers identifiersObj = JaxbUtil.toObject(Identifiers.class, identifiers);
    
    // get receiver manifest
    String receiverManifestUrl = receiverUri + "/AdapterService/" + receiverProj + "/" + receiverApp + "/manifest";
    Manifest receiverManifest = NetUtil.get(Manifest.class, receiverManifestUrl);
    
    // create data exchange request
    DxRequest dxRequest = new DxRequest();
    dxRequest.setManifest(receiverManifest);
    dxRequest.setIdentifiers(identifiersObj);
    
    // get sending DTOs
    String senderUrl = senderUri + "/ExchangeService/" + senderProj + "/" + senderApp + "/" + senderGraph + "/dxo";
    DataTransferObjects sendingDtos = NetUtil.post(DataTransferObjects.class, senderUrl, dxRequest);
      
    // get receiving DTOs
    String receiverUrl = receiverUri + "/ExchangeService/" + receiverProj + "/" + receiverApp + "/" + receiverGraph + "/dxo";    
    DataTransferObjects receivingDtos = NetUtil.post(DataTransferObjects.class, receiverUrl, dxRequest);
    
    return diff(sendingDtos, receivingDtos);
  }
  
  @POST
  @Path("/dxi")
  public String diff(DataTransferIndices sendingDxis, DataTransferIndices receivingDxis)
  {
    init();    
    DiffProvider diffProvider = new DiffProvider(settings);
    return diffProvider.diff(sendingDxis, receivingDxis);
  }
  
  @POST
  @Path("/dxo")
  public String diff(DataTransferObjects sendingDtos, DataTransferObjects receivingDtos)
  {
    init();
    DiffProvider diffProvider = new DiffProvider(settings);
    return diffProvider.diff(sendingDtos, receivingDtos);
  }
  
  private void init()
  {
    settings.put("baseDir", context.getRealPath("/"));
  }
}