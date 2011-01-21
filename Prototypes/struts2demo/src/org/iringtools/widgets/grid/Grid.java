package org.iringtools.widgets.grid;

import java.util.HashMap;
import java.util.List;

public class Grid
{
  protected String type;
  protected int total;
  protected List<Field> fields;
  protected List<List<String>> data;
  protected HashMap<String, String> properties;
  
  public String getType()
  {
    return type;    
  }
  
  public void setType(String type)
  {
    this.type= type;
  }
  
  public int getTotal()
  {
    return total;
  }
  
  public void setTotal(int total)
  {
    this.total = total;
  }
  
  public List<Field> getFields()
  {
    return fields;
  }
  
  public void setFields(List<Field> fields)
  {
    this.fields = fields;
  }
  
  public List<List<String>> getData()
  {
    return data;
  }
  
  public void setData(List<List<String>> data)
  {
    this.data = data;
  }
  
  public HashMap<String, String> getProperties()
  {
    return properties;
  }
  
  public void setProperties(HashMap<String, String> properties)
  {
    this.properties = properties;
  }
  
  
}
