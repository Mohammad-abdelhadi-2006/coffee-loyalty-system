/**
 * Where the Android build is served from.
 *
 * The file itself is **not** in this repository and is not produced by the
 * build. Drop the signed APK at
 *
 *   website/public/downloads/nakhat-finjan.apk
 *
 * and Vite copies it to the site root as-is, so this path resolves in both
 * `npm run dev` and a production build with nothing else to change.
 *
 * There is deliberately no second place to edit: the page reads this constant
 * for the link, the filename, and the presence check.
 */
export const APK_URL = '/downloads/nakhat-finjan.apk'

/** What the browser saves it as, taken from the path so the two cannot drift. */
export const APK_FILENAME = APK_URL.split('/').pop()

/** How long to wait on the presence check before giving up on it. */
const PROBE_TIMEOUT_MS = 6000

/**
 * Whether the APK is actually sitting at {@link APK_URL} right now.
 *
 * Checked at page load rather than at build time on purpose: the file is added
 * by hand after a release is signed, and a build-time flag would go stale the
 * moment it was dropped in without a rebuild. A `HEAD` costs one request and no
 * body.
 *
 * The `text/html` guard is the part that matters. A single-page site answers an
 * unknown path with `index.html` and a 200, so `response.ok` alone would report
 * a missing APK as present and hand the visitor a download that is really the
 * home page with the wrong extension.
 *
 * @returns {Promise<boolean>} true only if something that is not a web page is
 *   being served there.
 */
export async function apkIsAvailable() {
  const abort = new AbortController()
  const timer = setTimeout(() => abort.abort(), PROBE_TIMEOUT_MS)

  try {
    const response = await fetch(APK_URL, {
      method: 'HEAD',
      signal: abort.signal,
    })

    if (!response.ok) return false

    const type = response.headers.get('content-type') ?? ''

    return !type.toLowerCase().includes('text/html')
  } catch {
    // Offline, aborted, or the host refused the HEAD. Either way there is
    // nothing to promise the visitor, so the button stays in its waiting state.
    return false
  } finally {
    clearTimeout(timer)
  }
}

/**
 * The in-flight or settled result of {@link apkIsAvailable}, kept for the life
 * of the page.
 *
 * Memoised so that every surface asking the question gets the *same answer*,
 * not merely the same logic. The home page's store badge and the download page
 * both read this, and two independent probes could in principle disagree —
 * different moments, one request failing and the other not — which would put
 * "قريباً" on one screen and a live download on the other.
 *
 * One consequence, and it is the right one: an APK dropped in mid-session is
 * not noticed until the page is reloaded. A badge that changed under the
 * visitor's cursor would be worse than one that is consistent.
 */
let pending = null

/** The shared answer. Probes once per page load, then hands out the result. */
export function apkAvailability() {
  pending ??= apkIsAvailable()

  return pending
}
