package org.iringtools.services.diffsvc;

import java.util.Collections;
import java.util.Hashtable;
import java.util.List;
import org.apache.log4j.Logger;
import org.iringtools.adapter.library.dto.ClassObject;
import org.iringtools.adapter.library.dto.DataTransferIndices;
import org.iringtools.adapter.library.dto.DataTransferIndices.DataTransferIndex;
import org.iringtools.adapter.library.dto.DataTransferObject;
import org.iringtools.adapter.library.dto.DataTransferObjects;
import org.iringtools.adapter.library.dto.IdentifierComparator;
import org.iringtools.adapter.library.dto.RoleObject;
import org.iringtools.adapter.library.dto.RoleType;
import org.iringtools.adapter.library.dto.TemplateObject; 
import org.iringtools.adapter.library.dto.TransferType;

public class DiffProvider
{
  private static final Logger logger = Logger.getLogger(DiffProvider.class);
  //private Hashtable<String, String> settings;

  public DiffProvider(Hashtable<String, String> settings)
  {
    //this.settings = settings;
  }

  public DataTransferIndices diff(DataTransferIndices sourceDxis, DataTransferIndices targetDxis)
  {
    DataTransferIndices resultDxis = new DataTransferIndices();
    List<DataTransferIndex> resultDxiList = resultDxis.getDataTransferIndex();
    
    try
    {
      /*
       * Case 1:
       * 
       *    Source DXIs:
       *    Target DXIs: x x x x 
       */
      if (sourceDxis == null || sourceDxis.getDataTransferIndex().size() == 0)
      {
        if (targetDxis != null)
        {
          for (DataTransferIndex dxi : targetDxis.getDataTransferIndex())
          {
            dxi.setTransferType(TransferType.DELETE);
          }
        }
        
        return targetDxis;
      }

      /*
       * Case 2:
       * 
       *    Source DXIs: x x x x
       *    Target DXIs: 
       */
      if (targetDxis == null || targetDxis.getDataTransferIndex().size() == 0)
      {
        if (sourceDxis != null)
        {
          for (DataTransferIndex dxi : sourceDxis.getDataTransferIndex())
          {
            dxi.setTransferType(TransferType.ADD);
          }
        }

        return sourceDxis;
      }

      List<DataTransferIndex> sourceDxiList = sourceDxis.getDataTransferIndex();
      List<DataTransferIndex> targetDxiList = targetDxis.getDataTransferIndex();      
      IdentifierComparator identifierComparator = new IdentifierComparator();
      Collections.sort(sourceDxiList, identifierComparator);
      Collections.sort(targetDxiList, identifierComparator);

      /*
       * Case 3, 4:
       * 
       *    Source DXIs: x x x x
       *    Target DXIs:         x x x x 
       * 
       *    Source DXIs:         x x x x
       *    Target DXIs: x x x x 
       */
      if (sourceDxiList.get(0).getIdentifier().compareTo(targetDxiList.get(sourceDxiList.size() - 1).getIdentifier()) > 0 ||
          targetDxiList.get(0).getIdentifier().compareTo(sourceDxiList.get(sourceDxiList.size() - 1).getIdentifier()) > 0)
      {
        for (DataTransferIndex dxi : sourceDxiList)
        {
          dxi.setTransferType(TransferType.ADD);
        }
        
        for (DataTransferIndex dxi : targetDxiList)
        {
          dxi.setTransferType(TransferType.DELETE);
        }

        resultDxiList.addAll(sourceDxiList);
        resultDxiList.addAll(targetDxiList);
        
        return resultDxis;
      }

      /*
       * Case 5, 6, 7:
       *    
       *    Source DXIs: x x x x
       *    Target DXIs:     x x x x
       *
       *    Source DXIs: x x x x
       *    Target DXIs: x x x x
       *    
       *    Source DXIs:     x x x x
       *    Target DXIs: x x x x
       */
      int sourceIndex = 0;
      int targetIndex = 0;
      
      while (sourceIndex < sourceDxiList.size() && targetIndex < targetDxiList.size())
      {
        DataTransferIndex sourceDxi = sourceDxis.getDataTransferIndex().get(sourceIndex);
        DataTransferIndex targetDxi = targetDxis.getDataTransferIndex().get(targetIndex);
        
        int value = sourceDxi.getIdentifier().compareTo(targetDxi.getIdentifier());
        
        if (value < 0)
        {
          sourceDxi.setTransferType(TransferType.ADD);
          resultDxiList.add(sourceDxi);
          
          if (sourceIndex < sourceDxiList.size()) sourceIndex++;
        }
        else if (value == 0)
        {
          if (sourceDxi.getHashValue().compareTo(targetDxi.getHashValue()) == 0)
            targetDxi.setTransferType(TransferType.SYNC);
          else
            targetDxi.setTransferType(TransferType.CHANGE);
          
          resultDxiList.add(targetDxi);
          
          if (sourceIndex < sourceDxiList.size()) sourceIndex++;          
          if (targetIndex < targetDxiList.size()) targetIndex++;
        }
        else
        {
          targetDxi.setTransferType(TransferType.DELETE);
          resultDxiList.add(targetDxi);   
          
          if (targetIndex < targetDxiList.size()) targetIndex++;
        }
      }
      
      if (sourceIndex < sourceDxiList.size())
      {
        for (int i = sourceIndex; i < sourceDxiList.size(); i++)
        {
          DataTransferIndex sourceDxi = sourceDxis.getDataTransferIndex().get(i);
          sourceDxi.setTransferType(TransferType.ADD);
          resultDxiList.add(sourceDxi);
        }
      }
      else if (targetIndex < targetDxiList.size())
      {
        for (int i = targetIndex; i < targetDxiList.size(); i++)
        {
          DataTransferIndex targetDxi = targetDxis.getDataTransferIndex().get(i);
          targetDxi.setTransferType(TransferType.DELETE);
          resultDxiList.add(targetDxi);
        }
      }
      
      return resultDxis;
    }
    catch (Exception ex)
    {
      logger.error(ex);
      return null;
    }
  }

  // in-line differencing to reduce memory allocation
  public DataTransferObjects diff(DataTransferObjects sourceDtos, DataTransferObjects targetDtos)
  {
    try
    {
      List<DataTransferObject> targetDtoList = targetDtos.getDataTransferObject();
      List<DataTransferObject> sourceDtoList = sourceDtos.getDataTransferObject();
    
      Collections.sort(targetDtoList);      
      Collections.sort(sourceDtoList);

      for (int i = 0; i < targetDtoList.size(); i++)
      {
        DataTransferObject targetDto = targetDtoList.get(i);
        DataTransferObject sourceDto = sourceDtoList.get(i);
        
        // sanity check see if the data transfer object might have SYNC'ed since DXI differencing occurs 
        sourceDto.setTransferType(TransferType.SYNC); // default SYNC

        List<ClassObject> targetClassObjectList = targetDto.getClassObjects().getClassObject();
        List<ClassObject> sourceClassObjectList = sourceDto.getClassObjects().getClassObject();
        
        for (int j = 0; j < targetClassObjectList.size(); j++)
        {
          ClassObject targetClassObject = targetClassObjectList.get(j);
          ClassObject sourceClassObject = sourceClassObjectList.get(j);
          
          // assure target and source identifier are still the same
          if (j == 0 && !targetClassObject.getIdentifier().equalsIgnoreCase(sourceClassObject.getIdentifier()))
          {
            String message = String.format("Identifiers out of sync - target identifier [%s], source identifier [%s]", 
                targetClassObject.getIdentifier(), sourceClassObject.getIdentifier());
            logger.warn(message);
            break;
          }
          
          sourceClassObject.setTransferType(TransferType.SYNC); // default SYNC first

          List<TemplateObject> targetTemplateObjectList = targetClassObject.getTemplateObjects().getTemplateObject();
          List<TemplateObject> sourceTemplateObjectList = sourceClassObject.getTemplateObjects().getTemplateObject();
          
          for (int k = 0; k < targetTemplateObjectList.size(); k++)
          {
            TemplateObject targetTemplateObject = targetTemplateObjectList.get(k);
            TemplateObject sourceTemplateObject = sourceTemplateObjectList.get(k);    
            
            sourceTemplateObject.setTransferType(TransferType.SYNC); // default SYNC first
            
            List<RoleObject> targetRoleObjectList = targetTemplateObject.getRoleObjects().getRoleObject();
            List<RoleObject> sourceRoleObjectList = sourceTemplateObject.getRoleObjects().getRoleObject();
            
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
                  sourceTemplateObject.setTransferType(TransferType.CHANGE);
                  sourceClassObject.setTransferType(TransferType.CHANGE);
                  sourceDto.setTransferType(TransferType.CHANGE);
                }
              }
            }
          }
        }
      }
      
      return sourceDtos;
    }
    catch (Exception ex)
    {
      logger.error(ex);
      return null;
    }
  }
}
