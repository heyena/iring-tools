
package org.iringtools.grid;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for Grid complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="Grid">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="headerList" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="relatedItemList" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="ColumnData" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "Grid", propOrder = {
    "headerList",
    "relatedItemList",
    "columnData"
})
@XmlRootElement(name = "grid")
public class Grid {

    @XmlElement(required = true)
    protected String headerList;
    @XmlElement(required = true)
    protected String relatedItemList;
    @XmlElement(name = "ColumnData", required = true)
    protected String columnData;

    /**
     * Gets the value of the headerList property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getHeaderList() {
        return headerList;
    }

    /**
     * Sets the value of the headerList property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setHeaderList(String value) {
        this.headerList = value;
    }

    /**
     * Gets the value of the relatedItemList property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getRelatedItemList() {
        return relatedItemList;
    }

    /**
     * Sets the value of the relatedItemList property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setRelatedItemList(String value) {
        this.relatedItemList = value;
    }

    /**
     * Gets the value of the columnData property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getColumnData() {
        return columnData;
    }

    /**
     * Sets the value of the columnData property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setColumnData(String value) {
        this.columnData = value;
    }

}
