<?php
/**
 * @author Aswini Nayak (aknayak@bechtel.com)
*/
   $path = CONTROLLER_DIR."BaseController.php";
    require_once($path);

class ExchangeReader extends BaseController{
	private $modelObj;

	function __construct(){
		$this->modelObj = $this->useModel(get_class($this));
	}

	function exchangeXML($projectId=null)
	{
		if(!empty($projectId)){

			$this->modelObj->readXML();
		}
	}

	function exchangeList($params)
	{
            	if(is_array($params)){
                    // it can be used in View
			echo $this->modelObj->getDirectoryTreeJSONData();
		}
	}
}
?>