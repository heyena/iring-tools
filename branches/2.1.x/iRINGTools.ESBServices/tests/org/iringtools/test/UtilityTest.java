package org.iringtools.test;

import org.iringtools.utility.JsonUtil;

public class UtilityTest
{
  public static void main(String[] args) 
  {
    try
    {  
      java.util.HashMap<String, String> maps = new java.util.HashMap<String, String>();
      maps.put("k1", "v1");
      maps.put("k2", "v2");
      maps.put("k3", "v3");
      maps.put("k4", "v4");
      String jsonMaps = JsonUtil.serialize(maps);
      System.out.println("java to json: " + jsonMaps);
      
      java.util.HashMap<String, String> reverseMaps = JsonUtil.deserialize(MyHashMap.class.getGenericSuperclass(), jsonMaps);
      System.out.println("json to java: " + JsonUtil.serialize(reverseMaps));
    }
    catch (Exception ex)
    {
      System.out.println(ex);      
    }
  }
}
