
package org.iringtools.refdata.federation;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for repository complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="repository">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="id" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="description" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="isreadonly" type="{http://www.w3.org/2001/XMLSchema}boolean"/>
 *         &lt;element name="name" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="repositorytype" type="{http://www.iringtools.org/refdata/federation}repositorytype"/>
 *         &lt;element name="uri" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="updateUri" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="namespaces" type="{http://www.iringtools.org/refdata/federation}namespacelist" minOccurs="0"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "repository", propOrder = {
    "id",
    "description",
    "isreadonly",
    "name",
    "repositorytype",
    "uri",
    "updateUri",
    "namespaces"
})
public class Repository {

    @XmlElement(required = true)
    protected String id;
    @XmlElement(required = true)
    protected String description;
    protected boolean isreadonly;
    @XmlElement(required = true)
    protected String name;
    @XmlElement(required = true)
    protected Repositorytype repositorytype;
    @XmlElement(required = true)
    protected String uri;
    @XmlElement(required = true)
    protected String updateUri;
    protected Namespacelist namespaces;

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
     * Gets the value of the isreadonly property.
     * This getter has been renamed from isIsreadonly() to getIsreadonly() by cxf-xjc-boolean plugin.
     * 
     */
    public boolean getIsreadonly() {
        return isreadonly;
    }

    /**
     * Sets the value of the isreadonly property.
     * 
     */
    public void setIsreadonly(boolean value) {
        this.isreadonly = value;
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
     * Gets the value of the repositorytype property.
     * 
     * @return
     *     possible object is
     *     {@link Repositorytype }
     *     
     */
    public Repositorytype getRepositorytype() {
        return repositorytype;
    }

    /**
     * Sets the value of the repositorytype property.
     * 
     * @param value
     *     allowed object is
     *     {@link Repositorytype }
     *     
     */
    public void setRepositorytype(Repositorytype value) {
        this.repositorytype = value;
    }

    /**
     * Gets the value of the uri property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getUri() {
        return uri;
    }

    /**
     * Sets the value of the uri property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setUri(String value) {
        this.uri = value;
    }

    /**
     * Gets the value of the updateUri property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getUpdateUri() {
        return updateUri;
    }

    /**
     * Sets the value of the updateUri property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setUpdateUri(String value) {
        this.updateUri = value;
    }

    /**
     * Gets the value of the namespaces property.
     * 
     * @return
     *     possible object is
     *     {@link Namespacelist }
     *     
     */
    public Namespacelist getNamespaces() {
        return namespaces;
    }

    /**
     * Sets the value of the namespaces property.
     * 
     * @param value
     *     allowed object is
     *     {@link Namespacelist }
     *     
     */
    public void setNamespaces(Namespacelist value) {
        this.namespaces = value;
    }

}
