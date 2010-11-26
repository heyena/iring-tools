
package org.iringtools.ui.widgets.tree;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for Tree complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="Tree">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="treeNode" type="{http://www.iringtools.org/ui/widgets/tree}Node" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "Tree", propOrder = {
    "treeNodes"
})
@XmlRootElement(name = "tree")
public class Tree {

    @XmlElement(name = "treeNode", required = true)
    protected List<Node> treeNodes;

    /**
     * Gets the value of the treeNodes property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the treeNodes property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getTreeNodes().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Node }
     * 
     * 
     */
    public List<Node> getTreeNodes() {
        if (treeNodes == null) {
            treeNodes = new ArrayList<Node>();
        }
        return this.treeNodes;
    }

    /**
     * Sets the value of the treeNodes property.
     * 
     * @param treeNodes
     *     allowed object is
     *     {@link Node }
     *     
     */
    public void setTreeNodes(List<Node> treeNodes) {
        this.treeNodes = treeNodes;
    }

}
