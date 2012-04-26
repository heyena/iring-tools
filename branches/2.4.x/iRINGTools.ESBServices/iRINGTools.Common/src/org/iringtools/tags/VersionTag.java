package org.iringtools.tags;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.io.IOException;

import javax.servlet.ServletContext;
import javax.servlet.jsp.JspException;
import javax.servlet.jsp.tagext.TagSupport;

import org.apache.log4j.Logger;

public class VersionTag extends TagSupport 
{
  private static final Logger logger = Logger.getLogger(VersionTag.class);
  
	private static final long serialVersionUID = 1L;
	private static final String versionFileName = "version";

	public int doStartTag() throws JspException 
	{
		ServletContext context = pageContext.getServletContext();
		File file = new File(context.getRealPath("/") + versionFileName);
		
		if (file.exists())
		{
		  try
		  {
		    BufferedReader reader = new BufferedReader(new FileReader(file));
		    String version = reader.readLine();
		    reader.close();
		    
		    pageContext.getOut().write(version);
		  }
		  catch (IOException e)
		  {
		    logger.error("Error getting version: " + e);
		  }
		}

		return SKIP_BODY;
	}

	public int doEndTag() 
	{
		return EVAL_PAGE;
	}
}
