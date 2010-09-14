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

	function __construct(){
		$this->modelObj = useModel(get_class($this));
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