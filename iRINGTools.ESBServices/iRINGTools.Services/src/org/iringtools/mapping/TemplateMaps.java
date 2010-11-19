
package org.iringtools.mapping;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for TemplateMaps complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="TemplateMaps">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="templateMap" type="{http://www.iringtools.org/mapping}TemplateMap" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "TemplateMaps", propOrder = {
    "templateMaps"
})
public class TemplateMaps {

    @XmlElement(name = "templateMap", required = true)
    protected List<TemplateMap> templateMaps;

    /**
     * Gets the value of the templateMaps property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the templateMaps property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getTemplateMaps().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link TemplateMap }
     * 
     * 
     */
    public List<TemplateMap> getTemplateMaps() {
        if (templateMaps == null) {
            templateMaps = new ArrayList<TemplateMap>();
        }
        return this.templateMaps;
    }

    /**
     * Sets the value of the templateMaps property.
     * 
     * @param templateMaps
     *     allowed object is
     *     {@link TemplateMap }
     *     
     */
    public void setTemplateMaps(List<TemplateMap> templateMaps) {
        this.templateMaps = templateMaps;
    }

}
