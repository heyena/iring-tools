package org.iringtools.adapter.library;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import javax.xml.namespace.QName;
import org.apache.axiom.om.OMElement;
import org.apache.log4j.Logger;
import org.iringtools.adapter.library.dto.*;
import org.iringtools.adapter.library.dto.ClassObject.TemplateObjects;
import org.iringtools.adapter.library.dto.DataTransferObject.ClassObjects;
import org.iringtools.adapter.library.dto.TemplateObject.RoleObjects;
import org.iringtools.adapter.library.manifest.*;
import org.iringtools.adapter.library.manifest.GraphMap.ClassTemplateListMaps;
import org.iringtools.adapter.library.manifest.TemplateMap.RoleMaps;

public class DiffEngine
{
  private static final String RDF_NS = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
  private static final String OWL_NS = "http://www.w3.org/2002/07/owl#";
  private static final String RDL_NS = "http://rdl.rdlfacade.org/data#";
  private static final String TPL_NS = "http://tpl.rdlfacade.org/data#";

  private static final QName RDF_ABOUT = new QName(RDF_NS, "about");
  private static final QName RDF_TYPE = new QName(RDF_NS, "type");
  private static final QName RDF_RESOURCE = new QName(RDF_NS, "resource");
  private static final QName OWL_THING = new QName(OWL_NS, "Thing");

  private static final Logger logger = Logger.getLogger(DiffEngine.class);
  private GraphMap graphMap;

  public DiffEngine(GraphMap graphMap)
  {
    this.graphMap = graphMap;
  }

  private List<String> getClassInstances(OMElement rdf, String classId)
  {
    @SuppressWarnings("rawtypes")
    Iterator owlThings = rdf.getChildrenWithName(OWL_THING);
    String qualClassId = classId.replace("rdl:", RDL_NS);
    List<String> classInstances = new ArrayList<String>();

    while (owlThings.hasNext())
    {
      OMElement owlThing = (OMElement) owlThings.next();
      OMElement rdfType = owlThing.getFirstChildWithName(RDF_TYPE);

      if (rdfType != null && rdfType.getAttribute(RDF_RESOURCE).getAttributeValue().equals(qualClassId))
      {
        String classInstance = owlThing.getAttribute(RDF_ABOUT).getAttributeValue();
        classInstances.add(classInstance);
        owlThings.remove();
      }
    }

    return classInstances;
  }

  private List<TemplateObject> getTemplateObjectList(OMElement rdf, String classInstance, TemplateMap templateMap)
      throws Exception
  {
    List<TemplateObject> templateObjects = new ArrayList<TemplateObject>();
    String templateId = templateMap.getTemplateId();
    String qualTemplateId = templateId.replace("tpl:", TPL_NS);
    List<RoleMap> roleMaps = templateMap.getRoleMaps().getRoleMap();

    @SuppressWarnings("rawtypes")
    Iterator owlThings = rdf.getChildrenWithName(OWL_THING);

    while (owlThings.hasNext())
    {
      OMElement owlThing = (OMElement) owlThings.next();
      OMElement rdfType = owlThing.getFirstChildWithName(RDF_TYPE);

      if (rdfType.getAttribute(RDF_RESOURCE).getAttributeValue().equals(qualTemplateId))
      {
        boolean isClassInstance = false;
        RoleObjects roleObjects = new RoleObjects();
        List<RoleObject> roleObjectList = roleObjects.getRoleObject();

        for (RoleMap roleMap : roleMaps)
        {
          RoleType roleType = roleMap.getType();
          String roleId = roleMap.getRoleId();
          QName qualRoleId = new QName(TPL_NS, roleId.replace("tpl:", ""));
          OMElement roleElement = owlThing.getFirstChildWithName(qualRoleId);

          if (roleElement == null)
          {
            if (roleMap.getTransferOption() == TransferOption.REQUIRED)
              throw new Exception("Sending RDF does not have role [" + roleId + "].");

            break;
          }

          RoleObject roleObject = new RoleObject();
          roleObjectList.add(roleObject);
          roleObject.setRoleId(roleId);
          roleObject.setName(roleMap.getName());

          if (roleType == RoleType.POSSESSOR)
          {
            String reference = roleElement.getAttribute(RDF_RESOURCE).getAttributeValue();

            if (reference.equals(classInstance))
            {
              isClassInstance = true;
            }
            else
            {
              break;
            }
          }
          else if (roleType == RoleType.REFERENCE)
          {
            String reference = roleElement.getAttribute(RDF_RESOURCE).getAttributeValue();
            roleObject.setReference(reference.replace(RDL_NS, "rdl:"));
          }
          else
          {
            roleObject.setValue(roleElement.getText());
          }
        }

        if (isClassInstance)
        {
          TemplateObject templateObject = new TemplateObject();
          templateObjects.add(templateObject);
          templateObject.setTemplateId(templateId);
          templateObject.setName(templateMap.getName());
          templateObject.setRoleObjects(roleObjects);
          owlThings.remove();
        }
      }
    }

    return templateObjects;
  }

  private boolean roleObjectsMatch(TemplateObject sendingTemplateObject, TemplateObject receivingTemplateObject)
  {
    List<RoleObject> sendingRoleObjects = sendingTemplateObject.getRoleObjects().getRoleObject();
    List<RoleObject> receivingRoleObjects = receivingTemplateObject.getRoleObjects().getRoleObject();

    for (RoleObject sendingRoleObject : sendingRoleObjects)
    {
      for (RoleObject receivingRoleObject : receivingRoleObjects)
      {
        if (sendingRoleObject.getRoleId().equals((receivingRoleObject.getRoleId())))
        {
          String sendingRoleObjectReference = sendingRoleObject.getReference();
          String receivingRoleObjectReference = receivingRoleObject.getReference();
          String sendingRoleObjectValue = sendingRoleObject.getValue();
          String receivingRoleObjectValue = receivingRoleObject.getValue();

          if ((sendingRoleObjectReference != null && receivingRoleObjectReference != null && !sendingRoleObjectReference
              .equals(receivingRoleObjectReference))
              || (sendingRoleObjectValue != null && receivingRoleObjectValue != null && !sendingRoleObjectValue
                  .equals(receivingRoleObjectValue)))
          {
            return false;
          }
        }
      }
    }

    return true;
  }

  public DataTransferObjects diffRdf(OMElement sendingRdf, OMElement receivingRdf) throws Exception
  {
    if (graphMap == null || graphMap.getClassTemplateListMaps().getClassTemplateListMap().size() == 0)
    {
      String errorMessage = "Unable to perform diffencing due to empty graph.";

      logger.error(errorMessage);
      throw new Exception(errorMessage);
    }

    DataTransferObjects result = new DataTransferObjects();
    List<DataTransferObject> dtoList = result.getDataTransferObject();
    DataTransferObject dto = null;
    List<ClassObject> classObjectList = null;

    List<ClassTemplateListMap> classTemplateListMaps = graphMap.getClassTemplateListMaps().getClassTemplateListMap();
    List<Integer> intersectClassInstanceIndexes = new ArrayList<Integer>();
    boolean isRoot = true;

    for (ClassTemplateListMap classTemplateListMap : classTemplateListMaps)
    {
      ClassMap classMap = classTemplateListMap.getClassMap();
      List<TemplateMap> templateMaps = classTemplateListMap.getTemplateMaps().getTemplateMap();
      String classId = classMap.getClassId();
      List<String> sendingClassInstances = getClassInstances(sendingRdf, classId);
      List<String> receivingClassInstances = getClassInstances(receivingRdf, classId);

      for (int sendingClassInstanceIndex = 0; sendingClassInstanceIndex < sendingClassInstances.size(); sendingClassInstanceIndex++)
      {
        String sendingClassInstance = sendingClassInstances.get(sendingClassInstanceIndex);

        if (isRoot)
        {
          dto = new DataTransferObject();
          dtoList.add(dto);
          dto.setTransferType(TransferType.SYNC);
          ClassObjects classObjects = new ClassObjects();
          dto.setClassObjects(classObjects);
          classObjectList = classObjects.getClassObject();
        }
        else
        {
          dto = dtoList.get(sendingClassInstanceIndex);
          classObjectList = dto.getClassObjects().getClassObject();
        }

        ClassObject classObject = new ClassObject();
        classObjectList.add(classObject);
        classObject.setClassId(classId);
        classObject.setIdentifier(sendingClassInstance);
        classObject.setName(classMap.getName());
        TemplateObjects templateObjects = new TemplateObjects();
        classObject.setTemplateObjects(templateObjects);
        List<TemplateObject> templateObjectList = templateObjects.getTemplateObject();

        boolean classObjectMatched = false;

        for (int receivingClassInstanceIndex = 0; receivingClassInstanceIndex < receivingClassInstances.size(); receivingClassInstanceIndex++)
        {
          String receivingClassInstance = receivingClassInstances.get(receivingClassInstanceIndex);

          // class instance found : both sending and receiving rdf
          if (sendingClassInstance.equalsIgnoreCase(receivingClassInstance))
          {
            if (isRoot)
            {
              intersectClassInstanceIndexes.add(receivingClassInstanceIndex);
            }

            classObjectMatched = true;

            for (TemplateMap templateMap : templateMaps)
            {
              List<TemplateObject> sendingTemplateObjectList = getTemplateObjectList(sendingRdf, sendingClassInstance,
                  templateMap);
              List<TemplateObject> receivingTemplateObjectList = getTemplateObjectList(receivingRdf,
                  sendingClassInstance, templateMap);

              for (TemplateObject sendingTemplateObject : sendingTemplateObjectList)
              {
                boolean templateObjectFound = false;

                for (TemplateObject receivingTemplateObject : receivingTemplateObjectList)
                {
                  if (sendingTemplateObject.getTemplateId().equals(receivingTemplateObject.getTemplateId()))
                  {
                    templateObjectFound = true;

                    if (roleObjectsMatch(sendingTemplateObject, receivingTemplateObject))
                    {
                      sendingTemplateObject.setTransferType(TransferType.SYNC);

                      if (classObject.getTransferType() != TransferType.CHANGE)
                      {
                        classObject.setTransferType(TransferType.SYNC);
                      }
                    }
                    else
                    {
                      sendingTemplateObject.setTransferType(TransferType.CHANGE);
                      classObject.setTransferType(TransferType.CHANGE);
                      dto.setTransferType(TransferType.CHANGE);
                    }
                  }
                }

                if (!templateObjectFound)
                {
                  sendingTemplateObject.setTransferType(TransferType.ADD);
                  classObject.setTransferType(TransferType.CHANGE);
                  dto.setTransferType(TransferType.CHANGE);
                }
              }

              templateObjectList.addAll(sendingTemplateObjectList);
            }
          }
        }

        // class instance : sending rdf but not : receiving rdf, create dto with transfer type ADD
        if (!classObjectMatched)
        {
          if (isRoot)
          {
            dto.setTransferType(TransferType.ADD);
          }

          for (TemplateMap templateMap : templateMaps)
          {
            List<TemplateObject> sendingTemplateObjectList = getTemplateObjectList(sendingRdf, sendingClassInstance,
                templateMap);
            templateObjectList.addAll(sendingTemplateObjectList);
          }

          if (dto.getTransferType() != TransferType.ADD)
          {
            classObject.setTransferType(TransferType.CHANGE);
          }
        }
      }

      if (isRoot)
      {
        for (int receivingClassInstanceIndex = 0; receivingClassInstanceIndex < receivingClassInstances.size(); receivingClassInstanceIndex++)
        {
          if (!intersectClassInstanceIndexes.contains(receivingClassInstanceIndex))
          {
            String receivingClassInstance = receivingClassInstances.get(receivingClassInstanceIndex);

            dto = new DataTransferObject();
            dtoList.add(dto);
            dto.setTransferType(TransferType.DELETE);
            ClassObjects classObjects = new ClassObjects();
            dto.setClassObjects(classObjects);
            classObjectList = classObjects.getClassObject();

            ClassObject classObject = new ClassObject();
            classObjectList.add(classObject);
            classObject.setClassId(classId);
            classObject.setIdentifier(receivingClassInstance);
            classObject.setName(classMap.getName());
          }
        }
      }

      isRoot = false;
    }

    return result;
  }

  public DataTransferObjects diffDto(DataTransferObjects sendingDtos, DataTransferObjects receivingDtos)
      throws Exception
  {
    if (graphMap == null || graphMap.getClassTemplateListMaps().getClassTemplateListMap().size() == 0)
    {
      String errorMessage = "Unable to perform diffencing due to empty graph.";
      logger.error(errorMessage);
      throw new Exception(errorMessage);
    }

    DataTransferObjects result = new DataTransferObjects();
    List<DataTransferObject> resultDtoList = result.getDataTransferObject();
    List<Integer> intersectDtoIndexes = new ArrayList<Integer>();

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
    ClassTemplateListMaps classTemplateListMaps = graphMap.getClassTemplateListMaps();

    for (ClassTemplateListMap classTemplateListMap : classTemplateListMaps)
    {
      ClassMap classMap = classTemplateListMap.getClassMap();
      List<TemplateMap> templateMaps = classTemplateListMap.getTemplateMaps().getTemplateMap();
      ClassObjects classObjects = sourceDto.getClassObjects();
      boolean classFound = false;

      for (ClassObject classObject : classObjects)
      {
        if (classObject.getClassId().equals(classMap.getClassId()))
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
          
          for (TemplateMap templateMap : templateMaps)
          {
            boolean templateFound = false;
            TemplateObjects templateObjects = classObject.getTemplateObjects();

            for (TemplateObject templateObject : templateObjects)
            {
              if (templateObject.getTemplateId().equals(templateMap.getTemplateId()))
              {
                templateFound = true;
                RoleMaps roleMaps = templateMap.getRoleMaps();
                RoleObjects roleObjects = templateObject.getRoleObjects();
                
                if (validateRoleObjects(roleObjects, roleMaps))
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

                  for (RoleMap roleMap : roleMaps)
                  {
                    for (RoleObject roleObject : roleObjects)
                    {
                      if (roleObject.getRoleId().equals(roleMap.getRoleId()))
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
            
            if (!templateFound && templateMap.getTransferOption() == TransferOption.REQUIRED)
            {
              throw new Exception("Sending DTOs does not contain required template [" + templateMap.getName() + "].");
            }
          }
        }
      }

      if (!classFound && classMap.getTransferOption() == TransferOption.REQUIRED)
      {
        throw new Exception("Sending DTOs does not contain required class [" + classMap.getName() + "].");
      }
    }

    return resultDto;
  }

  private boolean validateRoleObjects(RoleObjects roleObjects, RoleMaps roleMaps)
  {
    for (RoleObject roleObject : roleObjects)
    {
      for (RoleMap roleMap : roleMaps)
      {
        if (roleObject.getRoleId().equals(roleMap.getRoleId()) && roleMap.getType() == RoleType.REFERENCE
            && roleObject.getReference() != roleMap.getValue())
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
