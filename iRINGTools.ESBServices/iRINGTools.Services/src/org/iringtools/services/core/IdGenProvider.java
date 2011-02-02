package org.iringtools.services.core;

import java.io.PrintWriter;
import java.net.MalformedURLException;
import java.net.URL;
import java.security.AccessControlException;
import java.security.NoSuchAlgorithmException;
import java.security.SecureRandom;
import java.text.DateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.Iterator;
import java.util.List;
import java.util.Random;

//import org.ids_adi.config.Config;
//import org.ids_adi.rdf.connection.IDSADIRDFConnection;
//import org.ids_adi.rdf.registry.IDSADIRDFRegistry.RegistryInfo;
//import org.ids_adi.rdf.registry.IDSADIRDFRegistry.RegistryState;
//import org.ids_adi.xml.IDSADIXMLSupport;
//
//import com.hp.hpl.jena.datatypes.RDFDatatype;
//import com.hp.hpl.jena.datatypes.TypeMapper;
//import com.hp.hpl.jena.rdf.model.Literal;
//import com.hp.hpl.jena.rdf.model.Model;
//import com.hp.hpl.jena.rdf.model.ModelMaker;
//import com.hp.hpl.jena.rdf.model.Property;
//import com.hp.hpl.jena.rdf.model.RDFNode;
//import com.hp.hpl.jena.rdf.model.Resource;
//import com.hp.hpl.jena.rdf.model.Statement;
//import com.hp.hpl.jena.rdf.model.StmtIterator;

public class IdGenProvider{

    private final Object lock = new Object();
    private final long floor;
    private final long ceiling;
    private final int maxseq;
    private final int maxcount;
    private final DateFormat xmlSchemaDateFormatter;
//    private final TypeMapper typeMapper;
//    private final RDFDatatype xmlSchemaDateType;
//    private final RDFDatatype xmlSchemaStringType;
    private final String randomAlgorithm;
//    private final ModelMaker modelMaker;
    private String registryExternalName;
    private Random useGetRandom;


    public IdGenProvider() {
	//this.modelMaker = null;

	//final Model model = this.modelMaker.getModel("registry");
	//final Resource resource = model.createResource(REGISTRY_NS);

	this.randomAlgorithm = null;
	//resource.getRequiredProperty(
	  //  model.createProperty(REGISTRY_NS + "random")).getString();
	this.floor = 0;
	//resource.getRequiredProperty(
	   // model.createProperty(REGISTRY_NS + "floor")).getLong();
	this.ceiling = 0;
	//resource.getRequiredProperty(
	  //  model.createProperty(REGISTRY_NS + "ceiling")).getLong();
	this.maxseq = 0;
	//resource.getRequiredProperty(
	   // model.createProperty(REGISTRY_NS + "max-sequence")).getInt();
	this.maxcount = 0;//resource.getRequiredProperty(
			
	   // model.createProperty(REGISTRY_NS + "max-count")).getInt();
	this.registryExternalName = null;
		//resource.getRequiredProperty(
	   // model.createProperty(REGISTRY_NS + "name")).getString();

	//this.typeMapper = null;
		//TypeMapper.getInstance();
	//this.xmlSchemaDateType = typeMapper.
	   // getTypeByName("http://www.w3.org/2001/XMLSchema#dateTime");
	//this.xmlSchemaStringType = typeMapper.
	   // getTypeByName("http://www.w3.org/2001/XMLSchema#string");
	this.xmlSchemaDateFormatter = null;
	   // IDSADIXMLSupport.newDateTimeFormatInstance();
    }
    

    public void info(PrintWriter printer) {
    	printer.println("Registry");
    	printer.println("  randomAlgorithm=" + randomAlgorithm);
    	printer.println("  floor=" + floor);
    	printer.println("  ceiling=" + ceiling + " " +
		(int)Math.log10(ceiling + 1) + " digits");
    	printer.println("  maxseq=" + maxseq);
    	printer.println("  maxcount=" + maxcount);
    	printer.println("  registryExternalName=" + registryExternalName);
    }

    protected Random getRandom() {
	synchronized (lock) {
	    if (this.useGetRandom == null) {
		try {
		    this.useGetRandom = SecureRandom.
		    	getInstance(randomAlgorithm);
		}
		catch (NoSuchAlgorithmException e) {
		    throw new Error("cannot find " + randomAlgorithm,
			e); // should never happen except in busted VMs.
		}
	    }
	}

	return (useGetRandom);
    }

    public boolean checkComplexEnough(String s) {
	final char array[] = s.toCharArray();
    	boolean ok = true;

    	for (int i = 0; ok && (i < array.length); i++) {
	    final char c = array[i];

	    int count = 0;
	    int seq = 0;

	    for (int j = 0; ok && (j < array.length); j++) {
	    	if (array[j] == c) {
		    count++;
		    seq++;

		    if (seq > maxseq) {
		    	ok = false;
		    }
		    else if (count > maxcount) {
		    	ok = false;
		    }
		}
		else {
		    seq = 0;
		}
	    }
	}

	return (ok);
    }

    public boolean isValidID(String uri) {
    	boolean result = false;

	try {
	    final URL url = new URL(uri);
	    final String fragment = url.getRef();

	    if (fragment == null) {
	    	// invalid - need fragment
	    }
	    else if (!fragment.matches("^[R][0-9]+$")) {
	    	// invalid - need matching fragment
	    }
	    else {
	    	result = true;
	    }
	}
	catch (MalformedURLException e) {
	    // ignore
	}

    	return (result);
    }

    public boolean isValidBase(String uri) {
    	boolean result = false;

	try {
	    final URL url = new URL(uri);
	    final String fragment = url.getRef();

	    if (fragment == null) {
	    	// invalid - need fragment
	    }
	    else if (!fragment.equals("")) {
	    	// invalid - need empty fragment
	    }
	    else {
	    	result = true;
	    }
	}
	catch (MalformedURLException e) {
	    // ignore
	}

    	return (result);
    }

    public boolean isValidTarget(String uri) {
    	boolean result = false;

	try {
	    final URL url = new URL(uri);

	    result = true;
	}
	catch (MalformedURLException e) {
	    // ignore
	}

    	return (result);
    }

    public long generateNumber() {
    	final long i;

	synchronized (lock) {
	    i = getRandom().nextLong();
	}

	return (floor + ((i & 0x7fffffffffffffffL) % (ceiling + 1 - floor)));
    }

    public String generateFragment() {
    	String s;

	do {
	    s = "" + generateNumber();
	}
	while (!checkComplexEnough(s));

    	return ("R" + s);
    }

    public String acquireIDIn(String base, String comment, String userName) {
    	String result;

	synchronized (lock) {
	    final RegistryState state = newRegistryState(userName);

	    try {
		result = null;

		state.setComment(result, comment);
	    }
	    finally {
		state.close();
	    }
	}

	return (result);
    }

    public void bindID(String uri, String target, String userName) {
    	synchronized (lock) {
	    final RegistryState state = newRegistryState(userName);

	    try {
	    	state.checkAccessTo(uri);

		state.bindID(uri, target);
	    }
	    finally {
		state.close();
	    }
	}
    }

    public void unbindID(String uri, String target, String userName) {
    	synchronized (lock) {
	    final RegistryState state = newRegistryState(userName);

	    try {
	    	state.checkAccessTo(uri);

		state.unbindID(uri, target);
	    }
	    finally {
		state.close();
	    }
	}
    }

    public void releaseID(String uri, String userName) {
    	synchronized (lock) {
	    final RegistryState state = newRegistryState(userName);

	    try {
	    	final RegistryInfo info = null;

		if (info.fixatedOn != null) {
		    throw new IllegalStateException("uri " + uri +
		    	" has been fixated and therefore cannot be" +
			" released");
		}

		state.releaseID(uri);
	    }
	    finally {
		state.close();
	    }
	}
    }

    public void fixateID(String uri, String userName) {
    	synchronized (lock) {
	    final RegistryState state = newRegistryState(userName);

	    try {
	    	final RegistryInfo info = null;

		if (info.fixatedOn != null) {
		    throw new IllegalStateException("uri " + uri +
			" has already been fixated by " +
			info.fixatedBy + " on " + info.fixatedOn);
		}

		state.fixateID(uri);
	    }
	    finally {
		state.close();
	    }
	}
    }

    public List listBaseURIs(String userName) {
    	List list;

    	synchronized (lock) {
	    final RegistryState state = newRegistryState(userName);

	    try {
		list = null;	    }
	    finally {
		state.close();
	    }
	}

	return (list);
    }

    public void addBaseURI(String baseURI, String comment, String userName) {
    	synchronized (lock) {
	    final RegistryState state = newRegistryState(userName);

	    try {
		state.addBaseURI(baseURI);
		state.setComment(baseURI, comment);
	    }
	    finally {
		state.close();
	    }
	}
    }

    public void setComment(String uri, String comment, String userName) {
    	synchronized (lock) {
	    final RegistryState state = newRegistryState(userName);

	    try {
	    	state.checkAccessTo(uri);

		state.setComment(uri, comment);
	    }
	    finally {
		state.close();
	    }
	}
    }

    public RegistryState newRegistryState(String userName) {
	return (new RegistryState(userName));
    }

    public RegistryInfo getRegistryInfo(String id, String userName) {
    	RegistryInfo info = new RegistryInfo(id);

	synchronized (lock) {
	    final RegistryState state = newRegistryState(userName);

	    try {
		if (true) {
		    info = null;
		}
	    }
	    finally {
		state.close();
	    }
	}

	return (info);
    }

    public final class RegistryInfo {
    	public final List bindings = new ArrayList();
    	public final List comments = new ArrayList();
    	public final List grants = new ArrayList();
	public final String id;
	public String registeredBy;
	public String registeredOn;
	public String fixatedBy;
	public String fixatedOn;

    	private RegistryInfo(String id) {
	    this.id = id;
	}

	public List getBindingList() {
	    return (bindings);
	}
    }

    public final class RegistryState {
	private final List properties = new ArrayList();
    	private final String ns;
//    	private final Model model;
//	private final Property registeredOnProperty;
//	private final Property registeredByProperty;
//	private final Property grantRightsToProperty;
//	private final Property boundProperty;
//	private final Property rdfsCommentProperty;
//	private final Resource baseURIType;
//	private final Property isAProperty;
//	private final Property fixatedOnProperty;
//	private final Property fixatedByProperty;
	private final String userName;

	private RegistryState(String userName) {
	    ns = "http://ns.ids-adi.org/registry/schema";
//	    model = modelMaker.openModel(registryExternalName, false);
//	    registeredOnProperty = createProperty("registeredOn");
//	    registeredByProperty = createProperty("registeredBy");
//	    fixatedOnProperty = createProperty("fixatedOn");
//	    fixatedByProperty = createProperty("fixatedBy");
//	    grantRightsToProperty = createProperty("grantRightsTo");
//	    boundProperty = createProperty("boundTo");
//	    baseURIType = createResource("baseURI");
//	    isAProperty = model.createProperty(
		//"http://www.w3.org/1999/02/22-rdf-syntax-ns#type");
	  //  rdfsCommentProperty = model.createProperty(
		//"http://www.w3.org/2000/01/rdf-schema#comment");

	    this.userName = userName;

	    properties.add(null);
	}

	protected void createProperty(String name) {
	   // final Property property = model.createProperty(ns + "#" + name);

	   // properties.add(property);

	   // return (property);
	}

	protected void createResource(String name) {
	   // final int resource = model.createResource(ns + "#" + name);

	  //  return (resource);
	}

	public void close() {
	    //model.close();
	}

	public void existsID(String id) {
	   // return (model.contains(model.createResource(id),
		//registeredOnProperty));
	}

	public String getAnyBoundID(String boundID, String prefix) {
	   // final StmtIterator it = model.listStatements((Resource)null,
		//boundProperty, model.createResource(boundID));
	    String id = "abc";

	    try {
		while ((id == null) ) {
		    

		    
		}
	    }
	    finally {
	    	
	    }

	    return (id);
	}

	public void createTimeStampLiteral() {
	   
	}

	public void acquireIDIn(String base) {
	    final int maxTries = 1024;
	    String result;
	    int count = 0;

	    do {
		result = base + generateFragment();
		count++;

		if (count > maxTries) {
		    throw new IllegalStateException(
			"too many tries to allocate id");
		}
	    }
	    while (true);

	    
	    
	}

	public void acquireID(String id) {
	    

	    if (userName != null) {
		
	    }
	}

	public void setComment(String id, String comment) {
	  
	}

	public void bindID(String id, String target) {
	   
	}

	public void unbindID(String id, String target) {
	  
	}

	public void releaseID(String id) {
	  
	}

	public void fixateID(String id) {
	   
	}

	public void addBaseURI(String baseURI) {
	  
	}

	private void setOwner() {
	  	}

	public void fillRegistryInfo(RegistryInfo info) {
	   
	}

	public void listBaseURIs() {
	  
	}

	protected void removeStatements() {
	    
	}

	public void checkAccessTo(RegistryInfo info) {
	    if (info.fixatedOn == null) {
		if (!info.grants.contains(userName)) {
		    throw new AccessControlException("uri " + info.id +
			" is not accessible to " + userName);
		}
	    }
	}

	public void checkAccessTo(String uri) {
	   
    }

    public void indicatesProhibited() {
    }
}
}
