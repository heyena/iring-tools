package org.iringtools.widgets.grid;

public class Field
{
  protected String name;  // header
  protected String dataIndex;
  protected String type;  // data type
  protected int width;
  protected boolean fixed = false;  // fixed width
  protected boolean filterable = true;
  protected boolean sortable = true;
  
  public void setName(String name)
  {
    this.name = name;
  }
  
  public String getName()
  {
    return name;
  }
  
  public void setDataIndex(String dataIndex)
  {
    this.dataIndex = dataIndex;
  }

  public String getDataIndex()
  {
    return dataIndex;
  }

  public void setType(String type)
  {
    this.type = type;
  }
  
  public String getType()
  {
    return type;
  } 
  
  public void setWidth(int width)
  {
    this.width = width;
  }
  
  public int getWidth()
  {
    return width;
  } 
  
  public void setFixed(boolean fixed)
  {
    this.fixed = fixed;
  }
  
  public boolean getFixed()
  {
    return fixed;
  }

  public void setFilterable(boolean filterable)
  {
    this.filterable = filterable;
  }

  public boolean getFilterable()
  {
    return filterable;
  }

  public void setSortable(boolean sortable)
  {
    this.sortable = sortable;
  }

  public boolean getSortable()
  {
    return sortable;
  } 
}
