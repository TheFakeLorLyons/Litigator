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

ALTER TABLE [Cases] DROP CONSTRAINT [FK_Cases_Attorneys_AssignedAttorneyId];

ALTER TABLE [Cases] DROP CONSTRAINT [FK_Cases_Clients_ClientId];

DROP TABLE [Attorneys];

ALTER TABLE [Clients] DROP CONSTRAINT [PK_Clients];

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'Address');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Clients] DROP COLUMN [Address];

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'ClientName');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Clients] DROP COLUMN [ClientName];

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'Phone');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Clients] DROP COLUMN [Phone];

EXEC sp_rename N'[Clients]', N'People', 'OBJECT';

EXEC sp_rename N'[Courts].[Address]', N'Email', 'COLUMN';

EXEC sp_rename N'[People].[ClientId]', N'SystemId', 'COLUMN';

EXEC sp_rename N'[People].[IX_Clients_Email]', N'IX_People_Email', 'INDEX';

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Courts]') AND [c].[name] = N'CourtType');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Courts] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Courts] ALTER COLUMN [CourtType] nvarchar(20) NULL;

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Courts]') AND [c].[name] = N'County');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Courts] DROP CONSTRAINT [' + @var4 + '];');
UPDATE [Courts] SET [County] = N'' WHERE [County] IS NULL;
ALTER TABLE [Courts] ALTER COLUMN [County] nvarchar(100) NOT NULL;
ALTER TABLE [Courts] ADD DEFAULT N'' FOR [County];

ALTER TABLE [Courts] ADD [AddressLine1] nvarchar(500) NULL;

ALTER TABLE [Courts] ADD [AddressLine2] nvarchar(500) NULL;

ALTER TABLE [Courts] ADD [BusinessHours] nvarchar(200) NULL;

ALTER TABLE [Courts] ADD [ChiefJudgeId] int NULL;

ALTER TABLE [Courts] ADD [City] nvarchar(100) NULL;

ALTER TABLE [Courts] ADD [ClerkOfCourt] nvarchar(100) NULL;

ALTER TABLE [Courts] ADD [Country] nvarchar(100) NULL DEFAULT N'United States';

ALTER TABLE [Courts] ADD [Description] nvarchar(500) NULL;

ALTER TABLE [Courts] ADD [Division] nvarchar(50) NULL;

ALTER TABLE [Courts] ADD [HasPrimaryAddress] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [Courts] ADD [HasPrimaryPhone] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [Courts] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [Courts] ADD [PhoneExtension] nvarchar(10) NULL;

ALTER TABLE [Courts] ADD [PhoneNumber] nvarchar(20) NULL;

ALTER TABLE [Courts] ADD [PostalCode] nvarchar(20) NULL;

ALTER TABLE [Courts] ADD [Website] nvarchar(500) NOT NULL DEFAULT N'';

ALTER TABLE [Cases] ADD [AssignedJudgeId] int NOT NULL DEFAULT 0;

ALTER TABLE [Cases] ADD [CurrentRealCost] decimal(18,2) NULL;

ALTER TABLE [People] ADD [AddressLine1] nvarchar(500) NULL;

ALTER TABLE [People] ADD [AddressLine2] nvarchar(500) NULL;

ALTER TABLE [People] ADD [Attorney_CaseId] int NULL;

ALTER TABLE [People] ADD [BarNumber] nvarchar(50) NULL;

ALTER TABLE [People] ADD [CaseId] int NULL;

ALTER TABLE [People] ADD [City] nvarchar(100) NULL;

ALTER TABLE [People] ADD [Country] nvarchar(100) NULL DEFAULT N'United States';

ALTER TABLE [People] ADD [County] nvarchar(100) NULL;

ALTER TABLE [People] ADD [FirstName] nvarchar(100) NOT NULL DEFAULT N'';

ALTER TABLE [People] ADD [HasPrimaryAddress] bit NULL DEFAULT CAST(1 AS bit);

ALTER TABLE [People] ADD [HasPrimaryPhone] bit NULL DEFAULT CAST(1 AS bit);

ALTER TABLE [People] ADD [IsActive] bit NULL;

ALTER TABLE [People] ADD [LastName] nvarchar(100) NULL;

ALTER TABLE [People] ADD [LegalProfessional_IsActive] bit NULL;

ALTER TABLE [People] ADD [MiddleName] nvarchar(100) NULL;

ALTER TABLE [People] ADD [ModifiedDate] datetime2 NULL;

ALTER TABLE [People] ADD [Notes] nvarchar(1000) NULL;

ALTER TABLE [People] ADD [PersonType] nvarchar(21) NOT NULL DEFAULT N'';

ALTER TABLE [People] ADD [PostalCode] nvarchar(20) NULL;

ALTER TABLE [People] ADD [PreferredName] nvarchar(100) NULL;

ALTER TABLE [People] ADD [PrimaryPhoneExtension] nvarchar(10) NULL;

ALTER TABLE [People] ADD [PrimaryPhoneNumber] nvarchar(20) NULL;

ALTER TABLE [People] ADD [Specialization] int NULL;

ALTER TABLE [People] ADD [State] nvarchar(50) NULL;

ALTER TABLE [People] ADD [Suffix] nvarchar(20) NULL;

ALTER TABLE [People] ADD [Title] nvarchar(20) NULL;

ALTER TABLE [People] ADD CONSTRAINT [PK_People] PRIMARY KEY ([SystemId]);

CREATE TABLE [ClientAttorney] (
    [AttorneysSystemId] int NOT NULL,
    [ClientsSystemId] int NOT NULL,
    CONSTRAINT [PK_ClientAttorney] PRIMARY KEY ([AttorneysSystemId], [ClientsSystemId]),
    CONSTRAINT [FK_ClientAttorney_People_AttorneysSystemId] FOREIGN KEY ([AttorneysSystemId]) REFERENCES [People] ([SystemId]),
    CONSTRAINT [FK_ClientAttorney_People_ClientsSystemId] FOREIGN KEY ([ClientsSystemId]) REFERENCES [People] ([SystemId]) ON DELETE CASCADE
);

CREATE TABLE [CourtLegalProfessional] (
    [CourtsCourtId] int NOT NULL,
    [LegalProfessionalsSystemId] int NOT NULL,
    CONSTRAINT [PK_CourtLegalProfessional] PRIMARY KEY ([CourtsCourtId], [LegalProfessionalsSystemId]),
    CONSTRAINT [FK_CourtLegalProfessional_Courts_CourtsCourtId] FOREIGN KEY ([CourtsCourtId]) REFERENCES [Courts] ([CourtId]) ON DELETE CASCADE,
    CONSTRAINT [FK_CourtLegalProfessional_People_LegalProfessionalsSystemId] FOREIGN KEY ([LegalProfessionalsSystemId]) REFERENCES [People] ([SystemId]) ON DELETE CASCADE
);

CREATE TABLE [PersonAddress] (
    [Id] int NOT NULL IDENTITY,
    [AddressLine1] nvarchar(500) NULL,
    [AddressLine2] nvarchar(500) NULL,
    [City] nvarchar(100) NULL,
    [County] nvarchar(100) NULL,
    [State] nvarchar(50) NULL,
    [PostalCode] nvarchar(20) NULL,
    [Country] nvarchar(100) NULL DEFAULT N'United States',
    [HasAddress] bit NOT NULL DEFAULT CAST(1 AS bit),
    [Type] int NOT NULL,
    [PersonId] int NULL,
    CONSTRAINT [PK_PersonAddress] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PersonAddress_People_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [People] ([SystemId]) ON DELETE CASCADE
);

CREATE TABLE [PersonPhoneNumber] (
    [Id] int NOT NULL IDENTITY,
    [PhoneNumber] nvarchar(20) NULL,
    [PhoneExtension] nvarchar(10) NULL,
    [HasPhone] bit NOT NULL DEFAULT CAST(1 AS bit),
    [Type] int NOT NULL,
    [PersonId] int NULL,
    CONSTRAINT [PK_PersonPhoneNumber] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PersonPhoneNumber_People_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [People] ([SystemId]) ON DELETE CASCADE
);

CREATE INDEX [IX_Courts_ChiefJudgeId] ON [Courts] ([ChiefJudgeId]);

CREATE INDEX [IX_Cases_AssignedJudgeId] ON [Cases] ([AssignedJudgeId]);

CREATE INDEX [IX_People_Attorney_CaseId] ON [People] ([Attorney_CaseId]);

CREATE UNIQUE INDEX [IX_People_BarNumber] ON [People] ([BarNumber]) WHERE [BarNumber] IS NOT NULL;

CREATE INDEX [IX_People_CaseId] ON [People] ([CaseId]);

CREATE UNIQUE INDEX [IX_People_Email1] ON [People] ([Email]) WHERE [Email] IS NOT NULL;

CREATE UNIQUE INDEX [IX_People_Email2] ON [People] ([Email]) WHERE [Email] IS NOT NULL;

CREATE INDEX [IX_ClientAttorney_ClientsSystemId] ON [ClientAttorney] ([ClientsSystemId]);

CREATE INDEX [IX_CourtLegalProfessional_LegalProfessionalsSystemId] ON [CourtLegalProfessional] ([LegalProfessionalsSystemId]);

CREATE INDEX [IX_PersonAddress_PersonId] ON [PersonAddress] ([PersonId]);

CREATE INDEX [IX_PersonPhoneNumber_PersonId] ON [PersonPhoneNumber] ([PersonId]);

ALTER TABLE [Cases] ADD CONSTRAINT [FK_Cases_People_AssignedAttorneyId] FOREIGN KEY ([AssignedAttorneyId]) REFERENCES [People] ([SystemId]) ON DELETE NO ACTION;

ALTER TABLE [Cases] ADD CONSTRAINT [FK_Cases_People_AssignedJudgeId] FOREIGN KEY ([AssignedJudgeId]) REFERENCES [People] ([SystemId]) ON DELETE NO ACTION;

ALTER TABLE [Cases] ADD CONSTRAINT [FK_Cases_People_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [People] ([SystemId]) ON DELETE NO ACTION;

ALTER TABLE [Courts] ADD CONSTRAINT [FK_Courts_People_ChiefJudgeId] FOREIGN KEY ([ChiefJudgeId]) REFERENCES [People] ([SystemId]) ON DELETE NO ACTION;

ALTER TABLE [People] ADD CONSTRAINT [FK_People_Cases_Attorney_CaseId] FOREIGN KEY ([Attorney_CaseId]) REFERENCES [Cases] ([CaseId]);

ALTER TABLE [People] ADD CONSTRAINT [FK_People_Cases_CaseId] FOREIGN KEY ([CaseId]) REFERENCES [Cases] ([CaseId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250704124143_UpdateSchema_v2', N'9.0.6');

COMMIT;
GO

