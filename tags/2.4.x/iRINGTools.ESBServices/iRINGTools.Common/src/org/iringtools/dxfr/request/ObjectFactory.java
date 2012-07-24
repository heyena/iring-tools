
package org.iringtools.dxfr.request;

import javax.xml.bind.annotation.XmlRegistry;


/**
 * This object contains factory methods for each 
 * Java content interface and Java element interface 
 * generated in the org.iringtools.dxfr.request package. 
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
     * Create a new ObjectFactory that can be used to create new instances of schema derived classes for package: org.iringtools.dxfr.request
     * 
     */
    public ObjectFactory() {
    }

    /**
     * Create an instance of {@link DxiRequest }
     * 
     */
    public DxiRequest createDxiRequest() {
        return new DxiRequest();
    }

    /**
     * Create an instance of {@link DfiRequest }
     * 
     */
    public DfiRequest createDfiRequest() {
        return new DfiRequest();
    }

    /**
     * Create an instance of {@link ExchangeRequest }
     * 
     */
    public ExchangeRequest createExchangeRequest() {
        return new ExchangeRequest();
    }

    /**
     * Create an instance of {@link DfoRequest }
     * 
     */
    public DfoRequest createDfoRequest() {
        return new DfoRequest();
    }

    /**
     * Create an instance of {@link DxoRequest }
     * 
     */
    public DxoRequest createDxoRequest() {
        return new DxoRequest();
    }

}
