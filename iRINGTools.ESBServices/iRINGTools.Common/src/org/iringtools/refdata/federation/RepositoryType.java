
package org.iringtools.refdata.federation;

import javax.xml.bind.annotation.XmlEnum;
import javax.xml.bind.annotation.XmlEnumValue;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for RepositoryType.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * <p>
 * <pre>
 * &lt;simpleType name="RepositoryType">
 *   &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *     &lt;enumeration value="Part8"/>
 *     &lt;enumeration value="RDSWIP"/>
 *     &lt;enumeration value="Camelot"/>
 *   &lt;/restriction>
 * &lt;/simpleType>
 * </pre>
 * 
 */
@XmlType(name = "RepositoryType")
@XmlEnum
public enum RepositoryType {

    @XmlEnumValue("Part8")
    PART_8("Part8"),
    RDSWIP("RDSWIP"),
    @XmlEnumValue("Camelot")
    CAMELOT("Camelot");
    private final String value;

    RepositoryType(String v) {
        value = v;
    }

    public String value() {
        return value;
    }

    public static RepositoryType fromValue(String v) {
        for (RepositoryType c: RepositoryType.values()) {
            if (c.value.equals(v)) {
                return c;
            }
        }
        throw new IllegalArgumentException(v);
    }

}
