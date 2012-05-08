
package org.iringtools.mapping;

import javax.xml.bind.annotation.XmlEnum;
import javax.xml.bind.annotation.XmlEnumValue;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for ClassificationStyle.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * <p>
 * <pre>
 * &lt;simpleType name="ClassificationStyle">
 *   &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *     &lt;enumeration value="Type"/>
 *     &lt;enumeration value="Template"/>
 *     &lt;enumeration value="Both"/>
 *   &lt;/restriction>
 * &lt;/simpleType>
 * </pre>
 * 
 */
@XmlType(name = "ClassificationStyle", namespace = "http://www.iringtools.org/mapping")
@XmlEnum
public enum ClassificationStyle {

    @XmlEnumValue("Type")
    TYPE("Type"),
    @XmlEnumValue("Template")
    TEMPLATE("Template"),
    @XmlEnumValue("Both")
    BOTH("Both");
    private final String value;

    ClassificationStyle(String v) {
        value = v;
    }

    public String value() {
        return value;
    }

    public static ClassificationStyle fromValue(String v) {
        for (ClassificationStyle c: ClassificationStyle.values()) {
            if (c.value.equals(v)) {
                return c;
            }
        }
        throw new IllegalArgumentException(v);
    }

}
