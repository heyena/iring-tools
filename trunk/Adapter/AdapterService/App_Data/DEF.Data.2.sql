DELETE FROM [DEF].[dbo].[Lines]
GO

INSERT INTO [DEF].[dbo].[Lines]
   ([Tag],[PlantArea],[System],[PID],[PIDRev],[Plant],[LengthUOM],[Length],[DiameterUOM],[Diameter],[Fluid],[TempUOM],[DesignTemperature],[OperatingTemperature])
VALUES
   ('Tag-1','PlantArea-1','System-1','PID-1','PIDRev-1','Plant-1','METER',1,'INCH',1,'Fluid-1','degC',1,1)
GO

INSERT INTO [DEF].[dbo].[Lines]
   ([Tag],[PlantArea],[System],[PID],[PIDRev],[Plant],[LengthUOM],[Length],[DiameterUOM],[Diameter],[Fluid],[TempUOM],[DesignTemperature],[OperatingTemperature])
VALUES
   ('Tag-2','PlantArea-2','System-2','PID-2','PIDRev-2','Plant-2','METER',2,'INCH',2,'Fluid-2','degC',2,2)
GO