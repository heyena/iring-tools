
USE [MASTER]
GO

IF EXISTS(SELECT name FROM sys.databases WHERE name = 'TIPS')
	DROP DATABASE [TIPS]
GO

CREATE DATABASE [TIPS] 
GO

USE [TIPS]
GO

CREATE TABLE [dbo].[ValueList](
	[VLID] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
  PRIMARY KEY ([VLID])
)
GO

CREATE TABLE [dbo].[ValueMap](
	[VMID] [int] NOT NULL,
	[URI] [nvarchar](255) NOT NULL,
	[Value] [nvarchar](max) NOT NULL,
	[VLID] [int] NOT NULL,
  PRIMARY KEY ([VMID]),
  FOREIGN KEY ([VLID]) REFERENCES [dbo].[ValueList](VLID)
)
GO

CREATE TABLE [dbo].[GraphMap](
	[GMID] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[DataObjectName] [nvarchar](50) NOT NULL,
	[TypicalValues] [nvarchar](50) NOT NULL,
  PRIMARY KEY ([GMID])
)
GO

CREATE TABLE [dbo].[ClassMap](
	[CID] [int] NOT NULL,
	[RID] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Identifier] [nvarchar](50) NOT NULL,
	[Delimiter] [nvarchar](10) NULL,
	[Value] [nvarchar](255) NULL,
	[GMID] [int] NOT NULL,
  PRIMARY KEY ([CID]),
  FOREIGN KEY ([GMID]) REFERENCES [dbo].[GraphMap](GMID)
)
GO

CREATE TABLE [dbo].[TemplateMap](
	[TMID] [int] NOT NULL,
	[RID] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
	[CID] [int] NOT NULL,
  PRIMARY KEY ([TMID]),
  FOREIGN KEY ([CID]) REFERENCES [dbo].[ClassMap](CID)
)
GO

CREATE TABLE [dbo].[RoleMap](
	[RMID] [int] NOT NULL,
	[RID] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
	[Property] [nvarchar](50) NULL,
	[DataType] [nvarchar](50) NULL,
	[DataLength] [int] NULL,
	[NumOfDecimals] [int] NULL,
	[Value] [nvarchar](max) NULL,
	[TMID] [int] NOT NULL,
	[VLID] [int] NULL,
	[CID] [int] NULL,
  PRIMARY KEY ([RMID]),
  FOREIGN KEY ([TMID]) REFERENCES [dbo].[TemplateMap](TMID),
  FOREIGN KEY ([VLID]) REFERENCES [dbo].[ValueList](VLID)
)
GO

CREATE TABLE [dbo].[CommodityList](
	[CMID] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
PRIMARY KEY ([CMID])
)
GO
CREATE TABLE [dbo].[CommodityMap](
	[CMPID] [int] NOT NULL,
	[GMID] [int] NOT NULL,
	[CMID] [int] NOT NULL,
	FOREIGN KEY ([CMID]) REFERENCES [dbo].[CommodityList](CMID),
	FOREIGN KEY ([GMID]) REFERENCES [dbo].[GraphMap](GMID),
  PRIMARY KEY ([CMPID])
)
GO


insert into CommodityList ( CMID, Name) values (1, 'Equipment');
insert into CommodityList ( CMID, Name) values (2, 'Instrument');
insert into CommodityList ( CMID, Name) values (3, 'LineSeg');
insert into CommodityList ( CMID, Name) values (4, 'Line');
insert into CommodityList ( CMID, Name) values (5, 'Specialty');
insert into CommodityList ( CMID, Name) values (6, 'Spool');
insert into CommodityList ( CMID, Name) values (7, 'Valve');
insert into CommodityList ( CMID, Name) values (8, 'Document');
insert into CommodityList ( CMID, Name) values (9, 'Weld');
insert into CommodityList ( CMID, Name) values (10, 'Isometric');
insert into CommodityList ( CMID, Name) values (11, 'Joint');
insert into CommodityList ( CMID, Name) values (12, 'Cable');


CREATE PROCEDURE [dbo].[InsertPatternData]
	(
    --pattern parameters
    @PatternId nvarchar(50),
    @PatternName nvarchar(50), 
    @Description nvarchar(50), 
    @DataObjectName nvarchar(50), 
    @TypicalValue nvarchar(50), 
    --class parameters
    @ClassId nvarchar(50), 
    @ClassName nvarchar(50), 
    @ClassIdentifier nvarchar(50), 
    @Delimiter nvarchar(50),
    --template parameters
    @TemplateId nvarchar(50),
    @TemaplateName nvarchar(50),
    @TemplateType nvarchar(50),
    --Role 1 parameters
    @Role1Id nvarchar(50),
    @Role1Name nvarchar(50),
    @Role1Type nvarchar(50),
    @Role1Property nvarchar(50),
    @Role1Value nvarchar(50),
    --Role 2 parameters
    @Role2Id nvarchar(50),
    @Role2Name nvarchar(50),
    @Role2Type nvarchar(50),
    @Role2Property nvarchar(50),
    @Role2Value nvarchar(50),
    --Role 3 parameters
    @Role3Id nvarchar(50),
    @Role3Name nvarchar(50),
    @Role3Type nvarchar(50),
    @Role3Property nvarchar(50),
    @Role3Value nvarchar(50),
    --Role 4 parameters
    @Role4Id nvarchar(50),
    @Role4Name nvarchar(50),
    @Role4Type nvarchar(50),
    @Role4Property nvarchar(50),
    @Role4Value nvarchar(50),
    --Role 5 parameters
    @Role5Id nvarchar(50),
    @Role5Name nvarchar(50),
    @Role5Type nvarchar(50),
    @Role5Property nvarchar(50),
    @Role5Value nvarchar(50),
    --Role 6 parameters
    @Role6Id nvarchar(50),
    @Role6Name nvarchar(50),
    @Role6Type nvarchar(50),
    @Role6Property nvarchar(50),
    @Role6Value nvarchar(50),
    --Commodity params
    @Commodity nvarchar(max))
    
AS 
Begin
Declare @CID int, @TMID int, @RMID int, @CMPID int, @idx int, @slice varchar(max)  

	if((select COUNT(*) from GraphMap where GMID = @PatternId)=0)
		INSERT into GraphMap (GMID, Name, Description, DataObjectName, TypicalValues) values
			(@PatternId, @PatternName, @Description, @DataObjectName, @TypicalValue);
	
	select @CID=MAX(CID)+1 from ClassMap

	if @CID is null
		set @CID = 1
	
	INSERT INTO ClassMap (CID, RID, Name, Identifier, Delimiter, GMID) values
		(@CID, @ClassId, @ClassName, @ClassIdentifier, @Delimiter, @PatternId);
		
	select @TMID=MAX(TMID)+1 from TemplateMap

	if @TMID is null
		set @TMID = 1
		
	INSERT INTO TemplateMap(TMID, RID, Name, Type, CID) VALUES
		(@TMID, @TemplateId, @TemaplateName, @TemplateType, @CID);
		
	
	select @RMID=MAX(rmid)+1 from RoleMap

	if @RMID is null
		set @RMID = 1
	if @Role1Id is not null
	INSERT INTO RoleMap(RMID, RID,Name,Type, Property, Value, TMID, CID) VALUES
		(@RMID, @Role1Id, @Role1Name, @Role1Type, @Role1Property,@Role1Value, @TMID, @CID);
	
	if @Role2Id is not null
	INSERT INTO RoleMap(RMID, RID,Name,Type, Property, Value, TMID, CID) VALUES
		(@RMID+1, @Role2Id, @Role2Name, @Role2Type, @Role2Property,@Role2Value, @TMID, @CID);
		
	if @Role3Id is not null
	INSERT INTO RoleMap(RMID, RID,Name,Type, Property, Value, TMID, CID) VALUES
		(@RMID+2, @Role3Id, @Role3Name, @Role3Type, @Role3Property,@Role3Value, @TMID, @CID);
	
	if @Role4Id is not null
	INSERT INTO RoleMap(RMID, RID,Name,Type, Property, Value, TMID, CID) VALUES
		(@RMID+3, @Role4Id, @Role4Name, @Role4Type, @Role4Property,@Role4Value, @TMID, @CID);
	
	if @Role5Id is not null
	INSERT INTO RoleMap(RMID, RID,Name,Type, Property, Value, TMID, CID) VALUES
		(@RMID+4, @Role5Id, @Role5Name, @Role5Type, @Role5Property,@Role5Value, @TMID, @CID);
	
	if @Role6Id is not null
	INSERT INTO RoleMap(RMID, RID,Name,Type, Property, Value, TMID, CID) VALUES
		(@RMID+5, @Role6Id, @Role6Name, @Role6Type, @Role6Property,@Role6Value, @TMID, @CID);
	
	select @CMPID=MAX(CMPID)+1 from CommodityMap

	if @CMPID is null
		set @CMPID = 1
		
	select @idx = 1       
    while @idx!= 0       
    begin       
        set @idx = charindex(',',@Commodity)       
        if @idx!=0       
            set @slice = left(@Commodity,@idx - 1)       
        else       
            set @slice = @Commodity       
          
        if(len(@slice)>0)  
            insert into CommodityMap (CMPID, GMID, CMID) values (
				@CMPID, @PatternId, 
				(select CMID from CommodityList where upper(Name) = upper(@slice)))
  
        set @Commodity = right(@Commodity,len(@Commodity) - @idx)       
        set @CMPID = @CMPID+1
        if len(@Commodity) = 0 break       
    end
	
END

