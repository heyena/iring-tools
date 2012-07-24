package org.iringtools.utility;

import java.net.URI;
import java.util.Hashtable;
import java.util.Map;
import java.util.Map.Entry;

public class NamespaceMapper implements NsMapper {

	public static final String RDF = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
	public static final String RDFS = "http://www.w3.org/2000/01/rdf-schema#";
	public static final String XSD = "http://www.w3.org/2001/XMLSchema#";
	public static final String OWL = "http://www.w3.org/2002/07/owl#";
	public static final String EG = "http://example.org/data#";
	public static final String RDL = "http://rdl.rdlfacade.org/data#";
	public static final String TPL = "http://tpl.rdlfacade.org/data#";
	public static final String DM = "http://dm.rdlfacade.org/data#";
	public static final String P8DM = "http://standards.tc184-sc4.org/iso/15926/-8/data-model#";
	public static final String OWL2XML = "http://www.w3.org/2006/12/owl2-xml#";
	public static final String P8 = "http://standards.tc184-sc4.org/iso/15926/-8/template-model#";
	public static final String TEMPLATES = "http://standards.tc184-sc4.org/iso/15926/-8/templates#";
	public static final String JORD = "http://posccaesar.org/endpoint/sparql#";
		public Map<String, URI> _uris;

	protected Map<Integer, String> _prefixes;

	public NamespaceMapper() throws Exception {
		this(false);
	}

	public NamespaceMapper(Boolean empty) throws Exception {
		this._uris = new Hashtable<String, URI>();
		this._prefixes = new Hashtable<Integer, String>();

		if (!empty) {
			// Add Standard Namespaces
			this.addNamespace("rdf", new URI(RDF));
			this.addNamespace("rdfs", new URI(RDFS));
			this.addNamespace("xsd", new URI(XSD));
			this.addNamespace("jordrdl", new URI(JORD));
			this.addNamespace("owl", new URI(OWL));
			this.addNamespace("owl2xml", new URI(OWL2XML));
			this.addNamespace("rdl", new URI(RDL));
			this.addNamespace("tpl", new URI(TPL));
			this.addNamespace("eg", new URI(EG));
			this.addNamespace("dm", new URI(DM));
			this.addNamespace("p8dm", new URI(P8DM));
			this.addNamespace("p8", new URI(P8));
			this.addNamespace("templates", new URI(TEMPLATES));
		}
	}

	protected NamespaceMapper(NsMapper nsmapper) throws Exception {
		this(true);
		this.importMap(nsmapper);
	}

	public String getPrefix(URI uri) throws Exception {
		int hash;

		hash = getEnhancedHashCode(uri);

		if (this._prefixes.containsKey(hash)) {
			return this._prefixes.get(hash);
		} else {
			throw new Exception("The Prefix for the given URI '"
					+ uri.toString()
					+ "' is not known by the in-scope NamespaceMapper");
		}
	}

	public URI getNamespaceUri(String prefix) {
		if (this._uris.containsKey(prefix)) {
			return this._uris.get(prefix);
		} else {
			throw new RuntimeException(
					"The Namespace URI for the given Prefix '" + prefix
							+ "' is not known by the in-scope NamespaceMapper");
		}
	}

	public void addNamespace(String prefix, URI uri) throws Exception {
		int hash = getEnhancedHashCode(uri);
		if (!this._uris.containsKey(prefix)) {
			// Add a New Prefix
			this._uris.put(prefix, uri);

			if (!this._prefixes.containsKey(hash)) {
				// Add a New Uri
				this._prefixes.put(hash, prefix);
			} else {
				this._prefixes.get(prefix);
			}
		} else {
			if (!this._uris.get(prefix).toString().equals(uri.toString())) {
				// Update the existing Prefix
				this._uris.put(prefix, uri);
				this._prefixes.put(hash, prefix);
			}
		}
	}

	public final void removeNamespace(String prefix) throws Exception {
		// Check the Namespace is defined
		if (this._uris.containsKey(prefix)) {
			URI u = this._uris.get(prefix);

			// Remove the Prefix to Uri Mapping
			this._uris.remove(prefix);

			// Remove the corresponding Uri to Prefix Mapping
			int hash = getEnhancedHashCode(u);
			if (this._prefixes.containsKey(hash)) {
				this._prefixes.remove(hash);
			}
		}
	}

	public static int getEnhancedHashCode(URI u) throws Exception {
		if (u == null) {
			throw new Exception(
					"Cannot calculate an Enhanced Hash Code for a null URI");
		}
		return u.toString().hashCode();
	}

	public boolean hasNamespace(String prefix) {
		return this._uris.containsKey(prefix);
	}

	public void clear() {
		this._prefixes.clear();
		this._uris.clear();
	}

	public final Iterable<String> getPrefixes() {
		return this._uris.keySet();
	}

	public final String reduceToQName(String uri)
			throws Exception {
		String qname = "";
		String tmp = "";
		for (URI u : this._uris.values()) {
			String baseuri = u.toString();

			// Does the Uri start with the Base Uri
			if (uri.startsWith(baseuri)) {
				// Remove the Base Uri from the front of the Uri
				tmp = uri.substring(baseuri.length());
				// Add the Prefix back onto the front plus the colon to give a
				// QName
				qname = this._prefixes.get(getEnhancedHashCode(u))
						+ ":" + tmp;
				if (qname.contains(":")) {
					return qname;
				}
				if (qname.contains("/")
						|| qname.contains("#")) {
					continue;
				}
				return qname;
			}
		}
		
		return "";
	}

	public void importMap(NsMapper nsmap) {
		String tempPrefix = "ns0";
		int tempPrefixID = 0;
		try {

			for (String prefix : nsmap.getPrefixes()) {
				if (!this._uris.containsKey(prefix)) {
					this.addNamespace(prefix, nsmap.getNamespaceUri(prefix));
				} else {
					if (!this._uris.get(prefix).equals(
							nsmap.getNamespaceUri(prefix))) {
						while (this._uris.containsKey(tempPrefix)) {
							tempPrefixID++;
							tempPrefix = "ns" + tempPrefixID;
						}
						this.addNamespace(tempPrefix,
								nsmap.getNamespaceUri(prefix));
					}
				}
			}
		} catch (Exception ex) {
		}
	}

	public final void dispose() {
		this._prefixes.clear();
		this._uris.clear();
	}

	public final String resolveQName(String qname, NamespaceMapper nsmap,
			URI baseUri) {
		String output;

		if (qname.startsWith(":")) {
			// QName in Default Namespace
			if (nsmap.hasNamespace("")) {
				// Default Namespace Defined
				output = nsmap.getNamespaceUri("").toString()
						+ qname.substring(1);
			} else {

				if (baseUri != null) {
					output = baseUri.toString();
					if (output.endsWith("#")) {
						output += qname.substring(1);
					} else {
						output += "#" + qname.substring(1);
					}
				} else {
					throw new RuntimeException(
							"Cannot resolve a QName in the Default Namespace when there is no in-scope Base URI and no Default Namespace defined");
				}
			}
		} else {
			// QName in some other Namespace
			String[] parts = qname.split("[:]", -1);
			if (parts.length == 1) {
				output = nsmap.getNamespaceUri("").toString() + parts[0];
			} else {
				output = nsmap.getNamespaceUri(parts[0]).toString() + parts[1];
			}
		}
		return output;
	}

	public final String prefixString() {
		StringBuilder prefixes = new StringBuilder();
		for (Entry<Integer, String> pref : this._prefixes.entrySet()) {
			prefixes.append("PREFIX " + pref.getValue() + ": <"
					+ this.getNamespaceUri(pref.getValue()) + ">/n");
		}
		return prefixes.toString();
	}
}
