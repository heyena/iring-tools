<?php //Not in use. Currently using file src/router.php
/*** get the route from the url ***/
	$route = (empty($_GET['rt'])) ? '' : $_GET['rt'];

	if (empty($route))
	{
		$route = 'index';
	}
	else
	{
		/*** get the parts of the route ***/
		$parts = explode('/', $route);
		$controller = $parts[0];
		if(isset( $parts[1]))
		{
                        //set the name of the controller class member function.
			$action = $parts[1];
		}
	}

	if (empty($controller))
	{
		$controller = 'index';
	}

	/*** Get action ***/
	if (empty($action))
	{
		$action = 'index';
	}

        /*** set the file path ***/
        $path = 'controller/'.$controller.'/';

        /*** check if path i sa directory ***/
	if (is_dir($path) == false)
	{
		throw new Exception ('Invalid controller path: `' . $path . '`');
	}

	/*** set the path ***/
 	$file = $path. $controller . '.class.php';

        /*** if the file is not there diaf ***/
	if (is_readable($file) == false)
	{
		throw new Exception ('Invalid controller class: `' . $file . '`');
	}

        require_once($file);

            $parms = array_slice($parts,2);
            /*** a new controller class instance ***/
            $controllerObj = new $controller;

        //*** check if the action is callable
        if(is_callable(array($controllerObj, $action))==true){

                $controller = $controllerObj->$action($parms);
        }

?>