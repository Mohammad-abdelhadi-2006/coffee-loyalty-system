function Contact() {
  return (
    <main>
      <div className="contact-header">
        <h1>نسعد بسماع رأيك</h1>
        <p>سواء عندك سؤال، اقتراح، أو حجز مناسبة — فريقنا جاهز للرد عليك.</p>
      </div>

      <section className="contact">
        <div className="send-message">
          <form action="">
            <h3>ارسل لنا رسالة</h3>
            <div className="row">
              <input type="text" placeholder="الأسم كامل" />
              <input type="email" placeholder="البريد الألكتروني" />
            </div>
            <input type="text" placeholder="الموضوع" className="topic" />
            <textarea placeholder="رسالتك" className="message"></textarea>
            <input type="submit" value="ارسل الرسالة" className="submit-message" />
          </form>
        </div>
        <div className="contact-us">
          <div className="location">
            <h5>العنوان</h5>
            <p>الرصيفة , شارع السعادة</p>
          </div>
          <div className="call">
            <h5>الهاتف و التواصل</h5>
            <ul>
              <li><a href="tel:00962771234567">+962 77 123 4567</a></li>
              <li><a href="mailto:exm@gmail.com">exm@gmail.com</a></li>
            </ul>
          </div>
          <div className="opening-time">
            <h3>ساعات العمل</h3>
            <p>السبت - الخميس : 8:00 صباحا - 10:00 مساءً</p>
            <p>الجمعة : 8:00 صباحا - 12:00 ليلاً</p>
          </div>
        </div>
      </section>
    </main>
  )
}

export default Contact