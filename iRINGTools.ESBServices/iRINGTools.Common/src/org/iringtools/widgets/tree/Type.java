package org.iringtools.widgets.tree;

public enum Type {

	SEARCH("SearchNode"),
    CLASS("ClassNode"),
    CLASSIFICATION("ClassificationsNode"),
    MEMBERS("MembersNode"),
    SUPERCLASS("SuperclassesNode"),
    SUBCLASS("SubclassesNode"),
    CLASSTEMPLATE("ClassTemplatesNode"),
    TEMPLATENODE("TemplateNode"),
	ROLENODE("RoleNode");
	
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
