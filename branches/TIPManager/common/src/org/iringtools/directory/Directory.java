
package org.iringtools.directory;

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
 *         &lt;element ref="{http://www.iringtools.org/directory}folder" maxOccurs="unbounded" minOccurs="0"/>
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
    "folderList"
})
@XmlRootElement(name = "directory")
public class Directory {

    @XmlElement(name = "folder")
    protected List<Folder> folderList;

    /**
     * Gets the value of the folderList property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the folderList property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getFolderList().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Folder }
     * 
     * 
     */
    public List<Folder> getFolderList() {
        if (folderList == null) {
            folderList = new ArrayList<Folder>();
        }
        return this.folderList;
    }

    /**
     * Sets the value of the folderList property.
     * 
     * @param folderList
     *     allowed object is
     *     {@link Folder }
     *     
     */
    public void setFolderList(List<Folder> folderList) {
        this.folderList = folderList;
    }

}
