
package org.iringtools.library;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for PropertyMap complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="PropertyMap">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="dataPropertyName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="relatedPropertyName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "PropertyMap", propOrder = {
    "dataPropertyName",
    "relatedPropertyName"
})
public class PropertyMap {

    @XmlElement(required = true)
    protected String dataPropertyName;
    @XmlElement(required = true)
    protected String relatedPropertyName;

    /**
     * Gets the value of the dataPropertyName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getDataPropertyName() {
        return dataPropertyName;
    }

    /**
     * Sets the value of the dataPropertyName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setDataPropertyName(String value) {
        this.dataPropertyName = value;
    }

    /**
     * Gets the value of the relatedPropertyName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getRelatedPropertyName() {
        return relatedPropertyName;
    }

    /**
     * Sets the value of the relatedPropertyName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setRelatedPropertyName(String value) {
        this.relatedPropertyName = value;
    }

}
