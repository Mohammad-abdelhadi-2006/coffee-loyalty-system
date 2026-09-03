import { MENU_IMAGE_SLUGS } from './menuImages.js'

/**
 * Which menu items the home page features.
 *
 * Keyed by photo slug, not by position. Position was the obvious choice and the
 * wrong one: removing a single drink from the menu silently re-pointed one of
 * these at its neighbour, and nothing broke loudly enough to notice. A slug is
 * stable — it survives reordering, insertion and removal.
 *
 * The slug is also what the photo is filed under, so the picture shown here is
 * by construction the same file the menu card uses.
 */
const FEATURED = [
  { slug: 'hot-coffee-spanish-latte', tag: 'top' },
  { slug: 'cold-coffee-frappuccino', tag: 'popular' },
  { slug: 'mojito-mojito', tag: '' },
  { slug: 'milkshake-strawberry', tag: '' },
]

/** Where a slug sits in the menu, or null if it is no longer there. */
function locate(slug) {
  for (let category = 0; category < MENU_IMAGE_SLUGS.length; category++) {
    const item = MENU_IMAGE_SLUGS[category].indexOf(slug)

    if (item !== -1) return { category, item }
  }

  return null
}

/**
 * The featured items as `{ category, item, tag }`, with anything that no longer
 * exists dropped rather than rendered as a blank card.
 */
export const BEST_SELLERS = FEATURED.map(({ slug, tag }) => {
  const at = locate(slug)

  if (!at && import.meta.env.DEV) {
    console.warn(
      `[bestSellers] "${slug}" is featured on the home page but is not in the ` +
        `menu any more. It has been dropped from the section.`,
    )
  }

  return at ? { ...at, tag } : null
}).filter(Boolean)
