package org.iringtools.utility;

import org.iringtools.ui.widgets.tree.Property;

public class WidgetUtil
{
  public static Property createProperty(String name, Object value)
  {
    Property property = new Property();
    property.setName(name);
    property.setValue(value);
    return property;
  }
}
