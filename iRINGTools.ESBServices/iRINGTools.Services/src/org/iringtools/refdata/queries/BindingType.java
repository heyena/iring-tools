
package org.iringtools.refdata.queries;

import javax.xml.bind.annotation.XmlEnum;
import javax.xml.bind.annotation.XmlEnumValue;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for BindingType.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * <p>
 * <pre>
 * &lt;simpleType name="BindingType">
 *   &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *     &lt;enumeration value="Uri"/>
 *     &lt;enumeration value="Label"/>
 *   &lt;/restriction>
 * &lt;/simpleType>
 * </pre>
 * 
 */
@XmlType(name = "BindingType")
@XmlEnum
public enum BindingType {

    @XmlEnumValue("Uri")
    URI("Uri"),
    @XmlEnumValue("Label")
    LABEL("Label");
    private final String value;

    BindingType(String v) {
        value = v;
    }

    public String value() {
        return value;
    }

    public static BindingType fromValue(String v) {
        for (BindingType c: BindingType.values()) {
            if (c.value.equals(v)) {
                return c;
            }
        }
        throw new IllegalArgumentException(v);
    }

}
