
package org.iringtools.library;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for DatabaseDictionary complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="DatabaseDictionary">
 *   &lt;complexContent>
 *     &lt;extension base="{http://www.iringtools.org/library}DataDictionary">
 *       &lt;sequence>
 *         &lt;element name="provider" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="connectionString" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="schemaName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="identityConfiguration" type="{http://www.iringtools.org/library}IdentityConfiguration"/>
 *       &lt;/sequence>
 *     &lt;/extension>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "DatabaseDictionary", propOrder = {
    "provider",
    "connectionString",
    "schemaName",
    "identityConfiguration"
})
@XmlRootElement(name = "dataBaseDictionary")
public class DatabaseDictionary
    extends DataDictionary
{

    @XmlElement(required = true)
    protected String provider;
    @XmlElement(required = true)
    protected String connectionString;
    @XmlElement(required = true)
    protected String schemaName;
    @XmlElement(required = true)
    protected IdentityConfiguration identityConfiguration;

    /**
     * Gets the value of the provider property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getProvider() {
        return provider;
    }

    /**
     * Sets the value of the provider property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setProvider(String value) {
        this.provider = value;
    }

    /**
     * Gets the value of the connectionString property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getConnectionString() {
        return connectionString;
    }

    /**
     * Sets the value of the connectionString property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setConnectionString(String value) {
        this.connectionString = value;
    }

    /**
     * Gets the value of the schemaName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSchemaName() {
        return schemaName;
    }

    /**
     * Sets the value of the schemaName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSchemaName(String value) {
        this.schemaName = value;
    }

    /**
     * Gets the value of the identityConfiguration property.
     * 
     * @return
     *     possible object is
     *     {@link IdentityConfiguration }
     *     
     */
    public IdentityConfiguration getIdentityConfiguration() {
        return identityConfiguration;
    }

    /**
     * Sets the value of the identityConfiguration property.
     * 
     * @param value
     *     allowed object is
     *     {@link IdentityConfiguration }
     *     
     */
    public void setIdentityConfiguration(IdentityConfiguration value) {
        this.identityConfiguration = value;
    }

}
