
package org.iringtools.library;

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
 *         &lt;element name="assembly" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="name" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="configurable" type="{http://www.w3.org/2001/XMLSchema}boolean"/>
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
    "assembly",
    "name",
    "configurable"
})
@XmlRootElement(name = "DataLayer")
public class DataLayer {

    @XmlElement(required = true)
    protected String assembly;
    @XmlElement(required = true)
    protected String name;
    protected boolean configurable;

    /**
     * Gets the value of the assembly property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getAssembly() {
        return assembly;
    }

    /**
     * Sets the value of the assembly property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setAssembly(String value) {
        this.assembly = value;
    }

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
     * Gets the value of the configurable property.
     * This getter has been renamed from isConfigurable() to getConfigurable() by cxf-xjc-boolean plugin.
     * 
     */
    public boolean getConfigurable() {
        return configurable;
    }

    /**
     * Sets the value of the configurable property.
     * 
     */
    public void setConfigurable(boolean value) {
        this.configurable = value;
    }

}
