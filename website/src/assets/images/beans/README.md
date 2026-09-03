# Bean photos

Drop a photo in here and it appears on the home page's bean section on the next
build. There is no code to edit — `src/constants/beanImages.js` picks up
whatever is in this folder by filename.

| Filename | Bean type |
|---|---|
| `brazilian.jpg` | قهوة برازيلي — Brazilian coffee |
| `specialty.jpg` | قهوة خصوصي — Specialty coffee |
| `gold.jpg` | قهوة جولد — Gold coffee |

## Specification

| | |
|---|---|
| Aspect ratio | **4:3** (landscape) |
| Minimum size | **800 × 600** |
| Recommended | 1200 × 900 |
| Format | `.jpg` (`.jpeg`, `.png`, `.webp` also work) |

Anything not 4:3 is centre-cropped to fit the card, so keep the beans away from
the edges. Until a photo exists the card shows a tinted stand-in with a bean
mark — that is the intended empty state, not a broken image.
