import { FaInstagram, FaFacebookF } from 'react-icons/fa'
import { FaXTwitter } from 'react-icons/fa6'
import { Link, useLocation } from 'react-router-dom'
import { useState } from 'react'
import { motion, useScroll, useMotionValueEvent } from 'motion/react'
import logoImg from './assets/images/logo.png'

function Header() {
  const [hidden, setHidden] = useState(false)
  const { scrollY } = useScroll()

  useMotionValueEvent(scrollY, 'change', (current) => {
    setHidden(current > scrollY.getPrevious() && current > 151)
  })

  return (
    <motion.header
      className="transparent" // <--- أصبح شفافاً لكل الصفحات دائماً
      animate={{ y: hidden ? '-100%' : 0 }}
      transition={{ duration: 0.35 }}
    >
      <Link to="/" className="logo-link">
        <img src={logoImg} alt="نكهة فنجان" className="logo-img" />
      </Link>
      <nav>
        <Link to="/">الرئيسية</Link>
        <Link to="/menu">المنيو</Link>
        <Link to="/aboutus">قصتنا</Link>
        <Link to="/contact">التواصل</Link>
        <Link to="#">app</Link>
      </nav>
      <a href="#">English</a>
    </motion.header>
  )
}


function Footer() {
  return (
    <footer>
      <div className="footer-inner">
        <div>
          <h3>نكهة فنجان</h3>
          <p>نقدم لكم قهوة مصنوعة يدوياً , و لحظات دافئة كل يوم.</p>
        </div>

        <div>
          <h3>روابط سريعة</h3>
          <ul>
            <li>        <Link to="/Home">الرئيسية</Link>
            </li>
            <li>        <Link to="/Menu">المنيو</Link>
            </li>
            <li>        <Link to="/AboutUs">قصتنا</Link>
            </li>
            <li>        <Link to="/Contact">التواصل</Link>
            </li>
          </ul>
        </div>

        <div>
          <h3>ساعات العمل</h3>
          <p>السبت - الخميس : 8:00 صباحا - 10:00 مساءً</p>
          <p>الجمعة : 8:00 صباحا - 12:00 ليلاً</p>
        </div>

        <div>
          <h3>للتواصل</h3>
          <ul>
            <li><a href="mailto:exm@gmail.com">ايميل</a></li>
            <li><a href="tel:+962771234567">رقم الهاتف</a></li>
            <li>الرصيفة ,شارع السعادة.</li>
          </ul>
        </div>

        <div className="social">
          <h4>تابعونا</h4>
          <a href="https://instagram.com/..." target='_blank'><FaInstagram /></a>
          <a href="https://facebook.com/..." target='_blank'><FaFacebookF /></a>
          <a href="https://x.com/..." target='_blank'><FaXTwitter /></a>
        </div>
      </div>
    </footer>
  )
}

export { Header, Footer }