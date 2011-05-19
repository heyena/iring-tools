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

  public DataTransferIndices diff(DfiRequest dfiRequest)
  {
    DataTransferIndices resultDtis = new DataTransferIndices();
    DataTransferIndexList resultDtiList = new DataTransferIndexList();
    resultDtis.setDataTransferIndexList(resultDtiList);
    List<DataTransferIndex> resultDtiListItems = resultDtiList.getItems();
    
    DataTransferIndices sourceDtis = null;
    DataTransferIndices targetDtis = null;
    
    /* determine source and target DTIs */
    List<DataTransferIndices> dtisList = dfiRequest.getDataTransferIndicies();
    
    if (dtisList == null || dtisList.size() < 2) 
      return null;
    
    if (dtisList.get(0).getScopeName().equalsIgnoreCase(dfiRequest.getSourceScopeName()) &&
      dtisList.get(0).getAppName().equalsIgnoreCase(dfiRequest.getSourceAppName()))
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
      
      resultDtis.setSortType(sourceDtis.getSortType());
      resultDtis.setSortOrder(sourceDtis.getSortOrder());
      
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
        {
          targetDti.setTransferType(TransferType.SYNC);
        }
        else
        {
          targetDti.setTransferType(TransferType.CHANGE);
          targetDti.setHashValue(sourceDti.getHashValue());  // use source hash value
          targetDti.setSortIndex(sourceDti.getSortIndex());  // use source sort index 
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
    
    resultDtis.setSortType(sourceDtis.getSortType());
    resultDtis.setSortOrder(sourceDtis.getSortOrder());
    
    return resultDtis;
  }

  // compare 2 DTO lists using in-line differencing - result will be saved in source DTO list
  public DataTransferObjects diff(DfoRequest dfoRequest) throws Exception
  {
    // determine which DTO is source and which DTO is target
    List<DataTransferObjects> dtosList = dfoRequest.getDataTransferObjects();
    DataTransferObjects sourceDtos = null;
    DataTransferObjects targetDtos = null;
    
    if (dtosList == null || dtosList.size() < 2) 
      return null;
    
    if (dtosList.get(0).getScopeName().equalsIgnoreCase(dfoRequest.getSourceScopeName()) &&
      dtosList.get(0).getAppName().equalsIgnoreCase(dfoRequest.getSourceAppName()))
    {
      sourceDtos = dtosList.get(0);
      targetDtos = dtosList.get(1);
    }
    else
    {
      sourceDtos = dtosList.get(1);
      targetDtos = dtosList.get(0);
    }    
    
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

      // sanity check see if the data transfer object has become SYNC'ed since DTI differencing occurs
      sourceDto.setTransferType(org.iringtools.dxfr.dto.TransferType.SYNC);

      if (targetDto.getClassObjects() != null && sourceDto.getClassObjects() != null)
      {
        List<ClassObject> targetClassObjectList = targetDto.getClassObjects().getItems();
        List<ClassObject> sourceClassObjectList = sourceDto.getClassObjects().getItems();
  
        for (int j = 0; j < targetClassObjectList.size(); j++)
        {
          ClassObject targetClassObject = targetClassObjectList.get(j);
          ClassObject sourceClassObject = getClassObject(sourceClassObjectList, targetClassObject.getClassId());
  
          // assure target and source identifier are still the same
          if (j == 0 && !targetClassObject.getIdentifier().equalsIgnoreCase(sourceClassObject.getIdentifier()))
          {
            throw new Exception(String.format(
                "Identifiers are out of sync - source identifier [%s], target identifier [%s]",
                sourceClassObject.getIdentifier(), targetClassObject.getIdentifier()));
          }
  
          sourceClassObject.setTransferType(org.iringtools.dxfr.dto.TransferType.SYNC); // default SYNC first
  
          if (targetClassObject.getTemplateObjects() != null && sourceClassObject.getTemplateObjects() != null)
          {
            List<TemplateObject> targetTemplateObjectList = targetClassObject.getTemplateObjects().getItems();
            List<TemplateObject> sourceTemplateObjectList = sourceClassObject.getTemplateObjects().getItems();
    
            for (TemplateObject targetTemplateObject : targetTemplateObjectList)
            {
              TemplateObject sourceTemplateObject = getTemplateObject(sourceTemplateObjectList, 
                  targetTemplateObject.getTemplateId());
              
              if (sourceTemplateObject != null)
              {
                sourceTemplateObject.setTransferType(org.iringtools.dxfr.dto.TransferType.SYNC); // default SYNC first
      
                if (targetTemplateObject.getRoleObjects() != null && sourceTemplateObject.getRoleObjects() != null)
                {
                  List<RoleObject> targetRoleObjectList = targetTemplateObject.getRoleObjects().getItems();
                  List<RoleObject> sourceRoleObjectList = sourceTemplateObject.getRoleObjects().getItems();
        
                  // find and set old value for roles that are changed
                  for (RoleObject targetRoleObject : targetRoleObjectList)
                  {
                    RoleType targetRoleType = targetRoleObject.getType();
        
                    if (targetRoleType == RoleType.PROPERTY ||
                        targetRoleType == RoleType.DATA_PROPERTY ||
                        targetRoleType == RoleType.OBJECT_PROPERTY ||
                        targetRoleType == RoleType.FIXED_VALUE)
                    {
                      RoleObject sourceRoleObject = getRoleObject(sourceRoleObjectList, targetRoleObject.getRoleId());
        
                      if (sourceRoleObject != null)
                      {
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
            }
          }
        }
      }
    }
    
    return sourceDtos;
  }
  
  private ClassObject getClassObject(List<ClassObject> classObjects, String classId)
  {
    for (ClassObject classObject : classObjects)
    {
      if (classObject.getClassId().equals(classId))
        return classObject;
    }
    
    return null;
  }
  
  private TemplateObject getTemplateObject(List<TemplateObject> templateObjects, String templateId)
  {
    for (TemplateObject templateObject : templateObjects)
    {
      if (templateObject.getTemplateId().equals(templateId))
        return templateObject;
    }
    
    return null;
  }
  
  private RoleObject getRoleObject(List<RoleObject> roleObjects, String roleId)
  {
    for (RoleObject roleObject : roleObjects)
    {
      if (roleObject.getRoleId().equals(roleId))
        return roleObject;
    }
    
    return null;
  }
}
