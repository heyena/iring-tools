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
	// collect request parameters
	//$start = $this->parseInt($_POST['start']);
	//$count = $this->parseInt($_POST['limit']);
	$start  = isset($_REQUEST['start'])  ? $this->parseInt($_REQUEST['start'])  :  0;
	$count  = isset($_REQUEST['limit'])  ? $this->parseInt($_REQUEST['limit'])  : PAGESIZE;
	$sort   = isset($_REQUEST['sort'])   ? $_REQUEST['sort']   : '';
	$dir    = isset($_REQUEST['dir'])    ? $_REQUEST['dir']    : 'ASC';
	$filters = isset($_REQUEST['filter']) ? $_REQUEST['filter'] : null;

	$identifier = $_POST['identifier'];
	$refClassIdentifier= $_POST['refClassIdentifier'];
	if($params[0]=='exchanges'){
	unset($params[3]);
	unset($params[4]);
	}
	if(isset($start) && (isset($count))){
		$urlParams = $this->urlParameters($params);
		$responseString = $this->modelObj->getPageData($urlParams,$start,$count,$identifier,$refClassIdentifier,$filters);
		echo $responseString;
	}
    }


    function getHistory($params){
        $urlParams = $this->urlParameters($params);
        $urlParams['exchangeAction']='viewHistory';

        /*echo '<pre>';
        print_r($urlParams);
        exit;*/
        $historyArray = $this->modelObj->getHistory($urlParams);

        echo ($historyArray);
    }

}
   
?>