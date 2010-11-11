package org.iringtools.test;

import java.io.BufferedReader;
import java.io.InputStreamReader;

import org.iringtools.common.response.ExchangeResponse;
import org.iringtools.directory.Directory;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.request.ExchangeRequest;
import org.iringtools.utility.JaxbUtil;
import org.iringtools.utility.NetUtil;

public class ServiceTest
{
  public static void main(String[] args) 
  {
    BufferedReader in = new BufferedReader(new InputStreamReader(System.in));
    String esbServiceUri = "http://localhost:8080/iringtools/services/esb";
    
    try
    {     
      System.out.println("Getting exchange definitions ...");
      Directory directory = NetUtil.get(Directory.class, esbServiceUri + "/directory");
      System.out.println(JaxbUtil.toXml(directory, true));      
      System.out.print("Press any key to continue...");
      in.readLine();
      
      System.out.println("Getting dti of exchange id 1...");
      DataTransferIndices dti = NetUtil.get(DataTransferIndices.class, esbServiceUri + "/12345_000/exchanges/1");
      System.out.println(JaxbUtil.toXml(dti, true));      
      System.out.print("Press any key to continue...");
      in.readLine();    
      
      System.out.println("Getting dto of exchange id 1...");
      DataTransferObjects dto = NetUtil.post(DataTransferObjects.class, esbServiceUri + "/12345_000/exchanges/1", dti);  
      System.out.println(JaxbUtil.toXml(dto, true));      
      System.out.print("Press any key to continue...");
      in.readLine();    
      
      System.out.println("Submitting dti to esb service...");
      ExchangeRequest exchangeRequest = new ExchangeRequest();
      exchangeRequest.setReviewed(true);
      exchangeRequest.setDataTransferIndices(dti);
      System.out.println(JaxbUtil.toXml(exchangeRequest, true));
      ExchangeResponse response = NetUtil.post(ExchangeResponse.class, esbServiceUri + "/12345_000/exchanges/1/submit", exchangeRequest);
      System.out.println(JaxbUtil.toXml(response, true));
      
      System.out.println("\nTest complete!");
    }
    catch (Exception ex)
    {
      System.out.println(ex);      
    }
  }
}
