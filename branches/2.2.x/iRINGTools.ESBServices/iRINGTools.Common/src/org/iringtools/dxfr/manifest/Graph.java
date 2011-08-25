
package org.iringtools.dxfr.manifest;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for Graph complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="Graph">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="name" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="classTemplatesList" type="{http://www.iringtools.org/dxfr/manifest}ClassTemplatesList"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "Graph", propOrder = {
    "name",
    "classTemplatesList"
})
public class Graph {

    @XmlElement(required = true)
    protected String name;
    @XmlElement(required = true)
    protected ClassTemplatesList classTemplatesList;

    /**
     * Gets the value of the name property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getName() {
        return name;
    }

    /**
     * Sets the value of the name property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setName(String value) {
        this.name = value;
    }

    /**
     * Gets the value of the classTemplatesList property.
     * 
     * @return
     *     possible object is
     *     {@link ClassTemplatesList }
     *     
     */
    public ClassTemplatesList getClassTemplatesList() {
        return classTemplatesList;
    }

    /**
     * Sets the value of the classTemplatesList property.
     * 
     * @param value
     *     allowed object is
     *     {@link ClassTemplatesList }
     *     
     */
    public void setClassTemplatesList(ClassTemplatesList value) {
        this.classTemplatesList = value;
    }

}
