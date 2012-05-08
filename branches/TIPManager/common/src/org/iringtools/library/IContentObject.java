package org.iringtools.library;

import java.io.InputStream;

public interface IContentObject extends IDataObject
{
	String getidentifier();
	void setidentifier(String value);
	
	String getcontentType();
	void setcontentType(String value);
	
	InputStream getcontent();
	void setcontent(InputStream value);
	
	String gethashType();
	void sethashType(String value);
	
	String gethash();
	void sethash(String value);
	
	String geturl();
	void seturl(String value);
}
