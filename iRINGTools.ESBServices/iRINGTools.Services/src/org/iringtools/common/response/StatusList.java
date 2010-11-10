
package org.iringtools.common.response;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for StatusList complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="StatusList">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="status" type="{http://www.iringtools.org/common/response}Status" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "StatusList", propOrder = {
    "statuses"
})
public class StatusList {

    @XmlElement(name = "status", required = true)
    protected List<Status> statuses;

    /**
     * Gets the value of the statuses property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the statuses property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getStatuses().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Status }
     * 
     * 
     */
    public List<Status> getStatuses() {
        if (statuses == null) {
            statuses = new ArrayList<Status>();
        }
        return this.statuses;
    }

    /**
     * Sets the value of the statuses property.
     * 
     * @param statuses
     *     allowed object is
     *     {@link Status }
     *     
     */
    public void setStatuses(List<Status> statuses) {
        this.statuses = statuses;
    }

}
