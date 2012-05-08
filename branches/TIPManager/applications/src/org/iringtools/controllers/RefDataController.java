package org.iringtools.controllers;

import javax.servlet.http.HttpServletRequest;

import org.apache.struts2.interceptor.ServletRequestAware;
import org.ids_adi.ns.qxf.model.Qmxf;
import org.iringtools.models.RefDataModel;
import org.iringtools.models.Result;
import org.iringtools.refdata.response.Response;
import org.iringtools.utility.XmlTypeCode;
import org.iringtools.widgets.tree.Tree;
import org.iringtools.widgets.tree.Type;

import com.opensymphony.xwork2.Action;

public class RefDataController extends AbstractController implements ServletRequestAware
{
  private static final long serialVersionUID = 1L;

  private Tree tree;
  private Response response;
  private Result result = new Result();
  private Qmxf qmxf;
  private XmlTypeCode xmlDataType[];

  
  public Qmxf getQmxf()
  {
    return qmxf;
  }

  public void setQmxf(Qmxf qmxf)
  {
    this.qmxf = qmxf;
  }

  public Response getResponse()
  {
    return response;
  }

  public void setResponse(Response response)
  {
    this.response = response;
  }

  private HttpServletRequest httpRequest = null;

  public RefDataController()
  {
    super();
  }

  public void setTree(Tree tree)
  {
    this.tree = tree;
  }

  public Tree getTree()
  {
    return tree;
  }

  public Result getResult()
  {
    return result;
  }

  public void setResult(Result result)
  {
    this.result = result;
  }

  public void setServletRequest(HttpServletRequest request)
  {
    this.httpRequest = request;
  }
  	public XmlTypeCode[] getXmlDataType() {
		return xmlDataType;
	}

	public void setXmlDataType(XmlTypeCode[] xmlDataType) {
		this.xmlDataType = xmlDataType;
	}
  

  public String searchPage()
  {

    RefDataModel refdata = new RefDataModel(session);
    Type type = Type.fromValue(httpRequest.getParameter("type"));

    switch (type)
    {
    case SEARCH:
      tree = refdata.populate(httpRequest);
      break;
    case CLASS:
      tree = refdata.getClass(httpRequest.getParameter("id"));
      break;
    case CLASSIFICATION:
      tree = refdata.getClass(httpRequest.getParameter("id"));
      break;
    case MEMBERS:
      tree = refdata.getMembers(httpRequest.getParameter("id"));
    case SUPERCLASS:
      tree = refdata.getSubSuperClasses(httpRequest.getParameter("id"), "Super");
      break;
    case SUBCLASS:
      tree = refdata.getSubSuperClasses(httpRequest.getParameter("id"), "Sub");
      break;
    case CLASSTEMPLATE:
      tree = refdata.getTemplates(httpRequest.getParameter("id"));
      break;
    case TEMPLATENODE:
      tree = refdata.getRole(httpRequest.getParameter("id"));
      break;
    }

    return Action.SUCCESS;
  }

  public String getTemplates()
  {
    RefDataModel refdata = new RefDataModel(session);
    String id = httpRequest.getParameter("id");
    tree = refdata.getTemplates(id);
    return Action.SUCCESS;
  }

  public String postClass()
  {
    RefDataModel refdata = new RefDataModel(session); 
    boolean successStatus = refdata.postClass(httpRequest);
    result.setSuccess(successStatus);
    return Action.SUCCESS;
  }

  public String postTemplate()
  {
    RefDataModel refdata = new RefDataModel(session);
    boolean successStatus = refdata.postTemplate(httpRequest);
    result.setSuccess(successStatus);
    return Action.SUCCESS;
  }
  public String getDataType(){
		System.out.println("Reaching getDataType Class");
		try{
		XmlTypeCode.TypeCode typeCode[] = XmlTypeCode.TypeCode.values();
		xmlDataType = new XmlTypeCode[typeCode.length];
		for(int i=0;i<typeCode.length;i++){
			xmlDataType[i] = new XmlTypeCode();
			/*xmlDataType[i].setId(i);*/
			xmlDataType[i].setE(typeCode[i]);
		}
		}catch(Exception e){
			e.printStackTrace();
		}
		return Action.SUCCESS;
	}
}
