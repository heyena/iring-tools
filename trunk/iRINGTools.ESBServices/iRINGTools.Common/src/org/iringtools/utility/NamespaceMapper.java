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
	protected Map<String, URI> _uris;

	protected Map<Integer, String> _prefixes;

	public NamespaceMapper() throws Exception {
		this(false);
	}

	public NamespaceMapper(Boolean empty) throws Exception {
		this._uris = new Hashtable<String, URI>();
		this._prefixes = new Hashtable<Integer, String>();

		if (!empty) {
			// Add Standard Namespaces
			this.AddNamespace("rdf", new URI(RDF));
			this.AddNamespace("rdfs", new URI(RDFS));
			this.AddNamespace("xsd", new URI(XSD));
			this.AddNamespace("owl", new URI(OWL));
			this.AddNamespace("owl2xml", new URI(OWL2XML));
			this.AddNamespace("rdl", new URI(RDL));
			this.AddNamespace("tpl", new URI(TPL));
			this.AddNamespace("eg", new URI(EG));
			this.AddNamespace("dm", new URI(DM));
			this.AddNamespace("p8dm", new URI(P8DM));
			this.AddNamespace("p8", new URI(P8));
			this.AddNamespace("templates", new URI(TEMPLATES));
		}
	}

	protected NamespaceMapper(NsMapper nsmapper) throws Exception {
		this(true);
		this.Import(nsmapper);
	}

	public String GetPrefix(URI uri) throws Exception {
		int hash;

		hash = GetEnhancedHashCode(uri);

		if (this._prefixes.containsKey(hash)) {
			return this._prefixes.get(hash);
		} else {
			throw new Exception("The Prefix for the given URI '"
					+ uri.toString()
					+ "' is not known by the in-scope NamespaceMapper");
		}
	}

	public URI GetNamespaceUri(String prefix) {
		if (this._uris.containsKey(prefix)) {
			return this._uris.get(prefix);
		} else {
			throw new RuntimeException(
					"The Namespace URI for the given Prefix '" + prefix
							+ "' is not known by the in-scope NamespaceMapper");
		}
	}

	public void AddNamespace(String prefix, URI uri) throws Exception {
		int hash = GetEnhancedHashCode(uri);
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

	public final void RemoveNamespace(String prefix) throws Exception {
		// Check the Namespace is defined
		if (this._uris.containsKey(prefix)) {
			URI u = this._uris.get(prefix);

			// Remove the Prefix to Uri Mapping
			this._uris.remove(prefix);

			// Remove the corresponding Uri to Prefix Mapping
			int hash = GetEnhancedHashCode(u);
			if (this._prefixes.containsKey(hash)) {
				this._prefixes.remove(hash);
			}
		}
	}

	public static int GetEnhancedHashCode(URI u) throws Exception {
		if (u == null) {
			throw new Exception(
					"Cannot calculate an Enhanced Hash Code for a null URI");
		}
		return u.toString().hashCode();
	}

	public boolean HasNamespace(String prefix) {
		return this._uris.containsKey(prefix);
	}

	public void Clear() {
		this._prefixes.clear();
		this._uris.clear();
	}

	public final Iterable<String> getPrefixes() {
		return this._uris.keySet();
	}

	public final boolean ReduceToQName(String uri, ReferenceObject<String> qname)
			throws Exception {
		for (URI u : this._uris.values()) {
			String baseuri = u.toString();

			// Does the Uri start with the Base Uri
			if (uri.startsWith(baseuri)) {
				// Remove the Base Uri from the front of the Uri
				qname.argumentValue = uri.substring(baseuri.length());
				// Add the Prefix back onto the front plus the colon to give a
				// QName
				qname.argumentValue = this._prefixes.get(GetEnhancedHashCode(u))
						+ ":" + qname.argumentValue;
				if (qname.argumentValue.equals(":")) {
					continue;
				}
				if (qname.argumentValue.contains("/")
						|| qname.argumentValue.contains("#")) {
					continue;
				}
				return true;
			}
		}
		qname.argumentValue = "";
		return false;
	}

	public void Import(NsMapper nsmap) {
		String tempPrefix = "ns0";
		int tempPrefixID = 0;
		try {

			for (String prefix : nsmap.getPrefixes()) {
				if (!this._uris.containsKey(prefix)) {
					this.AddNamespace(prefix, nsmap.GetNamespaceUri(prefix));
				} else {
					if (!this._uris.get(prefix).equals(
							nsmap.GetNamespaceUri(prefix))) {
						while (this._uris.containsKey(tempPrefix)) {
							tempPrefixID++;
							tempPrefix = "ns" + tempPrefixID;
						}

						this.AddNamespace(tempPrefix,
								nsmap.GetNamespaceUri(prefix));

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

	public final String ResolveQName(String qname, NamespaceMapper nsmap,
			URI baseUri) {
		String output;

		if (qname.startsWith(":")) {
			// QName in Default Namespace
			if (nsmap.HasNamespace("")) {
				// Default Namespace Defined
				output = nsmap.GetNamespaceUri("").toString()
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
				output = nsmap.GetNamespaceUri("").toString() + parts[0];
			} else {
				output = nsmap.GetNamespaceUri(parts[0]).toString() + parts[1];
			}
		}

		return output;
	}

	public final String prefixString() {
		StringBuilder prefixes = new StringBuilder();
		for (Entry<Integer, String> pref : this._prefixes.entrySet()) {
			prefixes.append("PREFIX " + pref.getValue() + ": <"
					+ this.GetNamespaceUri(pref.getValue()) + ">/n");
		}
		return prefixes.toString();
	}
}
