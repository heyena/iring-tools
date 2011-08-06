
package com.apigee.api.wadl._2010._07;

import javax.xml.bind.JAXBElement;
import javax.xml.bind.annotation.XmlElementDecl;
import javax.xml.bind.annotation.XmlRegistry;
import javax.xml.namespace.QName;


/**
 * This object contains factory methods for each 
 * Java content interface and Java element interface 
 * generated in the com.apigee.api.wadl._2010._07 package. 
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

    private final static QName _Payload_QNAME = new QName("http://api.apigee.com/wadl/2010/07/", "payload");
    private final static QName _Example_QNAME = new QName("http://api.apigee.com/wadl/2010/07/", "example");
    private final static QName _Tags_QNAME = new QName("http://api.apigee.com/wadl/2010/07/", "tags");
    private final static QName _Choice_QNAME = new QName("http://api.apigee.com/wadl/2010/07/", "choice");
    private final static QName _Authentication_QNAME = new QName("http://api.apigee.com/wadl/2010/07/", "authentication");

    /**
     * Create a new ObjectFactory that can be used to create new instances of schema derived classes for package: com.apigee.api.wadl._2010._07
     * 
     */
    public ObjectFactory() {
    }

    /**
     * Create an instance of {@link ExampleType }
     * 
     */
    public ExampleType createExampleType() {
        return new ExampleType();
    }

    /**
     * Create an instance of {@link TagsType }
     * 
     */
    public TagsType createTagsType() {
        return new TagsType();
    }

    /**
     * Create an instance of {@link AuthenticationType }
     * 
     */
    public AuthenticationType createAuthenticationType() {
        return new AuthenticationType();
    }

    /**
     * Create an instance of {@link ChoiceType }
     * 
     */
    public ChoiceType createChoiceType() {
        return new ChoiceType();
    }

    /**
     * Create an instance of {@link TagType }
     * 
     */
    public TagType createTagType() {
        return new TagType();
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link String }{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "http://api.apigee.com/wadl/2010/07/", name = "payload")
    public JAXBElement<String> createPayload(String value) {
        return new JAXBElement<String>(_Payload_QNAME, String.class, null, value);
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link ExampleType }{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "http://api.apigee.com/wadl/2010/07/", name = "example")
    public JAXBElement<ExampleType> createExample(ExampleType value) {
        return new JAXBElement<ExampleType>(_Example_QNAME, ExampleType.class, null, value);
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link TagsType }{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "http://api.apigee.com/wadl/2010/07/", name = "tags")
    public JAXBElement<TagsType> createTags(TagsType value) {
        return new JAXBElement<TagsType>(_Tags_QNAME, TagsType.class, null, value);
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link ChoiceType }{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "http://api.apigee.com/wadl/2010/07/", name = "choice")
    public JAXBElement<ChoiceType> createChoice(ChoiceType value) {
        return new JAXBElement<ChoiceType>(_Choice_QNAME, ChoiceType.class, null, value);
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link AuthenticationType }{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "http://api.apigee.com/wadl/2010/07/", name = "authentication")
    public JAXBElement<AuthenticationType> createAuthentication(AuthenticationType value) {
        return new JAXBElement<AuthenticationType>(_Authentication_QNAME, AuthenticationType.class, null, value);
    }

}
