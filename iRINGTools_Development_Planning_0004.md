## Agenda ##

  * Review Scrumy Task Board
  * Discuss Part8 and Mapping Terminology
  * Review Mapping Editor for new Issues

## Attendees ##
  * Koos
  * Monika
  * Pavan
  * Jim
  * Mohamed
  * Hahn
  * Rob

## Part8 & Mapping ##

  * Simple statement
```
subject predicate object
```

  * Template statement
```
template
  role    object
  role    object
  role    object
```

  * Purchase example
```
Purchase
  seller  <#Steve>
  buyer   <#Rob>
  price   "499.00"^^xsd:float
  item    <#iPad>
  store   <#BethesdaAppleStore>
```

  * Line Tag Template Definition (not really how definitions are expressed)
```
<#LineTagTemplate>
  object             <rdl:Line>
  identifier         <xsd:String>
  identificationType <rdl:LineTag>
```

  * Line Tag Template Instance
```
<#T00001>
  type	             <tpl:LineTagTemplate>
  object             <#AB-0001>
  identifier         "12345-AB-0001"^^xsd:String
  identificationType <rdl:LineTag>
```
    * The result of the mapping is a Template Instance
    * **object** is the ClassRole because it points to the class the template is "on".
    * **identifier** is a PropertyRole because a property from the DataObject is mapped to it.
    * **identificationType** is a "Fixed" role.  In this case it is a ReferenceRole.

  * Another case would be in a specialized template where the Role has a ValueQualfication.
```
  cardinality        "1"^^xsd:Integer
```
    * **cardinality** is also a "Fixed" role, but it is a ValueRole.

## Issues ##

| **Issue #** | **Issue Description** | **Assigned** |
|:------------|:----------------------|:-------------|
| [Issue 35](https://code.google.com/p/iring-tools/issues/detail?id=35) | RefDataService: Search result show duplicate roles when adding new role to template | Mohd |
| [Issue 52](https://code.google.com/p/iring-tools/issues/detail?id=52) | In MappingEditor class roles are not identified if more that one level of superclassing is used | Pavan |
| [Issue 53](https://code.google.com/p/iring-tools/issues/detail?id=53) | In MappingEditor non-class roles should be editable depending on value(literal/reference)/range. | Pavan |
| [Issue 54](https://code.google.com/p/iring-tools/issues/detail?id=54) | Detail pane not get updated when re-select a node. | Hahn |
| [Issue 55](https://code.google.com/p/iring-tools/issues/detail?id=55) | RefDataEditor: TemplateDefinitions prevent further browsing. | Koos |
| [Issue 56](https://code.google.com/p/iring-tools/issues/detail?id=56) | When clicking on Mapping Labels are not resolved. | Pavan |
| [Issue 57](https://code.google.com/p/iring-tools/issues/detail?id=57) | Key Values are wrong in properties for Mapping. | Koos |
| [Issue 58](https://code.google.com/p/iring-tools/issues/detail?id=58) | When mapping ReferenceRoles, PropertyMap being created. | Koos |
| [Issue 59](https://code.google.com/p/iring-tools/issues/detail?id=59) | 	DTO Generation needs to be changed to handle Fixed RoleMaps. | Hahn |
| [Issue 60](https://code.google.com/p/iring-tools/issues/detail?id=60) | SPARQL generation does not handle mapping changes. | Monika |
| [Issue 61](https://code.google.com/p/iring-tools/issues/detail?id=61) | Mapping DataTypes | Hahn |

## Action Items ##
  * Rob to update IssuesList on GoogleCode