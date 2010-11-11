
package org.ids_adi.ns.qxf.model;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for LicenseTerm complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="LicenseTerm">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="license-text" type="{http://ns.ids-adi.org/qxf/model#}LicenseText" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *       &lt;attribute name="id" type="{http://www.w3.org/2001/XMLSchema}string" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "LicenseTerm", propOrder = {
    "licenseTexts"
})
public class LicenseTerm {

    @XmlElement(name = "license-text", required = true)
    protected List<LicenseText> licenseTexts;
    @XmlAttribute(name = "id")
    protected String id;

    /**
     * Gets the value of the licenseTexts property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the licenseTexts property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getLicenseTexts().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link LicenseText }
     * 
     * 
     */
    public List<LicenseText> getLicenseTexts() {
        if (licenseTexts == null) {
            licenseTexts = new ArrayList<LicenseText>();
        }
        return this.licenseTexts;
    }

    /**
     * Gets the value of the id property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getId() {
        return id;
    }

    /**
     * Sets the value of the id property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setId(String value) {
        this.id = value;
    }

    /**
     * Sets the value of the licenseTexts property.
     * 
     * @param licenseTexts
     *     allowed object is
     *     {@link LicenseText }
     *     
     */
    public void setLicenseTexts(List<LicenseText> licenseTexts) {
        this.licenseTexts = licenseTexts;
    }

}
