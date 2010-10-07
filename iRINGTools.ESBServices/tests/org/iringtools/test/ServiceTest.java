package org.iringtools.test;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import org.iringtools.adapter.dti.DataTransferIndices;
import org.iringtools.adapter.dto.DataTransferObjects;
import org.iringtools.common.request.ExchangeRequest;
import org.iringtools.directory.Directory;
import org.iringtools.common.response.Response;
import org.iringtools.utility.JaxbUtil;
import org.iringtools.utility.NetUtil;

public class ServiceTest
{
  public static void main(String[] args) 
  {
    try
    {
      BufferedReader in = new BufferedReader(new InputStreamReader(System.in));
      
      System.out.println("Getting exchange definitions ...");
      String directoryUrl = "http://localhost:8080/iringtools/services/esb/directory";
      Directory directory = NetUtil.get(Directory.class, directoryUrl);
      System.out.println(JaxbUtil.toXml(directory, true));
      
      System.out.print("Press any key to continue...");
      in.readLine();
      
      System.out.println("Getting dti of exchange id 1...");
      String dtiUrl = "http://localhost:8080/iringtools/services/esb/12345_000/exchanges/1";
      DataTransferIndices dti = NetUtil.get(DataTransferIndices.class, dtiUrl);
      System.out.println(JaxbUtil.toXml(dti, true));
      
      System.out.print("Press any key to continue...");
      in.readLine();    
      
      System.out.println("Getting dto of exchange id 1...");
      String dtoUrl = "http://localhost:8080/iringtools/services/esb/12345_000/exchanges/1";
      DataTransferObjects dto = NetUtil.post(DataTransferObjects.class, dtoUrl, dti);  
      System.out.println(JaxbUtil.toXml(dto, true));
      
      System.out.print("Press any key to continue...");
      in.readLine();    
      
      System.out.println("Submitting dti to esb service...");
      String dxiUrl = "http://localhost:8080/iringtools/services/esb/12345_000/exchanges/1/submit";  
      ExchangeRequest exchangeRequest = new ExchangeRequest();
      exchangeRequest.setReviewed(true);
      exchangeRequest.setDataTransferIndices(dti);
      Response response = NetUtil.post(Response.class, dxiUrl, exchangeRequest);
      System.out.println(JaxbUtil.toXml(response, true));
      
      System.out.println("\nTest complete!");
    }
    catch (Exception ex)
    {
      System.out.println(ex);      
    }
  }
}
