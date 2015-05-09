# Now #

## NHibernateService ##
```
/NHibernateService/{scope}/{app}/dictionary             GET                     -> DatabaseDictionary
/NHibernateService/{scope}/{app}/dictionary             POST DatabaseDictionary -> Response 
/NHibernateService/{scope}/{app}/generate               GET                     -> Response
/NHibernateService/{scope}/{app}/schemaObjects          GET                     -> List<String>
/NHibernateService/{scope}/{app}/schemaObjects/{object} GET                     -> DataObject
/NHibernateService/{scope}/{app}/providers              GET                     -> List<String>
/NHibernateService/{scope}/{app}/relationships          GET                     -> List<String>
/NHibernateService/version                              GET                     -> Version
```

## AdapterService ##
```
/AdapterService/{scope}/{app}/{graph}?format=xml/dto/rdf      GET              -> XElement
/AdapterService/{scope}/{app}/{graph}?format=xml/dto/rdf      POST    XElement -> Response
/AdapterService/{scope}/{app}/{graph}/{id}?format=xml/dto/rdf GET              -> XElement
/AdapterService/{scope}/{app}/{graph}/delete                  GET              -> Reponse
/AdapterService/{scope}/{app}/{graph}/dti                     POST   dxRequest -> XElement
/AdapterService/{scope}/{app}/{graph}/dto                     POST   dxRequest -> XElement
/AdapterService/{scope}/{app}/{graph}/refresh                 GET              -> Reponse
/AdapterService/{scope}/{app}/{graph}/pull                    POST     Request -> Reponse
/AdapterService/{scope}/{app}/binding                         GET              -> XElement
/AdapterService/{scope}/{app}/binding                         POST    XElement -> Response
/AdapterService/{scope}/{app}/delete                          GET              -> Reponse
/AdapterService/{scope}/{app}/dictionary                      GET              -> DataDictionary
/AdapterService/{scope}/{app}/manifest                        GET              -> Manifest
/AdapterService/{scope}/{app}/mapping                         GET              -> Mapping (1.2.x)
/AdapterService/{scope}/{app}/mapping                         POST    XElement -> Response
/AdapterService/datalayers                                    GET              -> List<String>
/AdapterService/scopes                                        GET              -> List<Scope>
/AdapterService/scopes                                        POST List<Scope> -> Response
/AdapterService/version                                       GET              -> String
```

## RefDataService ##
```
/RefDataService/classes                              POST QMXF -> Response
/RefDataService/classes/{id}                         GET       -> QMXF
/RefDataService/classes/{id}/allsuperclasses         GET       -> List<Entity>
/RefDataService/classes/{id}/label                   GET       -> String
/RefDataService/classes/{id}/subclasses              GET       -> List<Entity>
/RefDataService/classes/{id}/superclasses            GET       -> List<Entity>
/RefDataService/classes/{id}/templates               GET       -> List<Entity>
/RefDataService/find/{query}                         GET       -> List<Entity>
/RefDataService/repositories                         GET       -> List<Repositories>
/RefDataService/search/{query}                       GET       -> List<Entity>
/RefDataService/search/{query}/{start}/{limit}       GET       -> List<Entity>
/RefDataService/search/{query}/{start}/{limit}/reset GET       -> List<Entity>
/RefDataService/search/{query}/reset                 GET       -> List<Entity>
/RefDataService/templates                            POST QMXF -> Response
/RefDataService/templates/{id}                       GET       -> QMXF
/RefDataService/version                              GET       -> Version
```


# 2.0.0 #

## NHibernateService ##
```
/NHibernateService/{scope}/{app}/dictionary       GET                     -> DatabaseDictionary
/NHibernateService/{scope}/{app}/dictionary       POST DatabaseDictionary -> Response 
/NHibernateService/{scope}/{app}/generate         GET                     -> Response
/NHibernateService/{scope}/{app}/objects          GET                     -> List<String>
/NHibernateService/{scope}/{app}/objects/{object} GET                     -> DataObject
/NHibernateService/{scope}/{app}/providers        GET                     -> List<String>
/NHibernateService/{scope}/{app}/relationships    GET                     -> List<String>
/NHibernateService/version                        GET                     -> Version
```

## AdapterService ##
```
/AdapterService/{scope}/{app}/{graph}?format=xml/dto/rdf      GET              -> XDocument
/AdapterService/{scope}/{app}/{graph}?format=xml/dto/rdf      POST   XDocument -> Response
/AdapterService/{scope}/{app}/{graph}/{id}?format=xml/dto/rdf GET              -> XDocument
/AdapterService/{scope}/{app}/{graph}/{id}                    DELETE           -> Response
/AdapterService/{scope}/{app}/{graph}/delete                  GET              -> Reponse
/AdapterService/{scope}/{app}/{graph}/refresh                 GET              -> Reponse
/AdapterService/{scope}/{app}/{graph}/pull                    POST     Request -> Reponse
/AdapterService/{scope}/{app}/binding                         GET              -> XDocument
/AdapterService/{scope}/{app}/binding                         POST   XDocument -> Response
/AdapterService/{scope}/{app}/delete                          GET              -> Reponse
/AdapterService/{scope}/{app}/dictionary                      GET              -> DataDictionary
/AdapterService/{scope}/{app}/mapping                         GET              -> Mapping (1.2.x)
/AdapterService/{scope}/{app}/mapping                         POST   XDocument -> Response
/AdapterService/datalayers                                    GET              -> DataLayers
/AdapterService/scopes                                        GET              -> Scopes
/AdapterService/scopes                                        POST      Scopes -> Response
/AdapterService/version                                       GET              -> Version
```

## DTO ##
```
/dto/{scope}/{app}/{graph}           GET                        -> DataTransferIndices
/dto/{scope}/{app}/{graph}/page      POST   DataTransferIndices -> DataTransferObjects
/dto/{scope}/{app}/{graph}           POST   DataTransferObjects -> Response
/dto/{scope}/{app}/{graph}/{id}      GET                        -> DataTransferObjects
/dto/{scope}/{app}/{graph}/{id}      DELETE                     -> Response
/dto/{scope}/{app}/{graph}/xfr       POST              Manifest -> DataTransferIndices
/dto/{scope}/{app}/{graph}/xfr/page  POST        DtoPageRequest -> DataTransferObjects
/dto/{scope}/{app}/manifest          GET                        -> Manifest
/dto/version                         GET                        -> Version
```

DtoPageRequest= Manifest + DataTransferIndices

## InterfaceService ##
```
/InterfaceService/query                         GET          -> HTML (SPARQL Query Form)
/InterfaceService/query?query={query}           GET          -> SPARQL Results
/InterfaceService/update                        GET          -> HTML (SPARQL Upadte Form)
/InterfaceService/update?update={query}         GET          -> SPARQL Results
/InterfaceService/version                       GET          -> Version
```

## RefDataService ##
```
/RefDataService/classes                              POST QMXF -> Response
/RefDataService/classes/{id}                         GET       -> QMXF
/RefDataService/classes/{id}/allsuperclasses         GET       -> List<Entity>
/RefDataService/classes/{id}/label                   GET       -> String
/RefDataService/classes/{id}/subclasses              GET       -> List<Entity>
/RefDataService/classes/{id}/superclasses            GET       -> List<Entity>
/RefDataService/classes/{id}/members                 GET       -> List<Entity>
/RefDataService/classes/{id}/templates               GET       -> List<Entity>
/RefDataService/find/{query}                         GET       -> List<Entity>
/RefDataService/repositories                         GET       -> List<Repositories>
/RefDataService/search/{query}                       GET       -> List<Entity>
/RefDataService/search/{query}/{start}/{limit}       GET       -> List<Entity>
/RefDataService/search/{query}/{start}/{limit}/reset GET       -> List<Entity>
/RefDataService/search/{query}/reset                 GET       -> List<Entity>
/RefDataService/templates                            POST QMXF -> Response
/RefDataService/templates/{id}                       GET       -> QMXF
/RefDataService/version                              GET       -> Version
```

## SandboxService ##
```
/SandboxService/query                 GET -> HTML (SPARQL Query Form)
/SandboxService/query?query={query}   GET -> SPARQL Results
/SandboxService/update                GET -> HTML (SPARQL Upadte Form)
/SandboxService/update?update={query} GET -> SPARQL Results
/SandboxService/version               GET -> Version
```

# Future #
## Hibernate ##
```
/hibernate/{scope}/{app}/dictionary       GET                     -> DatabaseDictionary
/hibernate/{scope}/{app}/dictionary       POST DatabaseDictionary -> Response 
/hibernate/{scope}/{app}/generate         GET                     -> Response
/hibernate/{scope}/{app}/objects          GET                     -> DataObjectList
/hibernate/{scope}/{app}/objects/{object} GET                     -> DataObject
/hibernate/{scope}/{app}/providers        GET                     -> Providers
/hibernate/{scope}/{app}/relationships    GET                     -> Relationships
/hibernate/version                        GET                     -> Version
```

## Adapter ##
```
/adapter/{scope}/{app}/binding    GET            -> XDocument
/adapter/{scope}/{app}/binding    POST XDocument -> Response
/adapter/{scope}/{app}/dictionary GET            -> DataDictionary
/adapter/{scope}/{app}/mapping    GET            -> Mapping (2.1.x)
/adapter/{scope}/{app}/mapping    POST XDocument -> Response
/adapter/datalayers               GET            -> DataLayers
/adapter/scopes                   GET            -> Scopes
/adapter/scopes                   POST    Scopes -> Response
/adapter/version                  GET            -> Version
```

## Data ##
```
/data/{scope}/{app}/{graph}?format=xml/dto/rdf      GET              -> XDocument
/data/{scope}/{app}/{graph}?format=xml/dto/rdf      POST   XDocument -> Response
/data/{scope}/{app}/{graph}/{id}?format=xml/dto/rdf GET              -> XDocument
/data/{scope}/{app}/{graph}/{id}                    DELETE           -> Response
/data/version                                       GET              -> Version
```

## DTO ##
```
/dto/{scope}/{app}/{graph}           GET                        -> DataTransferIndices
/dto/{scope}/{app}/{graph}/page      POST   DataTransferIndices -> DataTransferObjects
/dto/{scope}/{app}/{graph}           POST   DataTransferObjects -> Response
/dto/{scope}/{app}/{graph}/{id}      GET                        -> DataTransferObjects
/dto/{scope}/{app}/{graph}/{id}      DELETE                     -> Response
/dto/{scope}/{app}/{graph}/xfr       POST              Manifest -> DataTransferIndices
/dto/{scope}/{app}/{graph}/xfr/page  POST        DtoPageRequest -> DataTransferObjects
/dto/{scope}/{app}/manifest          GET                        -> Manifest
/dto/version                         GET                        -> Version
```

DtoPageRequest = Manifest + DataTransferIndices

## Facade ##
```
/facade/{scope}/{app}/{graph}/delete  GET          -> Reponse
/facade/{scope}/{app}/{graph}/refresh GET          -> Reponse
/facade/{scope}/{app}/{graph}/pull    POST Request -> Reponse
/facade/{scope}/{app}/delete          GET          -> Reponse
/facade/query                         GET          -> HTML (SPARQL Query Form)
/facade/query?query={query}           GET          -> SPARQL Results
/facade/update                        GET          -> HTML (SPARQL Upadte Form)
/facade/update?update={query}         GET          -> SPARQL Results
/facade/version                       GET          -> Version
```

## RefData ##
```
/refdata/classes                              POST QMXF -> Response
/refdata/classes/{id}	                      GET       -> QMXF
/refdata/classes/{id}/allsuperclasses         GET       -> Entities
/refdata/classes/{id}/label                   GET       -> Label
/refdata/classes/{id}/subclasses              GET       -> Entities
/refdata/classes/{id}/superclasses            GET       -> Entities
/refdata/classes/{id}/members                 GET       -> Entities
/refdata/classes/{id}/templates	              GET       -> Entities
/refdata/repositories	                      GET       -> List<Repositories>
/refdata/search/{query}	                      GET       -> Entities
/refdata/search/{query}/{start}/{limit}	      GET       -> Entities
/refdata/search/{query}/{start}/{limit}/reset GET       -> Entities
/refdata/search/{query}/reset	              GET       -> Entities
/refdata/templates                            POST QMXF -> Response
/refdata/templates/{id}	                      GET       -> QMXF
/refdata/version                              GET       -> Version
```

## Sandbox ##
```
/sandbox/query                 GET -> HTML (SPARQL Query Form)
/sandbox/query?query={query}   GET -> SPARQL Results
/sandbox/update                GET -> HTML (SPARQL Upadte Form)
/sandbox/update?update={query} GET -> SPARQL Results
/sandbox/version               GET -> Version
```

## ESB ##
```
/esb/{scope}/{app}/{graph}                              GET                        -> DataTransferIndices
/esb/{scope}/{app}/{graph}                              POST   DataTransferIndices -> DataTransferObjects
/esb/{scope}/{app}/{graph}                              POST   DataTransferObjects -> Response
/esb/{scope}/{app}/{graph}/{id}                         GET                        -> DataTransferObjects
/esb/{scope}/{app}/{graph}/{id}                         DELETE                     -> Response
/esb/{scope}/exchanges/{id}                             GET                        -> DataTransferIndices
/esb/{scope}/exchanges/{id}                             POST   DataTransferIndices -> DataTransferObjects
/esb/data/{scope}/{app}/{graph}?format=xml/dto/rdf      GET                        -> XDocument
/esb/data/{scope}/{app}/{graph}?format=xml/dto/rdf      POST             XDocument -> Response
/esb/data/{scope}/{app}/{graph}/{id}?format=xml/dto/rdf GET                        -> XDocument
/esb/data/{scope}/{app}/{graph}/{id}                    DELETE                     -> Response
/esb/directory                                          GET                        -> Directory
/esb/version                                            GET                        -> Version
```