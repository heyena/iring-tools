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
import org.iringtools.adapter.library.manifest.Class;
import org.iringtools.utility.IOUtil;

public class RdfDiffEngine implements DiffEngine
{
  private static final String RDF_NS = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
  private static final String OWL_NS = "http://www.w3.org/2002/07/owl#";
  private static final String RDL_NS = "http://rdl.rdlfacade.org/data#";
  private static final String TPL_NS = "http://tpl.rdlfacade.org/data#";

  private static final QName RDF_ABOUT = new QName(RDF_NS, "about");
  private static final QName RDF_TYPE = new QName(RDF_NS, "type");
  private static final QName RDF_RESOURCE = new QName(RDF_NS, "resource");
  private static final QName OWL_THING = new QName(OWL_NS, "Thing");

  private static final Logger logger = Logger.getLogger(DtoDiffEngine.class);

  @Override
  public DataTransferObjects diff(Graph graph, String sendingXml, String receivingXml) throws Exception
  {
    if (graph == null || graph.getClassTemplatesMaps().getClassTemplatesMap().size() == 0)
    {
      String errorMessage = "Unable to perform diffencing due to empty graph.";

      logger.error(errorMessage);
      throw new Exception(errorMessage);
    }
    
    OMElement sendingRdf = IOUtil.stringToXml(sendingXml);
    OMElement receivingRdf = IOUtil.stringToXml(receivingXml);

    DataTransferObjects result = new DataTransferObjects();
    List<DataTransferObject> dtoList = result.getDataTransferObject();
    DataTransferObject dto = null;
    List<ClassObject> classObjectList = null;

    List<ClassTemplatesMap> classTemplatesMaps = graph.getClassTemplatesMaps().getClassTemplatesMap();
    List<Integer> intersectClassInstanceIndexes = new ArrayList<Integer>();
    boolean isRoot = true;

    for (ClassTemplatesMap classTemplatesMap : classTemplatesMaps)
    {
      Class clazz = classTemplatesMap.getClazz();
      List<Template> templates = classTemplatesMap.getTemplates().getTemplate();
      String classId = clazz.getClassId();
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
        classObject.setName(clazz.getName());
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

            for (Template template : templates)
            {
              List<TemplateObject> sendingTemplateObjectList = getTemplateObjectList(sendingRdf, sendingClassInstance,
                  template);
              List<TemplateObject> receivingTemplateObjectList = getTemplateObjectList(receivingRdf,
                  sendingClassInstance, template);

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

          for (Template template : templates)
          {
            List<TemplateObject> sendingTemplateObjectList = getTemplateObjectList(sendingRdf, sendingClassInstance,
                template);
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
            classObject.setName(clazz.getName());
          }
        }
      }

      isRoot = false;
    }

    return result;
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

  private List<TemplateObject> getTemplateObjectList(OMElement rdf, String classInstance, Template template)
      throws Exception
  {
    List<TemplateObject> templateObjects = new ArrayList<TemplateObject>();
    String templateId = template.getTemplateId();
    String qualTemplateId = templateId.replace("tpl:", TPL_NS);
    List<Role> roles = template.getRoles().getRole();

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

        for (Role role : roles)
        {
          RoleType roleType = role.getType();
          String roleId = role.getRoleId();
          QName qualRoleId = new QName(TPL_NS, roleId.replace("tpl:", ""));
          OMElement roleElement = owlThing.getFirstChildWithName(qualRoleId);

          RoleObject roleObject = new RoleObject();
          roleObjectList.add(roleObject);
          roleObject.setRoleId(roleId);
          roleObject.setName(role.getName());

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
          templateObject.setName(template.getName());
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
}
