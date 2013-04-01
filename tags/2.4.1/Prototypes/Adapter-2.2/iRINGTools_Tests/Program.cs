using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.mapping;
using org.iringtools.utility;

namespace iRINGTools_Tests
{
  class Program
  {
    static void Main(string[] args)
    {
      testMapping();

      Console.WriteLine("\nPress any key to continue ...");
      Console.ReadKey();
    }

    static void testMapping()
    {
      ClassMap c1 = new ClassMap()
      {
        id = "c1",
      };

      ClassMap c2 = new ClassMap()
      {
        id = "c2",
      };

      ClassMap c3 = new ClassMap()
      {
        id = "c3",
      };

      ClassMap c4 = new ClassMap()
      {
        id = "c4",
      };

      ClassMap c5 = new ClassMap()
      {
        id = "c5",
      };

      Mapping mapping = new Mapping()
      {
        graphMaps = new GraphMaps() {
          new GraphMap() {
            name = "g1",
            group = "A",
            classTemplateMaps = new ClassTemplateMaps() {
              new ClassTemplateMap() {
                classMap = c1,
                templateMaps = new TemplateMaps() {
                  new TemplateMap() {
                    id = "t1",
                    roleMaps = new RoleMaps() {
                      new RoleMap() {
                        id = "r1",
                        type = RoleType.Reference,
                        classMap = c2
                      }
                    }
                  }
                }
              },
              new ClassTemplateMap() {
                classMap = c4,
                templateMaps = new TemplateMaps() {
                  new TemplateMap() {
                    id = "t4",
                    roleMaps = new RoleMaps() {
                      new RoleMap() {
                        id = "r4",
                        type = RoleType.Reference,
                        classMap = c5
                      }
                    }
                  }
                }
              }
            }
          },
          new GraphMap() {
            name = "g2",
            group = "A",
            classTemplateMaps = new ClassTemplateMaps() {
              new ClassTemplateMap() {
                classMap = c1,
                templateMaps = new TemplateMaps() {
                  new TemplateMap() {
                    id = "t1",
                    roleMaps = new RoleMaps() {
                      new RoleMap() {
                        id = "r1",
                        type = RoleType.Reference,
                        classMap = c4
                      }
                    }
                  }
                }
              },
              new ClassTemplateMap() {
                classMap = c2,
                templateMaps = new TemplateMaps() {
                  new TemplateMap() {
                    id = "t1",
                    roleMaps = new RoleMaps() {
                      new RoleMap() {
                        id = "r1",
                        type = RoleType.Reference,
                        classMap = c3
                      }
                    }
                  }
                }
              },
              new ClassTemplateMap() {
                classMap = c3,
                templateMaps = new TemplateMaps() {
                  new TemplateMap() {
                    id = "t3",
                    roleMaps = new RoleMaps() {
                      new RoleMap() {
                        id = "r3",
                        type = RoleType.Reference,
                        classMap = c1
                      }
                    }
                  }
                }
              },
              new ClassTemplateMap() {
                classMap = c5,
                templateMaps = new TemplateMaps() {
                  new TemplateMap() {
                    id = "t5",
                    roleMaps = new RoleMaps() {
                      new RoleMap() {
                        id = "r4",
                        type = RoleType.Property,
                        propertyName = "p1"
                      }
                    }
                  }
                }
              }
            }
          },
          new GraphMap() {
            name = "g1",
            group = "B",
            classTemplateMaps = new ClassTemplateMaps() {
              new ClassTemplateMap() {
                classMap = c1,
                templateMaps = new TemplateMaps() {
                  new TemplateMap() {
                    id = "t1",
                    roleMaps = new RoleMaps() {
                      new RoleMap() {
                        id = "r1",
                        type = RoleType.Reference,
                        classMap = c2
                      }
                    }
                  }
                }
              }
            }
          }
        }
      };

      Console.WriteLine("Case 1");
      bool case1 = mapping.ContainsCircularReference("A");
      Console.WriteLine("Result: " + (case1 ? "Circular reference." : "No circular reference."));

      Console.WriteLine("\nCase 2");
      bool case2 = mapping.ContainsCircularReference("A", "c1");
      Console.WriteLine("Result: " + (case2 ? "Circular reference." : "No circular reference."));

      Console.WriteLine("\nCase 3");
      bool case3 = mapping.ContainsCircularReference("A", "g1", "c1");
      Console.WriteLine("Result: " + (case3 ? "Circular reference." : "No circular reference."));

      Console.WriteLine("\nCase 4");      
      bool case4 = mapping.ContainsCircularReference("A", "g2", "c1");
      Console.WriteLine("Result: " + (case4 ? "Circular reference." : "No circular reference."));
    }
  }
}
