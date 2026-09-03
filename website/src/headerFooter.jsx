import { FaInstagram, FaFacebookF } from 'react-icons/fa'
import { Link, useLocation } from 'react-router-dom'
import { useEffect, useId, useState } from 'react'
import { LuMenu, LuX } from 'react-icons/lu'
import { motion, useScroll, useMotionValueEvent } from 'motion/react'
import logoImg from './assets/images/logo.png'
import { useLanguage } from './context/LanguageContext.jsx'
import { CONTACT } from './context/translations.js'

function Header() {
  const [hidden, setHidden] = useState(false)
  const [menuOpen, setMenuOpen] = useState(false)
  const { language, toggleLanguage, translations } = useLanguage()
  const { scrollY } = useScroll()
  const location = useLocation()
  const menuId = useId()

  useMotionValueEvent(scrollY, 'change', (current) => {
    setHidden(current > scrollY.getPrevious() && current > 151)
  })

  // Navigating closes the menu. Without this it stays open over the page the
  // visitor just asked for.
  useEffect(() => {
    setMenuOpen(false)
  }, [location.pathname])

  // While the panel is open the page behind it must not scroll under the
  // finger, and Escape has to close it — both are what people expect of an
  // overlay, and neither comes for free.
  useEffect(() => {
    if (!menuOpen) return

    const previousOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'

    const onKeyDown = (event) => {
      if (event.key === 'Escape') setMenuOpen(false)
    }

    window.addEventListener('keydown', onKeyDown)

    return () => {
      document.body.style.overflow = previousOverflow
      window.removeEventListener('keydown', onKeyDown)
    }
  }, [menuOpen])

  const links = [
    ['/', translations.nav.home],
    ['/menu', translations.nav.menu],
    ['/aboutus', translations.nav.story],
    ['/contact', translations.nav.contact],
    ['/download', translations.nav.app],
  ]

  return (
    <motion.header
      className={menuOpen ? 'transparent is-open' : 'transparent'}
      // The bar hides itself on scroll-down; doing that while its own menu is
      // open would take the menu off screen with it.
      animate={{ y: hidden && !menuOpen ? '-100%' : 0 }}
      transition={{ duration: 0.35 }}
    >
      <Link to="/" className="logo-link">
        <img src={logoImg} alt="نكهة فنجان" className="logo-img" />
      </Link>

      {/* One list, two presentations: a row on a wide screen, a panel under the
          bar on a phone. Rendering it once keeps the two in step and means a
          link never exists in one and not the other. */}
      <nav id={menuId} className={menuOpen ? 'is-open' : undefined}>
        {links.map(([to, label]) => (
          <Link key={to} to={to} onClick={() => setMenuOpen(false)}>
            {label}
          </Link>
        ))}

        {/* Inside the panel on a phone, so the language switch is reachable
            there too — it used to sit off the edge of the screen entirely. */}
        <button
          type="button"
          className="lang-toggle in-menu"
          onClick={toggleLanguage}
          aria-label="Change language"
        >
          {language === 'ar' ? translations.nav.english : translations.nav.arabic}
        </button>
      </nav>

      <button
        type="button"
        className="lang-toggle on-bar"
        onClick={toggleLanguage}
        aria-label="Change language"
      >
        {language === 'ar' ? translations.nav.english : translations.nav.arabic}
      </button>

      <button
        type="button"
        className="nav-toggle"
        onClick={() => setMenuOpen((open) => !open)}
        aria-expanded={menuOpen}
        aria-controls={menuId}
        aria-label={menuOpen ? 'إغلاق القائمة' : 'فتح القائمة'}
      >
        {menuOpen ? <LuX /> : <LuMenu />}
      </button>

      {/* Tapping away closes it, which on a phone is the gesture people try
          first. Hidden from assistive tech: the toggle already does this. */}
      {menuOpen && (
        <div
          className="nav-backdrop"
          onClick={() => setMenuOpen(false)}
          aria-hidden="true"
        />
      )}
    </motion.header>
  )
}

function Footer() {
  const { translations } = useLanguage()

  return (
    <footer>
      <div className="footer-inner">
        <div>
          <Link to="/" className="logo-link">
        <img src={logoImg} alt="نكهة فنجان" className="logo-img" />
      </Link>
          <p>{translations.footer.description}</p>
        </div>

        <div>
          <h3>{translations.footer.quickLinks}</h3>
          <ul>
            <li><Link to="/">{translations.nav.home}</Link>
            </li>
            <li><Link to="/menu">{translations.nav.menu}</Link>
            </li>
            <li><Link to="/aboutus">{translations.nav.story}</Link>
            </li>
            <li><Link to="/contact">{translations.nav.contact}</Link>
            </li>
          </ul>
        </div>

        <div>
          <h3>{translations.footer.openingHours}</h3>
          <p>{translations.footer.hoursDaily}</p>
        </div>

        <div>
          <h3>{translations.footer.contact}</h3>
          <ul>
            <li>
              <a href={`tel:${CONTACT.phoneDial}`}>
                {translations.footer.phone}
              </a>
            </li>
            <li>
              <a
                href={CONTACT.whatsApp}
                target="_blank"
                rel="noopener noreferrer"
              >
                {translations.footer.whatsapp}
              </a>
            </li>
            {/* Rendered the moment CONTACT.email stops being null — see translations.js. */}
            {CONTACT.email && (
              <li>
                <a href={`mailto:${CONTACT.email}`}>
                  {translations.footer.email}
                </a>
              </li>
            )}
            <li>{translations.footer.address}</li>
          </ul>
        </div>

        <div className="social">
          <h4>{translations.footer.followUs}</h4>
          {/*
            Only the two accounts the shop actually has. The X/Twitter icon that
            used to sit here pointed at a placeholder URL and no such account
            appears anywhere in the app source, so it is gone rather than
            guessed at.
          */}
          <a
            href={CONTACT.instagram}
            target="_blank"
            rel="noopener noreferrer"
            aria-label="Instagram"
          >
            <FaInstagram />
          </a>
          <a
            href={CONTACT.facebook}
            target="_blank"
            rel="noopener noreferrer"
            aria-label="Facebook"
          >
            <FaFacebookF />
          </a>
        </div>
      </div>
    </footer>
  )
}

export { Header, Footer }