
package org.iringtools.refdata.federation;

import javax.xml.bind.annotation.XmlRegistry;


/**
 * This object contains factory methods for each 
 * Java content interface and Java element interface 
 * generated in the org.iringtools.refdata.federation package. 
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
     * Create a new ObjectFactory that can be used to create new instances of schema derived classes for package: org.iringtools.refdata.federation
     * 
     */
    public ObjectFactory() {
    }

    /**
     * Create an instance of {@link IdGeneratorList }
     * 
     */
    public IdGeneratorList createIdGeneratorList() {
        return new IdGeneratorList();
    }

    /**
     * Create an instance of {@link Federation }
     * 
     */
    public Federation createFederation() {
        return new Federation();
    }

    /**
     * Create an instance of {@link IdGenerator }
     * 
     */
    public IdGenerator createIdGenerator() {
        return new IdGenerator();
    }

    /**
     * Create an instance of {@link Namespace }
     * 
     */
    public Namespace createNamespace() {
        return new Namespace();
    }

    /**
     * Create an instance of {@link RepositoryList }
     * 
     */
    public RepositoryList createRepositoryList() {
        return new RepositoryList();
    }

    /**
     * Create an instance of {@link Repository }
     * 
     */
    public Repository createRepository() {
        return new Repository();
    }

    /**
     * Create an instance of {@link NamespaceList }
     * 
     */
    public NamespaceList createNamespaceList() {
        return new NamespaceList();
    }

}
