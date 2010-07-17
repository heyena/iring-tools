package org.iringtools.adapter.library;

import org.iringtools.adapter.library.dto.DataTransferObjects;
import org.iringtools.adapter.library.manifest.GraphMap;
 
public interface DiffEngine
{
  DataTransferObjects diff(GraphMap graphMap, String sendingXml, String receivingXml) throws Exception;
}
