
package org.iringtools.library;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for BindingConfiguration complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="BindingConfiguration">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="bindingName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="interfaceClass" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="implementationClass" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="location" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "BindingConfiguration", propOrder = {
    "bindingName",
    "interfaceClass",
    "implementationClass",
    "location"
})
@XmlRootElement(name = "bindingConfiguration")
public class BindingConfiguration {

    @XmlElement(required = true)
    protected String bindingName;
    @XmlElement(required = true)
    protected String interfaceClass;
    @XmlElement(required = true)
    protected String implementationClass;
    @XmlElement(required = true)
    protected String location;

    /**
     * Gets the value of the bindingName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getBindingName() {
        return bindingName;
    }

    /**
     * Sets the value of the bindingName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setBindingName(String value) {
        this.bindingName = value;
    }

    /**
     * Gets the value of the interfaceClass property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getInterfaceClass() {
        return interfaceClass;
    }

    /**
     * Sets the value of the interfaceClass property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setInterfaceClass(String value) {
        this.interfaceClass = value;
    }

    /**
     * Gets the value of the implementationClass property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getImplementationClass() {
        return implementationClass;
    }

    /**
     * Sets the value of the implementationClass property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setImplementationClass(String value) {
        this.implementationClass = value;
    }

    /**
     * Gets the value of the location property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getLocation() {
        return location;
    }

    /**
     * Sets the value of the location property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setLocation(String value) {
        this.location = value;
    }

}
