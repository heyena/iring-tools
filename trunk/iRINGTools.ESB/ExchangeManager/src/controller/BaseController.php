<?php
class BaseController{
	protected function useModel($className,$parameter=null)
	{
		require_once('/model/'.$className.'/'.$className.'Model.class.php');
		$modelName = $className.'Model';
		return new $modelName($parameter);
	}
	protected $modelObj;
}
?>