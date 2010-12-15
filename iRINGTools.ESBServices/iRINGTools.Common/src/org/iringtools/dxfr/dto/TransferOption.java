
package org.iringtools.dxfr.dto;

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
 *     &lt;enumeration value="Sync"/>
 *     &lt;enumeration value="Add"/>
 *     &lt;enumeration value="Change"/>
 *     &lt;enumeration value="Delete"/>
 *   &lt;/restriction>
 * &lt;/simpleType>
 * </pre>
 * 
 */
@XmlType(name = "TransferOption")
@XmlEnum
public enum TransferOption {

    @XmlEnumValue("Sync")
    SYNC("Sync"),
    @XmlEnumValue("Add")
    ADD("Add"),
    @XmlEnumValue("Change")
    CHANGE("Change"),
    @XmlEnumValue("Delete")
    DELETE("Delete");
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
