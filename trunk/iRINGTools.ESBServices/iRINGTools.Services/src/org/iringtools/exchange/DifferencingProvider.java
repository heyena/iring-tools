package org.iringtools.exchange;

import java.util.Collections;
import java.util.Hashtable;
import java.util.List;

import org.iringtools.adapter.dti.IdentifierComparator;
import org.iringtools.adapter.dto.DataTransferObjectComparator;
import org.iringtools.adapter.dti.DataTransferIndices;
import org.iringtools.adapter.dti.DataTransferIndices.DataTransferIndex;
import org.iringtools.adapter.dti.TransferType;
import org.iringtools.adapter.dto.ClassObject;
import org.iringtools.adapter.dto.DataTransferObject;
import org.iringtools.adapter.dto.DataTransferObjects;
import org.iringtools.adapter.dto.RoleObject;
import org.iringtools.adapter.dto.RoleType;
import org.iringtools.adapter.dto.TemplateObject; 

public class DifferencingProvider
{
  //private Hashtable<String, String> settings;

  public DifferencingProvider(Hashtable<String, String> settings)
  {
    //this.settings = settings;
  }

  public DataTransferIndices diff(DataTransferIndices sourceDtis, DataTransferIndices targetDtis)
  {
    DataTransferIndices resultDtis = new DataTransferIndices();
    List<DataTransferIndex> resultDtiList = resultDtis.getDataTransferIndices();

    /*
     * Case 1:
     * 
     *    Source DTIs:
     *    Target DTIs: x x x x 
     */
    if (sourceDtis == null || sourceDtis.getDataTransferIndices().size() == 0)
    {
      if (targetDtis != null)
      {
        for (DataTransferIndex dti : targetDtis.getDataTransferIndices())
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
    if (targetDtis == null || targetDtis.getDataTransferIndices().size() == 0)
    {
      if (sourceDtis != null)
      {
        for (DataTransferIndex dti : sourceDtis.getDataTransferIndices())
        {
          dti.setTransferType(TransferType.ADD);
        }
      }

      return sourceDtis;
    }

    List<DataTransferIndex> sourceDtiList = sourceDtis.getDataTransferIndices();
    List<DataTransferIndex> targetDtiList = targetDtis.getDataTransferIndices();      
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

      resultDtiList.addAll(sourceDtiList);
      resultDtiList.addAll(targetDtiList);
      
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
      DataTransferIndex sourceDti = sourceDtis.getDataTransferIndices().get(sourceIndex);
      DataTransferIndex targetDti = targetDtis.getDataTransferIndices().get(targetIndex);
      
      int value = sourceDti.getIdentifier().compareTo(targetDti.getIdentifier());
      
      if (value < 0)
      {
        sourceDti.setTransferType(TransferType.ADD);
        resultDtiList.add(sourceDti);
        
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
        
        resultDtiList.add(targetDti);
        
        if (sourceIndex < sourceDtiList.size()) sourceIndex++;          
        if (targetIndex < targetDtiList.size()) targetIndex++;
      }
      else
      {
        targetDti.setTransferType(TransferType.DELETE);
        targetDti.setHashValue(null);
        resultDtiList.add(targetDti);   
        
        if (targetIndex < targetDtiList.size()) targetIndex++;
      }
    }
    
    if (sourceIndex < sourceDtiList.size())
    {
      for (int i = sourceIndex; i < sourceDtiList.size(); i++)
      {
        DataTransferIndex sourceDti = sourceDtis.getDataTransferIndices().get(i);
        sourceDti.setTransferType(TransferType.ADD);
        resultDtiList.add(sourceDti);
      }
    }
    else if (targetIndex < targetDtiList.size())
    {
      for (int i = targetIndex; i < targetDtiList.size(); i++)
      {
        DataTransferIndex targetDti = targetDtis.getDataTransferIndices().get(i);
        targetDti.setTransferType(TransferType.DELETE);
        targetDti.setHashValue(null);
        resultDtiList.add(targetDti);
      }
    }
    
    return resultDtis;
  }

  // compare 2 DTO lists using in-line differencing - result will be saved in source DTO list
  public DataTransferObjects diff(DataTransferObjects sourceDtos, DataTransferObjects targetDtos) throws Exception
  {
    if (sourceDtos == null || targetDtos == null) return null;
    
    List<DataTransferObject> targetDtoList = targetDtos.getDataTransferObjects();
    List<DataTransferObject> sourceDtoList = sourceDtos.getDataTransferObjects();
    
    if (sourceDtoList.size() == 0 || targetDtoList.size() == 0) return null;
  
    DataTransferObjectComparator dtoc = new DataTransferObjectComparator();
    Collections.sort(targetDtoList, dtoc);      
    Collections.sort(sourceDtoList, dtoc);

    for (int i = 0; i < targetDtoList.size(); i++)
    {
      DataTransferObject targetDto = targetDtoList.get(i);
      DataTransferObject sourceDto = sourceDtoList.get(i);
      
      // sanity check see if the data transfer object might have SYNC'ed since DTI differencing occurs 
      sourceDto.setTransferType(org.iringtools.adapter.dto.TransferType.SYNC);

      List<ClassObject> targetClassObjectList = targetDto.getClassObjects().getClassObjects();
      List<ClassObject> sourceClassObjectList = sourceDto.getClassObjects().getClassObjects();
      
      for (int j = 0; j < targetClassObjectList.size(); j++)
      {
        ClassObject targetClassObject = targetClassObjectList.get(j);
        ClassObject sourceClassObject = sourceClassObjectList.get(j);
        
        // assure target and source identifier are still the same
        if (j == 0 && !targetClassObject.getIdentifier().equalsIgnoreCase(sourceClassObject.getIdentifier()))
        {
          throw new Exception(String.format("Identifiers out of sync - source identifier [%s], target identifier [%s]", 
              sourceClassObject.getIdentifier(), targetClassObject.getIdentifier()));
        }
        
        sourceClassObject.setTransferType(org.iringtools.adapter.dto.TransferType.SYNC); // default SYNC first

        List<TemplateObject> targetTemplateObjectList = targetClassObject.getTemplateObjects().getTemplateObjects();
        List<TemplateObject> sourceTemplateObjectList = sourceClassObject.getTemplateObjects().getTemplateObjects();
        
        for (int k = 0; k < targetTemplateObjectList.size(); k++)
        {
          TemplateObject targetTemplateObject = targetTemplateObjectList.get(k);
          TemplateObject sourceTemplateObject = sourceTemplateObjectList.get(k);    
          
          sourceTemplateObject.setTransferType(org.iringtools.adapter.dto.TransferType.SYNC); // default SYNC first
          
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
                sourceTemplateObject.setTransferType(org.iringtools.adapter.dto.TransferType.CHANGE);
                sourceClassObject.setTransferType(org.iringtools.adapter.dto.TransferType.CHANGE);
                sourceDto.setTransferType(org.iringtools.adapter.dto.TransferType.CHANGE);
              }
            }
          }
        }
      }
    }
    
    return sourceDtos;
  }
}
