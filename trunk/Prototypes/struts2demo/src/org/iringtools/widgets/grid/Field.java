package org.iringtools.widgets.grid;

public class Field
{
  protected String name;
  protected String type;
  protected int width;
  protected boolean fixed = false; // fixed width
  
  public void setName(String name)
  {
    this.name = name;
  }
  
  public String getName()
  {
    return name;
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
}
