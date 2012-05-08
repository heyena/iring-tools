package org.iringtools.services.core;

import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.UnsupportedEncodingException;
import java.net.URI;
import java.net.URISyntaxException;
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
import org.ids_adi.ns.qxf.model.Value;
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
import org.iringtools.utility.HttpProxy;
import org.iringtools.utility.IOUtils;
import org.iringtools.utility.JaxbUtils;
import org.iringtools.utility.NamespaceMapper;
import org.iringtools.utility.NetworkCredentials;
import org.iringtools.utility.ReferenceObject;
import org.iringtools.utility.Utility;
import org.w3._2005.sparql.results.Binding;
import org.w3._2005.sparql.results.Result;
import org.w3._2005.sparql.results.Results;
import org.w3._2005.sparql.results.Sparql;

import com.hp.hpl.jena.graph.TripleIterator;
import com.hp.hpl.jena.rdf.model.Model;
import com.hp.hpl.jena.rdf.model.Literal;
import com.hp.hpl.jena.rdf.model.Property;
import com.hp.hpl.jena.rdf.model.RDFNode;
import com.hp.hpl.jena.rdf.model.Resource;
import com.hp.hpl.jena.rdf.model.ModelFactory;
import com.hp.hpl.jena.rdf.model.Statement;
import com.hp.hpl.jena.rdf.model.StmtIterator;
import com.hp.hpl.jena.shared.PrefixMapping;

public class RefDataProvider {
	private Map<String, Object> _settings;
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
	private static final Logger logger = Logger
			.getLogger(RefDataProvider.class);
	private String qName = null;
	private final String rdfssubClassOf = "rdfs:subClassOf";
	private final String rdfType = "rdf:type";

	private StringBuilder sparqlStr = new StringBuilder();

	public RefDataProvider(Map<String, Object> settings) {
		try {
			_settings = settings;
			if(!_settings.containsKey("baseDirectory")){
			}			
			_repositories = getRepositories();
			_queries = getQueries();
			_nsmap = new NamespaceMapper(false);
		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}

	public Queries getQueries() throws JAXBException, IOException,
			FileNotFoundException {
		String path = _settings.get("baseDirectory")
				+ "/WEB-INF/data/Queries.xml";
		return JaxbUtils.read(Queries.class, path);
	}

	public Federation getFederation() throws JAXBException, IOException {
		String path = _settings.get("baseDirectory") + "/WEB-INF/data/federation.xml";
		return JaxbUtils.read(Federation.class, path);
	}

	public Response saveFederation(Federation federation) throws Exception {
		Response response = new Response();
		try {
			String path = _settings.get("baseDirectory") + "/WEB-INF/data/federation.xml";
			JaxbUtils.write(federation, path, true);
			response.setLevel(Level.SUCCESS);
		} catch (Exception ex) {
			response.setLevel(Level.ERROR);
			throw ex;
		}
		return response;
	}

	public Response saveNamespace(Namespace namespace, boolean deleteFlag) throws Exception {
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
					for (Repository repo : federation.getRepositories().getItems()) {
						if (repo.getNamespaces() != null && repo.getNamespaces().getItems().contains(nsID))
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

			String path = _settings.get("baseDirectory") + "/WEB-INF/data/federation.xml";
			JaxbUtils.write(federation, path, true);

			msgs.add("Namespace saved.");
			response.setLevel(Level.SUCCESS);
		} catch (Exception ex) {
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

	public Response saveIdGenerator(IDGenerator idgenerator, boolean deleteFlag) throws Exception {
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
					index = federation.getIdGenerators().getItems().indexOf(idg);
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

			String path = _settings.get("baseDirectory") + "/WEB-INF/data/federation.xml";
			JaxbUtils.write(federation, path, true);

			msgs.add("ID Generator saved.");
			response.setLevel(Level.SUCCESS);
		} catch (Exception ex) {
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

	public Response saveRepository(Repository repository, boolean deleteFlag) throws Exception {
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
					index = federation.getRepositories().getItems().indexOf(repo);
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
			String path = _settings.get("baseDirectory") + "/WEB-INF/data/federation.xml";
			JaxbUtils.write(federation, path, true);

			msgs.add("Repository saved.");
			response.setLevel(Level.SUCCESS);
		} catch (Exception ex) {
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

	public String readSparql(String queryName) throws Exception {
		try {
			String path = _settings.get("baseDirectory") + "/WEB-INF/data/Sparqls/";

			String query = IOUtils.readString(path + queryName);

			return query;
		} catch (Exception ex) {
			logger.error(ex);
			return "";
		}
	}

	public Version getVersion() {
		return new Version();
	}

	public Entity getClassLabel(String id) throws Exception {
		return getLabel(_nsmap.getNamespaceUri("rdl").toString() + id);
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
			String sparql = "";
			// String relativeUri = "";

			Query queryContainsSearch = getQuery("GetClass");

			QueryBindings queryBindings = queryContainsSearch.getBindings();

			sparql = readSparql(queryContainsSearch.getFileName());

			if (namespaceUrl == null || namespaceUrl.isEmpty())
				namespaceUrl = _nsmap.getNamespaceUri("rdl").toString();

			String uri = "<" + namespaceUrl + id + ">";
			//String uri = namespaceUrl + id;

			sparql = sparql.replace("param1", uri);
			for (Repository repository : _repositories) {
				ClassDefinition classDefinition = null;

				if (rep != null)
					if (rep.getName() != repository.getName()) {
						continue;
					}
				Results sparqlResult = queryFromRepository(repository, sparql);
				List<Hashtable<String, String>> results = bindQueryResults(queryBindings, sparqlResult);

				classifications = new ArrayList<Classification>();
				specializations = new ArrayList<Specialization>();

				for (Hashtable<String, String> result : results) {
					classDefinition = new ClassDefinition();
					classDefinition.setId(uri.replace("<", "").replace(">",""));
					classDefinition.setRepository(repository.getName());
					name = new Name();
					description = new Description();
					status = new org.ids_adi.ns.qxf.model.Status();

					if (result.containsKey("type")) {
						URI typeName = new URI(result.get("type").substring(0, result.get("type").indexOf("#") + 1));
						if (_nsmap.getPrefix(typeName).equals("dm")) {
							EntityType et = new EntityType();
							et.setReference(result.get("type"));
							classDefinition.setEntityType(et);
						} else if (repository.getRepositoryType().equals(RepositoryType.PART_8)) {
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

					if (result.containsKey("creator")) {
						status.setAuthority(result.get("creator"));
					}
					if (result.containsKey("creationDate")) {
						status.setFrom(result.get("creationDate"));
					}
					if (result.containsKey("class")) {
						status.setClazz(result.get("class"));
					}

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
			logger.error("Error in GetClass: " + e);
			return qmxf;
		}
	}

	private List<Specialization> getSpecializations(String id, Repository rep)
			throws Exception {
		List<Specialization> specializations = new ArrayList<Specialization>();
		try {
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
			for (Repository repository : _repositories) {
				if (rep != null) {
					if (rep.getName() != repository.getName()) {
						continue;
					}
				}
				if (repository.getRepositoryType() == RepositoryType.PART_8) {
					Results sparqlResults = queryFromRepository(repository, sparqlPart8);

					List<Hashtable<String, String>> results = bindQueryResults(queryBindingsPart8, sparqlResults);

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
							names = getLabel(uri).getLabel().split("@", -1);
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
					Results sparqlResults = queryFromRepository(repository, sparql);
					List<Hashtable<String, String>> results = bindQueryResults(queryBindings, sparqlResults);

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
							label = getLabel(uri).getLabel();
						}

						specialization.setLabel(label);
						specialization.setLang(lang);
						specializations.add(specialization);
					}
				}

			}
			return specializations;
		} catch (RuntimeException e) {
			logger.error("Error while Getting Class: " + e);
			return specializations;
		}
	}

	private List<Classification> getClassifications(String id, Repository rep)
			throws Exception {
		List<Classification> classifications = new ArrayList<Classification>();
		try {
			String sparql = "";
			String relativeUri = "";
			Query getClassification;
			QueryBindings queryBindings;

			for (Repository repository : _repositories) {
				if (rep != null) {
					if (rep.getName() != repository.getName()) {
						continue;
					}
				}
				switch (rep.getRepositoryType()) {
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
		} catch (Exception e) {
			logger.error("Error in GetClassifications: " + e);
			return classifications;
		}
	}

	private List<Classification> processClassifications(Repository repository,
			String sparql, QueryBindings queryBindings) throws Exception {
		// Results res = null;
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
				String pref = _nsmap.getPrefix(new URI(result.get("uri").substring(0, result.get("uri").indexOf("#") + 1)));
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

	public final Qmxf getTemplate(String id) throws Exception {
		Qmxf qmxf = new Qmxf();

		try {
			List<TemplateQualification> templateQualifications = getTemplateQualification(id, null);
			if (templateQualifications.size() > 0) {
				qmxf.setTemplateQualifications(templateQualifications);
			} else {
				List<TemplateDefinition> templateDefinitions = getTemplateDefinition(id, null);
				qmxf.setTemplateDefinitions(templateDefinitions);
			}
		} catch (RuntimeException ex) {
			logger.error("Error in GetTemplate: " + ex);
			return qmxf;
		}
		return qmxf;
	}

	@SuppressWarnings("unchecked")
	public final org.iringtools.refdata.response.Response getClassMembers(String Id) {
		org.iringtools.refdata.response.Response membersResult = new org.iringtools.refdata.response.Response();
		Entities entities = new Entities();
		membersResult.setEntities(entities);
		List<Entity> entityList = new ArrayList<Entity>();
		entities.setItems(entityList);
		try {
			String sparql = "";
			String language = "";
			String[] names = null;

			Query getMembers = getQuery("GetMembers");
			QueryBindings memberBindings = getMembers.getBindings();
			sparql = readSparql(getMembers.getFileName());
			sparql = sparql.replace("param1", Id);

			for (Repository repository : _repositories) {
				Results sparqlResults = queryFromRepository(repository, sparql);

				List<Hashtable<String, String>> results = bindQueryResults(memberBindings, sparqlResults);

				for (Hashtable<String, String> result : results) {
					names = result.get("label").split("@");
					if (names.length == 1) {
						language = defaultLanguage;
					} else {
						language = names[names.length - 1];
					}
					Entity resultEntity = new Entity();
					resultEntity.setUri(result.get("uri"));
					resultEntity.setLabel(names[0]);
					resultEntity.setLang(language);
					resultEntity.setRepository(repository.getName());

					entityList.add(resultEntity);
					Utility.searchAndInsert((List)membersResult, resultEntity, Entity.sortAscending());
				} // queryResult.Add(resultEntity);
				membersResult.setTotal(entityList.size());
			}
		} catch (Exception ex) {
			logger.error("Error in Getmembers: " + ex);
		}
		return membersResult;
	}

	public final Qmxf getTemplate(String id, String templateType, Repository rep) throws HttpClientException, Exception {
		Qmxf qmxf = new Qmxf();
		List<TemplateQualification> templateQualification = null;
		List<TemplateDefinition> templateDefinition = null;
		try {
			if (templateType.equalsIgnoreCase("Qualification")) {
				templateQualification = getTemplateQualification(id, rep);
			} else {
				templateDefinition = getTemplateDefinition(id, rep);
			}

			if (templateQualification != null) {
				qmxf.getTemplateQualifications().addAll(templateQualification);
			} else {
				qmxf.getTemplateDefinitions().addAll(templateDefinition);
			}
		} catch (RuntimeException ex) {
			logger.error("Error in GetTemplate: " + ex);
			return qmxf;
		}

		return qmxf;
	}

	private List<TemplateDefinition> getTemplateDefinition(String id, Repository rep) throws Exception {
		List<TemplateDefinition> templateDefinitionList = new ArrayList<TemplateDefinition>();
		TemplateDefinition templateDefinition = null;

		try {
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
			for (Repository repository : _repositories) {
				if (rep != null) {
					if (rep.getName() != repository.getName()) {
						continue;
					}
				}
				Results sparqlResults = queryFromRepository(repository, sparql);
				List<Hashtable<String, String>> results = bindQueryResults(queryBindings, sparqlResults);

				for (Hashtable<String, String> result : results) {
					templateDefinition = new TemplateDefinition();
					Name name = new Name();

					templateDefinition.setRepository(repository.getName());

					if (result.containsKey("label")) {
						names = result.get("label").split("@", -1);
						name.setValue(names[0]);
						if (names.length == 1) {
							name.setLang(defaultLanguage);
						} else {
							name.setLang(names[names.length - 1]);
						}
					}

					if (result.containsKey("definition")) {
						names = result.get("definition").split("@", -1);
						description.setValue(names[0]);
						if (names.length == 1) {
							description.setLang(defaultLanguage);
						} else {
							description.setLang(names[names.length - 1]);
						}
					}

					if (result.containsKey("creationDate")) {
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
		} catch (RuntimeException e) {
			logger.error("Error in GetTemplateDefinition: " + e);
			return templateDefinitionList;
		}
	}

	private List<RoleDefinition> getRoleDefinition(String id, Repository repository) throws Exception {

		List<RoleDefinition> roleDefinitions = new ArrayList<RoleDefinition>();
		try {
			String sparql = "";
			String relativeUri = "";
			String sparqlQuery = "";
			String[] names = null;

			Description description = new Description();
			org.ids_adi.ns.qxf.model.Status status = new org.ids_adi.ns.qxf.model.Status();

			List<Entity> resultEntities = new ArrayList<Entity>();

			switch (repository.getRepositoryType()) {
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

			for (Hashtable<String, String> result : results) {
				RoleDefinition roleDefinition = new RoleDefinition();
				Name name = new Name();

				if (result.containsKey("label")) {
					names = result.get("label").split("@", -1);
					name.setValue(names[0]);
					if (names.length == 1) {
						name.setLang(defaultLanguage);
					} else {
						name.setLang(names[names.length - 1]);
					}
				}
				if (result.containsKey("role")) {
					roleDefinition.setId(result.get("role"));
				}
				if (result.containsKey("comment")) {
					names = result.get("comment").split("@", -1);
					description.setValue(names[0]);

					if (names.length == 1) {
						description.setLang(defaultLanguage);
					} else {
						description.setLang(names[names.length - 1]);
					}
				}
				if (result.containsKey("index")) {
					description.setValue(result.get("index").toString());
				}
				if (result.containsKey("type")) {
					roleDefinition.setRange(result.get("type"));
				}
				roleDefinition.getNames().add(name);
				roleDefinition.getDescriptions().add(description);
				roleDefinitions.add(roleDefinition);
				Utility.searchAndInsert(roleDefinitions, roleDefinition, RoleDefinition.sortAscending());
			}

			return roleDefinitions;
		} catch (RuntimeException e) {
			logger.error("Error in GetRoleDefinition: " + e);
			return roleDefinitions;
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
						if (rep.getName() != repository.getName()) {
							continue;
						}
					}

					switch (repository.getRepositoryType()) {
					case CAMELOT:
					case RDS_WIP:
						sparqlQuery = "GetTemplateQualification";
						break;
					case PART_8:
						sparqlQuery = "GetTemplateQualificationPart8";
						break;
					}
					getTemplateQualification = getQuery(sparqlQuery);
					queryBindings = getTemplateQualification.getBindings();

					sparql = readSparql(getTemplateQualification.getFileName());
					sparql = sparql.replace("param1", id);

					Results sparqlResults = queryFromRepository(repository, sparql);

					List<Hashtable<String, String>> results = bindQueryResults(
							queryBindings, sparqlResults);
					for (Hashtable<String, String> result : results) {
						templateQualification = new TemplateQualification();
						Description description = new Description();
						org.ids_adi.ns.qxf.model.Status status = new org.ids_adi.ns.qxf.model.Status();
						Name name = new Name();

						templateQualification.setRepository(repository.getName());

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
		} catch (RuntimeException e) {
			logger.error("Error in GetTemplateQualification: " + e);
			return templateQualificationList;
		}
	}

	private List<RoleQualification> getRoleQualification(String id, Repository rep) throws Exception {
		List<RoleQualification> roleQualifications = new ArrayList<RoleQualification>();
		try {
			String rangeSparql = "";
			String[] names = null;
			String referenceSparql = "";
			String valueSparql = "";
			String uri = "";
			Description description = new Description();
			org.ids_adi.ns.qxf.model.Status status = new org.ids_adi.ns.qxf.model.Status();

			for (Repository repository : _repositories) {
				if (rep != null) {
					if (rep.getName() != repository.getName()) {
						continue;
					}
				}
				switch (rep.getRepositoryType()) {
				case CAMELOT:
				case RDS_WIP:

//					List<Entity> rangeResultEntities = new ArrayList<Entity>();
//					List<Entity> referenceResultEntities = new ArrayList<Entity>();
//					List<Entity> valueResultEntities = new ArrayList<Entity>();

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

					List<Hashtable<String, String>> rangeBindingResults = bindQueryResults(rangeRestrictionBindings, rangeSparqlResults);
					List<Hashtable<String, String>> referenceBindingResults = bindQueryResults(referenceRestrictionBindings, referenceSparqlResults);
					List<Hashtable<String, String>> valueBindingResults = bindQueryResults(valueRestrictionBindings, valueSparqlResults);

					List<Hashtable<String, String>> combinedResults = mergeLists(mergeLists(rangeBindingResults, referenceBindingResults), valueBindingResults);

					for (Hashtable<String, String> combinedResult : combinedResults) {

						RoleQualification rq = new RoleQualification();
						uri = "";
						if (combinedResult.containsKey("qualifies")) {
							uri = combinedResult.get("qualifies");
							rq.setQualifies(uri);
							rq.setId(getIdFromURI(uri));
						}
						if (combinedResult.containsKey("name")) {
							String nameValue = combinedResult.get("name");

							if (nameValue == null) {
								nameValue = getLabel(uri).getLabel();
							}
							names = nameValue.split("@", -1);

							Name name = new Name();
							if (names.length > 1) {
								name.setLang(names[names.length - 1]);
							} else {
								name.setLang(defaultLanguage);
							}
							name.setValue(names[0]);
							rq.getNames().add(name);
						} else {
							String nameValue = getLabel(uri).getLabel();

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
							rq.getNames().add(name);
						}
						if (combinedResult.containsKey("range")) {
							rq.setRange(combinedResult.get("range"));
						}
						if (combinedResult.containsKey("reference")) {
							Value tempVar = new Value();
							tempVar.setReference(combinedResult.get("reference"));
							Value value = tempVar;

							rq.setValue(value);
						}
						if (combinedResult.containsKey("value")) {
							org.ids_adi.ns.qxf.model.Value tempVar2 = new Value();
							tempVar2.setText(combinedResult.get("value"));
							tempVar2.setAs(combinedResult.get("value_dataType"));
							Value value = tempVar2;

							rq.setValue(value);
						}
						roleQualifications.add(rq);
					}
					break;
				case PART_8:
					List<Entity> part8Entities = new ArrayList<Entity>();
					Query getPart8Roles = getQuery("GetPart8Roles");
					QueryBindings getPart8RolesBindings = getPart8Roles.getBindings();

					String part8RolesSparql = readSparql(getPart8Roles.getFileName());
					part8RolesSparql = part8RolesSparql.replace("param1", id);
					Results part8RolesResults = queryFromRepository(repository, part8RolesSparql);
					List<Hashtable<String, String>> part8RolesBindingResults = bindQueryResults(getPart8RolesBindings, part8RolesResults);
					for (Hashtable<String, String> result : part8RolesBindingResults) {
						RoleQualification rq = new RoleQualification();
						Name name = new Name();
						Value refValue = new Value();
						Value valValue = new Value();
						Description desc = new Description();
						if(result.containsKey("role")){
							uri = result.get("role");
							rq.setQualifies(uri);
							rq.setId(getIdFromURI(uri));
						}
						if (result.containsKey("comment") && result.get("comment") != null) {
							String[] comment = result.get("comment").split("@", -1);
							desc.setValue(comment[0]);
							if(comment.length > 1)
 							   desc.setLang(comment[1]);
							else
								desc.setLang(defaultLanguage);
						}
						if (result.containsKey("type") && result.get("type") != null) {
							rq.setRange(result.get("type"));
						}

						if (result.containsKey("label") ) {
							if(result.get("label") == null){
								Entity entity = getLabel(uri);
								name.setValue(entity.getLabel());
								name.setLang(entity.getLang());
							}
							else {
								String[] label = result.get("label").split("@", -1);
								name.setValue(label[0]);
								if(label.length > 1) 
									name.setLang(label[1]);
								else
									name.setLang(defaultLanguage);
							}
								
						}

						if (result.containsKey("index")) {
						}

						if (result.containsKey("role")) {
						}
						rq.getNames().add(name);
						rq.getDescriptions().add(desc);
						roleQualifications.add(rq);
					}
					break;
				}
			}
			return roleQualifications;
		} catch (RuntimeException e) {
			logger.error("Error in GetRoleQualification: " + e);
			return roleQualifications;
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
		Model delete = ModelFactory.createDefaultModel();
		Model insert = ModelFactory.createDefaultModel();
		for (String prefix : _nsmap.getPrefixes()) {
	    	delete.setNsPrefix(prefix, _nsmap.getNamespaceUri(prefix).toString());
			insert.setNsPrefix(prefix, _nsmap.getNamespaceUri(prefix).toString());
		}
		Response response = new Response();
		response.setLevel(Level.SUCCESS);
		boolean qn = false;
		String qName = null;
		try {
			Repository repository = getRepository(qmxf.getTargetRepository());
			if (repository == null || repository.isIsReadOnly()) {
				Status status = new Status();
 		        if (repository == null)
					status.getMessages().getItems().add("Repository not found!");
				else
					status.getMessages().getItems().add("Repository [" + qmxf.getTargetRepository()	+ "] is read-only!");;
			} else {
				String registry = (_useExampleRegistryBase) ? _settings.get(
						"ExampleRegistryBase").toString() : _settings.get(
						"ClassRegistryBase").toString();
				StringBuilder sparqlDelete = new StringBuilder();
		        /// Base templates do have the following properties
		        /// 1) Base class of owl:Thing
		        /// 2) rdfs:subClassOf = p8:BaseTemplateStateMent
	            /// 3) rdfs:label name of template
		        /// 4) optional rdfs:comment
	            /// 5) p8:valNumberOfRoles
				// region Template Definitions
				if (qmxf.getTemplateDefinitions().size() > 0) {
					for (TemplateDefinition newTDef : qmxf.getTemplateDefinitions()) {
						String language = "";
						int roleCount = 0;
						String templateName = null;
						String templateId = null;
						String generatedId = null;
						int index = 1;

						if (newTDef.getId() != null && newTDef.getId() != "") {
							templateId = getIdFromURI(newTDef.getId());
						}
						templateName = newTDef.getNames().get(0).getValue();
						// check for exisiting template
						Qmxf oldQmxf = new Qmxf();
						if (templateId != null && templateId != "") {
							oldQmxf = getTemplate(templateId, TemplateType.DEFINITION.toString(), repository);
						} else {
							if (_useExampleRegistryBase)
								generatedId = createIdsAdiId(_settings.get("ExampleRegistryBase").toString(), templateName);
							else
								generatedId = createIdsAdiId(_settings.get("TemplateRegistryBase").toString(), templateName);
							templateId = getIdFromURI(generatedId);
						}
						// region Form Delete/Insert SPARQL
						if (oldQmxf.getTemplateDefinitions().size() > 0) {
							for (TemplateDefinition oldTDef : oldQmxf.getTemplateDefinitions()) {
								for (Name newName : newTDef.getNames()) {
									templateName = newName.getValue();
									Name oldName = new Name();
									for (Name tempName : oldTDef.getNames()) {
										if (newName.getLang().equalsIgnoreCase(tempName.getLang())) {
											oldName = tempName;
										}
									}
									if (!oldName.getValue().equalsIgnoreCase(newName.getValue())) {
										delete = generateName(delete, oldName, templateId, oldTDef);
										insert = generateName(insert, newName, templateId, oldTDef);
									}
								}
								for (Description newDesc : newTDef.getDescriptions()) {
									Description oldDesc = new Description();
									for (Description tempName : oldTDef.getDescriptions()) {
										if (newDesc.getLang().equalsIgnoreCase(tempName.getLang())) {
											oldDesc = tempName;
										}
									}
									if (newDesc != null	&& oldDesc != null) {
										if (!oldDesc.getValue().equalsIgnoreCase(newDesc.getValue())) {
											delete = generateDescription(delete, oldDesc, templateId);
											insert = generateDescription(insert, newDesc, templateId);
										}
									} else if (newDesc != null && oldDesc == null) {
										insert = generateDescription(insert, newDesc, templateId);
									}
								}
								index = 1;
				                  ///  BaseTemplate roles do have the following properties
				                  /// 1) baseclass of owl:Class
				                  /// 2) rdfs:subClassOf = p8:TemplateRoleDescription
				                  /// 3) rdfs:label = rolename
				                  /// 4) p8:valRoleIndex
				                  /// 5) p8:hasRoleFillerType = qualifified class or dm:entityType
				                  /// 6) p8:hasTemplate = template ID
								if (oldTDef.getRoleDefinitions().size() < newTDef.getRoleDefinitions().size()) {
									for (RoleDefinition nrd : newTDef.getRoleDefinitions()) {
										String roleName = nrd.getNames().get(0).getValue();
										String nrdId = nrd.getId();

										if (nrdId == null || nrdId == "") {
											if (_useExampleRegistryBase)
												generatedId = createIdsAdiId(_settings.get("ExampleRegistryBase").toString(), roleName);
											else
												generatedId = createIdsAdiId(_settings.get("TemplateRegistryBase").toString(), roleName);
											nrdId = generatedId;
										}
										RoleDefinition ord = null;
										String tempRoleIdentifier = getIdFromURI(nrdId);
										for (RoleDefinition tempRoleDef : oldTDef.getRoleDefinitions()) {
											if (nrd.getId().equalsIgnoreCase(tempRoleDef.getId())) {
												ord = tempRoleDef;
											}
										}
										if (ord == null) {
											for (Name nrdName : nrd.getNames()) {
												insert = generateName(insert, nrdName, tempRoleIdentifier, nrd);
											}
											if (nrd.getDescriptions() != null) {
												insert = generateDescription(insert, nrd.getDescriptions().get(0), tempRoleIdentifier);
											}
											if (repository.getRepositoryType() == RepositoryType.PART_8) {
												insert = generateTypesPart8(insert, tempRoleIdentifier, templateId, nrd);
												insert = generateRoleIndexPart8(insert, tempRoleIdentifier, index, nrd);
											} else {
												insert = generateTypes(insert, tempRoleIdentifier, templateId, nrd);
												insert = generateRoleIndex(insert, tempRoleIdentifier, index);
											}
										}
										if (nrd.getRange() != null) {
											qName = _nsmap.reduceToQName(nrd.getRange());
											if (repository.getRepositoryType() == RepositoryType.PART_8) {
												insert = generateRoleFillerType(insert, tempRoleIdentifier, qName.toString());
											} else {
												insert = generateRoleDomain(insert, tempRoleIdentifier, templateId);
											}
										}
									}
								} else if (oldTDef.getRoleDefinitions().size() > newTDef.getRoleDefinitions().size()) {
									for (RoleDefinition ord : oldTDef.getRoleDefinitions()) {
										String tempRoleID = getIdFromURI(ord.getId());
										RoleDefinition oldRole = null;
										for (RoleDefinition tempRoleDef : oldTDef.getRoleDefinitions()) {
											if (ord.getId().equalsIgnoreCase(tempRoleDef.getId())) {
												oldRole = tempRoleDef;
											}
										}
										if (oldRole == null) {
											for (Name ordName : ord.getNames()) {
												delete = generateName(delete, ordName, tempRoleID, ord);
											}
											if (ord.getDescriptions().size() > 0) {
												delete = generateDescription(delete, ord.getDescriptions().get(0), tempRoleID);
											}
											if (repository.getRepositoryType() == RepositoryType.PART_8) {
												delete = generateTypesPart8(delete, tempRoleID, templateId, ord);
												delete = generateRoleIndexPart8(delete, tempRoleID, index, ord);
											} else {
												delete = generateTypes(delete, tempRoleID, templateId, ord);
												delete = generateRoleIndex(delete, tempRoleID, index);
											}
										}
										if (ord.getRange() != null) {
											qName = _nsmap.reduceToQName(ord.getRange());
											if (repository.getRepositoryType() == RepositoryType.PART_8) {
												delete = generateRoleFillerType(delete, tempRoleID, qName.toString());
											} else {
												delete = generateRoleDomain(delete, tempRoleID, templateId);
											}
										}
									}
								}
							}
							if (delete.isEmpty() && insert.isEmpty()) {
								String errMsg = "No changes made to template [" + templateName + "]";
								Status status = new Status();
								response.setLevel(Level.WARNING);
								status.getMessages().getItems().add(errMsg);
							}
						}
						if (insert.isEmpty() && delete.isEmpty()) {
							if (repository.getRepositoryType() == RepositoryType.PART_8) {
								insert = generateTypesPart8(insert, templateId, null, newTDef);
								insert = generateRoleCountPart8(insert, newTDef.getRoleDefinitions().size(), templateId, newTDef);
							} else {
								insert = generateTypes(insert, templateId, null, newTDef);
								insert = generateRoleCount(insert, newTDef.getRoleDefinitions().size(), templateId, newTDef);
							}
							for (Name name : newTDef.getNames()) {
								insert = generateName(insert, name, templateId, newTDef);
							}
							for (Description descr : newTDef.getDescriptions()) {
								insert = generateDescription(insert, descr, templateId);
							}
							// form labels
							for (RoleDefinition newRole : newTDef.getRoleDefinitions()) {
								String roleLabel = newRole.getNames().get(0).getValue();
								String newRoleID = "";
								generatedId = null;
								String genName = null;
								String range = newRole.getRange();
								genName = "Role definition " + roleLabel;
								if (newRole.getId() == null || newRole.getId() == "") {
									if (_useExampleRegistryBase)
										generatedId = createIdsAdiId(_settings.get("ExampleRegistryBase").toString(), genName);
									else
										generatedId = createIdsAdiId(_settings.get("TemplateRegistryBase").toString(), genName);
									newRoleID = getIdFromURI(generatedId);
								} else {
									newRoleID = getIdFromURI(newRole.getId());
								}
								for (Name newName : newRole.getNames()) {
									insert = generateName(insert, newName, newRoleID, newRole);
								}
								if (newRole.getDescriptions().size() > 0) {
									insert = generateDescription(insert, newRole.getDescriptions().get(0), newRoleID);
								}
								if (repository.getRepositoryType() == RepositoryType.PART_8) {
									insert = generateRoleIndexPart8(insert, newRoleID, ++roleCount, newRole);
									insert = generateHasTemplate(insert, newRoleID, templateId, newRole);
									insert = generateHasRole(insert, templateId, newRoleID.toString(), newRole);
								} else {
									insert = generateRoleIndex(insert, newRoleID, ++roleCount);
								}
								if (newRole.getRange() != null && newRole.getRange() != "") {
									qName = _nsmap.reduceToQName(newRole.getRange());
									if (repository.getRepositoryType() == RepositoryType.PART_8) {
										insert = generateRoleFillerType(insert, newRoleID, qName.toString());
									} else {
										insert = generateRoleDomain(insert, newRoleID, templateId);
										insert = generateTypes(insert, newRoleID, null, newRole);
									}
								}
							}
						}
						// endregion
						// region Generate Query and post Template Definition
						 if (!delete.isEmpty()) {
						   sparqlBuilder = addPrefixes(delete, sparqlBuilder);	 
						   sparqlBuilder.append(deleteData); 
						   sparqlBuilder = addStatements(delete, sparqlBuilder);
						   sparqlBuilder.append(" } ");
						 }
						String sparql = sparqlBuilder.toString();
						Response postResponse = postToRepository(repository, sparql);
						return postResponse;
					}
				}
				// endregion Generate Query and post Template Definition
				// endregion Template Definitions
				// region Template Qualification
		        /// Qualification templates do have the following properties
		        /// 1) Base class = owl:Thing
		        /// 2) rdfs:subClassOf = p8:SpecializedTemplateStatement
		        /// 3) rdfs:label = template name
				// /
				if (qmxf.getTemplateQualifications().size() > 0) {
					for (TemplateQualification newTQ : qmxf.getTemplateQualifications()) {
						int roleCount = 0;
						String templateName = null;
						String newTemplateID = "";
						String generatedId = null;
						String roleQualification = null;
						int index = 1;
						if (newTQ.getId() != null && newTQ.getId() != "")
							newTemplateID = getIdFromURI(newTQ.getId());

						templateName = newTQ.getNames().get(0).getValue();
						Qmxf oldQmxf = new Qmxf();
						if (newTemplateID != "" && newTemplateID != null) {
							oldQmxf = getTemplate(newTemplateID.toString(), TemplateType.QUALIFICATION.toString(), repository);
						} else {
							if (_useExampleRegistryBase)
								generatedId = createIdsAdiId(_settings.get("ExampleRegistryBase").toString(), templateName);
							else
								generatedId = createIdsAdiId(_settings.get("TemplateRegistryBase").toString(), templateName);
							newTemplateID = getIdFromURI(generatedId);
						}
						// region Form Delete/Insert SPARQL
						if (oldQmxf.getTemplateQualifications().size() > 0) {
							for (TemplateQualification oldTQ : oldQmxf.getTemplateQualifications()) {
								qName = _nsmap.reduceToQName(oldTQ.getQualifies());
								for (Name nn : newTQ.getNames()) {
									templateName = nn.getValue();
									Name on = new Name();
									for (Name tempName : oldTQ.getNames()) {
										if (nn.getLang().equalsIgnoreCase(tempName.getLang())) {
											on = tempName;
										}
									}
									if (on != null) {
										if (!on.getValue().equalsIgnoreCase(nn.getValue())) {
											delete = generateName(delete, on, newTemplateID, oldTQ);
											delete = generateName(insert, nn, newTemplateID, newTQ);
										}
									}
								}
								for (Description nd : newTQ.getDescriptions()) {
									if (nd.getLang() == null)
										nd.setLang(defaultLanguage);
									Description od = null;
									for (Description tempDesc : oldTQ.getDescriptions()) {
										if (nd.getLang().equalsIgnoreCase(tempDesc.getLang())) {
											od = new Description();
											od = tempDesc;
										}
									}
									if (od != null && od.getValue() != null) {
										if (!od.getValue().equalsIgnoreCase(nd.getValue())) {
											delete = generateDescription(delete, od, newTemplateID);
											insert = generateDescription(insert, nd, newTemplateID);
										}
									} else if (od == null
											&& nd.getValue() != null) {
										insert = generateDescription(insert, nd, newTemplateID);
									}
								}
								// role count
								if (oldTQ.getRoleQualifications().size() != newTQ.getRoleQualifications().size()) {
									if (repository.getRepositoryType() == RepositoryType.PART_8) {
										delete = generateRoleCountPart8(delete,oldTQ.getRoleQualifications().size(), newTemplateID, oldTQ);
										insert = generateRoleCountPart8(insert,newTQ.getRoleQualifications().size(), newTemplateID, newTQ);
									} else {
										delete = generateRoleCount(delete,oldTQ.getRoleQualifications().size(), newTemplateID,oldTQ);
										insert = generateRoleCount(insert,newTQ.getRoleQualifications().size(), newTemplateID,newTQ);
									}
								}

								for (Specialization ns : newTQ.getSpecializations()) {
									Specialization os = oldTQ.getSpecializations().get(0);
									if (os != null && !(os.getReference().equalsIgnoreCase(ns.getReference()))) {
										if (repository.getRepositoryType() == RepositoryType.PART_8) {
					                        GenerateTemplateSpesialization(delete, newTemplateID, os.getReference(), oldTQ);
					                        GenerateTemplateSpesialization(insert, newTemplateID, ns.getReference(), newTQ);
										} else {

										}
									}
								}
								//index = 1;
				                ///  Qualification roles do have the following properties
				                /// 1) baseclass of owl:Thing
				                /// 2) rdf:type = p8:TemplateRoleDescription
				                /// 3) rdfs:label = rolename
				                /// 4) p8:valRoleIndex
				                /// 5) p8:hasRoleFillerType = qualifified class
				                /// 6) p8:hasTemplate = template ID
								if (oldTQ.getRoleQualifications().size() < newTQ.getRoleQualifications().size()) {
									int count = 0;
									for (RoleQualification nrq : newTQ.getRoleQualifications()) {
										String roleName = nrq.getNames().get(0).getValue();
										String newRoleID = nrq.getId();
										String tempNewRoleID = getIdFromURI(newRoleID);
										if (newRoleID == null || newRoleID == "") {
											if (_useExampleRegistryBase)
												generatedId = createIdsAdiId(_settings.get("ExampleRegistryBase").toString(),roleName);
											else
												generatedId = createIdsAdiId(_settings.get("TemplateRegistryBase").toString(),roleName);
											newRoleID = generatedId;
										}
										RoleQualification orq = null;
										for (RoleQualification temprq : oldTQ.getRoleQualifications()) {
											if (newRoleID.equalsIgnoreCase(temprq.getId())) {
												orq = new RoleQualification();
												orq = temprq;
											}
										}
										if (orq == null) {
											if (repository.getRepositoryType() == RepositoryType.PART_8) {
												insert = generateTypesPart8(insert, tempNewRoleID,newTemplateID.toString(),nrq);
												for (Name nn : nrq.getNames()) {
													insert = generateName(insert, nn,tempNewRoleID, nrq);
												}
												insert = generateRoleIndexPart8(insert, tempNewRoleID,++count, nrq);
												insert = generateHasTemplate(insert, tempNewRoleID,newTemplateID, nrq);
												insert = generateHasRole(insert, newTemplateID,tempNewRoleID, newTQ);
												if (nrq.getRange() != null && nrq.getRange() == "") {
													qName = _nsmap.reduceToQName(nrq.getRange());
													insert = generateRoleFillerType(insert,tempNewRoleID,qName.toString());
												} else if (nrq.getValue() != null) {
													if (nrq.getValue().getReference() != null) {
														qName = _nsmap.reduceToQName(nrq.getValue().getReference());
														if (qName != null && qName != "")
															insert = generateRoleFillerType(insert,tempNewRoleID,qName.toString());
													} else if (nrq.getValue().getText() != null) {
														// /TODO
													}
												}
											} else // Not Part8 repository
											{
												URI uriQual = new URI(nrq.getQualifies());
												if (nrq.getRange() != null&& nrq.getRange() != "") {
													qName = _nsmap.reduceToQName(nrq.getRange());
													if (qName != null && qName != "")
														insert = generateRange(insert,tempNewRoleID,qName, nrq);
													insert = generateTypes(insert,tempNewRoleID,newTemplateID.toString(),nrq);
													insert = generateQualifies(insert,tempNewRoleID, uriQual.getFragment().substring(0), nrq);
												} else if (nrq.getValue() != null) {
													if (nrq.getValue().getReference() != null) {
														insert = generateReferenceType(insert,tempNewRoleID,newTemplateID, nrq);
														insert = generateReferenceQual(insert,tempNewRoleID,uriQual.getFragment().substring(0),nrq);
														qName = _nsmap.reduceToQName(nrq.getValue().getReference());
														if (qName != null && qName != "")
															insert = generateReferenceTpl(insert,tempNewRoleID,qName.toString(),nrq);
													} else if (nrq.getValue().getText() != null) {
														insert = generateValue(insert,tempNewRoleID.toString(),newTemplateID.toString(),nrq);
													}
												}
												insert = generateTypes(insert,tempNewRoleID,newTemplateID.toString(),nrq);
												insert = generateRoleDomain(insert, tempNewRoleID,newTemplateID.toString());
												insert = generateRoleIndex(insert, tempNewRoleID,++count);
											}
										}
									}
								} else if (oldTQ.getRoleQualifications().size() > newTQ.getRoleQualifications().size()) {
									int count = 0;
									for (RoleQualification orq : oldTQ.getRoleQualifications()) {
										String roleName = orq.getNames().get(0).getValue();
										String oldRoleID = orq.getId();
										String tmpOldRoleID = getIdFromURI(oldRoleID);
										if (oldRoleID == null || oldRoleID == "") {
											if (_useExampleRegistryBase)
												generatedId = createIdsAdiId(_settings.get("ExampleRegistryBase").toString(),roleName);
											else
												generatedId = createIdsAdiId(_settings.get("TemplateRegistryBase").toString(),roleName);
											oldRoleID = generatedId;
										}
										RoleQualification nrq = null;
										for (RoleQualification tempRq : newTQ.getRoleQualifications()) {
											if (oldRoleID.equalsIgnoreCase(tempRq.getId())) {
												nrq = new RoleQualification();
												nrq = tempRq;
											}
										}
										if (nrq == null) {
											if (repository.getRepositoryType() == RepositoryType.PART_8) {
												delete = generateTypesPart8(delete, tmpOldRoleID,newTemplateID.toString(),orq);
												for (Name nn : orq.getNames()) {
													delete = generateName(delete, nn,tmpOldRoleID, orq);
												}
												delete = generateRoleIndexPart8(delete, tmpOldRoleID,++count, orq);
												delete = generateHasTemplate(delete, tmpOldRoleID,newTemplateID.toString(),orq);
												delete = generateHasRole(delete, newTemplateID,tmpOldRoleID.toString(),oldTQ);
												if (orq.getRange() != null && orq.getRange() != "") {
													qName = _nsmap.reduceToQName(orq.getRange());
													if (qName != null && qName != "")
														delete = generateRoleFillerType(delete,tmpOldRoleID,qName.toString());
												} else if (orq.getValue() != null) {
													if (orq.getValue().getReference() != null) {
														qName = _nsmap.reduceToQName(orq.getValue().getReference());
														if (qName != null && qName != "")
															delete = generateRoleFillerType(delete,tmpOldRoleID,qName.toString());
													} else if (nrq.getValue().getText() != null) {
														// /TODO
													}
												}
											} else // Not Part8 repository
											{
												URI uriQual = new URI(orq.getQualifies());
												if (orq.getRange() != null && orq.getRange() != "") {
													qName = _nsmap.reduceToQName(orq.getRange());
													if (qName != null && qName != "")
														delete = generateRange(delete, tmpOldRoleID, qName.toString(), orq);
													delete = generateTypes(delete, tmpOldRoleID, newTemplateID.toString(), nrq);
													delete = generateQualifies(delete, tmpOldRoleID, uriQual.getFragment().substring(0), orq);
												} else if (orq.getValue() != null) {
													if (orq.getValue().getReference() != null) {
														delete = generateReferenceType(delete, tmpOldRoleID, newTemplateID.toString(), orq);
														delete = generateReferenceQual(delete, tmpOldRoleID, uriQual.getFragment().substring(0), orq);
														qName = _nsmap.reduceToQName(orq.getValue().getReference());
														if (qName != null && qName != "")
															insert = generateReferenceTpl(insert, tmpOldRoleID, qName.toString(), orq);
													} else if (orq.getValue().getText() != null) {
														delete = generateValue(delete, tmpOldRoleID.toString(), newTemplateID.toString(), orq);
													}
												}
												delete = generateTypes(delete, tmpOldRoleID, newTemplateID.toString(), orq);
												delete = generateRoleDomain(delete, tmpOldRoleID, newTemplateID.toString());
												delete = generateRoleIndex(delete, tmpOldRoleID, ++count);
											}
										}
									}
								}
							}
							if (delete.isEmpty() && insert.isEmpty()) {
								String errMsg = "No changes made to template [" + templateName + "]";
								Status status = new Status();
								response.setLevel(Level.WARNING);
								status.getMessages().getItems().add(errMsg);
 							    continue;//Nothing to be done
							}
						}
						// endregion
						// region Form Insert SPARQL
						if (delete.isEmpty()) {
							String templateLabel = null;
							String labelSparql = null;
							for (Name newName : newTQ.getNames()) {
								insert = generateName(insert, newName, newTemplateID, newTQ);
							}
							for (Description newDescr : newTQ.getDescriptions()) {
								if (newDescr.getValue() == null || newDescr.getValue() == "")
									continue;
								insert = generateDescription(insert, newDescr, newTemplateID);
							}
							if (repository.getRepositoryType() == RepositoryType.PART_8) {
								insert = generateRoleCountPart8(insert, newTQ.getRoleQualifications().size(), newTemplateID, newTQ);
								qName = _nsmap.reduceToQName(newTQ.getQualifies());
								if (qName != null && qName != "")
									insert = generateTypesPart8(insert, newTemplateID, qName.toString(), newTQ);
							} else {
								generateRoleCount(insert, newTQ.getRoleQualifications().size(), newTemplateID, newTQ);
								qName = _nsmap.reduceToQName(newTQ.getQualifies());
								if (qName != null && qName != "")
									insert = generateTypes(insert, newTemplateID, qName.toString(), newTQ);
							}
							for (Specialization spec : newTQ.getSpecializations()) {
								String specialization = spec.getReference();
								if (repository.getRepositoryType() == RepositoryType.PART_8) {
									// /TODO
								} else {
									// /TODO
								}
							}
							for (RoleQualification newRole : newTQ.getRoleQualifications()) {
								String roleLabel = newRole.getNames().get(0).getValue();
								String roleID = "";
								generatedId = null;
								String genName = null;
								String range = newRole.getRange();

								genName = "Role Qualification " + roleLabel;
								if (newRole.getId() == null && newRole.getId() == "") {
									if (_useExampleRegistryBase)
										generatedId = createIdsAdiId(_settings.get("ExampleRegistryBase").toString(), genName);
									else
										generatedId = createIdsAdiId(_settings.get("TemplateRegistryBase").toString(), genName);
									roleID = getIdFromURI(generatedId);
								} else {
									roleID = getIdFromURI(newRole.getId());
								}
								if (repository.getRepositoryType() == RepositoryType.PART_8) {
									insert = generateTypesPart8(insert, roleID, newTemplateID.toString(), newRole);
									for (Name newName : newRole.getNames()) {
										insert = generateName(insert, newName, roleID, newRole);
									}
									insert = generateRoleIndexPart8(insert, roleID, ++roleCount, newRole);
									insert = generateHasTemplate(insert, roleID, newTemplateID.toString(), newRole);
									insert = generateHasRole(insert, newTemplateID, roleID.toString(), newTQ);
									if (newRole.getRange() != null && newRole.getRange() != "") {
										qName = _nsmap.reduceToQName(newRole .getRange());
										if (qName != null && qName != "")
											insert = generateRoleFillerType(insert, roleID, qName.toString());
									} else if (newRole.getValue() != null) {
										if (newRole.getValue().getReference() != null) {
											qName = _nsmap .reduceToQName(newRole.getValue().getReference());
											if (qName != null && qName != "")
												insert = generateRoleFillerType(insert, roleID, qName.toString());
										} else if (newRole.getValue().getText() != null) {
											// /TODO
										}
									}
								} else // Not Part8 repository
								{
									URI nrqQual = new URI(newRole.getQualifies());
									if (newRole.getRange() != null && newRole.getRange() != "") {
										qName = _nsmap.reduceToQName(newRole.getRange());
										if (qName != null && qName != "")
											insert = generateRange(insert, roleID, qName.toString(), newRole);
										insert = generateTypes(insert, roleID, newTemplateID.toString(), newRole);
										insert = generateQualifies(insert, roleID, nrqQual.getFragment().substring(0), newRole);
									} else if (newRole.getValue() != null) {
										if (newRole.getValue().getReference() != null) {
											insert = generateReferenceType(insert, roleID, newTemplateID.toString(), newRole);
											insert = generateReferenceQual(insert, roleID, nrqQual.getFragment().substring(0), newRole);
											qName = _nsmap.reduceToQName(newRole.getValue().getReference());
											if (qName != null && qName != "")
												insert = generateReferenceTpl(insert, roleID, qName.toString(), newRole);
										} else if (newRole.getValue().getText() != null) {
											insert = generateValue(insert, roleID.toString(), newTemplateID.toString(), newRole);
										}
									}
									insert = generateTypes(insert, roleID, newTemplateID.toString(), newRole);
									insert = generateRoleDomain(insert, roleID, newTemplateID.toString());
									insert = generateRoleIndex(insert, roleID, ++roleCount);
								}
							}
						}
						// endregion
						// region Generate Query and Post Qualification Template
						 if (!delete.isEmpty()) {
							 sparqlBuilder = addPrefixes(delete, sparqlBuilder);
						     sparqlBuilder.append(deleteData);
						     sparqlBuilder = addStatements(delete, sparqlBuilder);
						     if(insert.isEmpty()) 
						    	 sparqlBuilder.append(" } ");
						     else {
						    	 sparqlBuilder.append(" }; ");
						    	 sparqlBuilder.append(insertData);
						    	 sparqlBuilder = addStatements(insert, sparqlBuilder);
						     }
						     sparqlBuilder.append(" } ");
						 }
						String sparql = sparqlBuilder.toString();
						Response postResponse = postToRepository(repository,sparql);
						response = postResponse;
					}
				}
			}
		}
		catch (Exception ex) {
			String errMsg = "Error in PostTemplate: " + ex;
			Status status = new Status();
			response.setLevel(Level.ERROR);
			status.getMessages().getItems().add(errMsg);
			logger.error(errMsg);
		}
		return response;
	}

	private Model generateValue(Model work, String subjId, String objId, RoleQualification nrq) throws URISyntaxException {
		URI uri = new URI(nrq.getQualifies());
	    Resource subj = work.createResource(String.format("tpl:%s", subjId));
		Property pred = work.createProperty("tpl:R56456315674");
	    Resource  obj = work.createResource(String.format("tpl:%s", objId));
	    work.add(subj, pred, obj);
	    pred = work.createProperty("tpl:R89867215482");
	     obj = work.createResource(String.format("tpl:%s", uri.getFragment().substring(0)));
	    work.add(subj, pred, obj);
	    pred = work.createProperty("tpl:R29577887690");
	    Literal lit = work.createTypedLiteral(nrq.getValue().getText(), (nrq.getValue().getLang() == null || nrq.getValue().getLang() == null) ? defaultLanguage : nrq.getValue().getLang());
	    work.add(subj, pred, lit);
		return null;
	}

	private int getIndexFromName(String name) {
		int index = 0;
		try {
			for (Repository repository : _repositories) {
				if (repository.getName().equalsIgnoreCase(name)) {
					return index;
				}
				index++;
			}
			index = 0;
			for (Repository repository : _repositories) {
				if (!repository.isIsReadOnly()) {
					return index;
				}
				index++;
			}
		} catch (Exception ex) {
			logger.error(ex);

		}
		return index;
	}

	private Response postToRepository(Repository repository, String sparql) {
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
				
			} else {

			}
		} catch (Exception ex) {
			logger.error(ex);
			return response;
		}

		return response;
	}

	@SuppressWarnings("unchecked")
	public Response postClass(Qmxf qmxf) {
		Model delete = ModelFactory.createDefaultModel();
		Model insert = ModelFactory.createDefaultModel();
		for (String prefix : _nsmap.getPrefixes()) {
			delete.setNsPrefix(prefix, _nsmap.getNamespaceUri(prefix).toString());
			insert.setNsPrefix(prefix, _nsmap.getNamespaceUri(prefix).toString());
		}

		Response response = new Response();
		response.setLevel(Level.SUCCESS);
		boolean qn = false;
		String qName = null;

		try {
			Repository repository = getRepository(qmxf.getTargetRepository());

			if (repository == null || repository.isIsReadOnly()) {
				Status status = new Status();
				response.setLevel(Level.ERROR);

				if (repository == null)
					status.getMessages().getItems().add("Repository not found!");
				else
					status.getMessages().getItems().add("Repository [" + qmxf.getTargetRepository() + "] is read-only!");
				// _response.Append(status);
			} else {
				String registry = _useExampleRegistryBase ?
						_settings.get("ExampleRegistryBase").toString() : 
						_settings.get("ClassRegistryBase").toString();

				for (ClassDefinition clsDef : qmxf.getClassDefinitions()) {

					String language = null;
					int classCount = 0;
					String clsId = getIdFromURI(clsDef.getId());
					Qmxf existingQmxf = new Qmxf();

					if (clsId != null) {
						existingQmxf = getClass(clsId, repository);
					}

					// delete class
					if (existingQmxf.getClassDefinitions().size() > 0) {

						for (ClassDefinition existingClsDef : existingQmxf.getClassDefinitions()) {
							for (Name clsName : clsDef.getNames()) {
								Name existingName = new Name();
								for (Name tempName : existingClsDef.getNames()) {
									if (clsName.getLang().equalsIgnoreCase(tempName.getLang())) {
										existingName = tempName;
									}
								}
								if (existingName != null) {
									if (!existingName.getValue()
											.equalsIgnoreCase(clsName.getValue())) {
										delete = generateClassName(delete, existingName, clsId, existingClsDef);
										insert = generateClassName(insert, clsName, clsId, clsDef);
									}
								}
								for (Description description : clsDef.getDescriptions()) {
									Description existingDescription = new Description();
									for (Description tempDesc : existingClsDef.getDescriptions()) {
										if (description.getLang().equalsIgnoreCase(tempDesc.getLang())) {
											existingDescription = tempDesc;
										}
									}
									if (existingDescription != null) {
										if (!existingDescription.getValue().equalsIgnoreCase(description.getValue())) {
											delete = generateClassDescription(delete, existingDescription, clsId);
											insert = generateClassDescription(insert, description, clsId);
										}
									}
								}
								if (clsDef.getSpecializations().size() == existingClsDef.getSpecializations().size()) {
									continue; // / no change ... so continue
								} else if (clsDef.getSpecializations().size() < existingClsDef.getSpecializations().size()) 
								{
									for (Specialization os : existingClsDef.getSpecializations()) {
										Specialization ns = null;
										for (Specialization tempSpec : clsDef.getSpecializations()) {
											if (os.getReference().equalsIgnoreCase(tempSpec.getReference())) {
												ns = new Specialization();
												ns = tempSpec;
											}
										}
										if (ns == null) {
											qName = _nsmap.reduceToQName(os.getReference());
											if (qName != null && qName != "")
												delete = generateRdfSubClass(delete, clsId, qName.toString());
										}
									}
								} else if (clsDef.getSpecializations().size() > existingClsDef.getSpecializations().size())
								{
									for (Specialization ns : clsDef.getSpecializations()) {
										Specialization os = null;
										for (Specialization tempSpec : existingClsDef.getSpecializations()) {
											if (ns.getReference().equalsIgnoreCase(tempSpec.getReference())) {
												os = new Specialization();
												os = tempSpec;
											}
										}
										if (os == null) {
											qName = _nsmap.reduceToQName(ns.getReference());
											if (qName != null && qName != "")
												insert = generateRdfSubClass(insert, clsId, qName.toString());
										}
									}
								}
								if (clsDef.getClassifications().size() == existingClsDef.getClassifications().size()) {
									continue; // no change...so continue
								} else if (clsDef.getClassifications().size() < existingClsDef.getClassifications().size()) {
									for (Classification oc : existingClsDef.getClassifications()) {
										Classification nc = null;
										for (Classification tempClas : clsDef.getClassifications()) {
											if (oc.getReference().equalsIgnoreCase(	tempClas.getReference())) {
												nc = new Classification();
												nc = tempClas;
											}
										}
										if (nc == null) {
											qName = _nsmap.reduceToQName(oc.getReference());
											if (repository.getRepositoryType() == RepositoryType.PART_8) {
												if (qName != null && qName != "")
													delete = generateSuperClass(delete, qName.toString(), clsId.toString());
											} else {
												if (qName != null && qName != "")
													delete = generateDmClassification(delete, clsId, qName.toString());
											}
										}
									}
								} else if (clsDef.getClassifications().size() > existingClsDef.getClassifications().size()) {
									for (Classification nc : clsDef.getClassifications()) {
										Classification oc = null;
										for (Classification tempClas : existingClsDef.getClassifications()) {
											if (nc.getReference().equalsIgnoreCase(tempClas.getReference())) {
												oc = new Classification();
												oc = tempClas;
											}
										}
										if (oc == null) {
											qName = _nsmap.reduceToQName(nc.getReference());
											if (repository.getRepositoryType() == RepositoryType.PART_8) {
												if (qName != null && qName != "")
													insert = generateSuperClass(insert, qName.toString(), clsId.toString()); 
											} else {
												if (qName != null && qName != "")
													insert = generateDmClassification(insert, clsId, qName.toString());
											}
										}
									}
								}
							}
						}

						if (delete.isEmpty() && insert.isEmpty()) {
							String errMsg = "No changes made to class ["
									+ qmxf.getClassDefinitions().get(0).getNames().get(0).getValue() + "]";
							Status status = new Status();
							response.setLevel(Level.WARNING);
							status.getMessages().getItems().add(errMsg);
							continue;// Nothing to be done
						}
					}
					// add class
					if (delete.isEmpty() && insert.isEmpty()) {
						String clsLabel = clsDef.getNames().get(0).getValue();
						if (clsId == null) {
							String newClsName = "Class definition " + clsLabel;
							clsId = getIdFromURI(createIdsAdiId(registry, newClsName));
						}
						// append entity type
						if (clsDef.getEntityType() != null
								&& (clsDef.getEntityType().getReference() != null && clsDef
										.getEntityType().getReference() != "")) {
							qName = _nsmap.reduceToQName(clsDef.getEntityType().getReference());
							if (qName != null && qName != "")
								insert = generateTypesPart8(insert, clsId, qName.toString(), clsDef);
						}
						// append specialization
						for (Specialization ns : clsDef.getSpecializations()) {
							if (ns.getReference() != null
									&& ns.getReference() != "") {
								qName = _nsmap.reduceToQName(ns.getReference());
								if (repository.getRepositoryType() == RepositoryType.PART_8) {
									if (qName != null && qName != "")
										insert = generateRdfSubClass(insert, clsId, qName.toString());
								} else {
									if (qName != null && qName != "")
										insert = generateDmSubClass(insert,	clsId, qName.toString());
								}
							}
						}
						// append description
						for (Description nd : clsDef.getDescriptions()) {
							if (nd.getValue() != null && nd.getValue() != "") {
								insert = generateClassDescription(insert, nd, clsId);
							}
						}
						for (Name nn : clsDef.getNames()) {
							// append label
							insert = generateClassName(insert, nn, clsId, clsDef);
						}
						// append classification
						for (Classification nc : clsDef.getClassifications()) {
							if (nc.getReference() != null && nc.getReference() != "") {
								qName = _nsmap.reduceToQName(nc.getReference());
								if (repository.getRepositoryType() == RepositoryType.PART_8) {
									if (qName != null && qName != "")
										insert = generateSuperClass(insert, qName.toString(), clsId.toString());
								} else {
									if (qName != null && qName != "")
										insert = generateDmClassification(insert, clsId, qName.toString());
								}
							}
						}
					}
					if (!delete.isEmpty()) {
						String objstr = "";
						addPrefixes(delete, sparqlBuilder);
						sparqlBuilder.append(deleteData);
						sparqlBuilder = addStatements(delete, sparqlBuilder);
						
						if (insert.isEmpty())
							sparqlBuilder.append("}");
						else
							sparqlBuilder.append("};");
					}

					if (!insert.isEmpty()) {
						
						addPrefixes(insert, sparqlBuilder);
						sparqlBuilder.append(insertData);
						sparqlBuilder = addStatements(insert, sparqlBuilder);
					}

					String sparql = sparqlBuilder.toString();
					Response postResponse = postToRepository(repository, sparql);
					response = postResponse;
				}
			}
		} catch (Exception ex) {
			String errMsg = "Error in PostClass: " + ex;
			Status status = new Status();

			response.setLevel(Level.ERROR);
			status.getMessages().getItems().add(errMsg);
			// response.Append(status);

			logger.error(errMsg);
		}

		return response;
	}

	private StringBuilder addStatements(Model model, StringBuilder sparql) {
		String objstr = "";
		StmtIterator inserts = model.listStatements();
		while (inserts.hasNext()) {
			Statement stmt = inserts.nextStatement();
			Resource subj = stmt.getSubject();
			Property pred = stmt.getPredicate();
			RDFNode   obj = stmt.getObject();
			if(obj instanceof Resource)
				objstr = obj.toString();
			else
				objstr = "\"" + obj.toString() + "\"";
			sparql.append(subj.toString()+ " "+ pred.toString()+ " " + objstr+ " . ");
		}
		sparql.append("}");
		return sparql;
	}

	private StringBuilder addPrefixes(Model model, StringBuilder sparql) {
		Map<String, String> prefixes = model.getNsPrefixMap();
		for(String prefix : _nsmap.getPrefixes()) {
			sparql.append("PREFIX "+prefix+": <" + _nsmap.getNamespaceUri(prefix).toString()+ "> ");
		}
		return sparql;
	}

	public List<Repository> getRepositories() throws Exception {
		List<Repository> repositoryList = new ArrayList<Repository>();
		try {
			Federation federation = getFederation();
			for (Repository repo : federation.getRepositories().getItems()) {
				repositoryList.add(repo);
			}
		} catch (Exception ex) {
			logger.error(ex);
			return repositoryList;
		}
		return repositoryList;

	}

	private Repository getRepository(String name) {
		// Repository repository = null;
		for (Repository tempRepo : _repositories) {
			if (tempRepo.getName().equalsIgnoreCase(name)) {
				return tempRepo;
			}
		}
		return null;
	}

	public org.iringtools.refdata.response.Response getClassTemplates(String id)
			throws Exception {
		org.iringtools.refdata.response.Response response = new org.iringtools.refdata.response.Response();
		Entities entities = new Entities();
		response.setEntities(entities);
		List<Entity> entityList = new ArrayList<Entity>();
		entities.setItems(entityList);

		String[] names = null;
		String language = "";
		try {
			String sparqlGetClassTemplates = "";
			String sparqlGetRelatedTemplates = "";
			String relativeUri = "";
			Query queryGetClassTemplates = getQuery("GetClassTemplates");
			QueryBindings queryBindingsGetClassTemplates = queryGetClassTemplates
					.getBindings();

			sparqlGetClassTemplates = readSparql(queryGetClassTemplates
					.getFileName());
			sparqlGetClassTemplates = sparqlGetClassTemplates.replace("param1",
					id);

			Query queryGetRelatedTemplates = getQuery("GetRelatedTemplates");
			QueryBindings queryBindingsGetRelatedTemplates = queryGetRelatedTemplates
					.getBindings();

			sparqlGetRelatedTemplates = readSparql(queryGetRelatedTemplates
					.getFileName());
			sparqlGetRelatedTemplates = sparqlGetRelatedTemplates.replace(
					"param1", id);

			for (Repository repository : _repositories) {
				if (repository.getRepositoryType()
						.equals(RepositoryType.PART_8)) {
					Results sparqlResults = queryFromRepository(repository,
							sparqlGetRelatedTemplates);

					List<Hashtable<String, String>> results = bindQueryResults(
							queryBindingsGetRelatedTemplates, sparqlResults);

					for (Hashtable<String, String> result : results) {

						names = result.get("label").split("@", -1);
						if (names.length == 1) {
							language = defaultLanguage;
						} else {
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
				} else {
					Results sparqlResults = queryFromRepository(repository,
							sparqlGetClassTemplates);

					List<Hashtable<String, String>> results = bindQueryResults(
							queryBindingsGetClassTemplates, sparqlResults);

					for (Hashtable<String, String> result : results) {
						names = result.get("label").split("@", -1);
						if (names.length == 1) {
							language = defaultLanguage;
						} else {
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
		} catch (RuntimeException e) {
			logger.error("Error in GetClassTemplates: " + e);
			return response;
		}
		return response;
	}

	public org.iringtools.refdata.response.Response search(String query)
			throws Exception {
		try {
			return searchPage(query, 0, 0);
		} catch (RuntimeException ex) {
			logger.error("Error in Search: " + ex);
			return new org.iringtools.refdata.response.Response();
		}
	}

	public org.iringtools.refdata.response.Response searchPage(String query,
			int i, int j) throws Exception {
		org.iringtools.refdata.response.Response response = new org.iringtools.refdata.response.Response();
		Entities entities = new Entities();
		response.setEntities(entities);
		List<Entity> entityList = new ArrayList<Entity>();
		entities.setItems(entityList);
		int counter = 0;

		try {
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

			for (Repository repository : _repositories) {
				Results sparqlResults = queryFromRepository(repository, sparql);
				if (sparqlResults != null) {
					List<Hashtable<String, String>> results = bindQueryResults(
							queryBindings, sparqlResults);
					for (Hashtable<String, String> result : results) {
						names = result.get("label").split("@", -1);
						if (names.length == 1) {
							language = defaultLanguage;
						} else {
							language = names[names.length - 1];
						}
						if (names[0].startsWith("has")
								|| names[0].startsWith("val"))
							continue;
						Entity tempVar = new Entity();
						tempVar.setUri(result.get("uri"));
						tempVar.setLabel(names[0]);
						tempVar.setLang(language);
						tempVar.setRepository(repository.getName());
						resultEntity = tempVar;

						key = resultEntity.getLabel();

						if (resultEntities.containsKey(key)) {
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

			if (j > 0) {
				response = getRequestedPage(response, i, j);
			}
			return response;
		} catch (RuntimeException e) {
			logger.error("Error in SearchPage: " + e);
			return response;
		}
	}

	private org.iringtools.refdata.response.Response getRequestedPage(
			org.iringtools.refdata.response.Response entities, int startIdx,
			int pageSize) {
		org.iringtools.refdata.response.Response page = new org.iringtools.refdata.response.Response();
		try {
			page.setTotal(entities.getEntities().getItems().size());
			Entities ent = new Entities();
			page.setEntities(ent);
			for (int i = startIdx; i < startIdx + pageSize; i++) {
				if (entities.getEntities().getItems().size() == i) {
					break;
				}
				Entity entity = entities.getEntities().getItems().get(i);
				ent.getItems().add(entity);
				// page.getEntities().getItems().add(i, entity);
			}

			return page;
		} catch (RuntimeException ex) {
			logger.error(ex);
			return page;
		}
	}

	public Map<String, Entity> searchReset(String query) {
		return null;
	}

	public org.iringtools.refdata.response.Response getSuperClasses(String id)
			throws Exception {
		org.iringtools.refdata.response.Response response = new org.iringtools.refdata.response.Response();
		Entities entities = new Entities();
		response.setEntities(entities);
		List<Entity> entityList = new ArrayList<Entity>();
		entities.setItems(entityList);

		String[] names = null;
		try {
			List<Specialization> specializations = getSpecializations(id, null);

			for (Specialization specialization : specializations) {
				Entity tempVar = new Entity();
				String uri = specialization.getReference();
				names = specialization.getLabel().split("@");
				String label = names[0];

				if (label == null) {
					names = getLabel(uri).getLabel().split("@");
					label = names[0];
				}
				if (names.length == 1) {
					tempVar.setLang(defaultLanguage);

				} else if (names.length == 2) {
					tempVar.setLang(names[names.length - 1]);
				}
				tempVar.setUri(uri);
				tempVar.setLabel(label);
				Entity resultEntity = tempVar;
				entityList.add(resultEntity);
			}
			response.setTotal(entityList.size());
			Collections.sort(entityList, new EntityComparator());
		} catch (RuntimeException e) {
			logger.error("Error in GetSuperClasses: " + e);
			return response;
		}
		return response;
	}

	public org.iringtools.refdata.response.Response getSubClasses(String id)
			throws Exception {
		org.iringtools.refdata.response.Response response = new org.iringtools.refdata.response.Response();
		Entities entities = new Entities();
		response.setEntities(entities);
		List<Entity> entityList = new ArrayList<Entity>();
		entities.setItems(entityList);

		try {
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
			QueryBindings queryBindingsPart8 = queryGetSubClassOfInverse
					.getBindings();

			sparqlPart8 = readSparql(queryGetSubClassOfInverse.getFileName());
			sparqlPart8 = sparqlPart8.replace("param1", id);

			for (Repository repository : _repositories) {
				if (repository.getRepositoryType()
						.equals(RepositoryType.PART_8)) {
					Results sparqlResults = queryFromRepository(repository,
							sparqlPart8);

					List<Hashtable<String, String>> results = bindQueryResults(
							queryBindingsPart8, sparqlResults);

					for (Hashtable<String, String> result : results) {
						names = result.get("label").split("@", -1);

						if (names.length == 1) {
							language = defaultLanguage;
						} else {
							language = names[names.length - 1];
						}
						Entity tempVar = new Entity();
						tempVar.setUri(result.get("uri"));
						tempVar.setLabel(names[0]);
						tempVar.setLang(language);
						// Entity resultEntity = tempVar;
						entityList.add(tempVar);
					}
				} else {
					Results sparqlResults = queryFromRepository(repository,
							sparql);
					List<Hashtable<String, String>> results = bindQueryResults(
							queryBindings, sparqlResults);
					for (Hashtable<String, String> result : results) {
						names = result.get("label").split("@", -1);
						if (names.length == 1) {
							language = defaultLanguage;
						} else {
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
		} catch (RuntimeException e) {
			logger.error("Error in GetSubClasses: " + e);
			return response;
		}
		return response;
	}

	public org.iringtools.refdata.response.Response getAllSuperClasses(String id)
			throws Exception {
		org.iringtools.refdata.response.Response response = new org.iringtools.refdata.response.Response();
		Entities entities = new Entities();
		response.setEntities(entities);
		List<Entity> entityList = new ArrayList<Entity>();
		entities.setItems(entityList);
		response = getAllSuperClasses(id, response);
		response.setTotal(response.getEntities().getItems().size());
		return response;
	}

	public org.iringtools.refdata.response.Response getAllSuperClasses(
			String id, org.iringtools.refdata.response.Response response)
			throws Exception {
		Entities entities = response.getEntities();
		List<Entity> entityList = entities.getItems();
		String[] names = null;
		try {
			List<Specialization> specializations = getSpecializations(id, null);
			// base case
			if (specializations.isEmpty()) {
				return response;
			}

			for (Specialization specialization : specializations) {
				String uri = specialization.getReference();
				String label = specialization.getLabel();
				String language = "";

				if (label == null) {
					names = getLabel(uri).getLabel().split("[@]", -1);
					label = names[0];
					if (names.length == 1) {
						language = defaultLanguage;
					} else {
						language = names[names.length - 1];
					}
				} else {
					names = label.split("@");
					if (names.length == 1) {
						language = defaultLanguage;
					} else {
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
				for (Entity entt : entities.getItems()) {
					if (resultEntity.getUri().equals(entt.getUri())) {
						found = true;
						break;
					}
				}

				if (!found) {
					trimmedUri = uri.substring(0, 0)
							+ uri.substring(0 + uri.lastIndexOf('#') + 1);
					entities.getItems().add(resultEntity);
					getAllSuperClasses(trimmedUri, response);
				}
			}
			Collections.sort(entityList, new EntityComparator());
		} catch (RuntimeException e) {
			logger.error("Error in GetAllSuperClasses: " + e);
			return response;
		}

		return response;
	}

	private Response queryIdGenerator(String serviceUrl)
			throws HttpClientException {
		Response result = null;
		HttpClient httpClient = null;
		try {
			String uri = _settings.get("idGenServiceUri").toString();
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
			logger.error("Error in createIdsAdiId: " + e);
			return idsAdiId;
		}
		return idsAdiId;
	}

	public Entity getLabel(String uri) throws Exception {
		Entity labelEntity = new Entity();

		try {
			String sparql = "";
			String[] names;
			Query queryContainsSearch = getQuery("GetLabel");
			QueryBindings queryBindings = queryContainsSearch.getBindings();
			sparql = readSparql(queryContainsSearch.getFileName());
			sparql = sparql.replace("param1", uri);

			for (Repository repository : _repositories) {

				Results sparqlResults = queryFromRepository(repository, sparql);
				List<Hashtable<String, String>> results = bindQueryResults(
						queryBindings, sparqlResults);
				for (Hashtable<String, String> result : results) {
					if (result.containsKey("label")) {
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
		} catch (Exception e) {
			logger.error("Error in GetClass: " + e);
			return labelEntity;
		}
	}

	private List<RoleDefinition> getRoleDefinition(String id) throws Exception,
			HttpClientException {

		List<RoleDefinition> roleDefinitions = new ArrayList<RoleDefinition>();
		try {
			String sparql = "";
			String sparqlQuery = "";
			String[] names = null;

			Description description = new Description();
			org.ids_adi.ns.qxf.model.Status status = new org.ids_adi.ns.qxf.model.Status();

			List<Entity> resultEntities = new ArrayList<Entity>();

			for (Repository repository : _repositories) {
				switch (repository.getRepositoryType()) {
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

				List<Hashtable<String, String>> results = bindQueryResults(
						queryBindings, sparqlResults);

				for (Hashtable<String, String> result : results) {
					RoleDefinition roleDefinition = new RoleDefinition();
					Name name = new Name();

					if (result.containsKey("label")) {
						names = result.get("label").split("@", -1);
						name.setValue(names[0]);
						if (names.length == 1) {
							name.setLang(defaultLanguage);
						} else {
							name.setLang(names[names.length - 1]);
						}
					}
					if (result.containsKey("role")) {
						roleDefinition.setId(result.get("role"));
					}
					if (result.containsKey("comment")) {
						names = result.get("comment").split("@", -1);
						description.setValue(names[0]);
						if (names.length == 1) {
							description.setLang(defaultLanguage);
						} else {
							description.setLang(names[names.length - 1]);
						}
					}
					if (result.containsKey("index")) {
						description.setValue(result.get("index").toString());
					}
					if (result.containsKey("type")) {
						roleDefinition.setRange(result.get("type"));
					}
					roleDefinition.getNames().add(name);
					roleDefinition.getDescriptions().add(description);
					roleDefinitions.add(roleDefinition);
				}
			}

			return roleDefinitions;
		} catch (RuntimeException e) {
			logger.error("Error in GetRoleDefinition: " + e);
			return roleDefinitions;
		}
	}

	private List<Hashtable<String, String>> bindQueryResults(
			QueryBindings queryBindings, Results sparqlResults) {
		String sBinding = "";
		String qBinding = "";
		List<Hashtable<String, String>> results = new ArrayList<Hashtable<String, String>>();
		try {

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
			logger.error(ex);
			return results;
		}
	}

	private String makeUniqueKey(Hashtable<String, String> hashtable,
			String duplicateKey) {

		String newKey = "";
		try {

			for (int i = 2; i < Integer.MAX_VALUE; i++) {
				String postFix = " (" + (new Integer(i)).toString() + ")";
				if (!hashtable.containsKey(duplicateKey + postFix)) {
					newKey += postFix;
					break;
				}
			}
			return newKey;
		} catch (RuntimeException ex) {
			logger.error(ex);
			return newKey;
		}
	}

	public org.iringtools.refdata.response.Response getEntityTypes() {
		org.iringtools.refdata.response.Response response = new org.iringtools.refdata.response.Response();
		Entities entities = new Entities();
		response.setEntities(entities);
		List<Entity> entityList = new ArrayList<Entity>();
		entities.setItems(entityList);
		String sparql = "";
		try {
			Query getEntities = getQuery("GetEntityTypes");
			sparql = readSparql(getEntities.getFileName());
			for (Repository rep : _repositories) {
				if (rep.getName().equals("EntityTypes")) {
					Results sparqlResults = queryFromRepository(rep, sparql);
					if (sparqlResults != null) {
						for (Result result : sparqlResults.getResults()) {
							Binding b = result.getBindings().get(0);
							URI uri = new URI(b.getUri().toString());
							Entity ent = new Entity();
							ent.setUri(uri.toString());
							ent.setLabel(uri.getFragment().substring(0));
							entityList.add(ent);
						}
					}
				}
			}
			response.setTotal(entityList.size());
		} catch (Exception ex) {
			logger.error(ex);
			return response;
		}
		return response;

	}

	private Results queryFromRepository(Repository repository, String sparql)
			throws HttpClientException, UnsupportedEncodingException,
			JAXBException {
		Results results = null;

		try {
			// TODO need to look at credentials
			NetworkCredentials credentials = new NetworkCredentials();
			HttpClient sparqlClient = new HttpClient(repository.getUri());
			sparqlClient.setNetworkCredentials(credentials);
			Sparql sparqlResults = sparqlClient.postSparql(Sparql.class, "",
					sparql, "");

			results = sparqlResults.getResults();
		} catch (RuntimeException ex) {
			logger.error(ex);
			return results;
		}

		return results;
	}

	private Query getQuery(String queryName) {
		Query query = null;
		List<QueryItem> items = _queries.getItems();

		for (QueryItem qry : items) {
			String qryKey = qry.getKey();
			if (qryKey.equals(queryName)) {
				query = qry.getQuery();
				break;
			}
		}
		return query;
	}

	// updations

	private Model generateTypes(Model work, String subjId, String objId, Object gObj) throws Exception {
		if (gObj instanceof TemplateDefinition) {
			Resource subj = work.createResource(String.format("tpl:%s", subjId));
		    Property pred = work.createProperty(rdfType);
		    Resource obj = work.createResource("tpl:R16376066707");
		    work.add(subj, pred, obj);
		} else if (gObj instanceof RoleDefinition) {
			Resource subj = work.createResource(String.format("tpl:%s", subjId));
	        Property pred = work.createProperty(rdfType);
	        Resource obj = work.createResource("tpl:R74478971040");
		} else if (gObj instanceof TemplateQualification) {
			Resource subj = work.createResource(objId);
			Property pred = work.createProperty("dm:hasSubclass");
		    Resource obj = work.createResource(String.format("tpl:%s", subjId));
		    work.add(subj, pred, obj);
		    subj = work.createResource(String.format("tpl:%s", subjId));
		    pred = work.createProperty("dm:hasSuperclass");
		    obj = work.createResource(objId);
		    work.add(subj, pred, obj);
		} else if (gObj instanceof RoleQualification) {
			Resource subj = work.createResource(subjId);
			Property pred = work.createProperty(rdfType);
	        Resource obj = work.createResource("tpl:R76288246068");
	        work.add(subj, pred, obj);
	        pred = work.createProperty("tpl:R99672026745");
	        obj = work.createResource(String.format("tpl:%s", objId));
	        work.add(subj, pred, obj);
	        pred = work.createProperty(rdfType);
	        obj = work.createResource("tpl:R67036823327");
	        work.add(subj, pred, obj);
		}
		return work;
	}

	private Model generateName(Model work,  Name name,String subjId, Object gobj) throws Exception {
		Resource subj = work.createResource(String.format("tpl:%s", subjId));
	    Property  pred = work.createProperty("rdfs:label");
	    Literal  obj = work.createLiteral(name.getValue(), (name.getLang() == null || name.getLang() == "") ? defaultLanguage : name.getLang());
	    work.add(subj, pred, obj);
		return work;
	}

	private Model generateReferenceQual(Model work, String subjId, String objId, Object gobj) {
	   Resource  subj = work.createResource(String.format("tpl:%s", subjId));
	   Property  pred = work.createProperty("tpl:R30741601855");
	   Resource   obj = work.createResource(String.format("tpl:%s", objId));
	   work.add(subj, pred, obj);
       return work;
	}

	private Model generateReferenceType(Model work, String subjId, String objId, Object gobj) {
	   Resource  subj = work.createResource(String.format("tpl:%s", subjId));
	   Property  pred = work.createProperty(rdfType);
	   Resource   obj = work.createResource("tpl:R40103148466");
	   work.add(subj, pred, obj);
	   pred = work.createProperty("tpl:R49267603385");
	   obj = work.createResource(String.format("tpl:%s", objId));
	   work.add(subj, pred, obj);
		return work;
	}

	private Model generateReferenceTpl(Model work, String subjId, String objId, Object gobj) {
	   Resource subj = work.createResource(String.format("tpl:$s", subjId));
	   Property pred = work.createProperty("tpl:R21129944603");
	   Resource  obj = work.createResource(objId);
	   work.add(subj, pred, obj);
	   return work;
	}

	private Model generateQualifies(Model work, String subjId, String objId, Object gobj) {
	   Resource subj = work.createResource(String.format("tpl:%s", subjId));
	   Property pred = work.createProperty("tpl:R91125890543");
	   Resource  obj = work.createResource(String.format("tpl:%s", objId));
	   work.add(subj, pred, obj);
		return work;
	}

	private Model generateRange(Model work, String subjId, String objId, Object gobj) {
	   Resource subj = work.createResource(String.format("tpl:%s", subjId));
	   Property pred = work.createProperty("rdfs:range");
	   Resource  obj = work.createResource(objId);
	   work.add(subj, pred, obj);
	   pred = work.createProperty("tpl:R98983340497");
	   obj = work.createResource(qName);
	   work.add(subj, pred, obj);
	   return work;
	}

	private Model generateHasRole(Model work, String subjId, String objId, Object gobj) {
	   Resource   subj = work.createResource(String.format("tpl:%s", subjId));
	   Property   pred = work.createProperty("p8:hasRole");
	   RDFNode   obj =  work.createResource(String.format("tpl:%s", objId));
	   work.add(subj, pred, obj);
	   return work;
	}

	private Model generateHasTemplate(Model work, String subjId, String objId, Object gobj) {
		if (gobj instanceof RoleDefinition || gobj instanceof RoleQualification) {
	       Resource subj = work.createResource(String.format("tpl:%s", subjId));
	       Property pred = work.createProperty("p8:hasTemplate");
	       RDFNode obj = work.createResource(String.format("tpl:%s", objId));
	       work.add(subj, pred, obj);
		}
		return work;

	}

	private Model generateRoleIndex(Model work, String subjId, int index) throws Exception {
	    Resource subj = work.createResource(String.format("tpl:%s", subjId));
	    Property pred = work.createProperty("tpl:R97483568938");
	    RDFNode   obj = work.createTypedLiteral(Integer.toString(index), "xsd:integer");
	    work.add(subj, pred, obj);
		return work;
	}

	private Model generateRoleIndexPart8(Model work, String subjId, int index, Object gobj) throws Exception {
		if (gobj instanceof RoleDefinition || gobj instanceof RoleQualification) {
	      Resource subj = work.createResource(String.format("tpl:%s", subjId));
	      Property pred = work.createProperty("p8:valRoleIndex");
	      RDFNode  obj = work.createTypedLiteral(Integer.toString(index), "xsd:integer");
	      work.add(subj, pred, obj);
		}
		return work;
	}

	private Model generateRoleDomain(Model work, String subjId, String objId) {
	   Resource  subj = work.createResource(String.format("tpl:%s", subjId));
	   Property  pred = work.createProperty("rdfs:domain");
	   RDFNode   obj = work.createResource(String.format("tpl:%s", objId));
	   work.add(subj, pred, obj);
	   return work;
	}

	private Model generateRoleFillerType(Model work, String subjId, String qName) {
	   Resource subj = work.createResource(String.format("tpl:%s", subjId));
	   Property pred = work.createProperty("p8:hasRoleFillerType");
	   RDFNode  obj = work.createResource(qName);
	   work.add(subj, pred, obj);
	   return work;
	}

	private Model generateRoleCount(Model work, int rolecount, String subjId, Object gobj) throws Exception {
		if (gobj instanceof TemplateDefinition || gobj instanceof TemplateQualification) {
	       Resource subj = work.createResource(String.format("tpl:%s", subjId));
	       Property pred = work.createProperty("tpl:R35529169909");
	       RDFNode   obj = work.createTypedLiteral(Integer.toString(rolecount), "xsd:integer");
	       work.add(subj, pred, obj);
		}
		return work;
	}

	private Model generateRoleCountPart8(Model work, int rolecount, String subjId, Object gobj) throws Exception {
		if (gobj instanceof TemplateDefinition || gobj instanceof TemplateQualification) {
	       Resource subj = work.createResource(String.format("tpl:%s", subjId));
	       Property pred = work.createProperty("p8:valNumberOfRoles");
	       RDFNode   obj = work.createTypedLiteral(Integer.toString(rolecount), "xsd:integer");
	       work.add(subj, pred, obj);
		}
		return work;
	}

	private Model generateTypesPart8(Model work, String subjId, String objectId, Object gobj) throws Exception {
	      if (gobj instanceof TemplateDefinition) {
	        Resource subj = work.createResource(String.format("tpl:%s", subjId));
	        Property pred = work.createProperty(rdfType);
	        RDFNode  obj = work.createResource("owl:Thing");
	        work.add(subj, pred, obj);
	        pred = work.createProperty(rdfssubClassOf);
	        obj = work.createResource("p8:BaseTemplateStatement");
	        work.add(subj, pred, obj);
	      }
	      else if (gobj instanceof RoleQualification)
	      {
	        if (((RoleQualification)gobj).getRange() != null)
	          qName = _nsmap.reduceToQName(((RoleQualification)gobj).getRange());
	        else
	          qName = _nsmap.reduceToQName(((RoleQualification)gobj).getValue().getReference());
	        Resource subj = work.createResource(String.format("tpl:%s", subjId));
	        Property pred = work.createProperty(rdfType);
	        RDFNode   obj = work.createResource("owl:Thing");
	        work.add(subj, pred, obj);
	        obj = work.createResource("p8:TemplateRoleDescription");
	        work.add(subj, pred, obj);
	        pred = work.createProperty("p8:hasTemplate");
	        obj = work.createResource(String.format("tpl:%s", objectId));
	        work.add(subj, pred, obj);
	        pred = work.createProperty("p8:hasRoleFillerType");
	        obj = work.createResource(qName);
	        work.add(subj, pred, obj);
	      }
	      else if (gobj instanceof RoleDefinition)
	      {
	        if (((RoleDefinition)gobj).getRange() != null)
	          qName = _nsmap.reduceToQName(((RoleDefinition)gobj).getRange());
	          Resource subj = work.createResource(String.format("tpl:%s", subjId));
	          Property pred = work.createProperty(rdfType);
	          RDFNode   obj = work.createResource("owl:Thing");
	          work.add(subj, pred, obj);
	          obj = work.createResource("p8:TemplateRoleDescription");
	          work.add(subj, pred, obj);
	          pred = work.createProperty("p8:hasTemplate");
	          obj = work.createResource(String.format("tpl:%s", objectId));
	          work.add(subj, pred, obj);
	          pred = work.createProperty("p8:hasRoleFillerType");
	          obj = work.createResource(qName);
	          work.add(subj, pred, obj);
	      }
	      else if (gobj instanceof TemplateQualification)
	      {
	        Resource subj = work.createResource(String.format("tpl:%s", subjId));
	        Property pred = work.createProperty(rdfType);
	        RDFNode   obj = work.createResource("owl:Thing");
	        work.add(subj, pred, obj);
	        pred = work.createProperty(rdfssubClassOf);
	        obj = work.createResource("p8:BaseTemplateStatement");
	        work.add(subj, pred, obj);
	        pred = work.createProperty(rdfssubClassOf);
	        obj = work.createResource(objectId);
	        work.add(subj, pred, obj);
	      }
	      else if (gobj instanceof ClassDefinition)
	      {
	        Resource subj = work.createResource(String.format("rdl:%s", subjId));
	        Property pred = work.createProperty(rdfType);
	        RDFNode  obj = work.createResource(objectId);
	        work.add(subj, pred, obj);
	        pred = work.createProperty(rdfType);
	        obj = work.createResource("owl:Class");
	        work.add(subj, pred, obj);
	      }
		return work;
	}

	private Model generateClassName(Model work, Name name, String subjId, Object gobj) {
	   Resource   subj = work.createResource(String.format("rdl:%s", subjId));
	   Property   pred = work.createProperty("rdfs:label");
	   RDFNode    obj =   work.createLiteral(name.getValue(), (name.getLang() == null || name.getLang() == "") ? defaultLanguage : name.getLang());
	   work.add(subj, pred, obj);
       return work;
	}

	private Model generateDescription(Model work, Description descr, String subjectId) {
	   Resource subj = work.createResource(String.format("tpl:%s", subjectId));
	   Property pred = work.createProperty("rdfs:comment");
	   RDFNode   obj =  work.createLiteral(descr.getValue(), (descr.getLang() == null || descr.getLang() == "") ? defaultLanguage : descr.getLang());
       work.add(subj, pred, obj);
	   return work;
	}

	private Model generateClassDescription(Model work, Description descr, String subjectId) {

	     Resource subj = work.createResource(String.format("rdl:%s", subjectId));
	     Property pred = work.createProperty("rdfs:comment");
	     RDFNode obj = work.createLiteral(descr.getValue(), (descr.getLang() == null || descr.getLang() == "") ? defaultLanguage : descr.getLang());
	     work.add(subj, pred, obj);
		
		return work;
	}

	private Model generateRdfType(Model work, String subjId, String objId) {

		return work;
	}

	private Model generateRdfSubClass(Model work, String subjId, String objId) {
	   Resource subj = work.createResource(String.format("rdl:%s", subjId));
	   Property pred = work.createProperty("rdfs:subClassOf");
	   RDFNode   obj = work.createResource(objId);
	   work.add(subj, pred, obj);
	   return work;
	}

	private Model generateSuperClass(Model work, String subjId, String objId) {
	    Resource subj = work.createResource(subjId);
	    Property pred = work.createProperty("rdfs:subClassOf");
	    RDFNode   obj = work.createResource(String.format("rdl:%s", objId));
	    work.add(subj, pred, obj);
		return work;
	}

    private Model GenerateTemplateSpesialization(Model work, String templateID, String qualifies, TemplateQualification oldTQ)
    {
      Resource subj = work.createResource(String.format("tpl:%s", templateID));
      Property pred = work.createProperty(rdfssubClassOf);
      RDFNode   obj = work.createResource(String.format("tpl:%s", qualifies));
      work.add(subj, pred, obj);
      return work;
    }
	
    private Model generateTemplateSpesialization(Model work, String templateID, String qualifies, TemplateQualification oldTQ)
    {
      Resource subj = work.createResource(String.format("tpl:%s", templateID));
      Property pred = work.createProperty(rdfssubClassOf);
      RDFNode   obj = work.createResource(String.format("tpl:%s", qualifies));
      work.add(subj, pred, obj);
      return work;
    }
	
	private Model generateDmClassification(Model work, String subjId, String objId) {
		Resource subj = work.createResource(String.format("rdl:%s", subjId));
	    Property  pred = work.createProperty("dm:hasClassified");
	    RDFNode  obj = work.createResource(objId);
	    work.add(subj, pred, obj);
	    pred = work.createProperty("dm:hasClassifier");
	    work.add(subj, pred, obj);
		return work;
	}

	private Model generateDmSubClass(Model work, String subjId, String objId) {
	    Resource  subj = work.createResource(String.format("rdl:%s", subjId));
	    Property  pred = work.createProperty("dm:hasSubclass");
	    RDFNode  obj = work.createResource(objId);
	    work.add(subj, pred, obj);
		return work;
	}

}