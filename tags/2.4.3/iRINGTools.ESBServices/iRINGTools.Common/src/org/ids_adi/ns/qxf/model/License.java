
package org.ids_adi.ns.qxf.model;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for License complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="License">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="license-terms" type="{http://ns.ids-adi.org/qxf/model#}LicenseTerm" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "License", propOrder = {
    "licenseTerms"
})
public class License {

    @XmlElement(name = "license-terms", required = true)
    protected List<LicenseTerm> licenseTerms;

    /**
     * Gets the value of the licenseTerms property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the licenseTerms property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getLicenseTerms().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link LicenseTerm }
     * 
     * 
     */
    public List<LicenseTerm> getLicenseTerms() {
        if (licenseTerms == null) {
            licenseTerms = new ArrayList<LicenseTerm>();
        }
        return this.licenseTerms;
    }

    /**
     * Sets the value of the licenseTerms property.
     * 
     * @param licenseTerms
     *     allowed object is
     *     {@link LicenseTerm }
     *     
     */
    public void setLicenseTerms(List<LicenseTerm> licenseTerms) {
        this.licenseTerms = licenseTerms;
    }

}
