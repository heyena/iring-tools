package org.iringtools.exchange.library;

import org.iringtools.adapter.library.dto.DataTransferObjects;
import org.iringtools.adapter.library.manifest.Graph;
 
public interface DiffEngine
{
  DataTransferObjects diff(Graph graph, String sendingXml, String receivingXml) throws Exception;
}
