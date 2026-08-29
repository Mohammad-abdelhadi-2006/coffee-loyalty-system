import { motion } from 'motion/react'
import aboutUsPic from '../assets/images/aboutUsPic.png'
import { useLanguage } from '../context/LanguageContext.jsx'

function AboutUs() {
  const { translations } = useLanguage()

  return (
    <main className="story-page">
      <header className="story-title">
        <div className="story-decoration story-decoration-left" />
        <div className="story-decoration story-decoration-right" />

        <div className="story-title-content">
          <p className="tag">{translations.about.tag}</p>

          <h1>{translations.about.title}</h1>

          <span className="story-line" />

          <p className="story-p">
            {translations.about.subtitle}
          </p>
        </div>
      </header>

      <section className="story-container">
        <motion.div
          className="story-text"
          initial={{ opacity: 0, x: -35 }}
          whileInView={{ opacity: 1, x: 0 }}
          viewport={{ once: true, amount: 0.25 }}
          transition={{ duration: 0.6 }}
        >
          <p className="text-tag">
            {translations.about.startedTag}
          </p>

          <h2>{translations.about.startedTitle}</h2>

          <span className="small-line" />

          <p>{translations.about.paragraphs[0]}</p>
          <p>{translations.about.paragraphs[1]}</p>
        </motion.div>

        <motion.figure
          className="story-pic"
          initial={{ opacity: 0, x: 35 }}
          whileInView={{ opacity: 1, x: 0 }}
          viewport={{ once: true, amount: 0.25 }}
          transition={{ duration: 0.6, delay: 0.1 }}
        >
          <div className="image-frame">
            <img
              src={aboutUsPic}
              alt={translations.about.title}
            />
          </div>
        </motion.figure>
      </section>

      <motion.section
        className="founder-text"
        initial={{ opacity: 0, y: 25 }}
        whileInView={{ opacity: 1, y: 0 }}
        viewport={{ once: true }}
        transition={{ duration: 0.6 }}
      >
        <span className="quote-mark">“</span>

        <h3>{translations.about.quote}</h3>

        <p>- {translations.about.founder}</p>
      </motion.section>
    </main>
  )
}

export default AboutUs