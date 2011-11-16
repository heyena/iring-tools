package org.iringtools.ui.widgets.grid;

import java.util.ArrayList;
import java.util.List;

public class Grid
{
  protected List<Filter> filterSets;
  protected List<Header> headerLists;
  protected List<Column> columnData;
  protected String classObjName;
  protected String success;
  protected String cacheData;
  protected String sortBy;
  protected String sortOrder;
  protected int pageSize;

  public List<Filter> getFilterSets()
  {
    if (filterSets == null)
    {
      filterSets = new ArrayList<Filter>();
    }
    return this.filterSets;
  }

  public void setFilterSets(List<Filter> filterSets)
  {
    this.filterSets = filterSets;
  }

  public List<Column> getColumnData()
  {
    if (columnData == null)
    {
      columnData = new ArrayList<Column>();
    }
    return this.columnData;
  }

  public void setColumnData(List<Column> columnDatas)
  {
    this.columnData = columnDatas;
  }

  public String getClassObjName()
  {
    return classObjName;
  }

  public void setClassObjName(String value)
  {
    this.classObjName = value;
  }

  public String getSuccess()
  {
    return success;
  }

  public void setSuccess(String value)
  {
    this.success = value;
  }

  public String getCacheData()
  {
    return cacheData;
  }

  public void setCacheData(String value)
  {
    this.cacheData = value;
  }
  
  public String getSortBy()
  {
    return sortBy;
  }

  public void setSortBy(String value)
  {
    this.sortBy = value;
  }
  
  public String getSortOrder()
  {
    return sortOrder;
  }

  public void setSortOrder(String value)
  {
    this.sortOrder = value;
  }


  public double getPageSize()
  {
    return pageSize;
  }

  public void setPageSize(int value)
  {
    this.pageSize = value;
  }

  public List<Header> getHeaderLists()
  {
    if (headerLists == null)
    {
      headerLists = new ArrayList<Header>();
    }
    return this.headerLists;
  }

  public void setHeaderLists(List<Header> headerLists)
  {
    this.headerLists = headerLists;
  }
}
