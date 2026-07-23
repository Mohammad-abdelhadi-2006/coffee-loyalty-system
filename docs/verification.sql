-- =============================================================================
-- Balance Integrity Verification  (decisions.md #21)
-- =============================================================================
-- Customer.PointsBalance is a denormalized running total (decision 2). It must
-- always equal the sum of that customer's PointsTransaction.Amount rows
-- (Amount is signed: + increases, - decreases). This query finds every customer
-- whose stored balance has drifted from its transaction ledger.
--
-- HOW TO READ THE RESULT:
--   ZERO ROWS returned  = the system is healthy — every stored balance agrees
--                         with its ledger.
--   ANY ROW returned    = a bug corrupted that customer's balance. Trace it to
--                         its source and fix the cause. NEVER silently patch the
--                         balance column (decision 21) — that hides the defect.
--
-- WHEN TO RUN: manually before each demo and at the end of each dev week.
-- A LEFT JOIN + COALESCE covers customers with no transactions yet (expected 0).
-- =============================================================================

SELECT
    c.Id                              AS CustomerId,
    c.PhoneNumber,
    c.PointsBalance                   AS StoredBalance,
    COALESCE(SUM(pt.Amount), 0)       AS LedgerBalance,
    c.PointsBalance - COALESCE(SUM(pt.Amount), 0) AS Drift
FROM Customers AS c
LEFT JOIN PointsTransactions AS pt
    ON pt.CustomerId = c.Id
GROUP BY c.Id, c.PhoneNumber, c.PointsBalance
HAVING c.PointsBalance <> COALESCE(SUM(pt.Amount), 0)
ORDER BY ABS(c.PointsBalance - COALESCE(SUM(pt.Amount), 0)) DESC;
