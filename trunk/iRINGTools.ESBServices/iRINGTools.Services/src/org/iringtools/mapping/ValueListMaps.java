
package org.iringtools.mapping;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for ValueListMaps complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="ValueListMaps">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="valueListMaps" type="{http://www.iringtools.org/mapping}ValueListMap" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "ValueListMaps", propOrder = {
    "valueListMaps"
})
public class ValueListMaps {

    @XmlElement(required = true)
    protected List<ValueListMap> valueListMaps;

    /**
     * Gets the value of the valueListMaps property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the valueListMaps property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getValueListMaps().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link ValueListMap }
     * 
     * 
     */
    public List<ValueListMap> getValueListMaps() {
        if (valueListMaps == null) {
            valueListMaps = new ArrayList<ValueListMap>();
        }
        return this.valueListMaps;
    }

    /**
     * Sets the value of the valueListMaps property.
     * 
     * @param valueListMaps
     *     allowed object is
     *     {@link ValueListMap }
     *     
     */
    public void setValueListMaps(List<ValueListMap> valueListMaps) {
        this.valueListMaps = valueListMaps;
    }

}
