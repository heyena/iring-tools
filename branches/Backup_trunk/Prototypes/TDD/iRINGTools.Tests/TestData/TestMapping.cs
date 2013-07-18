using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using iRINGTools.Data;

namespace iRINGTools.Tests
{
  public class TestMapping: Mapping
  {
    public TestMapping()
    {
      GraphMaps = new LazyList<GraphMap> {
        new GraphMap {
          Name = "Lines",
          DictionaryObjectName = "LINES",
          ClassTemplateMaps = new LazyList<ClassTemplateMap> {
            #region ClassTemplateMaps
            new ClassTemplateMap { 
              ClassMap = new ClassMap {
                Id = "rdl:R19192462550",
                Name = "PIPING NETWORK SYSTEM",
                Identifiers = new LazyList<String> { "LINES.TAG" }
              },
              TemplateMaps = new LazyList<TemplateMap> {
                #region TemplateMaps
                new TemplateMap { 
                  Id = "tpl:R66921101783",
                  Name = "IdentificationByTag",
                  RoleMaps = new LazyList<RoleMap>() {
                    #region RoleMaps
                    new RoleMap {
                      Type = RoleType.Possessor,
                      Id = "tpl:R44537504070",
                      Name = "hasObject"
                    },
                    new RoleMap {
                      Type = RoleType.Reference,
                      Id = "tpl:R30790108016",
                      Name = "hasIdentificationType",
                      Value = "rdl:R40471041754"
                    },
                    new RoleMap {
                      Id = "tpl:R22674749688",
                      Name = "valIdentifier",
                      DataType = "string",
                      PropertyName = "LINES.TAG"
                    }
                    #endregion
                  } 
                }, 
                new TemplateMap { 
                  Id = "tpl:R41248767846",
                  Name = "PlantAreaHasPlantObject",
                  RoleMaps = new LazyList<RoleMap> {
                    #region RoleMaps
                    new RoleMap { 
                      Type = RoleType.Possessor,
                      Id = "tpl:R77585325576",
                      Name = "part"
                    },  
                    new RoleMap {
                      Type = RoleType.Reference,
                      Id = "tpl:R62157498012",
                      Name = "assemblyType",
                      Value = "rdl:R30942378492"
                    }, 
                    new RoleMap { 
                      Type = RoleType.Reference,
                      Id = "tpl:R62348020201",
                      Name = "whole",
                      Value = "rdl:R49658319833",
                      ClassMap = new ClassMap { 
                        Id = "rdl:R49658319833",
                        Name = "PLANT AREA",
                        Identifiers = new LazyList<string> { "LINES.AREA" }
                      }
                    }
                    #endregion
                  }
                },
                new TemplateMap { 
                  Id = "tpl:R11327281485",
                  Name = "PAndIDRepresentation",
                  RoleMaps = new LazyList<RoleMap>() {
                    #region RoleMaps
                    new RoleMap {
                      Type = RoleType.Possessor,
                      Id = "tpl:R22715013877",
                      Name = "object"
                    },
                    new RoleMap {
                      Type = RoleType.Reference,
                      Id = "tpl:R35290929175",
                      Name = "representationType",
                      Value = "rdl:R99486931975"
                    }, 
                    new RoleMap {
                      Type = RoleType.Reference,
                      Id = "tpl:R74328815710",
                      Name = "representation",
                      Value = "rdl:R99486931975",
                      ClassMap = new ClassMap {
                        Id = "rdl:R99486931975",
                        Name = "P AND I DIAGRAM",
                        Identifiers = new LazyList<string> { "LINES.PIDNUMBER" }
                      }
                    } 
                    #endregion
                  } 
                },
                new TemplateMap {
                  Id = "tpl:R77443358818",
                  Name = "NominalDiameter",
                  RoleMaps = new LazyList<RoleMap> { 
                    #region RoleMaps
                    new RoleMap { 
                      Type = RoleType.Possessor,
                      Id = "tpl:R16314127825",
                      Name = "hasPossessor"
                    },
                    new RoleMap { 
                      Id = "tpl:R93884193692",
                      Name = "hasScale",
                      DataType = "String",
                      PropertyName = "LINES.UOM_NOMDIAMETER"
                    },
                    new RoleMap { 
                      Type = RoleType.Reference,
                      Id = "tpl:R59896853239",
                      Name = "hasType", 
                      Value = "rdl:R17622148043"
                    },
                    new RoleMap { 
                      Id = "tpl:R90688063648",
                      Name = "valValue",
                      DataType = "double",
                      PropertyName = "LINES.NOMDIAMETER"
                    }
                    #endregion
                  }
                }
                #endregion
              },
            },
            new ClassTemplateMap {
              ClassMap = new ClassMap { 
                Id = "rdl:R49658319833",
                Name = "PLANT AREA",
                Identifiers = new LazyList<string> { "LINES.AREA" }
              },
              TemplateMaps = new LazyList<TemplateMap> {
                new TemplateMap { 
                  Id = "tpl:R66921101783",
                  Name = "IdentificationByTag",
                  RoleMaps = new LazyList<RoleMap> {
                    #region RoleMap
                    new RoleMap { 
                      Type = RoleType.Possessor,
                      Id = "tpl:R44537504070",
                      Name = "hasObject"
                    },
                    new RoleMap { 
                      Type = RoleType.Reference,
                      Id = "tpl:R30790108016",
                      Name = "hasIdentificationType",
                      Value = "rdl:R40471041754"
                    },
                    new RoleMap { 
                      Id = "tpl:R22674749688",
                      Name = "valIdentifier",
                      DataType = "string",
                      PropertyName = "LINES.AREA"
                    }
                    #endregion
                  }
                }
              }
            },
            new ClassTemplateMap {
              ClassMap = new ClassMap { 
                Id = "rdl:R99486931975",
                Name = "P AND I DIAGRAM",
                Identifiers = new LazyList<string> { "LINES.PIDNUMBER" }
              },
              TemplateMaps = new LazyList<TemplateMap> {
                new TemplateMap { 
                  Id = "tpl:R30193386273",
                  Name = "ClassifiedIdentification1",
                  RoleMaps = new LazyList<RoleMap> {
                    #region RoleMaps
                    new RoleMap { 
                      Type = RoleType.Possessor,
                      Id = "tpl:R44537504070",
                      Name = "hasObject"
                    },
                    new RoleMap { 
                      Type = RoleType.Reference,
                      Id = "tpl:R30790108016",
                      Name = "hasIdentificationType",
                      Value = "rdl:R16893283050"
                    },
                    new RoleMap { 
                      Id = "tpl:R22674749688",
                      Name = "valIdentifier",
                      DataType = "string",
                      PropertyName = "LINES.PIDNUMBER"
                    }
                    #endregion
                  } 
                }
              } 
            }
            #endregion
          }
        }
      };
    }
  }
}
