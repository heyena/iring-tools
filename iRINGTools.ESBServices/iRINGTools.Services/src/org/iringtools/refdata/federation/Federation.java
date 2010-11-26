
package org.iringtools.refdata.federation;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for Federation complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="Federation">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="idGenerators" type="{http://www.iringtools.org/refdata/federation}IDGenerators"/>
 *         &lt;element name="namespaces" type="{http://www.iringtools.org/refdata/federation}Namespaces"/>
 *         &lt;element name="repositories" type="{http://www.iringtools.org/refdata/federation}Repositories"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "Federation", propOrder = {
    "idGenerators",
    "namespaces",
    "repositories"
})
@XmlRootElement(name = "federation")
public class Federation {

    @XmlElement(required = true)
    protected IDGenerators idGenerators;
    @XmlElement(required = true)
    protected Namespaces namespaces;
    @XmlElement(required = true)
    protected Repositories repositories;

    /**
     * Gets the value of the idGenerators property.
     * 
     * @return
     *     possible object is
     *     {@link IDGenerators }
     *     
     */
    public IDGenerators getIdGenerators() {
        return idGenerators;
    }

    /**
     * Sets the value of the idGenerators property.
     * 
     * @param value
     *     allowed object is
     *     {@link IDGenerators }
     *     
     */
    public void setIdGenerators(IDGenerators value) {
        this.idGenerators = value;
    }

    /**
     * Gets the value of the namespaces property.
     * 
     * @return
     *     possible object is
     *     {@link Namespaces }
     *     
     */
    public Namespaces getNamespaces() {
        return namespaces;
    }

    /**
     * Sets the value of the namespaces property.
     * 
     * @param value
     *     allowed object is
     *     {@link Namespaces }
     *     
     */
    public void setNamespaces(Namespaces value) {
        this.namespaces = value;
    }

    /**
     * Gets the value of the repositories property.
     * 
     * @return
     *     possible object is
     *     {@link Repositories }
     *     
     */
    public Repositories getRepositories() {
        return repositories;
    }

    /**
     * Sets the value of the repositories property.
     * 
     * @param value
     *     allowed object is
     *     {@link Repositories }
     *     
     */
    public void setRepositories(Repositories value) {
        this.repositories = value;
    }

}
