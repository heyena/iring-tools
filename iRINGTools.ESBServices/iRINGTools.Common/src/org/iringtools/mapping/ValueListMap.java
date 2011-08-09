
package org.iringtools.mapping;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for ValueListMap complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="ValueListMap">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="name" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="valueMaps" type="{http://www.iringtools.org/mapping}ValueMaps"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "ValueListMap", propOrder = {
    "name",
    "valueMaps"
})
public class ValueListMap {

    @XmlElement(required = true)
    protected String name;
    @XmlElement(required = true)
    protected ValueMaps valueMaps;

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
     * Gets the value of the valueMaps property.
     * 
     * @return
     *     possible object is
     *     {@link ValueMaps }
     *     
     */
    public ValueMaps getValueMaps() {
        return valueMaps;
    }

    /**
     * Sets the value of the valueMaps property.
     * 
     * @param value
     *     allowed object is
     *     {@link ValueMaps }
     *     
     */
    public void setValueMaps(ValueMaps value) {
        this.valueMaps = value;
    }

}
