CREATE TABLE [dbo].[Comments] (
    [Id]      INT            IDENTITY (1, 1) NOT NULL,
    [Body]    NVARCHAR (MAX) NOT NULL,
    [MovieId] INT            NOT NULL,
    [UserId]  NVARCHAR (450) NOT NULL,
    CONSTRAINT [PK__Comments__3214EC077E525B44] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Comments_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
);

