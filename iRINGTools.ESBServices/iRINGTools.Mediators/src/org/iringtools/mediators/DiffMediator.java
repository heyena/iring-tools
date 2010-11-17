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
import org.iringtools.dxfr.dto.ClassObject;
import org.iringtools.dxfr.dto.DataTransferObject;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.dto.RoleObject;
import org.iringtools.dxfr.dto.RoleType;
import org.iringtools.dxfr.dto.TemplateObject;
import org.iringtools.utility.JaxbUtil;

public class DiffMediator extends AbstractMediator implements ManagedLifecycle
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

      if (payload.getLocalName().equalsIgnoreCase("dxiRequest"))
      {
        @SuppressWarnings("rawtypes")
        Iterator dtiList = payload.getChildrenWithLocalName("dataTransferIndices");      
        OMElement firstDtiElement = (OMElement)dtiList.next();
        OMElement secondDtiElement = (OMElement)dtiList.next();
        
        DataTransferIndices firstDti = JaxbUtil.toObject(DataTransferIndices.class, firstDtiElement.toString());
        DataTransferIndices secondDti = JaxbUtil.toObject(DataTransferIndices.class, secondDtiElement.toString());
        
        // determine which DTI is source and which DTI is target
        DataTransferIndices sourceDti = null;
        DataTransferIndices targetDti = null;
        
        if (firstDti.getScopeName().equalsIgnoreCase(sourceScopeName) &&
            firstDti.getAppName().equalsIgnoreCase(sourceAppName))
        {
          sourceDti = firstDti;
          targetDti = secondDti;
        }
        else
        {
          sourceDti = secondDti;
          targetDti = firstDti;
        }
        
        // set DXI and set its scope and app name
        DataTransferIndices dxi = diff(sourceDti, targetDti);
        
        if (!sourceScopeName.equalsIgnoreCase(targetScopeName))
          dxi.setScopeName(sourceScopeName + " -> " + targetScopeName);
        else
          dxi.setScopeName(sourceScopeName);
        
        dxi.setAppName(sourceAppName + " -> " + targetAppName);
        
        // set DXI to be the result payload
        resultPayload = AXIOMUtil.stringToOM(JaxbUtil.toXml(dxi, false));
      }
      else if (payload.getLocalName().equalsIgnoreCase("dxoRequest"))
      {
        @SuppressWarnings("rawtypes")
        Iterator dtoList = payload.getChildrenWithLocalName("dataTransferObjects");      
        OMElement firstDtoElement = (OMElement)dtoList.next();
        OMElement secondDtoElement = (OMElement)dtoList.next();
        
        DataTransferObjects firstDto = JaxbUtil.toObject(DataTransferObjects.class, firstDtoElement.toString());
        DataTransferObjects secondDto = JaxbUtil.toObject(DataTransferObjects.class, secondDtoElement.toString());
        
        // determine which DTO is source and which DTO is target        
        DataTransferObjects sourceDto = null;
        DataTransferObjects targetDto = null;
        
        if (firstDto.getScopeName().equalsIgnoreCase(sourceScopeName) &&
            firstDto.getAppName().equalsIgnoreCase(sourceAppName))
        {
          sourceDto = firstDto;
          targetDto = secondDto;
        }
        else
        {
          sourceDto = secondDto;
          targetDto = firstDto;
        }
        
        // get DXO and set its scope and app name
        DataTransferObjects dxo = diff(sourceDto, targetDto);
        
        if (!sourceScopeName.equalsIgnoreCase(targetScopeName))
          dxo.setScopeName(sourceScopeName + " -> " + targetScopeName);
        else
          dxo.setScopeName(sourceScopeName);
        
        dxo.setAppName(sourceAppName + " -> " + targetAppName);
        
        // set DXO to be the result payload
        resultPayload = AXIOMUtil.stringToOM(JaxbUtil.toXml(dxo, false));
      }
      
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

  public void init(SynapseEnvironment synEnv) {}

  public void destroy() {}
  
  private DataTransferIndices diff(DataTransferIndices sourceDtis, DataTransferIndices targetDtis)
  {
    DataTransferIndices resultDtis = new DataTransferIndices();
    DataTransferIndexList resultDtiList = new DataTransferIndexList();
    resultDtis.setDataTransferIndexList(resultDtiList);
    List<DataTransferIndex> resultDtiListItems = resultDtiList.getDataTransferIndexListItems();
    
    /*
     * Case 1:
     * 
     *    Source DTIs:
     *    Target DTIs: x x x x 
     */
    if (sourceDtis == null || sourceDtis.getDataTransferIndexList().getDataTransferIndexListItems().size() == 0)
    {
      if (targetDtis != null)
      {
        for (DataTransferIndex dti : targetDtis.getDataTransferIndexList().getDataTransferIndexListItems())
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
     *    Source DTIs: x x x x
     *    Target DTIs: 
     */
    if (targetDtis == null || targetDtis.getDataTransferIndexList().getDataTransferIndexListItems().size() == 0)
    {
      if (sourceDtis != null)
      {
        for (DataTransferIndex dti : sourceDtis.getDataTransferIndexList().getDataTransferIndexListItems())
        {
          dti.setTransferType(TransferType.ADD);
        }
      }

      return sourceDtis;
    }

    List<DataTransferIndex> sourceDtiList = sourceDtis.getDataTransferIndexList().getDataTransferIndexListItems();
    List<DataTransferIndex> targetDtiList = targetDtis.getDataTransferIndexList().getDataTransferIndexListItems();      
    IdentifierComparator identifierComparator = new IdentifierComparator();
    
    Collections.sort(sourceDtiList, identifierComparator);
    Collections.sort(targetDtiList, identifierComparator);

    /*
     * Case 3, 4:
     * 
     *    Source DTIs: x x x x
     *    Target DTIs:         x x x x 
     * 
     *    Source DTIs:         x x x x
     *    Target DTIs: x x x x 
     */
    if (sourceDtiList.get(0).getIdentifier().compareTo(targetDtiList.get(targetDtiList.size() - 1).getIdentifier()) > 0 ||
        targetDtiList.get(0).getIdentifier().compareTo(sourceDtiList.get(sourceDtiList.size() - 1).getIdentifier()) > 0)
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
     *    Source DTIs: x x x x
     *    Target DTIs:     x x x x
     *
     *    Source DTIs: x x x x
     *    Target DTIs: x x x x
     *    
     *    Source DTIs:     x x x x
     *    Target DTIs: x x x x
     */
    int sourceIndex = 0;
    int targetIndex = 0;
    
    while (sourceIndex < sourceDtiList.size() && targetIndex < targetDtiList.size())
    {
      DataTransferIndex sourceDti = sourceDtis.getDataTransferIndexList().getDataTransferIndexListItems().get(sourceIndex);
      DataTransferIndex targetDti = targetDtis.getDataTransferIndexList().getDataTransferIndexListItems().get(targetIndex);
      
      int value = sourceDti.getIdentifier().compareTo(targetDti.getIdentifier());
      
      if (value < 0)
      {
        sourceDti.setTransferType(TransferType.ADD);
        resultDtiListItems.add(sourceDti);
        
        if (sourceIndex < sourceDtiList.size()) sourceIndex++;
      }
      else if (value == 0)
      {
        if (sourceDti.getHashValue().compareTo(targetDti.getHashValue()) == 0)
          targetDti.setTransferType(TransferType.SYNC);
        else
        {
          targetDti.setTransferType(TransferType.CHANGE);
          targetDti.setHashValue(sourceDti.getHashValue());  // only store source hash value
        }
        
        resultDtiListItems.add(targetDti);
        
        if (sourceIndex < sourceDtiList.size()) sourceIndex++;          
        if (targetIndex < targetDtiList.size()) targetIndex++;
      }
      else
      {
        targetDti.setTransferType(TransferType.DELETE);
        targetDti.setHashValue(null);
        resultDtiListItems.add(targetDti);   
        
        if (targetIndex < targetDtiList.size()) targetIndex++;
      }
    }
    
    if (sourceIndex < sourceDtiList.size())
    {
      for (int i = sourceIndex; i < sourceDtiList.size(); i++)
      {
        DataTransferIndex sourceDti = sourceDtis.getDataTransferIndexList().getDataTransferIndexListItems().get(i);
        sourceDti.setTransferType(TransferType.ADD);
        resultDtiListItems.add(sourceDti);
      }
    }
    else if (targetIndex < targetDtiList.size())
    {
      for (int i = targetIndex; i < targetDtiList.size(); i++)
      {
        DataTransferIndex targetDti = targetDtis.getDataTransferIndexList().getDataTransferIndexListItems().get(i);
        targetDti.setTransferType(TransferType.DELETE);
        targetDti.setHashValue(null);
        resultDtiListItems.add(targetDti);
      }
    }
    
    return resultDtis;
  }

  // compare 2 DTO lists using in-line differencing - result will be saved in source DTO list
  private DataTransferObjects diff(DataTransferObjects sourceDtos, DataTransferObjects targetDtos) throws Exception
  {
    if (sourceDtos == null || targetDtos == null) return null;
    
    List<DataTransferObject> targetDtoList = targetDtos.getDataTransferObjectList().getDataTransferObjectListItems();
    List<DataTransferObject> sourceDtoList = sourceDtos.getDataTransferObjectList().getDataTransferObjectListItems();
    
    if (sourceDtoList.size() == 0 || targetDtoList.size() == 0) return null;
  
    DataTransferObjectComparator dtoc = new DataTransferObjectComparator();
    Collections.sort(targetDtoList, dtoc);      
    Collections.sort(sourceDtoList, dtoc);

    for (int i = 0; i < targetDtoList.size(); i++)
    {
      DataTransferObject targetDto = targetDtoList.get(i);
      DataTransferObject sourceDto = sourceDtoList.get(i);
      
      // sanity check see if the data transfer object might have SYNC'ed since DTI differencing occurs 
      sourceDto.setTransferType(org.iringtools.dxfr.dto.TransferType.SYNC);

      List<ClassObject> targetClassObjectList = targetDto.getClassObjects().getClassObjects();
      List<ClassObject> sourceClassObjectList = sourceDto.getClassObjects().getClassObjects();
      
      for (int j = 0; j < targetClassObjectList.size(); j++)
      {
        ClassObject targetClassObject = targetClassObjectList.get(j);
        ClassObject sourceClassObject = sourceClassObjectList.get(j);
        
        // assure target and source identifier are still the same
        if (j == 0 && !targetClassObject.getIdentifier().equalsIgnoreCase(sourceClassObject.getIdentifier()))
        {
          throw new Exception(String.format("Identifiers are out of sync - source identifier [%s], target identifier [%s]", 
              sourceClassObject.getIdentifier(), targetClassObject.getIdentifier()));
        }
        
        sourceClassObject.setTransferType(org.iringtools.dxfr.dto.TransferType.SYNC); // default SYNC first

        List<TemplateObject> targetTemplateObjectList = targetClassObject.getTemplateObjects().getTemplateObjects();
        List<TemplateObject> sourceTemplateObjectList = sourceClassObject.getTemplateObjects().getTemplateObjects();
        
        for (int k = 0; k < targetTemplateObjectList.size(); k++)
        {
          TemplateObject targetTemplateObject = targetTemplateObjectList.get(k);
          TemplateObject sourceTemplateObject = sourceTemplateObjectList.get(k);    
          
          sourceTemplateObject.setTransferType(org.iringtools.dxfr.dto.TransferType.SYNC); // default SYNC first
          
          List<RoleObject> targetRoleObjectList = targetTemplateObject.getRoleObjects().getRoleObjects();
          List<RoleObject> sourceRoleObjectList = sourceTemplateObject.getRoleObjects().getRoleObjects();
          
          // find and set old value for roles that are changed
          for (int l = 0; l < targetRoleObjectList.size(); l++)
          {
            RoleObject targetRoleObject = targetRoleObjectList.get(l);
            
            if (targetRoleObject.getType() == RoleType.PROPERTY)
            {
              RoleObject sourceRoleObject = sourceRoleObjectList.get(l);     

              String targetRoleValue = targetRoleObject.getValue();
              String sourceRoleValue = sourceRoleObject.getValue();
              
              if (targetRoleValue == null) targetRoleValue = "";
              if (sourceRoleValue == null) sourceRoleValue = "";
              
              sourceRoleObject.setOldValue(targetRoleValue);
              
              if (!targetRoleValue.equals(sourceRoleValue))
              {
                sourceTemplateObject.setTransferType(org.iringtools.dxfr.dto.TransferType.CHANGE);
                sourceClassObject.setTransferType(org.iringtools.dxfr.dto.TransferType.CHANGE);
                sourceDto.setTransferType(org.iringtools.dxfr.dto.TransferType.CHANGE);
              }
            }
          }
        }
      }
    }
    
    return sourceDtos;
  }
}
