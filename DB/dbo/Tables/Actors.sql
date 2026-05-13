CREATE TABLE [dbo].[Actors] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Name]        NVARCHAR (50)  NOT NULL,
    [DateOfBirth] DATETIME2 (7)  NOT NULL,
    [Picture]     NVARCHAR (MAX) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

