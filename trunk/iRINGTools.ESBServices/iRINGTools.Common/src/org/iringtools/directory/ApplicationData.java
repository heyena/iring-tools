
package org.iringtools.directory;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for ApplicationData complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="ApplicationData">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="application" type="{http://www.iringtools.org/directory}Application" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "ApplicationData", propOrder = {
    "applications"
})
public class ApplicationData {

    @XmlElement(name = "application", required = true)
    protected List<Application> applications;

    /**
     * Gets the value of the applications property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the applications property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getApplications().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Application }
     * 
     * 
     */
    public List<Application> getApplications() {
        if (applications == null) {
            applications = new ArrayList<Application>();
        }
        return this.applications;
    }

    /**
     * Sets the value of the applications property.
     * 
     * @param applications
     *     allowed object is
     *     {@link Application }
     *     
     */
    public void setApplications(List<Application> applications) {
        this.applications = applications;
    }

}
