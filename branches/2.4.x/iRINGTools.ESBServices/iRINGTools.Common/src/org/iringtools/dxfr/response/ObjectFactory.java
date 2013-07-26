
package org.iringtools.dxfr.response;

import javax.xml.bind.JAXBElement;
import javax.xml.bind.annotation.XmlElementDecl;
import javax.xml.bind.annotation.XmlRegistry;
import javax.xml.namespace.QName;


/**
 * This object contains factory methods for each 
 * Java content interface and Java element interface 
 * generated in the org.iringtools.dxfr.response package. 
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

    private final static QName _ExchangeResponse_QNAME = new QName("http://www.iringtools.org/dxfr/response", "exchangeResponse");

    /**
     * Create a new ObjectFactory that can be used to create new instances of schema derived classes for package: org.iringtools.dxfr.response
     * 
     */
    public ObjectFactory() {
    }

    /**
     * Create an instance of {@link ExchangeResponse }
     * 
     */
    public ExchangeResponse createExchangeResponse() {
        return new ExchangeResponse();
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link ExchangeResponse }{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "http://www.iringtools.org/dxfr/response", name = "exchangeResponse")
    public JAXBElement<ExchangeResponse> createExchangeResponse(ExchangeResponse value) {
        return new JAXBElement<ExchangeResponse>(_ExchangeResponse_QNAME, ExchangeResponse.class, null, value);
    }

}
