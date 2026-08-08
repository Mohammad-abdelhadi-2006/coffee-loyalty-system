import { FaInstagram, FaFacebookF, FaTwitter } from 'react-icons/fa'
import { Link } from 'react-router-dom'
function Header() {
  return (
    <header>
       <Link to="/"> <span className="logo">نكهة فنجان</span></Link>
      <nav>
        <Link to="/">الرئيسية</Link>
        <Link to="/menu">المنيو</Link>
        <Link to="/aboutus">قصتنا</Link>
        <Link to="/contact">التواصل</Link>
        <Link to="#">app</Link>
      </nav>
      <a href="#">English</a>
    </header>
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
          <a href="https://twitter.com/..." target='_blank'><FaTwitter /></a>
        </div>
      </div>
    </footer>
  )
}

export { Header, Footer }