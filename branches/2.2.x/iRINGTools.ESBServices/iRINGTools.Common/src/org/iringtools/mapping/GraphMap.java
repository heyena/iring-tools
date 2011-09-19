
package org.iringtools.mapping;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for anonymous complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType>
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="name" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element ref="{http://www.iringtools.org/mapping}classTemplateMaps"/>
 *         &lt;element name="dataObjectName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "", propOrder = {
    "name",
    "classTemplateMaps",
    "dataObjectName"
})
@XmlRootElement(name = "graphMap")
public class GraphMap {

    @XmlElement(required = true)
    protected String name;
    @XmlElement(required = true)
    protected ClassTemplateMaps classTemplateMaps;
    @XmlElement(required = true)
    protected String dataObjectName;

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
     * Gets the value of the classTemplateMaps property.
     * 
     * @return
     *     possible object is
     *     {@link ClassTemplateMaps }
     *     
     */
    public ClassTemplateMaps getClassTemplateMaps() {
        return classTemplateMaps;
    }

    /**
     * Sets the value of the classTemplateMaps property.
     * 
     * @param value
     *     allowed object is
     *     {@link ClassTemplateMaps }
     *     
     */
    public void setClassTemplateMaps(ClassTemplateMaps value) {
        this.classTemplateMaps = value;
    }

    /**
     * Gets the value of the dataObjectName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getDataObjectName() {
        return dataObjectName;
    }

    /**
     * Sets the value of the dataObjectName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setDataObjectName(String value) {
        this.dataObjectName = value;
    }

}
