package org.iringtools.models;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;




import org.iringtools.common.response.Status;

import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.dxfr.dti.DataTransferIndexList;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.request.ExchangeRequest;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.ui.widgets.grid.Rows;
import org.iringtools.utility.HttpClient;

public class ExchResponseContainer {
	private String responseUrl = "";
	private ExchangeRequest exchangeRequest = null;	
	private ExchangeResponse exchangeResponse = null;	
	private HashMap<String, String> data = null;
	private List<HashMap<String, String>> dataList = null;
	

	public ExchResponseContainer () {
		
	}
	public void populateResponse(String URI) {
		try {			
			HttpClient httpClient = new HttpClient(URI);
			ExchangeResponse value = httpClient.post(
					ExchangeResponse.class, responseUrl, exchangeRequest);
			setExchangeResponse(value);
		} catch (Exception e) {
			System.out.println("Exception :" + e);
		}		
	}
	
	public void initialDataList() {
		dataList = new ArrayList<HashMap<String, String>>();
	}
	
	public void setResponseUrl (String url) {
		responseUrl = url;
	}

	public void setExchangeRequest(List<DataTransferIndex> dtiList, String hasReviewed)
	{
		boolean hasReview = false;
		DataTransferIndices dti = new DataTransferIndices();
		DataTransferIndexList list = new DataTransferIndexList();
		dti.setDataTransferIndexList(list);
		list.setItems(dtiList);
		exchangeRequest = new ExchangeRequest();
		exchangeRequest.setDataTransferIndices(dti);
		if (hasReviewed.toUpperCase().equals("TRUE"))
			hasReview = true;
		
		exchangeRequest.setReviewed(hasReview);
		
	}
	
	public void setExchangeResponse(ExchangeResponse value) {
		exchangeResponse = value;
	}	
	
	public void setRows(Rows rows) {
		String msg = "";
		int total = 0;
		
		initialDataList();
		List<Status> statusList = exchangeResponse.getStatusList().getItems();
		for (Status status : statusList) {
			data = new HashMap<String,String>();
			data.put("Identifier", status.getIdentifier());
			List<String> messageList = status.getMessages().getItems();
			
			for (String message : messageList) {
				msg = msg + message;
			}
			data.put("Message", msg);
			dataList.add(data);
			total++;
			msg = "";
		}		
		rows.setData(dataList);
		rows.setTotal(total);
		rows.setSuccess("true");
	}
	
}