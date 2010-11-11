package org.iringtools.test;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.util.Hashtable;

import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.protocol.manifest.Manifest;
import org.iringtools.utility.JaxbUtil;
import org.iringtools.utility.WebClient;

public class WebClientTest 
{  
  public static void main(String[] args) throws Exception
  {
    BufferedReader in = new BufferedReader(new InputStreamReader(System.in));
    
    WebClient webClient = new WebClient("http://localhost:54321/dto/12345_000/ABC");
    Manifest manifest = webClient.get(Manifest.class, "/manifest");
    System.out.println(webClient.post(String.class, "/LINES/xfr", manifest));
    System.out.print("Press any key to continue...");
    in.readLine();
    
    String manifestXml = JaxbUtil.toXml(manifest, false);
    DataTransferIndices dti = webClient.postMessage(DataTransferIndices.class, "/LINES/xfr", manifestXml);
    System.out.println(JaxbUtil.toXml(dti, true));    
    System.out.print("Press any key to continue...");
    in.readLine();
    
    WebClient multipartClient = new WebClient("http://search.yahoo.com");
    Hashtable<String, String> keyValuePairs = new Hashtable<String, String>();
    keyValuePairs.put("p", "hello");
    System.out.println(multipartClient.postMultipartMessage(String.class, "/search", keyValuePairs));
    
    System.out.println("\nTest complete!");
  }
}
