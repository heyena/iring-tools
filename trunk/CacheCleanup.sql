BEGIN TRY

	
		
	
	     
		DECLARE @name NVARCHAR(2000)
		DECLARE @tableId NVARCHAR(2000)  
		DECLARE @tablename NVARCHAR(2000)  
		DECLARE @Query nvarchar(2000) 
		SET @tableId =''
		SET @Query =''
		SET @tablename ='' 
			IF OBJECT_ID('tempdb..#temp', 'U') IS NOT NULL DROP TABLE tempdb..#temp 
			IF OBJECT_ID('tempdb..#temp2', 'U') IS NOT NULL DROP TABLE tempdb..#temp2 

				SELECT * into #temp FROM ( 
				SELECT CASE WHEN CHARINDEX('_', Data)<>0 THEN substring(Data, 1,(CHARINDEX('_', Data)-1))END AS Data
				FROM(SELECT name AS Data FROM sys.tables  )cache )a

				--Deleting unused table id stored in cache tables.
					
				SELECT * INTO #temp2 FROM Caches t1 
				WHERE NOT EXISTS (
					SELECT * 
					FROM #temp t2 
					WHERE t1.CacheId = t2.Data)
				
				DELETE FROM Caches WHERE CacheId IN (SELECT CacheId FROM #temp2) 	
				
				--deleting unused tables

				DECLARE db_cursor CURSOR FOR  
				SELECT name FROM sys.tables WHERE name NOT IN ('master','model','msdb','tempdb')  
				SET @tableId =''
				SET @Query =''
				SET @tablename =''
				OPEN db_cursor   
				FETCH NEXT FROM db_cursor INTO @name   

				WHILE @@FETCH_STATUS = 0   
				BEGIN   
				-- Print 
					
					set @tableId =  substring(@name, 1,(CHARINDEX('_', @name)-1))
					
					SELECT @tablename = CacheId FROM Caches WHERE CacheId = @tableId
					
					IF LEN(@tablename) = 0 OR @tablename =''
					BEGIN
						select LEN(@tablename)
						SET @Query = 'DROP TABLE ' + @name
						EXEC (@Query)
						print @Query
						
					END

					SET @tableId =''
					SET @Query =''
					SET @tablename =''
					FETCH NEXT FROM db_cursor INTO @name   
				END   
				
				CLOSE db_cursor   
				DEALLOCATE db_cursor
END TRY
BEGIN CATCH
  
	CLOSE db_cursor   
	DEALLOCATE db_cursor
END CATCH






  
    
  


   
  
   
   
   
    
    

