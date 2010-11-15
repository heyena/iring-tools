
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
 *         &lt;element name="queryItem" type="{http://www.iringtools.org/refdata/queries}QueryItem" maxOccurs="unbounded"/>
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
    "queryItems"
})
@XmlRootElement(name = "queries")
public class Queries {

    @XmlElement(name = "queryItem", required = true)
    protected List<QueryItem> queryItems;

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
     * {@link QueryItem }
     * 
     * 
     */
    public List<QueryItem> getQueryItems() {
        if (queryItems == null) {
            queryItems = new ArrayList<QueryItem>();
        }
        return this.queryItems;
    }

    /**
     * Sets the value of the queryItems property.
     * 
     * @param queryItems
     *     allowed object is
     *     {@link QueryItem }
     *     
     */
    public void setQueryItems(List<QueryItem> queryItems) {
        this.queryItems = queryItems;
    }

}
