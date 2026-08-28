import { useState } from 'react'
import emailjs from '@emailjs/browser'
import { LuMapPin, LuPhone, LuClock } from 'react-icons/lu'

function Contact() {
  const [isSending, setIsSending] = useState(false)
  const [status, setStatus] = useState('')

  async function handleSubmit(event) {
    event.preventDefault()
    setIsSending(true)
    setStatus('')

    const form = event.currentTarget
    const formData = new FormData(form)

    try {
      await emailjs.send(
        'service_8v3armg',
        'template_tpodqo2',
        {
          name: formData.get('name'),
          email: formData.get('email'),
          subject: formData.get('subject'),
          message: formData.get('message'),
        },
        'rIdyy0GwQ_WSZCZe5',
      )
      form.reset()
      setStatus('تم إرسال رسالتك بنجاح.')
    } catch (error) {
      console.error('EmailJS send failed:', error)
      setStatus('تعذر إرسال الرسالة. حاول مرة أخرى.')
    } finally {
      setIsSending(false)
    }
  }

  return (
    <main>
      <div className="contact-header">
        <h1>نسعد بسماع رأيك</h1>
        <p>سواء عندك سؤال، اقتراح، أو حجز مناسبة — فريقنا جاهز للرد عليك.</p>
      </div>

      <section className="contact">
        <div className="send-message">
          <form onSubmit={handleSubmit}>
            <h3>ارسل لنا رسالة</h3>
            <div className="row">
              <input type="text" name="name" placeholder="الأسم كامل" required />
              <input type="email" name="email" placeholder="البريد الألكتروني" required />
            </div>
            <input type="text" name="subject" placeholder="الموضوع" className="topic" required />
            <textarea name="message" placeholder="رسالتك" className="message" required></textarea>
            <input
              type="submit"
              value={isSending ? 'جاري الإرسال...' : 'ارسل الرسالة'}
              className="submit-message"
              disabled={isSending}
            />
            {status && <p role="status">{status}</p>}
          </form>
        </div>
        <div className="contact-us">
          <div className="location">
             <span className="card-icon"><LuMapPin /></span>
             <div className='contact-info'>
            <h5>العنوان</h5>
            <p>الرصيفة , شارع السعادة</p>
          </div>
          </div>
          <div className="call">
             <span className="card-icon"><LuPhone /></span>
             <div className='contact-info'>
            <h5>الهاتف و التواصل</h5>
            <ul>
              <li><a href="tel:+962771234567">+962771234567</a></li>
              <li><a href="mailto:exm@gmail.com">exm@gmail.com</a></li>
            </ul>
            </div>
          </div>
          <div className="opening-time">
             <span className="card-icon"><LuClock /></span>
             <div className='contact-info'>
            <h5>ساعات العمل</h5>
            <p>السبت - الخميس : 8:00 صباحا - 10:00 مساءً</p>
            <p>الجمعة : 8:00 صباحا - 12:00 ليلاً</p>
            </div>
          </div>
        </div>
      </section>
    </main>
  )
}

export default Contact