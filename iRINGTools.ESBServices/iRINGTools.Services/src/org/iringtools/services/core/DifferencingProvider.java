package org.iringtools.services.core;

import java.util.Collections;
import java.util.List;
import java.util.Map;

import org.apache.log4j.Logger;
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
import org.iringtools.dxfr.manifest.ClassTemplates;
import org.iringtools.dxfr.manifest.Graph;
import org.iringtools.dxfr.manifest.Manifest;
import org.iringtools.dxfr.manifest.Template;
import org.iringtools.dxfr.request.DfiRequest;
import org.iringtools.dxfr.request.DfoRequest;

public class DifferencingProvider
{
  private static final Logger logger = Logger.getLogger(DifferencingProvider.class);

  // private Map<String, Object> settings;

  public DifferencingProvider(Map<String, Object> settings)
  {
    // this.settings = settings;
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
    List<DataTransferIndices> dtisList = dfiRequest.getDataTransferIndices();

    if (dtisList == null || dtisList.size() < 2)
      return null;

    if (dtisList.get(0).getScopeName().equalsIgnoreCase(dfiRequest.getSourceScopeName())
        && dtisList.get(0).getAppName().equalsIgnoreCase(dfiRequest.getSourceAppName()))
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
     *    Source DTIs: 
     *    Target DTIs: x x x x
     */
    if (sourceDtis == null || sourceDtis.getDataTransferIndexList() == null || sourceDtis.getDataTransferIndexList().getItems().size() == 0)
    {
      if (targetDtis != null && targetDtis.getDataTransferIndexList() != null)
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
     *    Source DTIs: x x x x 
     *    Target DTIs:
     */
    if (targetDtis == null || targetDtis.getDataTransferIndexList() == null || targetDtis.getDataTransferIndexList().getItems().size() == 0)
    {
      if (sourceDtis != null && sourceDtis.getDataTransferIndexList() != null)
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
     * Case 3:
     *    Source DTIs:         x x x x 
     *    Target DTIs: x x x x
     * 
     * Case 4:
     *    Source DTIs: x x x x 
     *    Target DTIs:         x x x x
     */
    if (sourceDtiList.get(0).getIdentifier().toLowerCase().compareTo(targetDtiList.get(targetDtiList.size() - 1).getIdentifier().toLowerCase()) > 0
        || targetDtiList.get(0).getIdentifier().toLowerCase().compareTo(sourceDtiList.get(sourceDtiList.size() - 1).getIdentifier().toLowerCase()) > 0)
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
     * Case 5:
     *    Source DTIs:     x x x x 
     *    Target DTIs: x x x x
     *    
     * Case 6:
     *    Source DTIs: x x x x 
     *    Target DTIs:     x x x x
     *    
     * Case 7:
     *    Source DTIs: x x x x 
     *    Target DTIs: x x x x
     */
    int sourceIndex = 0;
    int targetIndex = 0;

    while (sourceIndex < sourceDtiList.size() && targetIndex < targetDtiList.size())
    {
      DataTransferIndex sourceDti = sourceDtis.getDataTransferIndexList().getItems().get(sourceIndex);
      DataTransferIndex targetDti = targetDtis.getDataTransferIndexList().getItems().get(targetIndex);

      int value = sourceDti.getIdentifier().toLowerCase().compareTo(targetDti.getIdentifier().toLowerCase());

      if (value < 0)
      {
        sourceDti.setTransferType(TransferType.ADD);
        resultDtiListItems.add(sourceDti);

        if (sourceIndex < sourceDtiList.size())
          sourceIndex++;
      }
      else if (value == 0)  // identifiers match, can be SYNC or CHANGE
      {
        DataTransferIndex resultDti = new DataTransferIndex();
        resultDti.setIdentifier(sourceDti.getIdentifier());
        resultDti.setInternalIdentifier(sourceDti.getInternalIdentifier() + "->" + targetDti.getInternalIdentifier());
        
        if (sourceDti.getHashValue().equalsIgnoreCase(targetDti.getHashValue()))
        {
          resultDti.setTransferType(TransferType.SYNC);
          resultDti.setHashValue(targetDti.getHashValue());
          resultDti.setSortIndex(targetDti.getSortIndex());
        }
        else
        {
          resultDti.setTransferType(TransferType.CHANGE);
          resultDti.setHashValue(sourceDti.getHashValue());
          resultDti.setSortIndex(sourceDti.getSortIndex());
        }

        resultDtiListItems.add(resultDti);

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

    resultDtis.setSortType(sourceDtis.getSortType());
    resultDtis.setSortOrder(sourceDtis.getSortOrder());    
  	
    return resultDtis;
  }

  // compare 2 DTO lists using in-line differencing - result will be saved in source DTO list
  public DataTransferObjects diff(DfoRequest dfoRequest) throws ServiceProviderException
  {
    // determine which DTO is source and which DTO is target
    List<DataTransferObjects> dtosList = dfoRequest.getDataTransferObjects();
    DataTransferObjects sourceDtos = null;
    DataTransferObjects targetDtos = null;
    Manifest manifest = dfoRequest.getManifest();
    String classId;
    
    if (dtosList == null || dtosList.size() < 2)
      return null;

    if (dtosList.get(0).getScopeName().equalsIgnoreCase(dfoRequest.getSourceScopeName())
        && dtosList.get(0).getAppName().equalsIgnoreCase(dfoRequest.getSourceAppName()))
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

      // sanity check see if the data transfer object has become SYNC'ed since last DTI differencing occurred
      sourceDto.setTransferType(org.iringtools.dxfr.dto.TransferType.SYNC);

      if (targetDto.getClassObjects() != null && sourceDto.getClassObjects() != null)
      {
        List<ClassObject> targetClassObjectList = targetDto.getClassObjects().getItems();
        List<ClassObject> sourceClassObjectList = sourceDto.getClassObjects().getItems();
        List<ClassTemplates> classTemplatesList = manifest.getGraphs().getItems().get(0).getClassTemplatesList().getItems();
        
        for (int j = 0; j < classTemplatesList.size(); j++)
        {
        	ClassTemplates classTemplates = classTemplatesList.get(j);
        	classId = classTemplates.getClazz().getId();
          ClassObject targetClassObject = getClassObject(targetClassObjectList, classId);
          ClassObject sourceClassObject = getClassObject(sourceClassObjectList, classId);
          ClassTemplates manifestClassObject = getClassTemplates(manifest, classId);          
          
          if (sourceClassObject != null && targetClassObject != null)
          {
            // assure target and source identifier are still the same
            if (j == 0 && !targetClassObject.getIdentifier().equalsIgnoreCase(sourceClassObject.getIdentifier()))
            {
              String message = String.format(
                  "Identifiers are out of sync - source identifier [%s], target identifier [%s]",
                  sourceClassObject.getIdentifier(), targetClassObject.getIdentifier());
              logger.error(message);
            }

            if (targetClassObject.getTemplateObjects() != null && sourceClassObject.getTemplateObjects() != null)
            {
              List<Template> manifestTemplateList = manifestClassObject.getTemplates().getItems();
              List<TemplateObject> targetTemplateObjectList = targetClassObject.getTemplateObjects().getItems();
              List<TemplateObject> sourceTemplateObjectList = sourceClassObject.getTemplateObjects().getItems();
              
              sourceClassObject.setTransferType(org.iringtools.dxfr.dto.TransferType.SYNC);  // default SYNC first

              for (Template template : manifestTemplateList)
              {
              	TemplateObject targetTemplateObject = getTemplateObject(targetTemplateObjectList, template.getId());        
                TemplateObject sourceTemplateObject = getTemplateObject(sourceTemplateObjectList, template.getId());
                
                if (targetTemplateObject != null && sourceTemplateObject != null)
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

                      if (targetRoleType == RoleType.PROPERTY || targetRoleType == RoleType.DATA_PROPERTY
                          || targetRoleType == RoleType.OBJECT_PROPERTY || targetRoleType == RoleType.FIXED_VALUE)
                      {
                        RoleObject sourceRoleObject = getRoleObject(sourceRoleObjectList, targetRoleObject.getRoleId());

                        if (sourceRoleObject != null)
                        {
                          boolean roleValueChanged = false;

                          if (sourceRoleObject.getValues() != null)
                          {
                            List<String> sourceValues = sourceRoleObject.getValues().getItems();

                            if (targetRoleObject.getValues() != null)
                            {
                              List<String> targetValues = targetRoleObject.getValues().getItems();

                              if (!(sourceValues.size() == targetValues.size() && sourceValues
                                  .containsAll(targetValues)))
                              {
                                roleValueChanged = true;
                              }
                            }
                            else
                            {
                              roleValueChanged = true;
                            }

                            sourceRoleObject.getOldValues().setItems(targetRoleObject.getValues().getItems());
                          }
                          else
                          {
                            String targetRoleValue = targetRoleObject.getValue();
                            String sourceRoleValue = sourceRoleObject.getValue();

                            if (targetRoleValue == null) targetRoleValue = "";
                            if (sourceRoleValue == null) sourceRoleValue = "";

                            if (!targetRoleValue.equalsIgnoreCase(sourceRoleValue))
                            {
                              sourceRoleObject.setOldValue(targetRoleValue);
                              roleValueChanged = true;
                            }
                          }
                          
                          if (roleValueChanged)
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
                else if (targetTemplateObject == null && sourceTemplateObject != null)
	              {             
                  for (RoleObject roleObject : sourceTemplateObject.getRoleObjects().getItems())
                  {
                    RoleType roleType = roleObject.getType();
                    
                    if (roleType == RoleType.PROPERTY || roleType == RoleType.DATA_PROPERTY
                        || roleType == RoleType.OBJECT_PROPERTY || roleType == RoleType.FIXED_VALUE)
                    {
                      roleObject.setOldValue(null);
                    }
                  }
                  
                  sourceTemplateObject.setTransferType(org.iringtools.dxfr.dto.TransferType.CHANGE);
                  sourceClassObject.setTransferType(org.iringtools.dxfr.dto.TransferType.CHANGE);
                  sourceDto.setTransferType(org.iringtools.dxfr.dto.TransferType.CHANGE);
	              }
                else if (targetTemplateObject != null && sourceTemplateObject == null)
	              {
                  sourceTemplateObjectList.add(targetTemplateObject);
                  
                  for (RoleObject roleObject : targetTemplateObject.getRoleObjects().getItems())
                  {
                    RoleType roleType = roleObject.getType();
                    
                    if (roleType == RoleType.PROPERTY || roleType == RoleType.DATA_PROPERTY
                        || roleType == RoleType.OBJECT_PROPERTY || roleType == RoleType.FIXED_VALUE)
                    {
                      roleObject.setOldValue(roleObject.getValue());
                      roleObject.setValue(null);
                    }
                  }
                  
                  targetTemplateObject.setTransferType(org.iringtools.dxfr.dto.TransferType.CHANGE);
                  sourceClassObject.setTransferType(org.iringtools.dxfr.dto.TransferType.CHANGE);
                  sourceDto.setTransferType(org.iringtools.dxfr.dto.TransferType.CHANGE);
	              }                
              }
            }
          }
          else if (sourceClassObject == null && targetClassObject != null)
          {      
            sourceClassObjectList.add(targetClassObject);
            
            for (TemplateObject templateObject : targetClassObject.getTemplateObjects().getItems())
            {
              for (RoleObject roleObject : templateObject.getRoleObjects().getItems())
              {
                RoleType roleType = roleObject.getType();
                
                if (roleType == RoleType.PROPERTY || roleType == RoleType.DATA_PROPERTY
                    || roleType == RoleType.OBJECT_PROPERTY || roleType == RoleType.FIXED_VALUE)
                {
                  roleObject.setOldValue(roleObject.getValue());
                  roleObject.setValue(null);
                }
              }
              
              templateObject.setTransferType(org.iringtools.dxfr.dto.TransferType.CHANGE);
            }
            
            targetClassObject.setTransferType(org.iringtools.dxfr.dto.TransferType.CHANGE);
            sourceDto.setTransferType(org.iringtools.dxfr.dto.TransferType.CHANGE);
          }
          else if (sourceClassObject != null && targetClassObject == null)
          {
            for (TemplateObject templateObject : sourceClassObject.getTemplateObjects().getItems())
            {
              for (RoleObject roleObject : templateObject.getRoleObjects().getItems())
              {
                RoleType roleType = roleObject.getType();
                
                if (roleType == RoleType.PROPERTY || roleType == RoleType.DATA_PROPERTY
                    || roleType == RoleType.OBJECT_PROPERTY || roleType == RoleType.FIXED_VALUE)
                {
                  roleObject.setOldValue(null);
                }
              }
              
              templateObject.setTransferType(org.iringtools.dxfr.dto.TransferType.CHANGE);
            }
            
            sourceClassObject.setTransferType(org.iringtools.dxfr.dto.TransferType.CHANGE);
            sourceDto.setTransferType(org.iringtools.dxfr.dto.TransferType.CHANGE);
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
  
  private ClassTemplates getClassTemplates(Manifest manifest, String classId)
  {
  	for (Graph graph : manifest.getGraphs().getItems())
  	{
	    for (ClassTemplates classTemplates : graph.getClassTemplatesList().getItems())
	    {
	      if (classTemplates.getClazz().getId().equals(classId))
	        return classTemplates;
	    }
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
