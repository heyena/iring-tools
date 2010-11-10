
package org.iringtools.dxfr.dto;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for DataTransferObjects complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="DataTransferObjects">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="scopeName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="appName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="dataTransferObjectList" type="{http://www.iringtools.org/dxfr/dto}DataTransferObjectList"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "DataTransferObjects", propOrder = {
    "scopeName",
    "appName",
    "dataTransferObjectList"
})
@XmlRootElement(name = "dataTransferObjects")
public class DataTransferObjects {

    @XmlElement(required = true)
    protected String scopeName;
    @XmlElement(required = true)
    protected String appName;
    @XmlElement(required = true)
    protected DataTransferObjectList dataTransferObjectList;

    /**
     * Gets the value of the scopeName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getScopeName() {
        return scopeName;
    }

    /**
     * Sets the value of the scopeName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setScopeName(String value) {
        this.scopeName = value;
    }

    /**
     * Gets the value of the appName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getAppName() {
        return appName;
    }

    /**
     * Sets the value of the appName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setAppName(String value) {
        this.appName = value;
    }

    /**
     * Gets the value of the dataTransferObjectList property.
     * 
     * @return
     *     possible object is
     *     {@link DataTransferObjectList }
     *     
     */
    public DataTransferObjectList getDataTransferObjectList() {
        return dataTransferObjectList;
    }

    /**
     * Sets the value of the dataTransferObjectList property.
     * 
     * @param value
     *     allowed object is
     *     {@link DataTransferObjectList }
     *     
     */
    public void setDataTransferObjectList(DataTransferObjectList value) {
        this.dataTransferObjectList = value;
    }

}
