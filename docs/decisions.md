# Design Decisions — Coffee Loyalty System

> Each decision: what we chose / the rejected alternative / why.

## 1. Points Calculation
- **Decision:** Points are calculated automatically at order creation per the formula in Decision 9 (on the cash amount paid, not the order total). Decision 9 is the single source of truth for the formula.
- **Rejected alternative:** Fixed points per product, set manually by the admin.
- **Why:** Shop owner's request + eliminates manual entry for every new product.

## 2. PointsBalance Denormalized on Customer
- **Decision:** The balance is stored as a ready column on Customer, updated with every transaction.
- **Rejected alternative:** Computing it by summing all PointsTransactions on every query.
- **Why:** The balance is read on almost every screen — repeated summing is expensive. (Deliberate denormalization.)

## 3. Price Snapshot on OrderItem
- **Decision:** The unit price is copied into OrderItem at order creation (UnitPriceSnapshot).
- **Rejected alternative:** Reading the price from the Product table when displaying the order.
- **Why:** Catalog price changes must not rewrite order history — old invoices must show what was actually paid.

## 4. Dashboard Auth: No Self-Registration
- **Decision:** The admin account is seeded into the database with a BCrypt hash. Cashier accounts are created by the admin from the dashboard (`POST /api/employees`, admin-only).
- **Rejected alternative:** An open "sign up" page on the dashboard.
- **Why:** The dashboard is for shop staff only — open registration is a security hole.

## 5. App Auth: Firebase OTP + Token Exchange
- **Decision:** The customer verifies their phone number via Firebase Auth (OTP). The app exchanges the Firebase ID token for our own JWT via `POST /api/auth/firebase-login`. The backend verifies the token with the Firebase Admin SDK, finds the customer by phone number or creates them, and issues a JWT with role=customer.
- **Rejected alternative:** Building a manual OTP system in ASP.NET with a paid SMS service.
- **Why:** Firebase provides OTP for free (with test numbers for the demo), and the token exchange unifies authorization — the API deals with a single JWT type for all users.

## 6. Cancellation & Returns: Permanent Trace, Not Deletion
- **Decision:** A mistaken order is cancelled (Order.Status = Cancelled) and stays in the database. Partial returns via OrderItem.ReturnedQuantity. Earned points are reversed with a negative PointsTransaction of type Refund. If the order was partially paid with points (PointsRedeemed > 0), the redeemed points are returned to the customer's balance via a positive transaction of type RedeemReversal on full cancellation.
- **Rejected alternative:** Deleting the order / confiscating redeemed points on cancellation.
- **Why:** A money-and-points system never deletes history. Confiscating redeemed points on a cancelled order = the customer paid for nothing.

## 7. QR-Based Returns: Deferred as Nice-to-Have
- **Decision:** The backend accepts returns by OrderId without caring where it came from. The cashier finds the order by searching the customer's phone number. QR (display in customer app + scanner in dashboard) is added in week 3 if time allows.
- **Rejected alternative:** Building QR from the start as a requirement for returns.
- **Why:** QR is an interface on top of the core, not part of it — deferring it changes nothing in the backend and protects the timeline.

## 8. Returns Rejected When Balance Is Insufficient
- **Decision:** Before accepting a return, the backend verifies the customer's balance covers the points to be clawed back. If not — the return is rejected (400 with a clear message). The check and the deduction happen in one conditional SQL statement (see Decision 11) — never a separate pre-read check.
- **Rejected alternative:** Allowing a negative balance to be settled from future points.
- **Why:** Negative balances are exploitable (earn → spend → return) and the shop loses twice. Rule: PointsBalance ≥ 0 always. (Shop owner's decision.)

## 9. Points Formula: Earning & Redemption
- **Decision:** Earning: 5 points per dinar paid in cash, rounded down (floor). Redemption: every 100 points = 1 JOD. Minimum per redemption = 250 points; any amount at or above that is allowed. Points are earned on the cash paid after the points discount — not on the full invoice.
- **Rejected alternative:** Earning on the full invoice / redemption with no minimum.
- **Why:** Earning points from points is a losing loop. The minimum prevents trivial redemptions that complicate the cashier's work. (Shop owner's decision.)
- **Documented note:** The 250-point minimum effectively makes redemption impossible on orders under 2.5 JOD. The shop owner is aware of this and chose it deliberately (encourages larger orders). Not a bug.

## 10. Order Number Semantics
- **Decision:** Order.Total = full order value and always equals the sum of its lines. PointsRedeemed is a separate column. Cash paid is computed (Total − PointsRedeemed/100) and never stored. PointsTransaction links to the Order via OrderId (NOT NULL — every points movement has a justifying order, no exceptions).
- **Rejected alternative:** Total = cash paid after the discount / nullable OrderId for manual adjustments.
- **Why:** The rule "Total = sum of lines" is always auditable — breaking it reintroduces the number-mismatch problem. Manual adjustments were removed (Decision 12), so there's no longer any reason for a nullable OrderId — the schema itself forbids sourceless points movements.

## 11. Balance Updates & Firebase Identity Link
- **Decision:** Increases (Earn, RedeemReversal) use an atomic increment inside the DB transaction: `SET PointsBalance = PointsBalance + @delta`. Decreases (Redeem, Refund) use a single conditional atomic UPDATE: `SET PointsBalance = PointsBalance - @x WHERE Id = @id AND PointsBalance >= @x` with a rows-affected check; 0 rows = operation rejected. The check and the deduction are one statement, never a separate read-then-write (TOCTOU). App customers are linked by FirebaseUid (filtered unique index, nullable); the first link matches the phone number after normalizing to E.164. Rates live in LoyaltyConstants (PointsPerDinar=5, RedeemRate=100, MinRedeemPoints=250).
- **Rejected alternative:** Read-modify-write on the balance / separating the check from the deduction / permanent reliance on phone-number matching / rates buried in code.
- **Why:** Read-modify-write loses updates under concurrency even inside transactions, and separating the check from the deduction opens TOCTOU (double-submit from the cashier device). Raw phone matching is fragile (07 vs +9627). Rates will change at the shop owner's request.

## 12. Points Movement Sources: Orders Only, No Manual Adjustment
- **Decision:** Points change exclusively via: earning at order creation, redemption at payment, reversal at return/cancellation. No endpoint exists for manual point adjustment — not even for the admin. On return: the cashier returns the item, and the system claws back its earned points if the balance covers them (conditional atomic UPDATE); if not — the return is rejected. Never a negative balance.
- **Rejected alternative:** Admin privilege to adjust points manually.
- **Why:** Every additional writer on the balance is a potential conflict source, and a movement without an order breaks auditability. Narrowing to one source (orders) simplifies the system.

## 13. Points Expiry: None (deliberate decision)
- **Decision:** Points never expire. A customer's balance is a permanent liability on the shop until spent or reversed.
- **Rejected alternative:** Expiry after 12 months of inactivity.
- **Why:** Expiry requires point-age tracking (FIFO batches), a daily background job, and a new transaction type — 3-4 dev days for marginal benefit in a single small shop where accumulated "debt" is limited anyway. Revisitable if the system grows. (Shop owner approved — pending one explicit confirmation.)

## 14. Product Deletion: Soft Delete Only
- **Decision:** Two columns on Product: `IsAvailable` (out of stock today — temporary, cashier toggles) and `IsActive` (on the menu at all — permanent, admin toggles). `DELETE /api/products/{id}` performs `IsActive = false`. Never a physical delete — old OrderItems reference the product via FK.
- **Rejected alternative:** Physical delete / overloading IsAvailable for both meanings.
- **Why:** Physical delete breaks the FK or erases order history (violates Decision 6). Merging the two meanings corrupts reports and the admin screen (a deleted product resurrects via the "out of stock" screen).

## 15. JWT Expiry
- **Decision:** Customer token: 60 days (repeated OTP kills the app). Employee token: 12 hours (login at shift start). No refresh tokens. Both durations live in LoyaltyConstants/appsettings.
- **Rejected alternative:** Refresh tokens with a short access token.
- **Why:** Refresh tokens are the industry-correct answer, but their cost (table + endpoint + rotation + Flutter interceptor ≈ 2 days) doesn't match the data sensitivity (coffee-shop points balance) or the project timeline. Documented accepted risk: a stolen customer token is valid up to 60 days and can't be revoked. Upgradeable later without breaking anything.

## 16. Return Window: One Day
- **Decision:** Returns (partial or full cancellation) accepted only within one day of order creation, Jordan time. Afterwards the endpoint rejects with 400. Duration lives in LoyaltyConstants (ReturnWindowDays = 1), changeable at the shop owner's request.
- **Rejected alternative:** Open-ended returns with no time limit.
- **Why:** Café products are consumed immediately — a late return is physically meaningless and opens points/cash manipulation. The rule also shields the cashier: rejection comes from the system, not a personal call. Comparison in Jordan time, not UTC — otherwise evening orders get wrongly rejected.