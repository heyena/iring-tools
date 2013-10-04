
package org.iringtools.mapping;

import java.io.Serializable;
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
 *         &lt;element ref="{http://www.iringtools.org/mapping}classMap"/>
 *         &lt;element ref="{http://www.iringtools.org/mapping}templateMaps"/>
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
    "classMap",
    "templateMaps"
})
@XmlRootElement(name = "classTemplateMap")
public class ClassTemplateMap
    implements Serializable
{

    private final static long serialVersionUID = 1L;
    @XmlElement(required = true)
    protected ClassMap classMap;
    @XmlElement(required = true)
    protected TemplateMaps templateMaps;

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
     *     {@link TemplateMaps }
     *     
     */
    public TemplateMaps getTemplateMaps() {
        return templateMaps;
    }

    /**
     * Sets the value of the templateMaps property.
     * 
     * @param value
     *     allowed object is
     *     {@link TemplateMaps }
     *     
     */
    public void setTemplateMaps(TemplateMaps value) {
        this.templateMaps = value;
    }

}
