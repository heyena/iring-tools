//
// This file was generated by the JavaTM Architecture for XML Binding(JAXB) Reference Implementation, vhudson-jaxb-ri-2.2-7 
// See <a href="http://java.sun.com/xml/jaxb">http://java.sun.com/xml/jaxb</a> 
// Any modifications to this file will be lost upon recompilation of the source schema. 
// Generated on: 2010.07.08 at 10:18:05 AM EDT 
//


package org.iringtools.adapter.library.dto;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for ClassObject complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="ClassObject">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="classId" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="name" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="identifier" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="templateObjects">
 *           &lt;complexType>
 *             &lt;complexContent>
 *               &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *                 &lt;sequence>
 *                   &lt;element name="templateObject" type="{http://iringtools.org/adapter/library/dto}TemplateObject" maxOccurs="unbounded" minOccurs="0"/>
 *                 &lt;/sequence>
 *               &lt;/restriction>
 *             &lt;/complexContent>
 *           &lt;/complexType>
 *         &lt;/element>
 *         &lt;element name="transferType" type="{http://iringtools.org/adapter/library/dto}TransferType"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "ClassObject", propOrder = {
    "classId",
    "name",
    "identifier",
    "templateObjects",
    "transferType"
})
public class ClassObject {

    @XmlElement(required = true)
    protected String classId;
    @XmlElement(required = true)
    protected String name;
    @XmlElement(required = true)
    protected String identifier;
    @XmlElement(required = true)
    protected ClassObject.TemplateObjects templateObjects;
    @XmlElement(required = true)
    protected TransferType transferType;

    /**
     * Gets the value of the classId property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getClassId() {
        return classId;
    }

    /**
     * Sets the value of the classId property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setClassId(String value) {
        this.classId = value;
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
     * Gets the value of the templateObjects property.
     * 
     * @return
     *     possible object is
     *     {@link ClassObject.TemplateObjects }
     *     
     */
    public ClassObject.TemplateObjects getTemplateObjects() {
        return templateObjects;
    }

    /**
     * Sets the value of the templateObjects property.
     * 
     * @param value
     *     allowed object is
     *     {@link ClassObject.TemplateObjects }
     *     
     */
    public void setTemplateObjects(ClassObject.TemplateObjects value) {
        this.templateObjects = value;
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
     * <p>Java class for anonymous complex type.
     * 
     * <p>The following schema fragment specifies the expected content contained within this class.
     * 
     * <pre>
     * &lt;complexType>
     *   &lt;complexContent>
     *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
     *       &lt;sequence>
     *         &lt;element name="templateObject" type="{http://iringtools.org/adapter/library/dto}TemplateObject" maxOccurs="unbounded" minOccurs="0"/>
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
        "templateObject"
    })
    public static class TemplateObjects implements Iterable<TemplateObject> {

        protected List<TemplateObject> templateObject;

        /**
         * Gets the value of the templateObject property.
         * 
         * <p>
         * This accessor method returns a reference to the live list,
         * not a snapshot. Therefore any modification you make to the
         * returned list will be present inside the JAXB object.
         * This is why there is not a <CODE>set</CODE> method for the templateObject property.
         * 
         * <p>
         * For example, to add a new item, do as follows:
         * <pre>
         *    getTemplateObject().add(newItem);
         * </pre>
         * 
         * 
         * <p>
         * Objects of the following type(s) are allowed in the list
         * {@link TemplateObject }
         * 
         * 
         */
        public List<TemplateObject> getTemplateObject() {
            if (templateObject == null) {
                templateObject = new ArrayList<TemplateObject>();
            }
            return this.templateObject;
        }

        /**
         * Sets the value of the templateObject property.
         * 
         * @param templateObject
         *     allowed object is
         *     {@link TemplateObject }
         *     
         */
        public void setTemplateObject(List<TemplateObject> templateObject) {
            this.templateObject = templateObject;
        }

        @Override
        public Iterator<TemplateObject> iterator()
        {
          return templateObject.iterator();
        }
    }
}
