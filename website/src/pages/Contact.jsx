import { useState } from 'react'
import emailjs from '@emailjs/browser'
import { motion } from 'motion/react'
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
    <main className="contact-page">
      <header className="contact-header">
        <div className="contact-decoration contact-decoration-left" />
        <div className="contact-decoration contact-decoration-right" />

        <div className="contact-header-content">
          <p className="contact-tag">✦ Contact</p>

          <h1>{translations.contact.title}</h1>

          <span className="contact-line" />

          <p>{translations.contact.subtitle}</p>
        </div>
      </header>

      <section className="contact">
        <motion.div
          className="send-message"
          initial={{ opacity: 0, x: -35 }}
          whileInView={{ opacity: 1, x: 0 }}
          viewport={{ once: true, amount: 0.2 }}
          transition={{ duration: 0.6 }}
        >
          <form onSubmit={handleSubmit}>
            <div className="form-heading">
              <span>✦</span>
              <h2>{translations.contact.formTitle}</h2>
            </div>

            <div className="row">
              <input
                type="text"
                name="name"
                placeholder={translations.contact.name}
                aria-label={translations.contact.name}
                autoComplete="name"
                required
              />

              <input
                type="email"
                name="email"
                placeholder={translations.contact.email}
                aria-label={translations.contact.email}
                autoComplete="email"
                required
              />
            </div>

            <input
              type="text"
              name="subject"
              placeholder={translations.contact.subject}
              aria-label={translations.contact.subject}
              required
            />

            <textarea
              name="message"
              placeholder={translations.contact.message}
              aria-label={translations.contact.message}
              required
            />

            <button
              type="submit"
              className="submit-message"
              disabled={isSending}
            >
              <span>
                {isSending
                  ? translations.contact.sending
                  : translations.contact.send}
              </span>

              {!isSending && <span className="button-arrow">→</span>}
            </button>

            {status && (
              <p className="contact-status" role="status">
                {status}
              </p>
            )}
          </form>
        </motion.div>

        <motion.aside
          className="contact-us"
          initial={{ opacity: 0, x: 35 }}
          whileInView={{ opacity: 1, x: 0 }}
          viewport={{ once: true, amount: 0.2 }}
          transition={{ duration: 0.6, delay: 0.1 }}
        >
          <div className="contact-card">
            <span className="card-icon">
              <LuMapPin />
            </span>

            <div className="contact-info">
              <h3>{translations.contact.address}</h3>
              <p>{translations.contact.addressValue}</p>
            </div>
          </div>

          <div className="contact-card">
            <span className="card-icon">
              <LuPhone />
            </span>

            <div className="contact-info">
              <h3>{translations.contact.phone}</h3>

              <a href={`tel:${translations.contact.phoneValue}`}>
                {translations.contact.phoneValue}
              </a>

              <a href={`mailto:${translations.contact.emailValue}`}>
                {translations.contact.emailValue}
              </a>
            </div>
          </div>

          <div className="contact-card">
            <span className="card-icon">
              <LuClock />
            </span>

            <div className="contact-info">
              <h3>{translations.contact.hours}</h3>
              <p>{translations.contact.weekdays}</p>
              <p>{translations.contact.friday}</p>
            </div>
          </div>
        </motion.aside>
      </section>
    </main>
  )
}

export default Contact