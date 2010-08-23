package org.iringtools.test;

import java.util.List;

import org.iringtools.adapter.library.dto.DataTransferIndices;
import org.iringtools.adapter.library.dto.DataTransferObjects;
import org.iringtools.exchange.library.directory.Directory;
import org.iringtools.library.Identifiers;
import org.iringtools.utility.JaxbUtil;
import org.iringtools.utility.NetUtil;

public class DiffServiceTest
{
  public static void main(String[] args) 
  {
    try
    {
      String directoryUrl = "http://localhost:8080/iringtools/directoryservice/exchanges";
      Directory directory = NetUtil.get(Directory.class, directoryUrl);
      System.out.println(JaxbUtil.toXml(directory));
      
      System.out.println("Press any key to continue...");
      System.in.read();
      
      String dxiUrl = "http://localhost:8080/iringtools/diffservice/dxi/1";
      DataTransferIndices dxi = NetUtil.get(DataTransferIndices.class, dxiUrl);
      System.out.println(JaxbUtil.toXml(dxi));
      
      System.out.println("Press any key to continue...");
      System.in.read();      
      
      String dxoUrl = "http://localhost:8080/iringtools/diffservice/dxo/1";
      Identifiers identifiers = new Identifiers();
      List<String> idList = identifiers.getIdentifier();
      idList.add("Tag-1");
      idList.add("Tag-3");
      DataTransferObjects dxo = NetUtil.post(DataTransferObjects.class, dxoUrl, identifiers);  
      System.out.println(JaxbUtil.toXml(dxo));
      
      // submit dxo to adapter service
    }
    catch (Exception ex)
    {
      System.out.println(ex);      
    }
  }
}
