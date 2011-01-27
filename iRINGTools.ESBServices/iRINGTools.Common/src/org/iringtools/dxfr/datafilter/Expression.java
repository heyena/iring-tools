
package org.iringtools.dxfr.datafilter;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for Expression complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="Expression">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="OpenGroupCount" type="{http://www.w3.org/2001/XMLSchema}int"/>
 *         &lt;element name="PropertyName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="relationalOperator" type="{http://www.iringtools.org/dxfr/dataFilter}RelationalOperator"/>
 *         &lt;element name="Values" type="{http://www.w3.org/2001/XMLSchema}string" maxOccurs="unbounded"/>
 *         &lt;element name="logicalOperator" type="{http://www.iringtools.org/dxfr/dataFilter}LogicalOperator"/>
 *         &lt;element name="CloseGroupCount" type="{http://www.w3.org/2001/XMLSchema}int"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "Expression", propOrder = {
    "openGroupCount",
    "propertyName",
    "relationalOperator",
    "values",
    "logicalOperator",
    "closeGroupCount"
})
public class Expression {

    @XmlElement(name = "OpenGroupCount")
    protected int openGroupCount;
    @XmlElement(name = "PropertyName", required = true)
    protected String propertyName;
    @XmlElement(required = true)
    protected RelationalOperator relationalOperator;
    @XmlElement(name = "Values", required = true)
    protected List<String> values;
    @XmlElement(required = true)
    protected LogicalOperator logicalOperator;
    @XmlElement(name = "CloseGroupCount")
    protected int closeGroupCount;

    /**
     * Gets the value of the openGroupCount property.
     * 
     */
    public int getOpenGroupCount() {
        return openGroupCount;
    }

    /**
     * Sets the value of the openGroupCount property.
     * 
     */
    public void setOpenGroupCount(int value) {
        this.openGroupCount = value;
    }

    /**
     * Gets the value of the propertyName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getPropertyName() {
        return propertyName;
    }

    /**
     * Sets the value of the propertyName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setPropertyName(String value) {
        this.propertyName = value;
    }

    /**
     * Gets the value of the relationalOperator property.
     * 
     * @return
     *     possible object is
     *     {@link RelationalOperator }
     *     
     */
    public RelationalOperator getRelationalOperator() {
        return relationalOperator;
    }

    /**
     * Sets the value of the relationalOperator property.
     * 
     * @param value
     *     allowed object is
     *     {@link RelationalOperator }
     *     
     */
    public void setRelationalOperator(RelationalOperator value) {
        this.relationalOperator = value;
    }

    /**
     * Gets the value of the values property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the values property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getValues().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link String }
     * 
     * 
     */
    public List<String> getValues() {
        if (values == null) {
            values = new ArrayList<String>();
        }
        return this.values;
    }

    /**
     * Gets the value of the logicalOperator property.
     * 
     * @return
     *     possible object is
     *     {@link LogicalOperator }
     *     
     */
    public LogicalOperator getLogicalOperator() {
        return logicalOperator;
    }

    /**
     * Sets the value of the logicalOperator property.
     * 
     * @param value
     *     allowed object is
     *     {@link LogicalOperator }
     *     
     */
    public void setLogicalOperator(LogicalOperator value) {
        this.logicalOperator = value;
    }

    /**
     * Gets the value of the closeGroupCount property.
     * 
     */
    public int getCloseGroupCount() {
        return closeGroupCount;
    }

    /**
     * Sets the value of the closeGroupCount property.
     * 
     */
    public void setCloseGroupCount(int value) {
        this.closeGroupCount = value;
    }

    /**
     * Sets the value of the values property.
     * 
     * @param values
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setValues(List<String> values) {
        this.values = values;
    }

}
