package org.iringtools.adapter;

public class AdapterSettings extends ServiceSettings
{
	public AdapterSettings()
	{
	  super();
	  this.put("InterfaceService", "http://localhost/services/facade/query");
	  this.put("ReferenceDataServiceUri", "http://localhost/services/refdata");
	  this.put("DefaultProjectionFormat", "xml");
	  this.put("EndpointTimeout", "30000");
	  this.put("dotNetRDFServer", ".\\SQLEXPRESS");
	  this.put("dotNetRDFCatalog", "InterfaceDb");
	  this.put("dotNetRDFUser", "dotNetRDF");
	  this.put("dotNetRDFPassword", "dotNetRDF");
	  this.put("TrimData", "False");
	  this.put("DumpSettings", "False");
	  this.put("ExecutingAssemblyName", "App_Code");

	  if (OperationContext.Current != null)
	  {
		String baseAddress = OperationContext.Current.Host.BaseAddresses[0].toString();

		if (!baseAddress.endsWith("/"))
		{
			baseAddress = baseAddress + "/";
		}

		this.put("GraphBaseUri", baseAddress);
	  }
	  else
	  {
		this.put("GraphBaseUri", "http://localhost:54321/data");
		//this.put("GraphBaseUri", @"http://yourcompany.com/");
	  }
	}
}
