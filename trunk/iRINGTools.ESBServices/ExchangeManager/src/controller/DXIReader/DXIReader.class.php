<?php
/**
 * @author Aswini Nayak (aknayak@bechtel.com)
 * @1st September 2010
*/
   require_once('/controller/BaseController.php');      
class DXIReader extends BaseController{
	protected $modelObj;
	private $paramsArray;
	private $keyparams=array('exchangeID');

	function getDxiDetails($params){
		echo $this->modelObj->getDXIInfo(array_combine($this->keyparams,$params));
	}
	function __construct(){
		$this->modelObj = $this->useModel(get_class($this));
	}

	function exchangeXML($projectId=null)
	{
		if(!empty($projectId)){

			$this->modelObj->readXML();
		}
	}

	function exchnageList($projectId=null)
	{
		if(!empty($projectId)){
			$this->modelObj->convertXMLtoJSON();
		}
	}
}
?>