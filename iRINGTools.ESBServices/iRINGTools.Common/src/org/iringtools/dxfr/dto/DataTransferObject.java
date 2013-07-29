
package org.iringtools.dxfr.dto;

import java.io.Serializable;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlInlineBinaryData;
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
 *         &lt;element name="identifier" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element ref="{http://www.iringtools.org/dxfr/dto}classObjects" minOccurs="0"/>
 *         &lt;element name="transferType" type="{http://www.iringtools.org/dxfr/dto}TransferType" minOccurs="0"/>
 *         &lt;element name="hasContent" type="{http://www.w3.org/2001/XMLSchema}boolean"/>
 *         &lt;element name="duplicateCount" type="{http://www.w3.org/2001/XMLSchema}int" minOccurs="0"/>
 *         &lt;element name="content" type="{http://www.w3.org/2001/XMLSchema}base64Binary" minOccurs="0"/>
 *         &lt;element name="internalIdentifier" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
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
    "identifier",
    "classObjects",
    "transferType",
    "hasContent",
    "duplicateCount",
    "content",
    "internalIdentifier"
})
@XmlRootElement(name = "dataTransferObject")
public class DataTransferObject
    implements Serializable
{

    private final static long serialVersionUID = 1L;
    @XmlElement(required = true)
    protected String identifier;
    protected ClassObjects classObjects;
    protected TransferType transferType;
    protected boolean hasContent;
    protected Integer duplicateCount;
    @XmlInlineBinaryData
    protected byte[] content;
    protected String internalIdentifier;

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
     * Gets the value of the hasContent property.
     * This getter has been renamed from isHasContent() to getHasContent() by cxf-xjc-boolean plugin.
     * 
     */
    public boolean getHasContent() {
        return hasContent;
    }

    /**
     * Sets the value of the hasContent property.
     * 
     */
    public void setHasContent(boolean value) {
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

    /**
     * Gets the value of the content property.
     * 
     * @return
     *     possible object is
     *     byte[]
     */
    public byte[] getContent() {
        return content;
    }

    /**
     * Sets the value of the content property.
     * 
     * @param value
     *     allowed object is
     *     byte[]
     */
    public void setContent(byte[] value) {
        this.content = ((byte[]) value);
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

}
