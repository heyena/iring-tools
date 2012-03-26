package org.iringtools.library;

public interface IDataObject  {
	Object getPropertyValue(String propertyName);
    void setPropertyValue(String propertyName, Object value);
}
