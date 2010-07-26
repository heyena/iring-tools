package org.iringtools.adapter.library;

import java.util.ArrayList;
import java.util.List;
import org.apache.log4j.Logger;
import org.iringtools.adapter.library.dto.*;
import org.iringtools.adapter.library.dto.ClassObject.TemplateObjects;
import org.iringtools.adapter.library.dto.DataTransferObject.ClassObjects;
import org.iringtools.adapter.library.dto.TemplateObject.RoleObjects;
import org.iringtools.adapter.library.manifest.*;
import org.iringtools.adapter.library.manifest.Class;
import org.iringtools.adapter.library.manifest.Graph.ClassTemplatesMaps;
import org.iringtools.adapter.library.manifest.Template.Roles;
import org.iringtools.utility.JaxbUtil;

public class DtoDiffEngine implements DiffEngine
{
  private static final Logger logger = Logger.getLogger(DtoDiffEngine.class);
  private Graph graph;
  
  @Override
  public DataTransferObjects diff(Graph graph, String sendingXml, String receivingXml)
      throws Exception
  {
    if (graph == null || graph.getClassTemplatesMaps().getClassTemplatesMap().size() == 0)
    {
      String errorMessage = "Unable to perform diffencing due to empty graph.";
      logger.error(errorMessage);
      throw new Exception(errorMessage);
    }

    this.graph = graph;
    
    DataTransferObjects result = new DataTransferObjects();
    List<DataTransferObject> resultDtoList = result.getDataTransferObject();
    List<Integer> intersectDtoIndexes = new ArrayList<Integer>();
    
    DataTransferObjects sendingDtos = JaxbUtil.deserialize(DataTransferObjects.class, sendingXml);
    DataTransferObjects receivingDtos = JaxbUtil.deserialize(DataTransferObjects.class, receivingXml);

    for (DataTransferObject sendingDto : sendingDtos)
    {
      TransferType dtoTransferType = TransferType.ADD;
      int receivingDtoIndex = 0;

      for (DataTransferObject receivingDto : receivingDtos)
      {
        ClassObjects sendingClassObjects = sendingDto.getClassObjects();

        for (ClassObject sendingClassObject : sendingClassObjects)
        {
          ClassObjects receivingClassObjects = receivingDto.getClassObjects();
          boolean firstClassObject = true;

          for (ClassObject receivingClassObject : receivingClassObjects)
          {
            if (sendingClassObject.getClassId().equals(receivingClassObject.getClassId()))
            {
              if (sendingClassObject.getIdentifier().equals(receivingClassObject.getIdentifier()))
              {
                if (firstClassObject) // DTOs intersect
                {
                  dtoTransferType = TransferType.SYNC; // assume the DTOs are the same for now
                  intersectDtoIndexes.add(receivingDtoIndex);
                }

                sendingClassObject.setTransferType(TransferType.SYNC); // assume class objects are the same for now
                TemplateObjects sendingTemplateObjects = sendingClassObject.getTemplateObjects();

                for (TemplateObject sendingTemplateObject : sendingTemplateObjects)
                {
                  TemplateObjects receivingTemplateObjects = receivingClassObject.getTemplateObjects();

                  for (TemplateObject receivingTemplateObject : receivingTemplateObjects)
                  {
                    RoleObjects sendingRoleObjects = sendingTemplateObject.getRoleObjects();
                    RoleObjects receivingRoleObjects = receivingTemplateObject.getRoleObjects();

                    if (sendingTemplateObject.getTemplateId().equals(receivingTemplateObject.getTemplateId())
                        && roleObjectsMatch(sendingRoleObjects, receivingRoleObjects))
                    {
                      for (RoleObject sendingRoleObject : sendingRoleObjects)
                      {
                        boolean sameRoleValues = true; // assume role values are the same for now

                        for (RoleObject receivingRoleObject : receivingRoleObjects)
                        {
                          if (sendingRoleObject.getRoleId().equals(receivingRoleObject.getRoleId()))
                          {
                            if (!sendingRoleObject.getValue().equals(receivingRoleObject.getValue()))
                            {
                              sameRoleValues = false;
                            }
                          }
                        }

                        if (!sameRoleValues) // there is a role value difference, update exchangeType
                        {
                          sendingTemplateObject.setTransferType(TransferType.CHANGE);
                          sendingClassObject.setTransferType(TransferType.CHANGE);
                          dtoTransferType = TransferType.CHANGE;
                        }
                        else
                        {
                          sendingTemplateObject.setTransferType(TransferType.SYNC);
                        }
                      }
                    }
                  }
                }
              }
            }

            firstClassObject = false;
          }
        }

        receivingDtoIndex++;
      }

      resultDtoList.add(createDiffDto(sendingDto, dtoTransferType));
    }

    List<DataTransferObject> receivingDtoList = receivingDtos.getDataTransferObject();
    for (int i = 0; i < receivingDtoList.size(); i++)
    {
      if (!intersectDtoIndexes.contains(i))
      {
        resultDtoList.add(createDiffDto(receivingDtoList.get(i), TransferType.DELETE));
      }
    }

    return result;
  }
  
  // creates dto according to receiving manifest and raise exception if sourceDto does not meet manifest requirement  
  private DataTransferObject createDiffDto(DataTransferObject sourceDto, TransferType dtoTransferType) throws Exception
  {
    DataTransferObject resultDto = new DataTransferObject();
    resultDto.setTransferType(dtoTransferType);
    ClassObjects resultClassObjects = new ClassObjects();
    resultDto.setClassObjects(resultClassObjects);
    List<ClassObject> resultClassObjectList = resultClassObjects.getClassObject();
    ClassTemplatesMaps classTemplatesMaps = graph.getClassTemplatesMaps();

    for (ClassTemplatesMap classTemplateListMap : classTemplatesMaps)
    {
      Class clazz = classTemplateListMap.getClazz();
      List<Template> templates = classTemplateListMap.getTemplates().getTemplate();
      ClassObjects classObjects = sourceDto.getClassObjects();
      boolean classFound = false;

      for (ClassObject classObject : classObjects)
      {
        if (classObject.getClassId().equals(clazz.getClassId()))
        {
          classFound = true;

          ClassObject resultClassObject = new ClassObject();
          resultClassObject.setClassId(classObject.getClassId());
          resultClassObject.setName(classObject.getName());
          resultClassObject.setIdentifier(classObject.getIdentifier());
          resultClassObject.setTransferType((dtoTransferType == TransferType.CHANGE) ? classObject.getTransferType()
              : dtoTransferType);
          resultClassObjectList.add(resultClassObject);

          TemplateObjects resultTemplateObjects = new TemplateObjects();
          resultClassObject.setTemplateObjects(resultTemplateObjects);
          List<TemplateObject> resultTemplateObjectList = resultTemplateObjects.getTemplateObject();
          
          for (Template template : templates)
          {
            boolean templateFound = false;
            TemplateObjects templateObjects = classObject.getTemplateObjects();

            for (TemplateObject templateObject : templateObjects)
            {
              if (templateObject.getTemplateId().equals(template.getTemplateId()))
              {
                templateFound = true;
                Roles roles = template.getRoles();
                RoleObjects roleObjects = templateObject.getRoleObjects();
                
                if (validateRoleObjects(roleObjects, roles))
                {
                  TemplateObject resultTemplateObject = new TemplateObject();
                  resultTemplateObject.setTemplateId(templateObject.getTemplateId());
                  resultTemplateObject.setName(templateObject.getName());
                  resultTemplateObject.setTransferType((dtoTransferType == TransferType.CHANGE) ? templateObject
                      .getTransferType() : dtoTransferType);
                  resultTemplateObjectList.add(resultTemplateObject);

                  RoleObjects resultRoleObjects = new RoleObjects();
                  resultTemplateObject.setRoleObjects(resultRoleObjects);
                  List<RoleObject> resultRoleObjectList = resultRoleObjects.getRoleObject();

                  for (Role role : roles)
                  {
                    for (RoleObject roleObject : roleObjects)
                    {
                      if (roleObject.getRoleId().equals(role.getRoleId()))
                      {
                        RoleObject resultRoleObject = new RoleObject();
                        resultRoleObject.setRoleId(roleObject.getRoleId());
                        resultRoleObject.setName(roleObject.getName());
                        resultRoleObject.setReference(roleObject.getReference());
                        resultRoleObject.setValue(roleObject.getValue());
                        resultRoleObjectList.add(resultRoleObject);
                      }
                    }
                  }
                }
              }
            }
            
            if (!templateFound && template.getTransferOption() == TransferOption.REQUIRED)
            {
              throw new Exception("Sending DTOs does not contain required template [" + template.getName() + "].");
            }
          }
        }
      }

      if (!classFound && clazz.getTransferOption() == TransferOption.REQUIRED)
      {
        throw new Exception("Sending DTOs does not contain required class [" + clazz.getName() + "].");
      }
    }

    return resultDto;
  }

  private boolean validateRoleObjects(RoleObjects roleObjects, Roles roles)
  {
    for (RoleObject roleObject : roleObjects)
    {
      for (Role role : roles)
      {
        if (roleObject.getRoleId().equals(role.getRoleId()) && role.getType() == RoleType.REFERENCE
            && roleObject.getReference() != role.getValue())
        {
          return false;
        }
      }
    }

    return true;
  }

  private boolean roleObjectsMatch(RoleObjects sendingRoleObjects, RoleObjects receivingRoleObjects)
  {
    for (RoleObject sendingRoleObject : sendingRoleObjects)
    {
      for (RoleObject receivingRoleObject : receivingRoleObjects)
      {
        if (sendingRoleObject.getRoleId().equals(receivingRoleObject.getRoleId())
            && !sendingRoleObject.getReference().equals(receivingRoleObject.getReference()))
        {
          return false;
        }
      }
    }

    return true;
  }
}
