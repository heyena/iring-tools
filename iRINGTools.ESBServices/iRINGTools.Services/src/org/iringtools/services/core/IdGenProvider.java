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

import org.ids_adi.config.Config;
import org.ids_adi.rdf.connection.IDSADIRDFConnection;
import org.ids_adi.rdf.registry.IDSADIRDFRegistry.RegistryInfo;
import org.ids_adi.rdf.registry.IDSADIRDFRegistry.RegistryState;
import org.ids_adi.xml.IDSADIXMLSupport;

import com.hp.hpl.jena.datatypes.RDFDatatype;
import com.hp.hpl.jena.datatypes.TypeMapper;
import com.hp.hpl.jena.rdf.model.Literal;
import com.hp.hpl.jena.rdf.model.Model;
import com.hp.hpl.jena.rdf.model.ModelMaker;
import com.hp.hpl.jena.rdf.model.Property;
import com.hp.hpl.jena.rdf.model.RDFNode;
import com.hp.hpl.jena.rdf.model.Resource;
import com.hp.hpl.jena.rdf.model.Statement;
import com.hp.hpl.jena.rdf.model.StmtIterator;

public class IdGenProvider{

    private final Object lock = new Object();
    private final long floor;
    private final long ceiling;
    private final int maxseq;
    private final int maxcount;
    private final DateFormat xmlSchemaDateFormatter;
    private final TypeMapper typeMapper;
    private final RDFDatatype xmlSchemaDateType;
    private final RDFDatatype xmlSchemaStringType;
    private final String randomAlgorithm;
    private final ModelMaker modelMaker;
    private String registryExternalName;
    private Random useGetRandom;


    public IdGenProvider() {
	this.modelMaker = null;

	final Model model = this.modelMaker.getModel("registry");
	final Resource resource = model.createResource(REGISTRY_NS);

	this.randomAlgorithm = resource.getRequiredProperty(
	    model.createProperty(REGISTRY_NS + "random")).getString();
	this.floor = resource.getRequiredProperty(
	    model.createProperty(REGISTRY_NS + "floor")).getLong();
	this.ceiling = resource.getRequiredProperty(
	    model.createProperty(REGISTRY_NS + "ceiling")).getLong();
	this.maxseq = resource.getRequiredProperty(
	    model.createProperty(REGISTRY_NS + "max-sequence")).getInt();
	this.maxcount = resource.getRequiredProperty(
	    model.createProperty(REGISTRY_NS + "max-count")).getInt();
	this.registryExternalName = resource.getRequiredProperty(
	    model.createProperty(REGISTRY_NS + "name")).getString();

	this.typeMapper = TypeMapper.getInstance();
	this.xmlSchemaDateType = typeMapper.
	    getTypeByName("http://www.w3.org/2001/XMLSchema#dateTime");
	this.xmlSchemaStringType = typeMapper.
	    getTypeByName("http://www.w3.org/2001/XMLSchema#string");
	this.xmlSchemaDateFormatter =
	    IDSADIXMLSupport.newDateTimeFormatInstance();
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
		result = state.acquireIDIn(base);

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
	    	final RegistryInfo info = state.checkAccessTo(uri);

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
	    	final RegistryInfo info = state.checkAccessTo(uri);

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
		list = state.listBaseURIs();
	    }
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
		if (!state.fillRegistryInfo(info)) {
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
    	private final Model model;
	private final Property registeredOnProperty;
	private final Property registeredByProperty;
	private final Property grantRightsToProperty;
	private final Property boundProperty;
	private final Property rdfsCommentProperty;
	private final Resource baseURIType;
	private final Property isAProperty;
	private final Property fixatedOnProperty;
	private final Property fixatedByProperty;
	private final String userName;

	private RegistryState(String userName) {
	    ns = "http://ns.ids-adi.org/registry/schema";
	    model = modelMaker.openModel(registryExternalName, false);
	    registeredOnProperty = createProperty("registeredOn");
	    registeredByProperty = createProperty("registeredBy");
	    fixatedOnProperty = createProperty("fixatedOn");
	    fixatedByProperty = createProperty("fixatedBy");
	    grantRightsToProperty = createProperty("grantRightsTo");
	    boundProperty = createProperty("boundTo");
	    baseURIType = createResource("baseURI");
	    isAProperty = model.createProperty(
		"http://www.w3.org/1999/02/22-rdf-syntax-ns#type");
	    rdfsCommentProperty = model.createProperty(
		"http://www.w3.org/2000/01/rdf-schema#comment");

	    this.userName = userName;

	    properties.add(rdfsCommentProperty);
	}

	protected Property createProperty(String name) {
	    final Property property = model.createProperty(ns + "#" + name);

	    properties.add(property);

	    return (property);
	}

	protected Resource createResource(String name) {
	    final Resource resource = model.createResource(ns + "#" + name);

	    return (resource);
	}

	public void close() {
	    model.close();
	}

	public boolean existsID(String id) {
	    return (model.contains(model.createResource(id),
		registeredOnProperty));
	}

	public String getAnyBoundID(String boundID, String prefix) {
	    final StmtIterator it = model.listStatements((Resource)null,
		boundProperty, model.createResource(boundID));
	    String id = null;

	    try {
		while ((id == null) && it.hasNext()) {
		    final String uri = it.nextStatement().getSubject().getURI();

		    if (uri == null) {
			/* ignore */
		    }
		    else if (!uri.startsWith(prefix)) {
			/* ignore */
		    }
		    else {
			id = uri;
		    }
		}
	    }
	    finally {
	    	it.close();
	    }

	    return (id);
	}

	public Literal createTimeStampLiteral() {
	    return (model.createTypedLiteral(
		xmlSchemaDateFormatter.format(new Date()),
		xmlSchemaDateType));
	}

	public String acquireIDIn(String base) {
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
	    while (existsID(result));

	    acquireID(result);

	    return (result);
	}

	public void acquireID(String id) {
	    final Resource idResource = model.createResource(id);

	    model.add(idResource, registeredOnProperty,
	    	createTimeStampLiteral());

	    if (userName != null) {
		setOwner(idResource);
	    }
	}

	public void setComment(String id, String comment) {
	    final Resource idResource = model.createResource(id);

	    model.add(idResource, rdfsCommentProperty,
		model.createTypedLiteral(comment,
		xmlSchemaStringType));
	}

	public void bindID(String id, String target) {
	    model.add(model.createResource(id), boundProperty,
		model.createResource(target));
	}

	public void unbindID(String id, String target) {
	    model.remove(model.createResource(id), boundProperty,
		model.createResource(target));
	}

	public void releaseID(String id) {
	    final Resource idResource = model.createResource(id);

	    for (Iterator it = properties.iterator(); it.hasNext(); ) {
		removeStatements(idResource, (Property)it.next());
	    }
	}

	public void fixateID(String id) {
	    final Resource idResource = model.createResource(id);

	    model.add(idResource, fixatedOnProperty,
	    	createTimeStampLiteral());
	    model.add(idResource, fixatedByProperty,
	    	model.createResource(userName));
	}

	public void addBaseURI(String baseURI) {
	    final Resource idResource = model.createResource(baseURI);

	    model.add(idResource, isAProperty, baseURIType);

	    if (userName != null) {
	    	setOwner(idResource);
	    }
	}

	private void setOwner(Resource idResource) {
	    removeStatements(idResource, registeredByProperty);
	    removeStatements(idResource, grantRightsToProperty);

	    final Resource userResource = model.createResource(userName);

	    model.add(idResource, registeredByProperty, userResource);
	    model.add(idResource, grantRightsToProperty, userResource);
	}

	public boolean fillRegistryInfo(RegistryInfo info) {
	    final Resource idResource = model.createResource(info.id);
	    final boolean exists = model.contains(idResource,
	    	registeredOnProperty);

	    if (exists) {
		final StmtIterator it = model.listStatements(idResource,
		    (Property)null, (RDFNode)null);

		try {
		    while (it.hasNext()) {
			final Statement statement = it.nextStatement();
			final Property property = statement.getPredicate();
			final RDFNode node = statement.getObject();

			if (property.equals(boundProperty)) {
			    if (node.isResource()) {
				info.bindings.add(((Resource)node).getURI());
			    }
			}
			else if (property.equals(registeredOnProperty)) {
			    if (node.isLiteral()) {
				info.registeredOn = ((Literal)node).getString();
			    }
			}
			else if (property.equals(registeredByProperty)) {
			    if (node.isResource()) {
				info.registeredBy = ((Resource)node).getURI();
			    }
			}
			else if (property.equals(fixatedOnProperty)) {
			    if (node.isLiteral()) {
				info.fixatedOn = ((Literal)node).getString();
			    }
			}
			else if (property.equals(fixatedByProperty)) {
			    if (node.isResource()) {
				info.fixatedBy = ((Resource)node).getURI();
			    }
			}
			else if (property.equals(grantRightsToProperty)) {
			    if (node.isResource()) {
				info.grants.add(((Resource)node).getURI());
			    }
			}
			else if (property.equals(rdfsCommentProperty)) {
			    if (node.isLiteral()) {
				info.comments.add(((Literal)node).getString());
			    }
			}
		    }
		}
		finally {
		    it.close();
		}
	    }

	    return (exists);
	}

	public List listBaseURIs() {
	    final StmtIterator it = model.listStatements((Resource)null,
		isAProperty, baseURIType);
	    final List list = new ArrayList();

	    while (it.hasNext()) {
		final Resource resource = it.nextStatement().getSubject();

		list.add(resource.getURI());
	    }

	    return (list);
	}

	protected void removeStatements(Resource resource, Property property) {
	    final StmtIterator it = model.listStatements(resource, property,
		    (RDFNode)null);

	    try {
		while (it.hasNext()) {
		    it.next();
		    it.remove();
		}
	    }
	    finally {
	    	it.close();
	    }
	}

	public void checkAccessTo(RegistryInfo info) {
	    if (info.fixatedOn == null) {
		if (!info.grants.contains(userName)) {
		    throw new AccessControlException("uri " + info.id +
			" is not accessible to " + userName);
		}
	    }
	}

	public RegistryInfo checkAccessTo(String uri) {
	    final RegistryInfo info = new RegistryInfo(uri);

	    if (!fillRegistryInfo(info)) {
		throw new IllegalArgumentException("uri " + uri +
		    " is not registered");
	    }

	    checkAccessTo(info);

	    return (info);
	}
    }

    public static boolean indicatesProhibited(Throwable t) {
    	final boolean intentional;

    	if (t instanceof AccessControlException) { // @todo fix
	    intentional = true;
	}
    	else if (t instanceof IllegalArgumentException) { // @todo fix
	    intentional = true;
	}
    	else if (t instanceof IllegalStateException) { // @todo fix
	    intentional = true;
	}
	else {
	    intentional = false;
	}

	return (intentional);
    }

    public static final String REGISTRY_NS =
	"http://ns.ids-adi.org/registry/schema#";

}
