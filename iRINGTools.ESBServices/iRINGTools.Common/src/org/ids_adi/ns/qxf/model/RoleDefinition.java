
package org.ids_adi.ns.qxf.model;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for RoleDefinition complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="RoleDefinition">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="restriction" type="{http://ns.ids-adi.org/qxf/model#}Restriction" maxOccurs="unbounded"/>
 *         &lt;element name="name" type="{http://ns.ids-adi.org/qxf/model#}Name" maxOccurs="unbounded"/>
 *         &lt;element name="suggested-designation" type="{http://www.w3.org/2001/XMLSchema}string" maxOccurs="unbounded"/>
 *         &lt;element name="designation" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="description" type="{http://ns.ids-adi.org/qxf/model#}Description" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *       &lt;attribute name="id" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="range" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="minimum" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="maximum" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="inverse-minimum" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="inverse-maximum" type="{http://www.w3.org/2001/XMLSchema}string" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "RoleDefinition", propOrder = {
    "restrictions",
    "names",
    "suggestedDesignations",
    "designation",
    "descriptions"
})
public class RoleDefinition {

    @XmlElement(name = "restriction", required = true)
    protected List<Restriction> restrictions;
    @XmlElement(name = "name", required = true)
    protected List<Name> names;
    @XmlElement(name = "suggested-designation", required = true)
    protected List<String> suggestedDesignations;
    @XmlElement(required = true)
    protected String designation;
    @XmlElement(name = "description", required = true)
    protected List<Description> descriptions;
    @XmlAttribute(name = "id")
    protected String id;
    @XmlAttribute(name = "range")
    protected String range;
    @XmlAttribute(name = "minimum")
    protected String minimum;
    @XmlAttribute(name = "maximum")
    protected String maximum;
    @XmlAttribute(name = "inverse-minimum")
    protected String inverseMinimum;
    @XmlAttribute(name = "inverse-maximum")
    protected String inverseMaximum;

    /**
     * Gets the value of the restrictions property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the restrictions property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getRestrictions().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Restriction }
     * 
     * 
     */
    public List<Restriction> getRestrictions() {
        if (restrictions == null) {
            restrictions = new ArrayList<Restriction>();
        }
        return this.restrictions;
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
     * Gets the value of the range property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getRange() {
        return range;
    }

    /**
     * Sets the value of the range property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setRange(String value) {
        this.range = value;
    }

    /**
     * Gets the value of the minimum property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getMinimum() {
        return minimum;
    }

    /**
     * Sets the value of the minimum property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setMinimum(String value) {
        this.minimum = value;
    }

    /**
     * Gets the value of the maximum property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getMaximum() {
        return maximum;
    }

    /**
     * Sets the value of the maximum property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setMaximum(String value) {
        this.maximum = value;
    }

    /**
     * Gets the value of the inverseMinimum property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getInverseMinimum() {
        return inverseMinimum;
    }

    /**
     * Sets the value of the inverseMinimum property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setInverseMinimum(String value) {
        this.inverseMinimum = value;
    }

    /**
     * Gets the value of the inverseMaximum property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getInverseMaximum() {
        return inverseMaximum;
    }

    /**
     * Sets the value of the inverseMaximum property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setInverseMaximum(String value) {
        this.inverseMaximum = value;
    }

    /**
     * Sets the value of the restrictions property.
     * 
     * @param restrictions
     *     allowed object is
     *     {@link Restriction }
     *     
     */
    public void setRestrictions(List<Restriction> restrictions) {
        this.restrictions = restrictions;
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

}
