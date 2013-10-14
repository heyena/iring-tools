package org.iringtools.utility;

import java.net.URI;


public interface NsMapper 
{
	void addNamespace(String prefix, URI uri) throws Exception;

	void clear();

	URI getNamespaceUri(String prefix);

	String getPrefix(URI uri) throws Exception;

	boolean hasNamespace(String prefix);

	void  importMap(NsMapper nsmap);

	Iterable<String> getPrefixes();

	String reduceToQName(String uri) throws Exception;

	void removeNamespace(String prefix) throws Exception;

	String resolveQName(String qname, NamespaceMapper nsmap, URI baseUri);
}

