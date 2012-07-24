
package org.ids_adi.ns.qxf.model;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for QMXF complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="QMXF">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="class-definition" type="{http://ns.ids-adi.org/qxf/model#}ClassDefinition" maxOccurs="unbounded"/>
 *         &lt;element name="template-definition" type="{http://ns.ids-adi.org/qxf/model#}TemplateDefinition" maxOccurs="unbounded"/>
 *         &lt;element name="template-qualification" type="{http://ns.ids-adi.org/qxf/model#}TemplateQualification" maxOccurs="unbounded"/>
 *         &lt;element name="license" type="{http://ns.ids-adi.org/qxf/model#}License" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *       &lt;attribute name="timestamp" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="license-ref" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="targetRepository" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="sourceRepository" type="{http://www.w3.org/2001/XMLSchema}string" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "QMXF", propOrder = {
    "classDefinitions",
    "templateDefinitions",
    "templateQualifications",
    "licenses"
})
@XmlRootElement(name = "qmxf")
public class Qmxf {

    @XmlElement(name = "class-definition", required = true)
    protected List<ClassDefinition> classDefinitions;
    @XmlElement(name = "template-definition", required = true)
    protected List<TemplateDefinition> templateDefinitions;
    @XmlElement(name = "template-qualification", required = true)
    protected List<TemplateQualification> templateQualifications;
    @XmlElement(name = "license", required = true)
    protected List<License> licenses;
    @XmlAttribute(name = "timestamp")
    protected String timestamp;
    @XmlAttribute(name = "license-ref")
    protected String licenseRef;
    @XmlAttribute(name = "targetRepository")
    protected String targetRepository;
    @XmlAttribute(name = "sourceRepository")
    protected String sourceRepository;

    /**
     * Gets the value of the classDefinitions property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the classDefinitions property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getClassDefinitions().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link ClassDefinition }
     * 
     * 
     */
    public List<ClassDefinition> getClassDefinitions() {
        if (classDefinitions == null) {
            classDefinitions = new ArrayList<ClassDefinition>();
        }
        return this.classDefinitions;
    }

    /**
     * Gets the value of the templateDefinitions property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the templateDefinitions property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getTemplateDefinitions().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link TemplateDefinition }
     * 
     * 
     */
    public List<TemplateDefinition> getTemplateDefinitions() {
        if (templateDefinitions == null) {
            templateDefinitions = new ArrayList<TemplateDefinition>();
        }
        return this.templateDefinitions;
    }

    /**
     * Gets the value of the templateQualifications property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the templateQualifications property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getTemplateQualifications().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link TemplateQualification }
     * 
     * 
     */
    public List<TemplateQualification> getTemplateQualifications() {
        if (templateQualifications == null) {
            templateQualifications = new ArrayList<TemplateQualification>();
        }
        return this.templateQualifications;
    }

    /**
     * Gets the value of the licenses property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the licenses property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getLicenses().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link License }
     * 
     * 
     */
    public List<License> getLicenses() {
        if (licenses == null) {
            licenses = new ArrayList<License>();
        }
        return this.licenses;
    }

    /**
     * Gets the value of the timestamp property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getTimestamp() {
        return timestamp;
    }

    /**
     * Sets the value of the timestamp property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setTimestamp(String value) {
        this.timestamp = value;
    }

    /**
     * Gets the value of the licenseRef property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getLicenseRef() {
        return licenseRef;
    }

    /**
     * Sets the value of the licenseRef property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setLicenseRef(String value) {
        this.licenseRef = value;
    }

    /**
     * Gets the value of the targetRepository property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getTargetRepository() {
        return targetRepository;
    }

    /**
     * Sets the value of the targetRepository property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setTargetRepository(String value) {
        this.targetRepository = value;
    }

    /**
     * Gets the value of the sourceRepository property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSourceRepository() {
        return sourceRepository;
    }

    /**
     * Sets the value of the sourceRepository property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSourceRepository(String value) {
        this.sourceRepository = value;
    }

    /**
     * Sets the value of the classDefinitions property.
     * 
     * @param classDefinitions
     *     allowed object is
     *     {@link ClassDefinition }
     *     
     */
    public void setClassDefinitions(List<ClassDefinition> classDefinitions) {
        this.classDefinitions = classDefinitions;
    }

    /**
     * Sets the value of the templateDefinitions property.
     * 
     * @param templateDefinitions
     *     allowed object is
     *     {@link TemplateDefinition }
     *     
     */
    public void setTemplateDefinitions(List<TemplateDefinition> templateDefinitions) {
        this.templateDefinitions = templateDefinitions;
    }

    /**
     * Sets the value of the templateQualifications property.
     * 
     * @param templateQualifications
     *     allowed object is
     *     {@link TemplateQualification }
     *     
     */
    public void setTemplateQualifications(List<TemplateQualification> templateQualifications) {
        this.templateQualifications = templateQualifications;
    }

    /**
     * Sets the value of the licenses property.
     * 
     * @param licenses
     *     allowed object is
     *     {@link License }
     *     
     */
    public void setLicenses(List<License> licenses) {
        this.licenses = licenses;
    }

}
