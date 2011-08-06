
package org.ids_adi.ns.qxf.model;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for LicenseText complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="LicenseText">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;attribute name="lang" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="formal" type="{http://www.w3.org/2001/XMLSchema}string" default="true" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "LicenseText")
public class LicenseText {

    @XmlAttribute(name = "lang")
    protected String lang;
    @XmlAttribute(name = "formal")
    protected String formal;

    /**
     * Gets the value of the lang property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getLang() {
        return lang;
    }

    /**
     * Sets the value of the lang property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setLang(String value) {
        this.lang = value;
    }

    /**
     * Gets the value of the formal property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getFormal() {
        if (formal == null) {
            return "true";
        } else {
            return formal;
        }
    }

    /**
     * Sets the value of the formal property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setFormal(String value) {
        this.formal = value;
    }

}
