
package org.iringtools.dxfr.dti;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for DataTransferIndices complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="DataTransferIndices">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="scopeName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="appName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="dataTransferIndexList" type="{http://www.iringtools.org/dxfr/dti}DataTransferIndexList"/>
 *         &lt;element name="sortType" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="sortOrder" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "DataTransferIndices", propOrder = {
    "scopeName",
    "appName",
    "dataTransferIndexList",
    "sortType",
    "sortOrder"
})
@XmlRootElement(name = "dataTransferIndices")
public class DataTransferIndices {

    @XmlElement(required = true)
    protected String scopeName;
    @XmlElement(required = true)
    protected String appName;
    @XmlElement(required = true)
    protected DataTransferIndexList dataTransferIndexList;
    @XmlElement(required = true)
    protected String sortType;
    @XmlElement(required = true)
    protected String sortOrder;

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
     * Gets the value of the dataTransferIndexList property.
     * 
     * @return
     *     possible object is
     *     {@link DataTransferIndexList }
     *     
     */
    public DataTransferIndexList getDataTransferIndexList() {
        return dataTransferIndexList;
    }

    /**
     * Sets the value of the dataTransferIndexList property.
     * 
     * @param value
     *     allowed object is
     *     {@link DataTransferIndexList }
     *     
     */
    public void setDataTransferIndexList(DataTransferIndexList value) {
        this.dataTransferIndexList = value;
    }

    /**
     * Gets the value of the sortType property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSortType() {
        return sortType;
    }

    /**
     * Sets the value of the sortType property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSortType(String value) {
        this.sortType = value;
    }

    /**
     * Gets the value of the sortOrder property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSortOrder() {
        return sortOrder;
    }

    /**
     * Sets the value of the sortOrder property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSortOrder(String value) {
        this.sortOrder = value;
    }

}
