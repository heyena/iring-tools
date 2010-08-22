package org.iringtools.services.diffsvc;

import java.util.ArrayList;
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
import org.iringtools.adapter.library.dto.TemplateObject;
import org.iringtools.adapter.library.dto.TransferType;
import org.iringtools.utility.JaxbUtil;

public class DiffProvider
{
  private static final Logger logger = Logger.getLogger(DiffProvider.class);
  //private Hashtable<String, String> settings;

  public DiffProvider(Hashtable<String, String> settings)
  {
    //this.settings = settings;
  }

  public String diff(DataTransferIndices sendingDxis, DataTransferIndices receivingDxis)
  {
    DataTransferIndices resultDxi = new DataTransferIndices();
    
    try
    {
      List<DataTransferIndex> sendingDxiList = sendingDxis.getDataTransferIndex();
      List<DataTransferIndex> receivingDxiList = receivingDxis.getDataTransferIndex();

      /*
       * Case 1:
       * 
       *    Receiving: * * * * 
       *    Sending:
       */
      if (sendingDxiList.size() == 0)
      {
        // TODO: verify JaxbUtil can serialize null and verify default transfer type is SYNC
        return JaxbUtil.toXml(receivingDxis);
      }

      /*
       * Case 2:
       * 
       *    Receiving: 
       *    Sending:   * * * *
       */
      if (receivingDxiList.size() == 0)
      {
        // TODO: verify JaxbUtil can serialize null
        for (DataTransferIndex dxi : sendingDxiList)
        {
          dxi.setTransferType(TransferType.ADD);
        }

        return JaxbUtil.toXml(sendingDxis);
      }

      IdentifierComparator identifierComparator = new IdentifierComparator();
      Collections.sort(sendingDxiList, identifierComparator);
      Collections.sort(receivingDxiList, identifierComparator);

      /*
       * Case 3, 4:
       * 
       *    Receiving:         * * * * 
       *    Sending:   * * * *
       * 
       *    Receiving: * * * * 
       *    Sending:           * * * *
       */
      if (sendingDxiList.get(0).getIdentifier().compareTo(receivingDxiList.get(sendingDxiList.size() - 1).getIdentifier()) > 0 ||
          receivingDxiList.get(0).getIdentifier().compareTo(sendingDxiList.get(sendingDxiList.size() - 1).getIdentifier()) > 0)
      {
        for (DataTransferIndex dxi : sendingDxiList)
        {
          dxi.setTransferType(TransferType.ADD);
        }

        receivingDxiList.addAll(sendingDxiList);
        resultDxi.setDataTransferIndex(receivingDxiList);
        
        return JaxbUtil.toXml(resultDxi);
      }

      List<DataTransferIndex> resultDxiList = new ArrayList<DataTransferIndex>();
      int lastComparedIndex = -1;

      /*
       * Case 5, 6, 7:
       * 
       *    Receiving: * * * * 
       *    Sending:       * * * *
       * 
       *    Receiving: * * * * 
       *    Sending:   * * * *
       * 
       *    Receiving:     * * * * 
       *    Sending:   * * * *
       */
      for (DataTransferIndex sendingDxi : sendingDxiList)
      {
        if (lastComparedIndex + 1 == receivingDxiList.size())
        {
          sendingDxi.setTransferType(TransferType.ADD);
          resultDxiList.add(sendingDxi);
        }
        else
        {
          for (int i = lastComparedIndex + 1; i < receivingDxiList.size(); i++)
          {
            DataTransferIndex receivingDxi = receivingDxiList.get(i);
            int diffValue = sendingDxi.getIdentifier().compareTo(receivingDxi.getIdentifier());
  
            if (diffValue < 0)
            {
              resultDxiList.add(receivingDxi);
            }
            else if (diffValue == 0)
            {
              if (sendingDxi.getHashValue().compareTo(receivingDxi.getHashValue()) != 0)
                sendingDxi.setTransferType(TransferType.CHANGE);
              
              resultDxiList.add(sendingDxi);
              lastComparedIndex = i;
              break;
            }
            else
            {
              sendingDxi.setTransferType(TransferType.ADD);
              resultDxiList.add(sendingDxi);
              break;
            }
          }
        }
      }
      
      for (int i = lastComparedIndex + 1; i < receivingDxiList.size(); i++)
      {
        resultDxiList.add(receivingDxiList.get(i));
      }
      
      resultDxi.setDataTransferIndex(resultDxiList);
      return JaxbUtil.toXml(resultDxi);
    }
    catch (Exception ex)
    {
      logger.error(ex);
      return null;
    }
  }

  // Assume that sending DTOs and receiving DTOs have same list of identifiers, which is done via differencing DXIs.
  // Result DTOs are receiving DTOs with transfer type on every class and template (in-line differencing).
  public String diff(DataTransferObjects sendingDtos, DataTransferObjects receivingDtos)
  {
    try
    {
      List<DataTransferObject> sendingDtoList = sendingDtos.getDataTransferObject();
      List<DataTransferObject> receivingDtoList = receivingDtos.getDataTransferObject();
    
      Collections.sort(sendingDtoList);      
      Collections.sort(receivingDtoList);
      
      for (int i = 0; i < sendingDtoList.size(); i++)
      {
        DataTransferObject sendingDto = sendingDtoList.get(i);
        DataTransferObject receivingDto = receivingDtoList.get(i);
        
        // sanity check see if the data transfer object has changed to SYNC since DXI differencing occurs 
        receivingDto.setTransferType(TransferType.SYNC); // default SYNC

        List<ClassObject> sendingClassObjectList = sendingDto.getClassObjects().getClassObject();
        List<ClassObject> receivingClassObjectList = receivingDto.getClassObjects().getClassObject();
        
        for (int j = 0; j < sendingClassObjectList.size(); j++)
        {
          ClassObject sendingClassObject = sendingClassObjectList.get(j);
          ClassObject receivingClassObject = receivingClassObjectList.get(j);
          
          receivingClassObject.setTransferType(TransferType.SYNC); // default SYNC first

          List<TemplateObject> sendingTemplateObjectList = sendingClassObject.getTemplateObjects().getTemplateObject();
          List<TemplateObject> receivingTemplateObjectList = receivingClassObject.getTemplateObjects().getTemplateObject();
          
          for (int k = 0; k < sendingTemplateObjectList.size(); k++)
          {
            TemplateObject sendingTemplateObject = sendingTemplateObjectList.get(k);
            TemplateObject receivingTemplateObject = receivingTemplateObjectList.get(k);    
            
            receivingTemplateObject.setTransferType(TransferType.SYNC); // default SYNC first
            
            List<RoleObject> sendingRoleObjectList = sendingTemplateObject.getRoleObjects().getRoleObject();
            List<RoleObject> receivingRoleObjectList = receivingTemplateObject.getRoleObjects().getRoleObject();
            
            for (int l = 0; l < sendingRoleObjectList.size(); l++)
            {
              RoleObject sendingRoleObject = sendingRoleObjectList.get(l);
              RoleObject receivingRoleObject = receivingRoleObjectList.get(l);     

              String sendingRoleValue = sendingRoleObject.getValue();
              String receivingRoleValue = receivingRoleObject.getValue();
              
              receivingRoleObject.setOldValue(sendingRoleValue);
              
              if (sendingRoleValue == null) sendingRoleValue = "";
              if (receivingRoleValue == null) receivingRoleValue = "";
              
              if (!sendingRoleValue.equals(receivingRoleValue))
              {
                receivingTemplateObject.setTransferType(TransferType.CHANGE);
                receivingClassObject.setTransferType(TransferType.CHANGE);
                receivingDto.setTransferType(TransferType.CHANGE);
              }
            }
          }
        }
      }
      
      return JaxbUtil.toXml(receivingDtos);
    }
    catch (Exception ex)
    {
      logger.error(ex);
      return null;
    }
  }
}
