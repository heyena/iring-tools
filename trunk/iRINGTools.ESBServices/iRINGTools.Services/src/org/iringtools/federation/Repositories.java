
package org.iringtools.federation;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for Repositories complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="Repositories">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="repository" type="{http://iringtools.org/federation}Repository" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "Repositories", propOrder = {
    "repositories"
})
public class Repositories {

    @XmlElement(name = "repository", required = true)
    protected List<Repository> repositories;

    /**
     * Gets the value of the repositories property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the repositories property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getRepositories().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Repository }
     * 
     * 
     */
    public List<Repository> getRepositories() {
        if (repositories == null) {
            repositories = new ArrayList<Repository>();
        }
        return this.repositories;
    }

    /**
     * Sets the value of the repositories property.
     * 
     * @param repositories
     *     allowed object is
     *     {@link Repository }
     *     
     */
    public void setRepositories(List<Repository> repositories) {
        this.repositories = repositories;
    }

}
