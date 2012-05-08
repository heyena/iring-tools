
package org.iringtools.directory;

import javax.xml.bind.annotation.XmlEnum;
import javax.xml.bind.annotation.XmlEnumValue;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for SourceType.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * <p>
 * <pre>
 * &lt;simpleType name="SourceType">
 *   &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *     &lt;enumeration value="dxfr"/>
 *     &lt;enumeration value="data"/>
 *   &lt;/restriction>
 * &lt;/simpleType>
 * </pre>
 * 
 */
@XmlType(name = "SourceType")
@XmlEnum
public enum SourceType {

    @XmlEnumValue("dxfr")
    DXFR("dxfr"),
    @XmlEnumValue("data")
    DATA("data");
    private final String value;

    SourceType(String v) {
        value = v;
    }

    public String value() {
        return value;
    }

    public static SourceType fromValue(String v) {
        for (SourceType c: SourceType.values()) {
            if (c.value.equals(v)) {
                return c;
            }
        }
        throw new IllegalArgumentException(v);
    }

}
