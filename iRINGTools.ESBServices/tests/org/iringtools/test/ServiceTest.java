package org.iringtools.test;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import org.iringtools.adapter.dti.DataTransferIndices;
import org.iringtools.adapter.dto.DataTransferObjects;
import org.iringtools.common.request.ExchangeRequest;
import org.iringtools.common.response.Response;
import org.iringtools.directory.Directory;
import org.iringtools.utility.JaxbUtil;
import org.iringtools.utility.WebClient;

public class ServiceTest
{
  public static void main(String[] args) 
  {
    try
    {
      WebClient esbClient = new WebClient("http://localhost:8080/iringtools/services/esb");
      BufferedReader in = new BufferedReader(new InputStreamReader(System.in));
      
      System.out.println("Getting exchange definitions ...");
      Directory directory = esbClient.get(Directory.class, "/directory");
      System.out.println(JaxbUtil.toXml(directory, true));      
      System.out.print("Press any key to continue...");
      in.readLine();
      
      System.out.println("Getting dti of exchange id 1...");
      DataTransferIndices dti = esbClient.get(DataTransferIndices.class, "/12345_000/exchanges/1");
      System.out.println(JaxbUtil.toXml(dti, true));      
      System.out.print("Press any key to continue...");
      in.readLine();    
      
      System.out.println("Getting dto of exchange id 1...");
      DataTransferObjects dto = esbClient.post(DataTransferObjects.class, "/12345_000/exchanges/1", dti);  
      System.out.println(JaxbUtil.toXml(dto, true));      
      System.out.print("Press any key to continue...");
      in.readLine();    
      
      System.out.println("Submitting dti to esb service...");
      ExchangeRequest exchangeRequest = new ExchangeRequest();
      exchangeRequest.setReviewed(true);
      exchangeRequest.setDataTransferIndices(dti);
      System.out.println(JaxbUtil.toXml(exchangeRequest, true));
      Response response = esbClient.post(Response.class, "/12345_000/exchanges/1/submit", exchangeRequest);
      System.out.println(JaxbUtil.toXml(response, true));
      
      System.out.println("\nTest complete!");
    }
    catch (Exception ex)
    {
      System.out.println(ex);      
    }
  }
}
