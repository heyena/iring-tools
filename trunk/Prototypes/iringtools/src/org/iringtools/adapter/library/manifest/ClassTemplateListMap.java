//
// This file was generated by the JavaTM Architecture for XML Binding(JAXB) Reference Implementation, vhudson-jaxb-ri-2.2-7 
// See <a href="http://java.sun.com/xml/jaxb">http://java.sun.com/xml/jaxb</a> 
// Any modifications to this file will be lost upon recompilation of the source schema. 
// Generated on: 2010.07.01 at 04:29:48 PM EDT 
//


package org.iringtools.adapter.library.manifest;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for ClassTemplateListMap complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="ClassTemplateListMap">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="classMap" type="{http://iringtools.org/adapter/library/manifest}ClassMap"/>
 *         &lt;element name="templateMaps">
 *           &lt;complexType>
 *             &lt;complexContent>
 *               &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *                 &lt;sequence>
 *                   &lt;element name="TemplateMap" type="{http://iringtools.org/adapter/library/manifest}TemplateMap" maxOccurs="unbounded" minOccurs="0"/>
 *                 &lt;/sequence>
 *               &lt;/restriction>
 *             &lt;/complexContent>
 *           &lt;/complexType>
 *         &lt;/element>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "ClassTemplateListMap", propOrder = {
    "classMap",
    "templateMaps"
})
public class ClassTemplateListMap {

    @XmlElement(required = true)
    protected ClassMap classMap;
    @XmlElement(required = true)
    protected ClassTemplateListMap.TemplateMaps templateMaps;

    /**
     * Gets the value of the classMap property.
     * 
     * @return
     *     possible object is
     *     {@link ClassMap }
     *     
     */
    public ClassMap getClassMap() {
        return classMap;
    }

    /**
     * Sets the value of the classMap property.
     * 
     * @param value
     *     allowed object is
     *     {@link ClassMap }
     *     
     */
    public void setClassMap(ClassMap value) {
        this.classMap = value;
    }

    /**
     * Gets the value of the templateMaps property.
     * 
     * @return
     *     possible object is
     *     {@link ClassTemplateListMap.TemplateMaps }
     *     
     */
    public ClassTemplateListMap.TemplateMaps getTemplateMaps() {
        return templateMaps;
    }

    /**
     * Sets the value of the templateMaps property.
     * 
     * @param value
     *     allowed object is
     *     {@link ClassTemplateListMap.TemplateMaps }
     *     
     */
    public void setTemplateMaps(ClassTemplateListMap.TemplateMaps value) {
        this.templateMaps = value;
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
     *         &lt;element name="TemplateMap" type="{http://iringtools.org/adapter/library/manifest}TemplateMap" maxOccurs="unbounded" minOccurs="0"/>
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
        "templateMap"
    })
    public static class TemplateMaps {

        @XmlElement(name = "TemplateMap")
        protected List<TemplateMap> templateMap;

        /**
         * Gets the value of the templateMap property.
         * 
         * <p>
         * This accessor method returns a reference to the live list,
         * not a snapshot. Therefore any modification you make to the
         * returned list will be present inside the JAXB object.
         * This is why there is not a <CODE>set</CODE> method for the templateMap property.
         * 
         * <p>
         * For example, to add a new item, do as follows:
         * <pre>
         *    getTemplateMap().add(newItem);
         * </pre>
         * 
         * 
         * <p>
         * Objects of the following type(s) are allowed in the list
         * {@link TemplateMap }
         * 
         * 
         */
        public List<TemplateMap> getTemplateMap() {
            if (templateMap == null) {
                templateMap = new ArrayList<TemplateMap>();
            }
            return this.templateMap;
        }

        /**
         * Sets the value of the templateMap property.
         * 
         * @param templateMap
         *     allowed object is
         *     {@link TemplateMap }
         *     
         */
        public void setTemplateMap(List<TemplateMap> templateMap) {
            this.templateMap = templateMap;
        }

    }

}
