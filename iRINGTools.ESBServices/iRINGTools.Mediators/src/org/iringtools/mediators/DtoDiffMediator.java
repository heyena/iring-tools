package org.iringtools.mediators;

import java.util.Collections;
import java.util.Iterator;
import java.util.List;

import javax.xml.namespace.QName;

import org.apache.axiom.om.OMElement;
import org.apache.axiom.om.impl.llom.util.AXIOMUtil;
import org.apache.axiom.soap.SOAPBody;
import org.apache.synapse.ManagedLifecycle;
import org.apache.synapse.MessageContext;
import org.apache.synapse.core.SynapseEnvironment;
import org.apache.synapse.mediators.AbstractMediator;
import org.iringtools.dxfr.dti.DataTransferIndex;
import org.iringtools.dxfr.dti.DataTransferIndices;
import org.iringtools.dxfr.dti.TransferType;
import org.iringtools.dxfr.dto.ClassObject;
import org.iringtools.dxfr.dto.DataTransferObject;
import org.iringtools.dxfr.dto.DataTransferObjectList;
import org.iringtools.dxfr.dto.DataTransferObjects;
import org.iringtools.dxfr.dto.RoleObject;
import org.iringtools.dxfr.dto.RoleType;
import org.iringtools.dxfr.dto.TemplateObject;
import org.iringtools.utility.JaxbUtils;

public class DtoDiffMediator extends AbstractMediator implements ManagedLifecycle
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
      
      OMElement dxi = (OMElement)mc.getProperty("dxi");
      
      OMElement dxiSourceElement = dxi.getFirstChildWithName(new QName("http://www.iringtools.org/dxfr/dti", "source"));
      String sourceDtiXml = dxiSourceElement.getFirstElement().toString();
      DataTransferIndices sourceDxfrIndices = JaxbUtils.toObject(DataTransferIndices.class, sourceDtiXml);
      
      OMElement dxiTargetElement = dxi.getFirstChildWithName(new QName("http://www.iringtools.org/dxfr/dti", "target"));
      String targetDtiXml = dxiTargetElement.getFirstElement().toString();
      DataTransferIndices targetDxfrIndices = JaxbUtils.toObject(DataTransferIndices.class, targetDtiXml);
       
      // get current message payload
      SOAPBody soapBody = mc.getEnvelope().getBody();
      OMElement payload = soapBody.getFirstElement();
      
      @SuppressWarnings("rawtypes")
      Iterator dtoLists = payload.getChildrenWithLocalName("dataTransferObjects");
      OMElement firstDtoList = (OMElement)dtoLists.next();
      OMElement secondDtoList = (OMElement)dtoLists.next();
      
      DataTransferObjects firstDtos = JaxbUtils.toObject(DataTransferObjects.class, firstDtoList.toString());
      DataTransferObjects secondDtos = JaxbUtils.toObject(DataTransferObjects.class, secondDtoList.toString());
      
      // determine which DTO is source and which DTO is target
      DataTransferObjects sourceDtos = null;
      DataTransferObjects targetDtos = null;

      if (firstDtos.getScopeName().equalsIgnoreCase(sourceScopeName)
          && firstDtos.getAppName().equalsIgnoreCase(sourceAppName))
      {
        sourceDtos = firstDtos;
        targetDtos = secondDtos;
      }
      else
      {
        sourceDtos = secondDtos;
        targetDtos = firstDtos;
      }
      
      DataTransferObjects resultDtos = new DataTransferObjects();
      DataTransferObjectList resultDtoList = new DataTransferObjectList();
      resultDtos.setDataTransferObjectList(resultDtoList);
      List<DataTransferObject> resultDtoListItems = resultDtoList.getItems();
      
      // set scope and app name
      if (!sourceScopeName.equalsIgnoreCase(targetScopeName))
        resultDtos.setScopeName(sourceScopeName + " -> " + targetScopeName);
      else
        resultDtos.setScopeName(sourceScopeName);

      resultDtos.setAppName(sourceAppName + " -> " + targetAppName);
      
      List<DataTransferObject> sourceDtoListItems = sourceDtos.getDataTransferObjectList().getItems();
      List<DataTransferObject> targetDtoListItems = targetDtos.getDataTransferObjectList().getItems();
      
      // extract add/sync DTOs from source, leave change ones to diff later
      for (int i = 0; i < sourceDtoListItems.size(); i++)
      {
        DataTransferObject dto = sourceDtoListItems.get(i);
        
        if (dto.getClassObjects() != null)
        {
          List<DataTransferIndex> dtiListItems = sourceDxfrIndices.getDataTransferIndexList().getItems();
          
          for (DataTransferIndex dti : dtiListItems)
          {
            if (dto.getIdentifier().equalsIgnoreCase(dti.getIdentifier()))
            {
              TransferType transferType = dti.getTransferType();
              
              if (transferType == TransferType.ADD)
              {
                DataTransferObject addDto = sourceDtoListItems.remove(i--);
                addDto.setTransferType(org.iringtools.dxfr.dto.TransferType.ADD);
                resultDtoListItems.add(addDto);
                break;
              }
              else if (transferType == TransferType.SYNC)
              {
                DataTransferObject syncDto = sourceDtoListItems.remove(i--);
                syncDto.setTransferType(org.iringtools.dxfr.dto.TransferType.SYNC);
                resultDtoListItems.add(syncDto);
                break;
              }
            }
          }
        }
      }
      
      // extract delete DTOs to target, leave change ones to diff later
      for (int i = 0; i < targetDtoListItems.size(); i++)
      {
        DataTransferObject dto = targetDtoListItems.get(i);
        
        if (dto.getClassObjects() != null)
        {
          List<DataTransferIndex> dtiListItems = targetDxfrIndices.getDataTransferIndexList().getItems();
          
          for (DataTransferIndex dti : dtiListItems)
          {
            if (dto.getIdentifier().equalsIgnoreCase(dti.getIdentifier()))
            {
              if (dti.getTransferType() == TransferType.DELETE)
              {
                DataTransferObject deleteDto = targetDtoListItems.remove(i--);
                deleteDto.setTransferType(org.iringtools.dxfr.dto.TransferType.DELETE);
                resultDtoListItems.add(deleteDto);
                break;
              }
            }
          }
        }
      }

      // get DXOs and add them to result
      DataTransferObjects dxos = diff(sourceDtos, targetDtos);
      
      if (dxos != null)
        resultDtoListItems.addAll(dxos.getDataTransferObjectList().getItems());

      // set result DTOs to be message payload
      resultPayload = AXIOMUtil.stringToOM(JaxbUtils.toXml(resultDtos, false));
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

  // compare 2 DTO lists using in-line differencing - result will be saved in source DTO list
  private DataTransferObjects diff(DataTransferObjects sourceDtos, DataTransferObjects targetDtos) throws Exception
  {
    if (sourceDtos == null || targetDtos == null)
      return null;

    List<DataTransferObject> targetDtoList = targetDtos.getDataTransferObjectList().getItems();
    List<DataTransferObject> sourceDtoList = sourceDtos.getDataTransferObjectList().getItems();

    if (sourceDtoList.size() == 0 || targetDtoList.size() == 0)
      return null;

    DataTransferObjectComparator dtoc = new DataTransferObjectComparator();
    Collections.sort(targetDtoList, dtoc);
    Collections.sort(sourceDtoList, dtoc);

    for (int i = 0; i < targetDtoList.size(); i++)
    {
      DataTransferObject targetDto = targetDtoList.get(i);
      DataTransferObject sourceDto = sourceDtoList.get(i);

      // sanity check see if the data transfer object might have SYNC'ed since DTI differencing occurs
      sourceDto.setTransferType(org.iringtools.dxfr.dto.TransferType.SYNC);

      List<ClassObject> targetClassObjectList = targetDto.getClassObjects().getItems();
      List<ClassObject> sourceClassObjectList = sourceDto.getClassObjects().getItems();

      for (int j = 0; j < targetClassObjectList.size(); j++)
      {
        ClassObject targetClassObject = targetClassObjectList.get(j);
        ClassObject sourceClassObject = sourceClassObjectList.get(j);

        // assure target and source identifier are still the same
        if (j == 0 && !targetClassObject.getIdentifier().equalsIgnoreCase(sourceClassObject.getIdentifier()))
        {
          throw new Exception(String.format(
              "Identifiers are out of sync - source identifier [%s], target identifier [%s]",
              sourceClassObject.getIdentifier(), targetClassObject.getIdentifier()));
        }

        sourceClassObject.setTransferType(org.iringtools.dxfr.dto.TransferType.SYNC); // default SYNC first

        List<TemplateObject> targetTemplateObjectList = targetClassObject.getTemplateObjects().getItems();
        List<TemplateObject> sourceTemplateObjectList = sourceClassObject.getTemplateObjects().getItems();

        for (int k = 0; k < targetTemplateObjectList.size(); k++)
        {
          TemplateObject targetTemplateObject = targetTemplateObjectList.get(k);
          TemplateObject sourceTemplateObject = sourceTemplateObjectList.get(k);

          sourceTemplateObject.setTransferType(org.iringtools.dxfr.dto.TransferType.SYNC); // default SYNC first

          List<RoleObject> targetRoleObjectList = targetTemplateObject.getRoleObjects().getItems();
          List<RoleObject> sourceRoleObjectList = sourceTemplateObject.getRoleObjects().getItems();

          // find and set old value for roles that are changed
          for (int l = 0; l < targetRoleObjectList.size(); l++)
          {
            RoleObject targetRoleObject = targetRoleObjectList.get(l);

            if (targetRoleObject.getType() == RoleType.PROPERTY)
            {
              RoleObject sourceRoleObject = sourceRoleObjectList.get(l);

              String targetRoleValue = targetRoleObject.getValue();
              String sourceRoleValue = sourceRoleObject.getValue();

              if (targetRoleValue == null)
                targetRoleValue = "";
              if (sourceRoleValue == null)
                sourceRoleValue = "";

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
