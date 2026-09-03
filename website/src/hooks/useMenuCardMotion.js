import { useEffect, useRef } from 'react'

/**
 * Motion for the menu grid: reveal-on-scroll, and a pointer tilt on the cards.
 *
 * Everything here does two things only — toggle a class, or set a CSS custom
 * property. The actual movement is CSS transforms and opacity, so it stays on
 * the compositor and no layout property is ever animated. That is also why this
 * is hand-rolled rather than done with the animation library the other pages
 * use: 35 cards is enough that one shared IntersectionObserver and a class
 * beats 35 library-driven components.
 */

/** Gap between two cards revealing in the same batch. */
const REVEAL_STEP_MS = 55

/** How many cards still stagger before the rest share the last delay. */
const MAX_STAGGER_STEPS = 9

/** Half the total sweep, so the card tilts at most this far off-axis. */
const MAX_TILT_DEG = 6

/** Reveal slightly before the card's edge clears, so it is never seen popping. */
const REVEAL_MARGIN = '0px 0px -8% 0px'

const matches = (query) =>
  typeof window !== 'undefined' &&
  typeof window.matchMedia === 'function' &&
  window.matchMedia(query).matches

/** The user has asked the system for less movement. Honoured everywhere below. */
export const prefersReducedMotion = () =>
  matches('(prefers-reduced-motion: reduce)')

/**
 * Whether this device should get the tilt at all.
 *
 * Three ways to be excluded, and any one is enough:
 *
 *  - no fine pointer to tilt toward — a touch screen has nothing to track, and
 *    the effect would only fire as a jolt on tap;
 *  - the reduced-motion preference;
 *  - a device unlikely to hold 60fps while doing it. `deviceMemory` and
 *    `hardwareConcurrency` are both absent on some browsers, so each is only
 *    consulted when actually reported — a missing value is never treated as a
 *    low reading.
 */
function shouldEnableTilt() {
  if (!matches('(hover: hover) and (pointer: fine)')) return false
  if (prefersReducedMotion()) return false

  const { deviceMemory, hardwareConcurrency, connection } = navigator

  if (typeof deviceMemory === 'number' && deviceMemory <= 4) return false
  if (typeof hardwareConcurrency === 'number' && hardwareConcurrency <= 4) {
    return false
  }
  if (connection?.saveData) return false

  return true
}

/** Per-card scratch state, so nothing is stashed on the DOM node itself. */
const cardState = new WeakMap()

/**
 * Wires up the menu cards.
 *
 * @param {unknown} resetKey Changing this re-runs the reveal from scratch —
 *   pass whatever identifies the current filter, so switching category
 *   re-staggers the new set instead of showing it already settled.
 * @returns {(element: HTMLElement | null) => void} A ref callback to put on
 *   every card.
 */
export function useMenuCardMotion(resetKey) {
  // The cards currently mounted. Filled by the ref callback, which React runs
  // before effects — so by the time the effect below wires anything up, every
  // card for this filter is already in here.
  const cardsRef = useRef(new Set())

  useEffect(() => {
    const reduced = prefersReducedMotion()
    const tiltEnabled = shouldEnableTilt()
    const cards = cardsRef.current

    // Reduced motion, or a browser without IntersectionObserver: show
    // everything at once, immediately. Content first — the animation was only
    // ever the nicety.
    const revealImmediately =
      reduced || typeof IntersectionObserver === 'undefined'

    let observer = null

    if (!revealImmediately) {
      observer = new IntersectionObserver(
        (entries) => {
          const arriving = entries.filter((entry) => entry.isIntersecting)

          if (arriving.length === 0) return

          // Entries are not guaranteed to arrive in document order, and a row
          // that staggers right-to-left one time and left-to-right the next
          // looks like a glitch. Sorting fixes the sweep in place.
          arriving.sort((a, b) =>
            a.target.compareDocumentPosition(b.target) &
            Node.DOCUMENT_POSITION_FOLLOWING
              ? -1
              : 1,
          )

          arriving.forEach((entry, index) => {
            const delay = Math.min(index, MAX_STAGGER_STEPS) * REVEAL_STEP_MS

            entry.target.style.setProperty('--reveal-delay', `${delay}ms`)
            entry.target.classList.add('is-visible')

            // Revealing is one-way; nothing re-hides on scroll back up.
            observer.unobserve(entry.target)
          })
        },
        { rootMargin: REVEAL_MARGIN, threshold: 0.01 },
      )
    }

    // ── Tilt ──────────────────────────────────────────────────────────────
    //
    // The rect is measured once on enter and reused for the whole hover, so a
    // pointer move never reads layout. Writes are coalesced into one rAF, which
    // caps the work at one transform update per frame per hovered card — and
    // only ever one card is hovered.

    const handleEnter = (event) => {
      const element = event.currentTarget

      cardState.set(element, {
        rect: element.getBoundingClientRect(),
        frame: 0,
      })

      element.classList.add('is-tilting')
    }

    const handleMove = (event) => {
      const element = event.currentTarget
      const state = cardState.get(element)

      if (!state || state.frame) return

      const { clientX, clientY } = event

      state.frame = requestAnimationFrame(() => {
        state.frame = 0

        const { rect } = state
        const offsetX = (clientX - rect.left) / rect.width - 0.5
        const offsetY = (clientY - rect.top) / rect.height - 0.5

        // rotateY follows the cursor across, rotateX against it, which is what
        // reads as the card facing the pointer rather than away from it.
        element.style.setProperty(
          '--tilt-y',
          `${(offsetX * MAX_TILT_DEG * 2).toFixed(2)}deg`,
        )
        element.style.setProperty(
          '--tilt-x',
          `${(-offsetY * MAX_TILT_DEG * 2).toFixed(2)}deg`,
        )
      })
    }

    const handleLeave = (event) => {
      const element = event.currentTarget
      const state = cardState.get(element)

      if (state?.frame) cancelAnimationFrame(state.frame)
      cardState.delete(element)

      // Dropping the custom properties lets the card fall back to 0deg, and
      // removing the class hands it the slower curve to spring back on.
      element.classList.remove('is-tilting')
      element.style.removeProperty('--tilt-x')
      element.style.removeProperty('--tilt-y')
    }

    const attachTilt = (element) => {
      element.addEventListener('pointerenter', handleEnter)
      element.addEventListener('pointermove', handleMove)
      element.addEventListener('pointerleave', handleLeave)
      element.addEventListener('pointercancel', handleLeave)
    }

    const detachTilt = (element) => {
      element.removeEventListener('pointerenter', handleEnter)
      element.removeEventListener('pointermove', handleMove)
      element.removeEventListener('pointerleave', handleLeave)
      element.removeEventListener('pointercancel', handleLeave)

      const state = cardState.get(element)
      if (state?.frame) cancelAnimationFrame(state.frame)
      cardState.delete(element)
    }

    cards.forEach((element) => {
      if (revealImmediately) {
        element.classList.add('is-visible')
      } else {
        observer.observe(element)
      }

      if (tiltEnabled) attachTilt(element)
    })

    return () => {
      observer?.disconnect()
      cards.forEach(detachTilt)
    }
    // resetKey is the point: a filter change tears this down and rebuilds it,
    // so the incoming cards are observed fresh and stagger again.
  }, [resetKey])

  // Stable across renders, so React is not detaching and re-attaching every
  // card's ref on each one. The returned function is React 19's ref cleanup,
  // which runs when that particular card unmounts.
  const registerCard = useRef((element) => {
    const cards = cardsRef.current

    cards.add(element)

    return () => {
      cards.delete(element)
    }
  }).current

  return registerCard
}
