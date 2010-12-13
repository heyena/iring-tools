package org.iringtools.services.core;

import java.io.FileNotFoundException;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Hashtable;
import java.util.List;

import javax.xml.bind.JAXBException;

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

import org.iringtools.utility.IOUtil;
import org.iringtools.utility.JaxbUtil;

public class RefDataProvider
{
  private Hashtable<String, String> settings;

  public RefDataProvider(Hashtable<String, String> settings)
  {
    this.settings = settings;
  }

  public Queries getQueries() throws JAXBException, IOException, FileNotFoundException
  {
    String path = settings.get("baseDirectory") + "/WEB-INF/data/Queries.xml";
    return JaxbUtil.read(Queries.class, path);   
  }
  
  public Federation getFederation() throws JAXBException, IOException 
  {
    String path = settings.get("baseDirectory") + "/WEB-INF/data/federation.xml";
    return JaxbUtil.read(Federation.class, path);    
  }
  
  public Response saveFederation(Federation federation) 
  {
	  Response response = new Response();
	  try
	  {
		  String path = settings.get("baseDirectory") + "/WEB-INF/data/federation.xml";
		  JaxbUtil.write(federation, path, true);
		  response.setLevel(Level.SUCCESS);
	  }
	  catch(Exception ex)
	  {
		response.setLevel(Level.ERROR);		  
	  }
	  return response;
  }
  
  public Response saveNamespace(Namespace namespace) 
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
		  for(Namespace ns : federation.getNamespaces().getItems())
		  {
			  if (ns.getAlias().equalsIgnoreCase(namespace.getAlias()))
			  {
				  index = federation.getNamespaces().getItems().indexOf(ns);
				  namespaceExist = true;
				  break;
			  }			  
		  }
		  if (namespaceExist)
			  federation.getNamespaces().getItems().remove(index);
			  
		  federation.getNamespaces().getItems().add(namespace);
		  String path = settings.get("baseDirectory") + "/WEB-INF/data/federation.xml";
		  JaxbUtil.write(federation, path, true);
		 
		  msgs.add("Namespace saved.");		  		  
		  response.setLevel(Level.SUCCESS);
	  }
	  catch(Exception ex)
	  {
		msgs.add("Error while saving namespace.");
		response.setLevel(Level.ERROR);		  
	  }
	  
	  messages.setItems(msgs);
	  status.setMessages(messages);
	  statuses.add(status);
	  sl.setItems(statuses);
	  response.setStatusList(sl);
	  return response;
  }

  public Response saveIdGenerator(IDGenerator idgenerator) 
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
		  for(IDGenerator idg : federation.getIdGenerators().getItems())
		  {
			  if (idg.getName().equalsIgnoreCase(idgenerator.getName()))
			  {
				  index = federation.getIdGenerators().getItems().indexOf(idg);
				  idgenExist = true;
				  break;
			  }			  
		  }
		  if (idgenExist)
			  federation.getIdGenerators().getItems().remove(index);
			  
		  federation.getIdGenerators().getItems().add(idgenerator);
		  String path = settings.get("baseDirectory") + "/WEB-INF/data/federation.xml";
		  JaxbUtil.write(federation, path, true);
		 
		  msgs.add("ID Generator saved.");		  		  
		  response.setLevel(Level.SUCCESS);
	  }
	  catch(Exception ex)
	  {
		msgs.add("Error while saving ID Generator.");
		response.setLevel(Level.ERROR);		  
	  }
	  
	  messages.setItems(msgs);
	  status.setMessages(messages);
	  statuses.add(status);
	  sl.setItems(statuses);
	  response.setStatusList(sl);
	  return response;
  }
  
  public Response saveRepository(Repository repository) 
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
		  for(Repository repo : federation.getRepositories().getItems())
		  {
			  if (repo.getName().equalsIgnoreCase(repository.getName()))
			  {
				  index = federation.getRepositories().getItems().indexOf(repo);
				  repositoryExist = true;
				  break;
			  }			  
		  }
		  if (repositoryExist)
			  federation.getRepositories().getItems().remove(index);
			  
		  federation.getRepositories().getItems().add(repository);
		  String path = settings.get("baseDirectory") + "/WEB-INF/data/federation.xml";
		  JaxbUtil.write(federation, path, true);
		 
		  msgs.add("Repository saved.");		  		  
		  response.setLevel(Level.SUCCESS);
	  }
	  catch(Exception ex)
	  {
		msgs.add("Error while saving Repository.");
		response.setLevel(Level.ERROR);		  
	  }
	  
	  messages.setItems(msgs);
	  status.setMessages(messages);
	  statuses.add(status);
	  sl.setItems(statuses);
	  response.setStatusList(sl);
	  return response;
  }
  
  public String ReadSPARQL(String queryName) throws Exception
  {
      try
      {
          String path = settings.get("baseDirectory") + "/WEB-INF/data/Sparqls/";

          String query = IOUtil.readString(path + queryName);

          return query;
      }
      catch (Exception ex)
      {
          throw ex;
      }
  }
}
