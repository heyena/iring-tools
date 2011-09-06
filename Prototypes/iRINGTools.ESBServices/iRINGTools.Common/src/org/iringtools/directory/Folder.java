
package org.iringtools.directory;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import javax.xml.bind.Element;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAnyAttribute;
import javax.xml.bind.annotation.XmlAnyElement;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;
import javax.xml.namespace.QName;


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
   *         &lt;element name="endpoint" type="{http://www.w3.org/2001/XMLSchema}anyType" maxOccurs="unbounded" minOccurs="0"/>
   *         &lt;element name="exchange" type="{http://www.w3.org/2001/XMLSchema}anyType" maxOccurs="unbounded" minOccurs="0"/>
   *         &lt;any processContents='lax' namespace='##other' maxOccurs="unbounded" minOccurs="0"/>
   *       &lt;/sequence>
   *       &lt;attribute name="name" type="{http://www.w3.org/2001/XMLSchema}string" />
   *       &lt;attribute name="type" type="{http://www.w3.org/2001/XMLSchema}string" />
   *       &lt;anyAttribute processContents='lax' namespace='##other'/>
   *     &lt;/restriction>
   *   &lt;/complexContent>
   * &lt;/complexType>
   * </pre>
   * 
   * 
   */
  @XmlAccessorType(XmlAccessType.FIELD)
  @XmlType(name = "", propOrder = {
      "endpoints",
      "exchanges",
      "anies"
  })
  @XmlRootElement(name = "folder")
  public class Folder {

      @XmlElement(name = "endpoint")
      protected List<Endpoint> endpoints;
      @XmlElement(name = "exchange")
      protected List<Exchange> exchanges;
      @XmlAnyElement (lax=true)
      protected List<Object> anies;
      @XmlAttribute(name = "name")
      protected String name;
      @XmlAttribute(name = "type")
      protected String type;
      @XmlAnyAttribute
      private Map<QName, String> otherAttributes = new HashMap<QName, String>();

      /**
       * Gets the value of the endpoints property.
       * 
       * <p>
       * This accessor method returns a reference to the live list,
       * not a snapshot. Therefore any modification you make to the
       * returned list will be present inside the JAXB object.
       * This is why there is not a <CODE>set</CODE> method for the endpoints property.
       * 
       * <p>
       * For example, to add a new item, do as follows:
       * <pre>
       *    getEndpoints().add(newItem);
       * </pre>
       * 
       * 
       * <p>
       * Objects of the following type(s) are allowed in the list
       * {@link Object }
       * 
       * 
       */
      public List<Endpoint> getEndpoints() {
          if (endpoints == null) {
              endpoints = new ArrayList<Endpoint>();
          }
          return this.endpoints;
      }

      /**
       * Gets the value of the exchanges property.
       * 
       * <p>
       * This accessor method returns a reference to the live list,
       * not a snapshot. Therefore any modification you make to the
       * returned list will be present inside the JAXB object.
       * This is why there is not a <CODE>set</CODE> method for the exchanges property.
       * 
       * <p>
       * For example, to add a new item, do as follows:
       * <pre>
       *    getExchanges().add(newItem);
       * </pre>
       * 
       * 
       * <p>
       * Objects of the following type(s) are allowed in the list
       * {@link Object }
       * 
       * 
       */
      public List<Exchange> getExchanges() {
          if (exchanges == null) {
              exchanges = new ArrayList<Exchange>();
          }
          return this.exchanges;
      }

      /**
       * Gets the value of the anies property.
       * 
       * <p>
       * This accessor method returns a reference to the live list,
       * not a snapshot. Therefore any modification you make to the
       * returned list will be present inside the JAXB object.
       * This is why there is not a <CODE>set</CODE> method for the anies property.
       * 
       * <p>
       * For example, to add a new item, do as follows:
       * <pre>
       *    getAnies().add(newItem);
       * </pre>
       * 
       * 
       * <p>
       * Objects of the following type(s) are allowed in the list
       * {@link Element }
       * 
       * 
       */
      public List<Object> getAnies() {
          if (anies == null) {
              anies = new ArrayList<Object>();
          }
          return this.anies;
      }

      /**
       * Gets the value of the name property.
       * 
       * @return
       *     possible object is
       *     {@link String }
       *     
       */
      public String getName() {
          return name;
      }

      /**
       * Sets the value of the name property.
       * 
       * @param value
       *     allowed object is
       *     {@link String }
       *     
       */
      public void setName(String value) {
          this.name = value;
      }

      /**
       * Gets the value of the type property.
       * 
       * @return
       *     possible object is
       *     {@link String }
       *     
       */
      public String getType() {
          return type;
      }

      /**
       * Sets the value of the type property.
       * 
       * @param value
       *     allowed object is
       *     {@link String }
       *     
       */
      public void setType(String value) {
          this.type = value;
      }

      /**
       * Gets a map that contains attributes that aren't bound to any typed property on this class.
       * 
       * <p>
       * the map is keyed by the name of the attribute and 
       * the value is the string value of the attribute.
       * 
       * the map returned by this method is live, and you can add new attribute
       * by updating the map directly. Because of this design, there's no setter.
       * 
       * 
       * @return
       *     always non-null
       */
      public Map<QName, String> getOtherAttributes() {
          return otherAttributes;
      }

      /**
       * Sets the value of the endpoints property.
       * 
       * @param endpoints
       *     allowed object is
       *     {@link Object }
       *     
       */
      public void setEndpoints(List<Endpoint> endpoints) {
          this.endpoints = endpoints;
      }

      /**
       * Sets the value of the exchanges property.
       * 
       * @param exchanges
       *     allowed object is
       *     {@link Object }
       *     
       */
      public void setExchanges(List<Exchange> exchanges) {
          this.exchanges = exchanges;
      }

      /**
       * Sets the value of the anies property.
       * 
       * @param anies
       *     allowed object is
       *     {@link Element }
       *     
       */
      public void setAnies(List<Object> anies) {
          this.anies = anies;
      }

  }


