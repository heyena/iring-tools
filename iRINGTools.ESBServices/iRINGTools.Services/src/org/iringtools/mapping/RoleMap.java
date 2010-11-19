
package org.iringtools.mapping;

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
 *         &lt;element name="type" type="{http://www.iringtools.org/mapping}RoleType"/>
 *         &lt;element name="roleId" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="name" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="dataType" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="value" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="propertyName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="valueListName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="classMap" type="{http://www.iringtools.org/mapping}ClassMap"/>
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
    "value",
    "propertyName",
    "valueListName",
    "classMap"
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
    protected String value;
    @XmlElement(required = true)
    protected String propertyName;
    @XmlElement(required = true)
    protected String valueListName;
    @XmlElement(required = true)
    protected ClassMap classMap;

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
     * Gets the value of the valueListName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getValueListName() {
        return valueListName;
    }

    /**
     * Sets the value of the valueListName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setValueListName(String value) {
        this.valueListName = value;
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

}
