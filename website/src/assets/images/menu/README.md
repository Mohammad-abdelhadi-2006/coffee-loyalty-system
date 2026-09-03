# Menu photos

Drop a product photo in here and it appears on `/menu` on the next build. There
is no code to edit — `src/constants/menuImages.js` picks up whatever is in this
folder by filename.

## Naming

    <category>-<product>.jpg

The exact filename each product looks for is listed in
`src/constants/menuImages.js` (`MENU_IMAGE_SLUGS`), with the Arabic and English
name of the drink in a comment on every line. A file whose name does not match a
slug is simply ignored.

## Specification

| | |
|---|---|
| Aspect ratio | **4:3** (landscape) |
| Minimum size | **800 × 600** |
| Recommended | 1200 × 900 |
| Format | `.jpg` (`.jpeg`, `.png`, `.webp` also work) |

Anything not 4:3 is centre-cropped to fit the card, so keep the drink away from
the edges. Until a photo exists, the card shows a tinted stand-in with the
category mark — that is the intended empty state, not a broken image.

## Categories

`hot-coffee` · `cold-coffee` · `mojito` · `milkshake` · `desserts`

Note that `فراولة`, `مانجو`, `بطيخ`, `مكس بيري` and `باشن فروت` each exist as
both a mojito and a milkshake. They are different drinks and need different
photos — that is why every filename carries its category.
