package org.iringtools.models;


public class ComplexHeader {

	private String name;
	private String type;
	
	public ComplexHeader(String name, String type) {
		this.name = name;
		if (type != null && type.contains(":"))
			this.type = type.substring(4, type.length());
		else
			this.type = type;
	}
	
	public void setName(String value) {
		this.name = value;
	}
	
	public void setType(String value) {
		this.type = value;
	}
	
	public String getName() {
		return name;
	}
	
	public String getType() {
		return type;
	}

}
