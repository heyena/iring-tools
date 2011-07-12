package org.iringtools.ext;

import org.iringtools.common.response.Response;
import org.iringtools.common.response.Status;


import org.iringtools.common.response.StatusList;
import org.iringtools.library.StatusLevel;



public class ResponseExtension extends Response {

    protected StatusLevel statuslevel;
	
	public void append(Response response)
	{
    	
	  StatusList statuslist = response.getStatusList();

	  for(Status status : statuslist.getItems()){
		  append(status);
	  }
	}
    
	 public void append(Status status)
		{
		  StatusExtension foundStatus = null;
		  boolean wasFound = false;
		  
		  StatusList statuslist = new StatusList();
		  
		  for (Status candidateStatus : statuslist.getItems())
		  {
			if (status.getIdentifier().equalsIgnoreCase(candidateStatus.getIdentifier()))
			{
			  foundStatus = (StatusExtension) candidateStatus;
			  wasFound = true;
			}
		  }

		  if (!wasFound)
		  {
			statuslist.getItems().add(status);
		  }
		  else
		  {
			if (foundStatus.getLevel().ordinal() < ((StatusExtension) status).getLevel().ordinal())
			{
			  foundStatus.setLevel(((StatusExtension) status).getLevel());
			}

			for (String message : status.getMessages().getItems())
	        {
	          foundStatus.getMessages().getItems().add(message);
	        }
		  }

		  if (this.statuslevel.ordinal() < ((StatusExtension) status).getLevel().ordinal())
		  {
			  this.setLevel(((StatusExtension) status).getLevel());
		  }
		}

	 public void setLevel(StatusLevel levelValue) {
	 		this.statuslevel = levelValue;
	 	}
    
	 
	 
}
