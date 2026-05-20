USE RestaurantDB;
GO

-- 1. Procedură pentru crearea Antetului Comenzii (OrderID)
CREATE PROCEDURE sp_CreateOrder
    @ClientID INT,
    @TotalCost DECIMAL(18, 2),
    @Status NVARCHAR(50),
    @OrderID INT OUTPUT
AS
BEGIN
    INSERT INTO Orders (ClientID, TotalCost, Status, OrderDate, OrderCode)
    VALUES (@ClientID, @TotalCost, @Status, GETDATE(), NEWID());

    -- Returnăm ID-ul comenzii tocmai create pentru a-l folosi la produse
    SET @OrderID = SCOPE_IDENTITY();
END;
GO

-- 2. Procedură pentru adăugarea produselor și SCĂDEREA STOCULUI
CREATE PROCEDURE sp_AddOrderItemAndReduceStock
    @OrderID INT,
    @ProductID INT,
    @Quantity INT
AS
BEGIN
    -- Inserăm produsul în detaliile comenzii
    INSERT INTO OrderItems (OrderID, ProductID, Quantity)
    VALUES (@OrderID, @ProductID, @Quantity);

    -- Scădem cantitatea din stocul restaurantului (TotalQuantity)
    UPDATE Products
    SET TotalQuantity = TotalQuantity - @Quantity
    WHERE ProductID = @ProductID;
END;
GO