/**
 * Which photo each bean type looks for.
 *
 * Same arrangement as the menu photos: drop a file in and it appears on the
 * next build — there is no import to add and no list to edit here.
 *
 *   src/assets/images/beans/<slug>.jpg
 *
 * Until a file exists the card shows a tinted stand-in in the site's own
 * tokens, which is the intended empty state rather than a broken image.
 */

/** Where the photos go, relative to the project root. */
export const BEAN_IMAGE_DIR = 'src/assets/images/beans'

/**
 * What a usable photo looks like.
 *
 * The 4:3 frame is the card's, matching the menu cards so the two grids read as
 * the same object. Anything squarer or wider is centre-cropped to fit.
 */
export const BEAN_IMAGE_SPEC = {
  aspectRatio: '4 / 3',
  minWidth: 800,
  minHeight: 600,
  recommended: '1200 × 900',
  format: 'jpg',
}

/**
 * The three types, in the order they are shown.
 *
 * `name` is not stored here — it lives in `translations.js` so it can be
 * Arabic or English. This list is only the stable, language-independent key
 * each photo is filed under.
 */
export const BEAN_SLUGS = [
  'brazilian', // قهوة برازيلي / Brazilian coffee
  'specialty', // قهوة خصوصي / Specialty coffee
  'gold', // قهوة جولد / Gold coffee
]

/**
 * Every bean photo present right now, resolved to its built URL.
 *
 * Extensions are listed explicitly rather than as a bare `*`, which would sweep
 * up this folder's README.md and fail the build trying to parse Markdown as a
 * module.
 */
const files = import.meta.glob(
  [
    '../assets/images/beans/*.jpg',
    '../assets/images/beans/*.jpeg',
    '../assets/images/beans/*.png',
    '../assets/images/beans/*.webp',
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

/**
 * The photo for the bean type at [index], or `null` when none is supplied yet.
 *
 * A null return is the normal state until the shop's photos arrive, not an
 * error — the card renders its stand-in and the section still looks finished.
 */
export function getBeanImage(index) {
  const slug = BEAN_SLUGS[index]

  return (slug && bySlug[slug]) || null
}
