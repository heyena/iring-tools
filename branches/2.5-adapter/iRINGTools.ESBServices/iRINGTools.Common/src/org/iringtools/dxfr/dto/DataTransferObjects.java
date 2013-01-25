
package org.iringtools.dxfr.dto;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;
import org.iringtools.dxfr.response.ExchangeResponse;


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
 *         &lt;element name="scopeName" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="appName" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="dataTransferObjectList" type="{http://www.iringtools.org/dxfr/dto}DataTransferObjectList" minOccurs="0"/>
 *         &lt;element name="version" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="senderScopeName" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="senderAppName" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="summary" type="{http://www.iringtools.org/dxfr/response}ExchangeResponse" minOccurs="0"/>
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
    "dataTransferObjectList",
    "version",
    "senderScopeName",
    "senderAppName",
    "summary"
})
@XmlRootElement(name = "dataTransferObjects")
public class DataTransferObjects {

    protected String scopeName;
    protected String appName;
    protected DataTransferObjectList dataTransferObjectList;
    protected String version;
    protected String senderScopeName;
    protected String senderAppName;
    protected ExchangeResponse summary;

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

    /**
     * Gets the value of the version property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getVersion() {
        return version;
    }

    /**
     * Sets the value of the version property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setVersion(String value) {
        this.version = value;
    }

    /**
     * Gets the value of the senderScopeName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSenderScopeName() {
        return senderScopeName;
    }

    /**
     * Sets the value of the senderScopeName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSenderScopeName(String value) {
        this.senderScopeName = value;
    }

    /**
     * Gets the value of the senderAppName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSenderAppName() {
        return senderAppName;
    }

    /**
     * Sets the value of the senderAppName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSenderAppName(String value) {
        this.senderAppName = value;
    }

    /**
     * Gets the value of the summary property.
     * 
     * @return
     *     possible object is
     *     {@link ExchangeResponse }
     *     
     */
    public ExchangeResponse getSummary() {
        return summary;
    }

    /**
     * Sets the value of the summary property.
     * 
     * @param value
     *     allowed object is
     *     {@link ExchangeResponse }
     *     
     */
    public void setSummary(ExchangeResponse value) {
        this.summary = value;
    }

}
