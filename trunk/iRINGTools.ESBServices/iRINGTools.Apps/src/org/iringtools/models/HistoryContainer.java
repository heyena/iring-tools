package org.iringtools.models;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

import org.iringtools.common.response.Status;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.history.History;
import org.iringtools.ui.widgets.grid.Column;
import org.iringtools.ui.widgets.grid.GridAndRows;
import org.iringtools.ui.widgets.grid.Header;
import org.iringtools.utility.HttpClient;

public class HistoryContainer {

	private History history = null;
	private String historyUrl = "";
	private HashMap<String, String> data = null;
	private List<HashMap<String, String>> dataList = null;
	private List<String> headerList;
	private List<Header> gridHeaderList; // list of headers
	private List<Column> columnList;
	private Column column;
	private Header header;
	double width;		

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

	public void initialList() {
		dataList = new ArrayList<HashMap<String, String>>();
		gridHeaderList = new ArrayList<Header>(); // list of headers
		columnList = new ArrayList<Column>();
		headerList = new ArrayList<String>();
	}
	
	public void setGridAndRows(GridAndRows gridAndRows) {
		List<Status> statusList;
		for (ExchangeResponse exchangeResponse : history.getResponses()) {
			statusList = exchangeResponse.getStatusList()
					.getItems();
			data = new HashMap<String, String>();
			data.put("Level", exchangeResponse.getLevel().name());
			data.put("StartTimeStamp", exchangeResponse.getStartTimeStamp().toString());
			data.put("RowCount", String.valueOf(statusList.size()));
			data.put("SenderUri", exchangeResponse.getSenderUri());
			data.put("ReceiverUri", exchangeResponse.getReceiverUri());
			dataList.add(data);			
		}		
		gridAndRows.setRowData(dataList);		
		gridAndRows.setSuccess("true");
	}

	public void setDetailGridAndRows(GridAndRows gridAndRows, String historyId) {
		List<String> messageList;
		String msg = "";
		List<Status> statusList;

		ExchangeResponse exchangeResponse;
		exchangeResponse = history.getResponses().get(
				Integer.parseInt(historyId));
		statusList = exchangeResponse.getStatusList().getItems();

		if (statusList.size() > 0) {
			for (Status status : exchangeResponse.getStatusList().getItems()) {
				data = new HashMap<String, String>();
				data.put("Identifier", status.getIdentifier());
				messageList = status.getMessages().getItems();
				for (String message : messageList) {
					msg = msg + message;
				}
				data.put("Message", msg);				
				dataList.add(data);
				msg = "";
			}
			
		}
		gridAndRows.setRowData(dataList);
		gridAndRows.setSuccess("true");

	}
	
	
	public void setHeaderList() {		
		headerList.add("Level");
		headerList.add("StartTimeStamp");
		headerList.add("RowCount");
		headerList.add("SenderUri");
		headerList.add("ReceiverUri");
	}
	
	public void setDetailHeaderList() {		
		headerList.add("Identifier");
		headerList.add("Message");		
	}
	
	public void setColumnWidth(boolean ifDetail) {
		if (ifDetail)
			if (width < 8)
				width = 600;
			else
				width = 110;			
		else
			if (width < 9)
				width = 110;
			else
				width = 600;			
	}

	public void setGridList(GridAndRows gridAndRows, boolean ifDetail) {

		
		for (String head : headerList) {
			column = new Column();
			column.setDataIndex(head);
			column.setHeader(head);
			column.setId(head);
			column.setSortable("true");
			width = head.length();
			setColumnWidth(ifDetail);
			column.setWidth(width);
			columnList.add(column);
			header = new Header();
			header.setName(head);
			gridHeaderList.add(header);			
		}
		
		gridAndRows.setColumnData(columnList);
		gridAndRows.setHeaderLists(gridHeaderList);		
		
	}
}