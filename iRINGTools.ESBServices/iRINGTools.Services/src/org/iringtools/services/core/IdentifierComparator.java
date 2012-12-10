package org.iringtools.services.core;

import java.util.Comparator;

import org.apache.log4j.Logger;
import org.iringtools.dxfr.dti.DataTransferIndex;

public class IdentifierComparator implements Comparator<DataTransferIndex>
{
  private static final Logger logger = Logger.getLogger(IdentifierComparator.class);
  
  @Override
  public int compare(DataTransferIndex leftDti, DataTransferIndex rightDti)
  {  
    if (leftDti.getIdentifier() == null && rightDti.getIdentifier() == null)
    {
      logger.debug("Left & Right DTI identifiers are null.");
      return 0;
    }
    
    if (leftDti.getIdentifier() == null)
    {
      logger.debug("Left DTI identifier is null.");
      logger.debug("Right DTI [" + rightDti.getIdentifier() + "(" + rightDti.getInternalIdentifier() + ")");
      
      return -1;
    }
        
    if (rightDti.getIdentifier() == null)
    {
      logger.debug("Left DTI [" + leftDti.getIdentifier() + "(" + leftDti.getInternalIdentifier() + ")");   
      logger.debug("Right DTI identifier is null.");     
      return 1;
    }
    
    return leftDti.getIdentifier().toLowerCase().compareTo(rightDti.getIdentifier().toLowerCase());
  }
}
