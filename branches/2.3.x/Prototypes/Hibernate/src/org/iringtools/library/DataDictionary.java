
package org.iringtools.library;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlSeeAlso;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for DataDictionary complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="DataDictionary">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="dataObjects" type="{http://www.iringtools.org/library}DataObject" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "DataDictionary", propOrder = {
    "dataObjects"
})
@XmlSeeAlso({
    DatabaseDictionary.class
})
public class DataDictionary {

    @XmlElement(required = true)
    protected List<DataObject> dataObjects;

    /**
     * Gets the value of the dataObjects property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the dataObjects property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getDataObjects().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link DataObject }
     * 
     * 
     */
    public List<DataObject> getDataObjects() {
        if (dataObjects == null) {
            dataObjects = new ArrayList<DataObject>();
        }
        return this.dataObjects;
    }

    /**
     * Sets the value of the dataObjects property.
     * 
     * @param dataObjects
     *     allowed object is
     *     {@link DataObject }
     *     
     */
    public void setDataObjects(List<DataObject> dataObjects) {
        this.dataObjects = dataObjects;
    }

    public static boolean isNumeric(DataType dataType)
	{
	  boolean isNumeric = false;

	  DataType[] numericTypes = new DataType[] { DataType.BYTE, DataType.DECIMAL, DataType.DOUBLE, DataType.INT_16, DataType.INT_32, DataType.INT_64, DataType.SINGLE };

	  if (numericTypes.equals(dataType))
	  {
		  isNumeric = true;
	  }

	  return isNumeric;
	}
}
