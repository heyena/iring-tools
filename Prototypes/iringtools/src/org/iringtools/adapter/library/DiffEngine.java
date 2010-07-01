package org.iringtools.adapter.library;

import java.util.List;
import java.util.ArrayList;
import org.iringtools.adapter.library.dto.ClassObject;
import org.iringtools.adapter.library.dto.ClassObject.TemplateObjects;
import org.iringtools.adapter.library.dto.DataTransferObject;
import org.iringtools.adapter.library.dto.DataTransferObject.ClassObjects;
import org.iringtools.adapter.library.dto.DataTransferObjects;
import org.iringtools.adapter.library.dto.RoleObject;
import org.iringtools.adapter.library.dto.TemplateObject;
import org.iringtools.adapter.library.dto.TemplateObject.RoleObjects;
import org.iringtools.adapter.library.manifest.GraphMap;
import org.apache.axiom.om.OMElement;

public class DiffEngine
{
  private GraphMap graphMap;
  private OMElement sendingRdf;
  private OMElement receivingRdf;
  
  public DiffEngine(GraphMap graphMap)
  {
    this.graphMap = graphMap;
  }
  
  private boolean validate()
  {
    return true;    
  }
  
  public DataTransferObjects diff(OMElement sendingRdf, OMElement receivingRdf)
  {
    this.sendingRdf = sendingRdf;
    this.receivingRdf = receivingRdf;
    
    DataTransferObjects result = new DataTransferObjects();
    
    if (validate())
    {
      /*List<DataTransferObject> dtoList = new ArrayList<DataTransferObject>();
      result.setDataTransferObject(dtoList);
      DataTransferObject dto = new DataTransferObject();
      dtoList.add(dto);
      ClassObjects classObjects = new ClassObjects();
      dto.setClassObjects(classObjects);
      List<ClassObject> classObjectList = new ArrayList<ClassObject>();
      classObjects.setClassObject(classObjectList);
      dto.setClassObjects(classObjects);    
      ClassObject classObject = new ClassObject();    
      classObjectList.add(classObject);
      classObject.setName("PIPING NETWORK SYSTEM");
      classObject.setClassId("rdl:R19192462550");
      classObject.setIdentifier("Line-1");
      TemplateObjects templateObjects = new TemplateObjects();
      classObject.setTemplateObjects(templateObjects);
      List<TemplateObject> templateObjectList = new ArrayList<TemplateObject>();
      templateObjects.setTemplateObject(templateObjectList);
      TemplateObject templateObject = new TemplateObject();
      templateObjectList.add(templateObject);
      templateObject.setName("IdentificationByTag");
      templateObject.setTemplateId("tpl:R66921101783");
      RoleObjects roleObjects = new RoleObjects();
      templateObject.setRoleObjects(roleObjects);
      List<RoleObject> roleObjectList = new ArrayList<RoleObject>();
      roleObjects.setRoleObject(roleObjectList);
      RoleObject roleObject1 = new RoleObject();
      roleObjectList.add(roleObject1);
      roleObject1.setName("hasObject");
      roleObject1.setRoleId("tpl:R44537504070");
      RoleObject roleObject2 = new RoleObject();
      roleObjectList.add(roleObject2);
      roleObject2.setName("valIdentifier");
      roleObject2.setRoleId("tpl:R22674749688");
      roleObject2.setValue("Tag-1");*/
    }
    
    return result;
  }
}
