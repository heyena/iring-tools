
package org.iringtools.refdata.queries;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for Queries complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="Queries">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="key" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="queryItems" type="{http://www.iringtools.org/refdata/queries}Query" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "Queries", propOrder = {
    "key",
    "queryItems"
})
@XmlRootElement(name = "queries")
public class Queries {

    @XmlElement(required = true)
    protected String key;
    @XmlElement(required = true)
    protected List<Query> queryItems;

    /**
     * Gets the value of the key property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getKey() {
        return key;
    }

    /**
     * Sets the value of the key property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setKey(String value) {
        this.key = value;
    }

    /**
     * Gets the value of the queryItems property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the queryItems property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getQueryItems().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Query }
     * 
     * 
     */
    public List<Query> getQueryItems() {
        if (queryItems == null) {
            queryItems = new ArrayList<Query>();
        }
        return this.queryItems;
    }

    /**
     * Sets the value of the queryItems property.
     * 
     * @param queryItems
     *     allowed object is
     *     {@link Query }
     *     
     */
    public void setQueryItems(List<Query> queryItems) {
        this.queryItems = queryItems;
    }

}
