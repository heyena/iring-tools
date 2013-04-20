
package org.iringtools.directory;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;
import org.iringtools.data.filter.DataFilter;


/**
 * <p>Java class for anonymous complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType>
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="id" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="name" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="description" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="poolSize" type="{http://www.w3.org/2001/XMLSchema}int" minOccurs="0"/>
 *         &lt;element name="cacheable" type="{http://www.w3.org/2001/XMLSchema}boolean" minOccurs="0"/>
 *         &lt;element name="sourceUri" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="sourceScope" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="sourceApp" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="sourceGraph" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="targetUri" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="targetScope" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="targetApp" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="targetGraph" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element ref="{http://www.iringtools.org/data/filter}dataFilter"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "", propOrder = {
    "id",
    "name",
    "description",
    "poolSize",
    "cacheable",
    "sourceUri",
    "sourceScope",
    "sourceApp",
    "sourceGraph",
    "targetUri",
    "targetScope",
    "targetApp",
    "targetGraph",
    "dataFilter"
})
@XmlRootElement(name = "exchange")
public class Exchange {

    @XmlElement(required = true)
    protected String id;
    @XmlElement(required = true)
    protected String name;
    protected String description;
    protected Integer poolSize;
    protected Boolean cacheable;
    @XmlElement(required = true)
    protected String sourceUri;
    @XmlElement(required = true)
    protected String sourceScope;
    @XmlElement(required = true)
    protected String sourceApp;
    @XmlElement(required = true)
    protected String sourceGraph;
    @XmlElement(required = true)
    protected String targetUri;
    @XmlElement(required = true)
    protected String targetScope;
    @XmlElement(required = true)
    protected String targetApp;
    @XmlElement(required = true)
    protected String targetGraph;
    @XmlElement(namespace = "http://www.iringtools.org/data/filter", required = true)
    protected DataFilter dataFilter;

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
     * Gets the value of the name property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getName() {
        return name;
    }

    /**
     * Sets the value of the name property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setName(String value) {
        this.name = value;
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

    /**
     * Gets the value of the cacheable property.
     * This getter has been renamed from isCacheable() to getCacheable() by cxf-xjc-boolean plugin.
     * 
     * @return
     *     possible object is
     *     {@link Boolean }
     *     
     */
    public Boolean getCacheable() {
        return cacheable;
    }

    /**
     * Sets the value of the cacheable property.
     * 
     * @param value
     *     allowed object is
     *     {@link Boolean }
     *     
     */
    public void setCacheable(Boolean value) {
        this.cacheable = value;
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
     * Gets the value of the sourceScope property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSourceScope() {
        return sourceScope;
    }

    /**
     * Sets the value of the sourceScope property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSourceScope(String value) {
        this.sourceScope = value;
    }

    /**
     * Gets the value of the sourceApp property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSourceApp() {
        return sourceApp;
    }

    /**
     * Sets the value of the sourceApp property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSourceApp(String value) {
        this.sourceApp = value;
    }

    /**
     * Gets the value of the sourceGraph property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSourceGraph() {
        return sourceGraph;
    }

    /**
     * Sets the value of the sourceGraph property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSourceGraph(String value) {
        this.sourceGraph = value;
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
     * Gets the value of the targetScope property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getTargetScope() {
        return targetScope;
    }

    /**
     * Sets the value of the targetScope property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setTargetScope(String value) {
        this.targetScope = value;
    }

    /**
     * Gets the value of the targetApp property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getTargetApp() {
        return targetApp;
    }

    /**
     * Sets the value of the targetApp property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setTargetApp(String value) {
        this.targetApp = value;
    }

    /**
     * Gets the value of the targetGraph property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getTargetGraph() {
        return targetGraph;
    }

    /**
     * Sets the value of the targetGraph property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setTargetGraph(String value) {
        this.targetGraph = value;
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
