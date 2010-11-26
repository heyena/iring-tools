
package org.iringtools.dxfr.dto;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for DataTransferObjectList complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="DataTransferObjectList">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="dataTransferObject" type="{http://www.iringtools.org/dxfr/dto}DataTransferObject" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "DataTransferObjectList", propOrder = {
    "dataTransferObjectListItems"
})
public class DataTransferObjectList {

    @XmlElement(name = "dataTransferObject", required = true)
    protected List<DataTransferObject> dataTransferObjectListItems;

    /**
     * Gets the value of the dataTransferObjectListItems property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the dataTransferObjectListItems property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getDataTransferObjectListItems().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link DataTransferObject }
     * 
     * 
     */
    public List<DataTransferObject> getDataTransferObjectListItems() {
        if (dataTransferObjectListItems == null) {
            dataTransferObjectListItems = new ArrayList<DataTransferObject>();
        }
        return this.dataTransferObjectListItems;
    }

    /**
     * Sets the value of the dataTransferObjectListItems property.
     * 
     * @param dataTransferObjectListItems
     *     allowed object is
     *     {@link DataTransferObject }
     *     
     */
    public void setDataTransferObjectListItems(List<DataTransferObject> dataTransferObjectListItems) {
        this.dataTransferObjectListItems = dataTransferObjectListItems;
    }

}
