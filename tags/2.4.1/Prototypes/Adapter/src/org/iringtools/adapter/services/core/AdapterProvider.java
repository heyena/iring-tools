package org.iringtools.adapter.services.core;

import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Hashtable;
import java.util.List;
import java.util.Map;

import javax.xml.bind.JAXBException;

import org.apache.log4j.Logger;
import org.iringtools.common.Version;
import org.iringtools.common.response.Level;
import org.iringtools.common.response.Messages;
import org.iringtools.common.response.Response;
import org.iringtools.common.response.Status;
import org.iringtools.ext.ResponseExtension;
import org.iringtools.library.Application;
import org.iringtools.library.Applications;
import org.iringtools.library.BindingConfiguration;
import org.iringtools.library.Scope;
import org.iringtools.library.Scopes;
import org.iringtools.mapping.GraphMap;
import org.iringtools.mapping.Mapping;
import org.iringtools.utility.JaxbUtils;

public class AdapterProvider {

	ResponseExtension _response = null;

	private boolean _isScopeInitialized = false;
	private boolean _isDataLayerInitialized = false;
	private Mapping _mapping = null;
	private GraphMap _graphMap = null;

	private static final Logger logger = Logger
			.getLogger(AdapterProvider.class);

	private Map<String, Object> _settings = null;

	public AdapterProvider(Map<String, Object> settings) {
		try {
			_settings = settings;
			// _repositories = getRepositories();
			// _queries = getQueries();
			// _nsmap = new NamespaceMapper();
			_response = new ResponseExtension();

			String dataPath = _settings.get("baseDirectory") + "WEB-INF/data/";
			_settings.put("dataPath", dataPath);

		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}

	}

	public Scopes getScopes() throws Exception {

		String scopesPath = _settings.get("dataPath") + "Scopes.xml";

		// System.out.println("scopesPath : "+scopesPath);

		/*
		 * Scopes newScope = new Scopes(); List<Scope> items = new
		 * ArrayList<Scope>(); newScope.setItems(items);
		 * 
		 * 
		 * List<Application> appitems = new ArrayList<Application>();
		 * 
		 * Application aap1 = new Application(); aap1.setName("ABC");
		 * 
		 * Application aap2 = new Application(); aap1.setName("DEF");
		 * 
		 * appitems.add(aap1); appitems.add(aap2);
		 * 
		 * 
		 * Applications applications = new Applications();
		 * applications.setItems(appitems);
		 * 
		 * 
		 * 
		 * Scope scope1= new Scope(); scope1.setName("12345_000");
		 * scope1.setApplications(applications);
		 * 
		 * items.add(scope1); Scope scope2= new Scope();
		 * scope2.setName("12345_000");
		 * 
		 * JaxbUtils.write(newScope, scopesPath, true);
		 */

		try {

			// System.out.println("scopePath:..... "+scopesPath);

			return JaxbUtils.read(Scopes.class, scopesPath);

		} catch (Exception ex) {
			logger.error(String.format("Error in GetScopes: {0}", ex));
			throw new Exception(String.format(
					"Error getting the list of scopes: {0}", ex));
		}

		//

		// return JaxbUtils.read(Scopes.class,
		// "D:/iRINGTools/iRINGTools.ESBServices/iRINGTools.Common/schemas/Scopes.xml");

	}

	public Version getVersion() {
		Version version = new Version();
		version.setBuild("1");
		version.setMajor("1.1");
		return version;
	}

	@SuppressWarnings("null")
	public Response updateScope(Scope updatedScope) {

		Scopes _scopes = null;
		ResponseExtension response = new ResponseExtension();
		Status status = null;
		Messages messages = new Messages();

		try {
			_scopes = getScopes();

		} catch (Exception e) {

			e.printStackTrace();
		}

		List<Scope> scopeList = _scopes.getItems();

		boolean findScope = false;

		if (_scopes != null && scopeList.size() > 0) {

			status = new Status();

			try {

				for (Scope scope : scopeList) {

					if (scope.getName()
							.equalsIgnoreCase(updatedScope.getName())) {

						findScope = true;

						scope.setDescription(updatedScope.getDescription());
						scope.setApplications(updatedScope.getApplications());

						/*
						 * for (Application updatedApplication :
						 * updatedScope.getApplications().getItems()) {
						 * 
						 * Application findApplication = null;
						 * 
						 * for (Application application :
						 * scope.getApplications().getItems()) {
						 * 
						 * if (application.getName().equalsIgnoreCase(
						 * updatedApplication.getName())) {
						 * 
						 * findApplication = updatedApplication;
						 * 
						 * application
						 * .setDescription(updatedApplication.getDescription());
						 * 
						 * } }
						 * 
						 * if (findApplication == null) {
						 * 
						 * scope.getApplications().getItems().add(updatedApplication
						 * ); }
						 * 
						 * }
						 */

					}
				}

				if (findScope == false) {

					_scopes.getItems().add(updatedScope);

				}

				String scopesPath = _settings.get("dataPath") + "Scopes.xml";
				JaxbUtils.write(_scopes, scopesPath, true);
				messages.getItems().add(
						"Scopes have been updated successfully.");
				status.setLevel(Level.SUCCESS);
				status.setMessages(messages);

			} catch (Exception ex) {

				logger.error(String.format("Error in UpdateScopes: {0}", ex));
				status = new Status();
				status.setLevel(Level.ERROR);
				messages.getItems().add(
						String.format("Error saving scopes: {0}", ex));
				status.setMessages(messages);
			}

			response.append(status);

		} else {

			return response;

		}
		return response;
	}

	public Response updateScopes(Scopes scopes) {

		Status status = new Status();
		ResponseExtension response = new ResponseExtension();
		Messages messages = new Messages();

		String scopesPath = _settings.get("dataPath") + "Scopes.xml";

		try {

			JaxbUtils.write(scopes, scopesPath, true);
			messages.getItems().add("Scopes have been updated successfully.");
			status.setLevel(Level.SUCCESS);
			status.setMessages(messages);

		} catch (JAXBException e) {

			// TODO Auto-generated catch block
			e.printStackTrace();
			logger.error(String.format("Error in UpdateScopes: {0}", e));
			status.setLevel(Level.ERROR);
			messages.getItems().add(
					String.format("Error saving scopes: {0}", e));
			status.setMessages(messages);

		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();

		}

		response.append(status);

		return response;

	}

	public Mapping getMapping(String scopeName, String applicationName) {
		// TODO Auto-generated method stub

		try {
			initializeScope(scopeName, applicationName);
		} catch (Exception ex) {
			logger.error(String.format("Error in GetMapping: %1$s", ex));
			// throw new Exception(String.format("Error getting mapping: {0}",
			// ex));
		}
		return _mapping;
	}

	private void initializeScope(String scopeName, String applicationName) {
		initializeScope(scopeName, applicationName, true);
	}

	private void initializeScope(String scopeName, String applicationName,
			boolean loadDataLayer) {
		/*
		 * try { if (!_isScopeInitialized) { boolean isScopeValid = false; for
		 * (Scope project : _scopes.getItems()) { if
		 * (project.getName().equalsIgnoreCase(scopeName)) { for (Application
		 * application : project.getApplications().getItems()) { if
		 * (application.getName().equalsIgnoreCase(applicationName)) {
		 * isScopeValid = true; } } } }
		 * 
		 * // 12345_000.ABC String scope = String.format("%1$s.%2$s", scopeName,
		 * applicationName);
		 * 
		 * if (!isScopeValid) { throw new
		 * RuntimeException(String.format("Invalid scope [%1$s].", scope)); }
		 * _settings.put("scopeName",scopeName);
		 * _settings.put("ApplicationName",applicationName);
		 * _settings.put("Scope",scope);
		 * 
		 * // String appSettingsPath = String.format("%1$s%2$s.config",
		 * _settings.get("XmlPath"), scope);
		 * 
		 * 
		 * String mappingPath = _settings.get("baseDirectory") +
		 * "WEB-INF/data/Mapping." + scopeName + "." + applicationName + ".xml";
		 * _settings.put("mapping." + scopeName + "." + applicationName,
		 * JaxbUtils.read(Mapping.class,mappingPath).toString());
		 * 
		 * if ((new File(appSettingsPath)).exists()) { { AppSettingsReader
		 * appSettings = new AppSettingsReader(appSettingsPath);
		 * _settings.AppendSettings(appSettings); }
		 * 
		 * //String relativePath =
		 * String.format("%1$sBindingConfiguration.%2$s.xml",
		 * _settings.get("XmlPath"), scope);
		 * 
		 * 
		 * //Ninject Extension requires fully qualified path. String
		 * bindingConfigurationPath = Path.Combine(
		 * _settings.get("BaseDirectoryPath"), relativePath );
		 * 
		 * 
		 * 
		 * String bindingConfigurationPath = _settings.get("xmlPath") +
		 * "BindingConfiguration." + scopeName + "." + applicationName + ".xml";
		 * 
		 * _settings.put("bindingConfigurationPath." + scopeName + "." +
		 * applicationName,
		 * JaxbUtils.read(BindingConfiguration.class,bindingConfigurationPath
		 * ).toString());
		 * 
		 * //_settings.put("BindingConfigurationPath" ,
		 * bindingConfigurationPath);
		 * 
		 * if (loadDataLayer) {
		 * 
		 * if (!new File(bindingConfigurationPath).exists()) {
		 * BindingConfiguration binding = new BindingConfiguration();
		 * binding.setBindingName(_settings.get("Scope"));
		 * binding.setInterfaceClass("org.iringtools.library.IDataLayer");
		 * binding.setImplementationClass("HibernateDataLayer");
		 * binding.setLocation
		 * ("org.iringtools.adapter.datalayer.HibernateDataLayer");
		 * 
		 * JaxbUtils.write(BindingConfiguration.class, bindingConfigurationPath,
		 * false);
		 * 
		 * //binding.Save(bindingConfigurationPath); } // Will use objectLoader
		 * _kernel.Load(bindingConfigurationPath); }
		 * 
		 * _settings.put("DBDictionaryPath",String.format(
		 * "%1$sDatabaseDictionary.%2$s.xml", _settings.get("XmlPath"), scope));
		 * 
		 * String mappingPath = String.format("%1$sMapping.%2$s.xml",
		 * _settings.get("XmlPath"), scope);
		 * 
		 * if (new File(mappingPath).exists()) { try { _mapping =
		 * JaxbUtils.read(Mapping.class,mappingPath); } catch (Exception
		 * legacyEx) { Status status = new Status();
		 * 
		 * //*** _mapping = LoadMapping(mappingPath, ref status);
		 * 
		 * logger.info(status.toString()); } } else { _mapping = new Mapping();
		 * JaxbUtils.write(_mapping, mappingPath,false); }
		 * 
		 * // Will use objectLoader
		 * _kernel.Bind<mapping.Mapping>().ToConstant(_mapping);
		 * 
		 * _isScopeInitialized = true; } } catch (Exception ex) {
		 * logger.error(String.format("Error initializing application: %1$s",
		 * ex)); throw new
		 * Exception(String.format("Error initializing application: %1$s)",
		 * ex)); }
		 */
	}

	public Response updateApplication(String scopeName,
			Application updatedApplication) {

		Scopes _scopes = null;
		ResponseExtension response = new ResponseExtension();
		Status status = null;
		Messages messages = new Messages();
		String actionType = null;

		try {
			_scopes = getScopes();

		} catch (Exception e) {

			e.printStackTrace();
		}

		List<Scope> scopeList = _scopes.getItems();

		if (_scopes != null && scopeList.size() > 0) {

			status = new Status();

			try {

				Scope findScope = null;

				for (Scope scopes : scopeList) {

					if (scopes.getName().equalsIgnoreCase(scopeName)) {
						findScope = scopes;
						break;
					}
				}

				if (findScope != null) {

					Application foundApplication = null;

					for (Application appl : findScope.getApplications()
							.getItems()) {

						if (appl.getName().equalsIgnoreCase(
								updatedApplication.getName())) {

							foundApplication = appl;
							actionType = "updated";
							break;
						}

					}

					if (foundApplication != null) {

						foundApplication.setDescription(updatedApplication
								.getDescription());
						actionType = "added";

					}

					if (foundApplication == null) {
						System.out.println("After Add......");
						findScope.getApplications().getItems()
								.add(updatedApplication);

					}

					String scopesPath = _settings.get("dataPath") + "Scopes.xml";
					JaxbUtils.write(_scopes, scopesPath, true);
					messages.getItems().add(
							"Application have been " + actionType
									+ " successfully.");
					status.setLevel(Level.SUCCESS);
					status.setMessages(messages);
					
				} else {

					// No scope matched
				}

			} catch (Exception ex) {

				logger.error(String.format("Error in Update Application: {0}",
						ex));
				status = new Status();
				status.setLevel(Level.ERROR);
				messages.getItems().add(
						String.format("Error saving Application: {0}", ex));
				status.setMessages(messages);

			}
		}
		
		response.append(status);

		return response;
	}
}
