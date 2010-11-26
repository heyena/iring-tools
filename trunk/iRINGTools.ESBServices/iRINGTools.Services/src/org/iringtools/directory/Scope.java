
package org.iringtools.directory;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for Scope complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="Scope">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="name" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="applicationData" type="{http://www.iringtools.org/directory}ApplicationData"/>
 *         &lt;element name="dataExchanges" type="{http://www.iringtools.org/directory}DataExchanges"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "Scope", propOrder = {
    "name",
    "applicationData",
    "dataExchanges"
})
public class Scope {

    @XmlElement(required = true)
    protected String name;
    @XmlElement(required = true)
    protected ApplicationData applicationData;
    @XmlElement(required = true)
    protected DataExchanges dataExchanges;

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
     * Gets the value of the applicationData property.
     * 
     * @return
     *     possible object is
     *     {@link ApplicationData }
     *     
     */
    public ApplicationData getApplicationData() {
        return applicationData;
    }

    /**
     * Sets the value of the applicationData property.
     * 
     * @param value
     *     allowed object is
     *     {@link ApplicationData }
     *     
     */
    public void setApplicationData(ApplicationData value) {
        this.applicationData = value;
    }

    /**
     * Gets the value of the dataExchanges property.
     * 
     * @return
     *     possible object is
     *     {@link DataExchanges }
     *     
     */
    public DataExchanges getDataExchanges() {
        return dataExchanges;
    }

    /**
     * Sets the value of the dataExchanges property.
     * 
     * @param value
     *     allowed object is
     *     {@link DataExchanges }
     *     
     */
    public void setDataExchanges(DataExchanges value) {
        this.dataExchanges = value;
    }

}
