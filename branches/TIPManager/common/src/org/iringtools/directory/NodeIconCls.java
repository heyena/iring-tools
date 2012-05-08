
package org.iringtools.directory;

import javax.xml.bind.annotation.XmlEnum;
import javax.xml.bind.annotation.XmlEnumValue;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for NodeIconCls.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * <p>
 * <pre>
 * &lt;simpleType name="NodeIconCls">
 *   &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *     &lt;enumeration value="folder"/>
 *     &lt;enumeration value="project"/>
 *     &lt;enumeration value="application"/>
 *     &lt;enumeration value="resource"/>
 *     &lt;enumeration value="exchange"/>
 *   &lt;/restriction>
 * &lt;/simpleType>
 * </pre>
 * 
 */
@XmlType(name = "NodeIconCls")
@XmlEnum
public enum NodeIconCls {

    @XmlEnumValue("folder")
    FOLDER("folder"),
    @XmlEnumValue("project")
    PROJECT("project"),
    @XmlEnumValue("application")
    APPLICATION("application"),
    @XmlEnumValue("resource")
    RESOURCE("resource"),
    @XmlEnumValue("exchange")
    EXCHANGE("exchange");
    private final String value;

    NodeIconCls(String v) {
        value = v;
    }

    public String value() {
        return value;
    }

    public static NodeIconCls fromValue(String v) {
        for (NodeIconCls c: NodeIconCls.values()) {
            if (c.value.equals(v)) {
                return c;
            }
        }
        throw new IllegalArgumentException(v);
    }

}
