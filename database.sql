IF DB_ID(N'EnhanzerPurchaseDb') IS NULL
BEGIN
    CREATE DATABASE EnhanzerPurchaseDb;
END
GO

USE EnhanzerPurchaseDb;
GO

IF OBJECT_ID(N'dbo.Location_Details', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Location_Details
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Location_Details PRIMARY KEY,
        Location_Code NVARCHAR(50) NOT NULL,
        Location_Name NVARCHAR(200) NOT NULL
    );

    CREATE UNIQUE INDEX UX_Location_Details_Location_Code
        ON dbo.Location_Details(Location_Code);
END
GO

IF OBJECT_ID(N'dbo.Purchase_Bills', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Purchase_Bills
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Purchase_Bills PRIMARY KEY,
        Created_At_Utc DATETIME2 NOT NULL,
        Created_By_Email NVARCHAR(256) NOT NULL,
        Total_Items INT NOT NULL,
        Total_Quantity DECIMAL(18,2) NOT NULL,
        Total_Cost DECIMAL(18,2) NOT NULL,
        Total_Selling DECIMAL(18,2) NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Purchase_Bill_Items', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Purchase_Bill_Items
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Purchase_Bill_Items PRIMARY KEY,
        Purchase_Bill_Id INT NOT NULL,
        Item NVARCHAR(100) NOT NULL,
        Batch NVARCHAR(200) NOT NULL,
        Standard_Cost DECIMAL(18,2) NOT NULL,
        Standard_Price DECIMAL(18,2) NOT NULL,
        Quantity DECIMAL(18,2) NOT NULL,
        Discount DECIMAL(5,2) NOT NULL,
        Total_Cost DECIMAL(18,2) NOT NULL,
        Total_Selling DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_Purchase_Bill_Items_Purchase_Bills
            FOREIGN KEY (Purchase_Bill_Id)
            REFERENCES dbo.Purchase_Bills(Id)
            ON DELETE CASCADE
    );
END
GO
