import { motion, useReducedMotion } from 'motion/react'
import { LuDownload, LuSmartphone, LuShieldCheck, LuInfo } from 'react-icons/lu'
import { APK_URL, APK_FILENAME } from '../constants/download.js'
import { useApkAvailability } from '../hooks/useApkAvailability.js'
import { useLanguage } from '../context/LanguageContext.jsx'

/**
 * The download button, in whichever of its three states applies.
 *
 * `checking` is its own state rather than an optimistic guess: rendering a live
 * link and then taking it away a moment later is worse than a beat of waiting,
 * and someone who taps during that beat would get the site's own home page
 * saved as a .apk.
 */
function DownloadButton({ status, copy }) {
  const isReady = status === 'ready'

  if (!isReady) {
    return (
      <span
        className="download-cta is-disabled"
        aria-disabled="true"
        role="button"
      >
        <LuDownload aria-hidden="true" />
        {status === 'checking' ? copy.checking : copy.ctaPending}
      </span>
    )
  }

  return (
    <a className="download-cta" href={APK_URL} download={APK_FILENAME}>
      <LuDownload aria-hidden="true" />
      {copy.cta}
    </a>
  )
}

function Download() {
  const { translations } = useLanguage()
  const copy = translations.download

  // `whileInView` does not consult the platform preference on its own, so the
  // entrance props are dropped entirely when reduced motion is asked for —
  // the content is simply there, at rest, on the first frame.
  const reduceMotion = useReducedMotion()

  const entrance = (delay = 0) =>
    reduceMotion
      ? {}
      : {
          initial: { opacity: 0, y: 24 },
          whileInView: { opacity: 1, y: 0 },
          viewport: { once: true, amount: 0.2 },
          transition: { duration: 0.5, delay },
        }

  // Shared with the home page's store badge, so the two can never contradict
  // each other. Resolved at load rather than at build: the APK is dropped in by
  // hand after a release is signed, and a build-time flag would be wrong from
  // that moment until the next deploy.
  const status = useApkAvailability()

  return (
    <main className="download-page">
      <header className="download-header">
        <div className="download-decoration download-decoration-left" />
        <div className="download-decoration download-decoration-right" />

        <div className="download-header-content">
          <p className="download-tag">✦ {copy.eyebrow}</p>
          <h1>{copy.title}</h1>
          <span className="download-line" />
          <p>{copy.description}</p>
        </div>
      </header>

      <motion.section className="download-panel" {...entrance()}>
        <span className="download-platform">
          <LuSmartphone aria-hidden="true" />
          {copy.forAndroid}
        </span>

        <DownloadButton status={status} copy={copy} />

        {status === 'pending' && (
          <p className="download-pending" role="status">
            {copy.pendingNote}
          </p>
        )}

        <p className="download-requirement">{copy.requirement}</p>

        <p className="download-rate">{copy.rate}</p>
      </motion.section>

      <section className="download-steps">
        <div className="download-steps-head">
          <h2>{copy.stepsTitle}</h2>
          <p>{copy.stepsLead}</p>
        </div>

        {/* Numbered because it genuinely is a sequence: each step only makes
            sense after the one above it has happened. */}
        <ol className="download-step-list">
          {copy.steps.map(([title, body], index) => (
            <motion.li
              className="download-step"
              key={title}
              {...entrance(Math.min(index, 4) * 0.05)}
            >
              <span className="download-step-number" aria-hidden="true">
                {index + 1}
              </span>

              <div className="download-step-body">
                <h3>{title}</h3>
                <p>{body}</p>
              </div>
            </motion.li>
          ))}
        </ol>

        {/* The single most important thing on the page: the warning is normal.
            Without this most people stop at step 4 and assume the file is
            malware. */}
        <aside className="download-safety">
          <span className="download-safety-icon">
            <LuShieldCheck aria-hidden="true" />
          </span>

          <div>
            <h3>
              <LuInfo aria-hidden="true" /> {copy.safetyTitle}
            </h3>
            <p>{copy.safetyBody}</p>
          </div>
        </aside>
      </section>
    </main>
  )
}

export default Download
