
package org.iringtools.common.request;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;
import org.iringtools.adapter.dti.DataTransferIndices;
import org.iringtools.protocol.manifest.Manifest;


/**
 * <p>Java class for DtoPageRequest complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="DtoPageRequest">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="manifest" type="{http://iringtools.org/protocol/manifest}Manifest"/>
 *         &lt;element name="dataTransferIndices" type="{http://iringtools.org/adapter/dti}DataTransferIndices"/>
 *         &lt;element name="hashAlgorithm" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "DtoPageRequest", propOrder = {
    "manifest",
    "dataTransferIndices",
    "hashAlgorithm"
})
@XmlRootElement(name = "dtoPageRequest")
public class DtoPageRequest {

    @XmlElement(required = true)
    protected Manifest manifest;
    @XmlElement(required = true)
    protected DataTransferIndices dataTransferIndices;
    @XmlElement(required = true)
    protected String hashAlgorithm;

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

    /**
     * Gets the value of the hashAlgorithm property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getHashAlgorithm() {
        return hashAlgorithm;
    }

    /**
     * Sets the value of the hashAlgorithm property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setHashAlgorithm(String value) {
        this.hashAlgorithm = value;
    }

}
