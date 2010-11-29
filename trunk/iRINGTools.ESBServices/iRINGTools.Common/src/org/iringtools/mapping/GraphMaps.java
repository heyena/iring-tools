
package org.iringtools.mapping;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for GraphMaps complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="GraphMaps">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="graphMap" type="{http://www.iringtools.org/mapping}GraphMap" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "GraphMaps", propOrder = {
    "graphMaps"
})
public class GraphMaps {

    @XmlElement(name = "graphMap", required = true)
    protected List<GraphMap> graphMaps;

    /**
     * Gets the value of the graphMaps property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the graphMaps property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getGraphMaps().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link GraphMap }
     * 
     * 
     */
    public List<GraphMap> getGraphMaps() {
        if (graphMaps == null) {
            graphMaps = new ArrayList<GraphMap>();
        }
        return this.graphMaps;
    }

    /**
     * Sets the value of the graphMaps property.
     * 
     * @param graphMaps
     *     allowed object is
     *     {@link GraphMap }
     *     
     */
    public void setGraphMaps(List<GraphMap> graphMaps) {
        this.graphMaps = graphMaps;
    }

}
