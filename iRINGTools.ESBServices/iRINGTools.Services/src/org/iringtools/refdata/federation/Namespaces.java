
package org.iringtools.refdata.federation;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for Namespaces complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="Namespaces">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="namespace" type="{http://www.iringtools.org/refdata/federation}Namespace" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "Namespaces", propOrder = {
    "namespaces"
})
public class Namespaces {

    @XmlElement(name = "namespace", required = true)
    protected List<Namespace> namespaces;

    /**
     * Gets the value of the namespaces property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the namespaces property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getNamespaces().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Namespace }
     * 
     * 
     */
    public List<Namespace> getNamespaces() {
        if (namespaces == null) {
            namespaces = new ArrayList<Namespace>();
        }
        return this.namespaces;
    }

    /**
     * Sets the value of the namespaces property.
     * 
     * @param namespaces
     *     allowed object is
     *     {@link Namespace }
     *     
     */
    public void setNamespaces(List<Namespace> namespaces) {
        this.namespaces = namespaces;
    }

}
