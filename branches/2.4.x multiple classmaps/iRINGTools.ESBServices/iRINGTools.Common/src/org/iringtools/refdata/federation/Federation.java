
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
 *         &lt;element name="idgeneratorlist" type="{http://www.iringtools.org/refdata/federation}IdGeneratorList"/>
 *         &lt;element name="namespacelist" type="{http://www.iringtools.org/refdata/federation}NamespaceList"/>
 *         &lt;element name="repositorylist" type="{http://www.iringtools.org/refdata/federation}RepositoryList"/>
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
    "idgeneratorlist",
    "namespacelist",
    "repositorylist"
})
@XmlRootElement(name = "federation")
public class Federation {

    @XmlElement(required = true)
    protected IdGeneratorList idgeneratorlist;
    @XmlElement(required = true)
    protected NamespaceList namespacelist;
    @XmlElement(required = true)
    protected RepositoryList repositorylist;

    /**
     * Gets the value of the idgeneratorlist property.
     * 
     * @return
     *     possible object is
     *     {@link IdGeneratorList }
     *     
     */
    public IdGeneratorList getIdgeneratorlist() {
        return idgeneratorlist;
    }

    /**
     * Sets the value of the idgeneratorlist property.
     * 
     * @param value
     *     allowed object is
     *     {@link IdGeneratorList }
     *     
     */
    public void setIdgeneratorlist(IdGeneratorList value) {
        this.idgeneratorlist = value;
    }

    /**
     * Gets the value of the namespacelist property.
     * 
     * @return
     *     possible object is
     *     {@link NamespaceList }
     *     
     */
    public NamespaceList getNamespacelist() {
        return namespacelist;
    }

    /**
     * Sets the value of the namespacelist property.
     * 
     * @param value
     *     allowed object is
     *     {@link NamespaceList }
     *     
     */
    public void setNamespacelist(NamespaceList value) {
        this.namespacelist = value;
    }

    /**
     * Gets the value of the repositorylist property.
     * 
     * @return
     *     possible object is
     *     {@link RepositoryList }
     *     
     */
    public RepositoryList getRepositorylist() {
        return repositorylist;
    }

    /**
     * Sets the value of the repositorylist property.
     * 
     * @param value
     *     allowed object is
     *     {@link RepositoryList }
     *     
     */
    public void setRepositorylist(RepositoryList value) {
        this.repositorylist = value;
    }

}
