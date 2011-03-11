package org.iringtools.services.core;

import java.io.FileNotFoundException;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Hashtable;
import java.util.List;
import javax.xml.bind.JAXBException;

import org.ids_adi.ns.qxf.model.ClassDefinition;
import org.ids_adi.ns.qxf.model.Description;
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
import org.iringtools.utility.NetworkCredentials;
import org.w3._2007.sparql.results.Binding;
import org.w3._2007.sparql.results.Result;
import org.w3._2007.sparql.results.Results;

import sun.misc.Version;

public class RefDataProvider {
	private Hashtable<String, String> settings;
	private List<Repository> _repositories = null;
	private Queries _queries = null;

	public RefDataProvider(Hashtable<String, String> settings) {
		try {
			this.settings = settings;
			this._repositories = this.getRepositories();
			this._queries = this.getQueries();
		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}

	public Queries getQueries() throws JAXBException, IOException,
			FileNotFoundException {
		String path = settings.get("baseDirectory")
				+ "/WEB-INF/data/Queries.xml";
		return JaxbUtil.read(Queries.class, path);
	}

	public Federation getFederation() throws JAXBException, IOException {
		String path = settings.get("baseDirectory")
				+ "/WEB-INF/data/federation.xml";
		return JaxbUtil.read(Federation.class, path);
	}

	public Response saveFederation(Federation federation) throws Exception {
		Response response = new Response();
		try {
			String path = settings.get("baseDirectory")
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

			String path = settings.get("baseDirectory")
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

			String path = settings.get("baseDirectory")
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
			String path = settings.get("baseDirectory")
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
			String path = settings.get("baseDirectory")
					+ "/WEB-INF/data/Sparqls/";

			String query = IOUtil.readString(path + queryName);

			return query;
		} catch (Exception ex) {
			throw ex;
		}
	}

	/*
	 * private SPARQLResults QueryFromRepository(Repository repository, String
	 * sparql) { try { SPARQLResults sparqlResults; String encryptedCredentials
	 * = repository.encryptedCredentials;
	 * 
	 * WebCredentials credentials = new WebCredentials(encryptedCredentials); if
	 * (credentials.isEncrypted) credentials.Decrypt(); sparqlResults =
	 * SPARQLClient.Query(repository.uri, sparql, credentials,
	 * _proxyCredentials); return sparqlResults; } catch (Exception ex) {
	 * //_logger.Error(string.Format("Failed to read repository['{0}']",
	 * repository.uri), ex); return new SPARQLResults(); } }
	 * 
	 * private String getLabel(String uri) { try { String label = null; String
	 * sparql = null; String relativeUri = null;
	 * 
	 * Query query =_queries["GetLabel"].getFileName();
	 * 
	 * //List<QueryItem> query = _queries.getItems(); QueryBindings
	 * queryBindings = query.getBindings(); sparql =
	 * ReadSPARQL(_queries.getFileName()); sparql = sparql.replace("param1",
	 * uri);
	 * 
	 * for (Repository repository:_repositories) { SPARQLResults sparqlResults =
	 * QueryFromRepository(repository, sparql);
	 * 
	 * List<Dictionary<String, String>> results =
	 * BindQueryResults(queryBindings, sparqlResults);
	 * 
	 * for (Dictionary<String, String> result : results) { if
	 * (result.get("label")) { label = result["label"]; } } }
	 * 
	 * return label; } catch (Exception e) {
	 * //logger.Error("Error in GetLabel: " + e); throw new
	 * Exception("Error while Getting Label for " + uri + ".\n" + e.toString(),
	 * e); }
	 * 
	 * }
	 */
	public Version getVersion() {
		return new Version();
	}

	
	public Qmxf GetClass(String id)
    {
		Qmxf qmxf = new Qmxf();
		String label = null;
		String comment = null;

      try
      {
        String sparql = null;

        ClassDefinition classDefinition;
        Name name;
        Description description;
        List<Specialization> specializations = new ArrayList<Specialization>();

        Query query = new Query();
        
        List<QueryItem> items = _queries.getItems();
		for (QueryItem qry : items) {
			if (qry.getKey().contains("GetPart8Class")) {
				query = qry.getQuery();
				break;
			}
		}

		
		QueryBindings queryBindings = query.getBindings();
		sparql = ReadSPARQL(query.getFileName());
		sparql = sparql.replace("param1", id);
		_repositories = getRepositories();

		for (Repository repository : _repositories) {
			Results sparqlResult = queryFromRepository(repository, sparql);
			
			List<Hashtable<String, String>> results = bindQueryResults(queryBindings, sparqlResult);
			classDefinition = new ClassDefinition();
			specializations = new ArrayList<Specialization>();
			
	        classDefinition.setId("http://rdl.rdlfacade.org/data#" + id);
	        classDefinition.setRepository(repository.getName());
	        name = new Name();
	          
			for (Hashtable<String, String> result : results) {
				description = new Description();
				if (result.containsKey("label")) {
					label = result.get("label");
					name.setLang(label);
				
				}
				if (result.containsKey("comment")) {
					comment = result.get("comment");
					description.setLang(comment);
				}
				
				classDefinition.getNames().add(name);
	            classDefinition.getDescriptions().add(description);
			}
			
			specializations = GetPart8Specializations(id);
	        classDefinition.setSpecializations(specializations);
	        
	        qmxf.getClassDefinitions().add(classDefinition);
		}

     }
      catch (Exception e)
      {
        //_logger.Error("Error in GetClass: " + e);
      }
      return qmxf;
    }
	
	private List<Specialization> GetPart8Specializations(String id)
    {

      String sparql = null;

      List<Specialization> specializations = new ArrayList<Specialization>();
      try
      {

        Query queryContainsSearch = new Query();
        
        List<QueryItem> items = _queries.getItems();
		for (QueryItem qry : items) {
			if (qry.getKey().contains("GetPart8Specialization")) {
				queryContainsSearch = qry.getQuery();
				break;
			}
		}

		
		QueryBindings queryBindings = queryContainsSearch.getBindings();
		sparql = ReadSPARQL(queryContainsSearch.getFileName());
		sparql = sparql.replace("param1", id);
		_repositories = getRepositories();
        
        
		for (Repository repository : _repositories) {
          Results sparqlResult = queryFromRepository(repository, sparql);

          List<Hashtable<String, String>> results = bindQueryResults(queryBindings, sparqlResult);

          for (Hashtable<String, String> result : results) 
          {
            Specialization specialization = new Specialization();

            String uri = null;
            String label = null;
            if (result.containsKey("uri"))
            {
              uri = result.get("uri");
              specialization.setReference(uri);
            }
            if (result.containsKey("label"))
            {
              label = result.get("label");
            }
            else
            {
              label = getLabel(uri);
            }

            specialization.setLabel(label);
            //Method to be implemented
            //Utility.SearchAndInsert(specializations, specialization, Specialization.sortAscending());
          }
        }

      }
      catch (Exception e)
      {
        //_logger.Error("Error in GetSpecializations: " + e);
      }
      return specializations;

    }


	public Qmxf GetTemplate(String id, Repository repository) {
		Qmxf federatedQmxf = new Qmxf();
		try {
			for (Repository repository1 : _repositories) {
				Qmxf qmxf = GetTemplate(id, repository);

				for (TemplateDefinition templateDefinition : qmxf
						.getTemplateDefinitions()) {
					if (templateDefinition != null)
						federatedQmxf.getTemplateDefinitions().add(
								templateDefinition);
				}
				for (TemplateQualification templateQualification : qmxf
						.getTemplateQualifications()) {
					if (templateQualification != null)
						federatedQmxf.getTemplateQualifications().add(
								templateQualification);
				}
			}
		} catch (Exception ex) {
			// _logger.Error("Error in GetTemplate: " + ex);
		}

		return federatedQmxf;
	}

	public Response PostTemplate(Qmxf qmxf) {
		return new Response();
	}

	public Response PostClass(Qmxf qmxf) {
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
	 * 
	 * public RefDataEntities search(String query) { }
	 * 
	 * public RefDataEntities searchPage(String query, String start, String
	 * limit) { }
	 * 
	 * public RefDataEntities searchReset(String query) { }
	 * 
	 * public List<Entity> getSuperClasses(String id) { }
	 * 
	 * public List<Entity> getAllSuperClasses(String id) { }
	 * 
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
			String uri = settings.get("idGenServiceUri");
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

	public String getLabel(String uri) {
		try {
			Query query = new Query();
			String label = "";
			String sparql = "";
			String relativeUri = "";
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
			_repositories = getRepositories();
			for (Repository repository : _repositories) {
				Results sparqlResult = queryFromRepository(repository, sparql);
				List<Hashtable<String, String>> results = bindQueryResults(
						queryBindings, sparqlResult);
				for (Hashtable<String, String> result : results) {
					if (result.containsKey("label")) {
						label = result.get("label");
					}
				}
			}

			return label;
		} catch (Exception ex) {
			return ex.toString();
		}

	}
	private List<Hashtable<String, String>> bindQueryResults(
			QueryBindings queryBindings, Results sparqlResults) {
		try {
			List<Hashtable<String, String>> results = new ArrayList<Hashtable<String, String>>();
			for(Result sparqlResult : sparqlResults.getResults()){
				Hashtable<String, String> result = new Hashtable<String, String>();
				String sortKey = "";
				for(Binding sparqlBinding : sparqlResult.getBindings()){
					for(QueryBinding queryBinding : queryBindings.getItems()){
						if(queryBinding.getName() == sparqlBinding.getName()){
							String bindingKey = queryBinding.getName();
							String bindingValue = "";
							String dataType = "";
							
							if(queryBinding.getType() == SPARQLBindingType.URI){
								bindingValue = sparqlBinding.getUri();
							}
							else if (queryBinding.getType() == SPARQLBindingType.LITERAL){
								bindingValue = sparqlBinding.getLiteral().getContent();
								dataType = sparqlBinding.getLiteral().getDatatype();
								sortKey = bindingValue;
							}
							
							if(result.containsKey(bindingKey)){
								bindingKey = makeUniqueKey(result, bindingKey);
							}
							
							result.put(bindingKey, bindingValue);
							
							if(!dataType.equals("") && dataType != null){
								result.put(bindingKey + "_dataType", bindingValue);
							}
						}
					}
				}
				results.add(result)	;
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
			for(int i = 2; i < Integer.MAX_VALUE; i++){
				String postFix = " (" + (new Integer(i)).toString() + ")";
				if(!hashtable.containsKey(duplicateKey + postFix)){
					newKey += postFix;
					break;
				}
			}
			return newKey;
		} catch (RuntimeException ex) {
			throw ex;
		}
	}

	private Results queryFromRepository(Repository repository, String sparql) throws HttpClientException {
		Results sparqlResults = null;
		try {		
			//String encryptedCredentials = repository.
			NetworkCredentials credentials = new NetworkCredentials();
			HttpClient sparqlClient = new HttpClient(repository.getUri(),credentials);
			sparqlResults = sparqlClient.post(Results.class, sparql);
			
		} catch (RuntimeException ex) {
			return sparqlResults;
		}
		return sparqlResults;
	}
}