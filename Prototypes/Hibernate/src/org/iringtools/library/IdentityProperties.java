
package org.iringtools.library;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for IdentityProperties complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="IdentityProperties">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="useIdentityFilter" type="{http://www.w3.org/2001/XMLSchema}boolean"/>
 *         &lt;element name="identityProperty" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="keyRingProperty" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="isCaseSensitive" type="{http://www.w3.org/2001/XMLSchema}boolean"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "IdentityProperties", propOrder = {
    "useIdentityFilter",
    "identityProperty",
    "keyRingProperty",
    "isCaseSensitive"
})
public class IdentityProperties {

    protected boolean useIdentityFilter;
    @XmlElement(required = true)
    protected String identityProperty;
    @XmlElement(required = true)
    protected String keyRingProperty;
    protected boolean isCaseSensitive;

    /**
     * Gets the value of the useIdentityFilter property.
     * This getter has been renamed from isUseIdentityFilter() to getUseIdentityFilter() by cxf-xjc-boolean plugin.
     * 
     */
    public boolean getUseIdentityFilter() {
        return useIdentityFilter;
    }

    /**
     * Sets the value of the useIdentityFilter property.
     * 
     */
    public void setUseIdentityFilter(boolean value) {
        this.useIdentityFilter = value;
    }

    /**
     * Gets the value of the identityProperty property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getIdentityProperty() {
        return identityProperty;
    }

    /**
     * Sets the value of the identityProperty property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setIdentityProperty(String value) {
        this.identityProperty = value;
    }

    /**
     * Gets the value of the keyRingProperty property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getKeyRingProperty() {
        return keyRingProperty;
    }

    /**
     * Sets the value of the keyRingProperty property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setKeyRingProperty(String value) {
        this.keyRingProperty = value;
    }

    /**
     * Gets the value of the isCaseSensitive property.
     * This getter has been renamed from isIsCaseSensitive() to getIsCaseSensitive() by cxf-xjc-boolean plugin.
     * 
     */
    public boolean getIsCaseSensitive() {
        return isCaseSensitive;
    }

    /**
     * Sets the value of the isCaseSensitive property.
     * 
     */
    public void setIsCaseSensitive(boolean value) {
        this.isCaseSensitive = value;
    }

}
