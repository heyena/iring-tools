
package org.iringtools.history;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;
import org.iringtools.dxfr.response.ExchangeResponse;


/**
 * <p>Java class for History complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="History">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="response" type="{http://www.iringtools.org/dxfr/response}ExchangeResponse" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "History", propOrder = {
    "responses"
})
@XmlRootElement(name = "history")
public class History {

    @XmlElement(name = "response", required = true)
    protected List<ExchangeResponse> responses;

    /**
     * Gets the value of the responses property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the responses property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getResponses().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link ExchangeResponse }
     * 
     * 
     */
    public List<ExchangeResponse> getResponses() {
        if (responses == null) {
            responses = new ArrayList<ExchangeResponse>();
        }
        return this.responses;
    }

    /**
     * Sets the value of the responses property.
     * 
     * @param responses
     *     allowed object is
     *     {@link ExchangeResponse }
     *     
     */
    public void setResponses(List<ExchangeResponse> responses) {
        this.responses = responses;
    }

}
