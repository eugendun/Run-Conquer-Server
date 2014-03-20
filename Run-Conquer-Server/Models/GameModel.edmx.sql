
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 03/20/2014 01:30:12
-- Generated from EDMX file: D:\Source\Repos\Run-Conquer-Server\Run-Conquer-Server\Models\GameModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [RunConquer];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_GameInstancePlayer]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PlayerSet] DROP CONSTRAINT [FK_GameInstancePlayer];
GO
IF OBJECT_ID(N'[dbo].[FK_MapGameInstance]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[MapSet] DROP CONSTRAINT [FK_MapGameInstance];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[PlayerSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PlayerSet];
GO
IF OBJECT_ID(N'[dbo].[GameInstanceSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GameInstanceSet];
GO
IF OBJECT_ID(N'[dbo].[TeamSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TeamSet];
GO
IF OBJECT_ID(N'[dbo].[MapSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[MapSet];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'PlayerSet'
CREATE TABLE [dbo].[PlayerSet] (
    [Id] int  NOT NULL,
    [Position_x] float  NOT NULL,
    [Position_y] float  NOT NULL,
    [GameInstanceId] int  NULL
);
GO

-- Creating table 'GameInstanceSet'
CREATE TABLE [dbo].[GameInstanceSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Map_Id] int  NULL
);
GO

-- Creating table 'TeamSet'
CREATE TABLE [dbo].[TeamSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Color] nvarchar(max)  NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [GameInstanceId] int  NOT NULL
);
GO

-- Creating table 'MapSet'
CREATE TABLE [dbo].[MapSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [LatLont_x] float  NOT NULL,
    [LatLont_y] float  NOT NULL,
    [Size_x] float  NOT NULL,
    [Size_y] float  NOT NULL,
    [Zoom] int  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'PlayerSet'
ALTER TABLE [dbo].[PlayerSet]
ADD CONSTRAINT [PK_PlayerSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'GameInstanceSet'
ALTER TABLE [dbo].[GameInstanceSet]
ADD CONSTRAINT [PK_GameInstanceSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'TeamSet'
ALTER TABLE [dbo].[TeamSet]
ADD CONSTRAINT [PK_TeamSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'MapSet'
ALTER TABLE [dbo].[MapSet]
ADD CONSTRAINT [PK_MapSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [GameInstanceId] in table 'PlayerSet'
ALTER TABLE [dbo].[PlayerSet]
ADD CONSTRAINT [FK_GameInstancePlayer]
    FOREIGN KEY ([GameInstanceId])
    REFERENCES [dbo].[GameInstanceSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GameInstancePlayer'
CREATE INDEX [IX_FK_GameInstancePlayer]
ON [dbo].[PlayerSet]
    ([GameInstanceId]);
GO

-- Creating foreign key on [Map_Id] in table 'GameInstanceSet'
ALTER TABLE [dbo].[GameInstanceSet]
ADD CONSTRAINT [FK_MapGameInstance]
    FOREIGN KEY ([Map_Id])
    REFERENCES [dbo].[MapSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_MapGameInstance'
CREATE INDEX [IX_FK_MapGameInstance]
ON [dbo].[GameInstanceSet]
    ([Map_Id]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------