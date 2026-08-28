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

import { LuFlame, LuHouse, LuClock, LuCoffee, LuDroplets, LuSparkles } from 'react-icons/lu'
import { motion } from 'motion/react';
import { useLanguage } from '../context/LanguageContext.jsx'

const bestSellerImages = [
  { id: 1, name: 'قهوة تركي ', price: '0.50 JD', img: turkishCoffeePic, tag: 'الأكثر مبيعا', about: 'ما بتنشرب على عجل. قهوة تركي بطعم مركّز ورغوة دافئة — للحظة بدك تاخدها على راحتك.' },
  { id: 2, name: 'آيس موكا', price: '2.00 JD', img: iceMochaPic, tag: 'ذات شعبية', about: 'إسبريسو داكن، شوكولاتة غنية، وحليب بارد على تلج. الحلا والقهوة بكوب واحد — بدون ما تختار بينهم.' },
  { id: 3, name: 'موهيتو', price: '1.75 JD', img: mojitoPic, tag: '', about: 'نعنع طازج ولمسة حامض على تلج مجروش.' },
  { id: 4, name: 'ميلك شيك - فراولة', price: '2.00 JD', img: milkshakeStrawberryPic, tag: '', about: 'باشن فروت وحليب مخفوق لقوام مخملي — حلو، حامض، وبارد بنفس الرشفة.' }

]
function Home() {
  const { translations } = useLanguage()
  const bestSellers = bestSellerImages.map((item, index) => ({
    ...item,
    name: translations.home.bestSellers[index][0],
    tag: translations.home.bestSellers[index][1],
    about: translations.home.bestSellers[index][2],
  }))

  return (
    <main>
      <section id="hero">
        <video src={HeroBG} autoPlay muted loop playsInline />
        <div id="hero-text">
          <h1>{translations.home.heroTitle}</h1>
          <p>{translations.home.heroDescription}</p>
          <div className="hero-btn">
            <Link to="/menu" className="btn-primary">{translations.home.menuButton}</Link>
            <Link to="/contact" className="btn-outline">{translations.home.locationButton}</Link>
          </div>
        </div>
      </section>

      <section className='quality'>
        <motion.div
          className='card'
          initial={{ opacity: 0, y: 100 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ duration: 0.5 }}>
          <span className="card-icon"><LuClock /></span>
          <h3>{translations.home.quality[0][0]}</h3>
          <p>{translations.home.quality[0][1]}</p>
        </motion.div>

        <motion.div
          className='card'
          initial={{ opacity: 0, y: 100 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ duration: 0.5, delay: 0.15 }}>
          <span className="card-icon"><LuHouse /></span>
          <h3>{translations.home.quality[1][0]}</h3>
          <p>{translations.home.quality[1][1]}</p>
        </motion.div>

        <motion.div
          className='card'
          initial={{ opacity: 0, y: 100 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ duration: 0.5, delay: 0.3 }}>
          <span className="card-icon"><LuFlame /></span>
          <h3>{translations.home.quality[2][0]}</h3>
          <p>{translations.home.quality[2][1]}</p>
        </motion.div>
      </section>

      <section className='crafted-coffee'>
        <h2>{translations.home.craftedTitle}</h2>
        <p>{translations.home.craftedDescription}</p>
        <div className='card-container'>
          <motion.div
          className='card'
          initial={{ opacity: 0, x: 100 }}
          whileInView={{ opacity: 1, x: 0 }}
          viewport={{ once: true }}
          transition={{ duration: 0.5}}>
            <span className='card-icon'><LuCoffee /></span>
            <h3>{translations.home.crafted[0][0]}</h3>
            <p>{translations.home.crafted[0][1]}</p>
          </motion.div>

          <motion.div
          className='card'
          initial={{ opacity: 0, x: 100 }}
          whileInView={{ opacity: 1, x: 0 }}
          viewport={{ once: true }}
          transition={{ duration: 0.5, delay: 0.15 }}>
            <span className='card-icon'><LuDroplets /></span>
            <h3>{translations.home.crafted[1][0]}</h3>
            <p>{translations.home.crafted[1][1]}</p>
          </motion.div>

          <motion.div
          className='card'
          initial={{ opacity: 0, x: 100 }}
          whileInView={{ opacity: 1, x: 0 }}
          viewport={{ once: true }}
          transition={{ duration: 0.5, delay: 0.3 }}>
            <span className='card-icon'><LuSparkles /></span>
            <h3>{translations.home.crafted[2][0]}</h3>
            <p>{translations.home.crafted[2][1]}</p>
          </motion.div>
        </div>
      </section>

      <section className='why-us'>
        <div>
          <img src={WhyUsPic} alt="" />
        </div>
        <div className="why-text">
          <h2>{translations.home.whyTitle}</h2>
          <p>{translations.home.whyDescription}</p>
          <div className="hero-btn">
            <Link to="/menu" className="btn-primary">{translations.home.menuButton}</Link>
          </div>
        </div>
      </section>

      <section className='best-sellers'>
        <h2>{translations.home.bestSellersTitle}</h2>
        <ul className='card-container'>
          {bestSellers.map(item => (
            <li key={item.id}>
              <img src={item.img} alt={item.name} />
              <div className='bestSellers-cardInfo'>
                <div className='namePrice-bestSellers'>
                  <h3>{item.name}</h3>
                  <span>{item.price}</span>
                </div>
                <span className='tag'>{item.tag}</span>
                <p>{item.about}</p>
              </div>
            </li>
          ))}
        </ul>
      </section>

      <section className='gallery'>
        <div className='galleryPic'>
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