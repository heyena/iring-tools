package org.iringtools.models;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

import org.iringtools.common.response.Status;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.history.History;
import org.iringtools.ui.widgets.grid.Rows;
import org.iringtools.utility.HttpClient;

public class HistoryContainer {

	private History history = null;
	private String historyUrl = "";
	private HashMap<String, String> data = null;
	private List<HashMap<String, String>> dataList = null;

	public void populateHistory(String URI) {
		try {
			HttpClient httpClient = new HttpClient(URI);
			History history = httpClient.get(History.class, historyUrl);
			setHistory(history);
		} catch (Exception e) {
			System.out.println("Exception :" + e);
		}
	}

	public void setHistoryUrl(String url) {
		historyUrl = url;
	}

	public void setHistory(History value) {
		history = value;
	}
	
	public History getHistory() {
		return history;
	}

	public void initialDataList() {
		dataList = new ArrayList<HashMap<String, String>>();
	}
	
	public void setRows(Rows rows) {
		String msg = "";
		int total = 0;
		
		initialDataList();
		List<ExchangeResponse> exchangeResponseList = history.getResponses();

		for (ExchangeResponse exchangeResponse : exchangeResponseList) {
			List<Status> statusList = exchangeResponse.getStatusList()
					.getItems();
			data = new HashMap<String, String>();
			for (Status status : statusList) {				
				data.put("Identifier", status.getIdentifier());
				List<String> messageList = status.getMessages().getItems();
				
				for (String message : messageList) {
					msg = msg + message;
				}
				data.put("Status", msg);
				msg = "";
			}
			data.put("Start Time", exchangeResponse.getStartTimeStamp()
					.toString());
			data.put("End Time", exchangeResponse.getEndTimeStamp().toString());
			dataList.add(data);
			total++;
		}
		
		rows.setData(dataList);
		rows.setTotal(total);
		rows.setSuccess("true");

	}
}