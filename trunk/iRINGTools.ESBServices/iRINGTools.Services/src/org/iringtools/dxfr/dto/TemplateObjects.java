
package org.iringtools.dxfr.dto;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for TemplateObjects complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="TemplateObjects">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="templateObject" type="{http://www.iringtools.org/dxfr/dto}TemplateObject" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "TemplateObjects", propOrder = {
    "templateObjects"
})
public class TemplateObjects {

    @XmlElement(name = "templateObject", required = true)
    protected List<TemplateObject> templateObjects;

    /**
     * Gets the value of the templateObjects property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the templateObjects property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getTemplateObjects().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link TemplateObject }
     * 
     * 
     */
    public List<TemplateObject> getTemplateObjects() {
        if (templateObjects == null) {
            templateObjects = new ArrayList<TemplateObject>();
        }
        return this.templateObjects;
    }

    /**
     * Sets the value of the templateObjects property.
     * 
     * @param templateObjects
     *     allowed object is
     *     {@link TemplateObject }
     *     
     */
    public void setTemplateObjects(List<TemplateObject> templateObjects) {
        this.templateObjects = templateObjects;
    }

}
