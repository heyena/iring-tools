package org.iringtools.adapter.datalayer;

import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Dictionary;
import java.util.List;
import java.util.Properties;

import javax.xml.bind.JAXBException;

import org.apache.axiom.om.OMElement;
import org.apache.axiom.om.impl.llom.util.AXIOMUtil;
import org.apache.log4j.Logger;
import org.hibernate.HibernateException;
import org.hibernate.Query;
import org.hibernate.SQLQuery;
import org.hibernate.Session;
import org.hibernate.SessionFactory;
import org.hibernate.cfg.Configuration;
import org.iringtools.adapter.AdapterSettings;
import org.iringtools.adapter.datalayer.proj_12345_000.ABC.LINES;
import org.iringtools.common.response.Level;
import org.iringtools.common.response.Messages;
import org.iringtools.common.response.Response;
import org.iringtools.common.response.Result;
import org.iringtools.common.response.Status;
import org.iringtools.data.filter.DataFilter;
import org.iringtools.data.filter.Expression;
import org.iringtools.data.filter.Expressions;
import org.iringtools.data.filter.LogicalOperator;
import org.iringtools.data.filter.RelationalOperator;
import org.iringtools.data.filter.Values;
import org.iringtools.directory.Authorized;
import org.iringtools.ext.ResponseExtension;
import org.iringtools.hibernate.EntityGenerator;
import org.iringtools.library.AuthroziedUsers;
import org.iringtools.library.DataDictionary;
import org.iringtools.library.DataObject;
import org.iringtools.library.DataProperty;
import org.iringtools.library.DataRelationship;
import org.iringtools.library.DataType;
import org.iringtools.library.DatabaseDictionary;
import org.iringtools.library.IDataLayer2;
import org.iringtools.library.IDataObject;
import org.iringtools.library.IdentityProperties;
import org.iringtools.library.KeyProperty;
import org.iringtools.library.KeyType;
import org.iringtools.library.PropertyMap;
import org.iringtools.library.Provider;
import org.iringtools.library.Providers;
import org.iringtools.library.RelationshipType;
import org.iringtools.library.Relationships;
import org.iringtools.library.StatusLevel;
import org.iringtools.library.VersionInfo;
import org.iringtools.utility.JaxbUtils;
import org.iringtools.utility.Utility;

public class HibernateDataLayer implements IDataLayer2 {
	private static final Logger logger = Logger
			.getLogger(HibernateDataLayer.class);

	boolean _isScopeInitialized = false;
	private String _dataDictionaryPath = "";
	private String _databaseDictionaryPath = "";
	private DataDictionary _dataDictionary;
	private DatabaseDictionary _databaseDictionary;
	private Dictionary _keyRing = null;
	private String _authorizationPath = "";
	private ResponseExtension _response = null;
	private String _hibernateConfigPath = "";
	private SessionFactory _sessionFactory;

	protected AdapterSettings _settings = null;
	
	 public HibernateDataLayer(AdapterSettings settings, Dictionary keyRing)
	    {
	      //_kernel = new StandardKernel(new NHibernateModule());
	      //_nSettings = _kernel.Get<NHibernateSettings>();
	      //_nSettings.AppendSettings(settings);
	      _settings = settings;
	      _keyRing = keyRing;
	      _response = new ResponseExtension();
	     // _kernel.Bind<Response>().ToConstant(_response);

	     // _adapterProvider = new AdapterProvider(_settings);

	      
	      
	      _hibernateConfigPath = String.format("{0}nh-configuration.{1}.xml",
	    	_settings.get("XmlPath"),
	        _settings.get("Scope")
	      );

	      String hibernateMappingPath = String.format("{0}nh-mapping.{1}.xml",
	    	_settings.get("XmlPath"),
	    	_settings.get("Scope")
	      );

	      _dataDictionaryPath = String.format("{0}DataDictionary.{1}.xml",
	    	_settings.get("XmlPath"),
	    	_settings.get("Scope")
	      );

	      String _databaseDictionaryPath = String.format("{0}DatabaseDictionary.{1}.xml",
	    	_settings.get("XmlPath"),
	    	_settings.get("Scope")
	      );

	      if ((new java.io.File(_databaseDictionaryPath)).isFile())
			try {
				_databaseDictionary = JaxbUtils.read(DatabaseDictionary.class,_databaseDictionaryPath);
			} catch (JAXBException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}

	      if ((new java.io.File(_hibernateConfigPath)).isFile() && (new java.io.File(hibernateMappingPath)).isFile())
	        _sessionFactory = new Configuration()
	          .configure(_hibernateConfigPath)
	          .addFile(hibernateMappingPath)
	          .buildSessionFactory();

	      _authorizationPath = String.format("{0}Authorization.{1}.xml",
	        _settings.get("DataPath"),
	        _settings.get("Scope")
	      );
	    }
	
	private DataFilter filterByIdentity(String objectType, DataFilter filter, IdentityProperties identityProperties)
	{
		
		DataObject dataObject = null;
		
		  for (DataObject dataObj : _databaseDictionary.getDataObjects())
		  {
			  if (dataObj.getObjectName().equalsIgnoreCase(objectType))
			  {
				  dataObject = dataObj;
				  break;
			  }
		  }
		  
		  DataProperty dataProperty = null;
		  
		  for(DataProperty dataProp : dataObject.getDataProperties()){
			  
			  if(dataProp.getPropertyName().equalsIgnoreCase(identityProperties.getIdentityProperty())){
				  
				  dataProperty = dataProp;
			  }
		  }
		  
	      if (dataProperty != null)
	      {
	        if (filter == null)
	        {
	          filter = new DataFilter();
	        }
	        
	        boolean hasExistingExpression = false;
	        
	        if (filter.getExpressions() == null)
	        {
	          filter.setExpressions(new Expressions());
	        }
	        else if (filter.getExpressions().getItems().size() > 0)
	        {
	        	List<Expression> expressionList = filter.getExpressions().getItems();
	        	
	          Expression firstExpression = expressionList.get(0);
	          Expression lastExpression = expressionList.get(expressionList.size()-1);

	          firstExpression.setOpenGroupCount(firstExpression.getOpenGroupCount()+1);
	          lastExpression.setCloseGroupCount(firstExpression.getCloseGroupCount()+1);
	          hasExistingExpression = true;
	        }
	        
	        String identityValue = _keyRing.get(identityProperties.getKeyRingProperty()).toString();
	        Expression expression = new Expression();
	        expression.setPropertyName(dataProperty.getPropertyName());
	        expression.setRelationalOperator(RelationalOperator.EQUAL_TO);
	        
	        List<String> items = new ArrayList<String>();
	        items.add(identityValue);
	        Values values = new Values();
	        values.setItems(items);
	        expression.setValues(values);
	        expression.setIsCaseSensitive(identityProperties.getIsCaseSensitive());

	        if (hasExistingExpression)
	          expression.setLogicalOperator(LogicalOperator.AND);
	        filter.getExpressions().getItems().add(expression);
	      }
	      return filter;
	}
	
	@Override
	public List<IDataObject> create(String objectType, List<String> identifiers) {
		// TODO 
		
		if (!isAuthorized())
	        try{
	        	throw new UnauthorizedAccessException("User not authorized to access NHibernate data layer.");
	        }catch(UnauthorizedAccessException e){
	        	e.printStackTrace();
	        }
	    
	      try
	      {
	        List<IDataObject> dataObjects = new ArrayList<IDataObject>();
	        DataObject dictionaryObject = null;
	        
	   
	        
	        for(DataObject dictionaryObj : _dataDictionary.getDataObjects()){
	        	
	        	if(dictionaryObj.getObjectName().equalsIgnoreCase(objectType)){
	        		
	        		dictionaryObject = dictionaryObj;
	        		break;
	        	}
	        }
	      
	        
	        Class<?> type = Class.forName(dictionaryObject.getObjectName() + "." + objectType + ", " + _settings.get("ExecutingAssemblyName"));
	        //Type type = Type.GetType(dictionaryObject.objectNamespace + "." + objectType + ", " + _settings["ExecutingAssemblyName"]);
	        
	        IDataObject dataObject = null;

	        if (identifiers != null)
	        {
	          Session session = OpenSession();

	          for (String identifier : identifiers)
	          {
	            if (identifier != null || !identifier.isEmpty() )
	            {
	              Query query = session.createQuery("from " + objectType + " where Id = ?");
	              
	              query.setString(0, identifier);
	              
	              //dataObject = query.List<IDataObject>().FirstOrDefault<IDataObject>();
	              
	              dataObject = (IDataObject)query.list().get(0);

	              if (dataObject == null)
	              {
	            	 dataObject = (IDataObject) type.newInstance();
	                
	                dataObject.setPropertyValue("Id", identifier);
	              }
	            }
	            else
	            {
	              dataObject = (IDataObject)type.newInstance();
	            }

	            dataObjects.add(dataObject);
	          }
	        }
	        else
	        {
	          dataObject = (IDataObject)type.newInstance();
	          dataObjects.add(dataObject);
	        }

	        return dataObjects;
	      }
	      catch (Exception ex)
	      {
	        logger.error("Error in CreateList: " + ex);
	        throw new RuntimeException(String.format("Error while creating a list of data objects of type [{0}]. {1}", objectType, ex));
	      }
	}

	@Override
	public long getCount(String objectType, DataFilter filter){
		// TODO Auto-generated method stub

		if (!isAuthorized())
			try {
				throw new UnauthorizedAccessException("User not authorized to access NHibernate data layer.");
			} catch (UnauthorizedAccessException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
	    

	        try
	        {
	          if (_databaseDictionary.getIdentityConfiguration() != null)
	          {
	            IdentityProperties identityProperties = _databaseDictionary.getIdentityConfiguration().get(objectType);

	            if (identityProperties.getUseIdentityFilter())
	            {
	              filter = filterByIdentity(objectType, filter, identityProperties);
	            }
	          }

	          StringBuilder queryString = new StringBuilder();
	          queryString.append("select count(*) from " + objectType);

	          if (filter != null && filter.getExpressions().getItems().size() > 0)
	          {
	            filter.getOrderExpressions().getItems().clear();
	            // TODO
	            String whereClause = "";//////filter.ToSqlWhereClause(_dataDictionary, objectType, null);
	            queryString.append(whereClause);
	          }

	          Session session = OpenSession();
	          Query query = session.createQuery(queryString.toString());
	          
	          //long count = query.List<long>().First();
	          
	          long count = Long.parseLong(query.list().get(0).toString());


	          return count;
	          
	        }
	        catch (Exception ex)
	        {
	          logger.error("Error in GetIdentifiers: " + ex);
	          throw new RuntimeException(String.format("Error while getting a list of identifiers of type [%1$s]. %2$s", objectType, ex));
	        }
	}

	@Override
	// TODO Auto-generated method stub

	public List<String> getIdentifiers(String objectType, DataFilter filter) {

		List<String> identifiers = null;
		try {
			Session session = OpenSession();
			session.beginTransaction();

			StringBuilder queryString = new StringBuilder();
			queryString.append("from " + objectType);

			@SuppressWarnings("unchecked")
			List<LINES> lists = session.createQuery(queryString.toString())
					.list();
			identifiers = new ArrayList<String>();

			for (LINES t : lists) {

				identifiers.add(t.getId());
			}

		} catch (HibernateException ex) {
			logger.error("Error in getIdentifiers: " + ex);
			 throw new
			 HibernateException(String.format("Error while getting a list of identifiers of type [%1$s]. %2$s",
			 objectType, ex));
		}
		return identifiers;
	}

	public static String join(List<String> coll, String delimiter) {
		if (coll.isEmpty())
			return "";

		StringBuilder sb = new StringBuilder();
		for (String x : coll)
			sb.append(x + delimiter);
		sb.delete(sb.length() - delimiter.length(), sb.length());
		return sb.toString();
	}

	@Override
	public List<IDataObject> get(String objectType, List<String> identifiers) {

		List<IDataObject> orderedDataObjects = new ArrayList<IDataObject>();
		StringBuilder queryString = new StringBuilder();
		queryString.append("from " + objectType);

		if (identifiers != null && identifiers.size() > 0) {
			queryString.append(" where Id in ('" + join(identifiers, "','")
					+ "')");
		}
		try {
			Session session = OpenSession();
			session.beginTransaction();
			Query query = session.createQuery(queryString.toString());
			@SuppressWarnings("unchecked")
			List<IDataObject> dataObjects = (List<IDataObject>) query.list();

			// order data objects as list of identifiers
			if (identifiers != null) {
				for (String identifier : identifiers) {
					for (IDataObject dataObject : dataObjects) {
						String id = (String) dataObject.getPropertyValue("Id");

						if (id != null && id.equalsIgnoreCase(identifier)) {
							orderedDataObjects.add(dataObject);
							break;
						}
					}
				}

			}

		} catch (HibernateException ex) {
			logger.error("Error in get: " + ex);
			 throw new
			 RuntimeException(String.format("Error while getting a list of identifiers of type [%1$s]. %2$s",
			 objectType, ex));
		}

		System.out.println(queryString);
		return orderedDataObjects;

		// return null;
	}

	@SuppressWarnings("unchecked")
	@Override
	public List<IDataObject> get(String objectType, DataFilter filter,
			int limit, int start) {
		
		List<IDataObject> dataObjects = new ArrayList<IDataObject>();
		
		try{
			//TODO
			if (!isAuthorized())
				try {
					throw new UnauthorizedAccessException("User not authorized to access NHibernate data layer.");
				} catch (UnauthorizedAccessException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
			
	          if (_databaseDictionary.getIdentityConfiguration() != null)
	          {
	            IdentityProperties identityProperties = _databaseDictionary.getIdentityConfiguration().get(objectType);

			  if (identityProperties.getUseIdentityFilter())
			  {
				filter = filterByIdentity(objectType, filter, identityProperties);
			  }
			}

			try {

				Session session = OpenSession();
				session.beginTransaction();
				StringBuilder queryString = new StringBuilder();
				queryString.append("from " + objectType);

				Query query = session.createQuery(queryString.toString());
				dataObjects = (List<IDataObject>) query.list();
				if (start + limit <= dataObjects.size()) {
					dataObjects = dataObjects.subList(start, start + limit);
				} else {
					dataObjects = dataObjects
							.subList(start, dataObjects.size());
				}
			} catch (Exception e) {
				logger.error("Error in Get: " + e);
			}
		} catch (Exception ex) {
			logger.error("Error in Get: " + ex);
			 throw new RuntimeException(String.format("Error while getting a list of data objects of type [%1$s]. %2$s",objectType, ex));
		}
		return dataObjects;
	}

	@Override
	public Response post(List<IDataObject> dataObjects) {
		// TODO Auto-generated method stub
		
		if (!isAuthorized())
			try {
				throw new UnauthorizedAccessException("User not authorized to access NHibernate data layer.");
			} catch (UnauthorizedAccessException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
			
			//Response response = new Response();
			ResponseExtension response = new ResponseExtension();

	        try
	        {
	          if (dataObjects != null && dataObjects.size() > 0)
	          {
	            Session session = OpenSession();
	            
	            
	              for (IDataObject dataObject : dataObjects)
	              {
	            	Status status = new Status();
	                status.setMessages(new Messages());

	                if (dataObject != null)
	                {
	                  String identifier = "";

	                  try
	                  {
	                    // NOTE: Id property is not available if it's not mapped and will cause exception
	                    identifier = dataObject.getPropertyValue("Id").toString();
	                  }
	                  catch (Exception ex) {
	  									logger.error(String.format("Error in Post: {0}", ex));
	    							}  // no need to handle exception because identifier is only used for statusing

	                  status.setIdentifier(identifier);

	                  try
	                  {
	                    session.saveOrUpdate(dataObject);
	                    session.flush();
	                    status.getMessages().getItems().add(String.format("Record [%1$s] have been saved successfully.", identifier));
	                  }
	                  catch (Exception ex)
	                  {
	                	  /*status.Level = StatusLevel.Error;
	                      status.Messages.Add(string.Format("Error while posting record [{0}]. {1}", identifier, ex));
	                      status.Results.Add("ResultTag", identifier);
	                      _logger.Error("Error in Post saving: " + ex);*/
	                      
	                    status.setLevel(Level.ERROR);
	                    status.getMessages().getItems().add(String.format("Error while posting record [%1$s]. %2$s", identifier, ex));
	                    
	                    Result result = new Result();
	                    result.setKey("ResultTag");
	                    result.setValue(identifier);
	                    
	                    status.getResults().getItems().add(result);
	  					logger.error("Error in Post saving: " + ex);				
	                  }
	                }
	                else
	                {
	                  status.setLevel(Level.ERROR);
	                  status.getMessages().getItems().add("Data object is null or duplicate.");
	                }

	                response.append(status);
	              }
	            }
	          

	          return response;
	        }
	        catch (Exception ex)
	        {
	          logger.error("Error in Post: " + ex);
	          Object sample = dataObjects.get(0);//.FirstOrDefault();
	          String objectType = (sample != null) ? sample.getClass().getName() : "";
	          throw new RuntimeException(String.format("Error while posting data objects of type [{0}]. {1}", objectType, ex));
	        }
	        
	}

	@Override
	public Response delete(String objectType, List<String> identifiers) {
		// TODO Auto-generated method stub

		if (!isAuthorized())
			try {
				throw new UnauthorizedAccessException("User not authorized to access NHibernate data layer.");
			} catch (UnauthorizedAccessException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
			
			ResponseExtension responseObj = new ResponseExtension();

	        try
	        {
	          List<IDataObject> dataObjects = create(objectType, identifiers);

	          Session session = OpenSession();
	          
	            for (IDataObject dataObject : dataObjects)
	            {
	              String identifier = dataObject.getPropertyValue("Id").toString();
	              session.delete(dataObject);

	              Status status = new Status();
	              status.setMessages(new Messages());
	              status.setIdentifier(identifier);
	  			 //status.Messages.Add(string.Format("Record [{0}] have been deleted successfully.", identifier));
	  			   status.getMessages().getItems().add(String.format("Record [{0}] have been deleted successfully.", identifier));

	  			 responseObj.append(status);
	            }

	            session.flush();
	          
	        }
	        catch (Exception ex)
	        {
	          logger.error("Error in Delete: " + ex);

	          Status status = new Status();
	          status.setLevel(Level.ERROR);
	          status.getMessages().getItems().add(String.format("Error while deleting data objects of type [{0}]. {1}", objectType, ex));
	          responseObj.append(status);
	        }

	        return responseObj;
	}

	@Override
	public Response delete(String objectType, DataFilter filter) {
		// TODO Auto-generated method stub
		
		if (!isAuthorized())
			try {
				throw new UnauthorizedAccessException("User not authorized to access NHibernate data layer.");
			} catch (UnauthorizedAccessException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
			
			ResponseExtension response = new ResponseExtension();
	        
	        //response.StatusList = new List<Status>();
	        
	        Status status = new Status();

	        try
	        {
	          if (_databaseDictionary.getIdentityConfiguration() != null)
	          {
	            //IdentityProperties identityProperties = _databaseDictionary.IdentityConfiguration[objectType];
	            IdentityProperties identityProperties = _databaseDictionary.getIdentityConfiguration().get(objectType);
	            
	            if (identityProperties.getUseIdentityFilter())
	            {
	              filter = filterByIdentity(objectType, filter, identityProperties);
	            }
	          }
	          status.setIdentifier(objectType);

	          StringBuilder queryString = new StringBuilder();
	          queryString.append("from " + objectType);

	          if (filter.getExpressions().getItems().size() > 0)
	          {
	        	  // TODO
	            String whereClause = "";//filter.ToSqlWhereClause(_dataDictionary, objectType, null);
	            queryString.append(whereClause);
	          }

	          	Session session = OpenSession();
	            session.delete(queryString);
	            session.flush();

	            //status.Messages.Add(string.Format("Records of type [{0}] has been deleted succesfully.", objectType));
	            
	            status.getMessages().getItems().add(String.format("Records of type [{0}] has been deleted succesfully.", objectType));
	        }
	        catch (Exception ex)
	        {
	          logger.error("Error in Delete: " + ex);
	          //throw new Exception(String.format("Error while deleting data objects of type [{0}]. {1}", objectType, ex));

	          //no need to status, thrown exception will be statused above.
	        }
	        response.append(status);
	        return response;
	}

	public DataDictionary getDictionary() {
		if ((new File(_dataDictionaryPath)).exists()) {
			// _dataDictionary =
			// Utility.<DataDictionary>read(_dataDictionaryPath);
			return _dataDictionary;
		} else {
			return new DataDictionary();
		}
	}

	@SuppressWarnings("unchecked")
	@Override
	public List<IDataObject> getRelatedObjects(IDataObject sourceDataObject,
			String relatedObjectType) {
		// TODO 
		
		
		if (!isAuthorized())
			try {
				throw new UnauthorizedAccessException("User not authorized to access NHibernate data layer.");
			} catch (UnauthorizedAccessException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
			
	     
		  List<IDataObject> relatedObjects = null;
	      
		  DataDictionary dictionary = getDictionary();

	      DataObject dataObject = null;
	      

	      List<DataObject> listofDataobject = dictionary.getDataObjects();
	     
	     if(listofDataobject.size()>0){
	    	 
		      for (DataObject dataobj : listofDataobject) {
		    
		    	  if(dataobj.getObjectName().equalsIgnoreCase(sourceDataObject.getClass().getName())){
		    		  dataObject = dataobj;
		    		  break;
		    	  }
		      }	    	 
	     }


	      DataRelationship dataRelationship =  null;
	      List<DataRelationship> listofDataRelationships = dataObject.getDataRelationships();
	      
	      if(listofDataRelationships.size()>0){
	      
	    	  for (DataRelationship dataRelations : listofDataRelationships) {
	    	  
	    		  if(dataRelations.getRelatedObjectName().equalsIgnoreCase(relatedObjectType)){
	    		  
	    		  dataRelationship =  dataRelations;
	    	  }
	    	  }
	      }
	    
	      StringBuilder sql = new StringBuilder();
	      sql.append("from " + dataRelationship.getRelatedObjectName() + " where ");

	      for (PropertyMap map : dataRelationship.getPropertyMaps())
	      {

	    	  //DataProperty propertyMap = dataObject.dataProperties.First(c => c.propertyName == map.dataPropertyName);
	    	  
	    	  DataProperty propertyMap = null;
	    	
		      for (DataProperty property : dataObject.getDataProperties())
		      {
		    	  if(property.getPropertyName().equalsIgnoreCase(map.getDataPropertyName())){

		    		  propertyMap =  property;
		    	  }
		      }

	        if (propertyMap.getDataType() == DataType.STRING)
	        {
	          sql.append(map.getRelatedPropertyName() + " = '" + sourceDataObject.getPropertyValue(map.getDataPropertyName()) + "' and ");
	        }
	        else
	        {
	          sql.append(map.getRelatedPropertyName() + " = " + sourceDataObject.getPropertyValue(map.getDataPropertyName()) + " and ");
	        }
	      }
	      	
	      	sql.replace(sql.length()-4, 4, sql.toString());

	      	Session session = OpenSession();
	        Query query = session.createQuery(sql.toString());
	        
	        //relatedObjects = (List<IDataObject>)query.list();
	        
	        relatedObjects = (List<IDataObject>)query.list();
	      
	      if (relatedObjects != null && relatedObjects.size() > 0 && dataRelationship.getRelationshipType() == RelationshipType.ONE_TO_ONE)
	      {
	    	  List<IDataObject> dataObjectlist = (List<IDataObject>) relatedObjects.get(0);

	    	  return dataObjectlist;
	      }

	      return relatedObjects;
		
	}
	
	@Override
	public Response configure(OMElement configuration) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public OMElement getConfiguration() {
		// TODO Auto-generated method stub
		return null;
	}

	public OMElement getConfiguration(String connectionInfo) {

		DatabaseDictionary databaseDictionary = null;

		try {
			databaseDictionary = new DatabaseDictionary();
			databaseDictionary.setConnectionString(connectionInfo);
		} catch (Exception ex) {
			logger.error("Error in GetConfiguration: " + ex);
		}

		return (OMElement) databaseDictionary;
	}

	public Response saveConfiguration(OMElement configuration) {
		
		  ResponseExtension _response = new ResponseExtension();
	      _response.setMessages(new Messages());
	      
	      String _projectName = _settings.get("Scope").split(".")[0];
	      String _applicationName = _settings.get("Scope").split(".")[1];
	      
	      try
	      {
	        _databaseDictionary = JaxbUtils.toObject(DatabaseDictionary.class, configuration.toString());
	        JaxbUtils.write(_databaseDictionary, _hibernateConfigPath, false);
	        _response = generate(_projectName, _applicationName);
	        _response.setLevel(Level.SUCCESS);
	        
	      }
	      catch (Exception ex)
	      {
	        _response.getMessages().getItems().add("Failed to Save datalayer Configuration");
	        _response.getMessages().getItems().add(ex.getMessage());
	        _response.setLevel(Level.ERROR);
	        logger.error("Error in SaveConfiguration: " + ex);
	      }
	      return _response;
	}

	public ResponseExtension generate(String projectName, String applicationName)
	    {
/*	      Status status = new Status();

	      try
	      {
	        status.setIdentifier(String.format("%1$s.%2$s", projectName, applicationName));

	        initializeScope(projectName, applicationName);

	        DatabaseDictionary dbDictionary = JaxbUtils.read(DatabaseDictionary.class,_settings.get("DBDictionaryPath"));

	        if ((projectName == null || projectName == "") || (applicationName == null || applicationName == ""))
	        {
	          status.getMessages().getItems().add("Error project name and application name can not be null");
	        }
	        else if (validateDatabaseDictionary(dbDictionary))
	        {
	          EntityGenerator generator = "";//_kernel.Get<EntityGenerator>();
	          _response.append(generator.Generate(dbDictionary, projectName, applicationName));

	          // Update binding configuration
	          XElement binding = new XElement("module",
	            new XAttribute("name", _settings["Scope"]),
	            new XElement("bind",
	              new XAttribute("name", "DataLayer"),
	              new XAttribute("service", "org.iringtools.library.IDataLayer, iRINGLibrary"),
	              new XAttribute("to", "org.iringtools.adapter.datalayer.NHibernateDataLayer, NHibernateLibrary")
	            )
	          );

	          Response localResponse = _adapterProvider.UpdateBinding(projectName, applicationName, binding);

	          _response.append(localResponse);

	          status.getMessages().getItems().add("Database dictionary updated successfully.");
	        }
	      }
	      catch (Exception ex)
	      {
	        logger.error(String.format("Error in UpdateDatabaseDictionary: {0}", ex));
	        status.setLevel(StatusLevel.ERROR);
	        status.getMessages().getItems().add(String.format("Error updating database dictionary: {0}", ex));
	      }

	      _response.append(status);
	      			return _response;
*/	      
			return null;
	    }
	
	public final DatabaseDictionary getDictionary(String projectName,
			String applicationName) {

		DatabaseDictionary databaseDictionary = new DatabaseDictionary();

		initializeScope(projectName, applicationName);

		if ((new File(_settings.get("DBDictionaryPath").toString())).exists()) {

				try {
					databaseDictionary = 	JaxbUtils.read(DatabaseDictionary.class,_settings.get("DBDictionaryPath"));
				} catch (JAXBException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				} catch (IOException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				} 
			}else
			{
			  databaseDictionary = new DatabaseDictionary();
			  try {
				JaxbUtils.write(databaseDictionary, _settings.get("DBDictionaryPath"), false);
			} catch (JAXBException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
			}
		 
		return databaseDictionary;
		}

	public Response postDictionary(String projectName, String applicationName, DatabaseDictionary databaseDictionary)
	{
		Status status = new Status();
		
		try
		{
			status.setIdentifier(String.format("{0}.{1}", projectName, applicationName));
			initializeScope(projectName, applicationName);
			JaxbUtils.write(databaseDictionary, _settings.get("DBDictionaryPath"),false);
			status.getMessages().getItems().add("Database Dictionary saved successfully");
		}
		catch (Exception ex)
		{
			logger.error("Error in SaveDatabaseDictionary: " + ex);
			status.getMessages().getItems().add("Error in saving database dictionary" + ex.getMessage());
		}

		_response.append(status);
		return _response;
	}

	public DatabaseDictionary getDatabaseSchema(String projectName, String applicationName, String schemaName)
	{
		
		DatabaseDictionary dbDictionary = new DatabaseDictionary();
		try
		{
			initializeScope(projectName, applicationName);

			if ((new File(_settings.get("DBDictionaryPath").toString())).exists()) {

				dbDictionary = JaxbUtils.read(DatabaseDictionary.class,_settings.get("DBDictionaryPath"));
			}
			else
			{
				JaxbUtils.write(dbDictionary, _settings.get("DBDictionaryPath"), false);
				return dbDictionary;
			}
			String connString = dbDictionary.getConnectionString();
			String dbProvider = dbDictionary.getProvider();
			dbProvider = dbProvider.toUpperCase();
			
			String parsedConnStr = parseConnectionString(connString, dbProvider);

			dbDictionary = new DatabaseDictionary();
			Properties properties = new Properties();
			
			String metadataQuery = "";
			dbDictionary.setConnectionString(parsedConnStr);
			dbDictionary.setDataObjects(new ArrayList<DataObject>());//.dataObjects = new System.Collections.Generic.List<DataObject>();

			//properties.setProperty("connection.provider", "NHibernate.Connection.DriverConnectionProvider");
			//properties.setProperty("proxyfactory.factory_class", "NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");
			//properties.setProperty("connection.connection_string", parsedConnStr);

			properties.setProperty("hibernate.connection.provider_class", "org.hibernate.connection.DriverManagerConnectionProvider");
			properties.setProperty("hibernate.connection.driver_class", getConnectionDriver(dbProvider));
			properties.setProperty("hibernate.dialect", getDatabaseDialect(dbProvider));

			

			if (dbProvider.equalsIgnoreCase("MSSQL"))
			{
				metadataQuery =
						"select t1.table_name, t1.column_name, t1.data_type, t2.max_length, t2.is_identity, t2.is_nullable, t5.constraint_type " +
						"from information_schema.columns t1 " +
						"inner join sys.columns t2 on t2.name = t1.column_name " +
						"inner join sys.tables t3 on t3.name = t1.table_name and t3.object_id = t2.object_id " +
						"left join information_schema.key_column_usage t4 on t4.table_name = t1.table_name and t4.column_name = t1.column_name " +
						"left join information_schema.table_constraints t5 on t5.constraint_name = t4.constraint_name " +
						"where t1.data_type not in ('image') " +
						"order by t1.table_name, t5.constraint_type, t1.column_name";// +
				properties.setProperty("connection.driver_class", "NHibernate.Driver.SqlClientDriver");

				if (dbProvider.equalsIgnoreCase("MSSQL2008"))
				{
						dbDictionary.setProvider(Provider.MS_SQL_2008.toString());
						properties.setProperty("dialect", "NHibernate.Dialect.MsSql2008Dialect");
						
				}else if(dbProvider.equalsIgnoreCase("MSSQL2005")){
						
						dbDictionary.setProvider(Provider.MS_SQL_2005.toString());
						properties.setProperty("dialect", "NHibernate.Dialect.MsSql2005Dialect");
						
				}else if(dbProvider.equalsIgnoreCase("MSSQL2000")){
						
						dbDictionary.setProvider(Provider.MS_SQL_2000.toString());
						properties.setProperty("dialect", "NHibernate.Dialect.MsSql2000Dialect");
						
				}else{
						throw new Exception("Database provider not supported.");
				}
			}
			else if (dbProvider.equalsIgnoreCase("ORACLE"))
			{
				metadataQuery = String.format(
					"select t1.object_name, t2.column_name, t2.data_type, t2.data_length, 0 as is_sequence, t2.nullable, t4.constraint_type " +
					"from all_objects t1 " +
					"inner join all_tab_cols t2 on t2.table_name = t1.object_name " +
					"left join all_cons_columns t3 on t3.table_name = t2.table_name and t3.column_name = t2.column_name " +
					"left join all_constraints t4 on t4.constraint_name = t3.constraint_name and (t4.constraint_type = 'P' or t4.constraint_type = 'R') " +
					"where t1.object_type = 'TABLE' and (t1.owner = '{0}') order by t1.object_name, t4.constraint_type, t2.column_name", schemaName);

				properties.setProperty("connection.driver_class", "NHibernate.Driver.OracleClientDriver");

				if (dbProvider.equalsIgnoreCase("ORACLE10G")){
						
						dbDictionary.setProvider(Provider.ORACLE_10_G.toString());
						properties.setProperty("dialect", "NHibernate.Dialect.Oracle10gDialect");
						
				}else if(dbProvider.equalsIgnoreCase("ORACLE9I")){
						
						dbDictionary.setProvider(Provider.ORACLE_9_I.toString());
						properties.setProperty("dialect", "NHibernate.Dialect.Oracle9iDialect");
						
				}else if(dbProvider.equalsIgnoreCase("ORACLE8I")){
						
						dbDictionary.setProvider(Provider.ORACLE_8_I.toString());
						properties.setProperty("dialect", "NHibernate.Dialect.Oracle8iDialect");
						
				}else if(dbProvider.equalsIgnoreCase("ORACLELITE")){
						
						dbDictionary.setProvider(Provider.ORACLE_LITE.toString());
						properties.setProperty("dialect", "NHibernate.Dialect.OracleLiteDialect");
						
				}else{
						throw new Exception("Database provider not supported.");
				}
			}
			else if (dbProvider.equalsIgnoreCase("MYSQL"))
			{
				metadataQuery = "SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE,CHARACTER_MAXIMUM_LENGTH, COLUMN_KEY, IS_NULLABLE " +
												"FROM INFORMATION_SCHEMA.COLUMNS " +
												String.format("WHERE TABLE_SCHEMA = '{0}'", ((connString.split(";")[1]).split("="))[1]);
				
				properties.setProperty("connection.driver_class", "NHibernate.Driver.MySqlDataDriver");
				
				metadataQuery = "SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE,CHARACTER_MAXIMUM_LENGTH, COLUMN_KEY, IS_NULLABLE " + "FROM INFORMATION_SCHEMA.COLUMNS " + String.format("WHERE TABLE_SCHEMA = '%1$s'", connString.split("[;]", -1)[1].split("[=]", -1)[1]);
				properties.setProperty("connection.driver_class", "NHibernate.Driver.MySqlDataDriver");
				  

				if(dbProvider.equalsIgnoreCase("MYSQL3")){
				
						dbDictionary.setProvider(Provider.MY_SQL_3.toString());
						properties.setProperty("dialect", "NHibernate.Dialect.MySQLDialect");
						
				}else if(dbProvider.equalsIgnoreCase("MYSQL4")){
						
						dbDictionary.setProvider(Provider.MY_SQL_4.toString());
						properties.setProperty("dialect", "NHibernate.Dialect.MySQLDialect");
						
				}else if(dbProvider.equalsIgnoreCase("MYSQL5")){
						
						dbDictionary.setProvider(Provider.MY_SQL_5.toString());
						properties.setProperty("dialect", "NHibernate.Dialect.MySQL5Dialect");
				}
			}


			Configuration config = new Configuration();
			config.addProperties(properties);

			SessionFactory sessionFactory = config.buildSessionFactory();
			Session session = sessionFactory.getCurrentSession();
			
			SQLQuery query = session.createSQLQuery(metadataQuery);

			@SuppressWarnings("unchecked")
			List<Object[]> metadataList = (List<Object[]>)query.list();
			
			session.close();

			DataObject table = null;
			
			String prevTableName = "";
			
			for(Object[] metadata : metadataList)
			{
				String tableName = metadata[0].toString();
				String columnName = metadata[1].toString();
				
				String dataType = Utility.sqlTypeToCSharpType(metadata[2].toString());
				int dataLength = (Integer) metadata[3];
				boolean isIdentity = (Boolean) metadata[4];
				String nullable = metadata[5].toString().toUpperCase();
				
				boolean isNullable = (nullable == "Y" || nullable == "TRUE");
				String constraint = metadata[6].toString();

				if (tableName != prevTableName)
				{
					
					table = new DataObject();
					table.setTableName(tableName);
					table.setDataProperties(new ArrayList<DataProperty>());
					table.setKeyProperties(new ArrayList<KeyProperty>());
					table.setDataRelationships(new ArrayList<DataRelationship>());
					table.setObjectName(Utility.nameSafe(tableName));
					dbDictionary.getDataObjects().add(table);
					prevTableName = tableName;
				}

				if (constraint == null ||  constraint == "") // process columns
				{
					DataProperty column = new DataProperty();
					column.setColumnName(columnName);
					column.setDataLength(dataLength);
					column.setIsNullable(isNullable);
					column.setPropertyName(Utility.nameSafe(columnName));
					column.setDataType(DataType.valueOf(dataType));
					table.getDataProperties().add(column);
				}
				else // process keys
				{
					KeyType keyType = KeyType.ASSIGNED;

					if (isIdentity)
					{
						keyType = KeyType.IDENTITY;
					}
					else if (constraint.equalsIgnoreCase("FOREIGN KEY") || constraint.equalsIgnoreCase("R"))
					{
						keyType = KeyType.FOREIGN;
					}

					DataProperty key = new DataProperty();
					key.setColumnName(columnName);
					key.setDataType(DataType.valueOf(dataType));
					key.setDataLength(dataLength);
					key.setIsNullable(isNullable);
					key.setPropertyName(Utility.nameSafe(columnName));
					key.setKeyType(keyType);
					
					table.getDataProperties().add(key);
					//  table.addKeyProperty(key);
					
					//****table.getKeyProperties().add(key);
				}
			}
			return dbDictionary;
		}
		catch (Exception ex)
		{
			logger.error("Error in GetDatabaseSchema: " + ex);
			return dbDictionary;
		}
	}

	public List<RelationshipType> getRelationships() {
		try {
			return new Relationships().getRelationships();
		} catch (Exception ex) {
			logger.error("Error in GetRelationships: " + ex);
			return null;
		}
	}

	public List<Provider> getProviders() {
		try {
			return new Providers().getProviders();
		} catch (Exception ex) {
			logger.error("Error in GetProviders: " + ex);
			return null;
		}
	}

	public List<String> getSchemaObjects(String projectName, String applicationName)
	{
		List<String> tableNames = new ArrayList<String>();
		DatabaseDictionary dbDictionary = new DatabaseDictionary();
		try
		{
			initializeScope(projectName, applicationName);
			if ((new File(_settings.get("DBDictionaryPath").toString())).exists()) 

				dbDictionary = JaxbUtils.read(DatabaseDictionary.class,_settings.get("DBDictionaryPath"));
			else
				return tableNames;

			String connString = dbDictionary.getConnectionString();
			String dbProvider = dbDictionary.getProvider().toString().toUpperCase();
			String schemaName = dbDictionary.getSchemaName();
			String parsedConnStr = parseConnectionString(connString, dbProvider);

			
			//logger.debug("ConnectString: {0} Provider: {1} SchemaName: {2} Parsed: {3}", connString, dbProvider, schemaName, parsedConnStr);
			

			Properties properties = new Properties();

			dbDictionary.setConnectionString(parsedConnStr);
			dbDictionary.setDataObjects(new ArrayList<DataObject>());

			properties.setProperty("hibernate.connection.provider_class", "org.hibernate.connection.DriverManagerConnectionProvider");
			properties.setProperty("hibernate.connection.driver_class", getConnectionDriver(dbProvider));
			properties.setProperty("hibernate.dialect", getDatabaseDialect(dbProvider));

			Configuration config = new Configuration();
			config.addProperties(properties);

			SessionFactory sessionFactory = config.buildSessionFactory();
			Session session = sessionFactory.getCurrentSession();
			String sql = getDatabaseMetaquery(dbProvider, (parsedConnStr.split(";")[1]).split("=")[1], schemaName);

			//logger.DebugFormat("SQL: {0}",sql);

			SQLQuery query = session.createSQLQuery(sql);

			List<String> metadataList = new ArrayList<String>();
			for (String tableName : (List<String>)query.list())
			{
				metadataList.add(tableName);
			}
			session.close();

			tableNames = metadataList;
			return tableNames;
		}
		catch (Exception ex)
		{
			return tableNames;
		}
	}

	public DataObject getSchemaObjectSchema(String projectName, String applicationName, String schemaObjectName)
	{
		List<String> tableNames = new ArrayList<String>();
		DatabaseDictionary dbDictionary = new DatabaseDictionary();
		DataObject dataObject = new DataObject();
		dataObject.setTableName(schemaObjectName);
		dataObject.setDataProperties(new ArrayList<DataProperty>());
		dataObject.setKeyProperties(new ArrayList<KeyProperty>());
		dataObject.setDataRelationships(new ArrayList<DataRelationship>());
		dataObject.setObjectName(Utility.nameSafe(schemaObjectName));

		try
		{
			initializeScope(projectName, applicationName);

			if ((new File(_settings.get("DBDictionaryPath").toString())).exists()) 
				dbDictionary = JaxbUtils.read(DatabaseDictionary.class,_settings.get("DBDictionaryPath"));
			
			String connString = dbDictionary.getConnectionString();
			String dbProvider = dbDictionary.getProvider().toString().toUpperCase();
			String schemaName = dbDictionary.getSchemaName();
			String parsedConnStr = parseConnectionString(connString, dbProvider);

			Properties properties = new Properties();

			dbDictionary.setConnectionString(parsedConnStr);
			dbDictionary.setDataObjects(new ArrayList<DataObject>());

			/*properties.setProperty("connection.provider", "Hibernate.Connection.DriverConnectionProvider");
			properties.setProperty("proxyfactory.factory_class", "NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");
			properties.setProperty("connection.connection_string", parsedConnStr);
			properties.setProperty("connection.driver_class", getConnectionDriver(dbProvider));
			properties.setProperty("dialect", getDatabaseDialect(dbProvider));*/
			
			properties.setProperty("hibernate.connection.provider_class", "org.hibernate.connection.DriverManagerConnectionProvider");
			properties.setProperty("hibernate.connection.driver_class", getConnectionDriver(dbProvider));
			properties.setProperty("hibernate.dialect", getDatabaseDialect(dbProvider));

			Configuration config = new Configuration();
			config.addProperties(properties);

			SessionFactory sessionFactory = config.buildSessionFactory();
			Session session = sessionFactory.getCurrentSession();
			SQLQuery query = session.createSQLQuery(getTableMetaQuery(dbProvider, (parsedConnStr.split(";")[1]).split("=")[1], schemaName, schemaObjectName));
			

			//List<Object[]> metadataList = (List<Object[]>)query.list();			
			
			@SuppressWarnings("unchecked")
			List<Object[]> metadataList = query.list();
			
			session.close();

			for (Object[] metadata : metadataList)
			{
				
				String columnName = metadata[0].toString();
				String dataType = Utility.sqlTypeToCSharpType(metadata[1].toString());
				int dataLength = (Integer) metadata[2];
				boolean isIdentity = (Boolean) metadata[3];
				String nullable = metadata[4].toString().toUpperCase();
				boolean isNullable = (nullable == "Y" || nullable == "TRUE");
				String constraint = metadata[5].toString();


				if (constraint == null || constraint == "" ) // process columns
				{
					
					DataProperty column = new DataProperty();
					column.setColumnName(columnName);
					column.setDataType(DataType.valueOf(dataType));
					column.setDataLength(dataLength);
					column.setIsNullable(isNullable);
					column.setPropertyName(Utility.nameSafe(columnName));
					dataObject.getDataProperties().add(column);
				}
				else
				{
					KeyType keyType = KeyType.ASSIGNED;

					if (isIdentity)
					{
						keyType = KeyType.IDENTITY;
					}

					
					else if (constraint.equalsIgnoreCase("FOREIGN KEY") || constraint.equalsIgnoreCase("R"))
					{
						keyType = KeyType.FOREIGN;
					}

					DataProperty key = new DataProperty();
					key.setColumnName(columnName);
					key.setDataType(DataType.valueOf(dataType));
					key.setDataLength(dataLength);
					key.setIsNullable(isNullable);
					key.setPropertyName(Utility.nameSafe(columnName));
					key.setKeyType(keyType);

					//****dataObject.addKeyProperty(key);
					dataObject.getDataProperties().add(key);
				}
			}
			return dataObject;
		}
		catch (Exception ex)
		{
			return dataObject;
		}
	}

	public VersionInfo getVersion()
	{
		// TODO
		//Version version = this.GetType().Assembly.GetName().Version;
		
		/*return new VersionInfo()
		{
			Major = version.Major,
			Minor = version.Minor,
			Build = version.Build,
			Revision = version.Revision
		};*/
		return new VersionInfo();
	}

	private Session OpenSession() throws HibernateException {
		try {
			Configuration conf = new Configuration();
			conf.addFile("C:\\test-workspace\\Hibernate\\src\\nh-mapping.12345_000.ABC.xml");
			conf.configure("nh-configuration.12345_000.ABC.xml");
			SessionFactory factory = conf.buildSessionFactory();
			Session session = factory.getCurrentSession();
			return session;
		} catch (Exception ex) {
			throw new HibernateException(
					"Error while openning hibernate session " + ex);
		}
	}

	private String getTableMetaQuery(String dbProvider, String databaseName, String schemaName, String objectName)
	{
	  String tableQuery = "";

	  if (dbProvider.toUpperCase().contains("MSSQL"))
	  {
		tableQuery = String.format("SELECT t1.COLUMN_NAME, t1.DATA_TYPE, t2.max_length, t2.is_identity, t2.is_nullable, t5.CONSTRAINT_TYPE " + "FROM INFORMATION_SCHEMA.COLUMNS AS t1 INNER JOIN sys.columns AS t2 ON t2.name = t1.COLUMN_NAME INNER JOIN  sys.schemas AS ts ON " + "ts.name = t1.TABLE_SCHEMA INNER JOIN  sys.tables AS t3 ON t3.schema_id = ts.schema_id AND t3.name = t1.TABLE_NAME AND " + "t3.object_id = t2.object_id LEFT OUTER JOIN  INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS t4 ON t4.TABLE_SCHEMA = t1.TABLE_SCHEMA AND " + "t4.TABLE_NAME = t1.TABLE_NAME AND t4.COLUMN_NAME = t1.COLUMN_NAME LEFT OUTER JOIN  INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS t5 ON " + "t5.CONSTRAINT_NAME = t4.CONSTRAINT_NAME AND t5.CONSTRAINT_SCHEMA = t4.TABLE_SCHEMA WHERE (t1.DATA_TYPE NOT IN ('image')) AND " + "(t1.TABLE_CATALOG = '%1$s') AND (t1.TABLE_SCHEMA = '%2$s') AND (t1.TABLE_NAME = '%3$s')  ORDER BY t1.COLUMN_NAME", databaseName, schemaName, objectName);
	  }
	  else if (dbProvider.toUpperCase().contains("MYSQL"))
	  {
		tableQuery = String.format("select t1.COLUMN_NAME, t1.DATA_TYPE, t1.CHARACTER_MAXIMUM_LENGTH, t1.COLUMN_KEY, t1.IS_NULLABLE, c1.CONSTRAINT_TYPE " + " from INFORMATION_SCHEMA.COLUMNS t1 join KEY_COLUMN_USAGE u1 on u1.TABLE_NAME = t1.TABLE_NAME and u1.TABLE_SCHEMA = t1.TABLE_SCHEMA and " + " t1.COLUMN_NAME = u1.COLUMN_NAME join INFORMATION_SCHEMA.TABLE_CONSTRAINTS c1 on u1.CONSTRAINT_NAME = c1.CONSTRAINT_NAME and u1.TABLE_NAME = c1.TABLE_NAME " + " where t1.TABLE_SCHEMA = '%1$s' and t1.TABLE_NAME = '%2$s' ORDER BY t1.COLUMN_NAME", schemaName, objectName);
	  }
	  else if (dbProvider.toUpperCase().contains("ORACLE"))
	  {

		tableQuery = String.format(" SELECT t2.column_name, t2.data_type, t2.data_length," + " 0 AS is_sequence, t2.nullable, t4.constraint_type" + " FROM all_objects t1 INNER JOIN all_tab_cols t2" + " ON t2.table_name = t1.object_name AND t2.owner = t1.owner" + " LEFT JOIN all_cons_columns t3 ON t3.table_name   = t2.table_name" + " AND t3.column_name = t2.column_name AND t3.owner = t2.owner" + " AND SUBSTR(t3.constraint_name, 0, 3) != 'SYS' LEFT JOIN all_constraints t4" + " ON t4.constraint_name = t3.constraint_name AND t4.owner = t3.owner" + " AND (t4.constraint_type = 'P' OR t4.constraint_type = 'R')" + " WHERE UPPER(t1.owner) = '%1$s' AND UPPER(t1.object_name) = '%2$s' ORDER BY" + " t1.object_name, t4.constraint_type, t2.column_name ORDER BY t2.column_name", schemaName.toUpperCase(), objectName.toUpperCase());
	  }
	  return tableQuery;
	}
	
	private String getDatabaseMetaquery(String dbProvider, String database, String schemaName)
	{
	  String metaQuery = "";

	  if (dbProvider.toUpperCase().contains("MSSQL"))
	  {
		metaQuery = String.format("select table_name from INFORMATION_SCHEMA.TABLES WHERE table_schema = '%1$s' order by table_name", schemaName);
	  }
	  else if (dbProvider.toUpperCase().contains("MYSQL"))
	  {
		metaQuery = String.format("select table_name from INFORMATION_SCHEMA.TABLES where table_schema = '%1$s' order by table_name;", schemaName);
	  }
	  else if (dbProvider.toUpperCase().contains("ORACLE"))
	  {
		metaQuery = String.format("select object_name from all_objects where object_type in ('TABLE', 'VIEW', 'SYNONYM') and UPPER(owner) = '%1$s' order by object_name", schemaName.toUpperCase());
	  }
	  else
	  {
		throw new RuntimeException(String.format("Database provider %1$s not supported.", dbProvider));
	  }

	  return metaQuery;
	}
	
	private String getDatabaseDialect(String dbProvider) {
		if (dbProvider.toUpperCase().equals("MSSQL2008")) {
			return "org.hibernate.dialect.SQLServer2008Dialect";

		} else if (dbProvider.toUpperCase().equals("MSSQL2005")) {
			return "org.hibernate.dialect.SQLServer2005Dialect";

		} else if (dbProvider.toUpperCase().equals("MSSQL2000")) {
			return "org.hibernate.dialect.SQLServerDialect";

		} else if (dbProvider.toUpperCase().equals("ORACLE10G")) {
			return "org.hibernate.dialect.Oracle10gDialect";

		} else if (dbProvider.toUpperCase().equals("ORACLE9I")) {
			return "org.hibernate.dialect.Oracle9Dialect";

		} else if (dbProvider.toUpperCase().equals("ORACLE8I")) {
			return "org.hibernate.dialect.Oracle8iDialect";

		} else if (dbProvider.toUpperCase().equals("ORACLELITE")) {
			return "org.hibernate.dialect.OracleDialect";

		} else if (dbProvider.toUpperCase().equals("MYSQL3")
				|| dbProvider.toUpperCase().equals("MYSQL4")) {

			return "org.hibernate.dialect.MySQLDialect";
		} else if (dbProvider.toUpperCase().equals("MYSQL5")) {
			return "org.hibernate.dialect.MySQL5Dialect";
		} else {
			throw new RuntimeException(String.format(
					"Database provider %1$s not supported.", dbProvider));
		}
	}

	private String getConnectionDriver(String dbProvider) {
		if (dbProvider.toUpperCase().contains("MSSQL")) {
			return "com.microsoft.sqlserver.jdbc.SQLServerDriver";
		} else if (dbProvider.toUpperCase().contains("MYSQL")) {
			return "com.mysql.jdbc.Driver";
		} else if (dbProvider.toUpperCase().contains("ORACLE")) {
			return "oracle.jdbc.driver.OracleDriver";
		} else {
			throw new RuntimeException(String.format(
					"Database provider %1$s is not supported", dbProvider));
		}
	}

	private void initializeScope(String projectName, String applicationName) {
		try {
			if (!_isScopeInitialized)
			{
			  String scope = String.format("%1$s.%2$s", projectName, applicationName);
			  _settings.put("ProjectName",projectName);
			  _settings.put("ApplicationName",applicationName);
			  _settings.put("Scope",scope);
			  _settings.put("DBDictionaryPath",String.format("%1$sDatabaseDictionary.%2$s.xml", _settings.get("XmlPath"), scope));
			  _isScopeInitialized = true;
			}
		} catch (RuntimeException ex) {
			logger.error(String.format("Error initializing application: %1$s",
					ex));
			throw new RuntimeException(String.format(
					"Error initializing application: %1$s)", ex));
		}
	}

	private boolean validateDatabaseDictionary(DatabaseDictionary dbDictionary) {
		Session session = null;

		try {
			// Validate connection string
			Configuration config = new Configuration();
			Properties properties = new Properties();
			
			/*
			properties.setProperty("connection.provider_class","NHibernate.Connection.DriverConnectionProvider");
			properties.setProperty("connection.connection_string",dbDictionary.getConnectionString());
			properties.setProperty("proxyfactory.factory_class","NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");
			properties.setProperty("default_schema",dbDictionary.getSchemaName());
			properties.setProperty("dialect", "NHibernate.Dialect."+ dbDictionary.getProvider() + "Dialect");
			 */

	        
			properties.setProperty("hibernate.connection.provider_class", "org.hibernate.connection.DriverManagerConnectionProvider");
			properties.setProperty("hibernate.default_schema",dbDictionary.getSchemaName());
			properties.setProperty("hibernate.dialect", getDatabaseDialect(dbDictionary.getProvider()));
			


			if (dbDictionary.getProvider().toString().toUpperCase().contains("MSSQL")) {
				properties.setProperty("hibernate.connection.driver_class","com.microsoft.sqlserver.jdbc.SQLServerDriver");

			} else if (dbDictionary.getProvider().toString().toUpperCase().contains("ORACLE")) {
				properties.setProperty("hibernate.connection.driver_class","oracle.jdbc.driver.OracleDriver");
			} else {
				throw new RuntimeException("Database not supported.");
			}

			config.addProperties(properties);
			SessionFactory factory = config.buildSessionFactory();
			session = factory.openSession();
		} catch (RuntimeException ex) {
			
			throw new RuntimeException("Invalid connection string: "+ ex.getMessage());
			
		} finally {
			if (session != null) {
				session.close();
			}
		}
		// Validate table key
		for (DataObject dataObject : dbDictionary.getDataObjects()) {
			
			if (dataObject.getKeyProperties() == null || dataObject.getKeyProperties().size() == 0) {
				
				throw new RuntimeException(String.format("Table \"%1$s\" has no key.",dataObject.getTableName()));
			}
		}

		return true;
	}

	private static String parseConnectionString(String connStr,
			String dbProvider) {
		try {
			String parsedConnStr = "";
			String[] connStrKeyValuePairs = connStr.split(";");
			
			
			for (String connStrKeyValuePair : connStrKeyValuePairs) {
				
				String[] connStrKeyValuePairTemp = connStrKeyValuePair.split(
						"[=]", -1);
				String connStrKey = connStrKeyValuePairTemp[0].trim();
				String connStrValue = connStrKeyValuePairTemp[1].trim();

				if (connStrKey.toUpperCase().equals("DATA SOURCE")
						|| connStrKey.toUpperCase().equals("USER ID")
						|| connStrKey.toUpperCase().equals("PASSWORD")) {
					parsedConnStr += connStrKey + "=" + connStrValue + ";";
				}

				if (dbProvider.toUpperCase().contains("MSSQL")) {
					if (connStrKey.toUpperCase().equals("INITIAL CATALOG")
							|| connStrKey.toUpperCase().equals("INTEGRATED SECURITY")) {
						parsedConnStr += connStrKey + "=" + connStrValue + ";";
					}
				} else if (dbProvider.toUpperCase().contains("MYSQL")) {
					parsedConnStr += connStrKey + "=" + connStrValue + ";";
				}
			}

			return parsedConnStr;
		} catch (RuntimeException ex) {
			throw ex;
		}
	}

	private boolean isAuthorized()
	{
	  if (!_keyRing.isEmpty() && _keyRing.get("Name") != null)
	  {
		String userName = _keyRing.get("Name").toString();
		userName = userName.substring(userName.indexOf('\\') + 1).toLowerCase();
		 if (userName == "anonymous")
	        {
	          return true;
	        }
		 
		AuthroziedUsers authUsers = null;

		try
		{
		  authUsers = JaxbUtils.read(AuthroziedUsers.class, _authorizationPath);
		}
		catch (RuntimeException ex)
		{
		  //logger.Warn("Error loading authorization file: " + ex);
		} catch (JAXBException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}

		if (authUsers != null)
		{
		  for (String authUser : authUsers.getUserIds())
		  {
			if (authUser.toLowerCase().equals(userName))
			{
			  return true;
			}
		  }
		}
	  }

	  return false;
	}


}
