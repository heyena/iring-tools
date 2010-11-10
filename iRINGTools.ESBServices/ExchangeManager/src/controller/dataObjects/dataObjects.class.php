<?php
    require_once(CONTROLLER_DIR."BaseController.php");
	/** Controller with capabilities :
	to generate the tree for Review & Acceptance
	to send the data of Review & Acceptance for Exchange
	to get the status
	Will use DXI & DXO Controller
	Function getReview() @params exchangeID
    @returns the JSON for Tree view + default grid view + array to be used in nodes
	*/
   class dataObjects extends BaseController{
	private $defaultGrid = array();
	private $treeData;
	private $keyparams=array('nodetype','scope','exchangeID');
	//private $keyparams=array();
	protected $modelObj;

	/**
	instantiate the model object 
	*/
	function __construct(){
		$this->modelObj = $this->useModel(get_class($this));
	}

	// This function will be called in AJAX Request like
	function getDataObjects($params){
		$urlParams = $this->urlParameters($params);
		$headerArray = $this->modelObj->getDataObjects($urlParams);
		echo ($headerArray);
	}

	function deleteDataObjects($params){
		$urlParams = $this->urlParameters($params);
		$headerArray = $this->modelObj->deleteDataObjects($urlParams);
		echo ($headerArray);
	}

	function deleteGraphObjects($params){
		$urlParams = $this->urlParameters($params);
		$headerArray = $this->modelObj->deleteDataObjects($urlParams);
		echo ($headerArray);
	}

	private function urlParameters($params){
		switch($params[0]){
			case "exchanges":
				$urlParamsArray = array_combine($this->keyparams,$params);
				break;
			case "graph":
				unset($this->keyparams[array_search('exchangeID',$this->keyparams)]);
				$this->keyparams[]="applname";
				$this->keyparams[]="graphs";
				$urlParamsArray = array_combine($this->keyparams,$params);
				break;
		}
		return $urlParamsArray;
	}

	// This function will show Graph Grid on central Panel
	function getGraphObjects($params){
		$urlParams = $this->urlParameters($params);
		$headerArray = $this->modelObj->getDataObjects($urlParams);
		echo ($headerArray);
	}

	function setDataObjects($params){
		$urlParams = $this->urlParameters($params);
		$urlParams['hasreviewed']=$_POST['hasreviewed'];

		/*echo '<pre>';
		print_r($urlParams);
		exit;*/
		$headerArray = $this->modelObj->setDataObjects($urlParams);
		echo ($headerArray);
	}


        /** This function used to show the Related Class Items Grid
         *
	 * @param FIVE [nodetype(exchanges),scope,exchangeID,dtoIdentifier,referenceClassIdentifier)]
	 * @returns
	 * @access public
	 */
        function getRelatedDataObjects($params)
	{
            	if(is_array($params)){

                    $dtoIdentifier = $params[3];
                    $referenceClassIdentifier = $params[4];
                    unset($params[3]);
                    unset($params[4]);
                    $urlParams = $this->urlParameters($params);
                    $urlParams['dtoIdentifier']=$dtoIdentifier;
                    $urlParams['referenceClassIdentifier']=$referenceClassIdentifier;       // RelatedClass identifier value
                    echo $this->modelObj->getRelatedDataObjects($urlParams);

                        // it can be used in View
                            //echo $this->modelObj->getRelatedItemsGridJSONData($identifier,$reference);
		}
	}
function getPageData($params){
	$start = $this->parseInt($_POST['start']);
	$limit = $this->parseInt($_POST['limit']);

	$identifier = $_POST['identifier'];
	$refClassIdentifier= $_POST['refClassIdentifier'];
	unset($params[3]);
	unset($params[4]);
	
	//echo '<pre>';
	//print_r($params);

	if(isset($start) && (isset($limit))){
		$urlParams = $this->urlParameters($params);
		// calculate Pageno
		//$pageNo = ceil($start/$limit)+1;
		$responseString = $this->modelObj->getPageData($urlParams,$start,$limit,$identifier,$refClassIdentifier);
		echo $responseString;
	}
}

}
   
?>