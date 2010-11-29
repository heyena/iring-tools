
package org.iringtools.mapping;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for ClassTemplateMapList complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="ClassTemplateMapList">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="classTemplateMaps" type="{http://www.iringtools.org/mapping}ClassTemplateMap" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "ClassTemplateMapList", propOrder = {
    "classTemplateMaps"
})
public class ClassTemplateMapList {

    @XmlElement(required = true)
    protected List<ClassTemplateMap> classTemplateMaps;

    /**
     * Gets the value of the classTemplateMaps property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the classTemplateMaps property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getClassTemplateMaps().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link ClassTemplateMap }
     * 
     * 
     */
    public List<ClassTemplateMap> getClassTemplateMaps() {
        if (classTemplateMaps == null) {
            classTemplateMaps = new ArrayList<ClassTemplateMap>();
        }
        return this.classTemplateMaps;
    }

    /**
     * Sets the value of the classTemplateMaps property.
     * 
     * @param classTemplateMaps
     *     allowed object is
     *     {@link ClassTemplateMap }
     *     
     */
    public void setClassTemplateMaps(List<ClassTemplateMap> classTemplateMaps) {
        this.classTemplateMaps = classTemplateMaps;
    }

}
