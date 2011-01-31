
package org.iringtools.dxfr.datafilter;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for DataFilter complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="DataFilter">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="expressions" type="{http://www.iringtools.org/dxfr/dataFilter}ExpressionList"/>
 *         &lt;element name="orderExpressions" type="{http://www.iringtools.org/dxfr/dataFilter}OrderExpressionList"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "DataFilter", propOrder = {
    "expressions",
    "orderExpressions"
})
@XmlRootElement(name = "dataFilter")
public class DataFilter {

    @XmlElement(required = true)
    protected ExpressionList expressions;
    @XmlElement(required = true)
    protected OrderExpressionList orderExpressions;

    /**
     * Gets the value of the expressions property.
     * 
     * @return
     *     possible object is
     *     {@link ExpressionList }
     *     
     */
    public ExpressionList getExpressions() {
        return expressions;
    }

    /**
     * Sets the value of the expressions property.
     * 
     * @param value
     *     allowed object is
     *     {@link ExpressionList }
     *     
     */
    public void setExpressions(ExpressionList value) {
        this.expressions = value;
    }

    /**
     * Gets the value of the orderExpressions property.
     * 
     * @return
     *     possible object is
     *     {@link OrderExpressionList }
     *     
     */
    public OrderExpressionList getOrderExpressions() {
        return orderExpressions;
    }

    /**
     * Sets the value of the orderExpressions property.
     * 
     * @param value
     *     allowed object is
     *     {@link OrderExpressionList }
     *     
     */
    public void setOrderExpressions(OrderExpressionList value) {
        this.orderExpressions = value;
    }

}
