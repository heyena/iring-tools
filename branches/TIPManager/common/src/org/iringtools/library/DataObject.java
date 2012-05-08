
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
 *         &lt;element name="tableName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="objectNamespace" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="objectName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="keyDelimeter" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element ref="{http://www.iringtools.org/library}keyProperties"/>
 *         &lt;element ref="{http://www.iringtools.org/library}dataProperties"/>
 *         &lt;element ref="{http://www.iringtools.org/library}dataRelationships" minOccurs="0"/>
 *         &lt;element name="isReadOnly" type="{http://www.w3.org/2001/XMLSchema}boolean" minOccurs="0"/>
 *         &lt;element name="hasContent" type="{http://www.w3.org/2001/XMLSchema}boolean" minOccurs="0"/>
 *         &lt;element name="isListOnly" type="{http://www.w3.org/2001/XMLSchema}boolean" minOccurs="0"/>
 *         &lt;element name="defaultProjectionFormat" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="defaultListProjectionFormat" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="description" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="isRelatedOnly" type="{http://www.w3.org/2001/XMLSchema}boolean" minOccurs="0"/>
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
    "tableName",
    "objectNamespace",
    "objectName",
    "keyDelimeter",
    "keyProperties",
    "dataProperties",
    "dataRelationships",
    "isReadOnly",
    "hasContent",
    "isListOnly",
    "defaultProjectionFormat",
    "defaultListProjectionFormat",
    "description",
    "isRelatedOnly"
})
@XmlRootElement(name = "dataObject")
public class DataObject {

    @XmlElement(required = true)
    protected String tableName;
    protected String objectNamespace;
    @XmlElement(required = true)
    protected String objectName;
    protected String keyDelimeter;
    @XmlElement(required = true)
    protected KeyProperties keyProperties;
    @XmlElement(required = true)
    protected DataProperties dataProperties;
    protected DataRelationships dataRelationships;
    protected Boolean isReadOnly;
    protected Boolean hasContent;
    protected Boolean isListOnly;
    protected String defaultProjectionFormat;
    protected String defaultListProjectionFormat;
    protected String description;
    protected Boolean isRelatedOnly;

    /**
     * Gets the value of the tableName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getTableName() {
        return tableName;
    }

    /**
     * Sets the value of the tableName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setTableName(String value) {
        this.tableName = value;
    }

    /**
     * Gets the value of the objectNamespace property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getObjectNamespace() {
        return objectNamespace;
    }

    /**
     * Sets the value of the objectNamespace property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setObjectNamespace(String value) {
        this.objectNamespace = value;
    }

    /**
     * Gets the value of the objectName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getObjectName() {
        return objectName;
    }

    /**
     * Sets the value of the objectName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setObjectName(String value) {
        this.objectName = value;
    }

    /**
     * Gets the value of the keyDelimeter property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getKeyDelimeter() {
        return keyDelimeter;
    }

    /**
     * Sets the value of the keyDelimeter property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setKeyDelimeter(String value) {
        this.keyDelimeter = value;
    }

    /**
     * Gets the value of the keyProperties property.
     * 
     * @return
     *     possible object is
     *     {@link KeyProperties }
     *     
     */
    public KeyProperties getKeyProperties() {
        return keyProperties;
    }

    /**
     * Sets the value of the keyProperties property.
     * 
     * @param value
     *     allowed object is
     *     {@link KeyProperties }
     *     
     */
    public void setKeyProperties(KeyProperties value) {
        this.keyProperties = value;
    }

    /**
     * Gets the value of the dataProperties property.
     * 
     * @return
     *     possible object is
     *     {@link DataProperties }
     *     
     */
    public DataProperties getDataProperties() {
        return dataProperties;
    }

    /**
     * Sets the value of the dataProperties property.
     * 
     * @param value
     *     allowed object is
     *     {@link DataProperties }
     *     
     */
    public void setDataProperties(DataProperties value) {
        this.dataProperties = value;
    }

    /**
     * Gets the value of the dataRelationships property.
     * 
     * @return
     *     possible object is
     *     {@link DataRelationships }
     *     
     */
    public DataRelationships getDataRelationships() {
        return dataRelationships;
    }

    /**
     * Sets the value of the dataRelationships property.
     * 
     * @param value
     *     allowed object is
     *     {@link DataRelationships }
     *     
     */
    public void setDataRelationships(DataRelationships value) {
        this.dataRelationships = value;
    }

    /**
     * Gets the value of the isReadOnly property.
     * This getter has been renamed from isIsReadOnly() to getIsReadOnly() by cxf-xjc-boolean plugin.
     * 
     * @return
     *     possible object is
     *     {@link Boolean }
     *     
     */
    public Boolean getIsReadOnly() {
        return isReadOnly;
    }

    /**
     * Sets the value of the isReadOnly property.
     * 
     * @param value
     *     allowed object is
     *     {@link Boolean }
     *     
     */
    public void setIsReadOnly(Boolean value) {
        this.isReadOnly = value;
    }

    /**
     * Gets the value of the hasContent property.
     * This getter has been renamed from isHasContent() to getHasContent() by cxf-xjc-boolean plugin.
     * 
     * @return
     *     possible object is
     *     {@link Boolean }
     *     
     */
    public Boolean getHasContent() {
        return hasContent;
    }

    /**
     * Sets the value of the hasContent property.
     * 
     * @param value
     *     allowed object is
     *     {@link Boolean }
     *     
     */
    public void setHasContent(Boolean value) {
        this.hasContent = value;
    }

    /**
     * Gets the value of the isListOnly property.
     * This getter has been renamed from isIsListOnly() to getIsListOnly() by cxf-xjc-boolean plugin.
     * 
     * @return
     *     possible object is
     *     {@link Boolean }
     *     
     */
    public Boolean getIsListOnly() {
        return isListOnly;
    }

    /**
     * Sets the value of the isListOnly property.
     * 
     * @param value
     *     allowed object is
     *     {@link Boolean }
     *     
     */
    public void setIsListOnly(Boolean value) {
        this.isListOnly = value;
    }

    /**
     * Gets the value of the defaultProjectionFormat property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getDefaultProjectionFormat() {
        return defaultProjectionFormat;
    }

    /**
     * Sets the value of the defaultProjectionFormat property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setDefaultProjectionFormat(String value) {
        this.defaultProjectionFormat = value;
    }

    /**
     * Gets the value of the defaultListProjectionFormat property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getDefaultListProjectionFormat() {
        return defaultListProjectionFormat;
    }

    /**
     * Sets the value of the defaultListProjectionFormat property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setDefaultListProjectionFormat(String value) {
        this.defaultListProjectionFormat = value;
    }

    /**
     * Gets the value of the description property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getDescription() {
        return description;
    }

    /**
     * Sets the value of the description property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setDescription(String value) {
        this.description = value;
    }

    /**
     * Gets the value of the isRelatedOnly property.
     * This getter has been renamed from isIsRelatedOnly() to getIsRelatedOnly() by cxf-xjc-boolean plugin.
     * 
     * @return
     *     possible object is
     *     {@link Boolean }
     *     
     */
    public Boolean getIsRelatedOnly() {
        return isRelatedOnly;
    }

    /**
     * Sets the value of the isRelatedOnly property.
     * 
     * @param value
     *     allowed object is
     *     {@link Boolean }
     *     
     */
    public void setIsRelatedOnly(Boolean value) {
        this.isRelatedOnly = value;
    }

}
