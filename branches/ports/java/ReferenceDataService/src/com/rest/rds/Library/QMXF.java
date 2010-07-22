/**
 * 
 */
package com.rest.rds.Library;

import java.util.ArrayList;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;

/**
 * @author Mohamed Moubarak
 *
 */

@XmlRootElement(name="Employee")
@XmlAccessorType(XmlAccessType.FIELD)
public class QMXF {
	
	@XmlElement
	public ArrayList<TemplateDefinition> td;
	
	@XmlAttribute
    public String targetRepository;
	
	public QMXF()
	{
		td = new ArrayList<TemplateDefinition>();
	}
	
	public class TemplateDefinition 
	{

	}
}
