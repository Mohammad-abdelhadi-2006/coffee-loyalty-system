# Coffee Loyalty System — ERD

```mermaid
erDiagram
    Customer {
        int Id PK
        string FullName
        string PhoneNumber UK
        string FirebaseUid "nullable, filtered UK"
        int PointsBalance
        int TokenVersion
        datetimeoffset CreatedAt
    }
    Employee {
        int Id PK
        string FullName
        string Username UK
        string PasswordHash
        string Role
        bool IsActive
        int TokenVersion
        datetimeoffset CreatedAt
    }
    Product {
        int Id PK
        string Name
        decimal Price
        string UnitType
        string Category
        bool IsAvailable
        bool IsActive
    }
    Order {
        int Id PK
        int CustomerId FK
        int EmployeeId FK
        decimal Total
        int PointsEarned
        int PointsRedeemed
        string Status
        datetimeoffset CreatedAt
    }
    OrderItem {
        int Id PK
        int OrderId FK
        int ProductId FK
        string ProductNameSnapshot
        decimal Quantity
        decimal ReturnedQuantity
        decimal UnitPriceSnapshot
    }
    PointsTransaction {
        int Id PK
        int CustomerId FK
        int OrderId FK
        int Amount
        string Type
        datetimeoffset CreatedAt
    }

    Customer ||--o{ Order : places
    Employee ||--o{ Order : creates
    Order ||--|{ OrderItem : contains
    Product ||--o{ OrderItem : "appears in"
    Customer ||--o{ PointsTransaction : has
    Order ||--o{ PointsTransaction : generates
```

## Column Notes

- **Customer.PhoneNumber**: NOT NULL + regular UK — the phone number is the customer's identity in the system; a customer without a phone number does not exist. All numbers are stored strictly in E.164 format (`+9627XXXXXXXX`) after normalizing any manual input. The backend rejects any number that doesn't match a valid Jordanian format after normalization — strict validation on manual entry.
- **Customer.FirebaseUid**: nullable — linked on first firebase-login. Customers registered by the cashier keep a NULL UID until their first app login; their balance is preserved and linked automatically (matched via normalized PhoneNumber). Uniqueness is enforced with a **filtered unique index** (`WHERE FirebaseUid IS NOT NULL`) — SQL Server treats NULL as a value in a regular unique index, so any two cashier-registered customers would break it.
- **Customer.PointsBalance**: denormalized (performance decision). **Increases** (Earn, RedeemReversal): atomic increment inside the DB transaction — `SET PointsBalance = PointsBalance + @delta`. **Decreases** (Redeem, Refund): conditional atomic UPDATE — `SET PointsBalance = PointsBalance - @x WHERE Id = @id AND PointsBalance >= @x` — with a rows-affected check; 0 rows = operation rejected. The check and the deduction are one statement, never a separate read-then-write (TOCTOU). Guarantees PointsBalance ≥ 0 always, even under double-submit from the cashier device.
- **Product.UnitType**: `Piece` or `Kg` — determines whether Quantity is a count or a weight.
- **Product.Category**: `HotCoffee`, `ColdCoffee`, `Mojito`, `Milkshake`,
  `Desserts`, or `CoffeeBeans`. Stored as a string (same pattern as
  UnitType). Stored in English; the client maps each value to its display
  label.
- **Customer.TokenVersion / Employee.TokenVersion**: NOT NULL, default 0. Stamped into every JWT as the `tv` claim at issue time and re-checked against this column on every authenticated request; incrementing it invalidates all of that user's existing tokens. No endpoint exposes it — an admin action or a manual `UPDATE` bumps it. See decisions.md #25.
- **Product.IsAvailable**: out of stock today — temporary, toggled by the cashier.
- **Product.IsActive**: on the menu at all — permanent, toggled by the admin. `DELETE /api/products/{id}` sets this to false (soft delete). Physical DELETE never happens; old OrderItems keep their FK reference. See decisions.md #14.
- **Order.Status**: `Completed`, `Returned`, or `Cancelled`.
- **Status transition on return**: the first return on an order (any OrderItem.ReturnedQuantity moving above 0) transitions the order `Completed → Returned`. The status stays `Returned` for all subsequent line-by-line returns, whether the order ends up partially or fully returned; it is a **one-way** move (a `Returned` order never goes back to `Completed`). Whether a `Returned` order is partial or full is *derived* at read time (`ReturnedQuantity == Quantity` on every line), not stored. See decisions.md #20.
- **Cancellation (Status = Cancelled)**: allowed **only if** the order is still `Completed` with no return (every OrderItem.ReturnedQuantity = 0) — otherwise points would be reversed twice. A `Returned` order can **never** be cancelled (the API rejects it); once any return happens, the only path is completing returns line by line. Cancellation reverses points in both directions: claws back PointsEarned (`Refund`, negative) and restores PointsRedeemed (`RedeemReversal`, positive), both linked to the same OrderId. `Cancelled` (sale never validly happened, lines keep `ReturnedQuantity = 0`) and a fully `Returned` order (real sale, goods came back item by item) stay distinguishable in reports. Partial-return details and calculations live in the api-contract (Returns section, week 2).
- **Return window**: returns and cancellations are accepted until the **end of the calendar day following the order** in Jordan time (`Asia/Amman`, UTC+3) — i.e. the deadline is `23:59:59.999` on `date(CreatedAt in Amman) + ReturnWindowDays`, **not** a rolling 24 hours from CreatedAt. So the usable window varies (~25–48h) and always errs in the customer's favour. Past the deadline the endpoint rejects with `RETURN_WINDOW_EXPIRED`. `ReturnWindowDays = 1`. See decisions.md #16.
- **Order.Total**: full order value — always equals the sum of its lines (`Σ Quantity × UnitPriceSnapshot`). Cash paid is **computed**, never stored.
- **Order.PointsRedeemed**: points spent on this order — defaults to 0. Subject to the redemption constraints below; any order request violating them is rejected with a validation error before anything is written.
- **Order.PointsEarned**: calculated on cash paid only, not on Total (decision 9).
- **OrderItem.Quantity**: decimal to support both weight (1.5 kg) and count (2 pieces).
- **OrderItem.ReturnedQuantity**: defaults to 0 — supports partial returns.
- **OrderItem.ProductNameSnapshot**: product name at order time (`nvarchar(100)`, NOT NULL) — frozen alongside the price, so a later product rename never rewrites past receipts. Order display and receipts read this column, not `Product.Name`; the FK to Product is kept only for reporting. See decisions.md #3.
- **OrderItem.UnitPriceSnapshot**: unit price at order time — frozen, unaffected by catalog price changes.
- **PointsTransaction.OrderId**: nullable **only** for `OpeningBalance`; NOT NULL for every other type, enforced by the check constraint `CK_PointsTransaction_Order` (`[Type] = 'OpeningBalance' OR [OrderId] IS NOT NULL`). Manual adjustments were removed by design (decisions.md #12), so the schema still forbids sourceless points movements — the one-time legacy import is the single documented exception (decisions.md #38, supersedes #10's NOT NULL rule).
- **PointsTransaction.Type**: `Earn` / `Redeem` / `Refund` / `RedeemReversal` / `OpeningBalance`. `Refund` is always negative (clawing back earned points on return/cancellation); `RedeemReversal` is always positive (restoring spent points on cancellation); `OpeningBalance` is positive and appears at most once per customer (decisions.md #38). Kept as separate types so reports can distinguish them.
- **PointsTransaction.Amount**: the sign reflects the effect on the balance (positive = increase, negative = decrease) — Type describes the reason, not the sign.
- **Rates**: `PointsPerDinar = 3` (lowered from 5 — decisions.md #37), `RedeemRate = 100`, `MinRedeemPoints = 200` (lowered from 250 — decisions.md #43), and `ReturnWindowDays = 1` live in `LoyaltyConstants` — one place, not buried in formulas.
- **CreatedAt** (Customer, Employee, Order, PointsTransaction): stored as `DateTimeOffset` (SQL Server `datetimeoffset`), not `DateTime`. The offset is preserved so an instant is unambiguous regardless of server timezone; return/cancellation windows are then compared in **Jordan time** by converting from the stored offset (see the Return window note above).
- **Employee.IsActive**: defaults to true. Deactivating an employee
  (resignation/termination) is done via `IsActive = false` through
  `PATCH /api/employees/{id}/status` (admin only) — never a physical
  delete; the employee is referenced by old Orders via FK (same soft-delete
  logic as products, decision 14). Login rule: `IsActive = false` → rejected
  with 401.

## Database CHECK Constraints (last line of defense)

Enforced in the initial EF Core migration — if any future bug tries to break these rules, the database throws instead of silently storing corruption:

```sql
ALTER TABLE Customers ADD CONSTRAINT CK_Customer_Balance
    CHECK (PointsBalance >= 0);

ALTER TABLE OrderItems ADD CONSTRAINT CK_OrderItem_Returned
    CHECK (ReturnedQuantity >= 0 AND ReturnedQuantity <= Quantity);

ALTER TABLE Orders ADD CONSTRAINT CK_Order_Points
    CHECK (PointsEarned >= 0 AND PointsRedeemed >= 0);
```

## Core Formulas

- Order total: `Total = Σ (Quantity × UnitPriceSnapshot)`
- Cash paid: `CashPaid = Total − (PointsRedeemed / RedeemRate)`
- Points earned: `PointsEarned = floor(CashPaid × PointsPerDinar)`

`Total`, `PointsEarned`, and `PointsRedeemed` are immutable snapshots of order creation and are **never** rewritten by a return or cancellation (decisions.md #22). The current net value after returns is always *derived*, never stored:

- Net total: `NetTotal = Σ ((Quantity − ReturnedQuantity) × UnitPriceSnapshot)`
- Net points earned: `NetPointsEarned = PointsEarned + Σ (Refund transactions on this order)` — `Refund` amounts are negative, so this subtracts the clawed-back points.

### Redemption Constraints (validated at order creation)

- `PointsRedeemed = 0` **or** `PointsRedeemed ≥ MinRedeemPoints (200)` — shop owner's minimum per redemption. Effectively no redemption on orders under 2 JOD — deliberate (encourages larger orders), not a bug. See decisions.md #9, minimum lowered by #43.
- `PointsRedeemed ≤ Customer.PointsBalance` — enforced via the conditional atomic UPDATE described in the PointsBalance note, not a separate pre-read check.
- `PointsRedeemed / RedeemRate ≤ Total` — CashPaid can never go negative; otherwise the formulas above would produce a negative PointsEarned and the system would *deduct* points from a paying customer.

> Worked example: order Total = 6 JOD, PointsRedeemed = 250 → CashPaid = 6 − (250/100) = 3.5 JOD → PointsEarned = floor(3.5 × 3) = floor(10.5) = **10 points**