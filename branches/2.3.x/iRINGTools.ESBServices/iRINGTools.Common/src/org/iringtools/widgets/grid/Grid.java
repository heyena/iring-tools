package org.iringtools.widgets.grid;

import java.util.List;

public class Grid
{
  protected String identifier;  // optional
  protected String description; // optional
  protected int total;
  protected List<Field> fields;
  protected List<List<String>> data;
  
  public String getIdentifier()
  {
    return identifier;    
  }
  
  public void setIdentifier(String identifier)
  {
    this.identifier= identifier;
  }
  
  public String getDescription()
  {
    return description;    
  }
  
  public void setDescription(String description)
  {
    this.description= description;
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
}
