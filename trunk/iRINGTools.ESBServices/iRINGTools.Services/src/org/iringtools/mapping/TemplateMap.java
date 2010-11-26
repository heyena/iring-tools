
package org.iringtools.mapping;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for TemplateMap complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="TemplateMap">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="templateId" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="templateType" type="{http://www.iringtools.org/mapping}TemplateType"/>
 *         &lt;element name="name" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="roleMaps" type="{http://www.iringtools.org/mapping}RoleMaps"/>
 *         &lt;element name="transferOption" type="{http://www.iringtools.org/mapping}TransferOption"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "TemplateMap", propOrder = {
    "templateId",
    "templateType",
    "name",
    "roleMaps",
    "transferOption"
})
public class TemplateMap {

    @XmlElement(required = true)
    protected String templateId;
    @XmlElement(required = true)
    protected TemplateType templateType;
    @XmlElement(required = true)
    protected String name;
    @XmlElement(required = true)
    protected RoleMaps roleMaps;
    @XmlElement(required = true, defaultValue = "Desired")
    protected TransferOption transferOption;

    /**
     * Gets the value of the templateId property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getTemplateId() {
        return templateId;
    }

    /**
     * Sets the value of the templateId property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setTemplateId(String value) {
        this.templateId = value;
    }

    /**
     * Gets the value of the templateType property.
     * 
     * @return
     *     possible object is
     *     {@link TemplateType }
     *     
     */
    public TemplateType getTemplateType() {
        return templateType;
    }

    /**
     * Sets the value of the templateType property.
     * 
     * @param value
     *     allowed object is
     *     {@link TemplateType }
     *     
     */
    public void setTemplateType(TemplateType value) {
        this.templateType = value;
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
     * Gets the value of the roleMaps property.
     * 
     * @return
     *     possible object is
     *     {@link RoleMaps }
     *     
     */
    public RoleMaps getRoleMaps() {
        return roleMaps;
    }

    /**
     * Sets the value of the roleMaps property.
     * 
     * @param value
     *     allowed object is
     *     {@link RoleMaps }
     *     
     */
    public void setRoleMaps(RoleMaps value) {
        this.roleMaps = value;
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
