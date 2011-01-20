package org.iringtools.widgets.grid;

import java.util.List;

public class Grid
{
  protected String type;
  protected List<Field> fields;
  protected List<List<String>> data;
  protected int total;
  
  public String getType()
  {
    return type;    
  }
  
  public void setType(String type)
  {
    this.type= type;
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
  
  public int getTotal()
  {
    return total;
  }
  
  public void setTotal(int total)
  {
    this.total = total;
  }
}
