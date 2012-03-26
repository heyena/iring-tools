
package org.iringtools.library;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for DataObject complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="DataObject">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="tableName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="objectNamespace" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="objectName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="keyDelimeter" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="keyProperties" type="{http://www.iringtools.org/library}KeyProperty" maxOccurs="unbounded"/>
 *         &lt;element name="dataProperties" type="{http://www.iringtools.org/library}DataProperty" maxOccurs="unbounded"/>
 *         &lt;element name="dataRelationships" type="{http://www.iringtools.org/library}DataRelationship" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "DataObject", propOrder = {
    "tableName",
    "objectNamespace",
    "objectName",
    "keyDelimeter",
    "keyProperties",
    "dataProperties",
    "dataRelationships"
})
@XmlRootElement(name = "dataObject")
public class DataObject {

    @XmlElement(required = true)
    protected String tableName;
    @XmlElement(required = true)
    protected String objectNamespace;
    @XmlElement(required = true)
    protected String objectName;
    @XmlElement(required = true)
    protected String keyDelimeter;
    @XmlElement(required = true)
    protected List<KeyProperty> keyProperties;
    @XmlElement(required = true)
    protected List<DataProperty> dataProperties;
    @XmlElement(required = true)
    protected List<DataRelationship> dataRelationships;

    /**
     * Gets the value of the tableName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getTableName() {
        return tableName;
    }

    /**
     * Sets the value of the tableName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setTableName(String value) {
        this.tableName = value;
    }

    /**
     * Gets the value of the objectNamespace property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getObjectNamespace() {
        return objectNamespace;
    }

    /**
     * Sets the value of the objectNamespace property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setObjectNamespace(String value) {
        this.objectNamespace = value;
    }

    /**
     * Gets the value of the objectName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getObjectName() {
        return objectName;
    }

    /**
     * Sets the value of the objectName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setObjectName(String value) {
        this.objectName = value;
    }

    /**
     * Gets the value of the keyDelimeter property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getKeyDelimeter() {
        return keyDelimeter;
    }

    /**
     * Sets the value of the keyDelimeter property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setKeyDelimeter(String value) {
        this.keyDelimeter = value;
    }

    /**
     * Gets the value of the keyProperties property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the keyProperties property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getKeyProperties().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link KeyProperty }
     * 
     * 
     */
    public List<KeyProperty> getKeyProperties() {
        if (keyProperties == null) {
            keyProperties = new ArrayList<KeyProperty>();
        }
        return this.keyProperties;
    }

    /**
     * Gets the value of the dataProperties property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the dataProperties property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getDataProperties().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link DataProperty }
     * 
     * 
     */
    public List<DataProperty> getDataProperties() {
        if (dataProperties == null) {
            dataProperties = new ArrayList<DataProperty>();
        }
        return this.dataProperties;
    }

    /**
     * Gets the value of the dataRelationships property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the dataRelationships property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getDataRelationships().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link DataRelationship }
     * 
     * 
     */
    public List<DataRelationship> getDataRelationships() {
        if (dataRelationships == null) {
            dataRelationships = new ArrayList<DataRelationship>();
        }
        return this.dataRelationships;
    }

    /**
     * Sets the value of the keyProperties property.
     * 
     * @param keyProperties
     *     allowed object is
     *     {@link KeyProperty }
     *     
     */
    public void setKeyProperties(List<KeyProperty> keyProperties) {
        this.keyProperties = keyProperties;
    }

    /**
     * Sets the value of the dataProperties property.
     * 
     * @param dataProperties
     *     allowed object is
     *     {@link DataProperty }
     *     
     */
    public void setDataProperties(List<DataProperty> dataProperties) {
        this.dataProperties = dataProperties;
    }

    /**
     * Sets the value of the dataRelationships property.
     * 
     * @param dataRelationships
     *     allowed object is
     *     {@link DataRelationship }
     *     
     */
    public void setDataRelationships(List<DataRelationship> dataRelationships) {
        this.dataRelationships = dataRelationships;
    }

}
