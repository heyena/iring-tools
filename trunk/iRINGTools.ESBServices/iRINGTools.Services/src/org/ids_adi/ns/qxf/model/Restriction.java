
package org.ids_adi.ns.qxf.model;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for Restriction complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="Restriction">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="type" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="value" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="valueFrom" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="cardinality" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "Restriction", propOrder = {
    "type",
    "value",
    "valueFrom",
    "cardinality"
})
public class Restriction {

    @XmlElement(required = true)
    protected String type;
    @XmlElement(required = true)
    protected String value;
    @XmlElement(required = true)
    protected String valueFrom;
    @XmlElement(required = true)
    protected String cardinality;

    /**
     * Gets the value of the type property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getType() {
        return type;
    }

    /**
     * Sets the value of the type property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setType(String value) {
        this.type = value;
    }

    /**
     * Gets the value of the value property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getValue() {
        return value;
    }

    /**
     * Sets the value of the value property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setValue(String value) {
        this.value = value;
    }

    /**
     * Gets the value of the valueFrom property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getValueFrom() {
        return valueFrom;
    }

    /**
     * Sets the value of the valueFrom property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setValueFrom(String value) {
        this.valueFrom = value;
    }

    /**
     * Gets the value of the cardinality property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getCardinality() {
        return cardinality;
    }

    /**
     * Sets the value of the cardinality property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setCardinality(String value) {
        this.cardinality = value;
    }

}
