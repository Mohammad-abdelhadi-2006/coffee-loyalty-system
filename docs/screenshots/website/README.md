# Marketing site — نكهة فنجان

The public site (`website/`), captured full-page: every route, in both languages, at desktop
and phone widths. 16 files.

Naming is `{viewport}-{language}-{page}.png`.

| Page | Route | Desktop AR | Desktop EN | Mobile AR | Mobile EN |
|---|---|---|---|---|---|
| Home | `/` | 1440×4201 | 1440×4247 | 390×6824 | 390×6943 |
| Menu | `/menu` | 1440×5091 | 1440×5091 | 390×7685 | 390×8321 |
| Our Story | `/aboutus` | 1440×2008 | 1440×2096 | 390×2927 | 390×3066 |
| Contact | `/contact` | 1440×1530 | 1440×1653 | 390×2617 | 390×2617 |

Captured at `deviceScaleFactor: 2`, so the pixel dimensions are twice the CSS ones above —
they hold up in a slide deck or in print.

## Notes for whoever recaptures these

**Language comes from `localStorage`, and setting it after the page loads does not work.**
`LanguageContext` reads the key on first render and an effect writes its own value straight
back, so a `setItem` after load is overwritten before the next paint. Set it before any of the
app's script runs — Puppeteer's `evaluateOnNewDocument`, or a fresh profile — and confirm by
reading back `document.documentElement.dir`: `rtl` for Arabic, `ltr` for English. Identical
file sizes between the two languages mean the switch silently did not happen.

**The hero holds a looping video and sections animate in on scroll.** Capturing on load gives a
black hero and half-faded sections. Wait for `video.readyState >= 3`, scroll the full height
once to trigger the entrance animations, then return to the top before shooting.

**Contact page content is placeholder.** The phone (`+962771234567`), the email
(`exm@gmail.com`) and the address are sample values in `src/context/translations.js`, not the
shop's real details — unlike the app's «تواصل معنا» screen, which carries the confirmed ones.
Worth reconciling before this site goes public.
