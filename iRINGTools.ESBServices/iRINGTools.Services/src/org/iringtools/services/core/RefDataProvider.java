package org.iringtools.services.core;

import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.UnsupportedEncodingException;
import java.net.URI;
import java.net.URISyntaxException;
import java.net.URLEncoder;
import java.util.ArrayList;
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
//			List<QueryItem> items = _queries.getItems();
//			for (QueryItem qry : items) {
//				if (qry.getKey().equals("GetClass")) {
//					queryContainsSearch = qry.getQuery();
//					break;
//				}
//			}
			QueryBindings queryBindings = queryContainsSearch.getBindings();

			sparql = ReadSPARQL(queryContainsSearch.getFileName());

			if ( namespaceUrl == null || namespaceUrl.isEmpty())
				namespaceUrl = _nsmap.GetNamespaceUri("rdl").toString();

			String uri = namespaceUrl + id;

			sparql = sparql.replace("param1", uri);
			for (Repository repository : _repositories) {
				ClassDefinition classDefinition = null;

				if (rep != null)
					if (rep.getName().equals(repository.getName())) {
						continue;
					}
				Sparql sparqlResult = queryFromRepository(repository, sparql);
				if (sparqlResult != null) {
					Results res = sparqlResult.getResults();

					List<Hashtable<String, String>> results = bindQueryResults(
							queryBindings, res);

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
							URI typeName = new URI(
									result.get("type")
											.substring(
													0,
													result.get("type").indexOf(
															"#") + 1));
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
					Sparql sparqlResults = queryFromRepository(repository,
							sparqlPart8);
					if (sparqlResults != null) {
						res = sparqlResults.getResults();
						List<Hashtable<String, String>> results = bindQueryResults(
								queryBindingsPart8, res);

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
					}
				} else {
					Sparql sparqlResults = queryFromRepository(repository,
							sparql);
					if (sparqlResults != null) {
						res = sparqlResults.getResults();
						List<Hashtable<String, String>> results = bindQueryResults(
								queryBindings, res);

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
		Sparql sparqlResults = queryFromRepository(repository, sparql);
		if (sparqlResults != null) {
			res = sparqlResults.getResults();
		}
		List<Hashtable<String, String>> results = bindQueryResults(
				queryBindings, res);
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

	public final Qmxf getTemplate(String id) {
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
			// _logger.Error("Error in GetTemplate: " + ex);
		}

		return qmxf;
	}

	private List<TemplateDefinition> getTemplateDefinition(String id,
			Repository repository) {
		// TODO Auto-generated method stub
		return null;
	}

	private List<TemplateQualification> getTemplateQualification(String id,
			Repository repository) {
		// TODO Auto-generated method stub
		return null;
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

	/*
	 * public RefDataEntities searchPageReset(String query, int startIdx,int
	 * pageLimit){
	 * 
	 * return new RefDataEntities(); }
	 * 
	 * public List<Entity> find(String query){ List<Entity> listEntities; }
	 */

	public Entities search(String query) {
		return null;
	}

	public Entities searchPage(String query, String start, String limit) {
		return null;
	}

	public Entities searchReset(String query) {
		return null;
	}

	public Entities getSuperClasses(String id) {
		return null;
	}

	public Entities getAllSuperClasses(String id) {
		return null;
	}

	/*
	 * public List<Entity> getSubClasses(String id) { }
	 * 
	 * public List<Entity> getClassTemplates(String id) {
	 * 
	 * }
	 */

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
			Sparql sparqlResult = queryFromRepository(repository, sparql);
			if (sparqlResult != null) {
				Results res = sparqlResult.getResults();
				List<Hashtable<String, String>> results = bindQueryResults(
						queryBindings, res);
				for (Hashtable<String, String> result : results) {
					if (result.containsKey("label")) {
						label = result.get("label");
					}
				}
			}
		}
		return label;
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

	private Sparql queryFromRepository(Repository repository, String sparql)
			throws HttpClientException, UnsupportedEncodingException {
		Sparql sparqlResults = null;
		String message = "query=" + URLEncoder.encode(sparql, "UTF-8");
		try {
			String baseUri = repository.getUri();
			// TODO need to look at credentials
			NetworkCredentials credentials = new NetworkCredentials();
			HttpClient sparqlClient = new HttpClient(baseUri);
			sparqlClient.setNetworkCredentials(credentials);
			sparqlResults = sparqlClient.PostMessage(Sparql.class, "", message);

		} catch (RuntimeException ex) {
			return sparqlResults = null;
		}
		return sparqlResults;
	}

	private Query getQuery(String queryName) {
		Query query = null;
		List<QueryItem> items = _queries.getItems();
		for (QueryItem qry : items) {
			if (qry.getKey().equals(queryName)) {
				query =  qry.getQuery();
				break;
			}
		}
		return query;
	}

}