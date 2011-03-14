package org.iringtools.services.core;

import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.UnsupportedEncodingException;
import java.net.URI;
import java.net.URISyntaxException;
import java.net.URLEncoder;
import java.util.ArrayList;
import java.util.Collection;
import java.util.HashMap;
import java.util.Hashtable;
import java.util.List;
import javax.xml.bind.JAXBException;

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
import org.iringtools.common.response.Level;
import org.iringtools.common.response.Messages;
import org.iringtools.common.response.Response;
import org.iringtools.common.response.Status;
import org.iringtools.common.response.StatusList;
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
import org.iringtools.utility.HttpClient;
import org.iringtools.utility.HttpClientException;
import org.iringtools.utility.IOUtil;
import org.iringtools.utility.JaxbUtil;
import org.iringtools.utility.NamespaceMapper;
import org.iringtools.utility.NetworkCredentials;
import org.w3._2005.sparql.results.Binding;
import org.w3._2005.sparql.results.Result;
import org.w3._2005.sparql.results.Results;
import org.w3._2005.sparql.results.Sparql;
import org.iringtools.refdata.response.*;

import sun.misc.Version;

public class RefDataProvider {
	private Hashtable<String, String> _settings;
	private List<Repository> _repositories = null;
	private Queries _queries = null;
	private String defaultLanguage = "en";
	private NamespaceMapper _nsmap = null;;

	public RefDataProvider(Hashtable<String, String> settings) {
		try {
			_settings = settings;
			_repositories = getRepositories();
			_queries = getQueries();
			_nsmap = new NamespaceMapper();
		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}

	public Queries getQueries() throws JAXBException, IOException,
			FileNotFoundException {
		String path = _settings.get("baseDirectory")
				+ "/WEB-INF/data/Queries.xml";
		return JaxbUtil.read(Queries.class, path);
	}

	public Federation getFederation() throws JAXBException, IOException {
		String path = _settings.get("baseDirectory")
				+ "/WEB-INF/data/federation.xml";
		return JaxbUtil.read(Federation.class, path);
	}

	public Response saveFederation(Federation federation) throws Exception {
		Response response = new Response();
		try {
			String path = _settings.get("baseDirectory")
					+ "/WEB-INF/data/federation.xml";
			JaxbUtil.write(federation, path, true);
			response.setLevel(Level.SUCCESS);
		} catch (Exception ex) {
			response.setLevel(Level.ERROR);
			throw ex;
		}
		return response;
	}

	public Response saveNamespace(Namespace namespace, boolean deleteFlag)
			throws Exception {
		Response response = new Response();
		StatusList sl = new StatusList();
		List<Status> statuses = new ArrayList<Status>();
		Status status = new Status();
		Messages messages = new Messages();
		List<String> msgs = new ArrayList<String>();
		boolean namespaceExist = false;
		int index = 0;
		try {
			Federation federation = getFederation();
			for (Namespace ns : federation.getNamespaces().getItems()) {
				if (ns.getId().equalsIgnoreCase(namespace.getId())) {
					index = federation.getNamespaces().getItems().indexOf(ns);
					namespaceExist = true;
					break;
				}
			}
			if (namespaceExist) {
				if (deleteFlag) {
					// find out the repositories that use this namespace and
					// remove the namespace
					Integer nsID = Integer.parseInt(namespace.getId());
					for (Repository repo : federation.getRepositories()
							.getItems()) {
						if (repo.getNamespaces() != null
								&& repo.getNamespaces().getItems()
										.contains(nsID))
							repo.getNamespaces().getItems().remove(nsID);
					}
				}

				// now remove the namespace
				federation.getNamespaces().getItems().remove(index);
			} else {
				int sequenceId = federation.getNamespaces().getSequenceId();
				namespace.setId(Integer.toString(++sequenceId));
				federation.getNamespaces().setSequenceId(sequenceId);
			}
			if (!deleteFlag) {
				federation.getNamespaces().getItems().add(namespace);
			}

			String path = _settings.get("baseDirectory")
					+ "/WEB-INF/data/federation.xml";
			JaxbUtil.write(federation, path, true);

			msgs.add("Namespace saved.");
			response.setLevel(Level.SUCCESS);
		} catch (Exception ex) {
			msgs.add("Error while saving namespace.");
			response.setLevel(Level.ERROR);
			throw ex;
		}

		messages.setItems(msgs);
		status.setMessages(messages);
		statuses.add(status);
		sl.setItems(statuses);
		response.setStatusList(sl);
		return response;
	}

	public Response saveIdGenerator(IDGenerator idgenerator, boolean deleteFlag)
			throws Exception {
		Response response = new Response();
		StatusList sl = new StatusList();
		List<Status> statuses = new ArrayList<Status>();
		Status status = new Status();
		Messages messages = new Messages();
		List<String> msgs = new ArrayList<String>();
		boolean idgenExist = false;
		int index = 0;
		try {
			Federation federation = getFederation();
			for (IDGenerator idg : federation.getIdGenerators().getItems()) {
				if (idg.getId().equalsIgnoreCase(idgenerator.getId())) {
					index = federation.getIdGenerators().getItems()
							.indexOf(idg);
					idgenExist = true;
					break;
				}
			}
			if (idgenExist) {
				if (deleteFlag) {
					// find out the namespaces that use this idGenerator and
					// remove the idGenerator
					String nsID = idgenerator.getId();
					for (Namespace ns : federation.getNamespaces().getItems()) {
						if (ns.getIdGenerator().equalsIgnoreCase(nsID)) {
							ns.setIdGenerator("0");
						}
					}
				}

				// now remove the namespace
				federation.getIdGenerators().getItems().remove(index);
			} else {
				int sequenceId = federation.getIdGenerators().getSequenceId();
				idgenerator.setId(Integer.toString(++sequenceId));
				federation.getIdGenerators().setSequenceId(sequenceId);
			}
			if (!deleteFlag) {
				federation.getIdGenerators().getItems().add(idgenerator);
			}

			String path = _settings.get("baseDirectory")
					+ "/WEB-INF/data/federation.xml";
			JaxbUtil.write(federation, path, true);

			msgs.add("ID Generator saved.");
			response.setLevel(Level.SUCCESS);
		} catch (Exception ex) {
			msgs.add("Error while saving ID Generator.");
			response.setLevel(Level.ERROR);
			throw ex;
		}

		messages.setItems(msgs);
		status.setMessages(messages);
		statuses.add(status);
		sl.setItems(statuses);
		response.setStatusList(sl);
		return response;
	}

	public Response saveRepository(Repository repository, boolean deleteFlag)
			throws Exception {
		Response response = new Response();
		StatusList sl = new StatusList();
		List<Status> statuses = new ArrayList<Status>();
		Status status = new Status();
		Messages messages = new Messages();
		List<String> msgs = new ArrayList<String>();
		boolean repositoryExist = false;
		int index = 0;
		try {
			Federation federation = getFederation();
			for (Repository repo : federation.getRepositories().getItems()) {
				if (repo.getId().equalsIgnoreCase(repository.getId())) {
					index = federation.getRepositories().getItems()
							.indexOf(repo);
					repositoryExist = true;
					break;
				}
			}
			if (repositoryExist) {
				federation.getRepositories().getItems().remove(index);
			} else {
				int sequenceId = federation.getRepositories().getSequenceId();
				repository.setId(Integer.toString(++sequenceId));
				federation.getRepositories().setSequenceId(sequenceId);
			}
			if (!deleteFlag) {
				federation.getRepositories().getItems().add(repository);
			}
			String path = _settings.get("baseDirectory")
					+ "/WEB-INF/data/federation.xml";
			JaxbUtil.write(federation, path, true);

			msgs.add("Repository saved.");
			response.setLevel(Level.SUCCESS);
		} catch (Exception ex) {
			msgs.add("Error while saving Repository.");
			response.setLevel(Level.ERROR);
			throw ex;
		}

		messages.setItems(msgs);
		status.setMessages(messages);
		statuses.add(status);
		sl.setItems(statuses);
		response.setStatusList(sl);
		return response;
	}

	public String ReadSPARQL(String queryName) throws Exception {
		try {
			String path = _settings.get("baseDirectory")
					+ "/WEB-INF/data/Sparqls/";

			String query = IOUtil.readString(path + queryName);

			return query;
		} catch (Exception ex) {
			throw ex;
		}
	}

	public Version getVersion() {
		return new Version();
	}

	public String getClassLabel(String id) throws Exception {
		return getLabel(_nsmap.GetNamespaceUri("rdl").toString() + id);
	}

	public Qmxf getClass(String id, Repository repository) throws Exception {
		return getClass(id, "", repository);
	}

	public Qmxf getClass(String id) throws Exception {
		return getClass(id, "", null);
	}

	public Qmxf getClass(String id, String namespaceUrl, Repository rep)
			throws Exception {
		Qmxf qmxf = new Qmxf();

		try {
			Name name;
			Description description;
			org.ids_adi.ns.qxf.model.Status status;
			String[] names = null;

			List<Classification> classifications = new ArrayList<Classification>();
			List<Specialization> specializations = new ArrayList<Specialization>();

			// Entities resultEntities = new Entities();
			// List<Entity> resultEnt = new ArrayList<Entity>();
			String sparql = "";
			// String relativeUri = "";

			Query queryContainsSearch = getQuery("GetClass");
			// List<QueryItem> items = _queries.getItems();
			// for (QueryItem qry : items) {
			// if (qry.getKey().equals("GetClass")) {
			// queryContainsSearch = qry.getQuery();
			// break;
			// }
			// }
			QueryBindings queryBindings = queryContainsSearch.getBindings();

			sparql = ReadSPARQL(queryContainsSearch.getFileName());

			if (namespaceUrl == null || namespaceUrl.isEmpty())
				namespaceUrl = _nsmap.GetNamespaceUri("rdl").toString();

			String uri = namespaceUrl + id;

			sparql = sparql.replace("param1", uri);
			for (Repository repository : _repositories) {
				ClassDefinition classDefinition = null;

				if (rep != null)
					if (rep.getName().equals(repository.getName())) {
						continue;
					}
				Results sparqlResult = queryFromRepository(repository, sparql);

				List<Hashtable<String, String>> results = bindQueryResults(
						queryBindings, sparqlResult);

				classifications = new ArrayList<Classification>();
				specializations = new ArrayList<Specialization>();

				for (Hashtable<String, String> result : results) {
					classDefinition = new ClassDefinition();
					classDefinition.setId(uri);
					classDefinition.setRepository(repository.getName());
					name = new Name();
					description = new Description();
					status = new org.ids_adi.ns.qxf.model.Status();

					if (result.containsKey("type")) {
						URI typeName = new URI(result.get("type").substring(0,
								result.get("type").indexOf("#") + 1));
						if (typeName.toString().contains("dm")) {
							EntityType et = new EntityType();
							et.setReference(result.get("type"));
							classDefinition.setEntityType(et);
						} else if (repository.getRepositoryType().equals(
								RepositoryType.PART_8)) {
							continue;
						}
					}

					if (result.containsKey("label")) {
						names = result.get("label").split("@");
						name.setValue(names[0]);
						if (names.length == 1) {
							name.setLang(defaultLanguage);
						} else {
							name.setLang(names[names.length - 1]);
						}
					}

					// legacy properties
					if (result.containsKey("definition")) {
						names = result.get("definition").split("@");
						description.setValue(names[0]);
						if (names.length == 1) {
							description.setLang(defaultLanguage);
						} else {
							description.setLang(names[names.length - 1]);
						}
					}
					// description.value = result["definition"];

					if (result.containsKey("creator")) {
						status.setAuthority(result.get("creator"));
					}
					if (result.containsKey("creationDate")) {
						status.setFrom(result.get("creationDate"));
					}
					if (result.containsKey("class")) {
						status.setClazz(result.get("class"));
					}
					// camelot properties
					if (result.containsKey("comment")) {
						names = result.get("comment").split("@");
						description.setValue(names[0]);
						if (names.length == 1) {
							description.setLang(defaultLanguage);
						} else {
							description.setLang(names[names.length - 1]);
						}
					}
					if (result.containsKey("authority")) {
						status.setAuthority(result.get("authority"));
					}
					if (result.containsKey("recorded")) {
						status.setClazz(result.get("recorded"));
					}
					if (result.containsKey("from")) {
						status.setFrom(result.get("from"));
					}

					classDefinition.getNames().add(name);

					classDefinition.getDescriptions().add(description);
					classDefinition.getStatuses().add(status);

					classifications = getClassifications(id, repository);
					specializations = getSpecializations(id, repository);

					if (classifications.size() > 0) {
						classDefinition.setClassifications(classifications);
					}
					if (specializations.size() > 0) {
						classDefinition.setSpecializations(specializations);
					}
				}
				if (classDefinition != null) {
					qmxf.getClassDefinitions().add(classDefinition);

				}
			}

			return qmxf;
		} catch (RuntimeException e) {
			// _logger.Error("Error in GetClass: " + e);
			throw e;
		}
	}

	private List<Specialization> getSpecializations(String id, Repository rep)
			throws Exception {
		try {
			String sparql = "";
			String sparqlPart8 = "";
			String relativeUri = "";
			String[] names = null;
			Results res = null;
			List<Specialization> specializations = new ArrayList<Specialization>();
			Query queryGetSpecialization = getQuery("GetSpecialization");
			QueryBindings queryBindings = queryGetSpecialization.getBindings();
			sparql = ReadSPARQL(queryGetSpecialization.getFileName());
			sparql = sparql.replace("param1", id);
			Query queryGetSubClassOf = getQuery("GetSubClassOf");
			QueryBindings queryBindingsPart8 = queryGetSubClassOf.getBindings();
			sparqlPart8 = ReadSPARQL(queryGetSubClassOf.getFileName());
			sparqlPart8 = sparqlPart8.replace("param1", id);
			for (Repository repository : _repositories) {
				if (rep != null) {
					if (!rep.getName().equals(repository.getName())) {
						continue;
					}
				}
				if (repository.getRepositoryType() == RepositoryType.PART_8) {
					Results sparqlResults = queryFromRepository(repository,
							sparqlPart8);

					List<Hashtable<String, String>> results = bindQueryResults(
							queryBindingsPart8, sparqlResults);

					for (Hashtable<String, String> result : results) {
						Specialization specialization = new Specialization();
						String uri = "";
						String label = "";
						String lang = "";
						if (result.containsKey("uri")) {
							uri = result.get("uri");
							specialization.setReference(uri);
						}
						if (result.containsKey("label")) {
							names = result.get("label").split("@", -1);
							label = names[0];
							if (names.length == 1) {
								lang = defaultLanguage;
							} else {
								lang = names[names.length - 1];
							}
						} else {
							names = getLabel(uri).split("@", -1);
							label = names[0];
							if (names.length == 1) {
								lang = defaultLanguage;
							} else {
								lang = names[names.length - 1];
							}
						}
						specialization.setLabel(label);
						specialization.setLang(lang);
						specializations.add(specialization);
					}

				} else {
					Results sparqlResults = queryFromRepository(repository,
							sparql);

					List<Hashtable<String, String>> results = bindQueryResults(
							queryBindings, sparqlResults);

					for (Hashtable<String, String> result : results) {
						Specialization specialization = new Specialization();
						String uri = "";
						String label = "";
						String lang = "";

						if (result.containsKey("uri")) {
							uri = result.get("uri");
							specialization.setReference(uri);
						}
						if (result.containsKey("label")) {
							names = result.get("label").split("@", -1);
							label = names[0];
							if (names.length == 1) {
								lang = defaultLanguage;
							} else {
								lang = names[names.length - 1];
							}
						} else {
							label = getLabel(uri);
						}

						specialization.setLabel(label);
						specialization.setLang(lang);
						specializations.add(specialization);
					}
				}

			}
			return specializations;
		} catch (RuntimeException e) {
			// _logger.Error("Error in GetSpecializations: " + e);
			throw new RuntimeException("Error while Getting Class: " + id
					+ ".\n" + e.toString(), e);
		}
	}

	private List<Classification> getClassifications(String id, Repository rep)
			throws Exception {
		try {
			String sparql = "";
			String relativeUri = "";

			List<Classification> classifications = new ArrayList<Classification>();
			Query getClassification;
			QueryBindings queryBindings;

			for (Repository repository : _repositories) {
				if (rep != null) {
					if (rep.getName().equals(repository.getName())) {
						continue;
					}
				}
				switch (rep.getRepositoryType()) {
				case CAMELOT:
				case RDS_WIP:

					getClassification = getQuery("GetClassification");
					queryBindings = getClassification.getBindings();

					sparql = ReadSPARQL(getClassification.getFileName());
					sparql = sparql.replace("param1", id);
					classifications = processClassifications(rep, sparql,
							queryBindings);
					break;
				case PART_8:
					getClassification = getQuery("GetPart8Classification");
					queryBindings = getClassification.getBindings();

					sparql = ReadSPARQL(getClassification.getFileName());
					sparql = sparql.replace("param1", id);
					classifications = processClassifications(rep, sparql,
							queryBindings);
					break;
				}
			}

			return classifications;
		} catch (Exception e) {
			// _logger.Error("Error in GetClassifications: " + e);
			throw e;// new Exception("Error while Getting Class: " + id + ".\n"
					// + e.ToString(), e);
		}
	}

	private List<Classification> processClassifications(Repository repository,
			String sparql, QueryBindings queryBindings) throws Exception {
		Results res = null;
		Results sparqlResults = queryFromRepository(repository, sparql);

		List<Hashtable<String, String>> results = bindQueryResults(
				queryBindings, sparqlResults);
		List<Classification> classifications = new ArrayList<Classification>();

		String[] names = null;

		for (Hashtable<String, String> result : results) {

			Classification classification = new Classification();
			String uri = "";
			String label = "";
			String lang = "";

			if (result.containsKey("uri")) {
				String pref = _nsmap.GetPrefix(new URI(result.get("uri")
						.substring(0, result.get("uri").indexOf("#") + 1)));
				String uriString = result.get("uri");
				if (pref.equals("owl") || pref.equals("dm")) {
					continue;
				}
				uri = uriString;
				classification.setReference(uri);
			}

			if (result.containsKey("label")) {
				names = result.get("label").split("@");
				label = names[0];
				if (names.length == 1)
					lang = defaultLanguage;
				else
					lang = names[names.length - 1];
			} else {
				names = getLabel(uri).split("@");
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

	public final Qmxf getTemplate(String id) throws Exception {
		Qmxf qmxf = new Qmxf();

		try {
			List<TemplateQualification> templateQualifications = getTemplateQualification(
					id, null);

			if (templateQualifications.size() > 0) {
				qmxf.setTemplateQualifications(templateQualifications);
			} else {
				List<TemplateDefinition> templateDefinitions = getTemplateDefinition(
						id, null);
				qmxf.setTemplateDefinitions(templateDefinitions);
			}
		} catch (RuntimeException ex) {
			// _logger.Error("Error in GetTemplate: " + ex);
		}

		return qmxf;
	}

	public final Qmxf getTemplate(String id, String templateType, Repository rep) throws HttpClientException, Exception
	{
		Qmxf qmxf = new Qmxf();
		List<TemplateQualification> templateQualification = null;
		List<TemplateDefinition> templateDefinition = null;
		try
		{
			if (templateType.equals("Qualification"))
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
			//_logger.Error("Error in GetTemplate: " + ex);
		}

		return qmxf;
	}
	
	private List<TemplateDefinition> getTemplateDefinition(String id, Repository rep) throws Exception {
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

			sparql = ReadSPARQL(queryContainsSearch.getFileName());
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
			//_logger.Error("Error in GetTemplateDefinition: " + e);
			throw new RuntimeException("Error while Getting Class: " + id + ".\n" + e.toString(), e);
		}
	}

	private List<RoleDefinition> getRoleDefinition(String id, Repository repository) throws Exception {
		   try
			{
				String sparql = "";
				String relativeUri = "";
				String sparqlQuery = "";
				String[] names = null;

				Description description = new Description();
				org.ids_adi.ns.qxf.model.Status status = new org.ids_adi.ns.qxf.model.Status();

				List<RoleDefinition> roleDefinitions = new ArrayList<RoleDefinition>();
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

				sparql = ReadSPARQL(queryContainsSearch.getFileName());
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
//					Utility.SearchAndInsert(roleDefinitions, roleDefinition, RoleDefinition.sortAscending());
				}

				return roleDefinitions;
			}
			catch (RuntimeException e)
			{
				//_logger.Error("Error in GetRoleDefinition: " + e);
				throw new RuntimeException("Error while Getting Class: " + id + ".\n" + e.toString(), e);
			}
		}

	private List<TemplateQualification> getTemplateQualification(String id,
			Repository rep) throws Exception, HttpClientException {
		TemplateQualification templateQualification = null;
		List<TemplateQualification> templateQualificationList = new ArrayList<TemplateQualification>();
		String[] names = null;

		try {
			String sparql = "";
			String relativeUri = "";
			String sparqlQuery = "";

			List<Entity> resultEntities = new ArrayList<Entity>();

			Query getTemplateQualification = null;
			QueryBindings queryBindings = null;

			{
				for (Repository repository : _repositories) {
					if (rep != null) {
						if (!rep.getName().equals(repository.getName())) {
							continue;
						}
					}

					switch (repository.getRepositoryType()) {
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

					sparql = ReadSPARQL(getTemplateQualification.getFileName());
					sparql = sparql.replace("param1", id);

					Results sparqlResults = queryFromRepository(repository,
							sparql);

					List<Hashtable<String, String>> results = bindQueryResults(
							queryBindings, sparqlResults);
					for (Hashtable<String, String> result : results) {
						templateQualification = new TemplateQualification();
						Description description = new Description();
						org.ids_adi.ns.qxf.model.Status status = new org.ids_adi.ns.qxf.model.Status();
						Name name = new Name();

						templateQualification.setRepository(repository
								.getName());

						if (result.containsKey("name")) {
							names = result.get("name").split("@", -1);
							name.setValue(names[0]);
						}
						if (names.length == 1) {
							name.setLang(defaultLanguage);
						} else {
							name.setLang(names[names.length - 1]);
						}

						if (result.containsKey("description")) {
							names = result.get("description").split("@", -1);
							description.setValue(names[0]);
						}
						if (names.length == 1) {
							description.setLang(defaultLanguage);
						} else {
							description.setLang(names[names.length - 1]);
						}
						if (result.containsKey("statusClass")) {
							status.setClazz(result.get("statusClass"));
						}
						if (result.containsKey("statusAuthority")) {
							status.setAuthority(result.get("statusAuthority"));
						}
						if (result.containsKey("qualifies")) {
							templateQualification.setQualifies(result
									.get("qualifies"));
						}

						templateQualification.setId(id);
						templateQualification.getNames().add(name);
						templateQualification.getDescriptions()
								.add(description);
						templateQualification.getStatuses().add(status);

						templateQualification.getRoleQualifications().addAll(
								getRoleQualification(id, repository));
						templateQualificationList.add(templateQualification);
					}
				}
			}
			return templateQualificationList;
		} catch (RuntimeException e) {
			// _logger.Error("Error in GetTemplateQualification: " + e);
			throw new RuntimeException("Error while Getting Template: " + id
					+ ".\n" + e.toString(), e);
		}
	}

	private List<RoleQualification> getRoleQualification(String id,
			Repository rep) throws Exception {
		try {
			String rangeSparql = "";
			String relativeUri = "";

			String[] names = null;

			String referenceSparql = "";
			String relativeUri1 = "";

			String valueSparql = "";
			String relativeUri2 = "";

			Description description = new Description();
			org.ids_adi.ns.qxf.model.Status status = new org.ids_adi.ns.qxf.model.Status();

			List<RoleQualification> roleQualifications = new ArrayList<RoleQualification>();

			for (Repository repository : _repositories) {
				if (rep != null) {
					if (!rep.getName().equals(repository.getName())) {
						continue;
					}
				}
				switch (rep.getRepositoryType()) {
				case CAMELOT:
				case RDS_WIP:

					List<Entity> rangeResultEntities = new ArrayList<Entity>();
					List<Entity> referenceResultEntities = new ArrayList<Entity>();
					List<Entity> valueResultEntities = new ArrayList<Entity>();

					Query getRangeRestriction = getQuery("GetRangeRestriction");
					QueryBindings rangeRestrictionBindings = getRangeRestriction
							.getBindings();

					Query getReferenceRestriction = getQuery("GetReferenceRestriction");
					QueryBindings referenceRestrictionBindings = getReferenceRestriction
							.getBindings();

					Query getValueRestriction = getQuery("GetValueRestriction");
					QueryBindings valueRestrictionBindings = getValueRestriction
							.getBindings();

					rangeSparql = ReadSPARQL(getRangeRestriction.getFileName());
					rangeSparql = rangeSparql.replace("param1", id);

					referenceSparql = ReadSPARQL(getReferenceRestriction
							.getFileName());
					referenceSparql = referenceSparql.replace("param1", id);

					valueSparql = ReadSPARQL(getValueRestriction.getFileName());
					valueSparql = valueSparql.replace("param1", id);
					Results rangeSparqlResults = queryFromRepository(
							repository, rangeSparql);
					Results referenceSparqlResults = queryFromRepository(
							repository, referenceSparql);
					Results valueSparqlResults = queryFromRepository(
							repository, valueSparql);

					List<Hashtable<String, String>> rangeBindingResults = bindQueryResults(
							rangeRestrictionBindings, rangeSparqlResults);
					List<Hashtable<String, String>> referenceBindingResults = bindQueryResults(
							referenceRestrictionBindings,
							referenceSparqlResults);
					List<Hashtable<String, String>> valueBindingResults = bindQueryResults(
							valueRestrictionBindings, valueSparqlResults);

					List<Hashtable<String, String>> combinedResults = mergeLists(
							mergeLists(rangeBindingResults,
									referenceBindingResults),
							valueBindingResults);

					for (Hashtable<String, String> combinedResult : combinedResults) {

						RoleQualification roleQualification = new RoleQualification();
						String uri = "";
						if (combinedResult.containsKey("qualifies")) {
							uri = combinedResult.get("qualifies");
							roleQualification.setQualifies(uri);
							roleQualification.setId(getIdFromURI(uri));
						}
						if (combinedResult.containsKey("name")) {
							String nameValue = combinedResult.get("name");

							if (nameValue == null) {
								nameValue = getLabel(uri);
							}
							names = nameValue.split("@", -1);

							Name name = new Name();
							if (names.length > 1) {
								name.setLang(names[names.length - 1]);
							} else {
								name.setLang(defaultLanguage);
							}

							name.setValue(names[0]);

							roleQualification.getNames().add(name);
						} else {
							String nameValue = getLabel(uri);

							if (nameValue.equals("")) {
								nameValue = "tpl:" + getIdFromURI(uri);
							}

							Name name = new Name();
							names = nameValue.split("@", -1);

							if (names.length > 1) {
								name.setLang(names[names.length - 1]);
							} else {
								name.setLang(defaultLanguage);
							}
							name.setValue(names[0]);

							roleQualification.getNames().add(name);
						}
						if (combinedResult.containsKey("range")) {
							roleQualification.setRange(combinedResult
									.get("range"));
						}
						if (combinedResult.containsKey("reference")) {
							org.ids_adi.ns.qxf.model.Value tempVar = new org.ids_adi.ns.qxf.model.Value();
							tempVar.setReference(combinedResult
									.get("reference"));
							org.ids_adi.ns.qxf.model.Value value = tempVar;

							roleQualification.setValue(value);
						}
						if (combinedResult.containsKey("value")) {
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
					QueryBindings getPart8RolesBindings = getPart8Roles
							.getBindings();

					String part8RolesSparql = ReadSPARQL(getPart8Roles
							.getFileName());
					part8RolesSparql = part8RolesSparql.replace("param1", id);
					Results part8RolesResults = queryFromRepository(repository,
							part8RolesSparql);
					List<Hashtable<String, String>> part8RolesBindingResults = bindQueryResults(
							getPart8RolesBindings, part8RolesResults);
					for (Hashtable<String, String> result : part8RolesBindingResults) {
						if (result.containsKey("comment")) {
						}

						if (result.containsKey("type")) {
						}

						if (result.containsKey("label")) {
						}

						if (result.containsKey("index")) {
						}

						if (result.containsKey("role")) {
						}
						RoleQualification roleQualification = new RoleQualification();
					}
					break;
				}
			}
			return roleQualifications;
		} catch (RuntimeException e) {
			// _logger.Error("Error in GetRoleQualification: " + e);
			throw new RuntimeException("Error while Getting Class: " + id
					+ ".\n" + e.toString(), e);
		}
	}

	private String getIdFromURI(String uri) {
		String id = uri;
		if (uri != null || !uri.isEmpty()) {
			if (id.contains("#")) {
				id = id.substring(id.lastIndexOf("#") + 1);
			} else if (id.contains(":")) {
				id = id.substring(id.lastIndexOf(":") + 1);
			}
		}
		if (id == null) {
			id = "";
		}
		return id;
	}

	private List<Hashtable<String, String>> mergeLists(
			List<Hashtable<String, String>> a, List<Hashtable<String, String>> b) {
		try {
			for (Hashtable<String, String> dictionary : b) {
				a.add(dictionary);
			}
			return a;
		} catch (RuntimeException ex) {
			throw ex;
		}
	}

	public Response postTemplate(Qmxf qmxf) {
		return new Response();
	}

	public Response postClass(Qmxf qmxf) {
		return new Response();
	}

	public List<Repository> getRepositories() throws Exception {
		List<Repository> repositoryList;
		try {
			Federation federation = getFederation();
			repositoryList = new ArrayList<Repository>();
			for (Repository repo : federation.getRepositories().getItems()) {
				repositoryList.add(repo);
			}
		} catch (Exception ex) {
			throw ex;
		}
		return repositoryList;

	}

	public Entities GetClassTemplates(String id) throws Exception
	{
		Entities queryResult = new Entities();
		String[] names = null;
		String language = "";
		try
		{
			String sparqlGetClassTemplates = "";
			String sparqlGetRelatedTemplates = "";
			String relativeUri = "";
			Query queryGetClassTemplates = getQuery("GetClassTemplates");
			QueryBindings queryBindingsGetClassTemplates = queryGetClassTemplates.getBindings();

			sparqlGetClassTemplates = ReadSPARQL(queryGetClassTemplates.getFileName());
			sparqlGetClassTemplates = sparqlGetClassTemplates.replace("param1", id);

			Query queryGetRelatedTemplates = getQuery("GetRelatedTemplates");
			QueryBindings queryBindingsGetRelatedTemplates = queryGetRelatedTemplates.getBindings();

			sparqlGetRelatedTemplates = ReadSPARQL(queryGetRelatedTemplates.getFileName());
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

						//Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
						queryResult.getItems().add(resultEntity);                        
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

						//Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
						queryResult.getItems().add(resultEntity);
					}
				}
			}
		}
		catch (RuntimeException e)
		{
			//_logger.Error("Error in GetClassTemplates: " + e);
			throw new RuntimeException("Error while Finding " + id + ".\n" + e.toString(), e);
		}
		return queryResult;
	}

	public Entities search(String query) {
		return null;
	}

	public Entities searchPage(String query, String start, String limit) {
		return null;
	}

	public Entities searchReset(String query) {
		return null;
	}

	public Entities getSuperClasses(String id) throws Exception
	{
		Entities queryResult = new Entities();
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
					names = getLabel(uri).split("@");
					label = names[0];
				}
				if(names.length == 1){
					tempVar.setLang(defaultLanguage);
					
				} else if(names.length == 2) {
					tempVar.setLang(names[names.length-1]);
				}					
				tempVar.setUri(uri);
				tempVar.setLabel(label);
				Entity resultEntity = tempVar;
				queryResult.getItems().add(resultEntity);
//				Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
			}
		}
		catch (RuntimeException e)
		{
			//_logger.Error("Error in GetSuperClasses: " + e);
			throw new RuntimeException("Error while Finding " + id + ".\n" + e.toString(), e);
		}
		return queryResult;
	}

	public Entities getSubClasses(String id) throws Exception
	{
		Entities queryResult = new Entities();

		try
		{
			String sparql = "";
			String sparqlPart8 = "";
			String relativeUri = "";
			String language = "";
			String[] names = null;

			Query queryGetSubClasses = getQuery("GetSubClasses");
			QueryBindings queryBindings = queryGetSubClasses.getBindings();

			sparql = ReadSPARQL(queryGetSubClasses.getFileName());
			sparql = sparql.replace("param1", id);

			Query queryGetSubClassOfInverse = getQuery("GetSubClassOfInverse");
			QueryBindings queryBindingsPart8 = queryGetSubClassOfInverse.getBindings();

			sparqlPart8 = ReadSPARQL(queryGetSubClassOfInverse.getFileName());
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
						Entity resultEntity = tempVar;
						queryResult.getItems().add(tempVar);
						//Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
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
						queryResult.getItems().add(resultEntity);
//						Utility.SearchAndInsert(queryResult, resultEntity, Entity.sortAscending());
					}
				}
			}
		}
		catch (RuntimeException e)
		{
//			_logger.Error("Error in GetSubClasses: " + e);
			throw new RuntimeException("Error while Finding " + id + ".\n" + e.toString(), e);
		}
		return queryResult;
	}
	
	public Entities getAllSuperClasses(String id) throws Exception
	{
		Entities list = new Entities();
		return getAllSuperClasses(id, list);
	}

	public Entities getAllSuperClasses(String id, Entities list) throws Exception
	{
		String[] names = null;
		try
		{
			List<Specialization> specializations = getSpecializations(id, null);
			//base case
			if (specializations.isEmpty())
			{
				return list;
			}

			for (Specialization specialization : specializations)
			{
				String uri = specialization.getReference();
				String label = specialization.getLabel();
				String language = "";

				if (label == null)
				{
					names = getLabel(uri).split("[@]", -1);
					label = names[0];
					if (names.length == 1)
					{
						language = defaultLanguage;
					}
					else
					{
						language = names[names.length - 1];
					}
				} else {
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
				for (Entity entt : list.getItems())
				{
					if (resultEntity.getUri().equals(entt.getUri()))
					{
						found = true;
					}
				}

				if (!found)
				{
					trimmedUri = uri.substring(0, 0) + uri.substring(0 + uri.lastIndexOf('#') + 1);
					list.getItems().add(resultEntity);
					//Utility.SearchAndInsert(list, resultEntity, Entity.sortAscending());
					getAllSuperClasses(trimmedUri, list);
				}
			}
		}
		catch (RuntimeException e)
		{
			//_logger.Error("Error in GetAllSuperClasses: " + e);
			throw new RuntimeException("Error while Finding " + id + ".\n" + e.toString(), e);
		}

		return list;
	}
	
	private Response queryIdGenerator(String serviceUrl)
			throws HttpClientException {
		Response result = null;
		HttpClient httpClient = null;
		try {
			String uri = _settings.get("idGenServiceUri");
			httpClient = new HttpClient(uri);
		} catch (Exception e) {
			System.out.println("Exception in IDGenServiceUri :" + e);
		}
		result = httpClient.get(Response.class, serviceUrl);
		return result;
	}

	private String createIdsAdiId(String uri, String comment) {
		String idsAdiId = "";
		Response responseText = null;
		try {
			String serviceUrl = "/acquireId/param?uri=" + uri + "&comment="
					+ comment;
			responseText = queryIdGenerator(serviceUrl);
			Messages messages = responseText.getMessages();
			List<String> messageList = messages.getItems();
			if (messageList != null) {
				idsAdiId = messageList.get(0);
			}
		} catch (Exception e) {
			// logger.Error("Error in createIdsAdiId: " + e);
			System.out.println("Error in createIdsAdiId: " + e);
		}
		return idsAdiId;
	}

	public String getLabel(String uri) throws Exception {
		String label = "";
		Query query = new Query();
		String sparql = "";
		List<QueryItem> items = _queries.getItems();
		for (QueryItem qry : items) {
			if (qry.getKey().contains("GetLabel")) {
				query = qry.getQuery();
				break;
			}
		}
		QueryBindings queryBindings = query.getBindings();
		sparql = ReadSPARQL(query.getFileName());
		sparql = sparql.replace("param1", uri);
		for (Repository repository : _repositories) {
			Results sparqlResults = queryFromRepository(repository, sparql);

			List<Hashtable<String, String>> results = bindQueryResults(
					queryBindings, sparqlResults);
			for (Hashtable<String, String> result : results) {
				if (result.containsKey("label")) {
					label = result.get("label");
				}
			}
		}
		return label;
	}
	
	private List<RoleDefinition> getRoleDefinition(String id) throws Exception, HttpClientException
	{
		try
		{
			String sparql = "";
			String relativeUri = "";
			String sparqlQuery = "";
			String[] names = null;

			Description description = new Description();
			org.ids_adi.ns.qxf.model.Status status = new org.ids_adi.ns.qxf.model.Status();

			java.util.ArrayList<RoleDefinition> roleDefinitions = new java.util.ArrayList<RoleDefinition>();

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

				sparql = ReadSPARQL(queryContainsSearch.getFileName());
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
//					Utility.SearchAndInsert(roleDefinitions, roleDefinition, RoleDefinition.sortAscending());
					//roleDefinitions.Add(roleDefinition);
				}
			}

			return roleDefinitions;
		}
		catch (RuntimeException e)
		{
	//		_logger.Error("Error in GetRoleDefinition: " + e);
			throw new RuntimeException("Error while Getting Class: " + id + ".\n" + e.toString(), e);
		}
	}
	

	private List<Hashtable<String, String>> bindQueryResults(
			QueryBindings queryBindings, Results sparqlResults) {
		String sBinding = "";
		String qBinding = "";
		try {
			List<Hashtable<String, String>> results = new ArrayList<Hashtable<String, String>>();
			for (Result sparqlResult : sparqlResults.getResults()) {
				Hashtable<String, String> result = new Hashtable<String, String>();
				String sortKey = "";
				for (Binding sparqlBinding : sparqlResult.getBindings()) {
					sBinding = sparqlBinding.getName();
					for (QueryBinding queryBinding : queryBindings.getItems()) {
						qBinding = queryBinding.getName();
						if (sBinding.equals(qBinding)) {
							String bindingKey = qBinding;
							String bindingValue = "";
							String dataType = "";
							if (queryBinding.getType() == SPARQLBindingType.URI) {
								bindingValue = sparqlBinding.getUri();
							} else if (queryBinding.getType() == SPARQLBindingType.LITERAL) {
								bindingValue = sparqlBinding.getLiteral()
										.getContent();
								dataType = sparqlBinding.getLiteral()
										.getDatatype();
								sortKey = bindingValue;
							}
							if (result.containsKey(bindingKey)) {
								bindingKey = makeUniqueKey(result, bindingKey);
							}
							result.put(bindingKey, bindingValue);

							if (dataType != null && !dataType.isEmpty()) {
								result.put(bindingKey + "_dataType",
										bindingValue);
							}
						}
					}
				}
				results.add(result);
			}
			return results;
		} catch (RuntimeException ex) {
			throw ex;
		}
	}

	private String makeUniqueKey(Hashtable<String, String> hashtable,
			String duplicateKey) {
		try {
			String newKey = "";
			for (int i = 2; i < Integer.MAX_VALUE; i++) {
				String postFix = " (" + (new Integer(i)).toString() + ")";
				if (!hashtable.containsKey(duplicateKey + postFix)) {
					newKey += postFix;
					break;
				}
			}
			return newKey;
		} catch (RuntimeException ex) {
			throw ex;
		}
	}

	private Results queryFromRepository(Repository repository, String sparql)
			throws HttpClientException, UnsupportedEncodingException {
		Sparql sparqlResults = new Sparql();
		Results results = new Results();
		String message = "query=" + URLEncoder.encode(sparql, "UTF-8");
		try {
			// TODO need to look at credentials
			NetworkCredentials credentials = new NetworkCredentials();
			HttpClient sparqlClient = new HttpClient(repository.getUri());
			sparqlClient.setNetworkCredentials(credentials);
			sparqlResults = sparqlClient.PostMessage(Sparql.class, "", message);
		
			results = sparqlResults.getResults();
		} catch (RuntimeException ex) {
			return results = null;
		}
		return results;
	}

	private Query getQuery(String queryName) {
		Query query = null;
		List<QueryItem> items = _queries.getItems();
		for (QueryItem qry : items) {
			if (qry.getKey().equals(queryName)) {
				query = qry.getQuery();
				break;
			}
		}
		return query;
	}

}