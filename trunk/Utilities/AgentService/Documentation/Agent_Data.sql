USE [iRINGAgent]

DELETE FROM [SCHEDULECACHE]
INSERT INTO [iRINGAgent].[dbo].[SCHEDULECACHE] ([TASK_NAME],[PROJECT],[APP],[CACHE_PAGE_SIZE],[SSO_URL],[CLIENT_ID],[CLIENT_SECRET],[GRANT_TYPE],[APP_KEY],[ACCESS_TOKEN],[REQUEST_TIMEOUT],[START_TIME],[END_TIME],[Created_Date],[Created_By],[Occurance],[NextStart_Date_Time],[End_Date_Time],[STATUS],[ACTIVE] ) 
VALUES('scope.App','Scope','App',2000,'https://sso.mypsn.com/as/token.oauth2','iRingTools','JxX6VHu8uZYff7zPUkTNw7sdDodvF3G3','client_credentials','wHKxvUyEqrLTNSvsVTPX1GJs02nAo5IF','TmMopozebXnR8ky6YgRnAV22ICOz',300000,'08/07/2014 06:00:00','','08/07/2014 07:00:00','Sujan Gorkhali','Daily','','10/16/2014 06:00:00','Ready',1)	


DELETE FROM [SCHEDULEExchange]
INSERT INTO [iRINGAgent].[dbo].[SCHEDULEExchange] 
	([Task_Name],[Scope],[Base_Url],[Exchange_Id],[Sso_Url],[Client_Id],[Client_Secret],[Grant_Type],[Request_Timeout],[Start_Time],[End_Time],[Created_Date],[Created_By],[Occurance],[NextStart_Date_Time],[End_Date_Time],[Status],[Active] ) 
	VALUES('Task_Name','ExchangeScope','ExchangeServerUrl/runUnattendedExchange','ExchangeID','https://sso.mypsn.com/as/token.oauth2','iRingTools','JxX6VHu8uZYff7zPUkTNw7sdDodvF3G3','client_credentials',300000,'08/07/2014 06:00:00','','08/07/2014 07:00:00','Sujan Gorkhali','Daily','','10/16/2015 06:00:00','Ready',1)

<<<<<<< .mine

INSERT INTO [iRINGAgent].[dbo].[SCHEDULEExchange] 
([Task_Name],[Scope],[Base_Url],[Exchange_Id],[Sso_Url],[Client_Id],[Client_Secret],[Grant_Type],[Request_Timeout],[Start_Time],[End_Time],[Created_Date],[Created_By],[Occurance],[NextStart_Date_Time],[End_Date_Time],[Status],[Active] ) 
VALUES('Equipment: eMRGen->BPSTestX','WLNG','http://localhost:8087/iringtools-apps/xchmgr','1','https://sso.mypsn.com/as/token.oauth2','iRingTools','0Lvnvat5T5OJk5n6VwD4optFJoq7/0POq++NfYkIgHYtmy6Pluix3aGy7EAN1Jxp','client_credentials',300000,'08/07/2014 06:00:00','','08/07/2014 07:00:00','Sujan Gorkhali','Daily','','10/16/2015 06:00:00','Ready',1)


=======
INSERT INTO [iRINGAgent].[dbo].[SCHEDULEExchange] 
	([Task_Name],[Scope],[Base_Url],[Exchange_Id],[Sso_Url],[Client_Id],[Client_Secret],[Grant_Type],[Request_Timeout],[Start_Time],[End_Time],[Created_Date],[Created_By],[Occurance],[NextStart_Date_Time],[End_Date_Time],[Status],[Active] ) 
	VALUES('Sender->Receiver','AgentServices','http://localhost:8087/iringtools-apps/runUnattendedExchange','1','https://sso.mypsn.com/as/token.oauth2','iRingTools','JxX6VHu8uZYff7zPUkTNw7sdDodvF3G3','client_credentials',300000,'08/07/2014 06:00:00','','08/07/2014 07:00:00','Sujan Gorkhali','Daily','','10/16/2015 06:00:00','Ready',1)

>>>>>>> .r7335
