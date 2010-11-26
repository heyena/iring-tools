
package org.iringtools.mapping;

import java.util.ArrayList;
import java.util.List;
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
 *         &lt;element name="valueMap" type="{http://www.iringtools.org/mapping}ValueMap" maxOccurs="unbounded"/>
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
    @XmlElement(name = "valueMap", required = true)
    protected List<ValueMap> valueMaps;

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
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the valueMaps property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getValueMaps().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link ValueMap }
     * 
     * 
     */
    public List<ValueMap> getValueMaps() {
        if (valueMaps == null) {
            valueMaps = new ArrayList<ValueMap>();
        }
        return this.valueMaps;
    }

    /**
     * Sets the value of the valueMaps property.
     * 
     * @param valueMaps
     *     allowed object is
     *     {@link ValueMap }
     *     
     */
    public void setValueMaps(List<ValueMap> valueMaps) {
        this.valueMaps = valueMaps;
    }

}
