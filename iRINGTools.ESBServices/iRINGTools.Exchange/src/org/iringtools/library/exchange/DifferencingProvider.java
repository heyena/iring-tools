package org.iringtools.library.exchange;

import java.util.Collections;
import java.util.List;

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
import org.iringtools.utility.DtiComparator;

public class DifferencingProvider
{
  private static final Logger logger = Logger.getLogger(DifferencingProvider.class);

  public DataTransferIndices diff(DataTransferIndices sourceDtis, DataTransferIndices targetDtis)
  {
    DtiComparator dtic = new DtiComparator();
    DataTransferIndices resultDtis = new DataTransferIndices();
    DataTransferIndexList resultDtiList = new DataTransferIndexList();
    resultDtis.setDataTransferIndexList(resultDtiList);
    List<DataTransferIndex> resultDtiListItems = resultDtiList.getItems();
    
    /*
     * Case 1:
     *    Source DTIs: 
     *    Target DTIs: x x x x
     */
    if (sourceDtis == null || sourceDtis.getDataTransferIndexList() == null || sourceDtis.getDataTransferIndexList().getItems().size() == 0)
    {
      if (targetDtis != null && targetDtis.getDataTransferIndexList() != null)
      {
        List<DataTransferIndex> targetDtiItems = targetDtis.getDataTransferIndexList().getItems();
        Collections.sort(targetDtiItems, dtic);
        DataTransferIndex previousDti = null;
        
        for (int i = 0; i < targetDtiItems.size(); i++)
        {
          DataTransferIndex dti = targetDtiItems.get(i);          
          dti.setDuplicateCount(1);
          dti.setTransferType(TransferType.DELETE);
          
          if (previousDti != null && dti.getIdentifier().equalsIgnoreCase(previousDti.getIdentifier()))
          {
            previousDti.setDuplicateCount(previousDti.getDuplicateCount() + 1);
            targetDtiItems.remove(i--);
          }
          else
          {
            previousDti = dti;
          }
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
        List<DataTransferIndex> sourceDtiItems = sourceDtis.getDataTransferIndexList().getItems();
        Collections.sort(sourceDtiItems, dtic);
        DataTransferIndex previousDti = null;
        
        for (int i = 0; i < sourceDtiItems.size(); i++)
        {
          DataTransferIndex dti = sourceDtiItems.get(i); 
          dti.setDuplicateCount(1);
          dti.setTransferType(TransferType.ADD);
          
          if (previousDti != null && dti.getIdentifier().equalsIgnoreCase(previousDti.getIdentifier()))
          {
            previousDti.setDuplicateCount(previousDti.getDuplicateCount() + 1);            
            sourceDtiItems.remove(i--);
          }
          else
          {
            previousDti = dti;
          }
        }
      }

      return sourceDtis;
    }    
  	
    List<DataTransferIndex> sourceDtiList = sourceDtis.getDataTransferIndexList().getItems();
    List<DataTransferIndex> targetDtiList = targetDtis.getDataTransferIndexList().getItems();
    
    Collections.sort(sourceDtiList, dtic);
    Collections.sort(targetDtiList, dtic);

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
      DataTransferIndex previousDti = null;
      List<DataTransferIndex> sourceDtiItems = sourceDtis.getDataTransferIndexList().getItems();
      
      for (int i = 0; i < sourceDtiItems.size(); i++)
      {
        DataTransferIndex dti = sourceDtiItems.get(i);          
        dti.setDuplicateCount(1);
        dti.setTransferType(TransferType.ADD);
        
        if (previousDti != null && dti.getIdentifier().equalsIgnoreCase(previousDti.getIdentifier()))
        {
          previousDti.setDuplicateCount(previousDti.getDuplicateCount() + 1);          
          sourceDtiItems.remove(i--);
        }
        else
        {
          previousDti = dti;
        }
      }
      
      List<DataTransferIndex> targetDtiItems = targetDtis.getDataTransferIndexList().getItems();
      
      for (int i = 0; i < targetDtiItems.size(); i++)
      {
        DataTransferIndex dti = targetDtiItems.get(i);          
        dti.setDuplicateCount(1);
        dti.setTransferType(TransferType.DELETE);
        
        if (previousDti != null && dti.getIdentifier().equalsIgnoreCase(previousDti.getIdentifier()))
        {
          previousDti.setDuplicateCount(previousDti.getDuplicateCount() + 1);
          targetDtiItems.remove(i--);
        }
        else
        {
          previousDti = dti;
        }
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
        sourceDti.setDuplicateCount(1);
        resultDtiListItems.add(sourceDti);
      
        // detect source duplicates and increment the count
        while (++sourceIndex < sourceDtiList.size() && sourceDti.getIdentifier().equalsIgnoreCase(sourceDtiList.get(sourceIndex).getIdentifier()))
        {
          sourceDti.setDuplicateCount(sourceDti.getDuplicateCount() + 1);
          
          if (sourceDti.getDuplicateCount() > 1)
          {
            sourceDtiList.remove(sourceIndex--);
          }
        }
      }
      else if (value == 0)  // identifiers match, can be SYNC or CHANGE
      {
        DataTransferIndex resultDti = new DataTransferIndex();
        resultDti.setDuplicateCount(1);
        resultDti.setIdentifier(sourceDti.getIdentifier());
        resultDti.setInternalIdentifier(sourceDti.getInternalIdentifier() + Constants.CHANGE_TOKEN + targetDti.getInternalIdentifier());
        
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
     
        // detect source duplicates and increment the count
        while (++sourceIndex < sourceDtiList.size() && (resultDti.getIdentifier().equalsIgnoreCase(sourceDtiList.get(sourceIndex).getIdentifier())))
        {
          resultDti.setDuplicateCount(resultDti.getDuplicateCount() + 1);
        }
        // skip target duplicates
        while (++targetIndex < targetDtiList.size() && (targetDti.getIdentifier().equalsIgnoreCase(targetDtiList.get(targetIndex).getIdentifier())))
          //   while (targetIndex < targetDtiList.size() && targetDti.getIdentifier().equalsIgnoreCase(targetDtiList.get(targetIndex).getIdentifier()))
         {
           resultDti.setDuplicateCount(resultDti.getDuplicateCount() + 1);
           //  targetIndex++;         	
         }       
      }
      else
      {
        targetDti.setTransferType(TransferType.DELETE);
        targetDti.setHashValue(null);
        targetDti.setDuplicateCount(1);
        resultDtiListItems.add(targetDti);

        // skip target duplicates
        while (++targetIndex < targetDtiList.size() && (targetDti.getIdentifier().equalsIgnoreCase(targetDtiList.get(targetIndex).getIdentifier())))
        {
        	targetDti.setDuplicateCount(targetDti.getDuplicateCount() + 1);
        	
          if (targetDti.getDuplicateCount() > 1)
          {
            targetDtiList.remove(targetIndex--);
          }
        }
      }
    }

    if (sourceIndex < sourceDtiList.size())
    {
      for (int i = sourceIndex; i < sourceDtiList.size(); i++)
      {
        DataTransferIndex sourceDti = sourceDtis.getDataTransferIndexList().getItems().get(i);
        
        if(sourceIndex ==  sourceDtiList.size())
        {
      	  break;
        }
        
        sourceDti.setTransferType(TransferType.ADD);
        sourceDti.setDuplicateCount(1);
        resultDtiListItems.add(sourceDti);  
              
        // detect source duplicates and increment the count
        while (++sourceIndex < sourceDtiList.size() && sourceDti.getIdentifier().equalsIgnoreCase(sourceDtiList.get(sourceIndex).getIdentifier()))
        {
          sourceDti.setDuplicateCount(sourceDti.getDuplicateCount() + 1);
          
          if (sourceDti.getDuplicateCount() > 1)
          {
            sourceDtiList.remove(sourceIndex--);
          }
        }
      }
    }
    else if (targetIndex < targetDtiList.size())
    {
      for (int i = targetIndex; i < targetDtiList.size(); i++)
      {
    	  if (targetIndex ==  targetDtiList.size())
        {
      	  break;
        }
    	  
        DataTransferIndex targetDti = targetDtis.getDataTransferIndexList().getItems().get(i);
        targetDti.setTransferType(TransferType.DELETE);
        targetDti.setHashValue(null);
        targetDti.setDuplicateCount(1);
        resultDtiListItems.add(targetDti);
        
        // skip target duplicates
        while (++targetIndex < targetDtiList.size() && (targetDti.getIdentifier().equalsIgnoreCase(targetDtiList.get(targetIndex).getIdentifier())))
        {
        	targetDti.setDuplicateCount(targetDti.getDuplicateCount() + 1);
        	
        	if (targetDti.getDuplicateCount() > 1)
          {
        	  targetDtiList.remove(targetIndex--);
          }
        }
      }
    }

    resultDtis.setSortType(sourceDtis.getSortType());
    resultDtis.setSortOrder(sourceDtis.getSortOrder());    
  	
    return resultDtis;
  }

  // compare 2 DTO lists using in-line differencing - result will be saved in source DTO list
  public DataTransferObjects diff(Manifest manifest, DataTransferObjects sourceDtos, DataTransferObjects targetDtos) 
  {
    if (sourceDtos == null || targetDtos == null)
      return null;    
  	
    List<DataTransferObject> targetDtoList = targetDtos.getDataTransferObjectList().getItems();
    List<DataTransferObject> sourceDtoList = sourceDtos.getDataTransferObjectList().getItems();

    if (sourceDtoList.size() == 0 || targetDtoList.size() == 0)
      return null;

    DtoComparator dtoc = new DtoComparator();
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
        	String classId = classTemplates.getClazz().getId();
        	//int classIndex = classTemplates.getClazz().getIndex();
        	String classPath = classTemplates.getClazz().getPath();
          ClassObject targetClassObject = getClassObject(targetClassObjectList, classId,classPath);
          ClassObject sourceClassObject = getClassObject(sourceClassObjectList, classId,classPath);
          ClassTemplates manifestClassObject = getClassTemplates(manifest, classId,classPath); 
          
                 
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

  private ClassObject getClassObject(List<ClassObject> classObjects, String classId, String classPath)
  {
	boolean flag = false;
	  
    for (ClassObject classObject : classObjects)
    {
      flag = (classObject.getPath() == null || classObject.getPath().length()==0)  ? (classPath == null || classPath.length()==0) : classObject.getPath().equals(classPath);
      if (classObject.getClassId().equals(classId) &&  flag )
        return classObject;
    }
    return null;
  }
  
  private ClassTemplates getClassTemplates(Manifest manifest, String classId, String classPath)
  {
	boolean flag = false;
	
  	for (Graph graph : manifest.getGraphs().getItems())
  	{
	    for (ClassTemplates classTemplates : graph.getClassTemplatesList().getItems())
	    {
	      flag = (classTemplates.getClazz().getPath() == null ||classTemplates.getClazz().getPath().length()==0 ) ? (classPath == null || classPath.length()==0) : classTemplates.getClazz().getPath().equals(classPath);
	      if (classTemplates.getClazz().getId().equals(classId) && flag)
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
