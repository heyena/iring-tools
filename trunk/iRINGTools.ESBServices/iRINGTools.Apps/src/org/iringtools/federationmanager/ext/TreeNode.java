package org.iringtools.federationmanager.ext;

import java.util.ArrayList;
import java.util.List;

import org.iringtools.refdata.federation.IDGenerator;
import org.iringtools.refdata.federation.Namespace;
import org.iringtools.refdata.federation.Repository;

import com.sun.istack.internal.Nullable;


public class TreeNode
{
	private String id;
	private String text;
	private String icon;
	private Boolean leaf;
	private Boolean expanded;
	
	//Common Properties
	private String name;
	private String URI;
	private String description;
	
	//Repository Details
	
	private String updateUri;
	private String repositoryType;
	private Boolean readOnly=null;
	
	//ID generator Details
	private Integer idNum=null;
	
	//Namespace Details
	private String alias;
	private Boolean writeable=null;
	private String idGenerator;
	
	
	public String getId() {
		return id;
	}
	public void setId(String id) {
		this.id = id;
	}
	public String getText() {
		return text;
	}
	public void setText(String text) {
		this.text = text;
	}
	public String getIcon() {
		return icon;
	}
	public void setIcon(String icon) {
		this.icon = icon;
	}
	public Boolean getLeaf() {
		return leaf;
	}
	public void setLeaf(Boolean leaf) {
		this.leaf = leaf;
	}
	public Boolean getExpanded() {
		return expanded;
	}
	public void setExpanded(Boolean expanded) {
		this.expanded = expanded;
	}
	public String getName() {
		return name;
	}
	public void setName(String name) {
		this.name = name;
	}
	public String getURI() {
		return URI;
	}
	public void setURI(String uRI) {
		URI = uRI;
	}
	public String getDescription() {
		return description;
	}
	public void setDescription(String description) {
		this.description = description;
	}
	public String getUpdateUri() {
		return updateUri;
	}
	public void setUpdateUri(String updateUri) {
		this.updateUri = updateUri;
	}
	public String getRepositoryType() {
		return repositoryType;
	}
	public void setRepositoryType(String repositoryType) {
		this.repositoryType = repositoryType;
	}
	public Boolean isReadOnly() {
		return readOnly;
	}
	public void setReadOnly(Boolean readOnly) {
		this.readOnly = readOnly;
	}
	public Integer getIdNum() {
		return idNum;
	}
	public void setIdNum(Integer idNum) {
		this.idNum = idNum;
	}
	public String getAlias() {
		return alias;
	}
	public void setAlias(String alias) {
		this.alias = alias;
	}
	public Boolean isWriteable() {
		return writeable;
	}
	public void setWriteable(Boolean writeable) {
		this.writeable = writeable;
	}
	public String getIdGenerator() {
		return idGenerator;
	}
	
	public void setIdGenerator(String idGenerator) {
		this.idGenerator = idGenerator;
	}
	
	
	public TreeNode setRepositoryDetails(Repository repository){
		TreeNode node = new TreeNode();
		node.setId(repository.getName());
		node.setText(repository.getName());
		node.setIcon("");
		node.setLeaf(true);
		
		node.setURI(repository.getUri());
		node.setDescription(repository.getDescription());
		node.setReadOnly(repository.isIsReadOnly());
		node.setRepositoryType(repository.getRepositoryType());
		node.setUpdateUri(repository.getUpdateUri());
		
		return node;
	}
	
	public TreeNode setIdGenDetails(IDGenerator idgenerator){
		TreeNode node = new TreeNode();
		node.setId(Integer.toString(idgenerator.getId()));
		node.setText(idgenerator.getName());
		node.setIcon("");
		node.setLeaf(true);
		
		node.setName(idgenerator.getName());
		node.setURI(idgenerator.getUri());
		node.setDescription(idgenerator.getDescription());
		return node;
	}
	
	public TreeNode setNameSpaceDet(Namespace namespace){
		TreeNode node = new TreeNode();
		node.setId(namespace.getAlias());
		node.setText(namespace.getAlias());
		node.setIcon("");
		node.setLeaf(true);
		
		node.setAlias(namespace.getAlias());
		node.setURI(namespace.getUri());
		node.setDescription(namespace.getDescription());
		node.setWriteable(namespace.isIsWritable());
		node.setIdNum(namespace.getIdGenerator());
		
		return node;
	}
}
