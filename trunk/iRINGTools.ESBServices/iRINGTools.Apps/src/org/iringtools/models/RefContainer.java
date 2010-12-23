package org.iringtools.models;

import org.iringtools.refdata.response.Entity;
import org.iringtools.utility.HttpClient;

import com.opensymphony.xwork2.ActionContext;

public class RefContainer {

	private static String URI;
	private Entity entity;

	public RefContainer() {
		try {
			URI = ActionContext.getContext().getApplication()
					.get("RefServiceUri").toString();
		} catch (Exception e) {
			System.out.println("Exception in RefServiceUri :" + e);
		}
	}

	public void populate(String id) {
		try {
			HttpClient httpClient = new HttpClient(URI);
			Entity value = httpClient.get(Entity.class,
					"/classes/" + id + "/label");
			setEnti(value);
		} catch (Exception e) {
			System.out.println("Exception :" + e);
		}
	}

	public void setEnti(Entity enti) {
		this.entity = enti;
	}

	public String getValue()
	{
		return entity.getLabel();
	
	}
}
