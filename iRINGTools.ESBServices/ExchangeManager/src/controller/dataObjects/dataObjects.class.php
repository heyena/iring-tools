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
	
}
   
?>