package org.iringtools.widgets.tree;

import java.util.HashMap;

public abstract class Node
{
  protected String text;
  protected String iconCls;
  protected String identifier;
  protected boolean hidden = false;
  protected HashMap<String, String> properties;

  public String getText()
  {
    return text;
  }

  public void setText(String value)
  {
    this.text = value;
  }
  
  public boolean getHidden()
  {
    return hidden;
  }

  public void setHidden(boolean value)
  {
    this.hidden = value;
  }

  public String getIconCls()
  {
    return iconCls;
  }

  public void setIconCls(String value)
  {
    this.iconCls = value;
  }
  
  public String getIdentifier()
  {
    return identifier;
  }

  public void setIdentifier(String value)
  {
    this.identifier = value;
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
