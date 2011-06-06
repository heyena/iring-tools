package org.iringtools.adapter.services.core;

import java.util.ArrayList;
//import java.util.Hashtable;
import java.util.List;

import org.iringtools.common.Version;
import org.iringtools.library.Application;
import org.iringtools.library.Applications;
import org.iringtools.library.Scope;


public class AdapterProvider {

//	private Hashtable<String, String> _settings;
/*	public AdapterProvider(Hashtable<String, String> settings) {
		try
	    {
	      _settings = settings;
	      _repositories = getRepositories();
	      _queries = getQueries();
	      _nsmap = new NamespaceMapper();
	    }
	    catch (Exception e)
	    {
	      // TODO Auto-generated catch block
	      e.printStackTrace();
	    }

	}
	*/		
	public Scope getScope() {
	
		Application app1=new Application();
		app1.setDescription("ABC is an application");
		app1.setName("ABC");
		
		Application app2=new Application();
		app2.setDescription("DEF is another application");
		app2.setName("DEF");

		List<Application> applications = new ArrayList<Application>();
		applications.add(app1);
		applications.add(app2);
		
		Applications appList = new Applications();
		appList.setItems(applications);
		Scope scope = new Scope();
		scope.setName("12345_000");
		scope.setDescription("scope name is 12345_000");
		scope.setApplications(appList);
		
		return scope;
		
	}
	public Version getVersion() {
		Version version = new Version();
		version.setBuild("1");
		version.setMajor("1.1");
		return version;
	}

}
