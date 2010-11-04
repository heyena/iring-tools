
package org.iringtools.directory;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for Exchanges complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="Exchanges">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="exchange" type="{http://iringtools.org/directory}Exchange" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "Exchanges", propOrder = {
    "exchanges"
})
public class Exchanges {

    @XmlElement(name = "exchange", required = true)
    protected List<Exchange> exchanges;

    /**
     * Gets the value of the exchanges property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the exchanges property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getExchanges().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Exchange }
     * 
     * 
     */
    public List<Exchange> getExchanges() {
        if (exchanges == null) {
            exchanges = new ArrayList<Exchange>();
        }
        return this.exchanges;
    }

    /**
     * Sets the value of the exchanges property.
     * 
     * @param exchanges
     *     allowed object is
     *     {@link Exchange }
     *     
     */
    public void setExchanges(List<Exchange> exchanges) {
        this.exchanges = exchanges;
    }

}
