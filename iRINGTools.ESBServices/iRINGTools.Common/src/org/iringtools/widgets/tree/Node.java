package org.iringtools.widgets.tree;

import java.util.HashMap;

public abstract class Node
{
  protected String text;
  protected String iconCls;
  protected String identifier;
  protected boolean hidden = false;
  protected String type;
  protected Object record;

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

  public String getType() {
	return type;
  }

  public void setType(String type) {
	this.type = type;
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
  
  public Object getRecord() {
		return record;
	}

	public void setRecord(Object record) {
		this.record = record;
	}

}
