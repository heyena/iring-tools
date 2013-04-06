
package org.iringtools.dxfr.request;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;
import org.iringtools.data.filter.DataFilter;
import org.iringtools.dxfr.manifest.Manifest;


/**
 * <p>Java class for DxiRequest complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="DxiRequest">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="manifest" type="{http://www.iringtools.org/dxfr/manifest}Manifest"/>
 *         &lt;element name="dataFilter" type="{http://www.iringtools.org/data/filter}DataFilter"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "DxiRequest", propOrder = {
    "manifest",
    "dataFilter"
})
@XmlRootElement(name = "dxiRequest")
public class DxiRequest {

    @XmlElement(required = true)
    protected Manifest manifest;
    @XmlElement(required = true)
    protected DataFilter dataFilter;

    /**
     * Gets the value of the manifest property.
     * 
     * @return
     *     possible object is
     *     {@link Manifest }
     *     
     */
    public Manifest getManifest() {
        return manifest;
    }

    /**
     * Sets the value of the manifest property.
     * 
     * @param value
     *     allowed object is
     *     {@link Manifest }
     *     
     */
    public void setManifest(Manifest value) {
        this.manifest = value;
    }

    /**
     * Gets the value of the dataFilter property.
     * 
     * @return
     *     possible object is
     *     {@link DataFilter }
     *     
     */
    public DataFilter getDataFilter() {
        return dataFilter;
    }

    /**
     * Sets the value of the dataFilter property.
     * 
     * @param value
     *     allowed object is
     *     {@link DataFilter }
     *     
     */
    public void setDataFilter(DataFilter value) {
        this.dataFilter = value;
    }

}
