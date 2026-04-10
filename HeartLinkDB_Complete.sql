/*
    HEARTLINK - DATABASE INITIALIZATION SCRIPT
    -----------------------------------------
    This script creates the database schema, indexes, and sample data.
    Run this in SQL Server Management Studio (SSMS).
*/

USE [master];
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'HeartLinkDB')
BEGIN
    ALTER DATABASE [HeartLinkDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [HeartLinkDB];
END
GO

CREATE DATABASE [HeartLinkDB];
GO

USE [HeartLinkDB];
GO

-- 1. Create Tables
CREATE TABLE [__EFMigrationsHistory] (
    [MigrationId] nvarchar(150) NOT NULL,
    [ProductVersion] nvarchar(32) NOT NULL,
    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
);

CREATE TABLE [ACCOUNT] (
    [AccountID] int NOT NULL IDENTITY,
    [Email] nvarchar(100) NOT NULL,
    [Password] nvarchar(255) NOT NULL,
    [PhoneNumber] nvarchar(15) NULL,
    [CreatedDate] datetime2 NOT NULL DEFAULT (GETDATE()),
    [Status] bit NOT NULL DEFAULT CAST(1 AS bit),
    [UserRole] nvarchar(20) NOT NULL DEFAULT N'User',
    [BanReason] nvarchar(500) NULL,
    CONSTRAINT [PK_ACCOUNT] PRIMARY KEY ([AccountID]),
    CONSTRAINT [UQ_ACCOUNT_Email] UNIQUE ([Email])
);

CREATE TABLE [PROFILE] (
    [ProfileID] int NOT NULL IDENTITY,
    [AccountID] int NOT NULL,
    [FullName] nvarchar(100) NOT NULL,
    [BirthDate] datetime2 NOT NULL,
    [Gender] nvarchar(10) NOT NULL,
    [Bio] nvarchar(500) NULL,
    [Location] nvarchar(255) NULL,
    [Avatar] nvarchar(255) NULL,
    [TargetGender] nvarchar(10) NULL,
    [MinAge] int NOT NULL DEFAULT 18,
    [MaxAge] int NOT NULL DEFAULT 99,
    [Radius] float NOT NULL DEFAULT 50.0,
    [Latitude] float NULL,
    [Longitude] float NULL,
    [CreatedDate] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_PROFILE] PRIMARY KEY ([ProfileID]),
    CONSTRAINT [FK_PROFILE_ACCOUNT_AccountID] FOREIGN KEY ([AccountID]) REFERENCES [ACCOUNT] ([AccountID]) ON DELETE CASCADE,
    CONSTRAINT [UQ_PROFILE_AccountID] UNIQUE ([AccountID])
);

CREATE TABLE [LIKES] (
    [LikeID] int NOT NULL IDENTITY,
    [SenderID] int NOT NULL,
    [ReceiverID] int NOT NULL,
    [Type] nvarchar(10) NOT NULL, -- 'Like' or 'Pass'
    [Timestamp] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_LIKES] PRIMARY KEY ([LikeID]),
    CONSTRAINT [FK_LIKES_ACCOUNT_ReceiverID] FOREIGN KEY ([ReceiverID]) REFERENCES [ACCOUNT] ([AccountID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_LIKES_ACCOUNT_SenderID] FOREIGN KEY ([SenderID]) REFERENCES [ACCOUNT] ([AccountID]) ON DELETE NO ACTION
);

CREATE TABLE [MATCHES] (
    [MatchID] int NOT NULL IDENTITY,
    [LikeID] int NOT NULL,
    [User1ID] int NOT NULL,
    [User2ID] int NOT NULL,
    [MatchedDate] datetime2 NOT NULL DEFAULT (GETDATE()),
    [Status] tinyint NOT NULL DEFAULT 1, -- 1: Active, 0: Cancelled
    [PairKey] AS CASE WHEN [User1ID] < [User2ID] THEN CONCAT([User1ID],N'-',[User2ID]) ELSE CONCAT([User2ID],N'-',[User1ID]) END PERSISTED,
    CONSTRAINT [PK_MATCHES] PRIMARY KEY ([MatchID]),
    CONSTRAINT [FK_MATCHES_ACCOUNT_User1ID] FOREIGN KEY ([User1ID]) REFERENCES [ACCOUNT] ([AccountID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MATCHES_ACCOUNT_User2ID] FOREIGN KEY ([User2ID]) REFERENCES [ACCOUNT] ([AccountID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MATCHES_LIKES_LikeID] FOREIGN KEY ([LikeID]) REFERENCES [LIKES] ([LikeID]) ON DELETE NO ACTION
);

CREATE TABLE [PROFILE_INTERESTS] (
    [ProfileID] int NOT NULL,
    [InterestName] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_PROFILE_INTERESTS] PRIMARY KEY ([ProfileID], [InterestName]),
    CONSTRAINT [FK_PROFILE_INTERESTS_PROFILE_ProfileID] FOREIGN KEY ([ProfileID]) REFERENCES [PROFILE] ([ProfileID]) ON DELETE CASCADE
);

CREATE TABLE [MESSAGE] (
    [MessageID] bigint NOT NULL IDENTITY,
    [MatchID] int NOT NULL,
    [SenderID] int NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [SentTime] datetime2 NOT NULL DEFAULT (GETDATE()),
    [IsRead] bit NOT NULL DEFAULT 0,
    CONSTRAINT [PK_MESSAGE] PRIMARY KEY ([MessageID]),
    CONSTRAINT [FK_MESSAGE_ACCOUNT_SenderID] FOREIGN KEY ([SenderID]) REFERENCES [ACCOUNT] ([AccountID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MESSAGE_MATCHES_MatchID] FOREIGN KEY ([MatchID]) REFERENCES [MATCHES] ([MatchID]) ON DELETE CASCADE
);

-- 2. Create Indexes
CREATE UNIQUE INDEX [IX_ACCOUNT_PhoneNumber] ON [ACCOUNT] ([PhoneNumber]) WHERE [PhoneNumber] IS NOT NULL;
CREATE UNIQUE INDEX [IX_LIKES_SenderID_ReceiverID] ON [LIKES] ([SenderID], [ReceiverID]);
CREATE UNIQUE INDEX [IX_MATCHES_PairKey] ON [MATCHES] ([PairKey]);
CREATE INDEX [IX_MESSAGE_MatchID] ON [MESSAGE] ([MatchID]);

-- 3. Register Migrations
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260408184047_AddTriggerSupport', N'8.0.8');
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260409091041_AddBanReasonAndLocation', N'8.0.8');

-- 4. Sample Data (Password is '123456' hashed by ASP.NET Identity)
-- Hash: AQAAAAIAAYagAAAAEOc9/tP9lS/WJ6/a1H4fQ2xYy8... (Placeholder, code will handle login)
-- Note: Replace with actual hashes if you need to login with these specific accounts.
-- Here we use a standard manual hash format for demo purposes.

SET IDENTITY_INSERT [ACCOUNT] ON;
INSERT INTO [ACCOUNT] ([AccountID], [Email], [Password], [PhoneNumber], [UserRole], [Status])
VALUES 
(1, N'admin@heartlink.vn', N'AQAAAAIAAYagAAAAEOfD7j4vTj8vH7...admin_hash', N'0900000001', N'Admin', 1),
(2, N'alice@heartlink.vn', N'AQAAAAIAAYagAAAAEOfD7j4vTj8vH7...user_hash', N'0900000002', N'User', 1),
(3, N'bob@heartlink.vn', N'AQAAAAIAAYagAAAAEOfD7j4vTj8vH7...user_hash', N'0900000003', N'User', 1),
(4, N'charlie@heartlink.vn', N'AQAAAAIAAYagAAAAEOfD7j4vTj8vH7...user_hash', N'0900000004', N'User', 1),
(5, N'diana@heartlink.vn', N'AQAAAAIAAYagAAAAEOfD7j4vTj8vH7...user_hash', N'0900000005', N'User', 1);
SET IDENTITY_INSERT [ACCOUNT] OFF;

SET IDENTITY_INSERT [PROFILE] ON;
INSERT INTO [PROFILE] ([ProfileID], [AccountID], [FullName], [BirthDate], [Gender], [Bio], [Location], [TargetGender], [MinAge], [MaxAge], [Radius])
VALUES 
(1, 1, N'Hệ Thống Admin', '1990-01-01', N'Nam', N'Quản trị viên HeartLink', N'Hà Nội', N'Tất cả', 18, 99, 50),
(2, 2, N'Nguyễn Thu Thảo', '2000-05-15', N'Nữ', N'Thích đi du lịch và chụp ảnh', N'Hà Nội', N'Nam', 20, 35, 30),
(3, 3, N'Trần Văn Bình', '1998-10-20', N'Nam', N'Yêu thể thao, tìm người cùng sở thích', N'Sài Gòn', N'Nữ', 18, 30, 50),
(4, 4, N'Lê Minh Anh', '2002-02-14', N'Nữ', N'Sinh viên năm 4, vui vẻ hòa đồng', N'Đà Nẵng', N'Nam', 20, 25, 20),
(5, 5, N'Hoàng Diệu Nhi', '1995-12-25', N'Nữ', N'Tìm kiếm mối quan hệ nghiêm túc', N'Vũng Tàu', N'Nam', 25, 40, 100);
SET IDENTITY_INSERT [PROFILE] OFF;

INSERT INTO [PROFILE_INTERESTS] ([ProfileID], [InterestName])
VALUES 
(2, N'Du lịch'), (2, N'Chụp ảnh'), (2, N'Cà phê'),
(3, N'Bóng đá'), (3, N'Gym'), (3, N'Game'),
(4, N'Sách'), (4, N'Mèo'),
(5, N'Nấu ăn'), (5, N'Âm nhạc');

GO
PRINT 'HeartLink Database initialized successfully.';
