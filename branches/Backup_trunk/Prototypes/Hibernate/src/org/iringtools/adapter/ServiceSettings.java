package org.iringtools.adapter;

import java.util.HashMap;



public class ServiceSettings extends HashMap<String,String>
{
	public ServiceSettings()
	{
	  this.put("BaseDirectoryPath", AppDomain.CurrentDomain.BaseDirectory);
	  this.put("XmlPath", ".\\XML\\");
	  this.put("DataPath", ".\\App_Data\\");
	  this.put("ProxyCredentialToken", "");
	  this.put("ProxyHost", "");
	  this.put("ProxyPort", "");
	  this.put("IgnoreSslErrors", "True");
	  this.put("PrimaryClassificationStyle", "Type");
	  this.put("SecondaryClassificationStyle", "Template");
	  this.put("ClassificationTemplateFile", ".\\XML\\ClassificationTemplate.xml");

	  if (OperationContext.Current != null)
	  {
		String baseAddress = OperationContext.Current.Host.BaseAddresses[0].toString();

		if (!baseAddress.endsWith("/"))
		{
		  baseAddress = baseAddress + "/";
		}

		this.put("BaseAddress", baseAddress);
	  }
	  else
	  {
		this.put("BaseAddress", "http://www.example.com/");
	  }
	}
}