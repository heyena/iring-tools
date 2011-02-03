package org.iringtools.widgets.grid;

public class Filter
{
  protected String type;
  protected String comparison;
  protected String value;
  protected String field;
  
  public void setType(String type)
  {
    this.type = type;
  }
  
  public String getType()
  {
    return type;
  }

  public void setComparison(String comparison)
  {
    this.comparison = comparison;
  }

  public String getComparison()
  {
    return comparison;
  }

  public void setValue(String value)
  {
    this.value = value;
  }

  public String getValue()
  {
    return value;
  }

  public void setField(String field)
  {
    this.field = field;
  }

  public String getField()
  {
    return field;
  }
}
