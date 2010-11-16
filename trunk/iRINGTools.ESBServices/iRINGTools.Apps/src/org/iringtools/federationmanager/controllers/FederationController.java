package org.iringtools.federationmanager.controllers;

import org.iringtools.refdata.federation.Federation;
import org.iringtools.utility.WebClient;

import com.opensymphony.xwork2.Action;

public class FederationController {
	
	private Federation federation;
	public FederationController(){

		federation = getFederationXML();
	}
	
	public String execute() {
        return Action.SUCCESS;
	}
 
	public Federation getFederationXML()
	  {
		String URI="http://localhost:8080/iringtools/services/refData/federation";
		Federation f=null;
		try{
			f = (new WebClient()).get(Federation.class,URI);
		}catch(Exception e)
		{
			System.out.println("#### :"+e);
		}
		return f;
	  }
	
	public Federation getFederation() {
		return federation;
	}

	public void setFederation(Federation federation) {
		this.federation = federation;
	}
}
