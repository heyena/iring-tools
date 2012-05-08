
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
 *         &lt;element ref="{http://www.iringtools.org/library}propertyMaps"/>
 *         &lt;element name="relatedObjectName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="relationshipName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="relationshipType" type="{http://www.iringtools.org/library}RelationshipType"/>
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
    "propertyMaps",
    "relatedObjectName",
    "relationshipName",
    "relationshipType"
})
@XmlRootElement(name = "dataRelationship")
public class DataRelationship {

    @XmlElement(required = true)
    protected PropertyMaps propertyMaps;
    @XmlElement(required = true)
    protected String relatedObjectName;
    @XmlElement(required = true)
    protected String relationshipName;
    @XmlElement(required = true)
    protected RelationshipType relationshipType;

    /**
     * Gets the value of the propertyMaps property.
     * 
     * @return
     *     possible object is
     *     {@link PropertyMaps }
     *     
     */
    public PropertyMaps getPropertyMaps() {
        return propertyMaps;
    }

    /**
     * Sets the value of the propertyMaps property.
     * 
     * @param value
     *     allowed object is
     *     {@link PropertyMaps }
     *     
     */
    public void setPropertyMaps(PropertyMaps value) {
        this.propertyMaps = value;
    }

    /**
     * Gets the value of the relatedObjectName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getRelatedObjectName() {
        return relatedObjectName;
    }

    /**
     * Sets the value of the relatedObjectName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setRelatedObjectName(String value) {
        this.relatedObjectName = value;
    }

    /**
     * Gets the value of the relationshipName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getRelationshipName() {
        return relationshipName;
    }

    /**
     * Sets the value of the relationshipName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setRelationshipName(String value) {
        this.relationshipName = value;
    }

    /**
     * Gets the value of the relationshipType property.
     * 
     * @return
     *     possible object is
     *     {@link RelationshipType }
     *     
     */
    public RelationshipType getRelationshipType() {
        return relationshipType;
    }

    /**
     * Sets the value of the relationshipType property.
     * 
     * @param value
     *     allowed object is
     *     {@link RelationshipType }
     *     
     */
    public void setRelationshipType(RelationshipType value) {
        this.relationshipType = value;
    }

}
