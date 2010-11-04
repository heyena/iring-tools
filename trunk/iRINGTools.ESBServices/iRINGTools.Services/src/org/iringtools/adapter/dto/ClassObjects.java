
package org.iringtools.adapter.dto;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for ClassObjects complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="ClassObjects">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="classObject" type="{http://iringtools.org/adapter/dto}ClassObject" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "ClassObjects", propOrder = {
    "classObjects"
})
public class ClassObjects {

    @XmlElement(name = "classObject", required = true)
    protected List<ClassObject> classObjects;

    /**
     * Gets the value of the classObjects property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the classObjects property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getClassObjects().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link ClassObject }
     * 
     * 
     */
    public List<ClassObject> getClassObjects() {
        if (classObjects == null) {
            classObjects = new ArrayList<ClassObject>();
        }
        return this.classObjects;
    }

    /**
     * Sets the value of the classObjects property.
     * 
     * @param classObjects
     *     allowed object is
     *     {@link ClassObject }
     *     
     */
    public void setClassObjects(List<ClassObject> classObjects) {
        this.classObjects = classObjects;
    }

}
