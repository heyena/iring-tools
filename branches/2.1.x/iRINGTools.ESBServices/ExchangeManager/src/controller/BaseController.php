<?php
class BaseController{
	protected function useModel($className,$parameter=null)
	{
		require_once('model/'.$className.'/'.$className.'Model.class.php');
		$modelName = $className.'Model';
		return new $modelName($parameter);
	}

	protected function parseInt($string) {
		//	return intval($string);
		if(preg_match('/(\d+)/', $string, $array)) {
			return $array[1];
		} else {
			return 0;
		}
	}

}
?>