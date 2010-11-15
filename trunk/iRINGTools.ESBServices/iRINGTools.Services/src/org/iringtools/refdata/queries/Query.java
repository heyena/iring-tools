
package org.iringtools.refdata.queries;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
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
 *         &lt;element name="fileName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="bindings" type="{http://www.iringtools.org/refdata/queries}QueryBindings"/>
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
    "fileName",
    "bindings"
})
public class Query {

    @XmlElement(required = true)
    protected String fileName;
    @XmlElement(required = true)
    protected QueryBindings bindings;

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
     * Gets the value of the bindings property.
     * 
     * @return
     *     possible object is
     *     {@link QueryBindings }
     *     
     */
    public QueryBindings getBindings() {
        return bindings;
    }

    /**
     * Sets the value of the bindings property.
     * 
     * @param value
     *     allowed object is
     *     {@link QueryBindings }
     *     
     */
    public void setBindings(QueryBindings value) {
        this.bindings = value;
    }

}
