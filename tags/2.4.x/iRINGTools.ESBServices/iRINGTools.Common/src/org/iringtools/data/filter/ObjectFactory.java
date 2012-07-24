
package org.iringtools.data.filter;

import javax.xml.bind.annotation.XmlRegistry;


/**
 * This object contains factory methods for each 
 * Java content interface and Java element interface 
 * generated in the org.iringtools.data.filter package. 
 * <p>An ObjectFactory allows you to programatically 
 * construct new instances of the Java representation 
 * for XML content. The Java representation of XML 
 * content can consist of schema derived interfaces 
 * and classes representing the binding of schema 
 * type definitions, element declarations and model 
 * groups.  Factory methods for each of these are 
 * provided in this class.
 * 
 */
@XmlRegistry
public class ObjectFactory {


    /**
     * Create a new ObjectFactory that can be used to create new instances of schema derived classes for package: org.iringtools.data.filter
     * 
     */
    public ObjectFactory() {
    }

    /**
     * Create an instance of {@link OrderExpressions }
     * 
     */
    public OrderExpressions createOrderExpressions() {
        return new OrderExpressions();
    }

    /**
     * Create an instance of {@link Expression }
     * 
     */
    public Expression createExpression() {
        return new Expression();
    }

    /**
     * Create an instance of {@link OrderExpression }
     * 
     */
    public OrderExpression createOrderExpression() {
        return new OrderExpression();
    }

    /**
     * Create an instance of {@link DataFilter }
     * 
     */
    public DataFilter createDataFilter() {
        return new DataFilter();
    }

    /**
     * Create an instance of {@link Values }
     * 
     */
    public Values createValues() {
        return new Values();
    }

    /**
     * Create an instance of {@link Expressions }
     * 
     */
    public Expressions createExpressions() {
        return new Expressions();
    }

}
