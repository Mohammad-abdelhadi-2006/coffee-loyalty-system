# API Contract — Coffee Loyalty System

> The single source of truth for request/response shapes between the backend (Mohammad),
> the dashboard (Samer), and the Flutter app. Formulas and business rules live in
> `decisions.md` and `ERD.md` — this file defines **shapes only** and never repeats rules.
>
> Base URL: `/api`. All bodies are JSON. All dates are ISO 8601 UTC unless stated otherwise.

---

## Conventions

### Auth levels

| Level | Meaning |
|---|---|
| `public` | No token required |
| `customer` | JWT with `role=customer` (from firebase-login) |
| `cashier` | JWT with `role=cashier` **or** `role=admin` |
| `admin` | JWT with `role=admin` only |

Token is sent as `Authorization: Bearer <jwt>`.
Missing/invalid token → `401`. Valid token, wrong role → `403`.

### Unified error shape

Every non-2xx response has exactly this body:

```json
{ "code": "MACHINE_READABLE_CODE", "message": "نص عربي للعرض المباشر" }
```

- `code` — stable English constant. The dashboard builds logic on it. **Never changes.**
- `message` — Arabic display text. May change freely.

### Error code registry

| Code | Status | When |
|---|---|---|
| `INVALID_CREDENTIALS` | 401 | Wrong username/password |
| `ACCOUNT_DISABLED` | 401 | Employee `IsActive = false` (decision 18) |
| `INVALID_FIREBASE_TOKEN` | 401 | Firebase ID token failed verification |
| `INVALID_PHONE` | 400 | Not a valid Jordanian number after E.164 normalization |
| `PHONE_ALREADY_EXISTS` | 409 | Registering a phone that already exists |
| `CUSTOMER_NOT_FOUND` | 404 | No customer with that id/phone |
| `PRODUCT_NOT_FOUND` | 404 | No active product with that id |
| `PRODUCT_UNAVAILABLE` | 400 | Product exists but `IsAvailable = false` |
| `INVALID_QUANTITY` | 400 | Quantity ≤ 0, or a count for a `Piece` product isn't a whole number |
| `REDEEM_BELOW_MINIMUM` | 400 | `0 < pointsRedeemed < 250` (decision 9) |
| `INSUFFICIENT_BALANCE` | 400 | `pointsRedeemed >` customer balance |
| `REDEEM_EXCEEDS_TOTAL` | 400 | `pointsRedeemed / 100 >` order total (ERD constraint) |
| `ORDER_NOT_FOUND` | 404 | No order with that id |
| `ORDER_ALREADY_CANCELLED` | 400 | Cancel/return on a cancelled order |
| `ORDER_HAS_RETURNS` | 400 | Full cancel after a partial return happened (ERD rule) |
| `RETURN_WINDOW_EXPIRED` | 400 | Past end of the day following the order, Jordan time (decision 16) |
| `ITEM_NOT_IN_ORDER` | 400 | `orderItemId` doesn't belong to this order |
| `RETURN_EXCEEDS_QUANTITY` | 400 | Returned qty > remaining (Quantity − ReturnedQuantity) |
| `INSUFFICIENT_BALANCE_FOR_RETURN` | 400 | Claw-back would push balance below zero (decision 8) |
| `ORDER_PAID_WITH_POINTS` | 400 | Partial return on an order where `PointsRedeemed > 0` (decision 19) |
| `VALIDATION_ERROR` | 400 | Any other malformed request body |

---

## 1. Auth

### POST /api/auth/login
Employee login (dashboard).

- **Auth:** public
- **Request:**
```json
{ "username": "ahmad", "password": "..." }
```
- **Success 200:**
```json
{
  "token": "eyJ...",
  "fullName": "أحمد",
  "role": "cashier",
  "expiresAt": "2026-07-15T20:00:00Z"
}
```
- **Errors:** `INVALID_CREDENTIALS`, `ACCOUNT_DISABLED`

### POST /api/auth/firebase-login
Customer token exchange (app). Backend verifies the Firebase ID token,
finds the customer by normalized phone, or creates one (decision 5).

- **Auth:** public
- **Request:**
```json
{ "firebaseIdToken": "eyJ...", "fullName": "محمد" }
```
  `fullName` is **used only when creating a new customer** on first login;
  ignored if the customer already exists.
- **Success 200:**
```json
{
  "token": "eyJ...",
  "fullName": "محمد",
  "pointsBalance": 340,
  "expiresAt": "2026-09-13T10:00:00Z"
}
```
- **Errors:** `INVALID_FIREBASE_TOKEN`, `INVALID_PHONE` (Firebase phone not a valid Jordanian number)

---

## 2. Employees

### GET /api/employees
- **Auth:** admin
- **Success 200:**
```json
[
  { "id": 1, "fullName": "أحمد", "username": "ahmad", "role": "cashier", "isActive": true, "createdAt": "..." }
]
```

### POST /api/employees
Create a cashier account (decision 4).

- **Auth:** admin
- **Request:**
```json
{ "fullName": "أحمد", "username": "ahmad", "password": "...", "role": "cashier" }
```
- **Success 201:** the created employee object (same shape as GET, no password).
- **Errors:** `VALIDATION_ERROR` (username taken → `VALIDATION_ERROR` with a clear message)

### PATCH /api/employees/{id}/status
Activate/deactivate an employee (decision 18). Never a physical delete.

- **Auth:** admin
- **Request:**
```json
{ "isActive": false }
```
- **Success 200:** the updated employee object.
- **Errors:** `VALIDATION_ERROR` (admin deactivating themselves is rejected)

---

## 3. Products

### GET /api/products
- **Auth:** customer / cashier
- **Behavior by role:**
  - `customer` → only products where `IsActive && IsAvailable` (the app menu).
  - `cashier` / `admin` → all `IsActive` products, including unavailable ones (with flags).
- **Success 200:**
```json
[
  {
    "id": 3,
    "name": "كابتشينو",
    "price": 2.50,
    "unitType": "Piece",
    "isAvailable": true,
    "isActive": true
  }
]
```

### POST /api/products
- **Auth:** admin
- **Request:**
```json
{ "name": "كابتشينو", "price": 2.50, "unitType": "Piece" }
```
  `unitType` ∈ `Piece` | `Kg`.
- **Success 201:** the created product object.
- **Errors:** `VALIDATION_ERROR`

### PUT /api/products/{id}
- **Auth:** admin
- **Request:** same shape as POST. Price changes never touch old orders (decision 3).
- **Success 200:** the updated product object.
- **Errors:** `PRODUCT_NOT_FOUND`, `VALIDATION_ERROR`

### DELETE /api/products/{id}
Soft delete — sets `IsActive = false` (decision 14).

- **Auth:** admin
- **Success 204:** empty body.
- **Errors:** `PRODUCT_NOT_FOUND`

### PATCH /api/products/{id}/availability
"Out of stock today" toggle (decision 14).

- **Auth:** cashier
- **Request:**
```json
{ "isAvailable": false }
```
- **Success 200:** the updated product object.
- **Errors:** `PRODUCT_NOT_FOUND`

---

## 4. Customers

### POST /api/customers
Cashier registers a customer at the counter.

- **Auth:** cashier
- **Request:**
```json
{ "fullName": "محمد", "phoneNumber": "0791234567" }
```
  Backend normalizes to E.164 (`+9627...`) and validates (ERD rule).
- **Success 201:**
```json
{ "id": 12, "fullName": "محمد", "phoneNumber": "+962791234567", "pointsBalance": 0, "createdAt": "..." }
```
- **Errors:** `INVALID_PHONE`, `PHONE_ALREADY_EXISTS`

### GET /api/customers?phone=0791234567
Lookup by phone — the cashier's main search (decision 7).

- **Auth:** cashier
- **Success 200:** single customer object (same shape as POST 201).
- **Errors:** `INVALID_PHONE`, `CUSTOMER_NOT_FOUND`

### GET /api/customers/{id}/orders
The customer's recent orders — feeds the returns screen.

- **Auth:** cashier
- **Query:** `?limit=10` (default 10, max 50)
- **Success 200:**
```json
[
  {
    "orderId": 45,
    "createdAt": "...",
    "status": "Completed",
    "total": 6.00,
    "pointsRedeemed": 250,
    "pointsEarned": 17,
    "items": [
      {
        "orderItemId": 101,
        "productName": "كابتشينو",
        "quantity": 2,
        "returnedQuantity": 0,
        "unitPriceSnapshot": 2.50
      }
    ]
  }
]
```
  `status` ∈ `Completed` | `Returned` | `Cancelled` (ERD rule). `Returned` is a one-way state set on the first return; whether it is partial or full is derived from each item's `returnedQuantity`, not a separate status.
- **Errors:** `CUSTOMER_NOT_FOUND`

### GET /api/customers/me
- **Auth:** customer
- **Success 200:**
```json
{ "fullName": "محمد", "phoneNumber": "+962791234567", "pointsBalance": 340 }
```

### GET /api/customers/me/transactions
- **Auth:** customer
- **Query:** `?limit=20` (default 20, max 100)
- **Success 200:**
```json
[
  { "id": 88, "type": "Earn", "amount": 17, "orderId": 45, "createdAt": "..." },
  { "id": 87, "type": "Redeem", "amount": -250, "orderId": 45, "createdAt": "..." }
]
```
  `type` ∈ `Earn` | `Redeem` | `Refund` | `RedeemReversal`. Sign is the balance effect (ERD rule).

---

## 5. Orders

### POST /api/orders
Create an order. **The client never sends prices** — the server reads the
catalog, snapshots prices, computes everything (formulas in ERD).

- **Auth:** cashier
- **Request:**
```json
{
  "customerId": 12,
  "pointsRedeemed": 250,
  "items": [
    { "productId": 3, "quantity": 2 },
    { "productId": 7, "quantity": 0.5 }
  ]
}
```
  `pointsRedeemed` defaults to 0 if omitted.
- **Success 201:**
```json
{
  "orderId": 45,
  "total": 6.00,
  "cashPaid": 3.50,
  "pointsRedeemed": 250,
  "pointsEarned": 17,
  "newBalance": 107
}
```
- **Errors:** `CUSTOMER_NOT_FOUND`, `PRODUCT_NOT_FOUND`, `PRODUCT_UNAVAILABLE`,
  `INVALID_QUANTITY`, `REDEEM_BELOW_MINIMUM`, `INSUFFICIENT_BALANCE`, `REDEEM_EXCEEDS_TOTAL`

### GET /api/orders/{id}
- **Auth:** cashier
- **Success 200:** same order shape as in `GET /api/customers/{id}/orders`, plus
  `"customerId"`, `"customerName"`, `"employeeName"`.
- **Errors:** `ORDER_NOT_FOUND`

### POST /api/orders/{id}/cancel
Full cancellation (decision 6). Claws back `PointsEarned` (Refund) and
restores `PointsRedeemed` (RedeemReversal). Rejected if any partial return
exists (ERD rule) or outside the window (decision 16).

- **Auth:** cashier
- **Request:** empty body.
- **Success 200:**
```json
{ "pointsClawedBack": 17, "pointsRestored": 250, "newBalance": 340 }
```
- **Errors:** `ORDER_NOT_FOUND`, `ORDER_ALREADY_CANCELLED`, `ORDER_HAS_RETURNS`,
  `RETURN_WINDOW_EXPIRED`, `INSUFFICIENT_BALANCE_FOR_RETURN`

### POST /api/orders/{id}/returns
Partial return. **Only allowed on orders where `PointsRedeemed = 0`** (decision 19);
orders paid with points can only be fully cancelled.

- **Auth:** cashier
- **Request:**
```json
{ "items": [ { "orderItemId": 101, "quantity": 0.5 } ] }
```
- **Success 200:**
```json
{ "refundAmount": 1.25, "pointsClawedBack": 6, "newBalance": 334 }
```
- **Claw-back formula (cumulative, drift-free):**
  ```
  returnedValueSoFar = Σ over all items (ReturnedQuantity × UnitPriceSnapshot)   // after this return
  targetClawBack     = floor(PointsEarned × returnedValueSoFar / Total)
  pointsClawedBack   = targetClawBack − alreadyClawedBackOnThisOrder
  ```
  Guarantees: the sum of claw-backs never exceeds `PointsEarned`, and returning
  the whole order line-by-line claws back exactly `PointsEarned`.
- **Errors:** `ORDER_NOT_FOUND`, `ORDER_ALREADY_CANCELLED`, `ORDER_PAID_WITH_POINTS`,
  `ITEM_NOT_IN_ORDER`, `RETURN_EXCEEDS_QUANTITY`, `RETURN_WINDOW_EXPIRED`,
  `INSUFFICIENT_BALANCE_FOR_RETURN`

---

## 6. Reports — Deferred (Week 3+)

Shape agreed now so the dashboard home screen can be designed; **endpoint built later.**

### GET /api/reports/summary?from=2026-07-01&to=2026-07-15
- **Auth:** admin
- **Success 200:**
```json
{
  "ordersCount": 210,
  "totalSales": 940.50,
  "totalCashPaid": 895.25,
  "pointsEarned": 4476,
  "pointsRedeemed": 4525,
  "outstandingPointsLiability": 12840
}
```
  `outstandingPointsLiability` = sum of all customers' current balances —
  the shop's total points debt.

---

## Out of scope (documented, not built)

- **QR-based returns** — decision 7, week 3 if time allows. No endpoints reserved.
- **Walk-in orders** — decision 17, handled outside the system entirely.
- **Manual points adjustment** — decision 12, deliberately impossible.