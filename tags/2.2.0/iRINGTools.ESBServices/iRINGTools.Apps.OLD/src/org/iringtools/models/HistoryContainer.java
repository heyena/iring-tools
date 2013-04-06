package org.iringtools.models;

import java.util.ArrayList;
import java.util.List;

import org.iringtools.common.response.Status;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.history.History;
import org.iringtools.ui.widgets.grid.Column;


import org.iringtools.ui.widgets.grid.Header;
import org.iringtools.ui.widgets.grid.GridDefinition;
import org.iringtools.utility.HttpClient;

public class HistoryContainer {

	private History history = null;
	private String historyUrl = "";
	private ArrayList<String> data = null;	
	private List<ArrayList<String>> dataStringList = null;
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

	public void initialList(GridDefinition gridDefinition) {
		dataStringList = gridDefinition.getRowData();		
		gridHeaderList = new ArrayList<Header>(); // list of headers
		columnList = new ArrayList<Column>();
		headerList = new ArrayList<String>();
	}
	
	public void setGridAndRows(GridDefinition gridDefinition) {
		List<Status> statusList;
		if (history != null) {
			for (ExchangeResponse exchangeResponse : history.getResponses()) {
				statusList = exchangeResponse.getStatusList().getItems();
				data = new ArrayList<String>();
				data.add(exchangeResponse.getLevel().name());
				data.add(exchangeResponse.getStartTimeStamp().toString());
				data.add(String.valueOf(statusList.size()));
				data.add(exchangeResponse.getSenderUri());
				data.add(exchangeResponse.getReceiverUri());
				dataStringList.add(data);
			}
			gridDefinition.setSuccess("true");
		} else
		
			gridDefinition.setSuccess("false");
	}

	public void setDetailGridAndRows(GridDefinition gridDefinition,
			String historyId) {
		List<String> messageList;
		String msg = "";
		List<Status> statusList;

		ExchangeResponse exchangeResponse;
		exchangeResponse = history.getResponses().get(
				Integer.parseInt(historyId));

		if (exchangeResponse != null) {
			statusList = exchangeResponse.getStatusList().getItems();

			if (statusList.size() > 0) {
				for (Status status : exchangeResponse.getStatusList()
						.getItems()) {

					data = new ArrayList<String>();
					data.add(status.getIdentifier());
					messageList = status.getMessages().getItems();
					for (String message : messageList) {
						msg = msg + message;
					}
					data.add(msg);

					dataStringList.add(data);
					msg = "";
				}

			}
		}

		gridDefinition.setSuccess("true");

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
				width = 300;
			else
				width = 80;			
		else {
			if (dataStringList.size() == 0) {
				if (width < 20)
					width = 110;
				else
					width = width + 130;
			} else {
			if (width < 9)
				width = 110;
			else
				width = 170;	
			}
		}
	}

	public void setGridList(GridDefinition gridDefinition, boolean ifDetail) {

		
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
		
		gridDefinition.setColumnData(columnList);
		gridDefinition.setHeaderLists(gridHeaderList);		
		
	}
}