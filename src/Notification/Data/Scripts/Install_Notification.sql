------------------------------------------------                        --------------------------------------------------------------
------------------------------------------------  DROP EXISTING TABLES  --------------------------------------------------------------
------------------------------------------------                        --------------------------------------------------------------

/****** Object:  Table [dbo].[Notification.Subscriptions]    Script Date: 03/11/2011 05:09:49 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Notification.Subscriptions]') AND type in (N'U'))
DROP TABLE [dbo].[Notification.Subscriptions]
GO

/****** Object:  Table [dbo].[Notification.Events]    Script Date: 03/11/2011 05:13:25 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Notification.Events]') AND type in (N'U'))
DROP TABLE [dbo].[Notification.Events]
GO

/****** Object:  Table [dbo].[Notification.Messages]    Script Date: 03/11/2011 05:15:42 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Notification.Messages]') AND type in (N'U'))
DROP TABLE [dbo].[Notification.Messages]
GO

/****** Object:  Table [dbo].[Notification.LastProcessTime]    Script Date: 03/11/2011 05:15:42 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Notification.LastProcessTime]') AND type in (N'U'))
DROP TABLE [dbo].[Notification.LastProcessTime]
GO

/****** Object:  Table [dbo].[Notification.Synchronization]    Script Date: 03/23/2011 14:45:30 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Notification.Synchronization]') AND type in (N'U'))
DROP TABLE [dbo].[Notification.Synchronization]
GO

/****** Object:  Table [dbo].[Notification.Subscriptions]    Script Date: 03/11/2011 05:09:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notification.Subscriptions](
	[SubscriptionId] [int] IDENTITY(1,1) NOT NULL,
	[UserEmail] [nvarchar](250) NOT NULL,
	[UserPath] [nvarchar](450) NOT NULL,
	[UserId] [int] NOT NULL,
	[UserName] [nvarchar](250) NOT NULL,
	[ContentPath] [nvarchar](450) NOT NULL,
	[FrequencyId] [int] NOT NULL,
	[Language] [nvarchar](5) NOT NULL,
	[Active] [tinyint] NOT NULL,
	[SitePath] [nvarchar](450) NULL,
	[SiteUrl] [nvarchar](200) NULL,
 CONSTRAINT [PK_Notification.Subscriptions] PRIMARY KEY CLUSTERED 
(
	[SubscriptionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Notification.Events]    Script Date: 03/11/2011 05:13:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notification.Events](
	[EventId] [int] IDENTITY(1,1) NOT NULL,
	[ContentPath] [nvarchar](450) NOT NULL,
	[CreatorId] [int] NOT NULL,
	[NotificationTypeId] [int] NOT NULL,
	[When] [datetime] NOT NULL,
	[LastModifierId] [int] NOT NULL,
	[Who] [nvarchar](250) NOT NULL,
 CONSTRAINT [PK_Notification.Events] PRIMARY KEY CLUSTERED 
(
	[EventId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Notification.Messages]    Script Date: 03/11/2011 05:16:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notification.Messages](
	[MessageId] [int] IDENTITY(1,1) NOT NULL,
	[Address] [nvarchar](250) NOT NULL,
	[Subject] [nvarchar](MAX) NOT NULL,
	[Body] [nvarchar](MAX) NOT NULL,
	[LockId] [nvarchar](500) NULL,
	[LockedUntil] [datetime] NULL,
	[SenderAddress] [nvarchar](250) NULL
 CONSTRAINT [PK_Notification.Messages] PRIMARY KEY CLUSTERED 
(
	[MessageId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Notification.LastProcessTime]    Script Date: 03/11/2011 05:16:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notification.LastProcessTime](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Immediately] [datetime] NULL,
	[Daily] [datetime] NULL,
	[Weekly] [datetime] NULL,
	[Monthly] [datetime] NULL,
 CONSTRAINT [PK_Notification.LastProcessTime] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Notification.Synchronization]    Script Date: 03/23/2011 14:40:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notification.Synchronization](
	[LockName] [nvarchar](200) NOT NULL,
	[Locked] [bit] NOT NULL,
	[LockedUntil] [datetime] NOT NULL,
	[ComputerName] [nvarchar](200) NULL,
	[LockId] [nvarchar](200) NOT NULL,
 CONSTRAINT [PK_Notification.Synchronization] PRIMARY KEY CLUSTERED 
(
	[LockName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO