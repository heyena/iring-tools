
package org.iringtools.federation;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for IDGenerators complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="IDGenerators">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="iDGenerator" type="{http://iringtools.org/federation}IDGenerator" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "IDGenerators", propOrder = {
    "idGenerators"
})
public class IDGenerators {

    @XmlElement(name = "iDGenerator", required = true)
    protected List<IDGenerator> idGenerators;

    /**
     * Gets the value of the idGenerators property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the idGenerators property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getIDGenerators().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link IDGenerator }
     * 
     * 
     */
    public List<IDGenerator> getIDGenerators() {
        if (idGenerators == null) {
            idGenerators = new ArrayList<IDGenerator>();
        }
        return this.idGenerators;
    }

    /**
     * Sets the value of the idGenerators property.
     * 
     * @param idGenerators
     *     allowed object is
     *     {@link IDGenerator }
     *     
     */
    public void setIDGenerators(List<IDGenerator> idGenerators) {
        this.idGenerators = idGenerators;
    }

}
