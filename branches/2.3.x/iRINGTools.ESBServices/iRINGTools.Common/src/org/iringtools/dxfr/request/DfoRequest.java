
package org.iringtools.dxfr.request;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.manifest.Manifest;


/**
 * <p>Java class for DfoRequest complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="DfoRequest">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="sourceScopeName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="sourceAppName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="targetScopeName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="targetAppName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="dataTransferObjects" type="{http://www.iringtools.org/dxfr/dto}DataTransferObjects" maxOccurs="2"/>
 *         &lt;element name="manifest" type="{http://www.iringtools.org/dxfr/manifest}Manifest"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "DfoRequest", propOrder = {
    "sourceScopeName",
    "sourceAppName",
    "targetScopeName",
    "targetAppName",
    "dataTransferObjects",
    "manifest"
})
@XmlRootElement(name = "dfoRequest")
public class DfoRequest {

    @XmlElement(required = true)
    protected String sourceScopeName;
    @XmlElement(required = true)
    protected String sourceAppName;
    @XmlElement(required = true)
    protected String targetScopeName;
    @XmlElement(required = true)
    protected String targetAppName;
    @XmlElement(required = true)
    protected List<DataTransferObjects> dataTransferObjects;
    @XmlElement(required = true)
    protected Manifest manifest;

    /**
     * Gets the value of the sourceScopeName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSourceScopeName() {
        return sourceScopeName;
    }

    /**
     * Sets the value of the sourceScopeName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSourceScopeName(String value) {
        this.sourceScopeName = value;
    }

    /**
     * Gets the value of the sourceAppName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSourceAppName() {
        return sourceAppName;
    }

    /**
     * Sets the value of the sourceAppName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSourceAppName(String value) {
        this.sourceAppName = value;
    }

    /**
     * Gets the value of the targetScopeName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getTargetScopeName() {
        return targetScopeName;
    }

    /**
     * Sets the value of the targetScopeName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setTargetScopeName(String value) {
        this.targetScopeName = value;
    }

    /**
     * Gets the value of the targetAppName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getTargetAppName() {
        return targetAppName;
    }

    /**
     * Sets the value of the targetAppName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setTargetAppName(String value) {
        this.targetAppName = value;
    }

    /**
     * Gets the value of the dataTransferObjects property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the dataTransferObjects property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getDataTransferObjects().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link DataTransferObjects }
     * 
     * 
     */
    public List<DataTransferObjects> getDataTransferObjects() {
        if (dataTransferObjects == null) {
            dataTransferObjects = new ArrayList<DataTransferObjects>();
        }
        return this.dataTransferObjects;
    }

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
     * Sets the value of the dataTransferObjects property.
     * 
     * @param dataTransferObjects
     *     allowed object is
     *     {@link DataTransferObjects }
     *     
     */
    public void setDataTransferObjects(List<DataTransferObjects> dataTransferObjects) {
        this.dataTransferObjects = dataTransferObjects;
    }

}
