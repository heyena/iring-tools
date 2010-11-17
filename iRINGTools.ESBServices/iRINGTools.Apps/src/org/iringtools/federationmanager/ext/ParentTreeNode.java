package org.iringtools.federationmanager.ext;

import java.util.List;

public class ParentTreeNode extends TreeNode
{
	private List<TreeNode> children;

	public List<TreeNode> getChildren() {
		return children;
	}

	public void setChildren(List<TreeNode> children) {
		this.children = children;
	}
}
