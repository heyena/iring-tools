package org.iringtools.library;

import java.util.List;

import org.apache.axiom.om.OMElement;
import org.iringtools.common.response.Response;
import org.iringtools.data.filter.DataFilter;

public interface IDataLayer {

	List<IDataObject> create(String objectType, List<String> identifiers);

	long getCount(String objectType, DataFilter filter);

	List<String> getIdentifiers(String objectType, DataFilter filter);

	List<IDataObject> get(String objectType, List<String> identifiers);

	List<IDataObject> get(String objectType, DataFilter filter, int pageSize,
			int startIndex);

	Response post(List<IDataObject> dataObjects);

	Response delete(String objectType, List<String> identifiers);

	Response delete(String objectType, DataFilter filter);

	DataDictionary getDictionary();

	List<IDataObject> getRelatedObjects(IDataObject dataObject,
			String relatedObjectType);
	
	Response configure(OMElement configuration);
	  OMElement getConfiguration();

	List<IDataObject> search(String objectType, String query, int pageSize, int startIndex);
	
	long getSearchCount(String objectType, String query);
	
	Response refreshAll();
	
	Response refresh(String objectType);

}
