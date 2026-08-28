import { useState } from 'react'
import emailjs from '@emailjs/browser'
import { LuMapPin, LuPhone, LuClock } from 'react-icons/lu'
import { useLanguage } from '../context/LanguageContext.jsx'

function Contact() {
  const { translations } = useLanguage()
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
      setStatus(translations.contact.success)
    } catch (error) {
      console.error('EmailJS send failed:', error)
      setStatus(translations.contact.failure)
    } finally {
      setIsSending(false)
    }
  }

  return (
    <main>
      <div className="contact-header">
        <h1>{translations.contact.title}</h1>
        <p>{translations.contact.subtitle}</p>
      </div>

      <section className="contact">
        <div className="send-message">
          <form onSubmit={handleSubmit}>
            <h3>{translations.contact.formTitle}</h3>
            <div className="row">
              <input type="text" name="name" placeholder={translations.contact.name} required />
              <input type="email" name="email" placeholder={translations.contact.email} required />
            </div>
            <input type="text" name="subject" placeholder={translations.contact.subject} className="topic" required />
            <textarea name="message" placeholder={translations.contact.message} className="message" required></textarea>
            <input
              type="submit"
              value={isSending ? translations.contact.sending : translations.contact.send}
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
            <h5>{translations.contact.address}</h5>
            <p>{translations.contact.addressValue}</p>
          </div>
          </div>
          <div className="call">
             <span className="card-icon"><LuPhone /></span>
             <div className='contact-info'>
            <h5>{translations.contact.phone}</h5>
            <ul>
              <li><a href={`tel:${translations.contact.phoneValue}`}>{translations.contact.phoneValue}</a></li>
              <li><a href={`mailto:${translations.contact.emailValue}`}>{translations.contact.emailValue}</a></li>
            </ul>
            </div>
          </div>
          <div className="opening-time">
             <span className="card-icon"><LuClock /></span>
             <div className='contact-info'>
            <h5>{translations.contact.hours}</h5>
            <p>{translations.contact.weekdays}</p>
            <p>{translations.contact.friday}</p>
            </div>
          </div>
        </div>
      </section>
    </main>
  )
}

export default Contact