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

## 3. Snapshot on OrderItem: Price and Name
- **Decision:** Both the unit price and the product name are copied into
  OrderItem at order creation (`UnitPriceSnapshot`, `ProductNameSnapshot`).
  The FK to Product is kept for reporting, but order display and receipts
  read the snapshot columns only.
- **Rejected alternative:** Reading price and name from the Product table
  when displaying the order / snapshotting the price alone.
- **Why:** Catalog changes must not rewrite order history — an old invoice
  must show what was actually sold at the price actually paid. A renamed
  product ("Espresso" → "Single Espresso") silently rewrites every past
  receipt if the name is read live, which is the same defect as reading a
  changed price. Snapshotting one field and not the other leaves the
  history half-protected.

## 4. Dashboard Auth: No Self-Registration
- **Decision:** The admin account is seeded into the database on first run
  with a BCrypt hash. The seed password is never written in source or in a
  committed `appsettings` file — it is read from .NET User Secrets in
  development and from an environment variable in production; the seeder
  fails fast with a clear error if it is absent. Cashier accounts are
  created by the admin from the dashboard (`POST /api/employees`,
  admin-only).
- **Rejected alternative:** An open "sign up" page on the dashboard /
  a hardcoded seed password committed to the repository.
- **Why:** The dashboard is for shop staff only — open registration is a
  security hole. And this repository is public: a committed seed password,
  hashed or not, is permanently exposed in git history even if removed
  later. Failing fast on a missing secret is preferable to silently
  seeding a default password nobody changes.

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
- **Decision:** Before accepting a return, the backend verifies the
  customer's balance covers the points to be clawed back. If not — the
  return is rejected (400, `INSUFFICIENT_POINTS_FOR_RETURN`). The check and
  the deduction happen in one conditional SQL statement (see Decision 11) —
  never a separate pre-read check.
- **Rejected alternative:** Allowing a negative balance to be settled from
  future points.
- **Why:** Negative balances are exploitable (earn → spend → return) and the
  shop loses twice. Rule: PointsBalance ≥ 0 always. (Shop owner's decision.)
- **Operational consequence (accepted):** A rejected return does not stop
  the physical refund — the cashier may hand the money back outside the
  system, leaving the order recorded as if never returned. The system
  protects the points ledger, not the cash drawer. The agreed procedure is
  that such cases are settled manually by the owner and are expected to be
  rare, since redeeming requires a 250-point minimum (Decision 9). Revisit
  if it occurs in practice.

## 9. Points Formula: Earning & Redemption
- **Decision:** Earning: 5 points per dinar paid in cash, rounded down (floor). Redemption: every 100 points = 1 JOD. Minimum per redemption = 250 points; any amount at or above that is allowed. Points are earned on the cash paid after the points discount — not on the full invoice.
- **Rejected alternative:** Earning on the full invoice / redemption with no minimum.
- **Why:** Earning points from points is a losing loop. The minimum prevents trivial redemptions that complicate the cashier's work. (Shop owner's decision.)
- **Documented note:** The 250-point minimum effectively makes redemption impossible on orders under 2.5 JOD. The shop owner is aware of this and chose it deliberately (encourages larger orders). Not a bug.

## 10. Order Number Semantics
- **Decision:** `Order.Total` = the full order value **at creation time**
  and always equals the sum of its lines as originally ordered
  (Σ Quantity × UnitPriceSnapshot). It is never recalculated afterwards —
  see Decision 22. `PointsRedeemed` is a separate column. Cash paid is
  computed (Total − PointsRedeemed/100) and never stored. PointsTransaction
  links to the Order via OrderId (NOT NULL — every points movement has a
  justifying order, no exceptions).
- **Rejected alternative:** Total = cash paid after the discount / nullable
  OrderId for manual adjustments / mutating Total on return.
- **Why:** The rule "Total = sum of lines as ordered" is always auditable —
  breaking it reintroduces the number-mismatch problem. Returns are recorded
  in `ReturnedQuantity`, not by editing Total, so the original transaction
  stays intact and the net value remains derivable. Manual adjustments were
  removed (Decision 12), so there's no reason for a nullable OrderId — the
  schema itself forbids sourceless points movements.

## 11. Balance Updates & Firebase Identity Link
- **Decision:** Increases (Earn, RedeemReversal) use an atomic increment
  inside the DB transaction: `SET PointsBalance = PointsBalance + @delta`.
  Decreases (Redeem, Refund) use a single conditional atomic UPDATE:
  `SET PointsBalance = PointsBalance - @x WHERE Id = @id AND PointsBalance >= @x`
  with a rows-affected check; 0 rows = operation rejected. The check and the
  deduction are one statement, never a separate read-then-write (TOCTOU).
  **The balance UPDATE and its matching PointsTransaction INSERT always
  execute inside a single database transaction — both commit or both roll
  back.** App customers are linked by FirebaseUid (filtered unique index,
  nullable); the first link matches the phone number after normalizing to
  E.164. Rates live in LoyaltyConstants (PointsPerDinar=5, RedeemRate=100,
  MinRedeemPoints=250).
- **Rejected alternative:** Read-modify-write on the balance / separating the
  check from the deduction / writing the balance and its transaction row as
  two independent operations / permanent reliance on phone-number matching /
  rates buried in code.
- **Why:** Read-modify-write loses updates under concurrency even inside
  transactions, and separating the check from the deduction opens TOCTOU
  (double-submit from the cashier device). A committed balance change whose
  transaction row failed to insert is unrecoverable corruption — the points
  are gone with no record of why — so the two writes must share one
  transaction boundary. Raw phone matching is fragile (07 vs +9627). Rates
  will change at the shop owner's request.

## 12. Points Movement Sources: Orders Only, No Manual Adjustment
- **Decision:** Points change exclusively via: earning at order creation, redemption at payment, reversal at return/cancellation. No endpoint exists for manual point adjustment — not even for the admin. On return: the cashier returns the item, and the system claws back its earned points if the balance covers them (conditional atomic UPDATE); if not — the return is rejected. Never a negative balance.
- **Rejected alternative:** Admin privilege to adjust points manually.
- **Why:** Every additional writer on the balance is a potential conflict source, and a movement without an order breaks auditability. Narrowing to one source (orders) simplifies the system.

## 13. Points Expiry: None (deliberate decision)
- **Decision:** Points never expire. A customer's balance is a permanent
  liability on the shop until spent or reversed. Confirmed with the owner.
- **Rejected alternative:** Expiry after 12 months of inactivity.
- **Why:** Expiry requires point-age tracking (FIFO batches), a daily
  background job, and a new transaction type — 3–4 dev days for marginal
  benefit in a single small shop where the accumulated liability is limited
  anyway. The rule is also easier to explain to customers, which matters more
  than the accounting nuance at this scale. Revisitable if the system grows
  to multiple branches.

## 14. Product Deletion: Soft Delete Only
- **Decision:** Two columns on Product: `IsAvailable` (out of stock today — temporary, cashier toggles) and `IsActive` (on the menu at all — permanent, admin toggles). `DELETE /api/products/{id}` performs `IsActive = false`. Never a physical delete — old OrderItems reference the product via FK.
- **Rejected alternative:** Physical delete / overloading IsAvailable for both meanings.
- **Why:** Physical delete breaks the FK or erases order history (violates Decision 6). Merging the two meanings corrupts reports and the admin screen (a deleted product resurrects via the "out of stock" screen).

## 15. JWT Expiry
- **Decision:** Customer token: 60 days (repeated OTP kills the app). Employee token: 12 hours (login at shift start). No refresh tokens. Both durations live in LoyaltyConstants/appsettings.
- **Rejected alternative:** Refresh tokens with a short access token.
- **Why:** Refresh tokens are the industry-correct answer, but their cost (table + endpoint + rotation + Flutter interceptor ≈ 2 days) doesn't match the data sensitivity (coffee-shop points balance) or the project timeline. Documented accepted risk: a stolen customer token is valid up to 60 days and can't be revoked. Upgradeable later without breaking anything.

## 16. Return Window: End of the Following Day
- **Decision:** Returns (partial or full cancellation) are accepted until the
  **end of the calendar day following the order**, in Jordan time
  (`Asia/Amman`, fixed UTC+3 — Jordan has no DST since 2022). Concretely, the
  deadline is `23:59:59.999` on `date(CreatedAt in Amman) + ReturnWindowDays`.
  Afterwards the endpoint rejects with 400 (`RETURN_WINDOW_EXPIRED`). The
  duration lives in LoyaltyConstants (`ReturnWindowDays = 1`), changeable at
  the shop owner's request.
  - Worked example: an order created 23:00 Sunday can be returned until
    23:59:59 Monday (≈25 hours). An order created 08:00 Sunday has until the
    same deadline (≈40 hours). The window is deliberately variable in length.
- **Rejected alternative:** A rolling 24-hour window measured from
  `CreatedAt` / open-ended returns with no time limit.
- **Why:** Café products are consumed immediately — a late return is
  physically meaningless and opens points/cash manipulation. The rule also
  shields the cashier: rejection comes from the system, not a personal call.
  A day boundary was chosen over a rolling 24 hours because staff and
  customers reason in days, not hours: "until the end of tomorrow" is a rule
  a cashier can state at the counter and a customer accepts, whereas "your
  24 hours ended at 15:47" invites an argument the cashier has to win. The
  variable window length (25–48 hours) is the accepted cost, and it always
  errs in the customer's favour — never shorter than a rolling day.
  Comparison in Jordan time, not UTC — otherwise late-evening orders get
  wrongly rejected.

## 17. Walk-in Orders: Out of Scope
- **Decision:** The system records orders for registered customers only
  (loyalty only). Regular walk-in orders are handled outside the system.
  Order.CustomerId stays NOT NULL.
- **Rejected alternative:** Nullable CustomerId to support orders without
  a customer.
- **Why:** This is a points system, not a full POS. Supporting walk-in
  customers changes validation, reports, and the cashier screen for zero
  loyalty benefit. (Shop owner's decision.)

## 18. Employee Deactivation: Soft Delete via IsActive
- **Decision:** `IsActive` column on Employee (default true). Deactivation via
  `PATCH /api/employees/{id}/status` (admin only). Login rejects a deactivated
  employee with 401. **In addition, every authenticated employee request
  re-checks `IsActive` against the database — the JWT alone is not sufficient
  proof of employment.** A request carrying a valid token for a deactivated
  employee is rejected with 401 (`EMPLOYEE_DEACTIVATED`). No physical delete —
  FK on Orders.
- **Rejected alternative:** Physical DELETE of the employee / checking
  IsActive at login only and relying on token expiry to lock the account out.
- **Why:** Physical delete breaks the FK on old orders or erases their history
  (violates Decision 6). Checking only at login leaves a real hole: employee
  tokens live 12 hours (Decision 15) and there are no refresh tokens to
  revoke, so a cashier terminated mid-shift keeps full API access — creating
  orders and redeeming points — until the token expires on its own. The
  per-request check makes deactivation take effect immediately, which is the
  entire point of the feature.  is a security hole.

  ## 19. Partial Returns: Cash-Paid Orders Only
- **Decision:** Partial returns are allowed only on orders where
  PointsRedeemed = 0. Orders paid (fully or partially) with points can
  only be fully cancelled (decision 6 covers full reversal). The API
  rejects with ORDER_PAID_WITH_POINTS.
- **Rejected alternative:** Proportional refund system — splitting each
  returned item's value between cash and points by the order's original
  payment ratio.
- **Why:** The proportional system needs rounding rules, a new
  transaction type, and multi-return drift handling (~1 week) for a
  scenario that is rare in a coffee shop. Full cancellation already
  covers the customer fairly. Deliberate, documented scope cut.

## 20. Order Status: Returned State & Transition Rule
- **Decision:** `OrderStatus` gains a third value, `Returned`. The first
  return on an order (any `OrderItem.ReturnedQuantity` moving above 0)
  transitions it `Completed → Returned`. This is a one-way move — the status
  stays `Returned` through every subsequent line-by-line return, whether the
  order ends up partially or fully returned, and never reverts to `Completed`.
  Whether a `Returned` order is partial or full is **derived** at read time
  (`ReturnedQuantity == Quantity` on every line), not stored as a separate
  state. Cancellation (`Status = Cancelled`) is accepted **only** from a
  `Completed` order with no returns; a `Returned` order can never be
  cancelled — the API rejects with `ORDER_ALREADY_RETURNED`.
- **Rejected alternative:** Keeping only `Completed`/`Cancelled` and inferring
  the returned state by scanning `ReturnedQuantity` at read time / adding a
  fourth `FullyReturned` value alongside `PartiallyReturned`.
- **Why:** An explicit status makes the returned state first-class and
  auditable, and it enforces the guard from Decision 6 at the state-machine
  level: allowing cancellation after a return would reverse the same points
  twice (once per the partial `Refund`, again on the full-cancellation
  `Refund`). Blocking `Cancelled` from `Returned` closes that double-reversal
  path structurally rather than relying on a runtime scan. A fourth
  `FullyReturned` value was rejected because it requires recomputing a
  whole-order condition on every single return and adds a transition that
  carries no rule of its own — the partial/full distinction is a display
  concern, not a state-machine concern. A single `Returned` value also avoids
  the naming lie of an order labelled "partially returned" when every item
  came back.
- **Cancelled vs fully returned:** Both end with the customer paid back in
  full, but they are deliberately different states and must stay
  distinguishable in reports. `Cancelled` means the sale never validly
  happened (wrong order, cashier error) and its lines keep
  `ReturnedQuantity = 0`. `Returned` means a real sale occurred and the goods
  came back afterwards, item by item, with `ReturnedQuantity` recording
  exactly what and how much. Revenue reporting excludes `Cancelled` orders
  entirely and nets `Returned` orders line by line (see Decision 23).

  ## 21. Balance Integrity Verification
- **Decision:** A verification query comparing `Customer.PointsBalance`
  against `SUM(PointsTransactions.Amount)` per customer is kept in
  `docs/verification.sql` and run manually before each demo and at the end of
  each development week. Any customer returned by it is a bug to be traced and
  fixed at its source; the balance is never silently patched.
- **Rejected alternative:** A scheduled background job that recalculates and
  corrects drifting balances automatically / no verification at all.
- **Why:** Denormalizing the balance (Decision 2) creates two sources of truth
  that must agree. Without a check, a bug corrupts balances silently and is
  discovered by a customer, not by the team. Auto-correction is worse than no
  check: it hides the defect while making the symptom disappear, so the same
  bug keeps producing corruption. A manual query is sufficient at single-shop
  scale and doubles as evidence of correctness at handover.

  ## 22. Order Totals Are Immutable Snapshots
- **Decision:** `Order.Total`, `Order.PointsEarned` and `Order.PointsRedeemed`
  record what happened **at order creation** and are never modified by a
  return or cancellation. The effect of a return lives entirely in
  `OrderItem.ReturnedQuantity` and in the reversing `PointsTransaction` rows.
  Net values are always derived, never stored:
  - `NetTotal = Σ ((Quantity − ReturnedQuantity) × UnitPriceSnapshot)`
  - `NetPointsEarned = PointsEarned + Σ (Refund transactions on this order)`
    (Refund amounts are negative, so this subtracts.)
- **Rejected alternative:** Decrementing `Total` and `PointsEarned` in place
  on each return so the row always reflects the current net value.
- **Why:** Mutating the original figures destroys the audit trail — the shop
  can no longer answer "what did this customer originally buy and earn?", and
  a bug in the decrement logic silently corrupts history with nothing to
  compare against. Keeping the original immutable and deriving the net keeps
  every number reconstructible from primary records, and matches the same
  principle already applied to `UnitPriceSnapshot` (Decision 3) and to never
  deleting orders (Decision 6).

  ## 23. Report Revenue Definition
- **Decision:** `GET /api/reports/summary` reports over `Order.CreatedAt`
  within the requested range, using these definitions:
  - `Cancelled` orders are excluded entirely from every figure.
  - `Completed` and `Returned` orders are included, with revenue netted line
    by line: `NetRevenue = Σ ((Quantity − ReturnedQuantity) × UnitPriceSnapshot)`.
  - `PointsIssued` and `PointsRedeemed` are computed from `PointsTransaction`
    rows in the range by `Type`, not from `Order` columns — reversals therefore
    reduce the totals automatically.
  - A return is attributed to the **original order's** date, not the return
    date, so a period's figures never change retroactively after it closes.
  - The response additionally exposes `OrdersCancelled` and `OrdersReturned`
    as counts, so the two outcomes stay visible instead of silently vanishing.
- **Rejected alternative:** Summing `Order.Total` for all non-cancelled orders /
  attributing returns to the date the return happened.
- **Why:** Summing `Total` overstates revenue by the full value of every
  returned item, which is the single most likely question at the project
  defense. Attributing returns to the return date makes closed periods mutate
  after the fact, so the same month reports different revenue depending on when
  it is queried. Deriving points from the transaction ledger rather than the
  order columns keeps the report consistent with Decision 22's rule that order
  figures are immutable.

## 24. Product Category as a Fixed Enum
- **Decision:** Product gains a `Category` field, stored as one of six
  fixed string values (HotCoffee, ColdCoffee, Mojito, Milkshake, Desserts,
  CoffeeBeans) via HasConversion<string>(), matching the UnitType pattern.
  Values are stored in English; the Flutter app and the React dashboard
  each map them to display labels.
- **Rejected alternative:** A free-text category string / a separate
  Categories table with a FK.
- **Why:** Free text lets the same section be spelled inconsistently and
  splits the menu into duplicates. A separate table with admin CRUD is
  real overhead (extra entity, endpoints, a management screen) for a fixed
  list that changes maybe once a year — deferred until the shop actually
  needs to edit categories. Storing English keeps the API payload and the
  enum consistent with the rest of the system; localisation is a display
  concern owned by the clients, not the backend.