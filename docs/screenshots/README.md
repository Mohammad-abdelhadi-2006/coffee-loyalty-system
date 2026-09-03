# App screens — نكهة فنجان

Every screen of the customer app, captured on a Pixel 6a emulator (1080×2400) against a local
API. In flow order.

| # | File | Screen |
|---|---|---|
| 01 | `01-splash.png` | Splash — the brand mark, held 900 ms so it does not flash past |
| 02 | `02-login.png` | «سجّل دخولك» — phone entry, CTA disabled |
| 03 | `03-login-filled.png` | Same with nine digits typed, CTA enabled |
| 04 | `04-otp.png` | «أدخل الرمز» — six boxes and the resend countdown |
| 05 | `05-name.png` | «شو اسمك؟» — asked once, on a first sign-in |
| 06 | `06-name-filled.png` | Same with a name typed |
| 07 | `07-home-loading.png` | Home's skeleton frame while the first load is in flight |
| 08 | `08-home.png` | Home — balance and the points ledger |
| 09 | `09-menu-hot-coffee.png` | Menu — «قهوة ساخنة» |
| 10 | `10-menu-beans.png` | Menu — «بن», priced per kilo |
| 11 | `11-purchases.png` | «مشترياتي» — three orders, one of them a redemption |
| 12 | `12-settings.png` | «الإعدادات» |
| 13–16 | `13-how-to-earn-*.png` | «كيف تكسب نقاط؟», scrolled top to bottom |
| 17–18 | `17-contact-*.png` | «تواصل معنا», scrolled top to bottom |
| 19 | `19-signout-sheet.png` | The sign-out confirmation sheet |

## About the data in these shots

The customer, the menu and the three orders are **demo data**, created for the capture and then
removed — the development database is back to its deploy state (67 imported customers, no
products, no orders). Nothing here is a real customer.

The account shown is the Firebase test number `+962790000000`.

## What the screens happen to demonstrate

Worth knowing, since these are the frames a reader will look at:

- **11** shows «طلب #1/#2/#3» under each date — the order number the cashier searches by, and
  «استبدلت 200 نقطة» on the redemption, at the current minimum.
- **05** is reachable at all because a first sign-in is asked for a name. An imported customer
  carries a blank one and is asked the same way, once.
- **13–16** quote the live rules — 3 points per dinar of cash paid, 100 points to the dinar,
  200 minimum — read off `LoyaltyConstants`, not written by hand.

## Recapturing

Screens change; these will go stale. To redo them, start the API, seed a menu and a few orders,
run the app on an emulator and walk the flow. Numbers in the file names are flow order, so a
new screen goes in at its position and the rest shift.
