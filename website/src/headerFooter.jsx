import { FaInstagram, FaFacebookF, FaTwitter } from 'react-icons/fa'
function Header() {
  return (
    <header>
      <span className="logo">نكهة فنجان</span>
      <nav>
        <a href="#hero">الرئيسية</a>
        <a href="#menu">المنيو</a>
        <a href="#">قصتنا</a>
        <a href="#">التواصل</a>
        <a href="#">app</a>
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
          <li>        <a href="#hero">الرئيسية</a>
          </li>
          <li>        <a href="#menu">المنيو</a>
          </li>
          <li>        <a href="#">قصتنا</a>
          </li>
          <li>        <a href="#">التواصل</a>
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