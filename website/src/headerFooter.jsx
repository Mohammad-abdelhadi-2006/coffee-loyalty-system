import { FaInstagram, FaFacebookF } from 'react-icons/fa'
import { FaXTwitter } from 'react-icons/fa6'
import { Link, useLocation } from 'react-router-dom'
import { useState } from 'react'
import { motion, useScroll, useMotionValueEvent } from 'motion/react'
import logoImg from './assets/images/logo.png'
import { useLanguage } from './context/LanguageContext.jsx'

function Header() {
  const [hidden, setHidden] = useState(false)
  const { language, toggleLanguage, translations } = useLanguage()
  const { scrollY } = useScroll()

  useMotionValueEvent(scrollY, 'change', (current) => {
    setHidden(current > scrollY.getPrevious() && current > 151)
  })

  return (
    <motion.header
      className="transparent"
      animate={{ y: hidden ? '-100%' : 0 }}
      transition={{ duration: 0.35 }}
    >
      <Link to="/" className="logo-link">
        <img src={logoImg} alt="نكهة فنجان" className="logo-img" />
      </Link>
      <nav>
        <Link to="/">{translations.nav.home}</Link>
        <Link to="/menu">{translations.nav.menu}</Link>
        <Link to="/aboutus">{translations.nav.story}</Link>
        <Link to="/contact">{translations.nav.contact}</Link>
        <Link to="#">{translations.nav.app}</Link>
      </nav>
      <button type="button" onClick={toggleLanguage} aria-label="Change language">
        {language === 'ar' ? translations.nav.english : translations.nav.arabic}
      </button>
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
          <p>{translations.footer.weekdays}</p>
          <p>{translations.footer.friday}</p>
        </div>

        <div>
          <h3>{translations.footer.contact}</h3>
          <ul>
            <li><a href="mailto:exm@gmail.com">{translations.footer.email}</a></li>
            <li><a href="tel:+962771234567">{translations.footer.phone}</a></li>
            <li>{translations.footer.address}</li>
          </ul>
        </div>

        <div className="social">
          <h4>{translations.footer.followUs}</h4>
          <a href="https://instagram.com/..." target='_blank'><FaInstagram /></a>
          <a href="https://facebook.com/..." target='_blank'><FaFacebookF /></a>
          <a href="https://x.com/..." target='_blank'><FaXTwitter /></a>
        </div>
      </div>
    </footer>
  )
}

export { Header, Footer }