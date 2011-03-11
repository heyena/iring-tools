
package org.ids_adi.ns.qxf.model;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for TemplateDefinition complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="TemplateDefinition">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="repository" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="suggested-designation" type="{http://www.w3.org/2001/XMLSchema}string" maxOccurs="unbounded"/>
 *         &lt;element name="designation" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="name" type="{http://ns.ids-adi.org/qxf/model#}Name" maxOccurs="unbounded"/>
 *         &lt;element name="description" type="{http://ns.ids-adi.org/qxf/model#}Description" maxOccurs="unbounded"/>
 *         &lt;element name="textual-definition" type="{http://ns.ids-adi.org/qxf/model#}TextualDefinition" maxOccurs="unbounded"/>
 *         &lt;element name="status" type="{http://ns.ids-adi.org/qxf/model#}Status" maxOccurs="unbounded"/>
 *         &lt;element name="role-definition" type="{http://ns.ids-adi.org/qxf/model#}RoleDefinition" maxOccurs="unbounded"/>
 *         &lt;element name="specialization" type="{http://ns.ids-adi.org/qxf/model#}Specialization" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *       &lt;attribute name="id" type="{http://www.w3.org/2001/XMLSchema}string" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "TemplateDefinition", propOrder = {
    "repository",
    "suggestedDesignations",
    "designation",
    "names",
    "descriptions",
    "textualDefinitions",
    "statuses",
    "roleDefinitions",
    "specializations"
})
public class TemplateDefinition {

    @XmlElement(required = true)
    protected String repository;
    @XmlElement(name = "suggested-designation", required = true)
    protected List<String> suggestedDesignations;
    @XmlElement(required = true)
    protected String designation;
    @XmlElement(name = "name", required = true)
    protected List<Name> names;
    @XmlElement(name = "description", required = true)
    protected List<Description> descriptions;
    @XmlElement(name = "textual-definition", required = true)
    protected List<TextualDefinition> textualDefinitions;
    @XmlElement(name = "status", required = true)
    protected List<Status> statuses;
    @XmlElement(name = "role-definition", required = true)
    protected List<RoleDefinition> roleDefinitions;
    @XmlElement(name = "specialization", required = true)
    protected List<Specialization> specializations;
    @XmlAttribute(name = "id")
    protected String id;

    /**
     * Gets the value of the repository property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getRepository() {
        return repository;
    }

    /**
     * Sets the value of the repository property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setRepository(String value) {
        this.repository = value;
    }

    /**
     * Gets the value of the suggestedDesignations property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the suggestedDesignations property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getSuggestedDesignations().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link String }
     * 
     * 
     */
    public List<String> getSuggestedDesignations() {
        if (suggestedDesignations == null) {
            suggestedDesignations = new ArrayList<String>();
        }
        return this.suggestedDesignations;
    }

    /**
     * Gets the value of the designation property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getDesignation() {
        return designation;
    }

    /**
     * Sets the value of the designation property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setDesignation(String value) {
        this.designation = value;
    }

    /**
     * Gets the value of the names property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the names property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getNames().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Name }
     * 
     * 
     */
    public List<Name> getNames() {
        if (names == null) {
            names = new ArrayList<Name>();
        }
        return this.names;
    }

    /**
     * Gets the value of the descriptions property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the descriptions property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getDescriptions().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Description }
     * 
     * 
     */
    public List<Description> getDescriptions() {
        if (descriptions == null) {
            descriptions = new ArrayList<Description>();
        }
        return this.descriptions;
    }

    /**
     * Gets the value of the textualDefinitions property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the textualDefinitions property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getTextualDefinitions().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link TextualDefinition }
     * 
     * 
     */
    public List<TextualDefinition> getTextualDefinitions() {
        if (textualDefinitions == null) {
            textualDefinitions = new ArrayList<TextualDefinition>();
        }
        return this.textualDefinitions;
    }

    /**
     * Gets the value of the statuses property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the statuses property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getStatuses().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Status }
     * 
     * 
     */
    public List<Status> getStatuses() {
        if (statuses == null) {
            statuses = new ArrayList<Status>();
        }
        return this.statuses;
    }

    /**
     * Gets the value of the roleDefinitions property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the roleDefinitions property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getRoleDefinitions().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link RoleDefinition }
     * 
     * 
     */
    public List<RoleDefinition> getRoleDefinitions() {
        if (roleDefinitions == null) {
            roleDefinitions = new ArrayList<RoleDefinition>();
        }
        return this.roleDefinitions;
    }

    /**
     * Gets the value of the specializations property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the specializations property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getSpecializations().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Specialization }
     * 
     * 
     */
    public List<Specialization> getSpecializations() {
        if (specializations == null) {
            specializations = new ArrayList<Specialization>();
        }
        return this.specializations;
    }

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
     * Sets the value of the suggestedDesignations property.
     * 
     * @param suggestedDesignations
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSuggestedDesignations(List<String> suggestedDesignations) {
        this.suggestedDesignations = suggestedDesignations;
    }

    /**
     * Sets the value of the names property.
     * 
     * @param names
     *     allowed object is
     *     {@link Name }
     *     
     */
    public void setNames(List<Name> names) {
        this.names = names;
    }

    /**
     * Sets the value of the descriptions property.
     * 
     * @param descriptions
     *     allowed object is
     *     {@link Description }
     *     
     */
    public void setDescriptions(List<Description> descriptions) {
        this.descriptions = descriptions;
    }

    /**
     * Sets the value of the textualDefinitions property.
     * 
     * @param textualDefinitions
     *     allowed object is
     *     {@link TextualDefinition }
     *     
     */
    public void setTextualDefinitions(List<TextualDefinition> textualDefinitions) {
        this.textualDefinitions = textualDefinitions;
    }

    /**
     * Sets the value of the statuses property.
     * 
     * @param statuses
     *     allowed object is
     *     {@link Status }
     *     
     */
    public void setStatuses(List<Status> statuses) {
        this.statuses = statuses;
    }

    /**
     * Sets the value of the roleDefinitions property.
     * 
     * @param roleDefinitions
     *     allowed object is
     *     {@link RoleDefinition }
     *     
     */
    public void setRoleDefinitions(List<RoleDefinition> roleDefinitions) {
        this.roleDefinitions = roleDefinitions;
    }

    /**
     * Sets the value of the specializations property.
     * 
     * @param specializations
     *     allowed object is
     *     {@link Specialization }
     *     
     */
    public void setSpecializations(List<Specialization> specializations) {
        this.specializations = specializations;
    }

}
