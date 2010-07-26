package org.iringtools.adapter.library;

import org.iringtools.adapter.library.dto.DataTransferObjects;
import org.iringtools.adapter.library.manifest.Graph;
 
public interface DiffEngine
{
  DataTransferObjects diff(Graph graphMap, String sendingXml, String receivingXml) throws Exception;
}
