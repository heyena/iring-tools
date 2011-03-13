package org.iringtools.utility;

import java.net.URI;


public interface NsMapper 
{
	void AddNamespace(String prefix, URI uri) throws Exception;

	void Clear();

	URI GetNamespaceUri(String prefix);

	String GetPrefix(URI uri) throws Exception;

	boolean HasNamespace(String prefix);


	void  Import(NsMapper nsmap);

	Iterable<String> getPrefixes();

	boolean ReduceToQName(String uri, ReferenceObject<String> qname) throws Exception;

	void RemoveNamespace(String prefix) throws Exception;

	String ResolveQName(String qname, NamespaceMapper nsmap, URI baseUri);
}

