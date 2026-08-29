/* ============================================================================
   Clears the development data before a deploy.

   DESTRUCTIVE. Everything transactional goes: orders, their lines, every points
   movement, every customer, and the product catalogue. Only the Employees table
   survives, because the admin account is what the dashboard is bootstrapped
   with and AdminSeeder will not recreate a password someone has since changed.

   A freshly migrated production database has nothing to delete, so this is a
   no-op there and is meant for the development database. Run it only when you
   intend to lose that data.

   Order of deletion follows the foreign keys: order lines and points movements
   both point at orders, and PointsTransaction -> Order is Restrict, not
   Cascade, so the movements must go before the orders they reference.

   After this, run import-opening-balances.sql, then have the shop enter the
   real menu from the dashboard -- the catalogue this drops was all test rows.
   ============================================================================ */

/* Required, not decoration: PointsTransactions carries filtered indexes, and SQL
   Server refuses any DML on such a table unless QUOTED_IDENTIFIER is ON. sqlcmd
   connects with it OFF, so without this line every DELETE below fails with
   Msg 1934. SSMS defaults it ON and would have hidden the problem. */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DELETE FROM OrderItems;
DELETE FROM PointsTransactions;
DELETE FROM Orders;
DELETE FROM Customers;
DELETE FROM Products;

/* Ids restart at 1. Cosmetic for the customers, but not for the orders: the
   customer reads an order number off the app and the cashier types it into the
   dashboard, and «طلب #22» on day one of a shop that has sold nothing reads as
   a bug. RESEED with 0 makes the next inserted row Id 1. */
DBCC CHECKIDENT ('OrderItems',        RESEED, 0);
DBCC CHECKIDENT ('PointsTransactions', RESEED, 0);
DBCC CHECKIDENT ('Orders',            RESEED, 0);
DBCC CHECKIDENT ('Customers',         RESEED, 0);
DBCC CHECKIDENT ('Products',          RESEED, 0);

COMMIT TRANSACTION;

/* All four must read 0; employees must read 1 or more. */
SELECT
    (SELECT COUNT(*) FROM Orders)             AS orders,
    (SELECT COUNT(*) FROM OrderItems)         AS order_items,
    (SELECT COUNT(*) FROM PointsTransactions) AS points_transactions,
    (SELECT COUNT(*) FROM Customers)          AS customers,
    (SELECT COUNT(*) FROM Products)           AS products,
    (SELECT COUNT(*) FROM Employees)          AS employees_kept;
