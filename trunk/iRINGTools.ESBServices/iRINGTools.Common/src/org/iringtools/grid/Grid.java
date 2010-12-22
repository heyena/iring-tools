
package org.iringtools.grid;

import java.util.ArrayList;
import java.util.List;
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
 *         &lt;element name="filterSet" type="{http://www.iringtools.org/grid}filter" maxOccurs="unbounded"/>
 *         &lt;element name="headerList" type="{http://www.iringtools.org/grid}header" maxOccurs="unbounded"/>
 *         &lt;element name="relatedItemList" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="ColumnData" type="{http://www.iringtools.org/grid}column" maxOccurs="unbounded"/>
 *         &lt;element name="ClassObjName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="success" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="cacheData" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="pageSize" type="{http://www.w3.org/2001/XMLSchema}double"/>
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
    "filterSets",
    "headerLists",
    "columnDatas",
    "classObjName",
    "success",
    "cacheData",
    "pageSize"
})
@XmlRootElement(name = "grid")
public class Grid {

    @XmlElement(name = "filterSet", required = true)
    protected List<Filter> filterSets;
    @XmlElement(name = "headerList", required = true)
    protected List<Header> headerLists;
    @XmlElement(name = "ColumnData", required = true)
    protected List<Column> columnDatas;
    @XmlElement(name = "ClassObjName", required = true)
    protected String classObjName;
    @XmlElement(required = true)
    protected String success;
    @XmlElement(required = true)
    protected String cacheData;
    protected double pageSize;

    /**
     * Gets the value of the filterSets property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the filterSets property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getFilterSets().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Filter }
     * 
     * 
     */
    public List<Filter> getFilterSets() {
        if (filterSets == null) {
            filterSets = new ArrayList<Filter>();
        }
        return this.filterSets;
    }

    /**
     * Gets the value of the headerLists property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the headerLists property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getHeaderLists().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Header }
     * 
     * 
     */
    public List<Header> getHeaderLists() {
        if (headerLists == null) {
            headerLists = new ArrayList<Header>();
        }
        return this.headerLists;
    }

   


    /**
     * Gets the value of the columnDatas property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the columnDatas property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getColumnDatas().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Column }
     * 
     * 
     */
    public List<Column> getColumnDatas() {
        if (columnDatas == null) {
            columnDatas = new ArrayList<Column>();
        }
        return this.columnDatas;
    }

    /**
     * Gets the value of the classObjName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getClassObjName() {
        return classObjName;
    }

    /**
     * Sets the value of the classObjName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setClassObjName(String value) {
        this.classObjName = value;
    }

    /**
     * Gets the value of the success property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSuccess() {
        return success;
    }

    /**
     * Sets the value of the success property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSuccess(String value) {
        this.success = value;
    }

    /**
     * Gets the value of the cacheData property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getCacheData() {
        return cacheData;
    }

    /**
     * Sets the value of the cacheData property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setCacheData(String value) {
        this.cacheData = value;
    }

    /**
     * Gets the value of the pageSize property.
     * 
     */
    public double getPageSize() {
        return pageSize;
    }

    /**
     * Sets the value of the pageSize property.
     * 
     */
    public void setPageSize(double value) {
        this.pageSize = value;
    }

    /**
     * Sets the value of the filterSets property.
     * 
     * @param filterSets
     *     allowed object is
     *     {@link Filter }
     *     
     */
    public void setFilterSets(List<Filter> filterSets) {
        this.filterSets = filterSets;
    }

    /**
     * Sets the value of the headerLists property.
     * 
     * @param headerLists
     *     allowed object is
     *     {@link Header }
     *     
     */
    public void setHeaderLists(List<Header> headerLists) {
        this.headerLists = headerLists;
    }

    /**
     * Sets the value of the columnDatas property.
     * 
     * @param columnDatas
     *     allowed object is
     *     {@link Column }
     *     
     */
    public void setColumnDatas(List<Column> columnDatas) {
        this.columnDatas = columnDatas;
    }

}
