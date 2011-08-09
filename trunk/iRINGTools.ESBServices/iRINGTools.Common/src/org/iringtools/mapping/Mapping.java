
package org.iringtools.mapping;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for Mapping complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="Mapping">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="graphMaps" type="{http://www.iringtools.org/mapping}GraphMaps"/>
 *         &lt;element name="valueListMaps" type="{http://www.iringtools.org/mapping}ValueListMaps"/>
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
@XmlType(name = "Mapping", propOrder = {
    "graphMaps",
    "valueListMaps",
    "version"
})
@XmlRootElement(name = "mapping")
public class Mapping {

    @XmlElement(required = true)
    protected GraphMaps graphMaps;
    @XmlElement(required = true)
    protected ValueListMaps valueListMaps;
    @XmlElement(required = true)
    protected String version;

    /**
     * Gets the value of the graphMaps property.
     * 
     * @return
     *     possible object is
     *     {@link GraphMaps }
     *     
     */
    public GraphMaps getGraphMaps() {
        return graphMaps;
    }

    /**
     * Sets the value of the graphMaps property.
     * 
     * @param value
     *     allowed object is
     *     {@link GraphMaps }
     *     
     */
    public void setGraphMaps(GraphMaps value) {
        this.graphMaps = value;
    }

    /**
     * Gets the value of the valueListMaps property.
     * 
     * @return
     *     possible object is
     *     {@link ValueListMaps }
     *     
     */
    public ValueListMaps getValueListMaps() {
        return valueListMaps;
    }

    /**
     * Sets the value of the valueListMaps property.
     * 
     * @param value
     *     allowed object is
     *     {@link ValueListMaps }
     *     
     */
    public void setValueListMaps(ValueListMaps value) {
        this.valueListMaps = value;
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
