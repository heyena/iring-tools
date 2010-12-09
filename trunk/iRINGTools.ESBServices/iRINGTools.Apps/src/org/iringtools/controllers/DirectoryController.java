package org.iringtools.controllers;

import java.io.IOException;

import javax.xml.bind.JAXBException;

import org.iringtools.models.DirectoryModel;
import org.iringtools.ui.widgets.tree.Tree;
import com.opensymphony.xwork2.Action;

public class DirectoryController {
	
	private DirectoryModel directory;
	private Tree tree;
	
	public DirectoryController()
	{
		directory = new DirectoryModel();
	}

	public void setTree(Tree tree) {
		this.tree = tree;
	}

	public Tree getTree() {
		return tree;
	}

	public String getDirectory() {
		directory.populate();
		tree = directory.toTree();
        return Action.SUCCESS;
	}
	
	public String postDirectory() throws IOException, JAXBException {		
		directory.readTree(tree);
		directory.save();
        return Action.SUCCESS;
	}
}
