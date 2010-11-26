
package org.iringtools.dxfr.dto;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for RoleObjects complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="RoleObjects">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="roleObject" type="{http://www.iringtools.org/dxfr/dto}RoleObject" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "RoleObjects", propOrder = {
    "roleObjects"
})
public class RoleObjects {

    @XmlElement(name = "roleObject", required = true)
    protected List<RoleObject> roleObjects;

    /**
     * Gets the value of the roleObjects property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the roleObjects property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getRoleObjects().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link RoleObject }
     * 
     * 
     */
    public List<RoleObject> getRoleObjects() {
        if (roleObjects == null) {
            roleObjects = new ArrayList<RoleObject>();
        }
        return this.roleObjects;
    }

    /**
     * Sets the value of the roleObjects property.
     * 
     * @param roleObjects
     *     allowed object is
     *     {@link RoleObject }
     *     
     */
    public void setRoleObjects(List<RoleObject> roleObjects) {
        this.roleObjects = roleObjects;
    }

}
