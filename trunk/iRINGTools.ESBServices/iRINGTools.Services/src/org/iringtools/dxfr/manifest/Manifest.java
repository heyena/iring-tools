
package org.iringtools.dxfr.manifest;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for Manifest complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="Manifest">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="graphs" type="{http://www.iringtools.org/dxfr/manifest}Graphs"/>
 *         &lt;element name="version" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "Manifest", propOrder = {
    "graphs",
    "version"
})
@XmlRootElement(name = "manifest")
public class Manifest {

    @XmlElement(required = true)
    protected Graphs graphs;
    @XmlElement(required = true)
    protected String version;

    /**
     * Gets the value of the graphs property.
     * 
     * @return
     *     possible object is
     *     {@link Graphs }
     *     
     */
    public Graphs getGraphs() {
        return graphs;
    }

    /**
     * Sets the value of the graphs property.
     * 
     * @param value
     *     allowed object is
     *     {@link Graphs }
     *     
     */
    public void setGraphs(Graphs value) {
        this.graphs = value;
    }

    /**
     * Gets the value of the version property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getVersion() {
        return version;
    }

    /**
     * Sets the value of the version property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setVersion(String value) {
        this.version = value;
    }

}
