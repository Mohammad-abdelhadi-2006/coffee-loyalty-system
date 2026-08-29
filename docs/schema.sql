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
    WHERE [MigrationId] = N'20260724191251_InitialCreate'
)
BEGIN
    CREATE TABLE [Customers] (
        [Id] int NOT NULL IDENTITY,
        [FullName] nvarchar(100) NOT NULL,
        [PhoneNumber] nvarchar(20) NOT NULL,
        [FirebaseUid] nvarchar(128) NULL,
        [PointsBalance] int NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Customers] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Customer_Balance] CHECK ([PointsBalance] >= 0)
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724191251_InitialCreate'
)
BEGIN
    CREATE TABLE [Employees] (
        [Id] int NOT NULL IDENTITY,
        [FullName] nvarchar(100) NOT NULL,
        [Username] nvarchar(50) NOT NULL,
        [PasswordHash] nvarchar(255) NOT NULL,
        [Role] nvarchar(20) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Employees] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724191251_InitialCreate'
)
BEGIN
    CREATE TABLE [Products] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Price] decimal(18,3) NOT NULL,
        [UnitType] nvarchar(20) NOT NULL,
        [Category] nvarchar(20) NOT NULL,
        [IsAvailable] bit NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724191251_InitialCreate'
)
BEGIN
    CREATE TABLE [Orders] (
        [Id] int NOT NULL IDENTITY,
        [CustomerId] int NOT NULL,
        [EmployeeId] int NOT NULL,
        [Total] decimal(18,3) NOT NULL,
        [PointsEarned] int NOT NULL,
        [PointsRedeemed] int NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Order_Points] CHECK ([PointsEarned] >= 0 AND [PointsRedeemed] >= 0),
        CONSTRAINT [FK_Orders_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Orders_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724191251_InitialCreate'
)
BEGIN
    CREATE TABLE [OrderItems] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [ProductId] int NOT NULL,
        [ProductNameSnapshot] nvarchar(100) NOT NULL,
        [Quantity] decimal(18,3) NOT NULL,
        [ReturnedQuantity] decimal(18,3) NOT NULL,
        [UnitPriceSnapshot] decimal(18,3) NOT NULL,
        CONSTRAINT [PK_OrderItems] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_OrderItem_Returned] CHECK ([ReturnedQuantity] >= 0 AND [ReturnedQuantity] <= [Quantity]),
        CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrderItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724191251_InitialCreate'
)
BEGIN
    CREATE TABLE [PointsTransactions] (
        [Id] int NOT NULL IDENTITY,
        [CustomerId] int NOT NULL,
        [OrderId] int NOT NULL,
        [Amount] int NOT NULL,
        [Type] nvarchar(20) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_PointsTransactions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PointsTransactions_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PointsTransactions_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724191251_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Customers_FirebaseUid] ON [Customers] ([FirebaseUid]) WHERE [FirebaseUid] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724191251_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Customers_PhoneNumber] ON [Customers] ([PhoneNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724191251_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Employees_Username] ON [Employees] ([Username]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724191251_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderItems_OrderId] ON [OrderItems] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724191251_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderItems_ProductId] ON [OrderItems] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724191251_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_CreatedAt] ON [Orders] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724191251_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_CustomerId] ON [Orders] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724191251_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_EmployeeId] ON [Orders] ([EmployeeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724191251_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PointsTransactions_CustomerId] ON [PointsTransactions] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724191251_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_PointsTransaction_Order_Type] ON [PointsTransactions] ([OrderId], [Type]) WHERE [Type] <> ''Refund''');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724191251_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260724191251_InitialCreate', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730163712_AddTokenVersion'
)
BEGIN
    ALTER TABLE [Employees] ADD [TokenVersion] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730163712_AddTokenVersion'
)
BEGIN
    ALTER TABLE [Customers] ADD [TokenVersion] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730163712_AddTokenVersion'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260730163712_AddTokenVersion', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260804212044_AllowOpeningBalanceTransactions'
)
BEGIN
    DROP INDEX [UX_PointsTransaction_Order_Type] ON [PointsTransactions];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260804212044_AllowOpeningBalanceTransactions'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PointsTransactions]') AND [c].[name] = N'OrderId');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [PointsTransactions] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [PointsTransactions] ALTER COLUMN [OrderId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260804212044_AllowOpeningBalanceTransactions'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_PointsTransaction_Order_Type] ON [PointsTransactions] ([OrderId], [Type]) WHERE [Type] <> ''Refund'' AND [OrderId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260804212044_AllowOpeningBalanceTransactions'
)
BEGIN
    EXEC(N'ALTER TABLE [PointsTransactions] ADD CONSTRAINT [CK_PointsTransaction_Order] CHECK ([Type] = ''OpeningBalance'' OR [OrderId] IS NOT NULL)');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260804212044_AllowOpeningBalanceTransactions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260804212044_AllowOpeningBalanceTransactions', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807183443_AddOrderIdempotencyKey'
)
BEGIN
    ALTER TABLE [Orders] ADD [IdempotencyKey] nvarchar(64) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807183443_AddOrderIdempotencyKey'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_Order_IdempotencyKey] ON [Orders] ([IdempotencyKey]) WHERE [IdempotencyKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807183443_AddOrderIdempotencyKey'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260807183443_AddOrderIdempotencyKey', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828201518_AddOpeningBalanceUniqueIndex'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_PointsTransaction_Customer_OpeningBalance] ON [PointsTransactions] ([CustomerId]) WHERE [Type] = ''OpeningBalance''');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260828201518_AddOpeningBalanceUniqueIndex'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260828201518_AddOpeningBalanceUniqueIndex', N'10.0.10');
END;

COMMIT;
GO

