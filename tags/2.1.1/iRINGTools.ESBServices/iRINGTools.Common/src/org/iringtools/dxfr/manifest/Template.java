
package org.iringtools.dxfr.manifest;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for Template complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="Template">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="id" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="name" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="roles" type="{http://www.iringtools.org/dxfr/manifest}Roles"/>
 *         &lt;element name="transferOption" type="{http://www.iringtools.org/dxfr/manifest}TransferOption"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "Template", propOrder = {
    "id",
    "name",
    "roles",
    "transferOption"
})
public class Template {

    @XmlElement(required = true)
    protected String id;
    @XmlElement(required = true)
    protected String name;
    @XmlElement(required = true)
    protected Roles roles;
    @XmlElement(required = true, defaultValue = "Desired")
    protected TransferOption transferOption;

    /**
     * Gets the value of the id property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getId() {
        return id;
    }

    /**
     * Sets the value of the id property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setId(String value) {
        this.id = value;
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
     * Gets the value of the roles property.
     * 
     * @return
     *     possible object is
     *     {@link Roles }
     *     
     */
    public Roles getRoles() {
        return roles;
    }

    /**
     * Sets the value of the roles property.
     * 
     * @param value
     *     allowed object is
     *     {@link Roles }
     *     
     */
    public void setRoles(Roles value) {
        this.roles = value;
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
