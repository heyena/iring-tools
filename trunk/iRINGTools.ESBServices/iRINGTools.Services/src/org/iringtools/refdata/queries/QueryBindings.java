
package org.iringtools.refdata.queries;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for QueryBindings complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="QueryBindings">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="queryBinding" type="{http://www.iringtools.org/refdata/queries}QueryBinding" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "QueryBindings", propOrder = {
    "queryBindings"
})
public class QueryBindings {

    @XmlElement(name = "queryBinding", required = true)
    protected List<QueryBinding> queryBindings;

    /**
     * Gets the value of the queryBindings property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the queryBindings property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getQueryBindings().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link QueryBinding }
     * 
     * 
     */
    public List<QueryBinding> getQueryBindings() {
        if (queryBindings == null) {
            queryBindings = new ArrayList<QueryBinding>();
        }
        return this.queryBindings;
    }

    /**
     * Sets the value of the queryBindings property.
     * 
     * @param queryBindings
     *     allowed object is
     *     {@link QueryBinding }
     *     
     */
    public void setQueryBindings(List<QueryBinding> queryBindings) {
        this.queryBindings = queryBindings;
    }

}
