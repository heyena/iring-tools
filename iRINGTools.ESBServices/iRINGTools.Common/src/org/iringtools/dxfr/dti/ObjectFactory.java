
package org.iringtools.dxfr.dti;

import javax.xml.bind.annotation.XmlRegistry;


/**
 * This object contains factory methods for each 
 * Java content interface and Java element interface 
 * generated in the org.iringtools.dxfr.dti package. 
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
     * Create a new ObjectFactory that can be used to create new instances of schema derived classes for package: org.iringtools.dxfr.dti
     * 
     */
    public ObjectFactory() {
    }

    /**
     * Create an instance of {@link DataTransferIndex }
     * 
     */
    public DataTransferIndex createDataTransferIndex() {
        return new DataTransferIndex();
    }

    /**
     * Create an instance of {@link DataTransferIndexList }
     * 
     */
    public DataTransferIndexList createDataTransferIndexList() {
        return new DataTransferIndexList();
    }

    /**
     * Create an instance of {@link DataTransferIndices }
     * 
     */
    public DataTransferIndices createDataTransferIndices() {
        return new DataTransferIndices();
    }

}
