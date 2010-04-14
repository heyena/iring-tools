USE [iring]
GO

/****** Object:  Table [dbo].[rdf_entities]    Script Date: 09/14/2009 15:10:15 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[rdf_entities]') AND type in (N'U'))
DROP TABLE [dbo].[rdf_entities]
GO

USE [iring]
GO

/****** Object:  Table [dbo].[rdf_entities]    Script Date: 09/14/2009 15:10:15 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[rdf_entities](
	[id] [int] NOT NULL,
	[value] [nvarchar](400) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO



USE [iring]
GO

/****** Object:  Table [dbo].[rdf_literals]    Script Date: 09/14/2009 15:10:27 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[rdf_literals]') AND type in (N'U'))
DROP TABLE [dbo].[rdf_literals]
GO

USE [iring]
GO

/****** Object:  Table [dbo].[rdf_literals]    Script Date: 09/14/2009 15:10:27 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[rdf_literals](
	[id] [int] NOT NULL,
	[value] [nvarchar](4000) NOT NULL,
	[language] [nvarchar](255) NULL,
	[datatype] [nvarchar](255) NULL,
	[hash] [nchar](28) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO



USE [iring]
GO

/****** Object:  Table [dbo].[rdf_statements]    Script Date: 09/14/2009 15:10:35 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[rdf_statements]') AND type in (N'U'))
DROP TABLE [dbo].[rdf_statements]
GO

USE [iring]
GO

/****** Object:  Table [dbo].[rdf_statements]    Script Date: 09/14/2009 15:10:35 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[rdf_statements](
	[subject] [int] NOT NULL,
	[predicate] [int] NOT NULL,
	[objecttype] [int] NOT NULL,
	[object] [int] NOT NULL,
	[meta] [int] NOT NULL
) ON [PRIMARY]

GO


