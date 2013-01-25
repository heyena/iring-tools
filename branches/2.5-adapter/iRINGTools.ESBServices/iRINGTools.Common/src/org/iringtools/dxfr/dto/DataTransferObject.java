
package org.iringtools.dxfr.dto;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for DataTransferObject complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="DataTransferObject">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="identifier" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="classObjects" type="{http://www.iringtools.org/dxfr/dto}ClassObjects" minOccurs="0"/>
 *         &lt;element name="transferType" type="{http://www.iringtools.org/dxfr/dto}TransferType" minOccurs="0"/>
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
@XmlType(name = "DataTransferObject", propOrder = {
    "identifier",
    "classObjects",
    "transferType",
    "duplicateCount"
})
public class DataTransferObject {

    @XmlElement(required = true)
    protected String identifier;
    protected ClassObjects classObjects;
    protected TransferType transferType;
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
     * Gets the value of the classObjects property.
     * 
     * @return
     *     possible object is
     *     {@link ClassObjects }
     *     
     */
    public ClassObjects getClassObjects() {
        return classObjects;
    }

    /**
     * Sets the value of the classObjects property.
     * 
     * @param value
     *     allowed object is
     *     {@link ClassObjects }
     *     
     */
    public void setClassObjects(ClassObjects value) {
        this.classObjects = value;
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
