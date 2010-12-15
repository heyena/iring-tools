
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
 *         &lt;element name="classObjects" type="{http://www.iringtools.org/dxfr/dto}ClassObjects"/>
 *         &lt;element name="transferOption" type="{http://www.iringtools.org/dxfr/dto}TransferOption"/>
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
    "transferOption"
})
public class DataTransferObject {

    @XmlElement(required = true)
    protected String identifier;
    @XmlElement(required = true)
    protected ClassObjects classObjects;
    @XmlElement(required = true)
    protected TransferOption transferOption;

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
