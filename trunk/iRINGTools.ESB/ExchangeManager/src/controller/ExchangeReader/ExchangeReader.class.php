<?php
/**
 * @author Aswini Nayak (aknayak@bechtel.com)
*/

function useModel($className,$parameter=null)
{
	require_once('/model/'.$className.'/'.$className.'Model.class.php');
	$modelName = $className.'Model';
		return new $modelName($parameter);
}

class ExchangeReader{
	private $modelObj;

	function __construct($projectId){
		$this->modelObj = useModel(get_class($this),$projectId);
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