package org.iringtools.mediators;

import java.util.Collections;
import java.util.Iterator;
import java.util.List;

import org.apache.axiom.om.OMElement;
import org.apache.axiom.om.impl.llom.util.AXIOMUtil;
import org.apache.axiom.soap.SOAPBody;
import org.apache.synapse.ManagedLifecycle;
import org.apache.synapse.MessageContext;
import org.apache.synapse.core.SynapseEnvironment;
import org.apache.synapse.mediators.AbstractMediator;
import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.dxfr.dti.DataTransferIndexList;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dti.TransferType;
import org.iringtools.utility.JaxbUtils;

public class DtiDiffMediator extends AbstractMediator implements ManagedLifecycle
{
  public boolean mediate(MessageContext mc)
  {
    OMElement resultPayload = null;

    try
    {
      // get message context properties
      String sourceScopeName = mc.getProperty("sourceScopeName").toString();
      String sourceAppName = mc.getProperty("sourceAppName").toString();
      String targetScopeName = mc.getProperty("targetScopeName").toString();
      String targetAppName = mc.getProperty("targetAppName").toString();

      // get current message payload
      SOAPBody soapBody = mc.getEnvelope().getBody();
      OMElement payload = soapBody.getFirstElement();

      @SuppressWarnings("rawtypes")
      Iterator dtiLists = payload.getChildrenWithLocalName("dataTransferIndices");
      OMElement firstDtiList = (OMElement)dtiLists.next();
      OMElement secondDtiList = (OMElement)dtiLists.next();

      DataTransferIndices firstDtis = JaxbUtils.toObject(DataTransferIndices.class, firstDtiList.toString());
      DataTransferIndices secondDtis = JaxbUtils.toObject(DataTransferIndices.class, secondDtiList.toString());

      // determine which DTI is source and which DTI is target
      DataTransferIndices sourceDtis = null;
      DataTransferIndices targetDtis = null;

      if (firstDtis.getScopeName().equalsIgnoreCase(sourceScopeName)
          && firstDtis.getAppName().equalsIgnoreCase(sourceAppName))
      {
        sourceDtis = firstDtis;
        targetDtis = secondDtis;
      }
      else
      {
        sourceDtis = secondDtis;
        targetDtis = firstDtis;
      }

      // set DXI and set its scope and app name
      DataTransferIndices dxi = diff(sourceDtis, targetDtis);

      if (!sourceScopeName.equalsIgnoreCase(targetScopeName))
        dxi.setScopeName(sourceScopeName + " -> " + targetScopeName);
      else
        dxi.setScopeName(sourceScopeName);

      dxi.setAppName(sourceAppName + " -> " + targetAppName);

      // set DXI to be the result payload
      resultPayload = AXIOMUtil.stringToOM(JaxbUtils.toXml(dxi, false));

      // set new message payload
      payload.discard();
      soapBody.addChild(resultPayload);
    }
    catch (Exception e)
    {
      handleException("Error in diff mediator service " + e, mc);
    }

    return true;
  }

  public void init(SynapseEnvironment synEnv)
  {
  }

  public void destroy()
  {
  }

  private DataTransferIndices diff(DataTransferIndices sourceDtis, DataTransferIndices targetDtis)
  {
    DataTransferIndices resultDtis = new DataTransferIndices();
    DataTransferIndexList resultDtiList = new DataTransferIndexList();
    resultDtis.setDataTransferIndexList(resultDtiList);
    List<DataTransferIndex> resultDtiListItems = resultDtiList.getItems();

    /*
     * Case 1:
     * 
     * Source DTIs: Target DTIs: x x x x
     */
    if (sourceDtis == null || sourceDtis.getDataTransferIndexList().getItems().size() == 0)
    {
      if (targetDtis != null)
      {
        for (DataTransferIndex dti : targetDtis.getDataTransferIndexList().getItems())
        {
          dti.setTransferType(TransferType.DELETE);
          dti.setHashValue(null);
        }
      }

      return targetDtis;
    }

    /*
     * Case 2:
     * 
     * Source DTIs: x x x x Target DTIs:
     */
    if (targetDtis == null || targetDtis.getDataTransferIndexList().getItems().size() == 0)
    {
      if (sourceDtis != null)
      {
        for (DataTransferIndex dti : sourceDtis.getDataTransferIndexList().getItems())
        {
          dti.setTransferType(TransferType.ADD);
        }
      }

      return sourceDtis;
    }

    List<DataTransferIndex> sourceDtiList = sourceDtis.getDataTransferIndexList().getItems();
    List<DataTransferIndex> targetDtiList = targetDtis.getDataTransferIndexList().getItems();
    IdentifierComparator identifierComparator = new IdentifierComparator();

    Collections.sort(sourceDtiList, identifierComparator);
    Collections.sort(targetDtiList, identifierComparator);

    /*
     * Case 3, 4:
     * 
     * Source DTIs: x x x x Target DTIs: x x x x
     * 
     * Source DTIs: x x x x Target DTIs: x x x x
     */
    if (sourceDtiList.get(0).getIdentifier().compareTo(targetDtiList.get(targetDtiList.size() - 1).getIdentifier()) > 0
        || targetDtiList.get(0).getIdentifier().compareTo(sourceDtiList.get(sourceDtiList.size() - 1).getIdentifier()) > 0)
    {
      for (DataTransferIndex dti : sourceDtiList)
      {
        dti.setTransferType(TransferType.ADD);
      }

      for (DataTransferIndex dti : targetDtiList)
      {
        dti.setTransferType(TransferType.DELETE);
        dti.setHashValue(null);
      }

      resultDtiListItems.addAll(sourceDtiList);
      resultDtiListItems.addAll(targetDtiList);

      return resultDtis;
    }

    /*
     * Case 5, 6, 7:
     * 
     * Source DTIs: x x x x Target DTIs: x x x x
     * 
     * Source DTIs: x x x x Target DTIs: x x x x
     * 
     * Source DTIs: x x x x Target DTIs: x x x x
     */
    int sourceIndex = 0;
    int targetIndex = 0;

    while (sourceIndex < sourceDtiList.size() && targetIndex < targetDtiList.size())
    {
      DataTransferIndex sourceDti = sourceDtis.getDataTransferIndexList().getItems()
          .get(sourceIndex);
      DataTransferIndex targetDti = targetDtis.getDataTransferIndexList().getItems()
          .get(targetIndex);

      int value = sourceDti.getIdentifier().compareTo(targetDti.getIdentifier());

      if (value < 0)
      {
        sourceDti.setTransferType(TransferType.ADD);
        resultDtiListItems.add(sourceDti);

        if (sourceIndex < sourceDtiList.size())
          sourceIndex++;
      }
      else if (value == 0)
      {
        if (sourceDti.getHashValue().compareTo(targetDti.getHashValue()) == 0)
          targetDti.setTransferType(TransferType.SYNC);
        else
        {
          targetDti.setTransferType(TransferType.CHANGE);
          targetDti.setHashValue(sourceDti.getHashValue()); // only store source hash value
        }

        resultDtiListItems.add(targetDti);

        if (sourceIndex < sourceDtiList.size())
          sourceIndex++;
        if (targetIndex < targetDtiList.size())
          targetIndex++;
      }
      else
      {
        targetDti.setTransferType(TransferType.DELETE);
        targetDti.setHashValue(null);
        resultDtiListItems.add(targetDti);

        if (targetIndex < targetDtiList.size())
          targetIndex++;
      }
    }

    if (sourceIndex < sourceDtiList.size())
    {
      for (int i = sourceIndex; i < sourceDtiList.size(); i++)
      {
        DataTransferIndex sourceDti = sourceDtis.getDataTransferIndexList().getItems().get(i);
        sourceDti.setTransferType(TransferType.ADD);
        resultDtiListItems.add(sourceDti);
      }
    }
    else if (targetIndex < targetDtiList.size())
    {
      for (int i = targetIndex; i < targetDtiList.size(); i++)
      {
        DataTransferIndex targetDti = targetDtis.getDataTransferIndexList().getItems().get(i);
        targetDti.setTransferType(TransferType.DELETE);
        targetDti.setHashValue(null);
        resultDtiListItems.add(targetDti);
      }
    }

    return resultDtis;
  }
}
