//
// This file was generated by the JavaTM Architecture for XML Binding(JAXB) Reference Implementation, vhudson-jaxb-ri-2.2-7 
// See <a href="http://java.sun.com/xml/jaxb">http://java.sun.com/xml/jaxb</a> 
// Any modifications to this file will be lost upon recompilation of the source schema. 
// Generated on: 2010.07.01 at 04:29:48 PM EDT 
//


package org.iringtools.adapter.library.manifest;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for RoleMap complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="RoleMap">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="type" type="{http://iringtools.org/adapter/library/manifest}RoleType"/>
 *         &lt;element name="roleId" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="name" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="dataType" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="propertyName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="value" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="valueList" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="classMap" type="{http://iringtools.org/adapter/library/manifest}ClassMap"/>
 *         &lt;element name="transferOption" type="{http://iringtools.org/adapter/library/manifest}TransferOption"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "RoleMap", propOrder = {
    "type",
    "roleId",
    "name",
    "dataType",
    "propertyName",
    "value",
    "valueList",
    "classMap",
    "transferOption"
})
public class RoleMap {

    @XmlElement(required = true)
    protected RoleType type;
    @XmlElement(required = true)
    protected String roleId;
    @XmlElement(required = true)
    protected String name;
    @XmlElement(required = true)
    protected String dataType;
    @XmlElement(required = true)
    protected String propertyName;
    @XmlElement(required = true)
    protected String value;
    @XmlElement(required = true)
    protected String valueList;
    @XmlElement(required = true)
    protected ClassMap classMap;
    @XmlElement(required = true, defaultValue = "Desired")
    protected TransferOption transferOption;

    /**
     * Gets the value of the type property.
     * 
     * @return
     *     possible object is
     *     {@link RoleType }
     *     
     */
    public RoleType getType() {
        return type;
    }

    /**
     * Sets the value of the type property.
     * 
     * @param value
     *     allowed object is
     *     {@link RoleType }
     *     
     */
    public void setType(RoleType value) {
        this.type = value;
    }

    /**
     * Gets the value of the roleId property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getRoleId() {
        return roleId;
    }

    /**
     * Sets the value of the roleId property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setRoleId(String value) {
        this.roleId = value;
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
     * Gets the value of the dataType property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getDataType() {
        return dataType;
    }

    /**
     * Sets the value of the dataType property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setDataType(String value) {
        this.dataType = value;
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
     * Gets the value of the value property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getValue() {
        return value;
    }

    /**
     * Sets the value of the value property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setValue(String value) {
        this.value = value;
    }

    /**
     * Gets the value of the valueList property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getValueList() {
        return valueList;
    }

    /**
     * Sets the value of the valueList property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setValueList(String value) {
        this.valueList = value;
    }

    /**
     * Gets the value of the classMap property.
     * 
     * @return
     *     possible object is
     *     {@link ClassMap }
     *     
     */
    public ClassMap getClassMap() {
        return classMap;
    }

    /**
     * Sets the value of the classMap property.
     * 
     * @param value
     *     allowed object is
     *     {@link ClassMap }
     *     
     */
    public void setClassMap(ClassMap value) {
        this.classMap = value;
    }

    /**
     * Gets the value of the transferOption property.
     * 
     * @return
     *     possible object is
     *     {@link TransferOption }
     *     
     */
    public TransferOption getTransferOption() {
        return transferOption;
    }

    /**
     * Sets the value of the transferOption property.
     * 
     * @param value
     *     allowed object is
     *     {@link TransferOption }
     *     
     */
    public void setTransferOption(TransferOption value) {
        this.transferOption = value;
    }

}
