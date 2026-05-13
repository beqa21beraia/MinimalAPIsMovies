CREATE TABLE [dbo].[Errors] (
    [Id]           UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [ErrorMessage] NVARCHAR (MAX)   NOT NULL,
    [StackTrace]   NVARCHAR (MAX)   NULL,
    [Date]         DATETIME2 (7)    NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

