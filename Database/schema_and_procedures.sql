/* SQL Server schema in 3NF for restaurant online orders */

CREATE TABLE Categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE Allergens (
    AllergenId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE Products (
    ProductId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    ProductType NVARCHAR(20) NOT NULL CHECK (ProductType IN ('Dish', 'Menu')),
    CategoryId INT NOT NULL,
    PortionQuantity NVARCHAR(100) NOT NULL,
    TotalQuantity DECIMAL(10,2) NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId)
);

CREATE TABLE ProductImages (
    ProductImageId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    ImageUrl NVARCHAR(300) NOT NULL,
    CONSTRAINT FK_ProductImages_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);

CREATE TABLE ProductAllergens (
    ProductId INT NOT NULL,
    AllergenId INT NOT NULL,
    CONSTRAINT PK_ProductAllergens PRIMARY KEY (ProductId, AllergenId),
    CONSTRAINT FK_ProductAllergens_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_ProductAllergens_Allergens FOREIGN KEY (AllergenId) REFERENCES Allergens(AllergenId)
);

CREATE TABLE MenuComponents (
    MenuProductId INT NOT NULL,
    DishProductId INT NOT NULL,
    ComponentPortion NVARCHAR(100) NOT NULL,
    CONSTRAINT PK_MenuComponents PRIMARY KEY (MenuProductId, DishProductId),
    CONSTRAINT FK_MenuComponents_Menu FOREIGN KEY (MenuProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_MenuComponents_Dish FOREIGN KEY (DishProductId) REFERENCES Products(ProductId)
);

CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    Phone NVARCHAR(20) NOT NULL,
    DeliveryAddress NVARCHAR(300) NOT NULL,
    PasswordHash NVARCHAR(300) NOT NULL,
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Client', 'Employee'))
);

CREATE TABLE Orders (
    OrderId INT IDENTITY(1,1) PRIMARY KEY,
    OrderCode NVARCHAR(40) NOT NULL UNIQUE,
    UserId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FoodCost DECIMAL(10,2) NOT NULL,
    ShippingCost DECIMAL(10,2) NOT NULL,
    DiscountValue DECIMAL(10,2) NOT NULL,
    TotalCost DECIMAL(10,2) NOT NULL,
    EstimatedDeliveryAt DATETIME2 NOT NULL,
    Status NVARCHAR(20) NOT NULL CHECK (Status IN ('registered','preparing','on_the_way','delivered','cancelled')),
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

CREATE TABLE OrderLines (
    OrderLineId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    CONSTRAINT FK_OrderLines_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
    CONSTRAINT FK_OrderLines_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);
GO

/* Stored procedures (parameterized for SQL injection protection) */

CREATE OR ALTER PROCEDURE sp_InsertCategory
    @Name NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Categories (Name) VALUES (@Name);
END;
GO

CREATE OR ALTER PROCEDURE sp_InsertAllergen
    @Name NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Allergens (Name) VALUES (@Name);
END;
GO

CREATE OR ALTER PROCEDURE sp_InsertProduct
    @Name NVARCHAR(150),
    @ProductType NVARCHAR(20),
    @CategoryId INT,
    @PortionQuantity NVARCHAR(100),
    @TotalQuantity DECIMAL(10,2),
    @UnitPrice DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Products (Name, ProductType, CategoryId, PortionQuantity, TotalQuantity, UnitPrice)
    VALUES (@Name, @ProductType, @CategoryId, @PortionQuantity, @TotalQuantity, @UnitPrice);
END;
GO

CREATE OR ALTER PROCEDURE sp_UpdateProduct
    @ProductId INT,
    @Name NVARCHAR(150),
    @CategoryId INT,
    @PortionQuantity NVARCHAR(100),
    @TotalQuantity DECIMAL(10,2),
    @UnitPrice DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Products
       SET Name = @Name,
           CategoryId = @CategoryId,
           PortionQuantity = @PortionQuantity,
           TotalQuantity = @TotalQuantity,
           UnitPrice = @UnitPrice
     WHERE ProductId = @ProductId;
END;
GO

CREATE OR ALTER PROCEDURE sp_UpdateOrderStatus
    @OrderCode NVARCHAR(40),
    @Status NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Orders
       SET Status = @Status
     WHERE OrderCode = @OrderCode
       AND Status NOT IN ('delivered','cancelled');
END;
GO

CREATE OR ALTER PROCEDURE sp_SelectRestaurantMenu
AS
BEGIN
    SET NOCOUNT ON;
    SELECT c.Name AS CategoryName,
           p.ProductId,
           p.Name,
           p.ProductType,
           p.PortionQuantity,
           p.TotalQuantity,
           p.UnitPrice
      FROM Products p
      INNER JOIN Categories c ON c.CategoryId = p.CategoryId
     WHERE p.IsActive = 1
     ORDER BY c.Name, p.Name;
END;
GO

CREATE OR ALTER PROCEDURE sp_SelectMenuByNameFilter
    @Keyword NVARCHAR(100),
    @ShouldContain BIT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.ProductId, p.Name, c.Name AS CategoryName, p.UnitPrice
      FROM Products p
      INNER JOIN Categories c ON c.CategoryId = p.CategoryId
     WHERE (@ShouldContain = 1 AND p.Name LIKE '%' + @Keyword + '%')
        OR (@ShouldContain = 0 AND p.Name NOT LIKE '%' + @Keyword + '%');
END;
GO

CREATE OR ALTER PROCEDURE sp_SelectMenuByAllergenFilter
    @Keyword NVARCHAR(100),
    @ShouldContain BIT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT p.ProductId, p.Name, c.Name AS CategoryName, p.UnitPrice
      FROM Products p
      INNER JOIN Categories c ON c.CategoryId = p.CategoryId
      LEFT JOIN ProductAllergens pa ON pa.ProductId = p.ProductId
      LEFT JOIN Allergens a ON a.AllergenId = pa.AllergenId
     WHERE (@ShouldContain = 1 AND a.Name LIKE '%' + @Keyword + '%')
        OR (@ShouldContain = 0 AND (a.Name IS NULL OR a.Name NOT LIKE '%' + @Keyword + '%'));
END;
GO

CREATE OR ALTER PROCEDURE sp_InsertOrder
    @OrderCode NVARCHAR(40),
    @UserId INT,
    @FoodCost DECIMAL(10,2),
    @ShippingCost DECIMAL(10,2),
    @DiscountValue DECIMAL(10,2),
    @TotalCost DECIMAL(10,2),
    @EstimatedDeliveryAt DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Orders (OrderCode, UserId, FoodCost, ShippingCost, DiscountValue, TotalCost, EstimatedDeliveryAt, Status)
    VALUES (@OrderCode, @UserId, @FoodCost, @ShippingCost, @DiscountValue, @TotalCost, @EstimatedDeliveryAt, 'registered');
END;
GO

CREATE OR ALTER PROCEDURE sp_InsertOrderLine
    @OrderId INT,
    @ProductId INT,
    @Quantity INT,
    @UnitPrice DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO OrderLines (OrderId, ProductId, Quantity, UnitPrice)
    VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice);
END;
GO

CREATE OR ALTER PROCEDURE sp_SelectOrdersByClient
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT o.OrderCode, o.CreatedAt, o.FoodCost, o.ShippingCost, o.DiscountValue, o.TotalCost, o.EstimatedDeliveryAt, o.Status
      FROM Orders o
     WHERE o.UserId = @UserId
     ORDER BY o.CreatedAt DESC;
END;
GO

CREATE OR ALTER PROCEDURE sp_SelectOrdersForEmployees
    @ActiveOnly BIT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT o.OrderCode,
           o.CreatedAt,
           o.FoodCost,
           o.ShippingCost,
           o.DiscountValue,
           o.TotalCost,
           o.EstimatedDeliveryAt,
           o.Status,
           u.FirstName,
           u.LastName,
           u.Phone,
           u.DeliveryAddress
      FROM Orders o
      INNER JOIN Users u ON u.UserId = o.UserId
     WHERE (@ActiveOnly = 0)
        OR (@ActiveOnly = 1 AND o.Status NOT IN ('delivered','cancelled'))
     ORDER BY o.CreatedAt DESC;
END;
GO

CREATE OR ALTER PROCEDURE sp_SelectLowStockDishes
    @Threshold DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Name, TotalQuantity
      FROM Products
     WHERE ProductType = 'Dish'
       AND TotalQuantity <= @Threshold
     ORDER BY Name;
END;
GO
