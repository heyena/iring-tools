package org.iringtools.widgets.grid;

import java.util.List;

public class Grid
{
  protected String label;
  protected String type;
  protected int total;
  protected List<Field> fields;
  protected List<List<String>> data;
  
  public String getLabel()
  {
    return label;    
  }
  
  public void setLabel(String label)
  {
    this.label= label;
  }
  
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
}
