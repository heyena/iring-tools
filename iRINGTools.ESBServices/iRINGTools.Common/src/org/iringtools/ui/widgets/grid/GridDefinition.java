package org.iringtools.ui.widgets.grid;

import java.util.ArrayList;
import java.util.List;

public class GridDefinition
{
  protected List<Header> headersList;
  protected List<Column> columnsData;
  protected String success;
  protected List<ArrayList<String>> rowData;

  public List<ArrayList<String>> getRowData()
  {
    if (rowData == null)
    {    	
    	rowData = new ArrayList<ArrayList<String>>();
    }
    return this.rowData;
  }

  public void setRowData(List<ArrayList<String>> data)
  {
    this.rowData = data;
  }
  
  public List<Column> getColumnData()
  {
    if (columnsData == null)
    {
    	columnsData = new ArrayList<Column>();
    }
    return this.columnsData;
  }

  public void setColumnData(List<Column> columnDatas)
  {
    this.columnsData = columnDatas;
  }


  public String getSuccess()
  {
    return success;
  }

  public void setSuccess(String value)
  {
    this.success = value;
  }

 

  public List<Header> getHeaderLists()
  {
    if (headersList == null)
    {
    	headersList = new ArrayList<Header>();
    }
    return this.headersList;
  }

  public void setHeaderLists(List<Header> headerLists)
  {
    this.headersList = headerLists;
  }
}
