import homeShot from '../assets/images/app/app-home.png'
import purchasesShot from '../assets/images/app/app-purchases.png'
import menuShot from '../assets/images/app/app-menu.png'

import { Link } from 'react-router-dom'
import { motion } from 'motion/react'
import { FaGooglePlay } from 'react-icons/fa'
import {
  LuWallet,
  LuCoffee,
  LuReceipt,
  LuKeyRound,
  LuDownload,
} from 'react-icons/lu'
import { useLanguage } from '../context/LanguageContext.jsx'
import { useApkAvailability } from '../hooks/useApkAvailability.js'
import { GET_APP_ID } from './getAppAnchor.js'

/**
 * The Play Store listing, once there is one.
 *
 * This is the ONLY value that needs changing when the app is published: set it
 * to the listing URL and the badge below turns from a disabled "coming soon"
 * chip into a real link, with no other edit anywhere on the site.
 *
 * TODO: real value — the app is not on Google Play yet, and it cannot be until
 * it is signed with a release key rather than the debug one
 * (mobile/nakhat_finjan/android/app/build.gradle.kts:39-42). Leave this null
 * until the listing exists; a guessed store URL is worse than no button.
 */
const PLAY_STORE_URL = null

/** One icon per feature, in the order the four features are written. */
const featureIcons = [LuWallet, LuCoffee, LuReceipt, LuKeyRound]

/**
 * The three screens shown, in the order copy.screens names them: home, my
 * purchases, menu. Copied from docs/screenshots/ (08-home, 11-purchases,
 * 10-menu-beans).
 */
const screenshots = [homeShot, purchasesShot, menuShot]

/**
 * The store button, in whichever of its three states applies.
 *
 * Precedence, highest first:
 *
 *  1. **A Play listing** — once {@link PLAY_STORE_URL} is set, that is where the
 *     app comes from and the badge says so.
 *  2. **A downloadable build** — the APK is sitting on the site, so the badge
 *     goes to `/download`, which is where the install instructions live. It
 *     deliberately does not link straight at the file: the sideload steps are
 *     the difference between an install and an abandoned download.
 *  3. **Neither** — the disabled "coming soon" chip, unchanged.
 *
 * The availability answer is the *same object* the download page reads, not a
 * second check of its own, so this badge and that page cannot disagree.
 *
 * The disabled form is deliberately not a link and not a button — there is
 * nothing to activate, so it takes no focus and announces itself as disabled
 * rather than looking clickable and doing nothing.
 */
function StoreBadge({ copy, downloadCopy }) {
  const status = useApkAvailability()
  const apkReady = status === 'ready'

  const badge = (icon, small, strong) => (
    <>
      {icon}

      <span className="get-app-badge-text">
        <small>{small}</small>
        <strong>{strong}</strong>
      </span>
    </>
  )

  if (PLAY_STORE_URL) {
    return (
      <a
        className="get-app-badge"
        href={PLAY_STORE_URL}
        target="_blank"
        rel="noopener noreferrer"
      >
        {badge(
          <FaGooglePlay className="get-app-badge-icon" aria-hidden="true" />,
          copy.playStorePrefix,
          copy.playStoreName,
        )}
      </a>
    )
  }

  if (apkReady) {
    // Labels come from the download page's own copy rather than a second set of
    // strings, so the two surfaces read identically as well as agreeing.
    return (
      <Link className="get-app-badge" to="/download">
        {badge(
          <LuDownload className="get-app-badge-icon" aria-hidden="true" />,
          downloadCopy.forAndroid,
          downloadCopy.cta,
        )}
      </Link>
    )
  }

  return (
    <span className="get-app-badge is-disabled" aria-disabled="true">
      {badge(
        <FaGooglePlay className="get-app-badge-icon" aria-hidden="true" />,
        copy.comingSoon,
        copy.playStoreName,
      )}
    </span>
  )
}

/**
 * One app screen in a phone frame.
 *
 * The images are the real captures kept in docs/screenshots/, taken on a Pixel
 * 6a against a live API — nothing here is mocked up or redrawn. The alt text
 * names the screen rather than announcing itself as a screenshot, which is what
 * someone hearing it read out actually wants to know.
 */
function PhoneFrame({ screenshot, label, raised }) {
  return (
    <div className={raised ? 'get-app-phone is-raised' : 'get-app-phone'}>
      <div className="get-app-phone-screen">
        <img src={screenshot} alt={label} loading="lazy" />
      </div>

      <span className="get-app-phone-label">{label}</span>
    </div>
  )
}

function GetApp() {
  const { translations } = useLanguage()
  const copy = translations.home.getApp

  return (
    <section className="get-app-section" id={GET_APP_ID}>
      <motion.div
        className="get-app-phones"
        aria-label={copy.screensLabel}
        initial={{ opacity: 0, y: 40 }}
        whileInView={{ opacity: 1, y: 0 }}
        viewport={{ once: true, amount: 0.2 }}
        transition={{ duration: 0.6 }}
      >
        {copy.screens.map((screen, index) => (
          <PhoneFrame
            key={screen}
            label={screen}
            screenshot={screenshots[index]}
            raised={index === 1}
          />
        ))}
      </motion.div>

      <motion.div
        className="get-app-content"
        initial={{ opacity: 0, y: 40 }}
        whileInView={{ opacity: 1, y: 0 }}
        viewport={{ once: true, amount: 0.2 }}
        transition={{ duration: 0.6, delay: 0.1 }}
      >
        <span className="section-eyebrow">{copy.eyebrow}</span>

        <h2>{copy.title}</h2>

        <p className="get-app-lead">{copy.description}</p>

        <ul className="get-app-features">
          {copy.features.map(([title, description], index) => {
            const Icon = featureIcons[index]

            return (
              <li className="get-app-feature" key={title}>
                <span className="home-card-icon">
                  <Icon />
                </span>

                <div>
                  <h3>{title}</h3>
                  <p>{description}</p>
                </div>
              </li>
            )
          })}
        </ul>

        <p className="get-app-rate">{copy.rate}</p>

        <StoreBadge copy={copy} downloadCopy={translations.download} />
      </motion.div>
    </section>
  )
}

export default GetApp
