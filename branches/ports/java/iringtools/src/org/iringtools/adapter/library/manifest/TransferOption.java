//
// This file was generated by the JavaTM Architecture for XML Binding(JAXB) Reference Implementation, vhudson-jaxb-ri-2.2-7 
// See <a href="http://java.sun.com/xml/jaxb">http://java.sun.com/xml/jaxb</a> 
// Any modifications to this file will be lost upon recompilation of the source schema. 
// Generated on: 2010.07.26 at 06:10:20 PM EDT 
//


package org.iringtools.adapter.library.manifest;

import javax.xml.bind.annotation.XmlEnum;
import javax.xml.bind.annotation.XmlEnumValue;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for TransferOption.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * <p>
 * <pre>
 * &lt;simpleType name="TransferOption">
 *   &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *     &lt;enumeration value="Desired"/>
 *     &lt;enumeration value="Required"/>
 *   &lt;/restriction>
 * &lt;/simpleType>
 * </pre>
 * 
 */
@XmlType(name = "TransferOption")
@XmlEnum
public enum TransferOption {

    @XmlEnumValue("Desired")
    DESIRED("Desired"),
    @XmlEnumValue("Required")
    REQUIRED("Required");
    private final String value;

    TransferOption(String v) {
        value = v;
    }

    public String value() {
        return value;
    }

    public static TransferOption fromValue(String v) {
        for (TransferOption c: TransferOption.values()) {
            if (c.value.equals(v)) {
                return c;
            }
        }
        throw new IllegalArgumentException(v);
    }

}
