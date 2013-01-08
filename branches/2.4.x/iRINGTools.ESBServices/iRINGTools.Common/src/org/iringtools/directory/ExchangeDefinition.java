
package org.iringtools.directory;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for ExchangeDefinition complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="ExchangeDefinition">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="id" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="sourceUri" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="description" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="sourceScopeName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="sourceAppName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="sourceGraphName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="targetUri" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="targetScopeName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="targetAppName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="targetGraphName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="hashAlgorithm" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="poolSize" type="{http://www.w3.org/2001/XMLSchema}int" minOccurs="0"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "ExchangeDefinition", propOrder = {
    "id",
    "sourceUri",
    "description",
    "sourceScopeName",
    "sourceAppName",
    "sourceGraphName",
    "targetUri",
    "targetScopeName",
    "targetAppName",
    "targetGraphName",
    "hashAlgorithm",
    "poolSize"
})
@XmlRootElement(name = "exchangeDefinition")
public class ExchangeDefinition {

    @XmlElement(required = true)
    protected String id;
    @XmlElement(required = true)
    protected String sourceUri;
    protected String description;
    @XmlElement(required = true)
    protected String sourceScopeName;
    @XmlElement(required = true)
    protected String sourceAppName;
    @XmlElement(required = true)
    protected String sourceGraphName;
    @XmlElement(required = true)
    protected String targetUri;
    @XmlElement(required = true)
    protected String targetScopeName;
    @XmlElement(required = true)
    protected String targetAppName;
    @XmlElement(required = true)
    protected String targetGraphName;
    protected String hashAlgorithm;
    protected Integer poolSize;

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
     * Gets the value of the sourceUri property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSourceUri() {
        return sourceUri;
    }

    /**
     * Sets the value of the sourceUri property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSourceUri(String value) {
        this.sourceUri = value;
    }

    /**
     * Gets the value of the description property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getDescription() {
        return description;
    }

    /**
     * Sets the value of the description property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setDescription(String value) {
        this.description = value;
    }

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
     * Gets the value of the sourceGraphName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSourceGraphName() {
        return sourceGraphName;
    }

    /**
     * Sets the value of the sourceGraphName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSourceGraphName(String value) {
        this.sourceGraphName = value;
    }

    /**
     * Gets the value of the targetUri property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getTargetUri() {
        return targetUri;
    }

    /**
     * Sets the value of the targetUri property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setTargetUri(String value) {
        this.targetUri = value;
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
     * Gets the value of the targetGraphName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getTargetGraphName() {
        return targetGraphName;
    }

    /**
     * Sets the value of the targetGraphName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setTargetGraphName(String value) {
        this.targetGraphName = value;
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

    /**
     * Gets the value of the poolSize property.
     * 
     * @return
     *     possible object is
     *     {@link Integer }
     *     
     */
    public Integer getPoolSize() {
        return poolSize;
    }

    /**
     * Sets the value of the poolSize property.
     * 
     * @param value
     *     allowed object is
     *     {@link Integer }
     *     
     */
    public void setPoolSize(Integer value) {
        this.poolSize = value;
    }

}
