import { translations } from '../context/translations.js'

/**
 * Which photo file each menu item looks for.
 *
 * The product data itself lives in `src/context/translations.js`, duplicated per
 * language and matched only by position — so a name cannot be the key for a
 * photo without the file having to be renamed when the Arabic or English
 * wording changes. The table below is that stable key instead: one slug per
 * item, in exactly the order `translations.<lang>.menu.categories[].items[]`
 * declares them.
 *
 * DROP A PHOTO IN AND IT APPEARS. Save it as
 *
 *   src/assets/images/menu/<slug>.jpg
 *
 * and the card picks it up on the next build — there is no list to edit here
 * and no import to add. Until a file exists, the card shows a styled stand-in
 * rather than a broken image.
 *
 * Keeping this in sync: the slug arrays must stay the same length, and in the
 * same order, as the items in `translations.js`. The drift check at the bottom
 * of this file verifies exactly that, and warns in development if it slips.
 */

/** Where the photos go, relative to the project root. Quoted in the README. */
export const MENU_IMAGE_DIR = 'src/assets/images/menu'

/**
 * What a usable photo looks like.
 *
 * The 4:3 frame is the card's, so anything squarer or wider is centre-cropped
 * to fit rather than letterboxed. The minimum is set by the widest the card
 * ever renders (a ~380px column on a 2× display).
 */
export const MENU_IMAGE_SPEC = {
  aspectRatio: '4 / 3',
  minWidth: 800,
  minHeight: 600,
  recommended: '1200 × 900',
  format: 'jpg',
}

/**
 * The category part of every slug, positionally aligned with
 * `translations.<lang>.menu.categories`.
 *
 * Language-independent on purpose: the same photo has to serve the Arabic and
 * the English page.
 */
export const MENU_CATEGORY_SLUGS = [
  'hot-coffee', // قهوة ساخنة / Hot Coffee
  'cold-coffee', // قهوة باردة / Cold Coffee
  'mojito', // موهيتو / Mojito
  'milkshake', // ميلك شيك / Milkshake
  'desserts', // حلويات / Desserts
]

/**
 * One slug per item, grouped by category, in declaration order.
 *
 * Derived from the English product names, prefixed by the category — the prefix
 * is not decoration: "فراولة / Strawberry", "مانجو / Mango", "بطيخ /
 * Watermelon", "مكس بيري / Mixed berry" and "باشن فروت / Passion fruit" each
 * exist as both a mojito and a milkshake, and they are different drinks that
 * need different photos.
 */
export const MENU_IMAGE_SLUGS = [
  // قهوة ساخنة / Hot Coffee
  [
    'hot-coffee-spanish-latte', // سبانش لاتيه / Spanish latte
    'hot-coffee-latte', // لاتيه / Latte
    'hot-coffee-cappuccino', // كابتشينو / Cappuccino
    'hot-coffee-americano', // أمريكانو / Americano
    'hot-coffee-v60', // V60 / V60
    'hot-coffee-espresso', // إسبريسو / Espresso
    'hot-coffee-double-espresso', // دبل شوت إسبريسو / Double espresso
    'hot-coffee-dark-mocha', // موكا دارك / Dark mocha
    'hot-coffee-white-mocha', // موكا وايت / White mocha
    'hot-coffee-pistachio-latte', // بستاشيو لاتيه / Pistachio latte
    'hot-coffee-flat-white', // فلات وايت / Flat white
    'hot-coffee-turkish-coffee', // قهوة تركية / Turkish coffee
    'hot-coffee-red-eye', // RED EYE / RED EYE
  ],

  // قهوة باردة / Cold Coffee
  [
    'cold-coffee-iced-spanish-latte', // آيس سبانش لاتيه / Iced Spanish latte
    'cold-coffee-iced-pistachio-latte', // آيس بستاشيو لاتيه / Iced pistachio latte
    'cold-coffee-iced-latte', // آيس لاتيه / Iced latte
    'cold-coffee-iced-mocha', // آيس موكا / Iced mocha
    'cold-coffee-iced-white-mocha', // آيس وايت موكا / Iced white mocha
    'cold-coffee-iced-caramel-macchiato', // آيس كراميل مكياتو / Iced caramel macchiato
    'cold-coffee-iced-americano', // آيس أمريكانو / Iced Americano
    'cold-coffee-iced-v60', // آيس V60 / Iced V60
    'cold-coffee-frappuccino', // فرابتشينو / Frappuccino
  ],

  // موهيتو / Mojito
  [
    'mojito-blue-curacao', // بلو كارساو / Blue curacao
    'mojito-mojito', // موهيتو / Mojito
    'mojito-strawberry', // فراولة / Strawberry
    'mojito-mixed-berry', // مكس بيري / Mixed berry
    'mojito-watermelon', // بطيخ / Watermelon
    'mojito-mango', // مانجو / Mango
    'mojito-passion-fruit', // باشن فروت / Passion fruit
  ],

  // ميلك شيك / Milkshake
  [
    'milkshake-passion-mango', // باشينجو / Passion mango
    // The photo labelled «بطيخ مع مانجا» is this drink — the shop calls it
    // ميلينجو on the menu, so there is one item here, not two.
    'milkshake-mango-blend', // ميلينجو / Mango blend
    'milkshake-strawberry', // فراولة / Strawberry
    'milkshake-mixed-berry', // مكس بيري / Mixed berry
    'milkshake-watermelon', // بطيخ / Watermelon
    'milkshake-mango', // مانجو / Mango
    'milkshake-passion-fruit', // باشن فروت / Passion fruit
  ],

  // حلويات / Desserts
  [
    'desserts-cookies', // كوكيز / Cookies
    'desserts-croissant', // كرواسون / Croissant
    'desserts-donut', // دونات / Donut
    'desserts-cupcake', // كب كيك / Cupcake
  ],
]

/**
 * Every photo present in the folder right now, resolved to its built URL.
 *
 * `import.meta.glob` is evaluated by Vite at build time, so adding a file is
 * the whole of the work — no import statement, no registration. An empty folder
 * yields an empty map and every card falls back to its stand-in. Extensions
 * beyond .jpg are accepted so a .webp or a .png does not silently do nothing.
 */
// Extensions are listed explicitly, and that is load-bearing: a bare `*` here
// sweeps up this folder's README.md and fails the build trying to parse
// Markdown as a module. (A `*.{jpg,png,…}` brace group works too — this form is
// just easier to read and to add a format to.)
const files = import.meta.glob(
  [
    '../assets/images/menu/*.jpg',
    '../assets/images/menu/*.jpeg',
    '../assets/images/menu/*.png',
    '../assets/images/menu/*.webp',
  ],
  { eager: true, import: 'default' },
)

/** Slug (the filename without its extension) -> resolved URL. */
const bySlug = Object.fromEntries(
  Object.entries(files).map(([path, url]) => [
    path.split('/').pop().replace(/\.[^.]+$/, ''),
    url,
  ]),
)

/** The slug an item's photo is filed under, or undefined if out of range. */
export const getMenuImageSlug = (categoryIndex, itemIndex) =>
  MENU_IMAGE_SLUGS[categoryIndex]?.[itemIndex]

/**
 * The item's photo URL, or `null` when nobody has supplied one yet.
 *
 * A null return is the normal state until the shop's photos arrive, not an
 * error — the card renders its stand-in and the page still looks finished.
 */
export function getMenuImage(categoryIndex, itemIndex) {
  const slug = getMenuImageSlug(categoryIndex, itemIndex)

  return (slug && bySlug[slug]) || null
}

// ── Drift check ─────────────────────────────────────────────────────────────
//
// Adding a drink to translations.js without adding its slug above would
// otherwise fail silently: that one item would never show a photo, however many
// files were dropped in the folder. Checked once when this module loads, and
// only in development — the whole block is dropped from the production bundle.
if (import.meta.env.DEV) {
  const categories = translations.ar.menu.categories

  if (MENU_IMAGE_SLUGS.length !== categories.length) {
    console.warn(
      `[menuImages] ${categories.length} categories in translations.js but ` +
        `${MENU_IMAGE_SLUGS.length} slug groups here. Photos will be missing.`,
    )
  } else {
    categories.forEach((category, index) => {
      const expected = category.items.length
      const actual = MENU_IMAGE_SLUGS[index].length

      if (expected !== actual) {
        console.warn(
          `[menuImages] "${category.title}" has ${expected} items in ` +
            `translations.js but ${actual} slugs here. Photos will be missing.`,
        )
      }
    })
  }
}
