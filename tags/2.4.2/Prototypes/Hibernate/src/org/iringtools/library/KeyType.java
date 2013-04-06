
package org.iringtools.library;

import javax.xml.bind.annotation.XmlEnum;
import javax.xml.bind.annotation.XmlEnumValue;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for KeyType.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * <p>
 * <pre>
 * &lt;simpleType name="KeyType">
 *   &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *     &lt;enumeration value="unassigned"/>
 *     &lt;enumeration value="assigned"/>
 *     &lt;enumeration value="foreign"/>
 *     &lt;enumeration value="identity"/>
 *     &lt;enumeration value="sequence"/>
 *   &lt;/restriction>
 * &lt;/simpleType>
 * </pre>
 * 
 */
@XmlType(name = "KeyType")
@XmlEnum
public enum KeyType {

    @XmlEnumValue("unassigned")
    UNASSIGNED("unassigned"),
    @XmlEnumValue("assigned")
    ASSIGNED("assigned"),
    @XmlEnumValue("foreign")
    FOREIGN("foreign"),
    @XmlEnumValue("identity")
    IDENTITY("identity"),
    @XmlEnumValue("sequence")
    SEQUENCE("sequence");
    private final String value;

    KeyType(String v) {
        value = v;
    }

    public String value() {
        return value;
    }

    public static KeyType fromValue(String v) {
        for (KeyType c: KeyType.values()) {
            if (c.value.equals(v)) {
                return c;
            }
        }
        throw new IllegalArgumentException(v);
    }

}
