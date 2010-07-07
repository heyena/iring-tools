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

  private List<OMElement> getOwlThingElements(OMElement rdf)
  {
    List<OMElement> list = new ArrayList<OMElement>();
    @SuppressWarnings("rawtypes")
    Iterator iter = rdf.getChildrenWithName(OWL_THING);

    while (iter.hasNext())
    {
      list.add((OMElement) iter.next());
    }

    return list;
  }

  private List<String> getClassInstances(List<OMElement> owlThings, String classId)
  {
    String qualClassId = classId.replace("rdl:", RDL_NS);
    List<String> classInstances = new ArrayList<String>();

    for (int i = 0; i < owlThings.size(); i++)
    {
      OMElement rdfType = owlThings.get(i).getFirstChildWithName(RDF_TYPE);

      if (rdfType != null && rdfType.getAttribute(RDF_RESOURCE).getAttributeValue().equals(qualClassId))
      {
        String classInstance = owlThings.get(i).getAttribute(RDF_ABOUT).getAttributeValue();
        classInstances.add(classInstance);
        owlThings.remove(i--);
      }
    }

    return classInstances;
  }

  private List<TemplateObject> getTemplateObjects(List<OMElement> owlThings, String classInstance, TemplateMap templateMap)
      throws Exception
  {
    List<TemplateObject> templateObjects = new ArrayList<TemplateObject>();
    String templateId = templateMap.getTemplateId();
    String qualTemplateId = templateId.replace("tpl:", TPL_NS);
    List<RoleMap> roleMaps = templateMap.getRoleMaps().getRoleMap();

    for (int i = 0; i < owlThings.size(); i++)
    {
      OMElement owlThing = owlThings.get(i);
      OMElement rdfType = owlThing.getFirstChildWithName(RDF_TYPE);

      if (rdfType.getAttribute(RDF_RESOURCE).getAttributeValue().equals(qualTemplateId))
      {
        boolean isClassInstance = false;
        List<RoleObject> roleObjectList = new ArrayList<RoleObject>();

        for (RoleMap roleMap : roleMaps)
        {
          RoleType roleType = roleMap.getType();
          String roleId = roleMap.getRoleId().replace("tpl:", "");
          QName qualRoleId = new QName(TPL_NS, roleId);
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
            //TODO: resolve value list/map and handle null value
            roleObject.setValue(roleElement.getText());
          }
        }

        if (isClassInstance)
        {
          TemplateObject templateObject = new TemplateObject();
          templateObjects.add(templateObject);
          templateObject.setTemplateId(templateId);
          templateObject.setName(templateMap.getName());

          RoleObjects roleObjects = new RoleObjects();
          templateObject.setRoleObjects(roleObjects);
          roleObjects.setRoleObject(roleObjectList);

          owlThings.remove(i--);
        }
      }
    }

    return templateObjects;
  }

  private boolean templateObjectsMatch(TemplateObject sendingTemplateObject, TemplateObject receivingTemplateObject)
  {
    List<RoleObject> sendingRoleObjects = sendingTemplateObject.getRoleObjects().getRoleObject();
    List<RoleObject> receivingRoleObjects = receivingTemplateObject.getRoleObjects().getRoleObject();

    for (RoleObject sendingRoleObject : sendingRoleObjects)
    {
      for (RoleObject receivingRoleObject : receivingRoleObjects)
      {
        if (sendingRoleObject.getRoleId().equals((receivingRoleObject.getRoleId()))
            && sendingRoleObject.getReference() != receivingRoleObject.getReference())
        {
          return false;
        }
      }
    }

    return true;
  }

  /*
   * private void recursiveDiff(OMElement sendingRdf, OMElement receivingRdf) throws Exception {
   * 
   * }
   */

  public DataTransferObjects diff(OMElement sendingRdf, OMElement receivingRdf) throws Exception
  {
    if (graphMap == null || graphMap.getClassTemplateListMaps().getClassTemplateListMap().size() == 0)
    {
      String errorMessage = "Unable to perform diffencing due to empty graph.";

      logger.error(errorMessage);
      throw new Exception(errorMessage);
    }

    List<OMElement> sendingOwlThings = getOwlThingElements(sendingRdf);
    sendingRdf = null;

    List<OMElement> receivingOwlThings = getOwlThingElements(receivingRdf);
    receivingRdf = null;

    DataTransferObjects result = new DataTransferObjects();
    List<DataTransferObject> resultDtoList = new ArrayList<DataTransferObject>();
    result.setDataTransferObject(resultDtoList);

    List<ClassTemplateListMap> classTemplateListMaps = graphMap.getClassTemplateListMaps().getClassTemplateListMap();

    /*
     * if (classTemplateListMaps.size() > 0) { ClassTemplateListMap classTemplateListMap = classTemplateListMaps.get(0);
     * ClassMap classMap = classTemplateListMap.getClassMap(); List<TemplateMap> templateMaps =
     * classTemplateListMap.getTemplateMaps().getTemplateMap();
     * 
     * for (TemplateMap templateMap : templateMaps) { RoleMaps roleMaps = templateMap.getRoleMaps(); List<RoleMap>
     * roleMapList = new ArrayList<RoleMap>();
     * 
     * for (RoleMap roleMap : roleMapList) { ClassMap referenceClassMap = roleMap.getClassMap();
     * 
     * if (referenceClassMap != null) { recursiveDiff(sendingRdf, receivingRdf); } } } }
     */

    boolean isFirstClassTemplateListMap = true;
    DataTransferObject resultDto = null;
    List<ClassObject> classObjectList = null;

    for (ClassTemplateListMap classTemplateListMap : classTemplateListMaps)
    {
      ClassMap classMap = classTemplateListMap.getClassMap();
      List<TemplateMap> templateMaps = classTemplateListMap.getTemplateMaps().getTemplateMap();

      String classId = classMap.getClassId();
      List<String> sendingClassInstances = getClassInstances(sendingOwlThings, classId);
      List<String> receivingClassInstances = getClassInstances(receivingOwlThings, classId);

      for (String sendingClassInstance : sendingClassInstances)
      {
        if (isFirstClassTemplateListMap)
        {
          resultDto = new DataTransferObject();
          resultDtoList.add(resultDto);
          resultDto.setTransferType(TransferType.SYNC);
          ClassObjects classObjects = new ClassObjects();
          resultDto.setClassObjects(classObjects);
          classObjectList = new ArrayList<ClassObject>();
          classObjects.setClassObject(classObjectList);
        }
        
        ClassObject classObject = new ClassObject();
        classObjectList.add(classObject);
        classObject.setClassId(classId);
        classObject.setIdentifier(sendingClassInstance);
        classObject.setName(classMap.getName());
        
        boolean classObjectMatched = false;

        for (String receivingClassInstance : receivingClassInstances)
        {
          // class instance found in both sending and receiving rdf
          if (sendingClassInstance.equalsIgnoreCase(receivingClassInstance))
          {
            classObjectMatched = true;

            for (TemplateMap templateMap : templateMaps)
            {
              List<TemplateObject> sendingTemplateObjects = getTemplateObjects(sendingOwlThings, sendingClassInstance, templateMap);
              List<TemplateObject> receivingTemplateObjects = getTemplateObjects(receivingOwlThings, sendingClassInstance, templateMap);
              
              for (TemplateObject sendingTemplateObject : sendingTemplateObjects)
              {
                for (TemplateObject receivingTemplateObject : receivingTemplateObjects)
                {
                  //TODO: consider comparing OMElements instead                  
                  if (!sendingTemplateObject.getTemplateId().equals(receivingTemplateObject.getTemplateId()))
                  {
                    if (templateObjectsMatch(sendingTemplateObject, receivingTemplateObject))
                    {
                      List<RoleObject> sendingRoleObjects = sendingTemplateObject.getRoleObjects().getRoleObject();
                      List<RoleObject> receivingRoleObjects = receivingTemplateObject.getRoleObjects().getRoleObject();

                      for (RoleObject sendingRoleObject : sendingRoleObjects)
                      {
                        for (RoleObject receivingRoleObject : receivingRoleObjects)
                        {
                          if (sendingRoleObject.getRoleId().equals(receivingRoleObject.getRoleId()))
                          {
                            if (!sendingRoleObject.getValue().equals(receivingRoleObject.getValue()))
                            {
                              sendingRoleObject.setTransferType(TransferType.CHANGE);
                              sendingTemplateObject.setTransferType(TransferType.CHANGE);
                              resultDto.setTransferType(TransferType.CHANGE);
                            }
                          }
                        }
                      }
                    }
                    else
                    {
                      sendingTemplateObject.setTransferType(TransferType.CHANGE);
                      resultDto.setTransferType(TransferType.CHANGE);
                    }
                  }
                  else if (templateMap.getTransferOpion() == TransferOption.REQUIRED)
                  {
                    throw new Exception("Sending RDF does not contain required template [" + templateMap.getTemplateId() + "].");
                  }
                }
              }
              
              TemplateObjects templateObjects = new TemplateObjects();
              templateObjects.setTemplateObject(sendingTemplateObjects);
              classObject.setTemplateObjects(templateObjects);
            }
          }
        }

        // class instance in sending rdf but not in receiving rdf, create dto with transfer type ADD
        if (!classObjectMatched)
        {
          resultDto.setTransferType(TransferType.ADD);

          TemplateObjects templateObjects = new TemplateObjects();
          classObject.setTemplateObjects(templateObjects);

          for (TemplateMap templateMap : templateMaps)
          {
            List<TemplateObject> templateObjectList = getTemplateObjects(sendingOwlThings, sendingClassInstance, templateMap);
            templateObjects.setTemplateObject(templateObjectList);
          }
        }
      }

      isFirstClassTemplateListMap = false;
    }

    return result;
  }
}
