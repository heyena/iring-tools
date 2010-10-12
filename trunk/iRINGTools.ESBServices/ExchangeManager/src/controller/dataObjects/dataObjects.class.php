<?php
    require_once("./controller/BaseController.php");
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
	// http://localhost:81/iRINGTools.ESB/ExchangeManager/ReviewGenerator/getReview/1
	
	function getDataObjects($params){
		switch($params[0]){
			case "exchanges":
				$urlParams = array_combine($this->keyparams,$params);
				break;
			case "graph":
				unset($this->keyparams[array_search('exchangeID',$this->keyparams)]);
				$this->keyparams[]="applname";
				$this->keyparams[]="graphs";
				$urlParams = array_combine($this->keyparams,$params);
				break;
		}
		$headerArray = $this->modelObj->getDataObjects($urlParams);
		echo ($headerArray);
		
	}

	function setReview($params){
	}
}
   
?>