/**
 * The anchor to the home page's "get the app" section.
 *
 * Kept apart from the component so the header can import the id and the scroll
 * helper without importing the section itself.
 */

/** The section's DOM id, and the hash the header links to. */
export const GET_APP_ID = 'get-app'

/**
 * Scrolls the app section into view, honouring a reduced-motion preference.
 *
 * The header calls this directly when the app link is clicked while already on
 * the home page: there is no route change to react to in that case, so nothing
 * else would move the page.
 */
export function scrollToGetApp() {
  const section = document.getElementById(GET_APP_ID)

  if (!section) return

  const prefersReducedMotion = window.matchMedia(
    '(prefers-reduced-motion: reduce)',
  ).matches

  section.scrollIntoView({
    behavior: prefersReducedMotion ? 'auto' : 'smooth',
    block: 'start',
  })
}
