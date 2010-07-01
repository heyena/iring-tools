//
// This file was generated by the JavaTM Architecture for XML Binding(JAXB) Reference Implementation, vhudson-jaxb-ri-2.2-7 
// See <a href="http://java.sun.com/xml/jaxb">http://java.sun.com/xml/jaxb</a> 
// Any modifications to this file will be lost upon recompilation of the source schema. 
// Generated on: 2010.07.01 at 04:29:48 PM EDT 
//


package org.iringtools.adapter.library.manifest;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for anonymous complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType>
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="graphMaps">
 *           &lt;complexType>
 *             &lt;complexContent>
 *               &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *                 &lt;sequence>
 *                   &lt;element name="GraphMap" type="{http://iringtools.org/adapter/library/manifest}GraphMap" maxOccurs="unbounded" minOccurs="0"/>
 *                 &lt;/sequence>
 *               &lt;/restriction>
 *             &lt;/complexContent>
 *           &lt;/complexType>
 *         &lt;/element>
 *         &lt;element name="valueLists">
 *           &lt;complexType>
 *             &lt;complexContent>
 *               &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *                 &lt;sequence>
 *                   &lt;element name="ValueList" type="{http://iringtools.org/adapter/library/manifest}ValueList" maxOccurs="unbounded" minOccurs="0"/>
 *                 &lt;/sequence>
 *               &lt;/restriction>
 *             &lt;/complexContent>
 *           &lt;/complexType>
 *         &lt;/element>
 *         &lt;element name="version" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "", propOrder = {
    "graphMaps",
    "valueLists",
    "version"
})
@XmlRootElement(name = "Manifest")
public class Manifest {

    @XmlElement(required = true)
    protected Manifest.GraphMaps graphMaps;
    @XmlElement(required = true)
    protected Manifest.ValueLists valueLists;
    @XmlElement(required = true)
    protected String version;

    /**
     * Gets the value of the graphMaps property.
     * 
     * @return
     *     possible object is
     *     {@link Manifest.GraphMaps }
     *     
     */
    public Manifest.GraphMaps getGraphMaps() {
        return graphMaps;
    }

    /**
     * Sets the value of the graphMaps property.
     * 
     * @param value
     *     allowed object is
     *     {@link Manifest.GraphMaps }
     *     
     */
    public void setGraphMaps(Manifest.GraphMaps value) {
        this.graphMaps = value;
    }

    /**
     * Gets the value of the valueLists property.
     * 
     * @return
     *     possible object is
     *     {@link Manifest.ValueLists }
     *     
     */
    public Manifest.ValueLists getValueLists() {
        return valueLists;
    }

    /**
     * Sets the value of the valueLists property.
     * 
     * @param value
     *     allowed object is
     *     {@link Manifest.ValueLists }
     *     
     */
    public void setValueLists(Manifest.ValueLists value) {
        this.valueLists = value;
    }

    /**
     * Gets the value of the version property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getVersion() {
        return version;
    }

    /**
     * Sets the value of the version property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setVersion(String value) {
        this.version = value;
    }


    /**
     * <p>Java class for anonymous complex type.
     * 
     * <p>The following schema fragment specifies the expected content contained within this class.
     * 
     * <pre>
     * &lt;complexType>
     *   &lt;complexContent>
     *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
     *       &lt;sequence>
     *         &lt;element name="GraphMap" type="{http://iringtools.org/adapter/library/manifest}GraphMap" maxOccurs="unbounded" minOccurs="0"/>
     *       &lt;/sequence>
     *     &lt;/restriction>
     *   &lt;/complexContent>
     * &lt;/complexType>
     * </pre>
     * 
     * 
     */
    @XmlAccessorType(XmlAccessType.FIELD)
    @XmlType(name = "", propOrder = {
        "graphMap"
    })
    public static class GraphMaps {

        @XmlElement(name = "GraphMap")
        protected List<GraphMap> graphMap;

        /**
         * Gets the value of the graphMap property.
         * 
         * <p>
         * This accessor method returns a reference to the live list,
         * not a snapshot. Therefore any modification you make to the
         * returned list will be present inside the JAXB object.
         * This is why there is not a <CODE>set</CODE> method for the graphMap property.
         * 
         * <p>
         * For example, to add a new item, do as follows:
         * <pre>
         *    getGraphMap().add(newItem);
         * </pre>
         * 
         * 
         * <p>
         * Objects of the following type(s) are allowed in the list
         * {@link GraphMap }
         * 
         * 
         */
        public List<GraphMap> getGraphMap() {
            if (graphMap == null) {
                graphMap = new ArrayList<GraphMap>();
            }
            return this.graphMap;
        }

        /**
         * Sets the value of the graphMap property.
         * 
         * @param graphMap
         *     allowed object is
         *     {@link GraphMap }
         *     
         */
        public void setGraphMap(List<GraphMap> graphMap) {
            this.graphMap = graphMap;
        }

    }


    /**
     * <p>Java class for anonymous complex type.
     * 
     * <p>The following schema fragment specifies the expected content contained within this class.
     * 
     * <pre>
     * &lt;complexType>
     *   &lt;complexContent>
     *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
     *       &lt;sequence>
     *         &lt;element name="ValueList" type="{http://iringtools.org/adapter/library/manifest}ValueList" maxOccurs="unbounded" minOccurs="0"/>
     *       &lt;/sequence>
     *     &lt;/restriction>
     *   &lt;/complexContent>
     * &lt;/complexType>
     * </pre>
     * 
     * 
     */
    @XmlAccessorType(XmlAccessType.FIELD)
    @XmlType(name = "", propOrder = {
        "valueList"
    })
    public static class ValueLists {

        @XmlElement(name = "ValueList")
        protected List<ValueList> valueList;

        /**
         * Gets the value of the valueList property.
         * 
         * <p>
         * This accessor method returns a reference to the live list,
         * not a snapshot. Therefore any modification you make to the
         * returned list will be present inside the JAXB object.
         * This is why there is not a <CODE>set</CODE> method for the valueList property.
         * 
         * <p>
         * For example, to add a new item, do as follows:
         * <pre>
         *    getValueList().add(newItem);
         * </pre>
         * 
         * 
         * <p>
         * Objects of the following type(s) are allowed in the list
         * {@link ValueList }
         * 
         * 
         */
        public List<ValueList> getValueList() {
            if (valueList == null) {
                valueList = new ArrayList<ValueList>();
            }
            return this.valueList;
        }

        /**
         * Sets the value of the valueList property.
         * 
         * @param valueList
         *     allowed object is
         *     {@link ValueList }
         *     
         */
        public void setValueList(List<ValueList> valueList) {
            this.valueList = valueList;
        }

    }

}
