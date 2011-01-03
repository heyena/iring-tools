package org.iringtools.test;

import java.io.BufferedReader;
import java.io.InputStreamReader;

import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.directory.Directory;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.request.ExchangeRequest;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.JaxbUtil;

public class ServiceTest
{
  public static void main(String[] args) 
  {
	BufferedReader in = new BufferedReader(new InputStreamReader(System.in));
    String esbServiceUri = "http://localhost:8081/services/esb";
    HttpClient httpClient = new HttpClient(esbServiceUri);
    
    try
    {     
      JaxbUtil.read(DataTransferIndices.class, "C:\\Documents and Settings\\fwei\\Desktop\\dti.xml");
    	
      System.out.println("Getting exchange definitions ...");
      Directory directory = httpClient.get(Directory.class, "/directory");
      System.out.println(JaxbUtil.toXml(directory, true));      
      System.out.print("Press any key to continue...");
      in.readLine();
      
      System.out.println("Getting dti of exchange id 1...");
      DataTransferIndices dti = httpClient.get(DataTransferIndices.class, "/12345_000/exchanges/1");
      System.out.println(JaxbUtil.toXml(dti, true));      
      System.out.print("Press any key to continue...");
      in.readLine();    
      
      System.out.println("Getting dto of exchange id 1...");
      DataTransferObjects dto = httpClient.post(DataTransferObjects.class, "/12345_000/exchanges/1", dti);  
      System.out.println(JaxbUtil.toXml(dto, true));      
      System.out.print("Press any key to continue...");
      in.readLine();    
      
      System.out.println("Submitting dti to esb service...");
      ExchangeRequest exchangeRequest = new ExchangeRequest();
      exchangeRequest.setReviewed(true);
      exchangeRequest.setDataTransferIndices(dti);
      System.out.println(JaxbUtil.toXml(exchangeRequest, true));
      ExchangeResponse response = httpClient.post(ExchangeResponse.class, "/12345_000/exchanges/1/submit", exchangeRequest);
      System.out.println(JaxbUtil.toXml(response, true));
      
      System.out.println("\nTest complete!");
    }
    catch (Exception ex)
    {
      System.out.println(ex);      
    }
  }
}
