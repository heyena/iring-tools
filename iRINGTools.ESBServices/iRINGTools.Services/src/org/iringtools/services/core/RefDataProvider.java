package org.iringtools.services.core;

import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.UnsupportedEncodingException;
import java.net.URI;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Hashtable;
import java.util.List;
import java.util.Map;
import java.util.TreeMap;

import javax.xml.bind.JAXBException;

import org.apache.log4j.Logger;
import org.ids_adi.ns.qxf.model.ClassDefinition;
import org.ids_adi.ns.qxf.model.Classification;
import org.ids_adi.ns.qxf.model.Description;
import org.ids_adi.ns.qxf.model.EntityType;
import org.ids_adi.ns.qxf.model.Name;
import org.ids_adi.ns.qxf.model.Qmxf;
import org.ids_adi.ns.qxf.model.RoleDefinition;
import org.ids_adi.ns.qxf.model.RoleQualification;
import org.ids_adi.ns.qxf.model.Specialization;
import org.ids_adi.ns.qxf.model.TemplateDefinition;
import org.ids_adi.ns.qxf.model.TemplateQualification;
import org.iringtools.common.Version;
import org.iringtools.common.response.Level;
import org.iringtools.common.response.Messages;
import org.iringtools.common.response.Response;
import org.iringtools.common.response.Status;
import org.iringtools.common.response.StatusList;
import org.iringtools.mapping.TemplateType;
import org.iringtools.refdata.federation.Federation;
import org.iringtools.refdata.federation.IDGenerator;
import org.iringtools.refdata.federation.Namespace;
import org.iringtools.refdata.federation.Repository;
import org.iringtools.refdata.federation.RepositoryType;
import org.iringtools.refdata.queries.Queries;
import org.iringtools.refdata.queries.Query;
import org.iringtools.refdata.queries.QueryBinding;
import org.iringtools.refdata.queries.QueryBindings;
import org.iringtools.refdata.queries.QueryItem;
import org.iringtools.refdata.queries.SPARQLBindingType;
import org.iringtools.refdata.response.Entities;
import org.iringtools.refdata.response.Entity;
import org.iringtools.utility.EntityComparator;
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpClientException;
import org.iringtools.utility.IOUtils;
import org.iringtools.utility.JaxbUtils;
import org.iringtools.utility.NamespaceMapper;
import org.iringtools.utility.NetworkCredentials;
import org.iringtools.utility.ReferenceObject;
import org.w3._2005.sparql.results.Binding;
import org.w3._2005.sparql.results.Result;
import org.w3._2005.sparql.results.Results;
import org.w3._2005.sparql.results.Sparql;

public class RefDataProvider
{
  private Hashtable<String, String> _settings;
  private List<Repository> _repositories = null;
  private Queries _queries = null;
  private String defaultLanguage = "en";
  private NamespaceMapper _nsmap = null;
  private Map<String, Entity> _searchHistory = new TreeMap<String, Entity>();
  private boolean _useExampleRegistryBase = false;
  private final String insertData = "INSERT DATA {";
  private final String deleteData = "DELETE DATA {";
  private final String deleteWhere = "DELETE WHERE {";
  private StringBuilder prefix = new StringBuilder();
  private StringBuilder sparqlBuilder = new StringBuilder();
  private static final Logger logger = Logger.getLogger(RefDataProvider.class);

  public RefDataProvider(Hashtable<String, String> settings)
  {
    try
    {
      _settings = settings;
      _repositories = getRepositories();
      _queries = getQueries();
      _nsmap = new NamespaceMapper();
    }
    catch (Exception e)
    {
      // TODO Auto-generated catch block
      e.printStackTrace();
    }
  }

  public Queries getQueries() throws JAXBException, IOException, FileNotFoundException
  {
    String path = _settings.get("baseDirectory") + "/WEB-INF/data/Queries.xml";
    return JaxbUtils.read(Queries.class, path);
  }

  public Federation getFederation() throws JAXBException, IOException
  {
    String path = _settings.get("baseDirectory") + "/WEB-INF/data/federation.xml";
    return JaxbUtils.read(Federation.class, path);
  }

  public Response saveFederation(Federation federation) throws Exception
  {
    Response response = new Response();
    try
    {
      String path = _settings.get("baseDirectory") + "/WEB-INF/data/federation.xml";
      JaxbUtils.write(federation, path, true);
      response.setLevel(Level.SUCCESS);
    }
    catch (Exception ex)
    {
      response.setLevel(Level.ERROR);
      throw ex;
    }
    return response;
  }

  public Response saveNamespace(Namespace namespace, boolean deleteFlag) throws Exception
  {
    Response response = new Response();
    StatusList sl = new StatusList();
    List<Status> statuses = new ArrayList<Status>();
    Status status = new Status();
    Messages messages = new Messages();
    List<String> msgs = new ArrayList<String>();
    boolean namespaceExist = false;
    int index = 0;
    try
    {
      Federation federation = getFederation();
      for (Namespace ns : federation.getNamespaces().getItems())
      {
        if (ns.getId().equalsIgnoreCase(namespace.getId()))
        {
          index = federation.getNamespaces().getItems().indexOf(ns);
          namespaceExist = true;
          break;
        }
      }
      if (namespaceExist)
      {
        if (deleteFlag)
        {
          // find out the repositories that use this namespace and
          // remove the namespace
          Integer nsID = Integer.parseInt(namespace.getId());
          for (Repository repo : federation.getRepositories().getItems())
          {
            if (repo.getNamespaces() != null && repo.getNamespaces().getItems().contains(nsID))
              repo.getNamespaces().getItems().remove(nsID);
          }
        }

        // now remove the namespace
        federation.getNamespaces().getItems().remove(index);
      }
      else
      {
        int sequenceId = federation.getNamespaces().getSequenceId();
        namespace.setId(Integer.toString(++sequenceId));
        federation.getNamespaces().setSequenceId(sequenceId);
      }
      if (!deleteFlag)
      {
        federation.getNamespaces().getItems().add(namespace);
      }

      String path = _settings.get("baseDirectory") + "/WEB-INF/data/federation.xml";
      JaxbUtils.write(federation, path, true);

      msgs.add("Namespace saved.");
      response.setLevel(Level.SUCCESS);
    }
    catch (Exception ex)
    {
      msgs.add("Error while saving namespace.");
      response.setLevel(Level.ERROR);
      logger.error(ex);
    }

    messages.setItems(msgs);
    status.setMessages(messages);
    statuses.add(status);
    sl.setItems(statuses);
    response.setStatusList(sl);
    return response;
  }

  public Response saveIdGenerator(IDGenerator idgenerator, boolean deleteFlag) throws Exception
  {
    Response response = new Response();
    StatusList sl = new StatusList();
    List<Status> statuses = new ArrayList<Status>();
    Status status = new Status();
    Messages messages = new Messages();
    List<String> msgs = new ArrayList<String>();
    boolean idgenExist = false;
    int index = 0;
    try
    {
      Federation federation = getFederation();
      for (IDGenerator idg : federation.getIdGenerators().getItems())
      {
        if (idg.getId().equalsIgnoreCase(idgenerator.getId()))
        {
          index = federation.getIdGenerators().getItems().indexOf(idg);
          idgenExist = true;
          break;
        }
      }
      if (idgenExist)
      {
        if (deleteFlag)
        {
          // find out the namespaces that use this idGenerator and
          // remove the idGenerator
          String nsID = idgenerator.getId();
          for (Namespace ns : federation.getNamespaces().getItems())
          {
            if (ns.getIdGenerator().equalsIgnoreCase(nsID))
            {
              ns.setIdGenerator("0");
            }
          }
        }

        // now remove the namespace
        federation.getIdGenerators().getItems().remove(index);
      }
      else
      {
        int sequenceId = federation.getIdGenerators().getSequenceId();
        idgenerator.setId(Integer.toString(++sequenceId));
        federation.getIdGenerators().setSequenceId(sequenceId);
      }
      if (!deleteFlag)
      {
        federation.getIdGenerators().getItems().add(idgenerator);
      }

      String path = _settings.get("baseDirectory") + "/WEB-INF/data/federation.xml";
      JaxbUtils.write(federation, path, true);

      msgs.add("ID Generator saved.");
      response.setLevel(Level.SUCCESS);
    }
    catch (Exception ex)
    {
      msgs.add("Error while saving ID Generator.");
      response.setLevel(Level.ERROR);
      logger.error(ex);
    }

    messages.setItems(msgs);
    status.setMessages(messages);
    statuses.add(status);
    sl.setItems(statuses);
    response.setStatusList(sl);
    return response;
  }

  public Response saveRepository(Repository repository, boolean deleteFlag) throws Exception
  {
    Response response = new Response();
    StatusList sl = new StatusList();
    List<Status> statuses = new ArrayList<Status>();
    Status status = new Status();
    Messages messages = new Messages();
    List<String> msgs = new ArrayList<String>();
    boolean repositoryExist = false;
    int index = 0;
    try
    {
      Federation federation = getFederation();
      for (Repository repo : federation.getRepositories().getItems())
      {
        if (repo.getId().equalsIgnoreCase(repository.getId()))
        {
          index = federation.getRepositories().getItems().indexOf(repo);
          repositoryExist = true;
          break;
        }
      }
      if (repositoryExist)
      {
        federation.getRepositories().getItems().remove(index);
      }
      else
      {
        int sequenceId = federation.getRepositories().getSequenceId();
        repository.setId(Integer.toString(++sequenceId));
        federation.getRepositories().setSequenceId(sequenceId);
      }
      if (!deleteFlag)
      {
        federation.getRepositories().getItems().add(repository);
      }
      String path = _settings.get("baseDirectory") + "/WEB-INF/data/federation.xml";
      JaxbUtils.write(federation, path, true);

      msgs.add("Repository saved.");
      response.setLevel(Level.SUCCESS);
    }
    catch (Exception ex)
    {
      msgs.add("Error while saving Repository.");
      response.setLevel(Level.ERROR);
      logger.error(ex);
    }

    messages.setItems(msgs);
    status.setMessages(messages);
    statuses.add(status);
    sl.setItems(statuses);
    response.setStatusList(sl);
    return response;
  }

  public String readSparql(String queryName) throws Exception
  {
    try
    {
      String path = _settings.get("baseDirectory") + "/WEB-INF/data/Sparqls/";

      String query = IOUtils.readString(path + queryName);

      return query;
    }
    catch (Exception ex)
    {
      logger.error(ex);
      return "";
    }
  }

  public Version getVersion()
  {
    return new Version();
  }

  public Entity getClassLabel(String id) throws Exception
  {
    return getLabel(_nsmap.getNamespaceUri("rdl").toString() + id);
  }

  public Qmxf getClass(String id, Repository repository) throws Exception
  {
    return getClass(id, "", repository);
  }

  public Qmxf getClass(String id) throws Exception
  {
    return getClass(id, "", null);
  }

  public Qmxf getClass(String id, String namespaceUrl, Repository rep) throws Exception
  {
    Qmxf qmxf = new Qmxf();

    try
    {
      Name name;
      Description description;
      org.ids_adi.ns.qxf.model.Status status;
      String[] names = null;

      List<Classification> classifications = new ArrayList<Classification>();
      List<Specialization> specializations = new ArrayList<Specialization>();
      String sparql = "";
      // String relativeUri = "";

      Query queryContainsSearch = getQuery("GetClass");

      QueryBindings queryBindings = queryContainsSearch.getBindings();

      sparql = readSparql(queryContainsSearch.getFileName());

      if (namespaceUrl == null || namespaceUrl.isEmpty())
        namespaceUrl = _nsmap.getNamespaceUri("rdl").toString();

      String uri = namespaceUrl + id;

      sparql = sparql.replace("param1", uri);
      for (Repository repository : _repositories)
      {
        ClassDefinition classDefinition = null;

        if (rep != null)
          if (rep.getName().equals(repository.getName()))
          {
            continue;
          }
        Results sparqlResult = queryFromRepository(repository, sparql);

        List<Hashtable<String, String>> results = bindQueryResults(queryBindings, sparqlResult);

        classifications = new ArrayList<Classification>();
        specializations = new ArrayList<Specialization>();

        for (Hashtable<String, String> result : results)
        {
          classDefinition = new ClassDefinition();
          classDefinition.setId(uri);
          classDefinition.setRepository(repository.getName());
          name = new Name();
          description = new Description();
          status = new org.ids_adi.ns.qxf.model.Status();

          if (result.containsKey("type"))
          {
            URI typeName = new URI(result.get("type").substring(0, result.get("type").indexOf("#") + 1));
            if (_nsmap.getPrefix(typeName).equals("dm"))
            {
              EntityType et = new EntityType();
              et.setReference(result.get("type"));
              classDefinition.setEntityType(et);
            }
            else if (repository.getRepositoryType().equals(RepositoryType.PART_8))
            {
              continue;
            }
          }

          if (result.containsKey("label"))
          {
            names = result.get("label").split("@");
            name.setValue(names[0]);
            if (names.length == 1)
            {
              name.setLang(defaultLanguage);
            }
            else
            {
              name.setLang(names[names.length - 1]);
            }
          }

          // legacy properties
          if (result.containsKey("definition"))
          {
            names = result.get("definition").split("@");
            description.setValue(names[0]);
            if (names.length == 1)
            {
              description.setLang(defaultLanguage);
            }
            else
            {
              description.setLang(names[names.length - 1]);
            }
          }
          // description.value = result["definition"];

          if (result.containsKey("creator"))
          {
            status.setAuthority(result.get("creator"));
          }
          if (result.containsKey("creationDate"))
          {
            status.setFrom(result.get("creationDate"));
          }
          if (result.containsKey("class"))
          {
            status.setClazz(result.get("class"));
          }
          // camelot properties
          if (result.containsKey("comment"))
          {
            names = result.get("comment").split("@");
            description.setValue(names[0]);
            if (names.length == 1)
            {
              description.setLang(defaultLanguage);
            }
            else
            {
              description.setLang(names[names.length - 1]);
            }
          }
          if (result.containsKey("authority"))
          {
            status.setAuthority(result.get("authority"));
          }
          if (result.containsKey("recorded"))
          {
            status.setClazz(result.get("recorded"));
          }
          if (result.containsKey("from"))
          {
            status.setFrom(result.get("from"));
          }

          classDefinition.getNames().add(name);

          classDefinition.getDescriptions().add(description);
          classDefinition.getStatuses().add(status);

          classifications = getClassifications(id, repository);
          specializations = getSpecializations(id, repository);

          if (classifications.size() > 0)
          {
            classDefinition.setClassifications(classifications);
          }
          if (specializations.size() > 0)
          {
            classDefinition.setSpecializations(specializations);
          }
        }
        if (classDefinition != null)
        {
          qmxf.getClassDefinitions().add(classDefinition);

        }
      }

      return qmxf;
    }
    catch (RuntimeException e)
    {
      logger.error("Error in GetClass: " + e);
      return qmxf;
    }
  }

  private List<Specialization> getSpecializations(String id, Repository rep) throws Exception
  {
    List<Specialization> specializations = new ArrayList<Specialization>();
    try
    {
      String sparql = "";
      String sparqlPart8 = "";
      String relativeUri = "";
      String[] names = null;
      Results res = null;

      Query queryGetSpecialization = getQuery("GetSpecialization");
      QueryBindings queryBindings = queryGetSpecialization.getBindings();
      sparql = readSparql(queryGetSpecialization.getFileName());
      sparql = sparql.replace("param1", id);
      Query queryGetSubClassOf = getQuery("GetSubClassOf");
      QueryBindings queryBindingsPart8 = queryGetSubClassOf.getBindings();
      sparqlPart8 = readSparql(queryGetSubClassOf.getFileName());
      sparqlPart8 = sparqlPart8.replace("param1", id);
      for (Repository repository : _repositories)
      {
        if (rep != null)
        {
          if (!rep.getName().equals(repository.getName()))
          {
            continue;
          }
        }
        if (repository.getRepositoryType() == RepositoryType.PART_8)
        {
          Results sparqlResults = queryFromRepository(repository, sparqlPart8);

          List<Hashtable<String, String>> results = bindQueryResults(queryBindingsPart8, sparqlResults);

          for (Hashtable<String, String> result : results)
          {
            Specialization specialization = new Specialization();
            String uri = "";
            String label = "";
            String lang = "";
            if (result.containsKey("uri"))
            {
              uri = result.get("uri");
              specialization.setReference(uri);
            }
            if (result.containsKey("label"))
            {
              names = result.get("label").split("@", -1);
              label = names[0];
              if (names.length == 1)
              {
                lang = defaultLanguage;
              }
              else
              {
                lang = names[names.length - 1];
              }
            }
            else
            {
              names = getLabel(uri).getLabel().split("@",-1);
              label = names[0];
              if (names.length == 1)
              {
                lang = defaultLanguage;
              }
              else
              {
                lang = names[names.length - 1];
              }
            }
            specialization.setLabel(label);
            specialization.setLang(lang);
            specializations.add(specialization);
          }

        }
        else
        {
          Results sparqlResults = queryFromRepository(repository, sparql);

          List<Hashtable<String, String>> results = bindQueryResults(queryBindings, sparqlResults);

          for (Hashtable<String, String> result : results)
          {
            Specialization specialization = new Specialization();
            String uri = "";
            String label = "";
            String lang = "";

            if (result.containsKey("uri"))
            {
              uri = result.get("uri");
              specialization.setReference(uri);
            }
            if (result.containsKey("label"))
            {
              names = result.get("label").split("@", -1);
              label = names[0];
              if (names.length == 1)
              {
                lang = defaultLanguage;
              }
              else
              {
                lang = names[names.length - 1];
              }
            }
            else
            {
            	label = getLabel(uri).getLabel();
            }

            specialization.setLabel(label);
            specialization.setLang(lang);
            specializations.add(specialization);
          }
        }

      }
      return specializations;
    }
    catch (RuntimeException e)
    {
      logger.error("Error while Getting Class: " + e);
      return specializations;
    }
  }

  private List<Classification> getClassifications(String id, Repository rep) throws Exception
  {
    List<Classification> classifications = new ArrayList<Classification>();
    try
    {
      String sparql = "";
      String relativeUri = "";
      Query getClassification;
      QueryBindings queryBindings;

      for (Repository repository : _repositories)
      {
        if (rep != null)
        {
          if (rep.getName().equals(repository.getName()))
          {
            continue;
          }
        }
        switch (rep.getRepositoryType())
        {
        case CAMELOT:
        case RDS_WIP:

          getClassification = getQuery("GetClassification");
          queryBindings = getClassification.getBindings();

          sparql = readSparql(getClassification.getFileName());
          sparql = sparql.replace("param1", id);
          classifications = processClassifications(rep, sparql, queryBindings);
          break;
        case PART_8:
          getClassification = getQuery("GetPart8Classification");
          queryBindings = getClassification.getBindings();

          sparql = readSparql(getClassification.getFileName());
          sparql = sparql.replace("param1", id);
          classifications = processClassifications(rep, sparql, queryBindings);
          break;
        }
      }

      return classifications;
    }
    catch (Exception e)
    {
      logger.error("Error in GetClassifications: " + e);
      return classifications;
    }
  }

  private List<Classification> processClassifications(Repository repository, String sparql, QueryBindings queryBindings)
      throws Exception
  {
    // Results res = null;
    Results sparqlResults = queryFromRepository(repository, sparql);

    List<Hashtable<String, String>> results = bindQueryResults(queryBindings, sparqlResults);
    List<Classification> classifications = new ArrayList<Classification>();

    String[] names = null;

    for (Hashtable<String, String> result : results)
    {

      Classification classification = new Classification();
      String uri = "";
      String label = "";
      String lang = "";

      if (result.containsKey("uri"))
      {
        String pref = _nsmap.getPrefix(new URI(result.get("uri").substring(0, result.get("uri").indexOf("#") + 1)));
        String uriString = result.get("uri");
        if (pref.equals("owl") || pref.equals("dm"))
        {
          continue;
        }
        uri = uriString;
        classification.setReference(uri);
      }

      if (result.containsKey("label"))
      {
        names = result.get("label").split("@");
        label = names[0];
        if (names.length == 1)
          lang = defaultLanguage;
        else
          lang = names[names.length - 1];
      }
      else
      {
    	names = getLabel(uri).getLabel().split("@");
        label = names[0];
        if (names.length == 1)
          lang = defaultLanguage;
        else
          lang = names[names.length - 1];
      }
      classification.setLabel(label);
      classification.setLang(lang);
      classifications.add(classification);
    }

    return classifications;
  }

  public final Qmxf getTemplate(String id) throws Exception
  {
    Qmxf qmxf = new Qmxf();

    try
    {
      List<TemplateQualification> templateQualifications = getTemplateQualification(id, null);

      if (templateQualifications.size() > 0)
      {
        qmxf.setTemplateQualifications(templateQualifications);
      }
      else
      {
        List<TemplateDefinition> templateDefinitions = getTemplateDefinition(id, null);
        qmxf.setTemplateDefinitions(templateDefinitions);
      }
    }
    catch (RuntimeException ex)
    {
      logger.error("Error in GetTemplate: " + ex);
      return qmxf;
    }

    return qmxf;
  }

  public final org.iringtools.refdata.response.Response getClassMembers(String Id)
  {
	  org.iringtools.refdata.response.Response membersResult = new org.iringtools.refdata.response.Response();
	    Entities entities = new Entities();
	    membersResult.setEntities(entities);
	    List<Entity> entityList = new ArrayList<Entity>();
	    entities.setItems(entityList);
    try
    {
      String sparql = "";
      String language = "";
      String[] names = null;

      Query getMembers =  getQuery("GetMembers");
      QueryBindings memberBindings = getMembers.getBindings();
      sparql = readSparql(getMembers.getFileName());
      sparql = sparql.replace("param1", Id);

      for (Repository repository : _repositories)
      {
        Results sparqlResults = queryFromRepository(repository, sparql);

        List<Hashtable<String, String>> results = bindQueryResults(memberBindings, sparqlResults);

        for (Hashtable<String, String> result : results)
        {
          names = result.get("label").split("@");
          if (names.length == 1) {
            language = defaultLanguage;
          }
          else {
            language = names[names.length - 1];
          }
          Entity resultEntity = new Entity();
            resultEntity.setUri(result.get("uri"));
            resultEntity.setLabel(names[0]);
            resultEntity.setLang(language);
            resultEntity.setRepository(repository.getName());
          
          entityList.add(resultEntity);
         // Utility.SearchAndInsert(membersResult, resultEntity, Entity.sortAscending());
        } //queryResult.Add(resultEntity);
        membersResult.setTotal(entityList.size());
      }
    }
    catch (Exception ex)
    {
      logger.error("Error in Getmembers: " + ex);
    }
    return membersResult;
  }

  
  public final Qmxf getTemplate(String id, String templateType, Repository rep) throws HttpClientException, Exception
  {
    Qmxf qmxf = new Qmxf();
    List<TemplateQualification> templateQualification = null;
    List<TemplateDefinition> templateDefinition = null;
    try
    {
      if (templateType.equalsIgnoreCase("Qualification"))
      {
        templateQualification = getTemplateQualification(id, rep);
      }
      else
      {
        templateDefinition = getTemplateDefinition(id, rep);
      }

      if (templateQualification != null)
      {
        qmxf.getTemplateQualifications().addAll(templateQualification);
      }
      else
      {
        qmxf.getTemplateDefinitions().addAll(templateDefinition);
      }
    }
    catch (RuntimeException ex)
    {
      logger.error("Error in GetTemplate: " + ex);
      return qmxf;
    }

    return qmxf;
  }

  private List<TemplateDefinition> getTemplateDefinition(String id, Repository rep) throws Exception
  {
    List<TemplateDefinition> templateDefinitionList = new ArrayList<TemplateDefinition>();
    TemplateDefinition templateDefinition = null;

    try
    {
      String sparql = "";
      String relativeUri = "";
      String[] names = null;

      Description description = new Description();
      org.ids_adi.ns.qxf.model.Status status = new org.ids_adi.ns.qxf.model.Status();

      List<Entity> resultEntities = new ArrayList<Entity>();

      Query queryContainsSearch = getQuery("GetTemplate");
      QueryBindings queryBindings = queryContainsSearch.getBindings();

      sparql = readSparql(queryContainsSearch.getFileName());
      sparql = sparql.replace("param1", id);
      for (Repository repository : _repositories)
      {
        if (rep != null)
        {
          if (!rep.getName().equals(repository.getName()))
          {
            continue;
          }
        }

        Results sparqlResults = queryFromRepository(repository, sparql);

        List<Hashtable<String, String>> results = bindQueryResults(queryBindings, sparqlResults);

        for (Hashtable<String, String> result : results)
        {
          templateDefinition = new TemplateDefinition();
          Name name = new Name();

          templateDefinition.setRepository(repository.getName());

          if (result.containsKey("label"))
          {
            names = result.get("label").split("@", -1);
            name.setValue(names[0]);
            if (names.length == 1)
            {
              name.setLang(defaultLanguage);
            }
            else
            {
              name.setLang(names[names.length - 1]);
            }
          }

          if (result.containsKey("definition"))
          {
            names = result.get("definition").split("@", -1);
            description.setValue(names[0]);
            if (names.length == 1)
            {
              description.setLang(defaultLanguage);
            }
            else
            {
              description.setLang(names[names.length - 1]);
            }
          }

          if (result.containsKey("creationDate"))
          {
            status.setFrom(result.get("creationDate"));
          }
          templateDefinition.setId("tpl:" + id);
          templateDefinition.getNames().add(name);
          templateDefinition.getDescriptions().add(description);
          templateDefinition.getStatuses().add(status);

          templateDefinition.getRoleDefinitions().addAll(getRoleDefinition(id, repository));
          templateDefinitionList.add(templateDefinition);
        }
      }

      return templateDefinitionList;
    }
    catch (RuntimeException e)
    {
      logger.error("Error in GetTemplateDefinition: " + e);
      return templateDefinitionList;
    }
  }

  private List<RoleDefinition> getRoleDefinition(String id, Repository repository) throws Exception
  {

    List<RoleDefinition> roleDefinitions = new ArrayList<RoleDefinition>();
    try
    {
      String sparql = "";
      String relativeUri = "";
      String sparqlQuery = "";
      String[] names = null;

      Description description = new Description();
      org.ids_adi.ns.qxf.model.Status status = new org.ids_adi.ns.qxf.model.Status();

      List<Entity> resultEntities = new ArrayList<Entity>();

      switch (repository.getRepositoryType())
      {
      case PART_8:
        sparqlQuery = "GetPart8Roles";
        break;
      case CAMELOT:
      case RDS_WIP:
        sparqlQuery = "GetRoles";
        break;
      }
      Query queryContainsSearch = getQuery(sparqlQuery);
      QueryBindings queryBindings = queryContainsSearch.getBindings();

      sparql = readSparql(queryContainsSearch.getFileName());
      sparql = sparql.replace("param1", id);

      Results sparqlResults = queryFromRepository(repository, sparql);
      List<Hashtable<String, String>> results = bindQueryResults(queryBindings, sparqlResults);

      for (Hashtable<String, String> result : results)
      {
        RoleDefinition roleDefinition = new RoleDefinition();
        Name name = new Name();

        if (result.containsKey("label"))
        {
          names = result.get("label").split("@", -1);
          name.setValue(names[0]);
          if (names.length == 1)
          {
            name.setLang(defaultLanguage);
          }
          else
          {
            name.setLang(names[names.length - 1]);
          }
        }
        if (result.containsKey("role"))
        {
          roleDefinition.setId(result.get("role"));
        }
        if (result.containsKey("comment"))
        {
          names = result.get("comment").split("@", -1);
          description.setValue(names[0]);

          if (names.length == 1)
          {
            description.setLang(defaultLanguage);
          }
          else
          {
            description.setLang(names[names.length - 1]);
          }
        }
        if (result.containsKey("index"))
        {
          description.setValue(result.get("index").toString());
        }
        if (result.containsKey("type"))
        {
          roleDefinition.setRange(result.get("type"));
        }
        roleDefinition.getNames().add(name);
        roleDefinition.getDescriptions().add(description);
        roleDefinitions.add(roleDefinition);
        // Utility.SearchAndInsert(roleDefinitions, roleDefinition,
        // RoleDefinition.sortAscending());
      }

      return roleDefinitions;
    }
    catch (RuntimeException e)
    {
      logger.error("Error in GetRoleDefinition: " + e);
      return roleDefinitions;
    }
  }

  private List<TemplateQualification> getTemplateQualification(String id, Repository rep) throws Exception,
      HttpClientException
  {
    TemplateQualification templateQualification = null;
    List<TemplateQualification> templateQualificationList = new ArrayList<TemplateQualification>();
    String[] names = null;

    try
    {
      String sparql = "";
      String relativeUri = "";
      String sparqlQuery = "";

      List<Entity> resultEntities = new ArrayList<Entity>();

      Query getTemplateQualification = null;
      QueryBindings queryBindings = null;

      {
        for (Repository repository : _repositories)
        {
          if (rep != null)
          {
            if (!rep.getName().equals(repository.getName()))
            {
              continue;
            }
          }

          switch (repository.getRepositoryType())
          {
          case CAMELOT:
          case RDS_WIP:
            sparqlQuery = "GetTemplateQualification";
            break;
          case PART_8:
            sparqlQuery = "GetTemplateQualification";
            break;
          }
          getTemplateQualification = getQuery(sparqlQuery);
          queryBindings = getTemplateQualification.getBindings();

          sparql = readSparql(getTemplateQualification.getFileName());
          sparql = sparql.replace("param1", id);

          Results sparqlResults = queryFromRepository(repository, sparql);

          List<Hashtable<String, String>> results = bindQueryResults(queryBindings, sparqlResults);
          for (Hashtable<String, String> result : results)
          {
            templateQualification = new TemplateQualification();
            Description description = new Description();
            org.ids_adi.ns.qxf.model.Status status = new org.ids_adi.ns.qxf.model.Status();
            Name name = new Name();

            templateQualification.setRepository(repository.getName());

            if (result.containsKey("name"))
            {
              names = result.get("name").split("@", -1);
              name.setValue(names[0]);
            }
            if (names.length == 1)
            {
              name.setLang(defaultLanguage);
            }
            else
            {
              name.setLang(names[names.length - 1]);
            }

            if (result.containsKey("description"))
            {
              names = result.get("description").split("@", -1);
              description.setValue(names[0]);
            }
            if (names.length == 1)
            {
              description.setLang(defaultLanguage);
            }
            else
            {
              description.setLang(names[names.length - 1]);
            }
            if (result.containsKey("statusClass"))
            {
              status.setClazz(result.get("statusClass"));
            }
            if (result.containsKey("statusAuthority"))
            {
              status.setAuthority(result.get("statusAuthority"));
            }
            if (result.containsKey("qualifies"))
            {
              templateQualification.setQualifies(result.get("qualifies"));
            }

            templateQualification.setId(id);
            templateQualification.getNames().add(name);
            templateQualification.getDescriptions().add(description);
            templateQualification.getStatuses().add(status);

            templateQualification.getRoleQualifications().addAll(getRoleQualification(id, repository));
            templateQualificationList.add(templateQualification);
          }
        }
      }
      return templateQualificationList;
    }
    catch (RuntimeException e)
    {
      logger.error("Error in GetTemplateQualification: " + e);
      return templateQualificationList;
    }
  }

  private List<RoleQualification> getRoleQualification(String id, Repository rep) throws Exception
  {

    List<RoleQualification> roleQualifications = new ArrayList<RoleQualification>();
    try
    {
      String rangeSparql = "";
      String[] names = null;
      String referenceSparql = "";
      String valueSparql = "";
      Description description = new Description();
      org.ids_adi.ns.qxf.model.Status status = new org.ids_adi.ns.qxf.model.Status();

      for (Repository repository : _repositories)
      {
        if (rep != null)
        {
          if (!rep.getName().equals(repository.getName()))
          {
            continue;
          }
        }
        switch (rep.getRepositoryType())
        {
        case CAMELOT:
        case RDS_WIP:

          List<Entity> rangeResultEntities = new ArrayList<Entity>();
          List<Entity> referenceResultEntities = new ArrayList<Entity>();
          List<Entity> valueResultEntities = new ArrayList<Entity>();

          Query getRangeRestriction = getQuery("GetRangeRestriction");
          QueryBindings rangeRestrictionBindings = getRangeRestriction.getBindings();

          Query getReferenceRestriction = getQuery("GetReferenceRestriction");
          QueryBindings referenceRestrictionBindings = getReferenceRestriction.getBindings();

          Query getValueRestriction = getQuery("GetValueRestriction");
          QueryBindings valueRestrictionBindings = getValueRestriction.getBindings();

          rangeSparql = readSparql(getRangeRestriction.getFileName());
          rangeSparql = rangeSparql.replace("param1", id);

          referenceSparql = readSparql(getReferenceRestriction.getFileName());
          referenceSparql = referenceSparql.replace("param1", id);

          valueSparql = readSparql(getValueRestriction.getFileName());
          valueSparql = valueSparql.replace("param1", id);
          Results rangeSparqlResults = queryFromRepository(repository, rangeSparql);
          Results referenceSparqlResults = queryFromRepository(repository, referenceSparql);
          Results valueSparqlResults = queryFromRepository(repository, valueSparql);

          List<Hashtable<String, String>> rangeBindingResults = bindQueryResults(rangeRestrictionBindings,
              rangeSparqlResults);
          List<Hashtable<String, String>> referenceBindingResults = bindQueryResults(referenceRestrictionBindings,
              referenceSparqlResults);
          List<Hashtable<String, String>> valueBindingResults = bindQueryResults(valueRestrictionBindings,
              valueSparqlResults);

          List<Hashtable<String, String>> combinedResults = mergeLists(
              mergeLists(rangeBindingResults, referenceBindingResults), valueBindingResults);

          for (Hashtable<String, String> combinedResult : combinedResults)
          {

            RoleQualification roleQualification = new RoleQualification();
            String uri = "";
            if (combinedResult.containsKey("qualifies"))
            {
              uri = combinedResult.get("qualifies");
              roleQualification.setQualifies(uri);
              roleQualification.setId(getIdFromURI(uri));
            }
            if (combinedResult.containsKey("name"))
            {
              String nameValue = combinedResult.get("name");

              if (nameValue == null)
              {
            	  nameValue = getLabel(uri).getLabel();
              }
              names = nameValue.split("@", -1);

              Name name = new Name();
              if (names.length > 1)
              {
                name.setLang(names[names.length - 1]);
              }
              else
              {
                name.setLang(defaultLanguage);
              }

              name.setValue(names[0]);

              roleQualification.getNames().add(name);
            }
            else
            {
            	String nameValue = getLabel(uri).getLabel();

              if (nameValue.equals(""))
              {
                nameValue = "tpl:" + getIdFromURI(uri);
              }

              Name name = new Name();
              names = nameValue.split("@", -1);

              if (names.length > 1)
              {
                name.setLang(names[names.length - 1]);
              }
              else
              {
                name.setLang(defaultLanguage);
              }
              name.setValue(names[0]);

              roleQualification.getNames().add(name);
            }
            if (combinedResult.containsKey("range"))
            {
              roleQualification.setRange(combinedResult.get("range"));
            }
            if (combinedResult.containsKey("reference"))
            {
              org.ids_adi.ns.qxf.model.Value tempVar = new org.ids_adi.ns.qxf.model.Value();
              tempVar.setReference(combinedResult.get("reference"));
              org.ids_adi.ns.qxf.model.Value value = tempVar;

              roleQualification.setValue(value);
            }
            if (combinedResult.containsKey("value"))
            {
              org.ids_adi.ns.qxf.model.Value tempVar2 = new org.ids_adi.ns.qxf.model.Value();
              tempVar2.setText(combinedResult.get("value"));
              tempVar2.setAs(combinedResult.get("value_dataType"));
              org.ids_adi.ns.qxf.model.Value value = tempVar2;

              roleQualification.setValue(value);
            }
            roleQualifications.add(roleQualification);
          }
          break;
        case PART_8:
          List<Entity> part8Entities = new ArrayList<Entity>();
          Query getPart8Roles = getQuery("GetPart8Roles");
          QueryBindings getPart8RolesBindings = getPart8Roles.getBindings();

          String part8RolesSparql = readSparql(getPart8Roles.getFileName());
          part8RolesSparql = part8RolesSparql.replace("param1", id);
          Results part8RolesResults = queryFromRepository(repository, part8RolesSparql);
          List<Hashtable<String, String>> part8RolesBindingResults = bindQueryResults(getPart8RolesBindings,
              part8RolesResults);
          for (Hashtable<String, String> result : part8RolesBindingResults)
          {
            if (result.containsKey("comment"))
            {
            }

            if (result.containsKey("type"))
            {
            }

            if (result.containsKey("label"))
            {
            }

            if (result.containsKey("index"))
            {
            }

            if (result.containsKey("role"))
            {
            }
            RoleQualification roleQualification = new RoleQualification();
          }
          break;
        }
      }
      return roleQualifications;
    }
    catch (RuntimeException e)
    {
      logger.error("Error in GetRoleQualification: " + e);
      return roleQualifications;
    }
  }

  private String getIdFromURI(String uri)
  {
    String id = uri;
    if (uri != null || !uri.isEmpty())
    {
      if (id.contains("#"))
      {
        id = id.substring(id.lastIndexOf("#") + 1);
      }
      else if (id.contains(":"))
      {
        id = id.substring(id.lastIndexOf(":") + 1);
      }
    }
    if (id == null)
    {
      id = "";
    }
    return id;
  }

  private List<Hashtable<String, String>> mergeLists(List<Hashtable<String, String>> a,
      List<Hashtable<String, String>> b)
  {
    try
    {
      for (Hashtable<String, String> dictionary : b)
      {
        a.add(dictionary);
      }
      return a;
    }
    catch (RuntimeException ex)
    {
      throw ex;
    }
  }

  public Response postTemplate(Qmxf qmxf)
  {

    Response response = new Response();
    response.setLevel(Level.SUCCESS);
    boolean qn = false;
    ReferenceObject qName = null;
    try
    {
      Repository repository = getRepository(qmxf.getTargetRepository());

      if (repository == null || repository.isIsReadOnly())
      {
        Status status = new Status();
        // status.Level = StatusLevel.Error;

        if (repository == null)
          status.getMessages().getItems().add("Repository not found!");
        else
          status.getMessages().getItems().add("Repository [" + qmxf.getTargetRepository() + "] is read-only!");

        // _response.Append(status);
      }
      else
      {
        String registry = (_useExampleRegistryBase) ? _settings.get("ExampleRegistryBase") : _settings
            .get("ClassRegistryBase");
        StringBuilder sparqlDelete = new StringBuilder();

        // region Template Definitions
        if (qmxf.getTemplateDefinitions().size() > 0)
        {
          for (TemplateDefinition newTemplateDefinition : qmxf.getTemplateDefinitions())
          {
            String language = null;
            int roleCount = 0;
            StringBuilder sparqlAdd = new StringBuilder();

            sparqlAdd.append(insertData);
            boolean hasDeletes = false;
            String templateName = null;
            String identifier = null;
            String generatedId = null;
            String roleDefinition = null;
            int index = 1;
            if (newTemplateDefinition.getId() != null)
            {
              identifier = getIdFromURI(newTemplateDefinition.getId());
            }

            templateName = newTemplateDefinition.getNames().get(0).getValue();
            // check for exisiting template
            Qmxf existingQmxf = new Qmxf();
            if (identifier != null)
            {
              // existingQmxf = getTemplate(identifier,
              // QMXFType.Definition, repository);
              existingQmxf = getTemplate(identifier, TemplateType.DEFINITION.toString(), repository);
            }
            else
            {
              if (_useExampleRegistryBase)
                generatedId = createIdsAdiId(_settings.get("ExampleRegistryBase"), templateName);
              else
                generatedId = createIdsAdiId(_settings.get("TemplateRegistryBase"), templateName);
              identifier = getIdFromURI(generatedId);
            }
            // region Form Delete/Insert SPARQL
            if (existingQmxf.getTemplateDefinitions().size() > 0)
            {
              StringBuilder sparqlStmts = new StringBuilder();

              for (TemplateDefinition existingTemplate : existingQmxf.getTemplateDefinitions())
              {
                for (Name name : newTemplateDefinition.getNames())
                {
                  templateName = name.getValue();
                  // Name existingName =
                  // existingTemplate.name.Find(n => n.lang ==
                  // name.lang);
                  Name existingName = new Name();
                  for (Name tempName : existingTemplate.getNames())
                  {
                    if (name.getLang().equalsIgnoreCase(tempName.getLang()))
                    {
                      existingName = tempName;
                    }
                  }
                  if (existingName.getLang() == null)
                    language = "@" + defaultLanguage;
                  else
                    language = "@" + existingName.getLang();

                  if (existingName != null)
                  {
                    if (!existingName.getValue().equalsIgnoreCase(name.getValue()))
                    {
                      hasDeletes = true;
                      sparqlStmts.append(String.format(" tpl:{0} rdfs:subClassOf p8:BaseTemplateStatement .",
                          identifier));
                      sparqlStmts.append(String.format("tpl:{0} rdfs:label \"{1}{2}\"^^xsd:string .", identifier,
                          existingName.getValue(), language));
                      sparqlAdd
                          .append(String.format(" tpl:{0} rdfs:subClassOf p8:BaseTemplateStatement .", identifier));
                      sparqlAdd.append(String.format("tpl:{0} rdfs:label \"{1}{2}\"^^xsd:string .", identifier,
                          name.getValue(), language));
                    }
                  }
                }
                // append changing descriptions to each block
                for (Description description : newTemplateDefinition.getDescriptions())
                {
                  // Description existingDescription =
                  // existingTemplate.description.Find(d =>
                  // d.lang == description.lang);
                  Description existingDescription = new Description();
                  for (Description tempName : existingTemplate.getDescriptions())
                  {
                    if (description.getLang().equalsIgnoreCase(tempName.getLang()))
                    {
                      existingDescription = tempName;
                    }
                  }
                  if (existingDescription.getLang() == null)
                    language = "@" + defaultLanguage;
                  else
                    language = "@" + existingDescription.getLang();

                  if (existingDescription != null)
                  {
                    if (!existingDescription.getValue().equalsIgnoreCase(description.getValue()))
                    {
                      hasDeletes = true;
                      sparqlStmts.append(String.format("rdl:{0} rdfs:comment \"{1}{2}\"^^xsd:string .", identifier,
                          existingDescription.getValue(), language));
                      sparqlAdd.append(String.format("rdl:{0} rdfs:comment \"{1}{2}\"^^xsd:string .", identifier,
                          description.getValue(), language));
                    }
                  }
                }

                // role count
                if (existingTemplate.getRoleDefinitions().size() != newTemplateDefinition.getRoleDefinitions().size())
                {
                  hasDeletes = true;
                  sparqlStmts.append(String.format("tpl:{0} p8:valNumberOfRoles {1}^^xsd:int .", identifier,
                      existingTemplate.getRoleDefinitions().size()));
                  sparqlAdd.append(String.format("tpl:{0} p8:valNumberOfRoles {1}^^xsd:int .", identifier,
                      newTemplateDefinition.getRoleDefinitions().size()));
                }

                index = 1;
                for (RoleDefinition role : newTemplateDefinition.getRoleDefinitions())
                {
                  String roleIdentifier = role.getId();
                  hasDeletes = false;

                  // get existing role if it exists
                  // RoleDefinition existingRole =
                  // existingTemplate.roleDefinition.Find(r =>
                  // r.identifier == role.identifier);
                  RoleDefinition existingRole = null;
                  for (RoleDefinition tempRoleDef : existingTemplate.getRoleDefinitions())
                  {
                    if (role.getId().equalsIgnoreCase(tempRoleDef.getId()))
                    {
                      existingRole = tempRoleDef;
                    }
                  }
                  // remove existing role from existing
                  // template, leftovers will be deleted later
                  existingTemplate.getRoleDefinitions().remove(existingRole);

                  if (existingRole != null)
                  {
                    // region Process Changing Role
                    String label = null;

                    for (Name name : role.getNames())
                    {
                      // QMXFName existingName =
                      // existingRole.name.Find(n =>
                      // n.lang == name.lang);
                      Name existingName = new Name();
                      for (Name tempName : existingRole.getNames())
                      {
                        if (name.getLang().equalsIgnoreCase(tempName.getLang()))
                        {
                          existingName = tempName;
                        }
                      }
                      if (existingName != null)
                      {
                        if (!existingName.getValue().equalsIgnoreCase(name.getValue()))
                        {
                          hasDeletes = true;
                          sparqlStmts.append(String.format("tpl:{0} rdf:type owl:Class .", existingRole.getId()));
                          sparqlStmts.append(String.format("tpl:{0}  rdfs:label \"{1}{2}\"^^xsd:string .",
                              existingRole.getId(), existingName.getValue(), name.getLang()));
                        }
                        // index
                        if (existingRole.getDesignation() != String.valueOf(index))
                        {
                          sparqlStmts.append(String.format("tpl:{0} p8:valRoleIndex {1}^^xsd:int .",
                              existingRole.getId(), existingRole.getDesignation()));
                        }
                      }
                    }
                    if (existingRole.getRange() != null)
                    {
                      if (!existingRole.getRange().equalsIgnoreCase(role.getRange()))
                      {
                        hasDeletes = true;
                        sparqlStmts.append(String.format("tpl:{0} p8:hasRoleFillerType {1} .", existingRole.getId(),
                            existingRole.getRange()));
                      }
                    }
                    // endregion
                  }
                  else
                  {
                    // region Insert New Role

                    String roleLabel = role.getNames().get(0).getValue().split("@")[0];
                    String roleID = null;
                    generatedId = null;
                    String genName = null;

                    if (role.getNames().get(0).getLang() == null)
                      language = "@" + defaultLanguage;
                    else
                      language = "@" + role.getNames().get(0).getLang();

                    genName = "Role definition " + roleLabel;
                    if (role.getId() == null)
                    {
                      if (_useExampleRegistryBase)
                        generatedId = createIdsAdiId(_settings.get("ExampleRegistryBase"), genName);
                      else
                        generatedId = createIdsAdiId(_settings.get("TemplateRegistryBase"), genName);
                      roleID = getIdFromURI(generatedId);
                    }
                    else
                    {
                      roleID = getIdFromURI(role.getId());
                    }
                    // sparqlAdd.append(String.format("tpl:{0} rdf:type owl:Class .",
                    // roleID));
                    sparqlAdd.append(String.format("tpl:{0} rdfs:label \"{1}{2}\"^^xsd:string .", roleID, roleLabel,
                        language));
                    sparqlAdd.append(String.format("tpl:{0} p8:valRoleIndex {1} .", roleID, index));
                    sparqlAdd.append(String.format("tpl:{0} p8:hasTemplate tpl:{1} .", roleID, identifier));

                    if (role.getRange() != null)
                    {
                      qn = _nsmap.reduceToQName(role.getRange(), qName);
                      if(qn)
                      sparqlAdd.append(String.format("tpl:{0} p8:hasRoleFillerType {1} .", roleID, qName));
                    }

                    /*
                     * for (Restriction restriction : role.getRestrictions()) {
                     * 
                     * }
                     */
                    sparqlAdd.append(String.format("tpl:{0} p8:hasRole rdl:{1} .", identifier, roleID));
                    // endregion
                  }

                  index++;
                }
              }

              sparqlDelete.append(prefix);
              sparqlDelete.append(deleteWhere);
              sparqlDelete.append(sparqlStmts);
              sparqlDelete.append(" }; ");
            }
            // endregion

            // region Form Insert SPARQL
            if (hasDeletes)
            {
              sparqlAdd.append("}");
            }
            else
            {
              String label = null;
              String labelSparql = null;
              // form labels
              for (Name name : newTemplateDefinition.getNames())
              {
                label = name.getValue().split("@")[0];

                if (name.getLang() == null)
                  language = "@" + defaultLanguage;
                else
                  language = "@" + name.getLang();
              }

              // sparqlAdd.append(String.format("  tpl:{0} rdf:type owl:Class .",
              // identifier));
              sparqlAdd.append(String.format("  tpl:{0} rdfs:label \"{1}{2}\"^^xsd:string .", identifier, label,
                  language));
              sparqlAdd.append(String.format("  tpl:{0} rdfs:subClassOf p8:BaseTemplateStatement .", identifier));
              sparqlAdd.append(String.format("  tpl:{0} rdf:type p8:Template .", identifier));

              // add descriptions to sparql
              for (Description descr : newTemplateDefinition.getDescriptions())
              {
                if (descr.getValue() == null)
                  continue;
                else
                {
                  if (descr.getLang() == null)
                    language = "@" + defaultLanguage;
                  else
                    language = "@" + descr.getLang();

                  sparqlAdd.append(String.format("tpl:{0} rdfs:comment \"{0}{1}\"^^xsd:string .", identifier, descr
                      .getValue().split("@")[0], language));
                }
              }
              sparqlAdd.append(String.format(" tpl:{0} p8:valNumberOfRoles {1} .", identifier, newTemplateDefinition
                  .getRoleDefinitions().size()));
              for (RoleDefinition role : newTemplateDefinition.getRoleDefinitions())
              {
                String roleLabel = role.getNames().get(0).getValue().split("@")[0];
                String roleID = null;
                generatedId = null;
                String genName = null;
                String range = role.getRange();

                if (role.getNames().get(0).getLang() == null)
                  language = "@" + defaultLanguage;
                else
                  language = role.getNames().get(0).getLang();

                genName = "Role definition " + roleLabel;
                if (role.getId() == null)
                {
                  if (_useExampleRegistryBase)
                    generatedId = createIdsAdiId(_settings.get("ExampleRegistryBase"), genName);
                  else
                    generatedId = createIdsAdiId(_settings.get("TemplateRegistryBase"), genName);
                  roleID = getIdFromURI(generatedId);
                }
                else
                {
                  roleID = getIdFromURI(role.getId());
                }
                // sparqlAdd.append(String.format("  tpl:{0} rdf:type owl:Class .",
                // roleID));
                sparqlAdd.append(String.format("  tpl:{0} rdfs:label \"{1}{2}\"^^xsd:string .", roleID, roleLabel,
                    language));
                sparqlAdd.append(String.format("  tpl:{0} p8:valRoleIndex {1} .", roleID, ++roleCount));
                sparqlAdd.append(String.format("  tpl:{0} p8:hasTemplate tpl:{1} .", roleID, identifier));
                sparqlAdd.append(String.format("  tpl:{0} p8:hasRole tpl:{1} .", identifier, roleID));

                if (role.getRange() != null)
                {
                  qn = _nsmap.reduceToQName(role.getRange(), qName);
                  if(qn)
                  sparqlAdd.append(String.format("tpl:{0} p8:hasRoleFillerType {1} .", roleID, qName));
                }
              }
              sparqlAdd.append("}");
            }
            // endregion

            // add prefix first
            sparqlBuilder.append(prefix);
            sparqlBuilder.append(sparqlDelete);
            sparqlBuilder.append(sparqlAdd);

            String sparql = sparqlBuilder.toString();
            Response postResponse = postToRepository(repository, sparql);
            // response.append(postResponse);
          }
        }

        // endregion Template Definitions

        // region Template Qualification

        if (qmxf.getTemplateQualifications().size() > 0)
        {
          for (TemplateQualification newTemplateQualification : qmxf.getTemplateQualifications())
          {
            String language = null;
            int roleCount = 0;
            StringBuilder sparqlAdd = new StringBuilder();

            sparqlAdd.append(insertData);
            boolean hasDeletes = false;
            String templateName = null;
            String identifier = null;
            String generatedId = null;
            String roleQualification = null;
            int index = 1;
            if (newTemplateQualification.getId() != null)
            {
              identifier = getIdFromURI(newTemplateQualification.getId());
            }

            templateName = newTemplateQualification.getNames().get(0).getValue();
            // check for exisitng template
            Qmxf existingQmxf = new Qmxf();
            if (identifier != null)
            {
              // existingQmxf = GetTemplate(identifier,
              // QMXFType.Qualification, repository);
              existingQmxf = getTemplate(identifier, TemplateType.QUALIFICATION.toString(), repository);
            }
            else
            {
              if (_useExampleRegistryBase)
                generatedId = createIdsAdiId(_settings.get("ExampleRegistryBase"), templateName);
              else
                generatedId = createIdsAdiId(_settings.get("TemplateRegistryBase"), templateName);

              identifier = getIdFromURI(generatedId);
            }
            // region Form Delete/Insert SPARQL
            if (existingQmxf.getTemplateQualifications().size() > 0)
            {
              StringBuilder sparqlStmts = new StringBuilder();

              for (TemplateQualification existingTemplate : existingQmxf.getTemplateQualifications())
              {
                for (Name name : newTemplateQualification.getNames())
                {
                  templateName = name.getValue();
                  // Name existingName =
                  // existingTemplate.name.Find(n => n.lang ==
                  // name.lang);
                  Name existingName = new Name();
                  for (Name tempName : existingTemplate.getNames())
                  {
                    if (name.getLang().equalsIgnoreCase(tempName.getLang()))
                    {
                      existingName = tempName;
                    }
                  }
                  if (existingName.getLang() == null)
                    language = "@" + defaultLanguage;
                  else
                    language = "@" + existingName.getLang();

                  if (existingName != null)
                  {
                    if (!existingName.getValue().equalsIgnoreCase(name.getValue()))
                    {
                      hasDeletes = true;
                      sparqlStmts.append(String.format("tpl:{0} rdfs:label \"{1}{2}\"^^xsd:string .", identifier,
                          existingName.getValue(), language));
                      sparqlStmts.append(String.format("tpl:{0} rdf:type p8:TemplateDescription .", identifier));
                      sparqlStmts.append(String.format(
                          "tpl:{0} rdf:hasTemplate p8:{1} .",
                          identifier,
                          existingName.getValue().replaceFirst(
                              existingName.getValue().substring(0, existingName.getValue().lastIndexOf("_") + 1), "")));

                      sparqlAdd.append(String.format("tpl:{0} rdfs:label \"{1}{2}\"^^xsd:string .", identifier,
                          name.getValue(), language));
                      sparqlAdd.append(String.format("tpl:{0} rdf:type p8:TemplateDescription .", identifier));
                      sparqlAdd.append(String.format("tpl:{0} rdf:hasTemplate p8:{1} .", identifier, name.getValue()
                          .replaceFirst(name.getValue().substring(0, name.getValue().lastIndexOf("_") + 1), "")));
                    }
                  }
                }

                // role count
                if (existingTemplate.getRoleQualifications().size() != newTemplateQualification.getRoleQualifications()
                    .size())
                {
                  hasDeletes = true;
                  sparqlStmts.append(String.format("tpl:{0} p8:valNumberOfRoles {1}^^xsd:int .", identifier,
                      existingTemplate.getRoleQualifications().size()));
                  sparqlAdd.append(String.format("tpl:{0} p8:valNumberOfRoles {1}^^xsd:int .", identifier,
                      newTemplateQualification.getRoleQualifications().size()));
                }

                for (Specialization spec : newTemplateQualification.getSpecializations())
                {
                  // Specialization existingSpecialization =
                  // existingTemplate.specialization.Find(n =>
                  // n.reference == spec.reference);
                  Specialization existingSpecialization = new Specialization();
                  for (Specialization tempSpecialisation : existingTemplate.getSpecializations())
                  {
                    if (spec.getReference().equalsIgnoreCase(tempSpecialisation.getReference()))
                    {
                      existingSpecialization = tempSpecialisation;
                    }
                  }
                  if (existingSpecialization != null)
                  {
                    String specialization = spec.getReference();
                    String existingSpec = existingSpecialization.getReference();

                    hasDeletes = true;
                    sparqlStmts.append(String.format("tpl:{0} rdf:type p8:TemplateSpecialization .", existingSpec));
                    sparqlStmts.append(String.format("tpl:{0} rdfs:label \"{0}{1}\"^^xds:string .", existingSpec,
                        existingSpecialization.getLabel().split("@")[0], language));
                    sparqlStmts
                        .append(String.format("tpl:{0} p8:hasSuperTemplate tpl:{1} .", existingSpec, identifier));
                    sparqlStmts.append(String.format("tpl:{0} p8:hasSubTemplate tpl:{1} .", identifier, existingSpec));

                    sparqlAdd.append(String.format("tpl:{0} rdf:type p8:TemplateSpecialization .", specialization));
                    sparqlAdd.append(String.format("tpl:{0} rdfs:label \"{0}{1}\"^^xds:string .", specialization, spec
                        .getLabel().split("@")[0], language));
                    sparqlAdd
                        .append(String.format("tpl:{0} p8:hasSuperTemplate tpl:{1} .", specialization, identifier));
                    sparqlAdd.append(String.format("tpl:{0} p8:hasSubTemplate tpl:{1} .", identifier, specialization));
                  }
                }

                index = 1;
                for (RoleQualification role : newTemplateQualification.getRoleQualifications())
                {
                  String roleIdentifier = role.getId();
                  hasDeletes = false;

                  // get existing role if it exists
                  // RoleQualification existingRole =
                  // existingTemplate.roleQualification.Find(r
                  // => r.identifier == role.identifier);
                  RoleQualification existingRole = null;
                  for (RoleQualification tempExistingRole : existingTemplate.getRoleQualifications())
                  {
                    if (role.getId().equalsIgnoreCase(tempExistingRole.getId()))
                    {
                      existingRole = tempExistingRole;
                    }
                  }
                  // remove existing role from existing
                  // template, leftovers will be deleted later
                  existingTemplate.getRoleQualifications().remove(existingRole);

                  if (existingRole != null)
                  {
                    // region Process Changing Role
                    String label = null;

                    for (Name name : role.getNames())
                    {
                      // QMXFName existingName =
                      // existingRole.name.Find(n =>
                      // n.lang == name.lang);
                      Name existingName = new Name();
                      for (Name tempExistingName : existingRole.getNames())
                      {
                        if (name.getLang().equalsIgnoreCase(tempExistingName.getLang()))
                        {
                          existingName = tempExistingName;
                        }
                      }
                      if (existingName != null)
                      {
                        if (!existingName.getValue().equalsIgnoreCase(name.getValue()))
                        {
                          // /TODO: Why are we
                          // removing this? We should
                          // remove the role from the
                          // template only and not
                          // from the repository
                          hasDeletes = true;
                          sparqlStmts.append(String.format("tpl:{0} rdf:type owl:Class .", existingRole.getId()));
                          sparqlStmts.append(String.format("tpl:{0}  rdfs:label \"{1}{2}\"^^xsd:string .",
                              existingRole.getId(), existingName.getValue(), name.getLang()));
                        }
                        // index
                        // if
                        // (existingRole.designation.value
                        // != index.ToString())
                        // {
                        // sparqlStmts.append(String.format("tpl:{0} p8:valRoleIndex {1}^^xsd:int .",
                        // existingRole.identifier,
                        // existingRole.designation.value));
                        // }
                      }
                    }
                    if (existingRole.getRange() != null)
                    {
                      if (!existingRole.getRange().equalsIgnoreCase(role.getRange()))
                      {
                        hasDeletes = true;
                        sparqlStmts.append(String.format("tpl:{0} p8:hasRoleFillerType {1} .", existingRole.getId(),
                            existingRole.getRange()));
                      }
                    }
                    // endregion
                  }
                  else
                  {
                    // region Insert New Role

                    String roleLabel = role.getNames().get(0).getValue().split("@")[0];
                    String roleID = null;
                    generatedId = null;
                    String genName = null;

                    if (role.getNames().get(0).getLang() == null)
                      language = "@" + defaultLanguage;
                    else
                      language = "@" + role.getNames().get(0).getLang();

                    genName = "Role Qualification " + roleLabel;
                    if (role.getId() == null)
                    {
                      if (_useExampleRegistryBase)
                        generatedId = createIdsAdiId(_settings.get("ExampleRegistryBase"), genName);
                      else
                        generatedId = createIdsAdiId(_settings.get("TemplateRegistryBase"), genName);

                      roleID = getIdFromURI(generatedId);
                    }
                    else
                    {
                      roleID = getIdFromURI(role.getId());
                    }
                    // sparqlAdd.append(String.format("tpl:{0} rdf:type owl:Class .",
                    // roleID));
                    sparqlAdd.append(String.format("tpl:{0} rdfs:label \"{1}{2}\"^^xsd:string .", roleID, roleLabel,
                        language));
                    sparqlAdd.append(String.format("tpl:{0} p8:valRoleIndex {1} .", roleID, index));
                    sparqlAdd.append(String.format("tpl:{0} p8:hasTemplate tpl:{1} .", roleID, identifier));

                    if (role.getRange() != null)
                    {
                      qn = _nsmap.reduceToQName(role.getRange(), qName);
                      if(qn)
                      sparqlAdd.append(String.format("tpl:{0} p8:hasRoleFillerType {1} .", roleID, qName));
                    }

                    // foreach (PropertyRestriction
                    // restriction in role.restrictions)
                    // {
                    //
                    // }
                    sparqlAdd.append(String.format("tpl:{0} p8:hasRole rdl:{1} .", identifier, roleID));
                    // endregion
                  }

                  index++;
                }
              }

              sparqlDelete.append(prefix);
              sparqlDelete.append(deleteWhere);
              sparqlDelete.append(sparqlStmts);
              sparqlDelete.append(" }; ");
            }
            // endregion

            // region Form Insert SPARQL
            if (hasDeletes)
            {
              sparqlAdd.append("}");
            }
            else
            {
              String label = null;
              String labelSparql = null;
              // form labels
              for (Name name : newTemplateQualification.getNames())
              {
                label = name.getValue().split("@")[0];

                if (name.getLang() == null)
                  language = "@" + defaultLanguage;
                else
                  language = "@" + name.getLang();
              }

              sparqlAdd.append(String
                  .format("tpl:{0} rdfs:label \"{1}{2}\"^^xsd:string .", identifier, label, language));
              sparqlAdd.append(String.format("tpl:{0} p8:valNumberOfRoles {1} .", identifier, newTemplateQualification
                  .getRoleQualifications().size()));

              // /TODO: Template Description R# should go here
              // instead
              sparqlAdd.append(String.format("tpl:{0} rdf:type p8:TemplateDescription .", identifier));

              // /TODO: BIG QUESTION AROUND THIS
              sparqlAdd.append(String.format("tpl:{0} rdf:hasTemplate p8:{1} .", identifier,
                  label.replaceFirst(label.substring(0, label.lastIndexOf("_") + 1), "")));

              for (Specialization spec : newTemplateQualification.getSpecializations())
              {
                // sparqlStr = new StringBuilder();
                String specialization = spec.getReference();
                sparqlAdd.append(prefix);
                sparqlAdd.append(insertData);

                // /TODO: Generate an id for template
                // specialization??

                // sparqlAdd.append(String.format("tpl:{0} rdfs:subClassOf {1} .",
                // identifier, specialization));
                sparqlAdd.append(String.format("tpl:{0} rdf:type p8:TemplateSpecialization .", specialization));
                sparqlAdd.append(String.format("tpl:{0} rdfs:label \"{0}{1}\"^^xds:string .", specialization, spec
                    .getLabel().split("@")[0], language));
                sparqlAdd.append(String.format("tpl:{0} p8:hasSuperTemplate tpl:{1} .", specialization, identifier));
                sparqlAdd.append(String.format("tpl:{0} p8:hasSubTemplate tpl:{1} .", identifier, specialization));
              }

              for (RoleQualification role : newTemplateQualification.getRoleQualifications())
              {
                String roleLabel = role.getNames().get(0).getValue().split("@")[0];
                String roleID = null;
                generatedId = null;
                String genName = null;
                String range = role.getRange();

                if (role.getNames().get(0).getLang() == null)
                  language = "@" + defaultLanguage;
                else
                  language = "@" + role.getNames().get(0).getLang();

                genName = "Role Qualification " + roleLabel;
                if (role.getId() == null)
                {
                  if (_useExampleRegistryBase)
                    generatedId = createIdsAdiId(_settings.get("ExampleRegistryBase"), genName);
                  else
                    generatedId = createIdsAdiId(_settings.get("TemplateRegistryBase"), genName);

                  roleID = getIdFromURI(generatedId);
                }
                else
                {
                  roleID = getIdFromURI(role.getId());
                }

                // /TODO: p8:TemplateRoleDecription has to be
                // replaced with R#
                sparqlAdd.append(String.format("tpl:{0} rdf:type p8:TemplateRoleDescription .", roleID));
                sparqlAdd.append(String.format("tpl:{0} rdfs:label \"{1}{2}\"^^xsd:string .", roleID, roleLabel,
                    language));
                sparqlAdd.append(String.format("tpl:{0} p8:valRoleIndex {1} .", roleID, ++roleCount));
                sparqlAdd.append(String.format("tpl:{0} p8:hasTemplate tpl:{1} .", roleID, identifier));
                sparqlAdd.append(String.format("tpl:{0} p8:hasRole tpl:{1} .", identifier, roleID));

                if (role.getRange() != null)
                {
                  qn = _nsmap.reduceToQName(role.getRange(), qName);
                  if(qn)
                  sparqlAdd.append(String.format("tpl:{0} p8:hasRoleFillerType {1} .", roleID, qName));
                }
              }
              sparqlAdd.append("}");
            }
            // endregion

            // add prefix first
            sparqlBuilder.append(prefix);
            sparqlBuilder.append(sparqlDelete);
            sparqlBuilder.append(sparqlAdd);

            String sparql = sparqlBuilder.toString();
            Response postResponse = postToRepository(repository, sparql);
            // response.append(postResponse);
          }
        }
        // endregion
      }
    }
    catch (Exception ex)
    {
      String errMsg = "Error in PostTemplate: " + ex;
      Status status = new Status();
      logger.error("Error in PostTemplate: " + ex);
      response.setLevel(Level.ERROR);
      status.getMessages().getItems().add(errMsg);
    }
    return response;

  }

  private int getIndexFromName(String name)
  {
    int index = 0;
    try
    {
      for (Repository repository : _repositories)
      {
        if (repository.getName().equalsIgnoreCase(name))
        {
          return index;
        }
        index++;
      }
      index = 0;
      for (Repository repository : _repositories)
      {
        if (!repository.isIsReadOnly())
        {
          return index;
        }
        index++;
      }
    }
    catch (Exception ex)
    {
      logger.error(ex);

    }
    return index;
  }

  private Response postToRepository(Repository repository, String sparql)
  {
    Response response = new Response();
    Status status = null;

      try { 

    	  String uri = repository.getUpdateUri().toString();
    	  NetworkCredentials credentials = new NetworkCredentials();
          if (repository.isIsReadOnly() == false) {
        	  HttpClient sparqlClient = new HttpClient(uri);
        	  sparqlClient.setNetworkCredentials(credentials);
        	  sparqlClient.postSparql(String.class, "", sparql, "");
        	  status = new Status();
        	  
        	  
          }else{
        	  
          }
      }
      catch(Exception ex){
    	  logger.error(ex);
    	  return response;
      }
     
    return response;
  }

  public Response postClass(Qmxf qmxf)
  {
	  Response response = new Response();
      response.setLevel(Level.SUCCESS);
      boolean qn = false;
      ReferenceObject qName = null;
      
      
      try
      {
          Repository repository = getRepository(qmxf.getTargetRepository());

          if (repository == null || repository.isIsReadOnly())
          {
              Status status = new Status();
              response.setLevel(Level.ERROR);
              
              if (repository == null)
                  status.getMessages().getItems().add("Repository not found!");
              else
                  status.getMessages().getItems().add("Repository [" + qmxf.getTargetRepository() + "] is read-only!");

              //_response.Append(status);
          }
          else
          {
              String registry = _useExampleRegistryBase ? _settings.get("ExampleRegistryBase") : _settings.get("ClassRegistryBase");
              StringBuilder sparqlDelete = new StringBuilder();

              for (ClassDefinition clsDef : qmxf.getClassDefinitions())
              {

                  String language = null;
                  List<String> names = new ArrayList<String>();
                  StringBuilder sparqlAdd = new StringBuilder();
                  sparqlAdd.append(insertData);
                  boolean hasDeletes = false;
                  int classCount = 0;
                  String clsId = getIdFromURI(clsDef.getId());
                  Qmxf existingQmxf = new Qmxf();

                  if (clsId!=null)
                  {
                      existingQmxf = getClass(clsId, repository);
                  }

                  // delete class
                  if (existingQmxf.getClassDefinitions().size() > 0)
                  {
                      StringBuilder sparqlStmts = new StringBuilder();


                      for (ClassDefinition existingClsDef : existingQmxf.getClassDefinitions())
                      {
                          for (Name clsName : clsDef.getNames())
                          {
                              //QMXFName existingName = existingClsDef.name.Find(n => n.lang == clsName.lang);
                              Name existingName = new Name();
								for (Name tempName : existingClsDef.getNames()) {
									if (clsName.getLang().equalsIgnoreCase(tempName.getLang())) {
										existingName = tempName;
									}
								}
								
								
                              if (existingName != null)
                              {
                                  if (!existingName.getValue().equalsIgnoreCase(clsName.getValue()))
                                  {
                                      hasDeletes = true;
                                      sparqlStmts.append(String.format(" rdl:{0} rdfs:label \"{1}{2}\"^^xsd:string . ", clsId, existingName.getValue(), clsName.getLang()));
                                      sparqlAdd.append(String.format(" rdl:{0}  rdfs:label \"{1}{2}\"^^xsd:string .", clsId, clsName.getValue(), clsName.getLang()));
                                  }
                              }

                              for (Description description : clsDef.getDescriptions())
                              {
                                  //Description existingDescription = existingClsDef.description.Find(d => d.lang == description.lang);
                              	Description existingDescription = new Description();
  								for (Description tempDesc : existingClsDef.getDescriptions()) {
  									if (description.getLang().equalsIgnoreCase(tempDesc.getLang())) {
  										existingDescription = tempDesc;
  									}
  								}
                                  if (existingDescription != null)
                                  {
                                      if (!existingDescription.getValue().equalsIgnoreCase(description.getValue()))
                                      {
                                          hasDeletes = true;
                                          sparqlStmts.append(String.format(" rdl:{0} rdfs:comment \"{1}{2}\"^^xsd:string . ", clsId, existingDescription.getValue(), description.getLang()));
                                          sparqlAdd.append(String.format(" rdl:{0} rdfs:comment \"{1}{2}\"^^xsd:string .", clsId, description.getValue(), description.getLang()));
                                      }
                                  }
                              }

                              // delete specialization
                              for (Specialization spec : clsDef.getSpecializations())
                              {

                                  //Specialization existingSpec = existingClsDef.specialization.Find(s => s.reference == spec.reference);
                                  Specialization existingSpec = new Specialization();
  								for (Specialization tempSpec : existingClsDef.getSpecializations()) {
  									if (spec.getReference().equalsIgnoreCase(tempSpec.getReference())) {
  										existingSpec = tempSpec;
  									}
  								}
                                  if (existingSpec != null && existingSpec.getReference() != null)
                                  {
                                      if (!existingSpec.getReference().equalsIgnoreCase(spec.getReference()))
                                      {
                                          hasDeletes = true;
                                          qn = _nsmap.reduceToQName(existingSpec.getReference(), qName);
                                          sparqlStmts.append(String.format("  ?a dm:hasSubclass {0} . ", qName));
                                          qn = _nsmap.reduceToQName(spec.getReference(), qName);
                                          if (qn)
                                              sparqlAdd.append(String.format(" ?a rdfs:subClassOf {0} .", qName));
                                      }
                                  }
                              }

                              // delete classification
                              for (Classification clsif : clsDef.getClassifications())
                              {
                                  //Classification existingClasif = existingClsDef.classification.Find(c => c.reference == clsif.reference);
                              	Classification existingClasif = new Classification();
  								for (Classification tempClasif : existingClsDef.getClassifications()) {
  									if (clsif.getReference().equalsIgnoreCase(tempClasif.getReference())) {
  										existingClasif = tempClasif;
  									}
  								}

                                  if (existingClasif != null && existingClasif.getReference() != null)
                                  {
                                      if (!existingClasif.getReference().equalsIgnoreCase(clsif.getReference()))
                                      {
                                          hasDeletes = true;
                                          qn = _nsmap.reduceToQName(existingClasif.getReference(), qName);
                                          if (qn)
                                          {
                                              sparqlStmts.append(String.format(" ?a dm:hasClassified {0} .", qName));
                                              sparqlStmts.append(String.format(" ?a dm:hasClassifier {0} .", qName));
                                          }
                                          qn = _nsmap.reduceToQName(clsif.getReference(), qName);
                                          if (qn)
                                          {
                                              sparqlAdd.append(String.format(" ?a dm:hasClassified {0} .", qName));
                                              sparqlAdd.append(String.format(" ?a dm:hasClassifier {0} .", qName));
                                          }

                                      }
                                  }
                              }
                          }
                      }
                      if (sparqlStmts.length() > 0)
                      {
                          sparqlDelete.append(deleteWhere);
                          sparqlDelete.append(sparqlStmts);
                          sparqlDelete.append(" }; ");
                      }
                  }

                  // add class
                  if (hasDeletes)
                  {
                      sparqlAdd.append("}");
                  }
                  else
                      for (Name clsName : clsDef.getNames())
                      {
                          String clsLabel = clsName.getValue().split("@")[0];

                          if (clsName.getLang()==null)
                              language = "@" + defaultLanguage;
                          else
                              language = "@" + clsName.getLang();

                          if (clsId==null)
                          {
                              String newClsName = "Class definition " + clsLabel;

                              clsId = createIdsAdiId(registry, newClsName);
                              clsId = getIdFromURI(clsId);
                          }

                          // append label
                          sparqlAdd.append(String.format(" rdl:{0} rdf:type owl:Class .", clsId));
                          sparqlAdd.append(String.format(" rdl:{0} rdfs:label \"{1}{2}\"^^xsd:string .", clsId, clsLabel, language));

                          // append entity type
                          if (clsDef.getEntityType()!= null && clsDef.getEntityType().getReference()!=null)
                          {
                              qn = _nsmap.reduceToQName(clsDef.getEntityType().getReference(), qName);

                              if (qn)
                                  sparqlAdd.append(String.format(" rdl:{0} rdf:type {1} .", clsId, qName));

                          }

                          // append description
                          for (Description desc : clsDef.getDescriptions())
                          {
                              if (desc.getValue()!=null)
                              {
                                  if (desc.getLang()==null)
                                      language = "@" + defaultLanguage;
                                  else
                                      language = "@" + desc.getLang();
                                  String description = desc.getValue().split("@")[0];
                                  sparqlAdd.append(String.format(" rdl:{0} rdfs:comment \"{1}{2}\"^^xsd:string . ", clsId, description, language));
                              }
                          }

                          // append specialization
                          for (Specialization spec : clsDef.getSpecializations())
                          {
                              if (spec.getReference()!=null)
                              {
                                  qn = _nsmap.reduceToQName(spec.getReference(), qName);
                                  if (qn)
                                      sparqlAdd.append(String.format(" rdl:{0} rdfs:subClassOf {1} .", clsId, qName));
                              }
                          }

                          classCount = clsDef.getClassifications().size();

                          // append classification
                          for (Classification clsif : clsDef.getClassifications())
                          {
                              if (clsif.getReference()!=null)
                              {
                                  qn = _nsmap.reduceToQName(clsif.getReference(), qName);
                                  if(qn){
	                                    if (repository.getRepositoryType()== RepositoryType.PART_8)
	                                    {
	                                        sparqlAdd.append(String.format("rdl:{0} rdf:type {1} .", clsId, qName));
	
	                                    }
	                                    else {
	                                        sparqlAdd.append(String.format("rdl:{0} dm:hasClassifier {1} .", clsId, qName));
	                                        sparqlAdd.append(String.format("{0} dm:hasClassified rdl:{1} .", qName, clsId));
	                                    }
                                  }
                              }
                          }

                          sparqlAdd.append("}");
                      }
                  sparqlBuilder.append(prefix);
                  sparqlBuilder.append(sparqlDelete);
                  sparqlBuilder.append(sparqlAdd);

                  String sparql = sparqlBuilder.toString();
                  Response postResponse = postToRepository(repository, sparql);
                  //response.append(postResponse);
              }
          }
      }
      catch (Exception ex)
      {
          String errMsg = "Error in PostClass: " + ex;
          Status status = new Status();

          response.setLevel(Level.ERROR);
          status.getMessages().getItems().add(errMsg);
          //response.Append(status);

          //_logger.Error(errMsg);
      }

      return response;
  }

  public List<Repository> getRepositories() throws Exception
  {
    List<Repository> repositoryList = new ArrayList<Repository>();
    try
    {
      Federation federation = getFederation();
      for (Repository repo : federation.getRepositories().getItems())
      {
        repositoryList.add(repo);
      }
    }
    catch (Exception ex)
    {
      logger.error(ex);
      return repositoryList;
    }
    return repositoryList;

  }

  private Repository getRepository(String name)
  {
    Repository repository = null;
    for (Repository tempRepo : _repositories)
    {
      if (tempRepo.getName().equalsIgnoreCase(name))
      {
        return repository;
      }
    }
    return repository;
  }

  public org.iringtools.refdata.response.Response getClassTemplates(String id) throws Exception
  {
    org.iringtools.refdata.response.Response response = new org.iringtools.refdata.response.Response();
    Entities entities = new Entities();
    response.setEntities(entities);
    List<Entity> entityList = new ArrayList<Entity>();
    entities.setItems(entityList);

    String[] names = null;
    String language = "";
    try
    {
      String sparqlGetClassTemplates = "";
      String sparqlGetRelatedTemplates = "";
      String relativeUri = "";
      Query queryGetClassTemplates = getQuery("GetClassTemplates");
      QueryBindings queryBindingsGetClassTemplates = queryGetClassTemplates.getBindings();

      sparqlGetClassTemplates = readSparql(queryGetClassTemplates.getFileName());
      sparqlGetClassTemplates = sparqlGetClassTemplates.replace("param1", id);

      Query queryGetRelatedTemplates = getQuery("GetRelatedTemplates");
      QueryBindings queryBindingsGetRelatedTemplates = queryGetRelatedTemplates.getBindings();

      sparqlGetRelatedTemplates = readSparql(queryGetRelatedTemplates.getFileName());
      sparqlGetRelatedTemplates = sparqlGetRelatedTemplates.replace("param1", id);

      for (Repository repository : _repositories)
      {
        if (repository.getRepositoryType().equals(RepositoryType.PART_8))
        {
          Results sparqlResults = queryFromRepository(repository, sparqlGetRelatedTemplates);

          List<Hashtable<String, String>> results = bindQueryResults(queryBindingsGetRelatedTemplates, sparqlResults);

          for (Hashtable<String, String> result : results)
          {

            names = result.get("label").split("@", -1);
            if (names.length == 1)
            {
              language = defaultLanguage;
            }
            else
            {
              language = names[names.length - 1];
            }

            Entity tempVar = new Entity();
            tempVar.setUri(result.get("uri"));
            tempVar.setLabel(names[0]);
            tempVar.setLang(language);
            tempVar.setRepository(repository.getName());
            Entity resultEntity = tempVar;

            // Utility.SearchAndInsert(queryResult, resultEntity,
            // Entity.sortAscending());
            entityList.add(resultEntity);
          }
        }
        else
        {
          Results sparqlResults = queryFromRepository(repository, sparqlGetClassTemplates);

          List<Hashtable<String, String>> results = bindQueryResults(queryBindingsGetClassTemplates, sparqlResults);

          for (Hashtable<String, String> result : results)
          {
            names = result.get("label").split("@", -1);
            if (names.length == 1)
            {
              language = defaultLanguage;
            }
            else
            {
              language = names[names.length - 1];
            }

            Entity tempVar2 = new Entity();
            tempVar2.setUri(result.get("uri"));
            tempVar2.setLabel(names[0]);
            tempVar2.setLang(language);
            tempVar2.setRepository(repository.getName());
            Entity resultEntity = tempVar2;

            entityList.add(resultEntity);
          }
        }
      }
      Collections.sort(entityList, new EntityComparator());
    }
    catch (RuntimeException e)
    {
      logger.error("Error in GetClassTemplates: " + e);
      return response;
    }
    return response;
  }

  public org.iringtools.refdata.response.Response search(String query) throws Exception
  {
    try
    {
      return searchPage(query, 0, 0);
    }
    catch (RuntimeException ex)
    {
      logger.error("Error in Search: " + ex);
      return new org.iringtools.refdata.response.Response();
    }
  }

  public org.iringtools.refdata.response.Response searchPage(String query, int i, int j) throws Exception
  {
    org.iringtools.refdata.response.Response response = new org.iringtools.refdata.response.Response();
    Entities entities = new Entities();
    response.setEntities(entities);
    List<Entity> entityList = new ArrayList<Entity>();
    entities.setItems(entityList);
    int counter = 0;

    try
    {
      String sparql = "";
      String[] names = null;
      String language = "";
      Entity resultEntity = null;
      String key = null;

      // TODO Check the search History for Optimization
      // if (_searchHistory.containsKey(query)) {
      // entityList.addAll(_searchHistory.keySet().ceilingEntry(query).getValue());
      // } else {
      Map<String, Entity> resultEntities = new TreeMap<String, Entity>();

      Query queryContainsSearch = getQuery("ContainsSearch");
      QueryBindings queryBindings = queryContainsSearch.getBindings();

      sparql = readSparql(queryContainsSearch.getFileName());
      sparql = sparql.replace("param1", query);

      for (Repository repository : _repositories)
      {
        Results sparqlResults = queryFromRepository(repository, sparql);

        List<Hashtable<String, String>> results = bindQueryResults(queryBindings, sparqlResults);
        for (Hashtable<String, String> result : results)
        {
          names = result.get("label").split("@", -1);
          if (names.length == 1)
          {
            language = defaultLanguage;
          }
          else
          {
            language = names[names.length - 1];
          }

          Entity tempVar = new Entity();
          tempVar.setUri(result.get("uri"));
          tempVar.setLabel(names[0]);
          tempVar.setLang(language);
          tempVar.setRepository(repository.getName());
          resultEntity = tempVar;

          key = resultEntity.getLabel();

          if (resultEntities.containsKey(key))
          {
            key += ++counter;
          }

          resultEntities.put(key, resultEntity);
        }
        results.clear();
      }

      _searchHistory.put(key, resultEntity);
      entityList.addAll(resultEntities.values());
      Collections.sort(entityList, new EntityComparator());
      response.setTotal(entityList.size());
      // }

      if (j > 0)
      {
        response = getRequestedPage(response, i, j);
      }
      return response;
    }
    catch (RuntimeException e)
    {
      logger.error("Error in SearchPage: " + e);
      return response;
    }
  }

  private org.iringtools.refdata.response.Response getRequestedPage(org.iringtools.refdata.response.Response entities,
      int startIdx, int pageSize)
  {
    org.iringtools.refdata.response.Response page = new org.iringtools.refdata.response.Response();
    try
    {
      page.setTotal(entities.getEntities().getItems().size());
      Entities ent = new Entities();
      page.setEntities(ent);
      for (int i = startIdx; i < startIdx + pageSize; i++)
      {
        if (entities.getEntities().getItems().size() == i)
        {
          break;
        }
        Entity entity = entities.getEntities().getItems().get(i);
        ent.getItems().add(entity);
       // page.getEntities().getItems().add(i, entity);
      }

      return page;
    }
    catch (RuntimeException ex)
    {
      logger.error(ex);
      return page;
    }
  }

  public Map<String, Entity> searchReset(String query)
  {
    return null;
  }

  public org.iringtools.refdata.response.Response getSuperClasses(String id) throws Exception
  {
    org.iringtools.refdata.response.Response response = new org.iringtools.refdata.response.Response();
    Entities entities = new Entities();
    response.setEntities(entities);
    List<Entity> entityList = new ArrayList<Entity>();
    entities.setItems(entityList);

    String[] names = null;
    try
    {
      List<Specialization> specializations = getSpecializations(id, null);

      for (Specialization specialization : specializations)
      {
        Entity tempVar = new Entity();
        String uri = specialization.getReference();
        names = specialization.getLabel().split("@");
        String label = names[0];

        if (label == null)
        {
          names = getLabel(uri).getLabel().split("@");
          label = names[0];
        }
        if (names.length == 1)
        {
          tempVar.setLang(defaultLanguage);

        }
        else if (names.length == 2)
        {
          tempVar.setLang(names[names.length - 1]);
        }
        tempVar.setUri(uri);
        tempVar.setLabel(label);
        Entity resultEntity = tempVar;
        entityList.add(resultEntity);
      }
      response.setTotal(entityList.size());
      Collections.sort(entityList, new EntityComparator());
    }
    catch (RuntimeException e)
    {
      logger.error("Error in GetSuperClasses: " + e);
      return response;
    }
    return response;
  }

  public org.iringtools.refdata.response.Response getSubClasses(String id) throws Exception
  {
    org.iringtools.refdata.response.Response response = new org.iringtools.refdata.response.Response();
    Entities entities = new Entities();
    response.setEntities(entities);
    List<Entity> entityList = new ArrayList<Entity>();
    entities.setItems(entityList);

    try
    {
      String sparql = "";
      String sparqlPart8 = "";
      String relativeUri = "";
      String language = "";
      String[] names = null;

      Query queryGetSubClasses = getQuery("GetSubClasses");
      QueryBindings queryBindings = queryGetSubClasses.getBindings();

      sparql = readSparql(queryGetSubClasses.getFileName());
      sparql = sparql.replace("param1", id);

      Query queryGetSubClassOfInverse = getQuery("GetSubClassOfInverse");
      QueryBindings queryBindingsPart8 = queryGetSubClassOfInverse.getBindings();

      sparqlPart8 = readSparql(queryGetSubClassOfInverse.getFileName());
      sparqlPart8 = sparqlPart8.replace("param1", id);

      for (Repository repository : _repositories)
      {
        if (repository.getRepositoryType().equals(RepositoryType.PART_8))
        {
          Results sparqlResults = queryFromRepository(repository, sparqlPart8);

          List<Hashtable<String, String>> results = bindQueryResults(queryBindingsPart8, sparqlResults);

          for (Hashtable<String, String> result : results)
          {
            names = result.get("label").split("@", -1);

            if (names.length == 1)
            {
              language = defaultLanguage;
            }
            else
            {
              language = names[names.length - 1];
            }
            Entity tempVar = new Entity();
            tempVar.setUri(result.get("uri"));
            tempVar.setLabel(names[0]);
            tempVar.setLang(language);
            // Entity resultEntity = tempVar;
            entityList.add(tempVar);
          }
        }
        else
        {
          Results sparqlResults = queryFromRepository(repository, sparql);
          List<Hashtable<String, String>> results = bindQueryResults(queryBindings, sparqlResults);
          for (Hashtable<String, String> result : results)
          {
            names = result.get("label").split("@", -1);
            if (names.length == 1)
            {
              language = defaultLanguage;
            }
            else
            {
              language = names[names.length - 1];
            }
            Entity tempVar2 = new Entity();
            tempVar2.setUri(result.get("uri"));
            tempVar2.setLabel(names[0]);
            tempVar2.setLang(language);
            Entity resultEntity = tempVar2;
            entityList.add(resultEntity);
          }
        }
      }
      response.setTotal(entityList.size());
      Collections.sort(entityList, new EntityComparator());
    }
    catch (RuntimeException e)
    {
      logger.error("Error in GetSubClasses: " + e);
      return response;
    }
    return response;
  }

  public org.iringtools.refdata.response.Response getAllSuperClasses(String id) throws Exception
  {
    org.iringtools.refdata.response.Response response = new org.iringtools.refdata.response.Response();
    Entities entities = new Entities();
    response.setEntities(entities);
    List<Entity> entityList = new ArrayList<Entity>();
    entities.setItems(entityList);
    response = getAllSuperClasses(id, response);
    response.setTotal(response.getEntities().getItems().size());
    return response;
  }

  public org.iringtools.refdata.response.Response getAllSuperClasses(String id,
      org.iringtools.refdata.response.Response response) throws Exception
  {
    Entities entities = response.getEntities();
    List<Entity> entityList = entities.getItems();
    String[] names = null;
    try
    {
      List<Specialization> specializations = getSpecializations(id, null);
      // base case
      if (specializations.isEmpty())
      {
        return response;
      }

      for (Specialization specialization : specializations)
      {
        String uri = specialization.getReference();
        String label = specialization.getLabel();
        String language = "";

        if (label == null)
        {
          names = getLabel(uri).getLabel().split("[@]",-1);
          label = names[0];
          if (names.length == 1)
          {
            language = defaultLanguage;
          }
          else
          {
            language = names[names.length - 1];
          }
        }
        else
        {
          names = label.split("@");
          if (names.length == 1)
          {
            language = defaultLanguage;
          }
          else
          {
            language = names[names.length - 1];
          }
          label = names[0];
        }
        Entity tempVar = new Entity();
        tempVar.setUri(uri);
        tempVar.setLabel(label);
        tempVar.setLang(language);
        Entity resultEntity = tempVar;

        String trimmedUri = "";
        boolean found = false;
        for (Entity entt : entities.getItems())
        {
          if (resultEntity.getUri().equals(entt.getUri()))
          {
            found = true;
            break;
          }
        }

        if (!found)
        {
          trimmedUri = uri.substring(0, 0) + uri.substring(0 + uri.lastIndexOf('#') + 1);
          entities.getItems().add(resultEntity);
          getAllSuperClasses(trimmedUri, response);
        }
      }
      Collections.sort(entityList, new EntityComparator());
    }
    catch (RuntimeException e)
    {
      logger.error("Error in GetAllSuperClasses: " + e);
      return response;
    }

    return response;
  }

  private Response queryIdGenerator(String serviceUrl) throws HttpClientException
  {
    Response result = null;
    HttpClient httpClient = null;
    try
    {
      String uri = _settings.get("idGenServiceUri");
      httpClient = new HttpClient(uri);
    }
    catch (Exception e)
    {
      System.out.println("Exception in IDGenServiceUri :" + e);
    }
    result = httpClient.get(Response.class, serviceUrl);
    return result;
  }

  private String createIdsAdiId(String uri, String comment)
  {
    String idsAdiId = "";
    Response responseText = null;
    try
    {
      String serviceUrl = "/acquireId/param?uri=" + uri + "&comment=" + comment;
      responseText = queryIdGenerator(serviceUrl);
      Messages messages = responseText.getMessages();
      List<String> messageList = messages.getItems();
      if (messageList != null)
      {
        idsAdiId = messageList.get(0);
      }
    }
    catch (Exception e)
    {
      logger.error("Error in createIdsAdiId: " + e);
      return idsAdiId;
    }
    return idsAdiId;
  }

  public Entity getLabel(String uri) throws Exception
  {
      Entity labelEntity = new Entity();

      try
      {
          String sparql = "";
          String[] names;
          Query queryContainsSearch = getQuery("GetLabel");
          QueryBindings queryBindings = queryContainsSearch.getBindings();
          sparql = readSparql(queryContainsSearch.getFileName());
          sparql = sparql.replace("param1", uri);

          for (Repository repository : _repositories)
          {
              
              Results sparqlResults = queryFromRepository(repository, sparql);
              List<Hashtable<String, String>> results = bindQueryResults(queryBindings, sparqlResults);
              for (Hashtable<String, String> result : results)
              {
                  if (result.containsKey("label"))
                  {
                      names = result.get("label").split("@");
                      if (names.length == 1)
                          labelEntity.setLang(defaultLanguage);
                      else
                          labelEntity.setLang(names[names.length - 1]);

                      labelEntity.setLabel(names[0]);
                      labelEntity.setRepository(repository.getName());
                      labelEntity.setUri(repository.getUri());
                      break;
                  }
              }
          }

          return labelEntity;
      }
      catch (Exception e)
      {
    	  logger.error("Error in GetClass: " + e);
    	  return labelEntity;
      }
  }

  private List<RoleDefinition> getRoleDefinition(String id) throws Exception, HttpClientException
  {

    List<RoleDefinition> roleDefinitions = new ArrayList<RoleDefinition>();
    try
    {
      String sparql = "";
      String sparqlQuery = "";
      String[] names = null;

      Description description = new Description();
      org.ids_adi.ns.qxf.model.Status status = new org.ids_adi.ns.qxf.model.Status();

      List<Entity> resultEntities = new ArrayList<Entity>();

      for (Repository repository : _repositories)
      {
        switch (repository.getRepositoryType())
        {
        case CAMELOT:
        case RDS_WIP:
          sparqlQuery = "GetRoles";
          break;
        case PART_8:
          sparqlQuery = "GetPart8Roles";
          break;
        }
        Query queryContainsSearch = getQuery(sparqlQuery);
        QueryBindings queryBindings = queryContainsSearch.getBindings();

        sparql = readSparql(queryContainsSearch.getFileName());
        sparql = sparql.replace("param1", id);
        Results sparqlResults = queryFromRepository(repository, sparql);

        List<Hashtable<String, String>> results = bindQueryResults(queryBindings, sparqlResults);

        for (Hashtable<String, String> result : results)
        {
          RoleDefinition roleDefinition = new RoleDefinition();
          Name name = new Name();

          if (result.containsKey("label"))
          {
            names = result.get("label").split("@", -1);
            name.setValue(names[0]);
            if (names.length == 1)
            {
              name.setLang(defaultLanguage);
            }
            else
            {
              name.setLang(names[names.length - 1]);
            }
          }
          if (result.containsKey("role"))
          {
            roleDefinition.setId(result.get("role"));
          }
          if (result.containsKey("comment"))
          {
            names = result.get("comment").split("@", -1);
            description.setValue(names[0]);
            if (names.length == 1)
            {
              description.setLang(defaultLanguage);
            }
            else
            {
              description.setLang(names[names.length - 1]);
            }
          }
          if (result.containsKey("index"))
          {
            description.setValue(result.get("index").toString());
          }
          if (result.containsKey("type"))
          {
            roleDefinition.setRange(result.get("type"));
          }
          roleDefinition.getNames().add(name);
          roleDefinition.getDescriptions().add(description);
          roleDefinitions.add(roleDefinition);
        }
      }

      return roleDefinitions;
    }
    catch (RuntimeException e)
    {
      logger.error("Error in GetRoleDefinition: " + e);
      return roleDefinitions;
    }
  }

  private List<Hashtable<String, String>> bindQueryResults(QueryBindings queryBindings, Results sparqlResults)
  {
    String sBinding = "";
    String qBinding = "";
    List<Hashtable<String, String>> results = new ArrayList<Hashtable<String, String>>();
    try
    {

      for (Result sparqlResult : sparqlResults.getResults())
      {
        Hashtable<String, String> result = new Hashtable<String, String>();
        String sortKey = "";
        for (Binding sparqlBinding : sparqlResult.getBindings())
        {
          sBinding = sparqlBinding.getName();
          for (QueryBinding queryBinding : queryBindings.getItems())
          {
            qBinding = queryBinding.getName();
            if (sBinding.equals(qBinding))
            {
              String bindingKey = qBinding;
              String bindingValue = "";
              String dataType = "";
              if (queryBinding.getType() == SPARQLBindingType.URI)
              {
                bindingValue = sparqlBinding.getUri();
              }
              else if (queryBinding.getType() == SPARQLBindingType.LITERAL)
              {
                bindingValue = sparqlBinding.getLiteral().getContent();
                dataType = sparqlBinding.getLiteral().getDatatype();
                sortKey = bindingValue;
              }
              if (result.containsKey(bindingKey))
              {
                bindingKey = makeUniqueKey(result, bindingKey);
              }
              result.put(bindingKey, bindingValue);

              if (dataType != null && !dataType.isEmpty())
              {
                result.put(bindingKey + "_dataType", bindingValue);
              }
            }
          }
        }
        results.add(result);
      }
      return results;
    }
    catch (RuntimeException ex)
    {
      logger.error(ex);
      return results;
    }
  }

  private String makeUniqueKey(Hashtable<String, String> hashtable, String duplicateKey)
  {

    String newKey = "";
    try
    {

      for (int i = 2; i < Integer.MAX_VALUE; i++)
      {
        String postFix = " (" + (new Integer(i)).toString() + ")";
        if (!hashtable.containsKey(duplicateKey + postFix))
        {
          newKey += postFix;
          break;
        }
      }
      return newKey;
    }
    catch (RuntimeException ex)
    {
      logger.error(ex);
      return newKey;
    }
  }

  private Results queryFromRepository(Repository repository, String sparql) throws HttpClientException,
      UnsupportedEncodingException, JAXBException
  {
    Results results = new Results();

    try
    {
      // TODO need to look at credentials
      NetworkCredentials credentials = new NetworkCredentials();
      HttpClient sparqlClient = new HttpClient(repository.getUri());
      sparqlClient.setNetworkCredentials(credentials);
      Sparql sparqlResults = sparqlClient.postSparql(Sparql.class, "", sparql, "");

      results = sparqlResults.getResults();
    }
    catch (RuntimeException ex)
    {
      logger.error(ex);
      return results;
    }
    
    return results;
  }

  private Query getQuery(String queryName)
  {
    Query query = null;
    List<QueryItem> items = _queries.getItems();
    for (QueryItem qry : items)
    {
      if (qry.getKey().equals(queryName))
      {
        query = qry.getQuery();
        break;
      }
    }
    return query;
  }

}