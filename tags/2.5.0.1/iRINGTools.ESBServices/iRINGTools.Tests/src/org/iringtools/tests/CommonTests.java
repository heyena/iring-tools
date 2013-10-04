package org.iringtools.tests;

import static org.junit.Assert.*;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpClientException;
import org.junit.Test;

public class CommonTests
{
  @Test
  public void testGetHttps()
  {
    String url = "https://iringcore.staging.mypsn.com";
    HttpClient client = new HttpClient(url);
    
    try
    {
      String response = client.get(String.class);
      assertTrue(response.length() > 0);
    }
    catch (HttpClientException e)
    {
      assertFalse(true);
    }
  }
}
