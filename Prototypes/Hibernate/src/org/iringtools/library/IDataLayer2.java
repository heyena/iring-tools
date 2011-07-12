package org.iringtools.library;

import org.apache.axiom.om.OMElement;
import org.iringtools.common.response.Response;

public interface IDataLayer2 extends IDataLayer
{
  Response configure(OMElement configuration);
  OMElement getConfiguration();
}