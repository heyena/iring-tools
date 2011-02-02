package org.iringtools.services.core;

import java.util.Collections;
import java.util.Hashtable;
import java.util.List;

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
import org.iringtools.dxfr.request.DfiRequest;
import org.iringtools.dxfr.request.DfoRequest;

public class DifferencingProvider
{
  //private Hashtable<String, String> settings;

  public DifferencingProvider(Hashtable<String, String> settings)
  {
    //this.settings = settings;
  }

  public DataTransferIndices diff(DfiRequest dxiRequest)
  {
    DataTransferIndices resultDtis = new DataTransferIndices();
    DataTransferIndexList resultDtiList = new DataTransferIndexList();
    resultDtis.setDataTransferIndexList(resultDtiList);
    List<DataTransferIndex> resultDtiListItems = resultDtiList.getItems();
    
    DataTransferIndices sourceDtis = null;
    DataTransferIndices targetDtis = null;
    
    /* determine source and target DTIs */
    List<DataTransferIndices> dtisList = dxiRequest.getDataTransferIndicies();
    
    if (dtisList == null || dtisList.size() < 2) 
      return null;
    
    if (dtisList.get(0).getScopeName().equalsIgnoreCase(dxiRequest.getSourceScopeName()) &&
      dtisList.get(0).getAppName().equalsIgnoreCase(dxiRequest.getSourceAppName()))
    {
      sourceDtis = dtisList.get(0);
      targetDtis = dtisList.get(1);
    }
    else
    {
      sourceDtis = dtisList.get(1);
      targetDtis = dtisList.get(0);
    }    

    /*
     * Case 1:
     * 
     *    Source DTIs:
     *    Target DTIs: x x x x 
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
     *    Source DTIs: x x x x
     *    Target DTIs: 
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
      DataTransferIndex sourceDti = sourceDtis.getDataTransferIndexList().getItems().get(sourceIndex);
      DataTransferIndex targetDti = targetDtis.getDataTransferIndexList().getItems().get(targetIndex);
      
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

  // compare 2 DTO lists using in-line differencing - result will be saved in source DTO list
  public DataTransferObjects diff(DfoRequest dxoRequest) throws Exception
  {
    /* determine source and target DTOs */
    List<DataTransferObjects> dtosList = dxoRequest.getDataTransferObjects();
    DataTransferObjects sourceDtos = null;
    DataTransferObjects targetDtos = null;
    
    if (dtosList == null || dtosList.size() < 2) 
      return null;
    
    if (dtosList.get(0).getScopeName().equalsIgnoreCase(dxoRequest.getSourceScopeName()) &&
      dtosList.get(0).getAppName().equalsIgnoreCase(dxoRequest.getSourceAppName()))
    {
      sourceDtos = dtosList.get(0);
      targetDtos = dtosList.get(1);
    }
    else
    {
      sourceDtos = dtosList.get(1);
      targetDtos = dtosList.get(0);
    }    
    
    if (sourceDtos == null || targetDtos == null) return null;
    
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
            RoleType targetRoleType = targetRoleObject.getType();

            if (targetRoleType == RoleType.PROPERTY ||
                targetRoleType == RoleType.DATA_PROPERTY ||
                targetRoleType == RoleType.OBJECT_PROPERTY)
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
