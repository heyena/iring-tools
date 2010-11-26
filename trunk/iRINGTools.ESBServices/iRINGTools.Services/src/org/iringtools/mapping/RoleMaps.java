
package org.iringtools.mapping;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for RoleMaps complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="RoleMaps">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="roleMap" type="{http://www.iringtools.org/mapping}RoleMap" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "RoleMaps", propOrder = {
    "roleMaps"
})
public class RoleMaps {

    @XmlElement(name = "roleMap", required = true)
    protected List<RoleMap> roleMaps;

    /**
     * Gets the value of the roleMaps property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the roleMaps property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getRoleMaps().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link RoleMap }
     * 
     * 
     */
    public List<RoleMap> getRoleMaps() {
        if (roleMaps == null) {
            roleMaps = new ArrayList<RoleMap>();
        }
        return this.roleMaps;
    }

    /**
     * Sets the value of the roleMaps property.
     * 
     * @param roleMaps
     *     allowed object is
     *     {@link RoleMap }
     *     
     */
    public void setRoleMaps(List<RoleMap> roleMaps) {
        this.roleMaps = roleMaps;
    }

}
