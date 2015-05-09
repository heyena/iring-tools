## Agenda ##
  * Status

## Attendees ##
  * Rob
  * Mohd
  * Hahn
  * Robertson
  * Monika
  * Gert
  * Koos

## Status ##
  * Monika
    * ApplicationEditor
      * Interface Changes
      * Silverlight and Provider
  * Gert
    * ApplicationEditor
      * Display of DBDictionary
      * Scopes
  * Koos
    * Prism is the problem
    * Commited Tab removal on MappingEditor
    * Need to test and verify
  * Robertson
    * Propose 2 layout options for TemplateEditor
      * Decided to go with Tabs
    * Had questions about RoleQualifications
      * Add Role Values
```
sparql += "_:role" + i + " rdf:type tpl:R67036823327 ; "
	  + " tpl:R56456315674 " + ID + " ; "
	  + " tpl:R89867215482 <" + role.qualifies + "> ; "
	  + " tpl:R29577887690 '" + role.value.text + "'^^" + role.value.As + " .";
```
  * Mohd
    * Still working on part 8
    * Having Issues with SVN
  * Hahn
    * DifferenceEngine
      * Need to validate Sending RDF contains enough stuff based on receiver manifest
      * Need to test and verify more complex RDF
> > > > (/branches/ports/java -> iring-tools-java)