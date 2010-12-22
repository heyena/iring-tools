package org.iringtools.models;

import java.util.List;
import java.util.ArrayList;

import org.iringtools.grid.Grid;
import org.iringtools.grid.Filter;
import org.iringtools.grid.Column;
import org.iringtools.grid.Header;
import org.iringtools.refdata.response.Entity;
import java.util.HashMap;
import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.dxfr.dti.DataTransferIndexList;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dto.ClassObject;
import org.iringtools.dxfr.dto.DataTransferObject;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.dto.RoleObject;
import org.iringtools.dxfr.dto.TemplateObject;

import org.iringtools.grid.Rows;

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
