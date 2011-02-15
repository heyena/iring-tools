
package org.iringtools.ui.widgets.tree;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for LeafNode complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="LeafNode">
 *   &lt;complexContent>
 *     &lt;extension base="{http://www.iringtools.org/ui/widgets/tree}Node">
 *       &lt;sequence>
 *         &lt;element name="leaf" type="{http://www.w3.org/2001/XMLSchema}boolean"/>
 *       &lt;/sequence>
 *     &lt;/extension>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "LeafNode", propOrder = {
    "leaf"
})
public class LeafNode
    extends Node
{

    @XmlElement(defaultValue = "true")
    protected boolean leaf;

    /**
     * Gets the value of the leaf property.
     * 
     */
    public boolean isLeaf() {
        return leaf;
    }

    /**
     * Sets the value of the leaf property.
     * 
     */
    public void setLeaf(boolean value) {
        this.leaf = value;
    }

}
