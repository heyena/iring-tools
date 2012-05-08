
package org.iringtools.library;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for anonymous complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType>
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element ref="{http://www.iringtools.org/library}dataObjects"/>
 *         &lt;element name="enableSearch" type="{http://www.w3.org/2001/XMLSchema}boolean"/>
 *         &lt;element name="enableSummary" type="{http://www.w3.org/2001/XMLSchema}boolean"/>
 *         &lt;element name="dataVersion" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "", propOrder = {
    "dataObjects",
    "enableSearch",
    "enableSummary",
    "dataVersion"
})
@XmlRootElement(name = "dataDictionary")
public class DataDictionary {

    @XmlElement(required = true)
    protected DataObjects dataObjects;
    protected boolean enableSearch;
    protected boolean enableSummary;
    @XmlElement(required = true)
    protected String dataVersion;

    /**
     * Gets the value of the dataObjects property.
     * 
     * @return
     *     possible object is
     *     {@link DataObjects }
     *     
     */
    public DataObjects getDataObjects() {
        return dataObjects;
    }

    /**
     * Sets the value of the dataObjects property.
     * 
     * @param value
     *     allowed object is
     *     {@link DataObjects }
     *     
     */
    public void setDataObjects(DataObjects value) {
        this.dataObjects = value;
    }

    /**
     * Gets the value of the enableSearch property.
     * This getter has been renamed from isEnableSearch() to getEnableSearch() by cxf-xjc-boolean plugin.
     * 
     */
    public boolean getEnableSearch() {
        return enableSearch;
    }

    /**
     * Sets the value of the enableSearch property.
     * 
     */
    public void setEnableSearch(boolean value) {
        this.enableSearch = value;
    }

    /**
     * Gets the value of the enableSummary property.
     * This getter has been renamed from isEnableSummary() to getEnableSummary() by cxf-xjc-boolean plugin.
     * 
     */
    public boolean getEnableSummary() {
        return enableSummary;
    }

    /**
     * Sets the value of the enableSummary property.
     * 
     */
    public void setEnableSummary(boolean value) {
        this.enableSummary = value;
    }

    /**
     * Gets the value of the dataVersion property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getDataVersion() {
        return dataVersion;
    }

    /**
     * Sets the value of the dataVersion property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setDataVersion(String value) {
        this.dataVersion = value;
    }

}
