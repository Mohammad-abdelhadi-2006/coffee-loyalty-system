import HeroBG from '../assets/videos/HeroBG.mp4'
import WhyUsPic from '../assets/images/WhyUsPic.png'
import turkishCoffeePic from '../assets/images/turkishCoffeePic.png'
import mojitoPic from '../assets/images/mojitoPic.png'
import iceMochaPic from '../assets/images/iceMochaPic.png'
import milkshakeStrawberryPic from '../assets/images/milkshakeStrawberryPic.png'
import galleryPic1 from '../assets/images/galleryPic1.png'
import galleryPic2 from '../assets/images/galleryPic2.png'
import galleryPic3 from '../assets/images/galleryPic3.png'
import galleryPic4 from '../assets/images/galleryPic4.png'

import { Link } from 'react-router-dom'
import { motion } from 'motion/react'
import {
  LuFlame,
  LuHouse,
  LuClock,
  LuCoffee,
  LuDroplets,
  LuSparkles,
} from 'react-icons/lu'
import { useLanguage } from '../context/LanguageContext.jsx'

const bestSellerImages = [
  {
    id: 1,
    img: turkishCoffeePic,
  },
  {
    id: 2,
    img: iceMochaPic,
  },
  {
    id: 3,
    img: mojitoPic,
  },
  {
    id: 4,
    img: milkshakeStrawberryPic,
  },
]

const qualityIcons = [LuClock, LuHouse, LuFlame]
const craftedIcons = [LuCoffee, LuDroplets, LuSparkles]

function Home() {
  const { translations } = useLanguage()

  const bestSellers = bestSellerImages.map((item, index) => ({
    ...item,
    name: translations.home.bestSellers[index][0],
    tag: translations.home.bestSellers[index][1],
    about: translations.home.bestSellers[index][2],
  }))

  return (
    <main className="home-page">
      {/* Hero */}
      <section className="home-hero">
        <video
          className="home-hero-video"
          src={HeroBG}
          autoPlay
          muted
          loop
          playsInline
        />

        <div className="home-hero-overlay" />

        <div className="hero-text">
          <span className="hero-eyebrow">✦ EST. WITH CARE</span>

          <h1>{translations.home.heroTitle}</h1>

          <p>{translations.home.heroDescription}</p>

          <div className="hero-buttons">
            <Link to="/menu" className="home-btn home-btn-primary">
              {translations.home.menuButton}
              <span>→</span>
            </Link>

            <Link to="/contact" className="home-btn home-btn-outline">
              {translations.home.locationButton}
            </Link>
          </div>
        </div>

        <div className="hero-corner-decoration" />
      </section>

      {/* Quality */}
      <section className="quality-section">
        {translations.home.quality.map(([title, description], index) => {
          const Icon = qualityIcons[index]

          return (
            <motion.article
              className="quality-card"
              key={title}
              initial={{ opacity: 0, y: 40 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true, amount: 0.2 }}
              transition={{
                duration: 0.5,
                delay: index * 0.12,
              }}
            >
              <span className="home-card-icon">
                <Icon />
              </span>

              <h3>{title}</h3>
              <p>{description}</p>
            </motion.article>
          )
        })}
      </section>

      {/* Crafted coffee */}
      <section className="crafted-section">
        <div className="section-heading">
          <span className="section-eyebrow">OUR PROMISE</span>

          <h2>{translations.home.craftedTitle}</h2>

          <p>{translations.home.craftedDescription}</p>
        </div>

        <div className="crafted-cards">
          {translations.home.crafted.map(([title, description], index) => {
            const Icon = craftedIcons[index]

            return (
              <motion.article
                className="crafted-card"
                key={title}
                initial={{ opacity: 0, y: 35 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true, amount: 0.2 }}
                transition={{
                  duration: 0.5,
                  delay: index * 0.12,
                }}
              >
                <span className="home-card-icon">
                  <Icon />
                </span>

                <h3>{title}</h3>
                <p>{description}</p>
              </motion.article>
            )
          })}
        </div>
      </section>

      {/* Why us */}
      <section className="why-section">
        <motion.div
          className="why-image-wrapper"
          initial={{ opacity: 0, x: -40 }}
          whileInView={{ opacity: 1, x: 0 }}
          viewport={{ once: true, amount: 0.2 }}
          transition={{ duration: 0.6 }}
        >
          <img src={WhyUsPic} alt={translations.home.whyTitle} />
        </motion.div>

        <motion.div
          className="why-content"
          initial={{ opacity: 0, x: 40 }}
          whileInView={{ opacity: 1, x: 0 }}
          viewport={{ once: true, amount: 0.2 }}
          transition={{ duration: 0.6, delay: 0.1 }}
        >
          <span className="section-eyebrow">WHY CHOOSE US</span>

          <h2>{translations.home.whyTitle}</h2>

          <p>{translations.home.whyDescription}</p>

          <Link to="/menu" className="home-btn home-btn-dark">
            {translations.home.menuButton}
            <span>→</span>
          </Link>
        </motion.div>
      </section>

      {/* Best sellers */}
      <section className="best-sellers-section">
        <div className="section-heading">
          <span className="section-eyebrow">FAVORITES</span>

          <h2>{translations.home.bestSellersTitle}</h2>
        </div>

        <ul className="best-sellers-grid">
          {bestSellers.map((item, index) => (
            <motion.li
              className="best-seller-card"
              key={item.id}
              initial={{ opacity: 0, y: 30 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true, amount: 0.2 }}
              transition={{
                duration: 0.45,
                delay: index * 0.08,
              }}
            >
              <div className="best-seller-image">
                <img src={item.img} alt={item.name} />
              </div>

              <div className="best-seller-info">
                <div className="best-seller-title">
                  <h3>{item.name}</h3>
                  <span>{item.price}</span>
                </div>

                {item.tag && (
                  <span className="best-seller-tag">
                    {item.tag}
                  </span>
                )}

                <p>{item.about}</p>
              </div>
            </motion.li>
          ))}
        </ul>
      </section>

      {/* Gallery */}
      <section className="home-gallery">
        <div className="gallery-heading">
          <span>✦</span>
          <p>Moments worth slowing down for</p>
          <span>✦</span>
        </div>

        <div className="gallery-grid">
          <img src={galleryPic1} alt="" />
          <img src={galleryPic2} alt="" />
          <img src={galleryPic3} alt="" />
          <img src={galleryPic4} alt="" />
        </div>
      </section>
    </main>
  )
}

export default Home