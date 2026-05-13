CREATE TABLE [dbo].[Users] (
    [Id]                   NVARCHAR (450)     NOT NULL,
    [UserName]             NVARCHAR (256)     NULL,
    [NormalizedUserName]   NVARCHAR (256)     NULL,
    [Email]                NVARCHAR (256)     NULL,
    [NormalizedEmail]      NVARCHAR (256)     NULL,
    [EmailConfirmed]       BIT                CONSTRAINT [DF_Users_EmailConfirmed] DEFAULT ('false') NOT NULL,
    [PasswordHash]         NVARCHAR (MAX)     NULL,
    [SecurityStamp]        NVARCHAR (MAX)     NULL,
    [ConcurrencyStamp]     NVARCHAR (MAX)     NULL,
    [PhoneNumber]          NVARCHAR (MAX)     NULL,
    [PhoneNumberConfirmed] BIT                CONSTRAINT [DF_Users_PhoneNumberConfirmed] DEFAULT ('false') NOT NULL,
    [TwoFactorEnabled]     BIT                CONSTRAINT [DF_Users_TwoFactorEnabled] DEFAULT ('false') NOT NULL,
    [LockoutEnd]           DATETIMEOFFSET (7) NULL,
    [LockoutEnabled]       BIT                CONSTRAINT [DF_Users_LockoutEnabled] DEFAULT ('false') NOT NULL,
    [AccessFailedCount]    INT                CONSTRAINT [DF_Users_AccessFailedCount] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
);

