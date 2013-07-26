
package org.iringtools.dxfr.dto;

import java.io.Serializable;
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
 *         &lt;element name="type" type="{http://www.iringtools.org/dxfr/dto}RoleType" minOccurs="0"/>
 *         &lt;element name="roleId" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="name" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="dataType" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="oldValue" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="value" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="relatedClassId" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="relatedClassName" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="hasValueMap" type="{http://www.w3.org/2001/XMLSchema}boolean" minOccurs="0"/>
 *         &lt;element name="values" type="{http://www.iringtools.org/dxfr/dto}RoleValues" minOccurs="0"/>
 *         &lt;element name="oldValues" type="{http://www.iringtools.org/dxfr/dto}RoleValues" minOccurs="0"/>
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
    "type",
    "roleId",
    "name",
    "dataType",
    "oldValue",
    "value",
    "relatedClassId",
    "relatedClassName",
    "hasValueMap",
    "values",
    "oldValues"
})
@XmlRootElement(name = "roleObject")
public class RoleObject
    implements Serializable
{

    private final static long serialVersionUID = 1L;
    protected RoleType type;
    @XmlElement(required = true)
    protected String roleId;
    @XmlElement(required = true)
    protected String name;
    protected String dataType;
    protected String oldValue;
    protected String value;
    protected String relatedClassId;
    protected String relatedClassName;
    protected Boolean hasValueMap;
    protected RoleValues values;
    protected RoleValues oldValues;

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
     * Gets the value of the oldValue property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getOldValue() {
        return oldValue;
    }

    /**
     * Sets the value of the oldValue property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setOldValue(String value) {
        this.oldValue = value;
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
     * Gets the value of the relatedClassId property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getRelatedClassId() {
        return relatedClassId;
    }

    /**
     * Sets the value of the relatedClassId property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setRelatedClassId(String value) {
        this.relatedClassId = value;
    }

    /**
     * Gets the value of the relatedClassName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getRelatedClassName() {
        return relatedClassName;
    }

    /**
     * Sets the value of the relatedClassName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setRelatedClassName(String value) {
        this.relatedClassName = value;
    }

    /**
     * Gets the value of the hasValueMap property.
     * This getter has been renamed from isHasValueMap() to getHasValueMap() by cxf-xjc-boolean plugin.
     * 
     * @return
     *     possible object is
     *     {@link Boolean }
     *     
     */
    public Boolean getHasValueMap() {
        return hasValueMap;
    }

    /**
     * Sets the value of the hasValueMap property.
     * 
     * @param value
     *     allowed object is
     *     {@link Boolean }
     *     
     */
    public void setHasValueMap(Boolean value) {
        this.hasValueMap = value;
    }

    /**
     * Gets the value of the values property.
     * 
     * @return
     *     possible object is
     *     {@link RoleValues }
     *     
     */
    public RoleValues getValues() {
        return values;
    }

    /**
     * Sets the value of the values property.
     * 
     * @param value
     *     allowed object is
     *     {@link RoleValues }
     *     
     */
    public void setValues(RoleValues value) {
        this.values = value;
    }

    /**
     * Gets the value of the oldValues property.
     * 
     * @return
     *     possible object is
     *     {@link RoleValues }
     *     
     */
    public RoleValues getOldValues() {
        return oldValues;
    }

    /**
     * Sets the value of the oldValues property.
     * 
     * @param value
     *     allowed object is
     *     {@link RoleValues }
     *     
     */
    public void setOldValues(RoleValues value) {
        this.oldValues = value;
    }

}
