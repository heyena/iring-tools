package org.iringtools.federationmanager.models;

import org.iringtools.common.response.Response;
import org.iringtools.refdata.federation.Federation;
import org.iringtools.refdata.federation.IDGenerators;
import org.iringtools.refdata.federation.Namespaces;
import org.iringtools.refdata.federation.Repositories;
import org.iringtools.utility.WebClient;
import org.iringtools.utility.WebClientException;

public class FederationModel {

	private Federation federation;

	public FederationModel() {
		//TODO: Perform local initialization.
	}
	
	public void populate() {
		//TODO: Get this from a configuration xml
		String URI = "http://localhost:8080/services/refdata";
		federation = null;

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
		//TODO: Get this from private member which is populated in constructor.
		String URI = "http://localhost:8080/services/refdata";
		federation = null;

		try {
			WebClient webclient = new WebClient(URI);
			Response response = webclient.post(Response.class, "/federation", federation);
			
			//TODO: Check response for errors/warnings
			
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
}
