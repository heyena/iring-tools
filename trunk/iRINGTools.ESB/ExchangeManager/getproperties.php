<?php
$ritu=array('Property'=>'Value','Property1'=>'Value1','Property2'=>'Value2',);
echo json_encode(array("props"=>array($ritu)));
exit;

echo '{"props":[
    {
        "First name":"Aswini",	
        "Last name":"Nayak",
        "E-mail":"aknayak@bechtel.com"
    }
]}';
?>