<?php
/**
 * @author Aswini Nayak (aknayak@bechtel.com)
*/
   require_once('/controller/BaseController.php');   

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

	function exchnageList($params)
	{
            	if(is_array($params)){
                    // it can be used in View
			echo $this->modelObj->getDirectoryTreeJSONData();
		}
	}
}
?>