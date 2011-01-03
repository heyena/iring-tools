package org.iringtools.ui.widgets.grid;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

public class Rows
{
  protected String success;
  protected double total;
  protected List<HashMap<String, String>> data;

  public String getSuccess()
  {
    return success;
  }

  public void setSuccess(String value)
  {
    this.success = value;
  }

  public double getTotal()
  {
    return total;
  }

  public void setTotal(double value)
  {
    this.total = value;
  }

  public List<HashMap<String, String>> getData()
  {
    if (data == null)
    {
      data = new ArrayList<HashMap<String, String>>();
    }
    return this.data;
  }

  public void setData(List<HashMap<String, String>> data)
  {
    this.data = data;
  }
}
