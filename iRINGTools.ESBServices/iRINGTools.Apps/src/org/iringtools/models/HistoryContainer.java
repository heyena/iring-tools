package org.iringtools.models;

import org.iringtools.history.History;
import org.iringtools.utility.HttpClient;

public class HistoryContainer {
	
	private History history = null;	
	private String historyUrl;

	public void populateHistory(String URI) {
		try {			
			HttpClient httpClient = new HttpClient(URI);
			History history = httpClient.get(
					History.class, historyUrl);
			setHistory(history);
		} catch (Exception e) {
			System.out.println("Exception :" + e);
		}		
	}
	
	
	
	public void setHistoryUrl (String url) {
		historyUrl = url;
	}

	public void setHistory(History val)
	{
		history = val;
	}
	
	
	public History getHistory() {
		return history;
	}
}