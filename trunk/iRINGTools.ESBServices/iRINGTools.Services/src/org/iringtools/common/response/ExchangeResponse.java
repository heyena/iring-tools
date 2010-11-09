
package org.iringtools.common.response;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlSchemaType;
import javax.xml.bind.annotation.XmlType;
import javax.xml.datatype.XMLGregorianCalendar;


/**
 * <p>Java class for ExchangeResponse complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="ExchangeResponse">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="level" type="{http://iringtools.org/common/response}Level"/>
 *         &lt;element name="startTimeStamp" type="{http://www.w3.org/2001/XMLSchema}dateTime"/>
 *         &lt;element name="endTimeStamp" type="{http://www.w3.org/2001/XMLSchema}dateTime"/>
 *         &lt;element name="senderUri" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="senderScopeName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="senderAppName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="senderGraphName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="receiverUri" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="receiverScopeName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="receiverAppName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="receiverGraphName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="statusList" type="{http://iringtools.org/common/response}StatusList"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "ExchangeResponse", propOrder = {
    "level",
    "startTimeStamp",
    "endTimeStamp",
    "senderUri",
    "senderScopeName",
    "senderAppName",
    "senderGraphName",
    "receiverUri",
    "receiverScopeName",
    "receiverAppName",
    "receiverGraphName",
    "statusList"
})
@XmlRootElement(name = "exchangeResponse")
public class ExchangeResponse {

    @XmlElement(required = true)
    protected Level level;
    @XmlElement(required = true)
    @XmlSchemaType(name = "dateTime")
    protected XMLGregorianCalendar startTimeStamp;
    @XmlElement(required = true)
    @XmlSchemaType(name = "dateTime")
    protected XMLGregorianCalendar endTimeStamp;
    @XmlElement(required = true)
    protected String senderUri;
    @XmlElement(required = true)
    protected String senderScopeName;
    @XmlElement(required = true)
    protected String senderAppName;
    @XmlElement(required = true)
    protected String senderGraphName;
    @XmlElement(required = true)
    protected String receiverUri;
    @XmlElement(required = true)
    protected String receiverScopeName;
    @XmlElement(required = true)
    protected String receiverAppName;
    @XmlElement(required = true)
    protected String receiverGraphName;
    @XmlElement(required = true)
    protected StatusList statusList;

    /**
     * Gets the value of the level property.
     * 
     * @return
     *     possible object is
     *     {@link Level }
     *     
     */
    public Level getLevel() {
        return level;
    }

    /**
     * Sets the value of the level property.
     * 
     * @param value
     *     allowed object is
     *     {@link Level }
     *     
     */
    public void setLevel(Level value) {
        this.level = value;
    }

    /**
     * Gets the value of the startTimeStamp property.
     * 
     * @return
     *     possible object is
     *     {@link XMLGregorianCalendar }
     *     
     */
    public XMLGregorianCalendar getStartTimeStamp() {
        return startTimeStamp;
    }

    /**
     * Sets the value of the startTimeStamp property.
     * 
     * @param value
     *     allowed object is
     *     {@link XMLGregorianCalendar }
     *     
     */
    public void setStartTimeStamp(XMLGregorianCalendar value) {
        this.startTimeStamp = value;
    }

    /**
     * Gets the value of the endTimeStamp property.
     * 
     * @return
     *     possible object is
     *     {@link XMLGregorianCalendar }
     *     
     */
    public XMLGregorianCalendar getEndTimeStamp() {
        return endTimeStamp;
    }

    /**
     * Sets the value of the endTimeStamp property.
     * 
     * @param value
     *     allowed object is
     *     {@link XMLGregorianCalendar }
     *     
     */
    public void setEndTimeStamp(XMLGregorianCalendar value) {
        this.endTimeStamp = value;
    }

    /**
     * Gets the value of the senderUri property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSenderUri() {
        return senderUri;
    }

    /**
     * Sets the value of the senderUri property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSenderUri(String value) {
        this.senderUri = value;
    }

    /**
     * Gets the value of the senderScopeName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSenderScopeName() {
        return senderScopeName;
    }

    /**
     * Sets the value of the senderScopeName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSenderScopeName(String value) {
        this.senderScopeName = value;
    }

    /**
     * Gets the value of the senderAppName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSenderAppName() {
        return senderAppName;
    }

    /**
     * Sets the value of the senderAppName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSenderAppName(String value) {
        this.senderAppName = value;
    }

    /**
     * Gets the value of the senderGraphName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSenderGraphName() {
        return senderGraphName;
    }

    /**
     * Sets the value of the senderGraphName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSenderGraphName(String value) {
        this.senderGraphName = value;
    }

    /**
     * Gets the value of the receiverUri property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getReceiverUri() {
        return receiverUri;
    }

    /**
     * Sets the value of the receiverUri property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setReceiverUri(String value) {
        this.receiverUri = value;
    }

    /**
     * Gets the value of the receiverScopeName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getReceiverScopeName() {
        return receiverScopeName;
    }

    /**
     * Sets the value of the receiverScopeName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setReceiverScopeName(String value) {
        this.receiverScopeName = value;
    }

    /**
     * Gets the value of the receiverAppName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getReceiverAppName() {
        return receiverAppName;
    }

    /**
     * Sets the value of the receiverAppName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setReceiverAppName(String value) {
        this.receiverAppName = value;
    }

    /**
     * Gets the value of the receiverGraphName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getReceiverGraphName() {
        return receiverGraphName;
    }

    /**
     * Sets the value of the receiverGraphName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setReceiverGraphName(String value) {
        this.receiverGraphName = value;
    }

    /**
     * Gets the value of the statusList property.
     * 
     * @return
     *     possible object is
     *     {@link StatusList }
     *     
     */
    public StatusList getStatusList() {
        return statusList;
    }

    /**
     * Sets the value of the statusList property.
     * 
     * @param value
     *     allowed object is
     *     {@link StatusList }
     *     
     */
    public void setStatusList(StatusList value) {
        this.statusList = value;
    }

}
