
package org.iringtools.refdata.federation;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for Repositories complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="Repositories">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="sequenceId" type="{http://www.w3.org/2001/XMLSchema}int"/>
 *         &lt;element name="repository" type="{http://www.iringtools.org/refdata/federation}Repository" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "Repositories", propOrder = {
    "sequenceId",
    "items"
})
public class Repositories {

    protected int sequenceId;
    @XmlElement(name = "repository", required = true)
    protected List<Repository> items;

    /**
     * Gets the value of the sequenceId property.
     * 
     */
    public int getSequenceId() {
        return sequenceId;
    }

    /**
     * Sets the value of the sequenceId property.
     * 
     */
    public void setSequenceId(int value) {
        this.sequenceId = value;
    }

    /**
     * Gets the value of the items property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the items property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getItems().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Repository }
     * 
     * 
     */
    public List<Repository> getItems() {
        if (items == null) {
            items = new ArrayList<Repository>();
        }
        return this.items;
    }

    /**
     * Sets the value of the items property.
     * 
     * @param items
     *     allowed object is
     *     {@link Repository }
     *     
     */
    public void setItems(List<Repository> items) {
        this.items = items;
    }

}
