package org.iringtools.widgets.tree;

import java.util.HashMap;

public abstract class Node
{
  protected String text;
  protected String iconCls;
  protected HashMap<String, String> properties;

  public String getText()
  {
    return text;
  }

  public void setText(String value)
  {
    this.text = value;
  }

  public String getIconCls()
  {
    return iconCls;
  }

  public void setIconCls(String value)
  {
    this.iconCls = value;
  }

  public HashMap<String, String> getProperties()
  {
    if (properties == null)
    {
      properties = new HashMap<String, String>();
    }
    return this.properties;
  }

  public void setProperties(HashMap<String, String> properties)
  {
    this.properties = properties;
  }
}
