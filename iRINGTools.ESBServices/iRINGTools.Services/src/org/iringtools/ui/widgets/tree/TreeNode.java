
package org.iringtools.ui.widgets.tree;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for TreeNode complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="TreeNode">
 *   &lt;complexContent>
 *     &lt;extension base="{http://www.iringtools.org/ui/widgets/tree}Node">
 *       &lt;sequence>
 *         &lt;element name="children" type="{http://www.iringtools.org/ui/widgets/tree}Node" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/extension>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "TreeNode", propOrder = {
    "children"
})
public class TreeNode
    extends Node
{

    @XmlElement(required = true)
    protected List<Node> children;

    /**
     * Gets the value of the children property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the children property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getChildren().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Node }
     * 
     * 
     */
    public List<Node> getChildren() {
        if (children == null) {
            children = new ArrayList<Node>();
        }
        return this.children;
    }

    /**
     * Sets the value of the children property.
     * 
     * @param children
     *     allowed object is
     *     {@link Node }
     *     
     */
    public void setChildren(List<Node> children) {
        this.children = children;
    }

}
