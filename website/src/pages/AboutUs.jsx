import aboutUsPic from '../assets/images/aboutUsPic.png'
import { useLanguage } from '../context/LanguageContext.jsx'
function AboutUs() {
  const { translations } = useLanguage()

  return (
    <main>
      <div className='story-title'>
        <p className='tag'>{translations.about.tag}</p>
        <h2>{translations.about.title}</h2>
        <p className='story-p'>{translations.about.subtitle}</p>
      </div>
      <div className='story-container'>
        <div className='story-text'>
          <p className='text-tag'>{translations.about.startedTag}</p>
          <h2>{translations.about.startedTitle}</h2>
          <p>{translations.about.paragraphs[0]}</p>
          <p>{translations.about.paragraphs[1]}</p>
        </div>

        <div className='story-pic'>
          <img src={aboutUsPic} alt="" />
        </div>

      </div>

      <div className='founder-text'>
        <h4>"{translations.about.quote}"</h4>
        <p>- {translations.about.founder}</p>
      </div>
    </main>
  )
}

export default AboutUs