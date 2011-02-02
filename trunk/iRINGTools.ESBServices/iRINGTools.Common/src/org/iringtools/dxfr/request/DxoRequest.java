
package org.iringtools.dxfr.request;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.manifest.Manifest;


/**
 * <p>Java class for DxoRequest complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="DxoRequest">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="manifest" type="{http://www.iringtools.org/dxfr/manifest}Manifest"/>
 *         &lt;element name="dataTransferIndices" type="{http://www.iringtools.org/dxfr/dti}DataTransferIndices"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "DxoRequest", propOrder = {
    "manifest",
    "dataTransferIndices"
})
@XmlRootElement(name = "dxoRequest")
public class DxoRequest {

    @XmlElement(required = true)
    protected Manifest manifest;
    @XmlElement(required = true)
    protected DataTransferIndices dataTransferIndices;

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
     * Gets the value of the dataTransferIndices property.
     * 
     * @return
     *     possible object is
     *     {@link DataTransferIndices }
     *     
     */
    public DataTransferIndices getDataTransferIndices() {
        return dataTransferIndices;
    }

    /**
     * Sets the value of the dataTransferIndices property.
     * 
     * @param value
     *     allowed object is
     *     {@link DataTransferIndices }
     *     
     */
    public void setDataTransferIndices(DataTransferIndices value) {
        this.dataTransferIndices = value;
    }

}
