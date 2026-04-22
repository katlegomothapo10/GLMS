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
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422144912_InitialCreate'
)
BEGIN
    CREATE TABLE [Clients] (
        [ClientId] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [Phone] nvarchar(max) NOT NULL,
        [Address] nvarchar(200) NOT NULL,
        [Region] nvarchar(50) NOT NULL,
        [ContactPerson] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Clients] PRIMARY KEY ([ClientId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422144912_InitialCreate'
)
BEGIN
    CREATE TABLE [Contracts] (
        [ContractId] int NOT NULL IDENTITY,
        [ContractNumber] nvarchar(450) NOT NULL,
        [ClientId] int NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [Status] int NOT NULL,
        [ServiceLevel] int NOT NULL,
        [ContractValueUSD] decimal(18,2) NOT NULL,
        [SpecialTerms] nvarchar(500) NULL,
        [SignedAgreementPath] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Contracts] PRIMARY KEY ([ContractId]),
        CONSTRAINT [FK_Contracts_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([ClientId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422144912_InitialCreate'
)
BEGIN
    CREATE TABLE [ServiceRequests] (
        [ServiceRequestId] int NOT NULL IDENTITY,
        [RequestNumber] nvarchar(450) NOT NULL,
        [ContractId] int NOT NULL,
        [Description] nvarchar(200) NOT NULL,
        [CostUSD] decimal(18,2) NOT NULL,
        [CostZAR] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        [ExchangeRateUsed] decimal(18,4) NOT NULL,
        [SpecialInstructions] nvarchar(max) NULL,
        [RequestedDate] datetime2 NOT NULL,
        [CompletedDate] datetime2 NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ServiceRequests] PRIMARY KEY ([ServiceRequestId]),
        CONSTRAINT [FK_ServiceRequests_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([ContractId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422144912_InitialCreate'
)
BEGIN
    CREATE TABLE [ServiceRequestLogs] (
        [LogId] int NOT NULL IDENTITY,
        [ServiceRequestId] int NOT NULL,
        [Action] nvarchar(max) NOT NULL,
        [Details] nvarchar(max) NULL,
        [Timestamp] datetime2 NOT NULL,
        [PerformedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_ServiceRequestLogs] PRIMARY KEY ([LogId]),
        CONSTRAINT [FK_ServiceRequestLogs_ServiceRequests_ServiceRequestId] FOREIGN KEY ([ServiceRequestId]) REFERENCES [ServiceRequests] ([ServiceRequestId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422144912_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Contracts_ClientId] ON [Contracts] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422144912_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Contracts_ContractNumber] ON [Contracts] ([ContractNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422144912_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ServiceRequestLogs_ServiceRequestId] ON [ServiceRequestLogs] ([ServiceRequestId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422144912_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ServiceRequests_ContractId] ON [ServiceRequests] ([ContractId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422144912_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ServiceRequests_RequestNumber] ON [ServiceRequests] ([RequestNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260422144912_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260422144912_InitialCreate', N'9.0.0');
END;

COMMIT;
GO

