
package org.iringtools.directory;

import javax.xml.bind.annotation.XmlRegistry;


/**
 * This object contains factory methods for each 
 * Java content interface and Java element interface 
 * generated in the org.iringtools.directory package. 
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
     * Create a new ObjectFactory that can be used to create new instances of schema derived classes for package: org.iringtools.directory
     * 
     */
    public ObjectFactory() {
    }

    /**
     * Create an instance of {@link Graphs }
     * 
     */
    public Graphs createGraphs() {
        return new Graphs();
    }

    /**
     * Create an instance of {@link Exchanges }
     * 
     */
    public Exchanges createExchanges() {
        return new Exchanges();
    }

    /**
     * Create an instance of {@link ApplicationData }
     * 
     */
    public ApplicationData createApplicationData() {
        return new ApplicationData();
    }

    /**
     * Create an instance of {@link Application }
     * 
     */
    public Application createApplication() {
        return new Application();
    }

    /**
     * Create an instance of {@link DataExchanges }
     * 
     */
    public DataExchanges createDataExchanges() {
        return new DataExchanges();
    }

    /**
     * Create an instance of {@link Exchange }
     * 
     */
    public Exchange createExchange() {
        return new Exchange();
    }

    /**
     * Create an instance of {@link Scope }
     * 
     */
    public Scope createScope() {
        return new Scope();
    }

    /**
     * Create an instance of {@link Graph }
     * 
     */
    public Graph createGraph() {
        return new Graph();
    }

    /**
     * Create an instance of {@link ExchangeDefinition }
     * 
     */
    public ExchangeDefinition createExchangeDefinition() {
        return new ExchangeDefinition();
    }

    /**
     * Create an instance of {@link Directory }
     * 
     */
    public Directory createDirectory() {
        return new Directory();
    }

    /**
     * Create an instance of {@link Commodity }
     * 
     */
    public Commodity createCommodity() {
        return new Commodity();
    }

}
