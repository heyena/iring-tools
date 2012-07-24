package org.iringtools.adapter.datalayer.proj_12345_000.ABC;

import org.iringtools.adapter.datalayer.HibernateDataLayer;
import org.iringtools.library.DatabaseDictionary;

public class Test {

	/**
	 * @param args
	 * @throws ClassNotFoundException
	 */
	public static void main(String[] args) {
		
		HibernateDataLayer dataLayer = new HibernateDataLayer();
		
		DatabaseDictionary db = new DatabaseDictionary();
		
		//System.out.println(dataLayer.parseConnectionString("Data Source=.\\SQLEXPRESS;Initial Catalog=ABC;User ID=abc;Password=abc;","MsSql2008"));
		
		/*List<String> identifiers = dataLayer.getIdentifiers("LINES", null);
		System.out.println(identifiers);
		*/
		/*List<IDataObject> idataObject = dataLayer.get("LINES", identifiers);
		List<IDataObject> idataObject1 =	dataLayer.get("LINES", null,10, 27);*/
		/*List ll = new ArrayList();
		ll.add("Aswinis");
		
		List l = new ArrayList();
		l.add("s");
		l.add("s");
		
		l.add(ll);
		*/
		/*
		@SuppressWarnings("unchecked")
		List<Object[]> metadataList = (List<Object[]>)l;
		
		//System.out.println(metadataList);
		
		for( Object metaData : metadataList){
			System.out.println(metaData);
		}
		HibernateDataLayer dataLayer = new HibernateDataLayer();
		*/
		//System.out.println(dataLayer.getProviders());
		 
	}
}
