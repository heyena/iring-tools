
package org.iringtools.refdata.queries;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for Query complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="Query">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="queryBindings" type="{http://www.iringtools.org/refdata/queries}QueryBinding" maxOccurs="unbounded"/>
 *         &lt;element name="fileName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "Query", propOrder = {
    "queryBindings",
    "fileName"
})
@XmlRootElement(name = "query")
public class Query {

    @XmlElement(required = true)
    protected List<QueryBinding> queryBindings;
    @XmlElement(required = true)
    protected String fileName;

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
     * Gets the value of the fileName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getFileName() {
        return fileName;
    }

    /**
     * Sets the value of the fileName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setFileName(String value) {
        this.fileName = value;
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
