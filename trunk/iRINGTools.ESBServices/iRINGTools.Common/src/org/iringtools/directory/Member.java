
package org.iringtools.directory;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for Member complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="Member">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="userId" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="aciRights" type="{http://www.iringtools.org/directory}AciRights"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "Member", propOrder = {
    "userId",
    "aciRights"
})
public class Member {

    @XmlElement(required = true)
    protected String userId;
    @XmlElement(required = true)
    protected AciRights aciRights;

    /**
     * Gets the value of the userId property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getUserId() {
        return userId;
    }

    /**
     * Sets the value of the userId property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setUserId(String value) {
        this.userId = value;
    }

    /**
     * Gets the value of the aciRights property.
     * 
     * @return
     *     possible object is
     *     {@link AciRights }
     *     
     */
    public AciRights getAciRights() {
        return aciRights;
    }

    /**
     * Sets the value of the aciRights property.
     * 
     * @param value
     *     allowed object is
     *     {@link AciRights }
     *     
     */
    public void setAciRights(AciRights value) {
        this.aciRights = value;
    }

}
