
package org.iringtools.directory;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for Graphs complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="Graphs">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="graph" type="{http://www.iringtools.org/directory}Graph" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "Graphs", propOrder = {
    "graphs"
})
public class Graphs {

    @XmlElement(name = "graph", required = true)
    protected List<Graph> graphs;

    /**
     * Gets the value of the graphs property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the graphs property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getGraphs().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Graph }
     * 
     * 
     */
    public List<Graph> getGraphs() {
        if (graphs == null) {
            graphs = new ArrayList<Graph>();
        }
        return this.graphs;
    }

    /**
     * Sets the value of the graphs property.
     * 
     * @param graphs
     *     allowed object is
     *     {@link Graph }
     *     
     */
    public void setGraphs(List<Graph> graphs) {
        this.graphs = graphs;
    }

}
