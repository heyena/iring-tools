
package org.iringtools.library;

import java.io.Serializable;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;


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
 *         &lt;element name="importURI" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="timeout" type="{http://www.w3.org/2001/XMLSchema}int" minOccurs="0"/>
 *         &lt;element ref="{http://www.iringtools.org/library}cacheEntries"/>
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
    "importURI",
    "timeout",
    "cacheEntries"
})
@XmlRootElement(name = "cacheInfo")
public class CacheInfo
    implements Serializable
{

    private final static long serialVersionUID = 1L;
    protected String importURI;
    protected Integer timeout;
    @XmlElement(required = true)
    protected CacheEntries cacheEntries;

    /**
     * Gets the value of the importURI property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getImportURI() {
        return importURI;
    }

    /**
     * Sets the value of the importURI property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setImportURI(String value) {
        this.importURI = value;
    }

    /**
     * Gets the value of the timeout property.
     * 
     * @return
     *     possible object is
     *     {@link Integer }
     *     
     */
    public Integer getTimeout() {
        return timeout;
    }

    /**
     * Sets the value of the timeout property.
     * 
     * @param value
     *     allowed object is
     *     {@link Integer }
     *     
     */
    public void setTimeout(Integer value) {
        this.timeout = value;
    }

    /**
     * Gets the value of the cacheEntries property.
     * 
     * @return
     *     possible object is
     *     {@link CacheEntries }
     *     
     */
    public CacheEntries getCacheEntries() {
        return cacheEntries;
    }

    /**
     * Sets the value of the cacheEntries property.
     * 
     * @param value
     *     allowed object is
     *     {@link CacheEntries }
     *     
     */
    public void setCacheEntries(CacheEntries value) {
        this.cacheEntries = value;
    }

}
