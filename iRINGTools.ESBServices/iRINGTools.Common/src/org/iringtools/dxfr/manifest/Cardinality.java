
package org.iringtools.dxfr.manifest;

import javax.xml.bind.annotation.XmlEnum;
import javax.xml.bind.annotation.XmlEnumValue;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for Cardinality.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * <p>
 * <pre>
 * &lt;simpleType name="Cardinality">
 *   &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *     &lt;enumeration value="Self"/>
 *     &lt;enumeration value="OneToOne"/>
 *     &lt;enumeration value="OneToMany"/>
 *   &lt;/restriction>
 * &lt;/simpleType>
 * </pre>
 * 
 */
@XmlType(name = "Cardinality")
@XmlEnum
public enum Cardinality {

    @XmlEnumValue("Self")
    SELF("Self"),
    @XmlEnumValue("OneToOne")
    ONE_TO_ONE("OneToOne"),
    @XmlEnumValue("OneToMany")
    ONE_TO_MANY("OneToMany");
    private final String value;

    Cardinality(String v) {
        value = v;
    }

    public String value() {
        return value;
    }

    public static Cardinality fromValue(String v) {
        for (Cardinality c: Cardinality.values()) {
            if (c.value.equals(v)) {
                return c;
            }
        }
        throw new IllegalArgumentException(v);
    }

}
