IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Attorneys] (
    [AttorneyId] int NOT NULL IDENTITY,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [BarNumber] nvarchar(50) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [Phone] nvarchar(20) NOT NULL,
    [Specialization] int NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    CONSTRAINT [PK_Attorneys] PRIMARY KEY ([AttorneyId])
);

CREATE TABLE [Clients] (
    [ClientId] int NOT NULL IDENTITY,
    [ClientName] nvarchar(200) NOT NULL,
    [Address] nvarchar(500) NOT NULL,
    [Phone] nvarchar(20) NOT NULL,
    [Email] nvarchar(100) NULL,
    [CreatedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_Clients] PRIMARY KEY ([ClientId])
);

CREATE TABLE [Courts] (
    [CourtId] int NOT NULL IDENTITY,
    [CourtName] nvarchar(200) NOT NULL,
    [Address] nvarchar(100) NOT NULL,
    [County] nvarchar(100) NULL,
    [State] nvarchar(50) NOT NULL,
    [CourtType] nvarchar(20) NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    CONSTRAINT [PK_Courts] PRIMARY KEY ([CourtId])
);

CREATE TABLE [Cases] (
    [CaseId] int NOT NULL IDENTITY,
    [CaseNumber] nvarchar(50) NOT NULL,
    [CaseTitle] nvarchar(200) NOT NULL,
    [CaseType] nvarchar(50) NOT NULL,
    [FilingDate] datetime2 NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [EstimatedValue] decimal(18,2) NULL,
    [ClientId] int NOT NULL,
    [AssignedAttorneyId] int NOT NULL,
    [CourtId] int NOT NULL,
    CONSTRAINT [PK_Cases] PRIMARY KEY ([CaseId]),
    CONSTRAINT [FK_Cases_Attorneys_AssignedAttorneyId] FOREIGN KEY ([AssignedAttorneyId]) REFERENCES [Attorneys] ([AttorneyId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Cases_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([ClientId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Cases_Courts_CourtId] FOREIGN KEY ([CourtId]) REFERENCES [Courts] ([CourtId]) ON DELETE NO ACTION
);

CREATE TABLE [Deadlines] (
    [DeadlineId] int NOT NULL IDENTITY,
    [DeadlineType] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [Notes] nvarchar(max) NULL,
    [DeadlineDate] datetime2 NOT NULL,
    [CompletedDate] datetime2 NULL,
    [IsCompleted] bit NOT NULL,
    [IsCritical] bit NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    [CaseId] int NOT NULL,
    CONSTRAINT [PK_Deadlines] PRIMARY KEY ([DeadlineId]),
    CONSTRAINT [FK_Deadlines_Cases_CaseId] FOREIGN KEY ([CaseId]) REFERENCES [Cases] ([CaseId]) ON DELETE CASCADE
);

CREATE TABLE [Documents] (
    [DocumentId] int NOT NULL IDENTITY,
    [DocumentName] nvarchar(200) NOT NULL,
    [DocumentType] nvarchar(50) NOT NULL,
    [FilePath] nvarchar(500) NOT NULL,
    [Description] nvarchar(max) NULL,
    [UploadDate] datetime2 NOT NULL,
    [FileSize] bigint NOT NULL,
    [UploadedBy] nvarchar(100) NOT NULL,
    [CaseId] int NOT NULL,
    CONSTRAINT [PK_Documents] PRIMARY KEY ([DocumentId]),
    CONSTRAINT [FK_Documents_Cases_CaseId] FOREIGN KEY ([CaseId]) REFERENCES [Cases] ([CaseId]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [IX_Attorneys_BarNumber] ON [Attorneys] ([BarNumber]);

CREATE UNIQUE INDEX [IX_Attorneys_Email] ON [Attorneys] ([Email]);

CREATE INDEX [IX_Cases_AssignedAttorneyId] ON [Cases] ([AssignedAttorneyId]);

CREATE UNIQUE INDEX [IX_Cases_CaseNumber] ON [Cases] ([CaseNumber]);

CREATE INDEX [IX_Cases_ClientId] ON [Cases] ([ClientId]);

CREATE INDEX [IX_Cases_CourtId] ON [Cases] ([CourtId]);

CREATE INDEX [IX_Cases_FilingDate] ON [Cases] ([FilingDate]);

CREATE INDEX [IX_Cases_Status] ON [Cases] ([Status]);

CREATE INDEX [IX_Clients_Email] ON [Clients] ([Email]);

CREATE INDEX [IX_Deadlines_CaseId] ON [Deadlines] ([CaseId]);

CREATE INDEX [IX_Deadlines_DeadlineDate] ON [Deadlines] ([DeadlineDate]);

CREATE INDEX [IX_Deadlines_IsCompleted] ON [Deadlines] ([IsCompleted]);

CREATE INDEX [IX_Documents_CaseId] ON [Documents] ([CaseId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250624062102_InitialCreate', N'9.0.6');

COMMIT;
GO

