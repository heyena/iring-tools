package org.iringtools.controllers;

import javax.servlet.http.HttpServletRequest;

import org.iringtools.models.FederationModel;
import org.iringtools.models.Result;
import org.iringtools.refdata.federation.RepositoryType;
import org.iringtools.widgets.tree.Tree;
import com.opensymphony.xwork2.Action;
import org.apache.struts2.interceptor.ServletRequestAware;

public class FederationController extends AbstractController implements ServletRequestAware
{
  private static final long serialVersionUID = 1L;

  private Tree tree;
  private HttpServletRequest httpRequest = null;
  private Result result = new Result();
  private String[] repositoryTypes;
  private String nodeId;
  private String parentNodeID;

  public FederationController() throws Exception
  {
    super();
    authorize("federationAdmins");
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

  public String[] getRepositorytypes()
  {
    return repositoryTypes;
  }

  public void setRepositorytypes(String[] repositoryTypes)
  {
    this.repositoryTypes = repositoryTypes;
  }

  public String getNodeId()
  {
    return nodeId;
  }

  public void setNodeId(String nodeId)
  {
    this.nodeId = nodeId;
  }

  public String getParentNodeID()
  {
    return parentNodeID;
  }

  public void setParentNodeID(String parentNodeID)
  {
    this.parentNodeID = parentNodeID;
  }

  public String getFederation()
  {
    FederationModel federation = new FederationModel(settings);
    federation.populate();
    tree = federation.toTree();
    return Action.SUCCESS;
  }

  public String postFederation()
  {
    System.out.println("Reaching post Federation");
    FederationModel federation = new FederationModel(settings);
    boolean successStatus = federation.readTree(httpRequest);
    result.setSuccess(successStatus);
    // result.setMessage("Details Successfully saved!");
    // federation.save();
    return Action.SUCCESS;
  }

  public String deleteNode()
  {
    System.out.println("Reaching deleteNode");
    FederationModel federation = new FederationModel(settings);
    boolean successStatus = federation.deleteNode(nodeId, parentNodeID);
    System.out.println("deleteNode executed :" + successStatus);
    result.setSuccess(successStatus);
    return Action.SUCCESS;
  }

  public String getRepoTypes()
  {
    RepositoryType[] repoTypes = RepositoryType.values();
    String returnArray[] = new String[(repoTypes.length)];
    int i = 0;
    for (RepositoryType repoType : repoTypes)
    {
      returnArray[i] = repoType.value();
      i++;
    }
    repositoryTypes = returnArray;
    return Action.SUCCESS;
  }
}
