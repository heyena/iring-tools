package org.iringtools.models;

import java.util.List;

import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.dxfr.dti.DataTransferIndexList;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.request.ExchangeRequest;
import org.iringtools.dxfr.response.ExchangeResponse;
import org.iringtools.utility.HttpClient;

public class ExchResponseContainer {
	private String responseUrl = "";
	private ExchangeRequest exchangeRequest = null;	
	private ExchangeResponse exchangeResponse = null;

	public void populateResponse(String URI) {
		try {			
			HttpClient httpClient = new HttpClient(URI);
			ExchangeResponse exResponse = httpClient.post(
					ExchangeResponse.class, responseUrl, exchangeRequest);
			setExchangeResponse(exResponse);
		} catch (Exception e) {
			System.out.println("Exception :" + e);
		}		
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
	
	public void setExchangeResponse(ExchangeResponse val) {
		exchangeResponse = val;
	}
	public ExchangeResponse getExchangeResponse() {
		return exchangeResponse;
	}
}