import HeroBG from '../assets/videos/HeroBG.mp4'
import galleryPic1 from '../assets/images/shop/gallery-1.jpg'
import galleryPic2 from '../assets/images/shop/gallery-2.jpg'
import galleryPic3 from '../assets/images/shop/gallery-3.jpg'
import galleryPic4 from '../assets/images/shop/gallery-4.jpg'

import { useEffect, useState } from 'react'
import { Link, useLocation } from 'react-router-dom'
import { motion, useReducedMotion } from 'motion/react'
import { LuCoffee } from 'react-icons/lu'
import { getBeanImage } from '../constants/beanImages.js'
import { BEST_SELLERS } from '../constants/bestSellers.js'
import { getMenuImage } from '../constants/menuImages.js'
import { useLanguage } from '../context/LanguageContext.jsx'
import GetApp from '../components/GetApp.jsx'
import { GET_APP_ID, scrollToGetApp } from '../components/getAppAnchor.js'

function BeanPhoto({ src, alt }) {
  const [loaded, setLoaded] = useState(false)

  return (
    <div className="bean-card-media">
      {src ? (
        <img
          src={src}
          alt={alt}
          loading="lazy"
          className={loaded ? 'is-loaded' : undefined}
          onLoad={() => setLoaded(true)}
        />
      ) : (
        <span className="bean-card-mark" aria-hidden="true">
          <LuCoffee />
        </span>
      )}
    </div>
  )
}

function Home() {
  const { translations } = useLanguage()
  const location = useLocation()

  // `whileInView` does not consult the platform preference on its own, so the
  // entrance props are dropped entirely when reduced motion is asked for — the
  // section is simply there, at rest, on the first frame.
  const reduceMotion = useReducedMotion()

  // The same distance, duration and easing the rest of the page uses, so this
  // section arrives feeling like part of it rather than its own effect. The
  // margin starts the run just before the element clears the fold, which is
  // what stops it being noticed as an animation at all.
  const rise = (delay = 0) =>
    reduceMotion
      ? {}
      : {
          initial: { opacity: 0, y: 30 },
          whileInView: { opacity: 1, y: 0 },
          viewport: { once: true, amount: 0.15, margin: '0px 0px -10% 0px' },
          transition: { duration: 0.55, delay, ease: 'easeOut' },
        }

  // Arriving from another page's app link, which navigates to /#get-app. The
  // section is below the fold and its images may still be settling, so this
  // waits for the next frame rather than scrolling against a moving layout.
  useEffect(() => {
    if (location.hash !== `#${GET_APP_ID}`) return

    const frame = requestAnimationFrame(scrollToGetApp)

    return () => cancelAnimationFrame(frame)
  }, [location])

  // Read straight out of the menu rather than kept as a second copy: the name,
  // the price, the description and the photo are all the ones the menu page
  // shows, so a change there cannot leave this section quietly wrong.
  const bestSellers = BEST_SELLERS.map(({ category, item, tag }) => {
    const [name, price, about] =
      translations.menu.categories[category]?.items[item] ?? []

    return {
      id: `${category}-${item}`,
      name,
      price,
      about,
      tag: translations.home.bestSellerTags[tag] ?? '',
      img: getMenuImage(category, item),
    }
  }).filter((entry) => entry.name)

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

      {/* Beans */}
      <section className="beans-section">
        {/* The heading arrives with the cards rather than sitting there while
            they fade in under it — the section reads as one movement, the way
            the rest of the page does. */}
        <motion.div className="section-heading" {...rise(0)}>
          <span className="section-eyebrow">{translations.home.beansEyebrow}</span>

          <h2>{translations.home.beansTitle}</h2>

          <p>{translations.home.beansIntro}</p>
        </motion.div>

        <ul className="beans-grid">
          {translations.home.beans.map(([name, description], index) => (
            <motion.li
              className="bean-card"
              key={name}
              {...rise(0.12 + index * 0.09)}
            >
              <BeanPhoto src={getBeanImage(index)} alt={name} />

              <div className="bean-card-body">
                <h3>{name}</h3>

                {/* Renders only when a description exists, so an empty string
                    leaves a clean name-and-photo card and needs no JSX change. */}
                {description && <p>{description}</p>}
              </div>
            </motion.li>
          ))}
        </ul>
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
                {item.img && <img src={item.img} alt={item.name} />}
              </div>

              <div className="best-seller-info">
                <div className="best-seller-title">
                  <h3>{item.name}</h3>
                  {/* Now a real price, read from the menu. This slot existed
                      before and rendered nothing, because the old hard-coded
                      list had no price field at all. */}
                  {item.price && <span>{item.price}</span>}
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

      {/* Get the app — last on the page, directly above the footer, which is
          where docs/Website_Info.md places it. */}
      <GetApp />
    </main>
  )
}

export default Home