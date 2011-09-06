
package org.iringtools.data.filter;

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
 *         &lt;element name="expressions" type="{http://www.iringtools.org/data/filter}Expressions"/>
 *         &lt;element name="orderExpressions" type="{http://www.iringtools.org/data/filter}OrderExpressions"/>
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
    protected Expressions expressions;
    @XmlElement(required = true)
    protected OrderExpressions orderExpressions;

    /**
     * Gets the value of the expressions property.
     * 
     * @return
     *     possible object is
     *     {@link Expressions }
     *     
     */
    public Expressions getExpressions() {
        return expressions;
    }

    /**
     * Sets the value of the expressions property.
     * 
     * @param value
     *     allowed object is
     *     {@link Expressions }
     *     
     */
    public void setExpressions(Expressions value) {
        this.expressions = value;
    }

    /**
     * Gets the value of the orderExpressions property.
     * 
     * @return
     *     possible object is
     *     {@link OrderExpressions }
     *     
     */
    public OrderExpressions getOrderExpressions() {
        return orderExpressions;
    }

    /**
     * Sets the value of the orderExpressions property.
     * 
     * @param value
     *     allowed object is
     *     {@link OrderExpressions }
     *     
     */
    public void setOrderExpressions(OrderExpressions value) {
        this.orderExpressions = value;
    }

}
