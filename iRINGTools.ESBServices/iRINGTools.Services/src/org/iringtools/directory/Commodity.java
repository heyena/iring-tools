
package org.iringtools.directory;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for Commodity complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="Commodity">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="name" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="exchanges" type="{http://www.iringtools.org/directory}Exchanges"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "Commodity", propOrder = {
    "name",
    "exchanges"
})
public class Commodity {

    @XmlElement(required = true)
    protected String name;
    @XmlElement(required = true)
    protected Exchanges exchanges;

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
     * Gets the value of the exchanges property.
     * 
     * @return
     *     possible object is
     *     {@link Exchanges }
     *     
     */
    public Exchanges getExchanges() {
        return exchanges;
    }

    /**
     * Sets the value of the exchanges property.
     * 
     * @param value
     *     allowed object is
     *     {@link Exchanges }
     *     
     */
    public void setExchanges(Exchanges value) {
        this.exchanges = value;
    }

}
