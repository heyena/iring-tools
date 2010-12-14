
package org.iringtools.grid;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for column complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="column">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="id" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="header" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="width" type="{http://www.w3.org/2001/XMLSchema}double"/>
 *         &lt;element name="sortable" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="dataIndex" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "column", propOrder = {
    "id",
    "header",
    "width",
    "sortable",
    "dataIndex"
})
public class Column {

    @XmlElement(required = true)
    protected String id;
    @XmlElement(required = true)
    protected String header;
    protected double width;
    @XmlElement(required = true)
    protected String sortable;
    @XmlElement(required = true)
    protected String dataIndex;

    /**
     * Gets the value of the id property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getId() {
        return id;
    }

    /**
     * Sets the value of the id property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setId(String value) {
        this.id = value;
    }

    /**
     * Gets the value of the header property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getHeader() {
        return header;
    }

    /**
     * Sets the value of the header property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setHeader(String value) {
        this.header = value;
    }

    /**
     * Gets the value of the width property.
     * 
     */
    public double getWidth() {
        return width;
    }

    /**
     * Sets the value of the width property.
     * 
     */
    public void setWidth(double value) {
        this.width = value;
    }

    /**
     * Gets the value of the sortable property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSortable() {
        return sortable;
    }

    /**
     * Sets the value of the sortable property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSortable(String value) {
        this.sortable = value;
    }

    /**
     * Gets the value of the dataIndex property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getDataIndex() {
        return dataIndex;
    }

    /**
     * Sets the value of the dataIndex property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setDataIndex(String value) {
        this.dataIndex = value;
    }

}
