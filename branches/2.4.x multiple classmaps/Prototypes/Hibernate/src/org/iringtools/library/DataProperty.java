
package org.iringtools.library;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for DataProperty complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="DataProperty">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="columnName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="propertyName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="dataType" type="{http://www.iringtools.org/library}DataType"/>
 *         &lt;element name="dataLength" type="{http://www.w3.org/2001/XMLSchema}int"/>
 *         &lt;element name="isNullable" type="{http://www.w3.org/2001/XMLSchema}boolean"/>
 *         &lt;element name="keyType" type="{http://www.iringtools.org/library}KeyType"/>
 *         &lt;element name="showOnIndex" type="{http://www.w3.org/2001/XMLSchema}boolean"/>
 *         &lt;element name="numberOfDecimals" type="{http://www.w3.org/2001/XMLSchema}int"/>
 *         &lt;element name="name" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="description" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "DataProperty", propOrder = {
    "columnName",
    "propertyName",
    "dataType",
    "dataLength",
    "isNullable",
    "keyType",
    "showOnIndex",
    "numberOfDecimals",
    "name",
    "description"
})
public class DataProperty {

    @XmlElement(required = true)
    protected String columnName;
    @XmlElement(required = true)
    protected String propertyName;
    @XmlElement(required = true)
    protected DataType dataType;
    protected int dataLength;
    protected boolean isNullable;
    @XmlElement(required = true)
    protected KeyType keyType;
    protected boolean showOnIndex;
    protected int numberOfDecimals;
    @XmlElement(required = true)
    protected String name;
    @XmlElement(required = true)
    protected String description;

    /**
     * Gets the value of the columnName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getColumnName() {
        return columnName;
    }

    /**
     * Sets the value of the columnName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setColumnName(String value) {
        this.columnName = value;
    }

    /**
     * Gets the value of the propertyName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getPropertyName() {
        return propertyName;
    }

    /**
     * Sets the value of the propertyName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setPropertyName(String value) {
        this.propertyName = value;
    }

    /**
     * Gets the value of the dataType property.
     * 
     * @return
     *     possible object is
     *     {@link DataType }
     *     
     */
    public DataType getDataType() {
        return dataType;
    }

    /**
     * Sets the value of the dataType property.
     * 
     * @param value
     *     allowed object is
     *     {@link DataType }
     *     
     */
    public void setDataType(DataType value) {
        this.dataType = value;
    }

    /**
     * Gets the value of the dataLength property.
     * 
     */
    public int getDataLength() {
        return dataLength;
    }

    /**
     * Sets the value of the dataLength property.
     * 
     */
    public void setDataLength(int value) {
        this.dataLength = value;
    }

    /**
     * Gets the value of the isNullable property.
     * This getter has been renamed from isIsNullable() to getIsNullable() by cxf-xjc-boolean plugin.
     * 
     */
    public boolean getIsNullable() {
        return isNullable;
    }

    /**
     * Sets the value of the isNullable property.
     * 
     */
    public void setIsNullable(boolean value) {
        this.isNullable = value;
    }

    /**
     * Gets the value of the keyType property.
     * 
     * @return
     *     possible object is
     *     {@link KeyType }
     *     
     */
    public KeyType getKeyType() {
        return keyType;
    }

    /**
     * Sets the value of the keyType property.
     * 
     * @param value
     *     allowed object is
     *     {@link KeyType }
     *     
     */
    public void setKeyType(KeyType value) {
        this.keyType = value;
    }

    /**
     * Gets the value of the showOnIndex property.
     * This getter has been renamed from isShowOnIndex() to getShowOnIndex() by cxf-xjc-boolean plugin.
     * 
     */
    public boolean getShowOnIndex() {
        return showOnIndex;
    }

    /**
     * Sets the value of the showOnIndex property.
     * 
     */
    public void setShowOnIndex(boolean value) {
        this.showOnIndex = value;
    }

    /**
     * Gets the value of the numberOfDecimals property.
     * 
     */
    public int getNumberOfDecimals() {
        return numberOfDecimals;
    }

    /**
     * Sets the value of the numberOfDecimals property.
     * 
     */
    public void setNumberOfDecimals(int value) {
        this.numberOfDecimals = value;
    }

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

}
