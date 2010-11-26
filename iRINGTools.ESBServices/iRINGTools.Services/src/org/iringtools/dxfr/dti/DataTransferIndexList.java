
package org.iringtools.dxfr.dti;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for DataTransferIndexList complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="DataTransferIndexList">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="dataTransferIndex" type="{http://www.iringtools.org/dxfr/dti}DataTransferIndex" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "DataTransferIndexList", propOrder = {
    "dataTransferIndexListItems"
})
public class DataTransferIndexList {

    @XmlElement(name = "dataTransferIndex", required = true)
    protected List<DataTransferIndex> dataTransferIndexListItems;

    /**
     * Gets the value of the dataTransferIndexListItems property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the dataTransferIndexListItems property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getDataTransferIndexListItems().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link DataTransferIndex }
     * 
     * 
     */
    public List<DataTransferIndex> getDataTransferIndexListItems() {
        if (dataTransferIndexListItems == null) {
            dataTransferIndexListItems = new ArrayList<DataTransferIndex>();
        }
        return this.dataTransferIndexListItems;
    }

    /**
     * Sets the value of the dataTransferIndexListItems property.
     * 
     * @param dataTransferIndexListItems
     *     allowed object is
     *     {@link DataTransferIndex }
     *     
     */
    public void setDataTransferIndexListItems(List<DataTransferIndex> dataTransferIndexListItems) {
        this.dataTransferIndexListItems = dataTransferIndexListItems;
    }

}
