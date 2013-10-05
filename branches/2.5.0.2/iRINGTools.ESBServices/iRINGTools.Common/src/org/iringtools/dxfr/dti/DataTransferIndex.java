
package org.iringtools.dxfr.dti;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for DataTransferIndex complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="DataTransferIndex">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="identifier" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="hashValue" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="transferType" type="{http://www.iringtools.org/dxfr/dti}TransferType"/>
 *         &lt;element name="sortIndex" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="internalIdentifier" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="hasContent" type="{http://www.w3.org/2001/XMLSchema}boolean" minOccurs="0"/>
 *         &lt;element name="duplicateCount" type="{http://www.w3.org/2001/XMLSchema}int" minOccurs="0"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "DataTransferIndex", propOrder = {
    "identifier",
    "hashValue",
    "transferType",
    "sortIndex",
    "internalIdentifier",
    "hasContent",
    "duplicateCount"
})
public class DataTransferIndex {

    @XmlElement(required = true)
    protected String identifier;
    @XmlElement(required = true)
    protected String hashValue;
    @XmlElement(required = true)
    protected TransferType transferType;
    @XmlElement(required = true)
    protected String sortIndex;
    protected String internalIdentifier;
    protected Boolean hasContent;
    protected Integer duplicateCount;

    /**
     * Gets the value of the identifier property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getIdentifier() {
        return identifier;
    }

    /**
     * Sets the value of the identifier property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setIdentifier(String value) {
        this.identifier = value;
    }

    /**
     * Gets the value of the hashValue property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getHashValue() {
        return hashValue;
    }

    /**
     * Sets the value of the hashValue property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setHashValue(String value) {
        this.hashValue = value;
    }

    /**
     * Gets the value of the transferType property.
     * 
     * @return
     *     possible object is
     *     {@link TransferType }
     *     
     */
    public TransferType getTransferType() {
        return transferType;
    }

    /**
     * Sets the value of the transferType property.
     * 
     * @param value
     *     allowed object is
     *     {@link TransferType }
     *     
     */
    public void setTransferType(TransferType value) {
        this.transferType = value;
    }

    /**
     * Gets the value of the sortIndex property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSortIndex() {
        return sortIndex;
    }

    /**
     * Sets the value of the sortIndex property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSortIndex(String value) {
        this.sortIndex = value;
    }

    /**
     * Gets the value of the internalIdentifier property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getInternalIdentifier() {
        return internalIdentifier;
    }

    /**
     * Sets the value of the internalIdentifier property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setInternalIdentifier(String value) {
        this.internalIdentifier = value;
    }

    /**
     * Gets the value of the hasContent property.
     * This getter has been renamed from isHasContent() to getHasContent() by cxf-xjc-boolean plugin.
     * 
     * @return
     *     possible object is
     *     {@link Boolean }
     *     
     */
    public Boolean getHasContent() {
        return hasContent;
    }

    /**
     * Sets the value of the hasContent property.
     * 
     * @param value
     *     allowed object is
     *     {@link Boolean }
     *     
     */
    public void setHasContent(Boolean value) {
        this.hasContent = value;
    }

    /**
     * Gets the value of the duplicateCount property.
     * 
     * @return
     *     possible object is
     *     {@link Integer }
     *     
     */
    public Integer getDuplicateCount() {
        return duplicateCount;
    }

    /**
     * Sets the value of the duplicateCount property.
     * 
     * @param value
     *     allowed object is
     *     {@link Integer }
     *     
     */
    public void setDuplicateCount(Integer value) {
        this.duplicateCount = value;
    }

}
