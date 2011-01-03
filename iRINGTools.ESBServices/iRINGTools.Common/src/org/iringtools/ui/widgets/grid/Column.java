package org.iringtools.ui.widgets.grid;

public class Column
{
  protected String id;
  protected String header;
  protected double width;
  protected String sortable;
  protected String dataIndex;

  public String getId()
  {
    return id;
  }

  public void setId(String value)
  {
    this.id = value;
  }

  public String getHeader()
  {
    return header;
  }

  public void setHeader(String value)
  {
    this.header = value;
  }

  public double getWidth()
  {
    return width;
  }

  public void setWidth(double value)
  {
    this.width = value;
  }

  public String getSortable()
  {
    return sortable;
  }

  public void setSortable(String value)
  {
    this.sortable = value;
  }

  public String getDataIndex()
  {
    return dataIndex;
  }

  public void setDataIndex(String value)
  {
    this.dataIndex = value;
  }
}
