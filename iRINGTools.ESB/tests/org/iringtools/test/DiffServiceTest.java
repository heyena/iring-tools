package org.iringtools.test;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.util.List;

import org.iringtools.adapter.library.dto.DataTransferIndices;
import org.iringtools.adapter.library.dto.DataTransferObjects;
import org.iringtools.exchange.library.directory.Directory;
import org.iringtools.library.Identifiers;
import org.iringtools.library.response.Response;
import org.iringtools.utility.JaxbUtil;
import org.iringtools.utility.NetUtil;

public class DiffServiceTest
{
  public static void main(String[] args) 
  {
    try
    {
      BufferedReader in = new BufferedReader(new InputStreamReader(System.in));

      System.out.println("Getting exchanges ...");
      String directoryUrl = "http://localhost:8080/iringtools/directoryservice/exchanges";
      Directory directory = NetUtil.get(Directory.class, directoryUrl);
      System.out.println(JaxbUtil.toXml(directory));
      
      System.out.print("Press any key to continue...");
      in.readLine();
      
      System.out.println("Getting dxi of exchange [1]...");
      String dxiUrl = "http://localhost:8080/iringtools/diffservice/dxi/1";
      DataTransferIndices dxi = NetUtil.get(DataTransferIndices.class, dxiUrl);
      System.out.println(JaxbUtil.toXml(dxi));
      
      System.out.print("Press any key to continue...");
      in.readLine();    
      
      System.out.println("Getting dxo of exchange [1]...");
      String dxoUrl = "http://localhost:8080/iringtools/diffservice/dxo/1";
      Identifiers identifiers = new Identifiers();
      List<String> idList = identifiers.getIdentifier();
      idList.add("Tag-1");
      idList.add("Tag-3");
      DataTransferObjects dxo = NetUtil.post(DataTransferObjects.class, dxoUrl, identifiers);  
      System.out.println(JaxbUtil.toXml(dxo));
      
      System.out.print("Press any key to continue...");
      in.readLine();    
      
      System.out.println("Submit dxo to adapter service...");
      String adapterServiceUrl = "http://localhost:54321/AdapterService/12345_000/ABC/Lines?format=dxo";
      Response response = NetUtil.post(Response.class, adapterServiceUrl, dxo);
      System.out.println(JaxbUtil.toXml(response));
    }
    catch (Exception ex)
    {
      System.out.println(ex);      
    }
    
    System.out.println("done!");
  }
}
