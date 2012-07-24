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
import org.iringtools.refdata.federation.IdGenerator;
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
import org.iringtools.utility.HttpUtils;
import org.iringtools.utility.IOUtils;
import org.iringtools.utility.JaxbUtils;
import org.iringtools.utility.NamespaceMapper;
import org.w3._2005.sparql.results.Binding;
import org.w3._2005.sparql.results.Result;
import org.w3._2005.sparql.results.Results;
import org.w3._2005.sparql.results.Sparql;

import com.hp.hpl.jena.rdf.model.Literal;
import com.hp.hpl.jena.rdf.model.Model;
import com.hp.hpl.jena.rdf.model.ModelFactory;
import com.hp.hpl.jena.rdf.model.Property;
import com.hp.hpl.jena.rdf.model.Resource;
import com.hp.hpl.jena.rdf.model.StmtIterator;

public class RefDataProvider
{
  private Map<String, Object> _settings;
  private List<Repository> _repositories = null;
  private Queries _queries = null;
  private String defaultLanguage = "en";
  private NamespaceMapper _nsmap = null;
  private Map<String, Entity> _searchHistory = new TreeMap<String, Entity>();
  private boolean _useExampleRegistryBase = false;
  private final String insertData = "INSERT DATA {";
  private final String deleteData = "DELETE DATA {";
  //private final String deleteWhere = "DELETE WHERE {";
  //private StringBuilder prefix = new StringBuilder();
  private StringBuilder sparqlBuilder = new StringBuilder();
  private static final Logger logger = Logger.getLogger(RefDataProvider.class);
  //private final String rdfssubClassOf = "rdfs:subClassOf";
  private final String rdfType = "rdf:type";

  //private StringBuilder sparqlStr = new StringBuilder();

  // Resource dsubj ,isubj, subj;
  // Property dpred, ipred, pred;
  // RDFNode dobj, iobj, obj;

  public RefDataProvider(Map<String, Object> settings)
  {
    try
    {
      _settings = settings;
      _repositories = getRepositories();
      _queries = getQueries();
      _nsmap = new NamespaceMapper(false);
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
    //return JaxbUtils.read(Federation.class, path);
    Federation f = JaxbUtils.read(Federation.class, path);
    System.out.println(JaxbUtils.toXml(f, true));
    return f;    
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
      for (Namespace ns : federation.getNamespacelist().getItems())
      {
        if (ns.getId() == namespace.getId())
        {
          index = federation.getNamespacelist().getItems().indexOf(ns);
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
          Integer nsID = namespace.getId();
          for (Repository repo : federation.getRepositorylist().getItems())
          {
            if (repo.getNamespaces() != null && repo.getNamespaces().getItems().contains(nsID))
              repo.getNamespaces().getItems().remove(nsID);
          }
        }

        // now remove the namespace
        federation.getNamespacelist().getItems().remove(index);
      }
      else
      {
        int sequenceId = federation.getNamespacelist().getSequenceid();
        namespace.setId(++sequenceId);
        federation.getNamespacelist().setSequenceid(sequenceId);
      }
      if (!deleteFlag)
      {
        federation.getNamespacelist().getItems().add(namespace);
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

  public Response saveIdGenerator(IdGenerator idgenerator, boolean deleteFlag) throws Exception
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
      for (IdGenerator idg : federation.getIdgeneratorlist().getItems())
      {
        if (idg.getId() == idgenerator.getId())
        {
          index = federation.getIdgeneratorlist().getItems().indexOf(idg);
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
          int nsID = idgenerator.getId();
          for (Namespace ns : federation.getNamespacelist().getItems())
          {
            if (ns.getIdgenerator() == nsID)
            {
              ns.setIdgenerator(0);
            }
          }
        }

        // now remove the namespace
        federation.getIdgeneratorlist().getItems().remove(index);
      }
      else
      {
        int sequenceId = federation.getIdgeneratorlist().getSequenceid();
        idgenerator.setId(++sequenceId);
        federation.getIdgeneratorlist().setSequenceid(sequenceId);
      }
      if (!deleteFlag)
      {
        federation.getIdgeneratorlist().getItems().add(idgenerator);
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
      for (Repository repo : federation.getRepositorylist().getItems())
      {
        if (repo.getId().equalsIgnoreCase(repository.getId()))
        {
          index = federation.getRepositorylist().getItems().indexOf(repo);
          repositoryExist = true;
          break;
        }
      }
      if (repositoryExist)
      {
        federation.getRepositorylist().getItems().remove(index);
      }
      else
      {
        int sequenceId = federation.getRepositorylist().getSequenceid();
        repository.setId(Integer.toString(++sequenceId));
        federation.getRepositorylist().setSequenceid(sequenceId);
      }
      if (!deleteFlag)
      {
        federation.getRepositorylist().getItems().add(repository);
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
	char firstChar = id.charAt(1);
	boolean isInt = Character.isDigit( firstChar );
	Entity entity = null;
	  
	if (isInt)
	{
	  entity = getLabel(_nsmap.getNamespaceUri("rdl").toString() + id);
	}
	else
	{
	  String url = _nsmap.getNamespaceUri("jordrdl").toString();
	  entity = getLabel(url + id);			  
	}
	return entity;	  
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
      QueryBindings queryBindings = null;

      List<Classification> classifications = new ArrayList<Classification>();
      List<Specialization> specializations = new ArrayList<Specialization>();
      String sparql = "";
      // String relativeUri = "";

      Query queryContainsSearch = getQuery("GetClass");
      
      Query queryContainsSearchJord = getQuery("GetClassJORD");
      
      namespaceUrl = _nsmap.getNamespaceUri("rdl").toString();
      String uri = namespaceUrl + id;

      sparql = sparql.replace("param1", uri);
      for (Repository repository : _repositories)
      {
    	if (rep != null)
            if (rep.getName() != repository.getName())
            {
              continue;
            }
    	  
    	if (repository.getRepositorytype() == RepositoryType.JORD)
    	{
    		queryBindings = queryContainsSearchJord.getBindings();
    	    sparql = readSparql(queryContainsSearchJord.getFileName());
    	}
    	else
    	{
    		
    		queryBindings = queryContainsSearch.getBindings();
    	    sparql = readSparql(queryContainsSearch.getFileName());
    	}
    	  
        ClassDefinition classDefinition = null;

        
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
            else if (repository.getRepositorytype().equals(RepositoryType.PART_8))
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
      //String sparqlJORD = "";
      //String relativeUri = "";
      Results sparqlResults = null;
      List<Hashtable<String, String>> results = null;
      String[] names = null;
      //Results res = null;

      Query queryGetSpecialization = getQuery("GetSpecialization");
      QueryBindings queryBindings = queryGetSpecialization.getBindings();
      sparql = readSparql(queryGetSpecialization.getFileName()).replace("param1", id);
      
      Query queryGetSubClassOf = getQuery("GetSubClassOf");
      QueryBindings queryBindingsPart8 = queryGetSubClassOf.getBindings();
      sparqlPart8 = readSparql(queryGetSubClassOf.getFileName()).replace("param1", id);
      
      //Query queryJord = getQuery("GetSuperclassJORD");
      //QueryBindings queryBindingsJORD = queryJord.getBindings();
     // sparqlJORD = readSparql(queryJord.getFileName()).replace("param1", id);
      
      for (Repository repository : _repositories)
      {
        if (rep != null)
        {
          if (rep.getName() != repository.getName())
          {
            continue;
          }
        }
        switch (repository.getRepositorytype())
        {
        	case PART_8:
        		sparqlResults = queryFromRepository(repository, sparqlPart8);

                results = bindQueryResults(queryBindingsPart8, sparqlResults);

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
                    names = getLabel(uri).getLabel().split("@", -1);
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
        		break;
        	case CAMELOT:
        	case RDS_WIP:
        		sparqlResults = queryFromRepository(repository, sparql);

                results = bindQueryResults(queryBindings, sparqlResults);

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
        		break;
        	case JORD:
        		sparqlResults = queryFromRepository(repository, sparqlPart8);

                results = bindQueryResults(queryBindingsPart8, sparqlResults);

                for (Hashtable<String, String> result : results)
                {
                  Specialization specialization = new Specialization();
                  String uri = "";
                  String label = "";
                  String lang = "";
                  String rdsuri = "";
                  if (result.containsKey("uri"))
                  {
                    uri = result.get("uri");
                    specialization.setReference(uri);
                  }
                  else if (result.containsKey("rdsuri"))
                  {
                	  rdsuri = result.get("rdsuri");
                      specialization.setRdsuri(rdsuri);
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
                    names = getLabel(uri).getLabel().split("@", -1);
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
        		break;
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
      //String relativeUri = "";
      Query getClassification;
      QueryBindings queryBindings;

      for (Repository repository : _repositories)
      {
        if (rep != null)
        {
          if (rep.getName() != repository.getName())
          {
            continue;
          }
        }
        switch (rep.getRepositorytype())
        {
        case CAMELOT:
        case RDS_WIP:

          getClassification = getQuery("GetClassification");
          queryBindings = getClassification.getBindings();

          sparql = readSparql(getClassification.getFileName()).replace("param1", id);
          classifications = processClassifications(rep, sparql, queryBindings);
          break;
        case JORD:
            getClassification = getQuery("GetClassificationJORD");
            queryBindings = getClassification.getBindings();

            sparql = readSparql(getClassification.getFileName()).replace("param1", id);
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

  public final org.iringtools.refdata.response.Response getClassMembers(String Id, Repository repo)
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
      QueryBindings memberBindings = null;
      
      Query getMembers = getQuery("GetMembers");
      Query getMembersJord = getQuery("GetMembersJORD");
      

      for (Repository repository : _repositories)
      {
    	  if (repo != null)
          {
            if (repo.getName() != repository.getName())
            {
              continue;
            }
          }
    	  
    	  if (repository.getRepositorytype() == RepositoryType.JORD)
      	{
      		memberBindings = getMembersJord.getBindings();
      		sparql = readSparql(getMembersJord.getFileName()).replace("param1", Id);
      	}
      	else
      	{
      		memberBindings = getMembers.getBindings();
      		sparql = readSparql(getMembers.getFileName()).replace("param1", Id);
      	}
    	  
        Results sparqlResults = queryFromRepository(repository, sparql);

        List<Hashtable<String, String>> results = bindQueryResults(memberBindings, sparqlResults);

        for (Hashtable<String, String> result : results)
        {
          names = result.get("label").split("@");
          if (names.length == 1)
          {
            language = defaultLanguage;
          }
          else
          {
            language = names[names.length - 1];
          }
          Entity resultEntity = new Entity();
          if(result.get("uri")!=null)
          {
        	  resultEntity.setUri(result.get("uri"));
          }
          else if (result.get("rds")!=null)
          {
        	  resultEntity.setRdsuri(result.get("rds"));
          }
          resultEntity.setLabel(names[0]);
          resultEntity.setLang(language);
          resultEntity.setRepository(repository.getName());

          entityList.add(resultEntity);
          // Utility.SearchAndInsert(membersResult, resultEntity, Entity.sortAscending());
        } // queryResult.Add(resultEntity);
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
      //String relativeUri = "";
      String[] names = null;

      Description description = new Description();
      org.ids_adi.ns.qxf.model.Status status = new org.ids_adi.ns.qxf.model.Status();

      //List<Entity> resultEntities = new ArrayList<Entity>();

      Query queryContainsSearch = getQuery("GetTemplate");
      QueryBindings queryBindings = queryContainsSearch.getBindings();

      sparql = readSparql(queryContainsSearch.getFileName());
      sparql = sparql.replace("param1", id);
      for (Repository repository : _repositories)
      {
        if (rep != null)
        {
          if (rep.getName() != repository.getName())
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
      //String relativeUri = "";
      String sparqlQuery = "";
      String[] names = null;

      Description description = new Description();
      //org.ids_adi.ns.qxf.model.Status status = new org.ids_adi.ns.qxf.model.Status();

      //List<Entity> resultEntities = new ArrayList<Entity>();

      switch (repository.getRepositorytype())
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
      //String relativeUri = "";
      String sparqlQuery = "";

      //List<Entity> resultEntities = new ArrayList<Entity>();

      Query getTemplateQualification = null;
      QueryBindings queryBindings = null;

      {
        for (Repository repository : _repositories)
        {
          if (rep != null)
          {
            if (rep.getName() != repository.getName())
            {
              continue;
            }
          }

          switch (repository.getRepositorytype())
          {
          case CAMELOT:
          case RDS_WIP:
            sparqlQuery = "GetTemplateQualification";
            break;
          case PART_8:
            sparqlQuery = "GetTemplateQualificationPart8";
            break;
          case JORD:
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
      //Description description = new Description();
      //org.ids_adi.ns.qxf.model.Status status = new org.ids_adi.ns.qxf.model.Status();

      for (Repository repository : _repositories)
      {
        if (rep != null)
        {
          if (rep.getName() != repository.getName())
          {
            continue;
          }
        }
        switch (rep.getRepositorytype())
        {
        case CAMELOT:
        case RDS_WIP:

          //List<Entity> rangeResultEntities = new ArrayList<Entity>();
          //List<Entity> referenceResultEntities = new ArrayList<Entity>();
          //List<Entity> valueResultEntities = new ArrayList<Entity>();

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
          //List<Entity> part8Entities = new ArrayList<Entity>();
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
            {}

            if (result.containsKey("type"))
            {}

            if (result.containsKey("label"))
            {}

            if (result.containsKey("index"))
            {}

            if (result.containsKey("role"))
            {}
            //RoleQualification roleQualification = new RoleQualification();
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
    if (uri != null && !uri.isEmpty())
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

    // TODO - to be initialized
    Model delete = ModelFactory.createDefaultModel();
    Model insert = ModelFactory.createDefaultModel();

    Response response = new Response();
    response.setLevel(Level.SUCCESS);
    boolean qn = false;
    //String qName = null;
    try
    {
      Repository repository = getRepository(qmxf.getTargetRepository());

      if (repository == null || repository.getIsreadonly())
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
        //String registry = (_useExampleRegistryBase) ? _settings.get("ExampleRegistryBase").toString() : _settings.get(
            //"ClassRegistryBase").toString();
        //StringBuilder sparqlDelete = new StringBuilder();
        // /////////////////////////////////////////////////////////////////////////////
        // / Base templates do have the following properties
        // / 1) Base class of owl:Thing
        // / 2) rdfs:type = p8:TemplateDescription
        // / 3) rdfs:label name of template
        // / 4) optional rdfs:comment
        // / 5) p8:valNumberOfRoles
        // / 6) p8:hasTemplate = tpl:{TemplateName} - this probably could be eliminated -- pointer to self
        // /////////////////////////////////////////////////////////////////////////////

        // region Template Definitions
        if (qmxf.getTemplateDefinitions().size() > 0)
        {
          for (TemplateDefinition newTemplateDefinition : qmxf.getTemplateDefinitions())
          {
            int roleCount = 0;
            String templateName = null;
            String identifier = null;
            String generatedId = null;
            int index = 1;

            if (newTemplateDefinition.getId() != null && newTemplateDefinition.getId() != "")
            {
              identifier = getIdFromURI(newTemplateDefinition.getId());
            }

            templateName = newTemplateDefinition.getNames().get(0).getValue();
            // check for exisiting template
            Qmxf existingQmxf = new Qmxf();
            if (identifier != null && identifier != "")
            {
              existingQmxf = getTemplate(identifier, TemplateType.DEFINITION.toString(), repository);
            }
            else
            {
              if (_useExampleRegistryBase)
                generatedId = createIdsAdiId(_settings.get("ExampleRegistryBase").toString(), templateName);
              else
                generatedId = createIdsAdiId(_settings.get("TemplateRegistryBase").toString(), templateName);
              identifier = getIdFromURI(generatedId);
            }

            // region Form Delete/Insert SPARQL
            if (existingQmxf.getTemplateDefinitions().size() > 0)
            {
              for (TemplateDefinition existingTemplate : existingQmxf.getTemplateDefinitions())
              {
                for (Name name : newTemplateDefinition.getNames())
                {
                  templateName = name.getValue();
                  Name existingName = new Name();
                  for (Name tempName : existingTemplate.getNames())
                  {
                    if (name.getLang().equalsIgnoreCase(tempName.getLang()))
                    {
                      existingName = tempName;
                    }
                  }
                  if (!existingName.getValue().equalsIgnoreCase(existingName.getValue()))
                  {
                    delete = GenerateName(delete, existingName, identifier, existingTemplate);
                    insert = GenerateName(insert, existingName, identifier, existingTemplate);
                  }
                }

                // append changing descriptions to each block
                for (Description description : newTemplateDefinition.getDescriptions())
                {
                  Description existingDescription = new Description();
                  for (Description tempName : existingTemplate.getDescriptions())
                  {
                    if (description.getLang().equalsIgnoreCase(tempName.getLang()))
                    {
                      existingDescription = tempName;
                    }
                  }
                  if (description != null && existingDescription != null)
                  {
                    if (!existingDescription.getValue().equalsIgnoreCase(description.getValue()))
                    {
                      delete = GenerateDescription(delete, existingDescription, identifier);
                      insert = GenerateDescription(insert, description, identifier);
                    }
                  }
                  else if (description != null && existingDescription == null)
                  {
                    insert = GenerateDescription(insert, description, identifier);
                  }
                }

                // role count
                index = 1;
                // / BaseTemplate roles do have the following properties
                // / 1) baseclass of owl:Thing
                // / 2) rdf:type = p8:TemplateRoleDescription
                // / 3) rdfs:label = rolename
                // / 4) p8:valRoleIndex
                // / 5) p8:hasRoleFillerType = qualifified class
                // / 6) p8:hasTemplate = template ID
                // / 7) p8:hasRole = role ID --- again probably should not use this --- pointer to self
                if (existingTemplate.getRoleDefinitions().size() < newTemplateDefinition.getRoleDefinitions().size())
                {
                  for (RoleDefinition role : newTemplateDefinition.getRoleDefinitions())
                  {

                    String roleName = role.getNames().get(0).getValue();
                    String roleIdentifier = role.getId();

                    if (roleIdentifier == null || roleIdentifier == "")
                    {
                      if (_useExampleRegistryBase)
                        generatedId = createIdsAdiId(_settings.get("ExampleRegistryBase").toString(), roleName);
                      else
                        generatedId = createIdsAdiId(_settings.get("TemplateRegistryBase").toString(), roleName);
                      roleIdentifier = generatedId;
                    }

                    RoleDefinition existingRole = null;
                    String tempRoleIdentifier = getIdFromURI(roleIdentifier);
                    for (RoleDefinition tempRoleDef : existingTemplate.getRoleDefinitions())
                    {
                      if (role.getId().equalsIgnoreCase(tempRoleDef.getId()))
                      {
                        existingRole = tempRoleDef;
                      }
                    }
                    if (existingRole == null) // / need to add it
                    {
                      for (Name name : role.getNames())
                      {
                        insert = GenerateName(insert, name, tempRoleIdentifier, role);
                      }
                      if (role.getDescriptions() != null)
                      {
                        insert = GenerateDescription(insert, role.getDescriptions().get(0), tempRoleIdentifier);
                      }
                      if (repository.getRepositorytype() == RepositoryType.PART_8)
                      {
                        insert = GenerateTypesPart8(insert, tempRoleIdentifier, identifier, role);
                        insert = GenerateRoleIndexPart8(insert, tempRoleIdentifier, index, role);
                      }
                      else
                      {
                        insert = GenerateTypes(insert, tempRoleIdentifier, identifier, role);
                        insert = GenerateRoleIndex(insert, tempRoleIdentifier, index);
                      }
                    }
                    if (role.getRange() != null)
                    {
                      //qName = _nsmap.reduceToQName(role.getRange());
                      if (repository.getRepositorytype() == RepositoryType.PART_8)
                      {
                        insert = GenerateRoleFillerType(insert, tempRoleIdentifier, role.getRange());
                      }
                      else
                      {
                        insert = GenerateRoleDomain(insert, tempRoleIdentifier, identifier);
                      }
                    }
                  }
                }
                else if (existingTemplate.getRoleDefinitions().size() > newTemplateDefinition.getRoleDefinitions()
                    .size()) // /Role(s) removed
                {
                  for (RoleDefinition role : existingTemplate.getRoleDefinitions())
                  {
                    String tempRoleID = getIdFromURI(role.getId());
                    // RoleDefinition nrd = newTDef.roleDefinition.Find(r => r.identifier == ord.identifier);
                    RoleDefinition existingRole = null;
                    for (RoleDefinition tempRoleDef : existingTemplate.getRoleDefinitions())
                    {
                      if (role.getId().equalsIgnoreCase(tempRoleDef.getId()))
                      {
                        existingRole = tempRoleDef;
                      }
                    }
                    if (existingRole == null) // / need to add it
                    {
                      for (Name name : role.getNames())
                      {
                        delete = GenerateName(delete, name, tempRoleID, role);
                      }
                      if (role.getDescriptions() != null)
                      {
                        delete = GenerateDescription(delete, role.getDescriptions().get(0), tempRoleID);
                      }
                      if (repository.getRepositorytype() == RepositoryType.PART_8)
                      {
                        delete = GenerateTypesPart8(delete, tempRoleID, identifier, role);
                        delete = GenerateRoleIndexPart8(delete, tempRoleID, index, role);
                      }
                      else
                      {
                        delete = GenerateTypes(delete, tempRoleID, identifier, role);
                        delete = GenerateRoleIndex(delete, tempRoleID, index);
                      }
                    }
                    if (role.getRange() != null)
                    {
                      //qName = _nsmap.reduceToQName(role.getRange());
                      if (repository.getRepositorytype() == RepositoryType.PART_8)
                      {
                        delete = GenerateRoleFillerType(delete, tempRoleID, role.getRange());
                      }
                      else
                      {
                        delete = GenerateRoleDomain(delete, tempRoleID, identifier);
                      }
                    }
                  }
                }
              }
              if (delete.isEmpty() && insert.isEmpty())
              {
                String errMsg = "No changes made to template [" + templateName + "]";
                Status status = new Status();
                response.setLevel(Level.WARNING);
                status.getMessages().getItems().add(errMsg);
                // response.Append(status);
              }
            }
            // endregion Form Delete/Insert
            // region Form Insert SPARQL
            if (insert.isEmpty() && delete.isEmpty())
            {
              if (repository.getRepositorytype() == RepositoryType.PART_8)
              {
                insert = GenerateTypesPart8(insert, identifier, null, newTemplateDefinition);
                insert = GenerateRoleCountPart8(insert, newTemplateDefinition.getRoleDefinitions().size(), identifier,
                    newTemplateDefinition);
              }
              else
              {
                insert = GenerateTypes(insert, identifier, null, newTemplateDefinition);
                insert = GenerateRoleCount(insert, newTemplateDefinition.getRoleDefinitions().size(), identifier,
                    newTemplateDefinition);
              }
              for (Name name : newTemplateDefinition.getNames())
              {
                insert = GenerateName(insert, name, identifier, newTemplateDefinition);
              }

              for (Description descr : newTemplateDefinition.getDescriptions())
              {
                insert = GenerateDescription(insert, descr, identifier);
              }
              // form labels
              for (RoleDefinition role : newTemplateDefinition.getRoleDefinitions())
              {

                String roleLabel = role.getNames().get(0).getValue();
                String roleID = "";
                generatedId = null;
                String genName = null;
                //String range = role.getRange();

                genName = "Role definition " + roleLabel;
                if (role.getId() == null || role.getId() == "")
                {
                  if (_useExampleRegistryBase)
                    generatedId = createIdsAdiId(_settings.get("ExampleRegistryBase").toString(), genName);
                  else
                    generatedId = createIdsAdiId(_settings.get("TemplateRegistryBase").toString(), genName);
                  roleID = getIdFromURI(generatedId);
                }
                else
                {
                  roleID = getIdFromURI(role.getId());
                }
                for (Name newName : role.getNames())
                {
                  insert = GenerateName(insert, newName, roleID, role);
                }

                if (role.getDescriptions() != null)
                {
                  insert = GenerateDescription(insert, role.getDescriptions().get(0), roleID);
                }

                if (repository.getRepositorytype() == RepositoryType.PART_8)
                {
                  insert = GenerateRoleIndexPart8(insert, roleID, ++roleCount, role);
                  insert = GenerateHasTemplate(insert, roleID, identifier, role);
                  insert = GenerateHasRole(insert, identifier, roleID.toString(), role);
                }
                else
                {
                  insert = GenerateRoleIndex(insert, roleID, ++roleCount);
                }
                if (role.getRange() != null && role.getRange() != "")
                {
                  //qName = _nsmap.reduceToQName(role.getRange());
                  if (repository.getRepositorytype() == RepositoryType.PART_8)
                  {
                    insert = GenerateRoleFillerType(insert, roleID, role.getRange());
                  }
                  else
                  {
                    insert = GenerateRoleDomain(insert, roleID, identifier);
                    insert = GenerateTypes(insert, roleID, null, role);
                  }
                }
              }
            }
            // endregion
            // region Generate Query and post Template Definition
            // TODO - formatter not working
            /*
             * if (!delete.isEmpty()) { sparqlBuilder.append(deleteData); for (TripleImpl t : delete.Triples) {
             * sparqlBuilder.append(t.ToString(formatter)); } if (insert.isEmpty()) sparqlBuilder.append("}"); else
             * sparqlBuilder.append("};"); } if (!insert.isEmpty()) { sparqlBuilder.append(insertData); for (TripleImpl
             * t : insert.Triples) { sparqlBuilder.AppendLine(t.ToString(formatter)); } sparqlBuilder.append("}"); }
             */
            //String sparql = sparqlBuilder.toString();
            //Response postResponse = postToRepository(repository, sparql);
            // response.Append(postResponse);
          }
        }
        // endregion Generate Query and post Template Definition
        // endregion Template Definitions

        // region Template Qualification
        // / Specialized templates do have the following properties
        // / 1) Base class = owl:Thing
        // / 2) rdf:type = p8:TemplateSpecialization
        // / 3) p8:hasSuperTemplate = Super Template ID
        // / 4) p8:hasSubTemplate = Sub Template ID
        // / 5) rdfs:label = template name
        // /
        if (qmxf.getTemplateQualifications().size() > 0)
        {
          for (TemplateQualification newTQ : qmxf.getTemplateQualifications())
          {
            int roleCount = 0;
            String templateName = null;
            String templateID = "";
            String generatedId = null;
            //String roleQualification = null;
            //int index = 1;
            if (newTQ.getId() != null && newTQ.getId() != "")
              templateID = newTQ.getId();

            templateName = newTQ.getNames().get(0).getValue();
            Qmxf oldQmxf = new Qmxf();
            // if (templateID!=null && templateID!="")
            if (templateID != "" && templateID != null)
            {
              oldQmxf = getTemplate(templateID.toString(), TemplateType.QUALIFICATION.toString(), repository);
            }
            else
            {
              if (_useExampleRegistryBase)
                generatedId = createIdsAdiId(_settings.get("ExampleRegistryBase").toString(), templateName);
              else
                generatedId = createIdsAdiId(_settings.get("TemplateRegistryBase").toString(), templateName);

              templateID = getIdFromURI(generatedId);
            }
            // region Form Delete/Insert SPARQL
            if (oldQmxf.getTemplateQualifications().size() > 0)
            {
              for (TemplateQualification oldTQ : oldQmxf.getTemplateQualifications())
              {
                //qName = _nsmap.reduceToQName(oldTQ.getQualifies());
                for (Name nn : newTQ.getNames())
                {
                  templateName = nn.getValue();
                  // Name on = oldTQ.name.Find(n => n.lang == nn.lang);
                  Name on = new Name();
                  for (Name tempName : oldTQ.getNames())
                  {
                    if (nn.getLang().equalsIgnoreCase(tempName.getLang()))
                    {
                      on = tempName;
                    }
                  }

                  if (on != null)
                  {
                    if (!on.getValue().equalsIgnoreCase(nn.getValue()))
                    {
                      delete = GenerateName(delete, on, templateID, oldTQ);
                      delete = GenerateName(insert, nn, templateID, newTQ);
                    }
                  }
                }
                for (Description nd : newTQ.getDescriptions())
                {
                  if (nd.getLang() == null)
                    nd.setLang(defaultLanguage);
                  // od = oldTQ.description.Find(d => d.lang == nd.lang);
                  Description od = null;
                  for (Description tempDesc : oldTQ.getDescriptions())
                  {
                    if (nd.getLang().equalsIgnoreCase(tempDesc.getLang()))
                    {
                      od = new Description();
                      od = tempDesc;
                    }
                  }

                  if (od != null && od.getValue() != null)
                  {
                    if (!od.getValue().equalsIgnoreCase(nd.getValue()))
                    {
                      delete = GenerateDescription(delete, od, templateID);
                      insert = GenerateDescription(insert, nd, templateID);
                    }
                  }
                  else if (od == null && nd.getValue() != null)
                  {
                    insert = GenerateDescription(insert, nd, templateID);
                  }
                }
                // role count
                if (oldTQ.getRoleQualifications().size() != newTQ.getRoleQualifications().size())
                {
                  if (repository.getRepositorytype() == RepositoryType.PART_8)
                  {
                    delete = GenerateRoleCountPart8(delete, oldTQ.getRoleQualifications().size(), templateID, oldTQ);
                    insert = GenerateRoleCountPart8(insert, newTQ.getRoleQualifications().size(), templateID, newTQ);
                  }
                  else
                  {
                    delete = GenerateRoleCount(delete, oldTQ.getRoleQualifications().size(), templateID, oldTQ);
                    insert = GenerateRoleCount(insert, newTQ.getRoleQualifications().size(), templateID, newTQ);
                  }
                }
                // // TODO need to work out howto correctly handle specializations
                for (Specialization ns : newTQ.getSpecializations())
                {
                  Specialization os = oldTQ.getSpecializations().get(0);

                  if (os != null && !(os.getReference().equalsIgnoreCase(ns.getReference())))
                  {
                    if (repository.getRepositorytype() == RepositoryType.PART_8)
                    {

                    }
                    else
                    {

                    }
                  }
                }

                //index = 1;
                // / SpecializedTemplate roles do have the following properties
                // / 1) baseclass of owl:Thing
                // / 2) rdf:type = p8:TemplateRoleDescription
                // / 3) rdfs:label = rolename
                // / 4) p8:valRoleIndex
                // / 5) p8:hasRoleFillerType = qualifified class
                // / 6) p8:hasTemplate = template ID
                // / 6) p8:hasRole = tpl:{roleName} probably don't need to use this -- pointer to self
                if (oldTQ.getRoleQualifications().size() < newTQ.getRoleQualifications().size())
                {
                  int count = 0;
                  for (RoleQualification nrq : newTQ.getRoleQualifications())
                  {
                    String roleName = nrq.getNames().get(0).getValue();
                    String newRoleID = nrq.getId();
                    String tempNewRoleID = getIdFromURI(newRoleID);

                    if (newRoleID == null || newRoleID == "")
                    {
                      if (_useExampleRegistryBase)
                        generatedId = createIdsAdiId(_settings.get("ExampleRegistryBase").toString(), roleName);
                      else
                        generatedId = createIdsAdiId(_settings.get("TemplateRegistryBase").toString(), roleName);
                      newRoleID = generatedId;
                    }
                    // RoleQualification orq = oldTQ.roleQualification.Find(r => r.identifier == newRoleID);
                    RoleQualification orq = null;
                    for (RoleQualification temprq : oldTQ.getRoleQualifications())
                    {
                      if (newRoleID.equalsIgnoreCase(temprq.getId()))
                      {
                        orq = new RoleQualification();
                        orq = temprq;
                      }
                    }

                    if (orq == null)
                    {
                      if (repository.getRepositorytype() == RepositoryType.PART_8)
                      {
                        insert = GenerateTypesPart8(insert, tempNewRoleID, templateID.toString(), nrq);
                        for (Name nn : nrq.getNames())
                        {
                          insert = GenerateName(insert, nn, tempNewRoleID, nrq);
                        }
                        insert = GenerateRoleIndexPart8(insert, tempNewRoleID, ++count, nrq);
                        insert = GenerateHasTemplate(insert, tempNewRoleID, templateID, nrq);
                        insert = GenerateHasRole(insert, templateID, tempNewRoleID, newTQ);
                        if (nrq.getRange() != null && nrq.getRange() == "")
                        {
                          //qName = _nsmap.reduceToQName(nrq.getRange());
                          if (qn)
                            insert = GenerateRoleFillerType(insert, tempNewRoleID, nrq.getRange());
                        }
                        else if (nrq.getValue() != null)
                        {
                          if (nrq.getValue().getReference() != null)
                          {
                            //qName = _nsmap.reduceToQName(nrq.getValue().getReference());
                            
                              insert = GenerateRoleFillerType(insert, tempNewRoleID, nrq.getValue().getReference());
                          }
                          else if (nrq.getValue().getText() != null)
                          {
                            // /TODO
                          }
                        }
                      }
                      else
                      // Not Part8 repository
                      {
                        if (nrq.getRange() != null && nrq.getRange() != "") // range restriction
                        {
                          //qName = _nsmap.reduceToQName(nrq.getRange());
                          
                            insert = GenerateRange(insert, tempNewRoleID, nrq.getRange(), nrq);
                          insert = GenerateTypes(insert, tempNewRoleID, templateID.toString(), nrq);
                          insert = GenerateQualifies(insert, tempNewRoleID, nrq.getQualifies().split("#")[1], nrq);
                        }
                        else if (nrq.getValue() != null)
                        {
                          if (nrq.getValue().getReference() != null) // reference restriction
                          {
                            insert = GenerateReferenceType(insert, tempNewRoleID, templateID, nrq);
                            insert = GenerateReferenceQual(insert, tempNewRoleID, nrq.getQualifies().split("#")[1], nrq);
                            //qName = _nsmap.reduceToQName(nrq.getValue().getReference());
                            
                              insert = GenerateReferenceTpl(insert, tempNewRoleID, nrq.getValue().getReference(), nrq);
                          }
                          else if (nrq.getValue().getText() != null)// value restriction
                          {
                            insert = GenerateValue(insert, tempNewRoleID.toString(), templateID.toString(), nrq);
                          }
                        }
                        insert = GenerateTypes(insert, tempNewRoleID, templateID.toString(), nrq);
                        insert = GenerateRoleDomain(insert, tempNewRoleID, templateID.toString());
                        insert = GenerateRoleIndex(insert, tempNewRoleID, ++count);
                      }
                    }
                  }
                }
                else if (oldTQ.getRoleQualifications().size() > newTQ.getRoleQualifications().size())
                {
                  int count = 0;
                  for (RoleQualification orq : oldTQ.getRoleQualifications())
                  {
                    String roleName = orq.getNames().get(0).getValue();
                    String newRoleID = orq.getId();
                    String tempNewRoleID = getIdFromURI(newRoleID);

                    if (newRoleID == null || newRoleID == "")
                    {
                      if (_useExampleRegistryBase)
                        generatedId = createIdsAdiId(_settings.get("ExampleRegistryBase").toString(), roleName);
                      else
                        generatedId = createIdsAdiId(_settings.get("TemplateRegistryBase").toString(), roleName);
                      newRoleID = generatedId;
                    }
                    // RoleQualification nrq = newTQ.roleQualification.Find(r => r.identifier == newRoleID);
                    RoleQualification nrq = null;
                    for (RoleQualification tempRq : newTQ.getRoleQualifications())
                    {
                      if (newRoleID.equalsIgnoreCase(tempRq.getId()))
                      {
                        nrq = new RoleQualification();
                        nrq = tempRq;
                      }
                    }
                    if (nrq == null)
                    {
                      if (repository.getRepositorytype() == RepositoryType.PART_8)
                      {
                        delete = GenerateTypesPart8(delete, tempNewRoleID, templateID.toString(), orq);
                        for (Name nn : orq.getNames())
                        {
                          delete = GenerateName(delete, nn, tempNewRoleID, orq);
                        }
                        delete = GenerateRoleIndexPart8(delete, tempNewRoleID, ++count, orq);
                        delete = GenerateHasTemplate(delete, tempNewRoleID, templateID.toString(), orq);
                        delete = GenerateHasRole(delete, templateID, tempNewRoleID.toString(), oldTQ);
                        if (orq.getRange() != null && orq.getRange() != "")
                        {
                          //qName = _nsmap.reduceToQName(orq.getRange());
                          
                            delete = GenerateRoleFillerType(delete, tempNewRoleID, orq.getRange());
                        }
                        else if (orq.getValue() != null)
                        {
                          if (orq.getValue().getReference() != null)
                          {
                            //qName = _nsmap.reduceToQName(orq.getValue().getReference());
                           
                              delete = GenerateRoleFillerType(delete, tempNewRoleID, orq.getValue().getReference());
                          }
                        }
                      }
                      else
                      // Not Part8 repository
                      {
                        if (orq.getRange() != null && orq.getRange() != "") // range restriction
                        {
                          //qName = _nsmap.reduceToQName(orq.getRange());
                         
                            delete = GenerateRange(delete, tempNewRoleID, orq.getRange(), orq);
                          delete = GenerateTypes(delete, tempNewRoleID, templateID.toString(), nrq);
                          delete = GenerateQualifies(delete, tempNewRoleID, orq.getQualifies().split("#")[1], orq);
                        }
                        else if (orq.getValue() != null)
                        {
                          if (orq.getValue().getReference() != null) // reference restriction
                          {
                            delete = GenerateReferenceType(delete, tempNewRoleID, templateID.toString(), orq);
                            delete = GenerateReferenceQual(delete, tempNewRoleID, orq.getQualifies().split("#")[1], orq);
                            //qName = _nsmap.reduceToQName(orq.getValue().getReference());
                           
                              insert = GenerateReferenceTpl(insert, tempNewRoleID, orq.getValue().getReference(), orq);
                          }
                          else if (orq.getValue().getText() != null)// value restriction
                          {
                            delete = GenerateValue(delete, tempNewRoleID.toString(), templateID.toString(), orq);
                          }
                        }
                        delete = GenerateTypes(delete, tempNewRoleID, templateID.toString(), orq);
                        delete = GenerateRoleDomain(delete, tempNewRoleID, templateID.toString());
                        delete = GenerateRoleIndex(delete, tempNewRoleID, ++count);
                      }
                    }
                  }
                }
              }
              if (delete.isEmpty() && insert.isEmpty())
              {
                String errMsg = "No changes made to template [" + templateName + "]";
                Status status = new Status();
                response.setLevel(Level.WARNING);
                status.getMessages().getItems().add(errMsg);
                // response.Append(status);
                // continue;//Nothing to be done
              }
            }
            // endregion
            // region Form Insert SPARQL
            if (delete.isEmpty())
            {
              //String templateLabel = null;
              //String labelSparql = null;

              for (Name newName : newTQ.getNames())
              {
                insert = GenerateName(insert, newName, templateID, newTQ);
              }
              for (Description newDescr : newTQ.getDescriptions())
              {
                if (newDescr.getValue() == null || newDescr.getValue() == "")
                  continue;
                insert = GenerateDescription(insert, newDescr, templateID);
              }

              if (repository.getRepositorytype() == RepositoryType.PART_8)
              {
                insert = GenerateRoleCountPart8(insert, newTQ.getRoleQualifications().size(), templateID, newTQ);
                //qName = _nsmap.reduceToQName(newTQ.getQualifies());
                
                  insert = GenerateTypesPart8(insert, templateID, newTQ.getQualifies(), newTQ);
              }
              else
              {
                GenerateRoleCount(insert, newTQ.getRoleQualifications().size(), templateID, newTQ);
                //qName = _nsmap.reduceToQName(newTQ.getQualifies());
                
                  insert = GenerateTypes(insert, templateID, newTQ.getQualifies(), newTQ);

              }
              /*for (Specialization spec : newTQ.getSpecializations())
              {
                //String specialization = spec.getReference();
                if (repository.getRepositorytype() == RepositoryType.PART_8)
                {
                  // /TODO
                }
                else
                {
                  // /TODO
                }
              }*/

              for (RoleQualification newRole : newTQ.getRoleQualifications())
              {
                String roleLabel = newRole.getNames().get(0).getValue();
                String roleID = "";
                generatedId = null;
                String genName = null;
                //String range = newRole.getRange();

                genName = "Role Qualification " + roleLabel;
                if (newRole.getId() == null && newRole.getId() == "")
                {
                  if (_useExampleRegistryBase)
                    generatedId = createIdsAdiId(_settings.get("ExampleRegistryBase").toString(), genName);
                  else
                    generatedId = createIdsAdiId(_settings.get("TemplateRegistryBase").toString(), genName);

                  roleID = generatedId;
                }
                else
                {
                  roleID = newRole.getId();
                }
                if (repository.getRepositorytype() == RepositoryType.PART_8)
                {
                  insert = GenerateTypesPart8(insert, roleID, templateID.toString(), newRole);
                  for (Name newName : newRole.getNames())
                  {
                    insert = GenerateName(insert, newName, roleID, newRole);
                  }
                  insert = GenerateRoleIndexPart8(insert, roleID, ++roleCount, newRole);
                  insert = GenerateHasTemplate(insert, roleID, templateID.toString(), newRole);
                  insert = GenerateHasRole(insert, templateID, roleID.toString(), newTQ);
                  if (newRole.getRange() != null && newRole.getRange() != "")
                  {
                    //qName = _nsmap.reduceToQName(newRole.getRange());
                    
                      insert = GenerateRoleFillerType(insert, roleID, newRole.getRange());
                  }
                  else if (newRole.getValue() != null)
                  {
                    if (newRole.getValue().getReference() != null)
                    {
                      //qName = _nsmap.reduceToQName(newRole.getValue().getReference());
                      
                        insert = GenerateRoleFillerType(insert, roleID, newRole.getValue().getReference());
                    }
                    else if (newRole.getValue().getText() != null)
                    {
                      // /TODO
                    }
                  }
                }
                else
                // Not Part8 repository
                {
                  if (newRole.getRange() != null && newRole.getRange() != "") // range restriction
                  {

                    //qName = _nsmap.reduceToQName(newRole.getRange());
                    
                      insert = GenerateRange(insert, roleID, newRole.getRange(), newRole);
                    insert = GenerateTypes(insert, roleID, templateID.toString(), newRole);
                    insert = GenerateQualifies(insert, roleID, newRole.getQualifies().split("#")[1], newRole);
                  }
                  else if (newRole.getValue() != null)
                  {
                    if (newRole.getValue().getReference() != null) // reference restriction
                    {
                      insert = GenerateReferenceType(insert, roleID, templateID.toString(), newRole);
                      insert = GenerateReferenceQual(insert, roleID, newRole.getQualifies().split("#")[1], newRole);
                      //qName = _nsmap.reduceToQName(newRole.getValue().getReference());
                      
                        insert = GenerateReferenceTpl(insert, roleID, newRole.getValue().getReference(), newRole);
                    }
                    else if (newRole.getValue().getText() != null)// value restriction
                    {
                      insert = GenerateValue(insert, roleID.toString(), templateID.toString(), newRole);
                    }
                  }
                  insert = GenerateTypes(insert, roleID, templateID.toString(), newRole);
                  insert = GenerateRoleDomain(insert, roleID, templateID.toString());
                  insert = GenerateRoleIndex(insert, roleID, ++roleCount);
                }
              }
            }

            // endregion
            // region Generate Query and Post Qualification Template
            // TODO
            /*
             * if (!delete.isEmpty()) { sparqlBuilder.append(deleteData); for (Triple t : delete.Triples) {
             * sparqlBuilder.append(t.ToString(formatter)); } if (insert.isEmpty()) sparqlBuilder.append("}"); else
             * sparqlBuilder.append("};"); } if (!insert.isEmpty()) { sparqlBuilder.append(insertData); for (Triple t :
             * insert.Triples) { sparqlBuilder.append(t.ToString(formatter)); } sparqlBuilder.append("}"); }
             */

            //String sparql = sparqlBuilder.toString();
            //Response postResponse = postToRepository(repository, sparql);
            // response.Append(postResponse);
          }
        }

      }
    }

    catch (Exception ex)
    {
      String errMsg = "Error in PostTemplate: " + ex;
      Status status = new Status();

      response.setLevel(Level.ERROR);
      status.getMessages().getItems().add(errMsg);
      // response.Append(status);

      logger.error(errMsg);
    }

    return response;
  }

//  private int getIndexFromName(String name)
//  {
//    int index = 0;
//    try
//    {
//      for (Repository repository : _repositories)
//      {
//        if (repository.getName().equalsIgnoreCase(name))
//        {
//          return index;
//        }
//        index++;
//      }
//      index = 0;
//      for (Repository repository : _repositories)
//      {
//        if (!repository.isIsReadOnly())
//        {
//          return index;
//        }
//        index++;
//      }
//    }
//    catch (Exception ex)
//    {
//      logger.error(ex);
//
//    }
//    return index;
//  }

//  private Response postToRepository(Repository repository, String sparql)
//  {
//    Response response = new Response();
//    //Status status = null;
//
//    try
//    {
//      String uri = repository.getUpdateUri().toString();
//      
//      if (repository.isIsReadOnly() == false)
//      {
//        HttpClient sparqlClient = new HttpClient(uri);
//        HttpUtils.addHttpHeaders(_settings, sparqlClient);
//        sparqlClient.postSparql(String.class, "", sparql, "");
//        //status = new Status();
//      }
//      else
//      {
//
//      }
//    }
//    catch (Exception ex)
//    {
//      logger.error(ex);
//      return response;
//    }
//
//    return response;
//  }

    public Response postClass(Qmxf qmxf)
  {
    // TODO - to be initialised
    Model delete = ModelFactory.createDefaultModel();
    Model insert = ModelFactory.createDefaultModel();
    for (String prefix : _nsmap.getPrefixes())
    {
      delete.setNsPrefix(prefix, _nsmap.getNamespaceUri(prefix).toString());
      insert.setNsPrefix(prefix, _nsmap.getNamespaceUri(prefix).toString());
    }

    Response response = new Response();
    response.setLevel(Level.SUCCESS);
    //boolean qn = false;
    //String qName = null;

    try
    {
      Repository repository = getRepository(qmxf.getTargetRepository());

      if (repository == null || repository.getIsreadonly())
      {
        Status status = new Status();
        response.setLevel(Level.ERROR);

        if (repository == null)
          status.getMessages().getItems().add("Repository not found!");
        else
          status.getMessages().getItems().add("Repository [" + qmxf.getTargetRepository() + "] is read-only!");

        // _response.Append(status);
      }
      else
      {
        String registry = _useExampleRegistryBase ? _settings.get("ExampleRegistryBase").toString() : _settings.get(
            "ClassRegistryBase").toString();

        for (ClassDefinition clsDef : qmxf.getClassDefinitions())
        {

          //String language = null;
          //int classCount = 0;
          String clsId = clsDef.getId();
          Qmxf existingQmxf = new Qmxf();

          if (clsId != null)
          {
            existingQmxf = getClass(clsId, repository);
          }

          // delete class
          if (existingQmxf.getClassDefinitions().size() > 0)
          {

            for (ClassDefinition existingClsDef : existingQmxf.getClassDefinitions())
            {
              for (Name clsName : clsDef.getNames())
              {
                // Name existingName = existingClsDef.name.Find(n => n.lang == clsName.lang);
                Name existingName = new Name();
                for (Name tempName : existingClsDef.getNames())
                {
                  if (clsName.getLang().equalsIgnoreCase(tempName.getLang()))
                  {
                    existingName = tempName;
                  }
                }

                if (existingName != null)
                {
                  if (!existingName.getValue().equalsIgnoreCase(clsName.getValue()))
                  {
                    delete = GenerateClassName(delete, existingName, clsId, existingClsDef);
                    insert = GenerateClassName(insert, clsName, clsId, clsDef);
                  }
                }

                for (Description description : clsDef.getDescriptions())
                {
                  // Description existingDescription = existingClsDef.description.Find(d => d.lang == description.lang);
                  Description existingDescription = new Description();
                  for (Description tempDesc : existingClsDef.getDescriptions())
                  {
                    if (description.getLang().equalsIgnoreCase(tempDesc.getLang()))
                    {
                      existingDescription = tempDesc;
                    }
                  }
                  if (existingDescription != null)
                  {
                    if (!existingDescription.getValue().equalsIgnoreCase(description.getValue()))
                    {
                      delete = GenerateClassDescription(delete, existingDescription, clsId);
                      insert = GenerateClassDescription(insert, description, clsId);
                    }
                  }
                }
                if (clsDef.getSpecializations().size() == existingClsDef.getSpecializations().size())
                {
                  continue; // / no change ... so continue
                }
                else if (clsDef.getSpecializations().size() < existingClsDef.getSpecializations().size()) // some is
                                                                                                          // deleted
                                                                                                          // ...focus on
                                                                                                          // old to find
                                                                                                          // deleted
                {
                  for (Specialization os : existingClsDef.getSpecializations())
                  {
                    // Specialization ns = newClsDef.specialization.Find(s => s.reference == os.reference);
                    Specialization ns = null;
                    for (Specialization tempSpec : clsDef.getSpecializations())
                    {
                      if (os.getReference().equalsIgnoreCase(tempSpec.getReference()))
                      {
                        ns = new Specialization();
                        ns = tempSpec;
                      }
                    }

                    if (ns == null)
                    {
                      //qName = _nsmap.reduceToQName(os.getReference());
                      
                        delete = GenerateRdfSubClass(delete, clsId, os.getReference());
                    }
                  }
                }
                else if (clsDef.getSpecializations().size() > existingClsDef.getSpecializations().size())// some is
                                                                                                         // added ...
                                                                                                         // find added
                {
                  for (Specialization ns : clsDef.getSpecializations())
                  {
                    // Specialization os = oldClsDef.specialization.Find(s => s.reference == ns.reference);
                    Specialization os = null;
                    for (Specialization tempSpec : existingClsDef.getSpecializations())
                    {
                      if (ns.getReference().equalsIgnoreCase(tempSpec.getReference()))
                      {
                        os = new Specialization();
                        os = tempSpec;
                      }
                    }
                    if (os == null)
                    {
                      //qName = _nsmap.reduceToQName(ns.getReference());
                      
                        insert = GenerateRdfSubClass(insert, clsId, ns.getReference());
                    }
                  }
                }
                if (clsDef.getClassifications().size() == existingClsDef.getClassifications().size())
                {
                  continue; // no change...so continue
                }
                else if (clsDef.getClassifications().size() < existingClsDef.getClassifications().size()) // some is
                                                                                                          // deleted
                                                                                                          // ...focus on
                                                                                                          // old to find
                                                                                                          // deleted
                {
                  for (Classification oc : existingClsDef.getClassifications())
                  {
                    // Classification nc = newClsDef.classification.Find(c => c.reference == oc.reference);
                    Classification nc = null;
                    for (Classification tempClas : clsDef.getClassifications())
                    {
                      if (oc.getReference().equalsIgnoreCase(tempClas.getReference()))
                      {
                        nc = new Classification();
                        nc = tempClas;
                      }
                    }
                    if (nc == null)
                    {
                      //qName = _nsmap.reduceToQName(oc.getReference());
                      if (repository.getRepositorytype() == RepositoryType.PART_8)
                      {
                       
                          delete = GenerateSuperClass(delete, oc.getReference(), clsId.toString()); // /delete from old
                      }
                      else
                      {
                      
                          delete = GenerateDmClassification(delete, clsId, oc.getReference());
                      }
                    }
                  }
                }
                else if (clsDef.getClassifications().size() > existingClsDef.getClassifications().size())// some is
                                                                                                         // added ...
                                                                                                         // find added
                                                                                                         // classifications
                {
                  for (Classification nc : clsDef.getClassifications())
                  {
                    // Classification oc = oldClsDef.classification.Find(c => c.reference == nc.reference);
                    Classification oc = null;
                    for (Classification tempClas : existingClsDef.getClassifications())
                    {
                      if (nc.getReference().equalsIgnoreCase(tempClas.getReference()))
                      {
                        oc = new Classification();
                        oc = tempClas;
                      }
                    }
                    if (oc == null)
                    {
                      //qName = _nsmap.reduceToQName(nc.getReference());
                      if (repository.getRepositorytype() == RepositoryType.PART_8)
                      {
                        
                          insert = GenerateSuperClass(insert, nc.getReference(), clsId.toString()); // /insert from new
                      }
                      else
                      {
                        
                          insert = GenerateDmClassification(insert, clsId, nc.getReference());
                      }
                    }
                  }
                }
              }
            }

            if (delete.isEmpty() && insert.isEmpty())
            {
              String errMsg = "No changes made to class ["
                  + qmxf.getClassDefinitions().get(0).getNames().get(0).getValue() + "]";
              Status status = new Status();
              response.setLevel(Level.WARNING);
              status.getMessages().getItems().add(errMsg);
              // response.Append(status);
              continue;// Nothing to be done
            }
          }
          // add class
          if (delete.isEmpty() && insert.isEmpty())
          {
            String clsLabel = clsDef.getNames().get(0).getValue();
            if (clsId == null)
            {
              String newClsName = "Class definition " + clsLabel;
              clsId = createIdsAdiId(registry, newClsName);
            }
            // append entity type
            if (clsDef.getEntityType() != null
                && (clsDef.getEntityType().getReference() != null && clsDef.getEntityType().getReference() != ""))
            {
              //qName = _nsmap.reduceToQName(clsDef.getEntityType().getReference());
              
                insert = GenerateTypesPart8(insert, clsId, clsDef.getEntityType().getReference(), clsDef);
            }
            // append specialization
            for (Specialization ns : clsDef.getSpecializations())
            {
              if (ns.getReference() != null && ns.getReference() != "")
              {
                //qName = _nsmap.reduceToQName(ns.getReference());
                if (repository.getRepositorytype() == RepositoryType.PART_8)
                {
                  
                    insert = GenerateRdfSubClass(insert, clsId, ns.getReference());
                }
                else
                {
                  
                    insert = GenerateDmSubClass(insert, clsId, ns.getReference());
                }
              }
            }
            // append description
            for (Description nd : clsDef.getDescriptions())
            {
              if (nd.getValue() != null && nd.getValue() != "")
              {
                insert = GenerateClassDescription(insert, nd, clsId);
              }
            }
            for (Name nn : clsDef.getNames())
            {
              // append label
              insert = GenerateClassName(insert, nn, clsId, clsDef);
            }
            // append classification
            for (Classification nc : clsDef.getClassifications())
            {
              if (nc.getReference() != null && nc.getReference() != "")
              {
                //qName = _nsmap.reduceToQName(nc.getReference());
                if (repository.getRepositorytype() == RepositoryType.PART_8)
                {
                 
                    insert = GenerateSuperClass(insert, nc.getReference(), clsId.toString());
                }
                else
                {
                 
                    insert = GenerateDmClassification(insert, clsId, nc.getReference());
                }
              }
            }
          }
          if (!delete.isEmpty())
          {
            sparqlBuilder.append(deleteData);
            // TODO
            StmtIterator deletes = delete.listStatements();
            while (deletes.hasNext())
            {
              sparqlBuilder.append(deletes.nextStatement().toString());

            }
            if (insert.isEmpty())
              sparqlBuilder.append("}");
            else
              sparqlBuilder.append("};");
          }

          /*
           * for (Triple t in delete.Triples) { sparqlBuilder.AppendLine(t.ToString(formatter)); }
           */
          if (!insert.isEmpty())
          {
            sparqlBuilder.append(insertData);
            StmtIterator inserts = insert.listStatements();
            while (inserts.hasNext())
            {
              sparqlBuilder.append(inserts.nextStatement().toString());

            }
            /*
             * for (Triple t in insert.Triples) { sparqlBuilder.AppendLine(t.ToString(formatter)); }
             */
            sparqlBuilder.append("}");
          }

          //String sparql = sparqlBuilder.toString();
          //Response postResponse = postToRepository(repository, sparql);
          // response.append(postResponse);
        }
      }
    }
    catch (Exception ex)
    {
      String errMsg = "Error in PostClass: " + ex;
      Status status = new Status();

      response.setLevel(Level.ERROR);
      status.getMessages().getItems().add(errMsg);
      // response.Append(status);

      logger.error(errMsg);
    }

    return response;
  }

  public List<Repository> getRepositories() throws Exception
  {
    List<Repository> repositoryList = new ArrayList<Repository>();
    try
    {
      Federation federation = getFederation();
      for (Repository repo : federation.getRepositorylist().getItems())
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
    // Repository repository = null;
    for (Repository tempRepo : _repositories)
    {
      if (tempRepo.getName().equalsIgnoreCase(name))
      {
        return tempRepo;
      }
    }
    return null;
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
      //String relativeUri = "";
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
        if (repository.getRepositorytype().equals(RepositoryType.PART_8))
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
      QueryBindings queryBindings = null;
      // TODO Check the search History for Optimization
      // if (_searchHistory.containsKey(query)) {
      // entityList.addAll(_searchHistory.keySet().ceilingEntry(query).getValue());
      // } else {
      Map<String, Entity> resultEntities = new TreeMap<String, Entity>();

      Query queryContainsSearch = getQuery("ContainsSearch");
      
      Query queryContainsSearchJORD = getQuery("ContainsSearchJORD");

      for (Repository repository : _repositories)
      {
        
        if (repository.getRepositorytype() == RepositoryType.JORD)
        {
        	sparql = readSparql(queryContainsSearch.getFileName());
            sparql = sparql.replace("param1", query);
            queryBindings = queryContainsSearch.getBindings();
        }
        else 
        {
            sparql = readSparql(queryContainsSearchJORD.getFileName());
            sparql = sparql.replace("param1", query);
            queryBindings = queryContainsSearchJORD.getBindings();
        }
        
        Results sparqlResults = null;
        
        try
        {
          sparqlResults = queryFromRepository(repository, sparql);
        }
        catch (Exception e)
        {
          logger.error(e.getMessage());
        }
        
        if (sparqlResults != null)
        {
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
            if (names[0].startsWith("has") || names[0].startsWith("val"))
              continue;
            Entity tempVar = new Entity();
            if(result.get("uri") != null)
            	tempVar.setUri(result.get("uri"));
            else if (result.get("rds") != null)
            	tempVar.setRdsuri(result.get("rds"));
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
    }
    catch (Exception e)
    {
      logger.error("Error in SearchPage: " + e);
    }

    return response;
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

  public org.iringtools.refdata.response.Response getSuperClasses(String id, Repository repo) throws Exception
  {
    org.iringtools.refdata.response.Response response = new org.iringtools.refdata.response.Response();
    Entities entities = new Entities();
    response.setEntities(entities);
    List<Entity> entityList = new ArrayList<Entity>();
    entities.setItems(entityList);

    String[] names = null;
    try
    {
      List<Specialization> specializations = getSpecializations(id, repo);

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

  public org.iringtools.refdata.response.Response getSubClasses(String id, Repository repo) throws Exception
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
      String sparqlJord = "";
      //String relativeUri = "";
      String language = "";
      String[] names = null;

      Query queryGetSubClasses = getQuery("GetSubClasses");
      QueryBindings queryBindings = queryGetSubClasses.getBindings();

      sparql = readSparql(queryGetSubClasses.getFileName()).replace("param1", id);

      Query queryGetSubClassOfInverse = getQuery("GetSubClassOfInverse");
      QueryBindings queryBindingsPart8 = queryGetSubClassOfInverse.getBindings();

      sparqlPart8 = readSparql(queryGetSubClassOfInverse.getFileName()).replace("param1", id);
      
      Query queryGetSubClassJord = getQuery("GetSubClassesJORD");
      QueryBindings queryBindingsJord = queryGetSubClassJord.getBindings();

      sparqlJord = readSparql(queryGetSubClassJord.getFileName()).replace("param1", id);

      for (Repository repository : _repositories)
      {
    	  if (repo != null)
          {
            if (repo.getName() != repository.getName())
            {
              continue;
            }
          }
    	  
        if (repository.getRepositorytype().equals(RepositoryType.PART_8))
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
        else if (repository.getRepositorytype().equals(RepositoryType.JORD))
        {
        	Results sparqlResults = queryFromRepository(repository, sparqlJord);

            List<Hashtable<String, String>> results = bindQueryResults(queryBindingsJord, sparqlResults);

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
              if (result.get("uri")!=null)
              {
            	  tempVar.setUri(result.get("uri"));
              }
              else if (result.get("rds")!=null)
              {
            	  tempVar.setRdsuri(result.get("rds"));
              }
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
          names = getLabel(uri).getLabel().split("[@]", -1);
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
      String uri = _settings.get("idGenServiceUri").toString();
      httpClient = new HttpClient(uri);
      HttpUtils.addHttpHeaders(_settings, httpClient);
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
      Query queryEquivalent = getQuery("GetLabelRdlEquivalent");
      Query query = getQuery("GetLabel");
      QueryBindings queryBindings = null;

      for (Repository repository : _repositories)
      {
    	  if (repository.getRepositorytype() == RepositoryType.JORD && uri.contains("#"))
          {
    		  sparql = readSparql(queryEquivalent.getFileName()).replace("param1", uri);
    		  queryBindings = queryEquivalent.getBindings();
    	  }
    	  else
    	  {
    		  sparql = readSparql(query.getFileName()).replace("param1", uri);
    		  queryBindings = query.getBindings();
    	  }
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

//  private List<RoleDefinition> getRoleDefinition(String id) throws Exception, HttpClientException
//  {
//
//    List<RoleDefinition> roleDefinitions = new ArrayList<RoleDefinition>();
//    try
//    {
//      String sparql = "";
//      String sparqlQuery = "";
//      String[] names = null;
//
//      Description description = new Description();
//      //org.ids_adi.ns.qxf.model.Status status = new org.ids_adi.ns.qxf.model.Status();
//
//      //List<Entity> resultEntities = new ArrayList<Entity>();
//
//      for (Repository repository : _repositories)
//      {
//        switch (repository.getRepositorytype())
//        {
//        case CAMELOT:
//        case RDS_WIP:
//          sparqlQuery = "GetRoles";
//          break;
//        case PART_8:
//          sparqlQuery = "GetPart8Roles";
//          break;
//        }
//        Query queryContainsSearch = getQuery(sparqlQuery);
//        QueryBindings queryBindings = queryContainsSearch.getBindings();
//
//        sparql = readSparql(queryContainsSearch.getFileName());
//        sparql = sparql.replace("param1", id);
//        Results sparqlResults = queryFromRepository(repository, sparql);
//
//        List<Hashtable<String, String>> results = bindQueryResults(queryBindings, sparqlResults);
//
//        for (Hashtable<String, String> result : results)
//        {
//          RoleDefinition roleDefinition = new RoleDefinition();
//          Name name = new Name();
//
//          if (result.containsKey("label"))
//          {
//            names = result.get("label").split("@", -1);
//            name.setValue(names[0]);
//            if (names.length == 1)
//            {
//              name.setLang(defaultLanguage);
//            }
//            else
//            {
//              name.setLang(names[names.length - 1]);
//            }
//          }
//          if (result.containsKey("role"))
//          {
//            roleDefinition.setId(result.get("role"));
//          }
//          if (result.containsKey("comment"))
//          {
//            names = result.get("comment").split("@", -1);
//            description.setValue(names[0]);
//            if (names.length == 1)
//            {
//              description.setLang(defaultLanguage);
//            }
//            else
//            {
//              description.setLang(names[names.length - 1]);
//            }
//          }
//          if (result.containsKey("index"))
//          {
//            description.setValue(result.get("index").toString());
//          }
//          if (result.containsKey("type"))
//          {
//            roleDefinition.setRange(result.get("type"));
//          }
//          roleDefinition.getNames().add(name);
//          roleDefinition.getDescriptions().add(description);
//          roleDefinitions.add(roleDefinition);
//        }
//      }
//
//      return roleDefinitions;
//    }
//    catch (RuntimeException e)
//    {
//      logger.error("Error in GetRoleDefinition: " + e);
//      return roleDefinitions;
//    }
//  }

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
        //String sortKey = "";
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
                //sortKey = bindingValue;
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
    Results results = null;

    try
    {
      // TODO need to look at credentials
      HttpClient sparqlClient = new HttpClient(repository.getUri());
      HttpUtils.addHttpHeaders(_settings, sparqlClient);
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

  // updations

  private Model GenerateValue(Model work, String subjId, String objId, Object gobj)
  {
    RoleQualification role = (RoleQualification) gobj;
    Resource subj = work.createResource(String.format("<%s>", subjId));
    Property pred = work.createProperty("tpl:R56456315674");
    Resource obj = work.createResource(String.format("<%s>", objId));
    work.add(ModelFactory.createDefaultModel().createStatement(subj, pred, obj));
    pred = work.createProperty("tpl:R89867215482");
    obj = work.createResource(String.format("<%s>", role.getQualifies().split("#", 1).toString()));
    work.add(subj, pred, obj);
    pred = work.createProperty("tpl:R29577887690");
    Literal obj1 = work.createTypedLiteral(role.getValue().getText(), (role.getValue().getLang() == null || role
        .getValue().getLang() == "") ? defaultLanguage : role.getValue().getLang());
    work.add(subj, pred, obj1);
    return work;
  }

  private Model GenerateReferenceQual(Model work, String subjId, String objId, Object gobj)
  {
    Resource subj = work.createResource(String.format("<%s>", subjId));
    Property pred = work.createProperty("tpl:R30741601855");
    Resource obj = work.createResource(String.format("<%s>", objId));
    work.add(subj, pred, obj);
    return work;
  }

  private Model GenerateReferenceType(Model work, String subjId, String objId, Object gobj)
  {
    Resource subj = work.createResource(String.format("<%s>", subjId));
    Property pred = work.createProperty(rdfType);
    Resource obj = work.createResource("tpl:R40103148466");
    work.add(subj, pred, obj);
    pred = work.createProperty("tpl:R49267603385");
    obj = work.createProperty(String.format("<%s>", objId));
    work.add(subj, pred, obj);
    return work;
  }

  private Model GenerateReferenceTpl(Model work, String subjId, String objId, Object gobj)
  {
    Resource subj = work.createResource(String.format("<%s>", subjId));
    Property pred = work.createProperty("tpl:R21129944603");
    Resource obj = work.createResource(objId);
    work.add(subj, pred, obj);
    return work;
  }

  private Model GenerateQualifies(Model work, String subjId, String objId, Object gobj)
  {
    Resource subj = work.createResource(String.format("<%s>", subjId));
    Property pred = work.createProperty("tpl:R91125890543");
    Resource obj = work.createResource(String.format("<%s>", objId));
    work.add(subj, pred, obj);
    return work;
  }

  private Model GenerateRange(Model work, String subjId, String objId, Object gobj)
  {
    Resource subj = work.createResource(String.format("<%s>", subjId));
    Property pred = work.createProperty("rdfs:range");
    Resource obj = work.createResource(objId);
    work.add(subj, pred, obj);
    pred = work.createProperty("tpl:R98983340497");
    obj = work.createProperty(String.format("<%s>", objId));
    work.add(subj, pred, obj);
    return work;
  }

  private Model GenerateHasRole(Model work, String subjId, String objId, Object gobj)
  {
    Resource subj = work.createResource(String.format("<%s>", subjId));
    Property pred = work.createProperty("p8:hasRole");
    Resource res = work.createResource(String.format("<%s>", objId));
    work.add(subj, pred, res);
    return work;
  }

  private Model GenerateHasTemplate(Model work, String subjId, String objId, Object gobj)
  {
    if (gobj instanceof RoleDefinition || gobj instanceof RoleQualification)
    {
      Resource subj = work.createResource(String.format("<%s>", subjId));
      Property pred = work.createProperty("p8:hasTemplate");
      Resource res = work.createResource(String.format("<%s>", objId));
      work.add(subj, pred, res);
    }
    return work;

  }

  private Model GenerateRoleIndex(Model work, String subjId, int index) throws Exception
  {
    Resource subj = work.createResource(String.format("tpl:%s", subjId));
    Property pred = work.createProperty("tpl:R97483568938");
    Literal obj = work.createTypedLiteral(String.valueOf(index), "xsd:integer");
    work.add(subj, pred, obj);
    return work;
  }

  private Model GenerateRoleIndexPart8(Model work, String subjId, int index, Object gobj) throws Exception
  {
    if (gobj instanceof RoleDefinition || gobj instanceof RoleQualification)
    {
      Resource subj = work.createResource(String.format("<%s>", subjId));
      Property pred = work.createProperty("p8:valRoleIndex");
      Literal obj = work.createTypedLiteral(String.valueOf(index), "xsd:integer");
      work.add(subj, pred, obj);
    }
    return work;
  }

  private Model GenerateRoleDomain(Model work, String subjId, String objId)
  {
    Resource subj = work.createResource(String.format("<%s>", subjId));
    Property pred = work.createProperty("rdfs:domain");
    Resource obj = work.createProperty(String.format("<%s>", objId));
    work.add(subj, pred, obj);
    return work;
  }

  private Model GenerateRoleFillerType(Model work, String subjId, String range)
  {
    Resource subj = work.createResource(String.format("<%s>", subjId));
    Property pred = work.createProperty("p8:hasRoleFillerType");
    Resource obj = work.createProperty(String.format("<%s>", range));
    work.add(subj, pred, obj);
    return work;
  }

  private Model GenerateRoleCount(Model work, int rolecount, String subjId, Object gobj) throws Exception
  {
    if (gobj instanceof TemplateDefinition || gobj instanceof TemplateQualification)
    {
      Resource subj = work.createResource(String.format("<%s>", subjId));
      Property pred = work.createProperty("tpl:R35529169909");
      Literal obj = work.createTypedLiteral(String.valueOf(rolecount), "xsd:integer");
      work.add(subj, pred, obj);
    }
    return work;
  }

  private Model GenerateRoleCountPart8(Model work, int rolecount, String subjId, Object gobj) throws Exception
  {
    if (gobj instanceof TemplateDefinition || gobj instanceof TemplateQualification)
    {
      Resource subj = work.createResource(String.format("tpl:%s", subjId));
      Property pred = work.createProperty("p8:valNumberOfRoles");
      Literal obj = work.createTypedLiteral(String.valueOf(rolecount), "xsd:integer");
      work.add(subj, pred, obj);
    }
    return work;
  }

  private Model GenerateTypesPart8(Model work, String subjId, String objectId, Object gobj)
  {
    if (gobj instanceof TemplateDefinition)
    {
      Resource subj = work.createResource(String.format("<%s>", subjId));
      Property pred = work.createProperty(rdfType);
      Resource obj = work.createResource("p8:TemplateDescription");
      work.add(subj, pred, obj);
      obj = work.createProperty("owl:Thing");
      work.add(subj, pred, obj);
    }
    else if (gobj instanceof RoleQualification)
    {
      Resource subj = work.createResource(String.format("<%s>", subjId));
      Property pred = work.createProperty(rdfType);
      Resource obj = work.createResource("owl:Thing");
      work.add(subj, pred, obj);
      obj = work.createResource("p8:TemplateRoleDescription");
      work.add(subj, pred, obj);
      pred = work.createProperty("p8:hasTemplate");
      obj = work.createResource(String.format("<%s>", objectId));
      work.add(subj, pred, obj);
      pred = work.createProperty("p8:hasRoleFillerType");
      obj = work.createResource(String.format("<%s>", ((RoleQualification)gobj).getRange()));
      work.add(subj, pred, obj);
    }
    else if (gobj instanceof RoleDefinition)
    {
    	Resource subj = work.createResource(String.format("<%s>", subjId));
        Property pred = work.createProperty(rdfType);
        Resource obj = work.createResource("owl:Thing");
        work.add(subj, pred, obj);
        obj = work.createResource("p8:TemplateRoleDescription");
        work.add(subj, pred, obj);
        pred = work.createProperty("p8:hasTemplate");
        obj = work.createResource(String.format("<%s>", objectId));
        work.add(subj, pred, obj);
        pred = work.createProperty("p8:hasRoleFillerType");
        obj = work.createResource(String.format("<%s>", ((RoleDefinition)gobj).getRange()));
        work.add(subj, pred, obj);
    }
    else if (gobj instanceof TemplateQualification)
    {
      Resource subj = work.createResource(String.format("<%s>", subjId));
      Property pred = work.createProperty(rdfType);
      Resource obj = work.createResource("p8:TemplateDescription");
      work.add(subj, pred, obj);
      obj = work.createResource("owl:Thing");
      work.add(subj, pred, obj);
      obj = work.createResource("p8:CoreTemplate");
      work.add(subj, pred, obj);
      pred = work.createProperty("p8:hasSuperTemplate");
      obj = work.createResource(objectId);
      work.add(subj, pred, obj);
      subj = work.createResource(objectId);
      pred = work.createProperty("p8:hasSubTemplate");
      obj = work.createResource(String.format("<%s>", subjId));
      work.add(subj, pred, obj);
    }
    else if (gobj instanceof ClassDefinition)
    {
      Resource subj = work.createResource(String.format("rdl:%s", subjId));
      Property pred = work.createProperty(rdfType);
      Resource obj = work.createResource(objectId);
      work.add(subj, pred, obj);
      obj = work.createResource("owl:Class");
      work.add(subj, pred, obj);
    }
    return work;
  }

  private Model GenerateTypes(Model work, String subjId, String objId, Object gobj)
  {
    if (gobj instanceof TemplateDefinition)
    {
      Resource subj = work.createResource(String.format("<%s>", subjId));
      Property pred = work.createProperty(rdfType);
      Resource obj = work.createResource("tpl:R16376066707");
      work.add(subj, pred, obj);
    }
    else if (gobj instanceof RoleDefinition)
    {
      Resource subj = work.createResource(String.format("<%s>", subjId));
      Property pred = work.createProperty(rdfType);
      Resource obj = work.createResource("tpl:R74478971040");
      work.add(subj, pred, obj);
    }
    else if (gobj instanceof TemplateQualification)
    {
      Resource subj = work.createResource(String.format("<%s>",objId));
      Property pred = work.createProperty("dm:hasSubclass");
      Resource obj = work.createResource(String.format("<%s>", subjId));
      work.add(subj, pred, obj);
      subj = work.createResource(String.format("<%s>", subjId));
      pred = work.createProperty("dm:hasSuperclass");
      obj = work.createResource(String.format("<%s>",objId));
      work.add(subj, pred, obj);
    }
    else if (gobj instanceof RoleQualification)
    {
      Resource subj = work.createResource(String.format("<%s>",subjId));
      Property pred = work.createProperty(rdfType);
      Resource obj = work.createResource("tpl:R76288246068");
      work.add(subj, pred, obj);
      pred = work.createProperty("tpl:R99672026745");
      obj = work.createResource(String.format("<%s>", objId));
      work.add(subj, pred, obj);
      pred = work.createProperty(rdfType);
      obj = work.createResource("tpl:R67036823327");
      work.add(subj, pred, obj);
    }
    return work;
  }

  private Model GenerateName(Model work, Name name, String subjId, Object gobj)
  {
    Resource subj = work.createResource(String.format("<%s>", subjId));
    Property pred = work.createProperty("rdfs:label");
    Literal obj = work.createTypedLiteral(name.getValue(),
        (name.getLang() == null || name.getLang() == "") ? defaultLanguage : name.getLang());
    work.add(subj, pred, obj);
    return work;
  }

  private Model GenerateClassName(Model work, Name name, String subjId, Object gobj)
  {
    Resource subj = work.createResource(String.format("rdl:%s" + subjId));
    Property pred = work.createProperty("rdfs:label");
    Literal obj = work.createTypedLiteral(name.getValue(),
        (name.getLang() == null || name.getLang() == "") ? defaultLanguage : name.getLang());
    work.add(subj, pred, obj);
    return work;
  }

  private Model GenerateDescription(Model work, Description descr, String subjectId)
  {
    Resource subj = work.createResource(String.format("tpl:%s", subjectId));
    Property pred = work.createProperty("rdfs:comment");
    Literal obj = work.createTypedLiteral(descr.getValue(),
        (descr.getLang() == null || descr.getLang() == "") ? defaultLanguage : descr.getLang());
    work.add(subj, pred, obj);
    return work;
  }

  private Model GenerateClassDescription(Model work, Description descr, String subjectId)
  {
    Resource subj = work.createResource(String.format("rdl:%s", subjectId));
    Property pred = work.createProperty("rdfs:comment");
    Literal obj = work.createTypedLiteral(descr.getValue(),
        (descr.getLang() == null || descr.getLang() == "") ? defaultLanguage : descr.getLang());
    work.add(subj, pred, obj);
    return work;
  }

//  private Model GenerateRdfType(Model work, String subjId, String objId)
//  {
//    Resource subj = work.createResource(String.format("rdl:%s", subjId));
//    Property pred = work.createProperty("rdf:type");
//    Resource obj = work.createResource(objId);
//    work.add(subj, pred, obj);
//    return work;
//  }

  private Model GenerateRdfSubClass(Model work, String subjId, String objId)
  {
    Resource subj = work.createResource(String.format("<%s>", objId));
    Property pred = work.createProperty("rdfs:subClassOf");
    Resource obj = work.createResource(String.format("<%s>", subjId));
    work.add(subj, pred, obj);
    return work;
  }

  private Model GenerateSuperClass(Model work, String subjId, String objId)
  {
    Resource subj = work.createResource(String.format("rdl:%s", Long.parseLong(objId)));
    Property pred = work.createProperty("rdfs:subClassOf");
    Resource obj = work.createResource(subjId);
    work.add(subj, pred, obj);
    return work;
  }

  private Model GenerateDmClassification(Model work, String subjId, String objId)
  {
    Resource subj = work.createResource(String.format("<%s>", subjId));
    Property pred = work.createProperty("dm:hasClassified");
    Resource obj = work.createResource(String.format("<%s>", objId));
    work.add(subj, pred, obj);
    pred = work.createProperty("dm:hasClassifier");
    work.add(subj, pred, obj);
    return work;
  }

  private Model GenerateDmSubClass(Model work, String subjId, String objId)
  {
    Resource subj = work.createResource(String.format("<%s>", subjId));
    Property pred = work.createProperty("dm:hasSubclass");
    Resource obj = work.createResource(String.format("<%s>", objId));
    work.add(subj, pred, obj);
    return work;
  }

}