using org.iringtools.adapter;
using org.iringtools.refdata;
using org.iringtools.library;
using org.iringtools.utility;
using org.ids_adi.qmxf;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System;
using NUnit.Framework;
using org.iringtools.refdata.federation;
using StaticDust.Configuration;

namespace NUnit.Tests
{
  [TestFixture]
  public class ReferenceDataTests
  {
    private ReferenceDataProvider _refdataProvider = null;
    private ReferenceDataSettings _settings = null;
    private string _baseDirectory = string.Empty;

    public ReferenceDataTests()
    {
      _baseDirectory = Directory.GetCurrentDirectory();
      _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\\Bin"));      
      Directory.SetCurrentDirectory(_baseDirectory);
      AdapterSettings adapterSettings = new AdapterSettings();
      adapterSettings.AppendSettings(new AppSettingsReader("App.config"));
      _settings = new ReferenceDataSettings();
      _settings.AppendSettings((ServiceSettings)adapterSettings);
      _settings["BaseDirectoryPath"] = _baseDirectory;      
      _refdataProvider = new ReferenceDataProvider(_settings);
    }   

    //[Test]
    public void GetFederation()
        {
          Federation fed = new Federation();

          Namespace dm = new Namespace
          {
            Id = 1,
            Prefix = "dm",
            Uri = "http://dm.rdlfacade.org/data#",
            Description = "Data Model",
            IsWriteable = false
          };

          Namespace rdl = new Namespace
          {
            Id = 2,
            Prefix = "rdl",
            Uri = "http://rdl.rdlfacade.org/data#",
            Description = "RDL Classes",
            IsWriteable = true,
            IdGenerator = 1
          };

          Namespace tpl = new Namespace
          {
            Id = 3,
            Prefix = "tpl",
            Uri = "http://tpl.rdlfacade.org/data#",
            Description = "RDL Templates",
            IsWriteable = false
          };
          Namespace owl = new Namespace
          {
            Id = 4,
            Prefix = "owl",
            Uri = "http://www.w3.org/2002/07/owl#",
            Description = "w3 owl",
            IsWriteable = false
          };

          Namespace owl2xml = new Namespace
          {
            Id = 5,
            Prefix = "owl2xml",
            Uri = "http://www.w3.org/2006/12/owl2-xml#",
            Description = "w3 owl2 xml",
            IsWriteable = false
          };

          Namespace p8 = new Namespace
          {
            Id = 6,
            Prefix = "p8",
            Uri = "http://standards.tc184-sc4.org/iso/15926/-8/template-model#",
            Description = "ISO15926 Part 8 Classes",
            IsWriteable = false
          };

          Namespace p8dm = new Namespace
           {
             Id = 7,
             Prefix = "p8dm",
             Uri = "http://standards.tc184-sc4.org/iso/15926/-8/data-model#",
             Description = "ISO15926 Part 8 Data Model",
             IsWriteable = false
           };

          Namespace templates = new Namespace
          {
            Id = 8,
            Prefix = "templates",
            Uri = "http://standards.tc184-sc4.org/iso/15926/-8/templates#",
            Description = "ISO15926 Part 8 Templates",
            IsWriteable = false
          };


          Namespace jorddm = new Namespace
          {
            Id = 9,
            Prefix = "jorddm",
            Uri = "http://rds.posccaesar.org/2008/02/OWL/ISO-15926-2_2003#",
            Description = "JORD Data Model",
            IsWriteable = false,
            IdGenerator = 1
          };

          Namespace jordrdl = new Namespace
          {
            Id = 10,
            Prefix = "jordrdl",
            Uri = "http://posccaesar.org/rdl/",
            Description = "JORD RDL CLasses",
            IsWriteable = false
          };

          Namespace jordtpl = new Namespace
          {
            Id = 11,
            Prefix = "jordtpl",
            Uri = "http://posccaesar.org/tpl/",
            Description = "JORD RDL Templates",
            IsWriteable = false
          };

          Repository repo1 = new Repository { 
            Id = 1, 
            Name = "My Private Sandbox", 
            RepositoryType = RepositoryType.Part8, 
            Uri = "http://localhost:54321/sandbox/query", 
            UpdateUri = "http://localhost:54321/sandbox/update", 
            IsReadOnly = false,
            Description = "Development Sandbox"
          };

           

          IDGenerator idg1 = new IDGenerator
          {
            Id = 1,
            Name = "ids-adi.org",
            Description = "IDS-ADI ID Generator",
            Uri = "https://secure.ids-adi.org/registry"
          };
          
          Repository repo2 = new Repository
          {
            Id = 2,
            Name = "iRING Sandbox",
            RepositoryType = RepositoryType.Camelot,
            Uri = "http://www.iringsandbox.org/repositories/sandbox/query",
            IsReadOnly = true,
            Description = "iRING Sandbox" ,
          };
          
          Repository repo3 = new Repository
          {
            Id = 3,
            Name = "ReferenceData",
            RepositoryType = RepositoryType.RDSWIP,
            Uri = "http://rdl.rdlfacade.org/data",
            IsReadOnly = true,
            Description = "RDS/WIP Part 4 Reference Data Library"
          };

          Repository repo4 = new Repository
          {
            Id = 4,
            Name = "Proto and Initial",
            RepositoryType = RepositoryType.Camelot,
            Uri = "http://www.iringsandbox.org/repositories/tempInitSet/query",
            IsReadOnly = true,
            Description = "RDS/Proto and P7 Initial Set Templates"
          };

          Repository repo5 = new Repository
          {
            Id = 5,
            Name = "JORD",
            RepositoryType = RepositoryType.JORD,
            Uri = "http://posccaesar.org/endpoint/sparql",
            IsReadOnly = true,
            Description = "JORD Endpoint"
          };


          fed.IDGenerators.Add(idg1);
          fed.Namespaces.Add(owl);
          fed.Namespaces.Add(owl2xml);
          fed.Namespaces.Add(p8dm);
          fed.Namespaces.Add(p8);
          fed.Namespaces.Add(templates);
          fed.Namespaces.Add(rdl);
          fed.Namespaces.Add(tpl);
          fed.Namespaces.Add(dm);
          fed.Namespaces.Add(jorddm);
          fed.Namespaces.Add(jordrdl);
          fed.Namespaces.Add(jordtpl);

          fed.Repositories.Add(repo1);
          fed.Repositories.Add(repo2);
          fed.Repositories.Add(repo3);
          fed.Repositories.Add(repo4);
          fed.Repositories.Add(repo5);

          Utility.Write<Federation>(fed, "E:\\iring-tools\\branches\\2.4.x\\Tests\\NUnit.Tests\\App_Data\\Federation_Default.xml", true);
        }

    [Test]
    public void Search()
    {
      RefDataEntities _entities = _refdataProvider.Search("possible individual");
      Assert.AreNotEqual(0, _entities.Total);

    }

    [Test]
    public void GetClassLabel()
    {
      Entity entity = _refdataProvider.GetClassLabel("R99781532089");
      StringAssert.IsMatch("ISO 15926-4 POSSIBLE INDIVIDUAL", entity.Label);
    }

    [Test]
    public void GetRepositories()
    {
      List<Repository> repositories = _refdataProvider.GetFederation().Repositories;
      Assert.Greater(repositories.Count, 0);
    }

    [Test]
    public void GetClass()
    {
      QMXF qmxf = _refdataProvider.GetClass("R99781532089");
      Assert.IsNotNull(qmxf);
    }

    //[Test]
    //public void PostClass()
    //{
    //    ClassDefinition _classdef = null;
    //    Classification _classif = null;
    //    Specialization _spec = null;

    //    QMXF qmxf = new QMXF();
    //    foreach (Repository rep in _refdataProvider.GetRepositories())
    //    {
    //        if (rep.isReadOnly == false)
    //        {
    //            qmxf.targetRepository = rep.name;
    //            break;
    //        }
    //    }
    //    _classdef = new ClassDefinition();
    //    _classif = new Classification();
    //    _spec = new Specialization();

    //    _classif.label = "ISO 15926-4 INDIVIDUAL CLASS";
    //    _classif.reference = "http://rdl.rdlfacade.org/data#R82407290725";
    //    Description descr = new Description();
    //    descr.value = "A 15926-4 possible individual is a 15926-4 thing that exists in space and time. This includes: - things where any of the space time dimensions are vanishingly small, - those that are either all space for any time, or all time and any space, - the entirety of all space time - things that actually exist, or have existed, - things that are fictional or conjectured and possibly exist in the past, present or future, - temporal parts (states) of other individuals, - things that have a specific position, but zero extent in one or more dimensions, such as points, lines, and surfaces. In this context existence is based upon being imaginable within some consistent logic, including actual, hypothetical, planned, expected, or required individuals.";
    //    EntityType et = new EntityType();
    //    et.reference = "http://dm.rdlfacade.org/data#ClassOfIndividual";
    //    _classdef.classification.Add(_classif);

    //    _spec.label = "ISO 15926-4 THING";
    //    _spec.reference = "http://rdl.rdlfacade.org/data#R95758656562";
    //    _classdef.specialization.Add(_spec);

    //    _classdef.description.Add(descr);
    //    _classdef.entityType = et;
    //    _classdef.identifier = "http://rdl.rdlfacade.org/data#R99781532089";


    //    qmxf.classDefinitions.Add(_classdef);

    //    Response response = _refdataProvider.PostClass(qmxf);
    //    Assert.AreEqual(StatusLevel.Success, response.Level);
    //    Assert.AreNotEqual(0, response.StatusList.Count);
    //}

    [Test]
    public void GetTemplate()
    {
      QMXF qmxf = _refdataProvider.GetTemplate("R93761651329");
        Assert.IsNotNull(qmxf);
    }
  }
}
