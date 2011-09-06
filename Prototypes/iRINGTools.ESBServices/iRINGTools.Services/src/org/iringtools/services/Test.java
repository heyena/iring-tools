package org.iringtools.services;

import java.util.ArrayList;
import java.util.List;

import org.iringtools.directory.Directory;
import org.iringtools.directory.Endpoint;
import org.iringtools.directory.Exchange;
import org.iringtools.directory.Folder;
import org.iringtools.utility.JaxbUtils;

public class Test {
	public static void main (String [] args) throws Exception {
		Directory d = new Directory();
		List<Folder> fl = new ArrayList<Folder>();
		d.setFolders(fl);		
		
		Folder f = new Folder();
		f.setName("12345_000");
		f.setType("scope");
		fl.add(f);
		
		Folder sf = new Folder();
		sf.setName("Application Data");
		sf.setType("folder");
		List<Object> sfll = new ArrayList<Object>();
		f.setAnies(sfll);
		sfll.add((Object)sf);
		
		Folder ef = new Folder();
		ef.setName("Data Exchanges");
		ef.setType("folder");		
		sfll.add((Object)ef);
		
		Folder ff = new Folder();
		ff.setName("ABC");
		ff.setType("application");		
		List<Object> sfl = new ArrayList<Object>();
		sf.setAnies(sfl);
		sfl.add((Object)ff);
		
		Folder ff1 = new Folder();
		ff1.setName("DEF");
		ff1.setType("application");				
		sfl.add((Object)ff1);
		
		
		List<Endpoint> el = new ArrayList<Endpoint>();
		ff.setEndpoints(el);
		Endpoint e = new Endpoint();		
		el.add(e);		
		
		e.setName("LINES");
		e.setBaseUri("http://localhost:54321/dxfr/12345_000/ABC");
		e.setDescription("ABC Lines List");
	
		List<Endpoint> el1 = new ArrayList<Endpoint>();
		ff1.setEndpoints(el1);
		Endpoint e1 = new Endpoint();		
		el1.add(e1);		
		
		e1.setName("LINES");
		e1.setBaseUri("http://localhost:54321/dxfr/12345_000/DEF");
		e1.setDescription("DEF Lines List");
		
		
		
		Folder cf = new Folder();
		cf.setName("PipingNetworkSystem");
		cf.setType("commodity");
		List<Object> cfl = new ArrayList<Object>();
		ef.setAnies(cfl);
		cfl.add((Object)cf);		
		
		List<Exchange> x4l = new ArrayList<Exchange>();
		cf.setExchanges(x4l);
		Exchange x4 = new Exchange();
		x4l.add(x4);
		x4.setName("ABC.LINES->DEF.LINES");		
		x4.setId("1");
		
		Exchange x = new Exchange();
		x4l.add(x);
		x.setName("DEF.LINES->ABC.LINES");
		x.setId("2");
		
		System.out.println(JaxbUtils.toXml(d, true));
	}
}
