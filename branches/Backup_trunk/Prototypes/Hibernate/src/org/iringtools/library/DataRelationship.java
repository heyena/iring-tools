
package org.iringtools.library;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for DataRelationship complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="DataRelationship">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="propertyMaps" type="{http://www.iringtools.org/library}PropertyMap" maxOccurs="unbounded"/>
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
@XmlType(name = "DataRelationship", propOrder = {
    "propertyMaps",
    "relatedObjectName",
    "relationshipName",
    "relationshipType"
})
public class DataRelationship {

    @XmlElement(required = true)
    protected List<PropertyMap> propertyMaps;
    @XmlElement(required = true)
    protected String relatedObjectName;
    @XmlElement(required = true)
    protected String relationshipName;
    @XmlElement(required = true)
    protected RelationshipType relationshipType;

    /**
     * Gets the value of the propertyMaps property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the propertyMaps property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getPropertyMaps().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link PropertyMap }
     * 
     * 
     */
    public List<PropertyMap> getPropertyMaps() {
        if (propertyMaps == null) {
            propertyMaps = new ArrayList<PropertyMap>();
        }
        return this.propertyMaps;
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

    /**
     * Sets the value of the propertyMaps property.
     * 
     * @param propertyMaps
     *     allowed object is
     *     {@link PropertyMap }
     *     
     */
    public void setPropertyMaps(List<PropertyMap> propertyMaps) {
        this.propertyMaps = propertyMaps;
    }

}
