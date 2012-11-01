
package org.iringtools.dxfr.response;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlSchemaType;
import javax.xml.bind.annotation.XmlType;
import javax.xml.datatype.XMLGregorianCalendar;
import org.iringtools.common.response.Level;


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
 *         &lt;element name="level" type="{http://www.iringtools.org/common/response}Level"/>
 *         &lt;element name="startTime" type="{http://www.w3.org/2001/XMLSchema}dateTime"/>
 *         &lt;element name="endTime" type="{http://www.w3.org/2001/XMLSchema}dateTime"/>
 *         &lt;element name="senderUri" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="senderScope" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="senderApp" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="senderGraph" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="receiverUri" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="receiverScope" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="receiverApp" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="receiverGraph" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="itemCount" type="{http://www.w3.org/2001/XMLSchema}int"/>
 *         &lt;element name="itemCountSync" type="{http://www.w3.org/2001/XMLSchema}int"/>
 *         &lt;element name="itemCountAdd" type="{http://www.w3.org/2001/XMLSchema}int"/>
 *         &lt;element name="itemCountChange" type="{http://www.w3.org/2001/XMLSchema}int"/>
 *         &lt;element name="itemCountDelete" type="{http://www.w3.org/2001/XMLSchema}int"/>
 *         &lt;element name="poolSize" type="{http://www.w3.org/2001/XMLSchema}int"/>
 *         &lt;element name="summary" type="{http://www.w3.org/2001/XMLSchema}string"/>
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
    "level",
    "startTime",
    "endTime",
    "senderUri",
    "senderScope",
    "senderApp",
    "senderGraph",
    "receiverUri",
    "receiverScope",
    "receiverApp",
    "receiverGraph",
    "itemCount",
    "itemCountSync",
    "itemCountAdd",
    "itemCountChange",
    "itemCountDelete",
    "poolSize",
    "summary"
})
@XmlRootElement(name = "exchangeResponse")
public class ExchangeResponse {

    @XmlElement(required = true)
    protected Level level;
    @XmlElement(required = true)
    @XmlSchemaType(name = "dateTime")
    protected XMLGregorianCalendar startTime;
    @XmlElement(required = true)
    @XmlSchemaType(name = "dateTime")
    protected XMLGregorianCalendar endTime;
    @XmlElement(required = true)
    protected String senderUri;
    @XmlElement(required = true)
    protected String senderScope;
    @XmlElement(required = true)
    protected String senderApp;
    @XmlElement(required = true)
    protected String senderGraph;
    @XmlElement(required = true)
    protected String receiverUri;
    @XmlElement(required = true)
    protected String receiverScope;
    @XmlElement(required = true)
    protected String receiverApp;
    @XmlElement(required = true)
    protected String receiverGraph;
    protected int itemCount;
    protected int itemCountSync;
    protected int itemCountAdd;
    protected int itemCountChange;
    protected int itemCountDelete;
    protected int poolSize;
    @XmlElement(required = true)
    protected String summary;

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
     * Gets the value of the startTime property.
     * 
     * @return
     *     possible object is
     *     {@link XMLGregorianCalendar }
     *     
     */
    public XMLGregorianCalendar getStartTime() {
        return startTime;
    }

    /**
     * Sets the value of the startTime property.
     * 
     * @param value
     *     allowed object is
     *     {@link XMLGregorianCalendar }
     *     
     */
    public void setStartTime(XMLGregorianCalendar value) {
        this.startTime = value;
    }

    /**
     * Gets the value of the endTime property.
     * 
     * @return
     *     possible object is
     *     {@link XMLGregorianCalendar }
     *     
     */
    public XMLGregorianCalendar getEndTime() {
        return endTime;
    }

    /**
     * Sets the value of the endTime property.
     * 
     * @param value
     *     allowed object is
     *     {@link XMLGregorianCalendar }
     *     
     */
    public void setEndTime(XMLGregorianCalendar value) {
        this.endTime = value;
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
     * Gets the value of the senderScope property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSenderScope() {
        return senderScope;
    }

    /**
     * Sets the value of the senderScope property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSenderScope(String value) {
        this.senderScope = value;
    }

    /**
     * Gets the value of the senderApp property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSenderApp() {
        return senderApp;
    }

    /**
     * Sets the value of the senderApp property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSenderApp(String value) {
        this.senderApp = value;
    }

    /**
     * Gets the value of the senderGraph property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSenderGraph() {
        return senderGraph;
    }

    /**
     * Sets the value of the senderGraph property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSenderGraph(String value) {
        this.senderGraph = value;
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
     * Gets the value of the receiverScope property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getReceiverScope() {
        return receiverScope;
    }

    /**
     * Sets the value of the receiverScope property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setReceiverScope(String value) {
        this.receiverScope = value;
    }

    /**
     * Gets the value of the receiverApp property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getReceiverApp() {
        return receiverApp;
    }

    /**
     * Sets the value of the receiverApp property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setReceiverApp(String value) {
        this.receiverApp = value;
    }

    /**
     * Gets the value of the receiverGraph property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getReceiverGraph() {
        return receiverGraph;
    }

    /**
     * Sets the value of the receiverGraph property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setReceiverGraph(String value) {
        this.receiverGraph = value;
    }

    /**
     * Gets the value of the itemCount property.
     * 
     */
    public int getItemCount() {
        return itemCount;
    }

    /**
     * Sets the value of the itemCount property.
     * 
     */
    public void setItemCount(int value) {
        this.itemCount = value;
    }

    /**
     * Gets the value of the itemCountSync property.
     * 
     */
    public int getItemCountSync() {
        return itemCountSync;
    }

    /**
     * Sets the value of the itemCountSync property.
     * 
     */
    public void setItemCountSync(int value) {
        this.itemCountSync = value;
    }

    /**
     * Gets the value of the itemCountAdd property.
     * 
     */
    public int getItemCountAdd() {
        return itemCountAdd;
    }

    /**
     * Sets the value of the itemCountAdd property.
     * 
     */
    public void setItemCountAdd(int value) {
        this.itemCountAdd = value;
    }

    /**
     * Gets the value of the itemCountChange property.
     * 
     */
    public int getItemCountChange() {
        return itemCountChange;
    }

    /**
     * Sets the value of the itemCountChange property.
     * 
     */
    public void setItemCountChange(int value) {
        this.itemCountChange = value;
    }

    /**
     * Gets the value of the itemCountDelete property.
     * 
     */
    public int getItemCountDelete() {
        return itemCountDelete;
    }

    /**
     * Sets the value of the itemCountDelete property.
     * 
     */
    public void setItemCountDelete(int value) {
        this.itemCountDelete = value;
    }

    /**
     * Gets the value of the poolSize property.
     * 
     */
    public int getPoolSize() {
        return poolSize;
    }

    /**
     * Sets the value of the poolSize property.
     * 
     */
    public void setPoolSize(int value) {
        this.poolSize = value;
    }

    /**
     * Gets the value of the summary property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSummary() {
        return summary;
    }

    /**
     * Sets the value of the summary property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSummary(String value) {
        this.summary = value;
    }

}
