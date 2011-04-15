package org.iringtools.widgets.tree;

public enum Type {

	SEARCH("SearchNode"),
    CLASS("ClassNode"),
    TEMPLATE("TemplateNode"),
    CLASSIFICATION("ClassificationsNode"),
    SUPERCLASS("SuperclassesNode"),
    SUBCLASS("SubclassesNode"),
    CLASSTEMPLATE("ClassTemplatesNode"),
    TEMPLATENODE("TemplateNode");
	
    private final String value;

    Type(String v) {
        value = v;
    }

    public String value() {
        return value;
    }

    public static Type fromValue(String v) {
        for (Type c: Type.values()) {
            if (c.value.equals(v)) {
                return c;
            }
        }
        throw new IllegalArgumentException(v);
    }

}
