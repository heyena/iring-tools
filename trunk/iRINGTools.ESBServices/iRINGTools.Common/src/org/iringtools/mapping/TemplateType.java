
package org.iringtools.mapping;

import javax.xml.bind.annotation.XmlEnum;
import javax.xml.bind.annotation.XmlEnumValue;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for TemplateType.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * <p>
 * <pre>
 * &lt;simpleType name="TemplateType">
 *   &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *     &lt;enumeration value="Qualification"/>
 *     &lt;enumeration value="Definition"/>
 *   &lt;/restriction>
 * &lt;/simpleType>
 * </pre>
 * 
 */
@XmlType(name = "TemplateType")
@XmlEnum
public enum TemplateType {

    @XmlEnumValue("Qualification")
    QUALIFICATION("Qualification"),
    @XmlEnumValue("Definition")
    DEFINITION("Definition");
    private final String value;

    TemplateType(String v) {
        value = v;
    }

    public String value() {
        return value;
    }

    public static TemplateType fromValue(String v) {
        for (TemplateType c: TemplateType.values()) {
            if (c.value.equals(v)) {
                return c;
            }
        }
        throw new IllegalArgumentException(v);
    }

}
