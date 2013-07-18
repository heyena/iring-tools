
package org.iringtools.library;

import javax.xml.bind.annotation.XmlEnum;
import javax.xml.bind.annotation.XmlEnumValue;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for RelationshipType.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * <p>
 * <pre>
 * &lt;simpleType name="RelationshipType">
 *   &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *     &lt;enumeration value="OneToOne"/>
 *     &lt;enumeration value="OneToMany"/>
 *   &lt;/restriction>
 * &lt;/simpleType>
 * </pre>
 * 
 */
@XmlType(name = "RelationshipType")
@XmlEnum
public enum RelationshipType {

    @XmlEnumValue("OneToOne")
    ONE_TO_ONE("OneToOne"),
    @XmlEnumValue("OneToMany")
    ONE_TO_MANY("OneToMany");
    private final String value;

    RelationshipType(String v) {
        value = v;
    }

    public String value() {
        return value;
    }

    public static RelationshipType fromValue(String v) {
        for (RelationshipType c: RelationshipType.values()) {
            if (c.value.equals(v)) {
                return c;
            }
        }
        throw new IllegalArgumentException(v);
    }

}
