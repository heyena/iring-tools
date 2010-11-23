package org.iringtools.federationmanager.models;


import org.iringtools.common.response.Response;
import org.iringtools.refdata.federation.Federation;
import org.iringtools.refdata.federation.IDGenerators;
import org.iringtools.refdata.federation.Namespaces;
import org.iringtools.refdata.federation.Repositories;
import org.iringtools.utility.WebClient;
import org.iringtools.utility.WebClientException;

import com.opensymphony.xwork2.ActionContext;



public class FederationModel {

	private Federation federation=null;
	private String URI;

	public FederationModel() {
		try{
			URI = ActionContext.getContext().getApplication().get("RefDataServiceUri").toString();
		}catch(Exception e){
			System.out.println("Exception in RefDataServiceUri :"+e);
		}
		federation=null;
	}
	
	public void populate() {

		try {
			WebClient webclient = new WebClient(URI);
			federation = webclient.get(Federation.class, "/federation");
		} catch (WebClientException wce) {
			System.out.println("WebClientException :" + wce);
		} catch (Exception e) {
			System.out.println("Exception :" + e);
		}
	}
	
	public void save() {
		
		try {
			WebClient webclient = new WebClient(URI);
			Response response = webclient.post(Response.class, "/federation", federation);
			
			//TODO: Check response for errors/warnings
			if(response==null){
				throw new WebClientException("Respose Null");
			}
			
		} catch (WebClientException wce) {
			System.out.println("WebClientException :" + wce);
		} catch (Exception e) {
			System.out.println("Exception :" + e);
		}
	}

	public IDGenerators getIdGenerators() {
		return federation.getIdGenerators();
	}

	public Namespaces getNamespaces() {
		return federation.getNamespaces();
	}

	public Repositories getRepositories() {
		return federation.getRepositories();
	}
	
	public Federation getFederation() {
		return federation;
	}
}
