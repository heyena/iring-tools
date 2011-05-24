
package org.iringtools.data.filter;

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
 *         &lt;element name="openGroupCount" type="{http://www.w3.org/2001/XMLSchema}int"/>
 *         &lt;element name="propertyName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="relationalOperator" type="{http://www.iringtools.org/data/filter}RelationalOperator"/>
 *         &lt;element name="values" type="{http://www.iringtools.org/data/filter}Values"/>
 *         &lt;element name="logicalOperator" type="{http://www.iringtools.org/data/filter}LogicalOperator"/>
 *         &lt;element name="closeGroupCount" type="{http://www.w3.org/2001/XMLSchema}int"/>
 *         &lt;element name="isCaseSensitive" type="{http://www.w3.org/2001/XMLSchema}boolean"/>
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
    "closeGroupCount",
    "isCaseSensitive"
})
public class Expression {

    protected int openGroupCount;
    @XmlElement(required = true)
    protected String propertyName;
    @XmlElement(required = true)
    protected RelationalOperator relationalOperator;
    @XmlElement(required = true)
    protected Values values;
    @XmlElement(required = true)
    protected LogicalOperator logicalOperator;
    protected int closeGroupCount;
    protected boolean isCaseSensitive;

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
     * @return
     *     possible object is
     *     {@link Values }
     *     
     */
    public Values getValues() {
        return values;
    }

    /**
     * Sets the value of the values property.
     * 
     * @param value
     *     allowed object is
     *     {@link Values }
     *     
     */
    public void setValues(Values value) {
        this.values = value;
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
     * Gets the value of the isCaseSensitive property.
     * This getter has been renamed from isIsCaseSensitive() to getIsCaseSensitive() by cxf-xjc-boolean plugin.
     * 
     */
    public boolean getIsCaseSensitive() {
        return isCaseSensitive;
    }

    /**
     * Sets the value of the isCaseSensitive property.
     * 
     */
    public void setIsCaseSensitive(boolean value) {
        this.isCaseSensitive = value;
    }

}
