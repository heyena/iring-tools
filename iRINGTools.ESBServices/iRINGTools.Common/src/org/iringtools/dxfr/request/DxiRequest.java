
package org.iringtools.dxfr.request;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;
import org.iringtools.dxfr.dti.DataTransferIndices;


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
 *         &lt;element name="sourceScopeName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="sourceAppName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="targetScopeName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="targetAppName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="dataTransferIndicies" type="{http://www.iringtools.org/dxfr/dti}DataTransferIndices" maxOccurs="2"/>
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
    "sourceScopeName",
    "sourceAppName",
    "targetScopeName",
    "targetAppName",
    "dataTransferIndicies"
})
@XmlRootElement(name = "dxiRequest")
public class DxiRequest {

    @XmlElement(required = true)
    protected String sourceScopeName;
    @XmlElement(required = true)
    protected String sourceAppName;
    @XmlElement(required = true)
    protected String targetScopeName;
    @XmlElement(required = true)
    protected String targetAppName;
    @XmlElement(required = true)
    protected List<DataTransferIndices> dataTransferIndicies;

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
     * Gets the value of the dataTransferIndicies property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the dataTransferIndicies property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getDataTransferIndicies().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link DataTransferIndices }
     * 
     * 
     */
    public List<DataTransferIndices> getDataTransferIndicies() {
        if (dataTransferIndicies == null) {
            dataTransferIndicies = new ArrayList<DataTransferIndices>();
        }
        return this.dataTransferIndicies;
    }

    /**
     * Sets the value of the dataTransferIndicies property.
     * 
     * @param dataTransferIndicies
     *     allowed object is
     *     {@link DataTransferIndices }
     *     
     */
    public void setDataTransferIndicies(List<DataTransferIndices> dataTransferIndicies) {
        this.dataTransferIndicies = dataTransferIndicies;
    }

}
