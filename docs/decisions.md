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
> **Partly superseded by Decision 37** — the earning rate is now 3 points per dinar.
> Everything else below (redemption rate, minimum, earning on cash paid) still stands.
> The original text is kept unedited on purpose.

- **Decision:** Earning: 5 points per dinar paid in cash, rounded down (floor). Redemption: every 100 points = 1 JOD. Minimum per redemption = 250 points; any amount at or above that is allowed. Points are earned on the cash paid after the points discount — not on the full invoice.
- **Rejected alternative:** Earning on the full invoice / redemption with no minimum.
- **Why:** Earning points from points is a losing loop. The minimum prevents trivial redemptions that complicate the cashier's work. (Shop owner's decision.)
- **Documented note:** The 250-point minimum effectively makes redemption impossible on orders under 2.5 JOD. The shop owner is aware of this and chose it deliberately (encourages larger orders). Not a bug.

## 10. Order Number Semantics
> **Partly superseded by Decision 38** — `PointsTransaction.OrderId` is now nullable for
> the `OpeningBalance` type. Everything else below still stands.
> The original text is kept unedited on purpose.

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
- **Decision:** Customer token: 365 days (repeated OTP kills the app). Employee token: 12 hours (login at shift start). No refresh tokens. Both durations live in LoyaltyConstants/appsettings.
- **Rejected alternative:** Refresh tokens with a short access token.
- **Superseded in part by Decision 25:** the "can't be revoked" clause below no longer
  holds — tokens are now revocable via `TokenVersion`. Both lifetimes here are unchanged.
- **Why:** Refresh tokens are the industry-correct answer, but their cost (table + endpoint + rotation + Flutter interceptor ≈ 2 days) doesn't match the data sensitivity (coffee-shop points balance) or the project timeline. Documented accepted risk: a stolen customer token is valid up to 365 days and can't be revoked. Upgradeable later without breaking anything. The customer lifetime was raised from 60 to 365 days because a stolen customer token is low-risk (points balance only) and convenience is preferred; the employee token stays 12 hours because there is no token revocation.

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

## 25. Token Revocation via `TokenVersion`
- **Decision:** `Customer` and `Employee` each gain an `int TokenVersion` column
  (default 0, migration `AddTokenVersion`). `JwtTokenService` writes the row's current
  value into the token as the `tv` claim at issue time. On **every authenticated
  request**, `JwtBearerEvents.OnTokenValidated` loads the user by `sub` and fails the
  authentication when `tv` ≠ the stored `TokenVersion` — so incrementing a user's
  `TokenVersion` immediately invalidates every token already issued to them.
  The same read also re-checks `Employee.IsActive`, which is the per-request check
  Decision 18 asked for.
  - **No endpoint exposes `TokenVersion`.** It is bumped by an admin action or a manual
    `UPDATE` — deliberately, so there is no anonymous or self-service way to churn
    tokens (consistent with Decision 12's "one writer" principle).
  - A failed check returns the existing `401 UNAUTHORIZED` body via `OnChallenge`. No new
    error code: the reason a token is dead (revoked, deactivated, deleted) is exactly
    what we do not want to tell an unauthenticated caller. Decision 18's
    `EMPLOYEE_DEACTIVATED` code stays unemitted and is not in the contract's registry;
    adding it later would be an additive change, not a breaking one.
  - **Both lifetimes from Decision 15 are unchanged** — customer 365 days, employee 12 hours.
- **Rejected alternative:** Staying fully stateless and accepting that a leaked token is
  valid until it expires / switching to short access tokens plus refresh tokens.
- **Why:** Decision 15 accepted "a stolen customer token is valid up to 365 days and
  can't be revoked" as a documented risk. A year is long enough that the risk needed a
  lever, and this is the cheapest one that exists: one integer column, one claim, no new
  table, no rotation protocol, no client-side interceptor — i.e. none of the ~2 days of
  refresh-token work Decision 15 rejected. It also closes the deactivated-employee gap
  Decision 18 described for free, since the check needs the same row read either way.
- **Accepted cost (deliberate):** this makes authentication **stateful** — one indexed
  primary-key read per authenticated request, which is precisely the property Decision 15
  chose to avoid. Accepted by the maintainer on the grounds that the API already hits the
  database on essentially every endpoint, so one extra keyed read is noise next to the
  query the request was going to run anyway. If that stops being true, the check can be
  cached per user with a short TTL, trading revocation latency for reads.

## 26. Rate Limiting: Auth Endpoints Only
- **Superseded by Decision 27.** The IP-partitioned middleware described here was removed
  before it ever reached production; the "Known limitation" below is exactly why. Kept for
  the reasoning on *which* endpoints to protect and on rejecting per-username lockout,
  both of which still hold.
- **Decision:** `POST /api/auth/login` and `POST /api/auth/firebase-login` are limited to
  **10 requests per client IP per minute** (fixed window, no queue) using ASP.NET Core's
  built-in `AddRateLimiter` — no third-party package. A rejection returns **429** in the
  standard error body with the new code `TOO_MANY_REQUESTS`. No other endpoint is limited.
- **Rejected alternative:** A global limiter across the whole API / per-username lockout
  after N failed logins / no limiting at all.
- **Why:** These two are the only anonymous endpoints that authenticate anybody, so they
  are the only ones worth brute-forcing, and neither is on a hot path — a cashier logs in
  once a shift and a customer once a year (Decision 15). A global limiter would throttle
  the cashier screen mid-service for no security gain. Per-username lockout was rejected
  because it hands an attacker a denial-of-service: guessing wrong at a known username
  repeatedly would lock a real cashier out at the counter.
- **Known limitation (documented, not fixed):** the partition key is
  `RemoteIpAddress`. Behind MonsterASP.NET's reverse proxy that is the **proxy's**
  address, so every caller would share one partition and the limit would effectively
  apply to the whole shop. Fixing it needs `UseForwardedHeaders` configured with the
  proxy's actual address (`KnownProxies` / `KnownNetworks`); it is left as a `TODO` in
  `AuthSetupExtensions` rather than guessed, because clearing `KnownProxies` to make
  `X-Forwarded-For` "just work" would let any caller spoof the header and bypass the
  limit entirely — strictly worse than the current state.
  *(This limitation is what Decision 27 resolves; the TODO no longer exists in the code.)*

## 27. Auth Throttling: Per Account, Not Per IP
- **Decision:** Brute-force protection on the two auth endpoints moves out of the
  rate-limiter middleware and into `IAuthService`, keyed on the **submitted account
  identifier**. `AddRateLimiter` / `UseRateLimiter` / `[EnableRateLimiting]` and the
  forwarded-headers TODO are removed. `TOO_MANY_REQUESTS` (429) is unchanged as a
  contract code, but is now raised as `ApiException.TooManyRequests()` from the service
  instead of written by the limiter's `OnRejected`.
- **Mechanism:** `ILoginThrottle` / `LoginThrottle` over `IMemoryCache` (built in — no
  new package). `MaxFailures` failures inside a rolling `FailureWindow` block the key for
  `BlockDuration`; a successful login calls `Reset`. Defaults 5 / 15 min / 15 min, bound
  from the `LoginThrottle` section of appsettings via `LoginThrottleOptions` with
  fail-fast validation, mirroring `JwtOptions`. Nothing in the section is secret.
  - Employee key: `employee:` + username, trimmed and lower-cased, so padding or
    re-casing the username cannot buy a fresh counter.
  - Customer key: `customer:` + normalized E.164 phone, falling back to
    `customer-uid:` + Firebase uid when the token's phone claim is unusable.
  - **Failures are counted for identifiers that do not exist, identically to ones that
    do.** A username that never gets throttled would be a username an attacker knows is
    not real — that would undo the timing equalization from the previous commit through a
    different channel.
  - A blocked account is refused *before* the credential check, so the block is not an
    oracle for "that password guess was correct".
- **Rejected alternative:** Keeping the IP limiter / configuring `ForwardedHeaders` with
  MonsterASP.NET's proxy address and keeping IP partitioning.
- **Why:** The rate-limiter middleware runs before model binding, so it can only
  partition on transport data — in practice the client IP. Behind the MonsterASP.NET
  reverse proxy that IP is the **proxy's**, so all traffic collapsed into one shop-wide
  bucket: an attacker got the whole allowance, and ordinary staff could be locked out by
  someone else's attempts. There are real users, so shipping that was not acceptable.
  Fixing it via `ForwardedHeaders` would have made brute-force protection depend on
  correctly enumerating the host's proxy addresses — a value we do not control and which
  fails open (spoofable `X-Forwarded-For`) if configured loosely. Throttling on the
  submitted identifier needs none of that: it is proxy-independent, survives a hosting
  change, and matches the actual threat, which is repeated guessing against a **specific
  account** rather than volume from a specific socket.
  Note this is not the per-username lockout Decision 26 rejected: that concern was a
  permanent, admin-unlock lockout weaponizable for denial of service. This block expires
  on its own after `BlockDuration`, so the worst an attacker achieves is delaying one
  cashier by 15 minutes — and the same attacker on the old IP limiter could delay
  *everyone*.
- **Accepted limitations (deliberate):**
  - **In-process store.** Counts live in this instance's memory: they reset on restart or
    redeploy, and are not shared if the API is ever scaled out. At single-instance,
    single-café scale this is adequate; a distributed cache is the upgrade path and needs
    no call-site change, only a different `ILoginThrottle`.
  - **Credential stuffing across many usernames is not stopped** — an attacker rotating
    identifiers never exhausts any single counter. Catching that needs volume-based
    limiting, which needs a trustworthy client IP, which is the problem this decision
    exists to avoid. Accepted.
  - **An unverifiable Firebase token is not throttled**, because it carries no identifier
    to count against. Not a real gap: the token is RS256-signed by Google, so it must be
    forged rather than guessed.

## 28. Unique-Constraint Races: Confirm, Then Translate
- **Decision:** Inserts guarded by a unique index (customer phone number, employee
  username) keep their up-front duplicate check, but also catch `DbUpdateException` on
  `SaveChangesAsync`. The handler detaches the failed entity, **re-queries for the
  conflicting row, and only translates the failure into the duplicate error**
  (`PHONE_ALREADY_EXISTS` / the username-taken `VALIDATION_ERROR`) once that row is
  confirmed to exist. If it does not, the original exception is re-thrown and becomes a
  500. Mirrors the race handling already in `AuthService.FindOrCreateCustomerAsync`.
- **Rejected alternative:** Trusting the pre-check alone / translating every
  `DbUpdateException` on these paths into the duplicate error without confirming.
- **Why:** The pre-check is not a lock — two cashiers registering the same number at once
  both pass it, and the index rejects the loser, who would otherwise get a 500 for a
  perfectly ordinary conflict. But translating blindly is worse than the bug it fixes: a
  connection failure, a check-constraint violation or a truncation error would all be
  reported to the client as "this phone is already registered", hiding a real defect behind
  a plausible message. Confirming the row costs one keyed read on a path that has already
  failed.

## 29. Absent vs. Invalid in Request DTOs
- **Decision:** Every request field whose type has a usable default — enums and `bool` —
  is declared **nullable and `[Required]`** (`UnitType?`, `ProductCategory?`, `IsAvailable`,
  `IsActive`). An omitted field is therefore a `VALIDATION_ERROR`, and the service reads
  `.Value` knowing model validation has already run.
- **Rejected alternative:** Non-nullable properties, letting the binder fill in the
  default.
- **Why:** A non-nullable field has no "absent" state. An omitted `category` would bind to
  member 0 (`HotCoffee`) and be saved as if the admin had chosen it; an omitted
  `isAvailable` would bind to `false` and quietly take a product off the menu nobody asked
  to change. The client's mistake must not become the server's silent decision.

## 30. Enum Wire Formats: Converter for Products, Explicit Mapping for Roles
- **Decision:** A global `JsonStringEnumConverter` is registered in `AddControllers`, so
  `UnitType` and `ProductCategory` travel as their **member names** (`"Piece"`,
  `"HotCoffee"`) in both directions — the same strings `HasConversion<string>()` stores.
  `EmployeeRole` is deliberately excluded: it is typed as a `string` on both the request
  and the response DTO and mapped by hand through `RoleNames.ToWireString` /
  `RoleNames.TryParseWireString` (case-insensitive; unknown ⇒ `VALIDATION_ERROR`).
- **Rejected alternative:** Letting the converter handle `EmployeeRole` too, with a
  camelCase/lowercase naming policy / mapping the product enums by hand as well.
- **Why:** The role's wire vocabulary is the lowercase one from Decision 25's JWT claim
  (`"cashier"`), which does not match the enum member names, and it is the vocabulary the
  authorization policies are built from. A naming policy would make that contract a
  side-effect of member spelling: renaming `EmployeeRole.Cashier` would silently change
  what clients are allowed to send *and* what the `role` claim says. The product enums have
  no such second vocabulary — name, stored value and wire value are one string — so the
  converter is exactly right for them and hand-mapping would be noise.

## 31. Product Price Validated Against the Column, Not by It
- **Decision:** `price` is validated before it reaches SQL Server: `Range` enforces
  `> 0` and an upper bound of `999999999999999.999`, and the service rejects any value with
  more than 3 decimal places. Both failures are `VALIDATION_ERROR`. Documented in the
  contract's §3.
- **Rejected alternative:** Letting `decimal(18,3)` take whatever arrives.
- **Why:** The column silently **rounds** a fourth decimal and **overflows** on an
  oversized value. Rounding means the admin is shown a price the shop is not charging —
  a wrong number nobody is told about, which then gets snapshotted into orders
  (Decision 3) and is unrecoverable. Overflow means an unhandled 500 instead of the
  contract's error body. Neither is something to discover in production; both are one
  attribute and one comparison to prevent.

## 32. Soft-Deleted Products Are Unaddressable
- **Decision:** Every product write route (`PUT`, `DELETE`, `PATCH .../availability`)
  loads the row with `IsActive == true` and otherwise raises `PRODUCT_NOT_FOUND` — the
  registry's wording, "no *active* product with that id". A soft-deleted product is
  therefore invisible to the whole API, and **there is no way to bring one back through
  it**.
- **Rejected alternative:** Letting `PUT` operate on inactive rows / adding a reactivate
  route on the spot.
- **Why:** Decision 14 made `IsActive` the permanent, admin-level removal; a product that
  can be silently edited or resurrected through the ordinary edit form is not permanent
  removal, it is a second availability flag. The contract defines no reactivate route, so
  inventing one here would put a shape in the API that no client was promised. If the shop
  needs it, it is an additive endpoint and a decision of its own — the data is still there.

## 33. Multi-Role Endpoints Use a Role List, Not a New Policy
- **Decision:** `GET /api/products` is the one endpoint open to all three roles at once.
  It is authorized with `[Authorize(Roles = ...)]` built from the same `RoleNames`
  constants, rather than by registering a fourth policy. Every single-level endpoint keeps
  `[Authorize(Policy = RoleNames.Xxx)]`.
- **Rejected alternative:** Stacking the three existing policy attributes / adding an
  `anyRole` policy to `AuthSetupExtensions` / a bare `[Authorize]`.
- **Why:** Stacked policies combine with **AND**, so the three attributes together admit
  nobody — a trap worth stating out loud. A fourth policy would mean editing the
  authorization setup, which is reviewed security code, to express a rule used exactly
  once. A bare `[Authorize]` would admit any authenticated principal, including whatever
  role is added next; listing the roles means a future one has to be let in on purpose.

## 34. Employee Password: 8-Character Minimum
- **Decision:** `POST /api/employees` requires a password of **at least 8 characters**
  (upper bound 72, BCrypt's own limit — see the DTO). Shorter ⇒ `VALIDATION_ERROR`.
  Documented in the contract's §2. No complexity rules, no expiry, no reuse check.
- **Rejected alternative:** No minimum at all / a full complexity policy (classes,
  rotation, history).
- **Why:** These accounts are the dashboard's only door and the admin sets the initial
  password for someone else, so "1" must not be a valid choice. A minimum length is the
  one control that actually raises the guessing cost; complexity rules and forced rotation
  mostly produce written-down passwords and are not worth the friction at this scale.
  Note that Decision 27's throttle protects the *login*, not the password's quality —
  they are complementary, not substitutes.

## 35. Missing Employee: 404 `EMPLOYEE_NOT_FOUND`
- **Decision:** `PATCH /api/employees/{id}/status` against an unknown id returns
  **404 `EMPLOYEE_NOT_FOUND`**, added to the registry alongside `PRODUCT_NOT_FOUND` and
  `CUSTOMER_NOT_FOUND`. The self-deactivation refusal stays a 400 `VALIDATION_ERROR`.
- **Supersedes:** the initial implementation, which reported a missing employee as
  `VALIDATION_ERROR` with an Arabic message, because the registry had no code for it.
- **Rejected alternative:** Keeping the 400 stopgap / reusing `VALIDATION_ERROR` for both
  failures.
- **Why:** The stopgap made the same class of failure — "the id you named does not
  exist" — carry a different status and code depending on which entity was named, so a
  dashboard could not handle not-found once. Adding a code to the registry is an additive
  change no existing client breaks on. The two failures stay distinct because they are
  distinct: a missing id is a bad reference, self-deactivation is a well-formed request
  the rules forbid.

## 36. Identifiers Are Stored Trimmed, Not Case-Folded
- **Decision:** `Username` and every display name are `Trim()`-ed before insert; case is
  left exactly as typed. Uniqueness and login lookups rely on the column's
  case-insensitive collation. Phone numbers are the exception — they are rewritten to
  canonical E.164 by `JordanPhoneNumber` (Decision 11), because their spelling is genuinely
  ambiguous. `[Required]` accepts `" "`, so a whitespace-only name is rejected explicitly
  after trimming.
- **Rejected alternative:** Lower-casing usernames on write / storing them exactly as
  typed with no trim.
- **Why:** The collation already prevents `ahmad` and `Ahmad` from both existing, and
  `AuthService` compares the same way, so folding the case adds no safety — it only makes
  the stored name differ from what the admin typed and saw. Trimming is a different matter:
  a trailing space is invisible, would create a second account indistinguishable from the
  first on screen, and would then fail every login the admin is sure they typed correctly.

## 37. Earning Rate Lowered: 5 → 3 Points per Dinar
- **Decision:** Earning is **3 points per dinar of cash paid**, floored. `LoyaltyConstants.PointsPerDinar`
  goes from `5` to `3` and stays the single place the rate is written. Nothing else about the
  formula changes: redemption is still 100 points = 1 JOD with a 250-point minimum, and points
  are still earned on cash paid (Total − PointsRedeemed/100), never on the full invoice.
- **Supersedes:** the rate in Decision 9, which stays on record with its original text.
- **Rejected alternative:** Keeping 5 and reducing the redemption value instead / making the rate
  an admin-editable setting.
- **Why:** Shop owner's request — at 5 points per dinar the outstanding points liability grew
  faster than they were willing to carry. Lowering the earning rate is the change customers feel
  least: it slows accrual but never devalues points already earned, whereas moving the redemption
  rate would retroactively shrink every existing balance. It stays a compile-time constant rather
  than a setting because a rate that changes mid-day makes two orders in the same shift
  irreproducible, and past orders are not recalculated in any case (Decision 22).
- **Documented note:** Orders created before this change keep the points they earned at 5/dinar.
  `PointsEarned` is an immutable snapshot, so returns and cancellations on those orders claw back
  against their own recorded value — old and new orders can be reconciled side by side.

## 38. Nullable `PointsTransaction.OrderId` for Opening Balances
- **Decision:** `PointsTransaction.OrderId` becomes `int?`, and a new `OpeningBalance` transaction
  type is the **only** type allowed to leave it NULL. The rule is enforced in the database, not
  just in code, by the check constraint
  `CK_PointsTransaction_Order`: `[Type] = 'OpeningBalance' OR [OrderId] IS NOT NULL`.
- **Supersedes:** Decision 10's "OrderId NOT NULL — no exceptions", which stays on record with its
  original text, and its explicit rejection of a nullable OrderId.
- **Rejected alternative:** Keeping OrderId NOT NULL and inventing a synthetic zero-total "migration
  order" per customer to hang the opening balance on / seeding balances by writing
  `Customer.PointsBalance` directly with no transaction row.
- **Why:** Shop owner wants the balances from the old paper punch-cards imported so customers do not
  start from zero. Decision 10 rejected a nullable OrderId to block *manual adjustments* — a staff
  member granting arbitrary points. That reasoning is untouched: `OpeningBalance` is a one-time,
  admin-only, idempotent import behind an off-by-default flag, not a general adjustment facility,
  and the check constraint means no other type can ever slip through the same hole. Fake migration
  orders would have polluted every sales report with rows that were never sales; writing the balance
  column directly would have broken the system's core invariant
  (`PointsBalance == SUM(PointsTransaction.Amount)`) on day one.
- **Consequence for the unique index:** `UX_PointsTransaction_Order_Type` is refiltered to
  `[Type] <> 'Refund' AND [OrderId] IS NOT NULL`. SQL Server treats NULLs as equal in a unique
  index, so without the second clause only one customer in the whole system could ever hold an
  opening balance.
- **TODO (build with the Phase 6 import, not before):** add a filtered unique index
  `UX_PointsTransaction_Customer_OpeningBalance` on `CustomerId` with filter
  `[Type] = 'OpeningBalance'`, so the database itself refuses a second opening balance for a
  customer even if two concurrent import runs both pass the app-level "already imported?" check.
  The app-level check is a TOCTOU race; the index is the real guarantee.
