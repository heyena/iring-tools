
package org.iringtools.directory;

import javax.xml.bind.annotation.XmlEnum;
import javax.xml.bind.annotation.XmlEnumValue;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for AciRight.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * <p>
 * <pre>
 * &lt;simpleType name="AciRight">
 *   &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *     &lt;enumeration value="All"/>
 *     &lt;enumeration value="Read"/>
 *     &lt;enumeration value="Add"/>
 *     &lt;enumeration value="Change"/>
 *     &lt;enumeration value="Delete"/>
 *   &lt;/restriction>
 * &lt;/simpleType>
 * </pre>
 * 
 */
@XmlType(name = "AciRight")
@XmlEnum
public enum AciRight {

    @XmlEnumValue("All")
    ALL("All"),
    @XmlEnumValue("Read")
    READ("Read"),
    @XmlEnumValue("Add")
    ADD("Add"),
    @XmlEnumValue("Change")
    CHANGE("Change"),
    @XmlEnumValue("Delete")
    DELETE("Delete");
    private final String value;

    AciRight(String v) {
        value = v;
    }

    public String value() {
        return value;
    }

    public static AciRight fromValue(String v) {
        for (AciRight c: AciRight.values()) {
            if (c.value.equals(v)) {
                return c;
            }
        }
        throw new IllegalArgumentException(v);
    }

}
