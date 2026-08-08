import HeroPic from '../assets/images/HeroPic.jpg'
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

const bestSellers = [
  { id: 1, name: 'قهوة تركي ', price: '0.50 JD', img: turkishCoffeePic, tag: 'الأكثر مبيعا', about: 'ما بتنشرب على عجل. قهوة تركي بطعم مركّز ورغوة دافئة — للحظة بدك تاخدها على راحتك.' },
  { id: 2, name: 'آيس موكا', price: '2.00 JD', img: iceMochaPic, tag: 'ذات شعبية', about: 'إسبريسو داكن، شوكولاتة غنية، وحليب بارد على تلج. الحلا والقهوة بكوب واحد — بدون ما تختار بينهم.' },
  { id: 3, name: 'موهيتو', price: '1.75 JD', img: mojitoPic, tag: '', about: 'نعنع طازج ولمسة حامض على تلج مجروش.' },
  { id: 4, name: 'ميلك شيك - فراولة', price: '2.00 JD', img: milkshakeStrawberryPic, tag: '', about: 'باشن فروت وحليب مخفوق لقوام مخملي — حلو، حامض، وبارد بنفس الرشفة.' }

]
function Home() {
  return (
    <main>
      <section id="hero">
        <div>
          <img src={HeroPic} alt="" />
        </div>
        <div id="hero-text">
          <h1>قهوة طازجة لكل مزاج</h1>
          <p>نكهات غنية وأجواء دافئة كل يوم.</p>
          <div className="hero-btn">
            <Link to="/menu" className="btn-primary">المنيو</Link>
            <Link to="/contact" className="btn-outline">موقعنا</Link>
          </div>
        </div>
      </section>

      <section className='quality'>
        <div className='card'>
          <span className="card-icon"><LuClock /></span>
          <h3>خدمة سريعة و ودودة</h3>
          <p>من الطلب الأول حتى الرشفة الاخيرة.<br /> نجعل كل زيارة سلسة و دافئة و لا <br />تنسى.</p>        </div>

        <div className='card'>
          <span className="card-icon"><LuHouse /></span>
          <h3>اجواء مقهى مريحة</h3>
          <p>مساحة ترحيبية للاجتماعات ,و اللحظات الهادئة ,و الالتقاء بالاصدقاءعلى فنجان قهوة .</p>
        </div>

        <div className='card'>
          <span className="card-icon"><LuFlame /></span>
          <h3>حبوب محمصة طازجة</h3>
          <p>نختار حبوبا فاخرة و نحمصها بعناية لإبراز رائحة عميقة و نكهة قوية في كل كوب.</p>
        </div>
      </section>

      <section className='crafted-coffee'>
        <h2>تشكيلة قهوة مميزة</h2>
        <p>من الإسبريسو القوي إلى الخلطات الكريمية الناعمة، خيارات تناسب كل ذوق.</p>
        <div className='card-container'>
          <div className='card'>
            <span className='card-icon'><LuCoffee /></span>
            <h3>إسبريسو كلاسيكي</h3>
            <p>خيارات جريئة وغنية لمحبّي الطعم القوي والأصيل.</p>
          </div>

          <div className='card'>
            <span className='card-icon'><LuDroplets /></span>
            <h3>نكهات كريمية</h3>
            <p>لاتيه وكابتشينو بقوام حليب حريري ونكهة متوازنة.</p>
          </div>

          <div className='card'>
            <span className='card-icon'><LuSparkles /></span>
            <h3>خلطاتنا الخاصة</h3>
            <p>وصفات خاصة من مطبخنا تمنحك تجربة فريدة في كل رشفة.</p>
          </div>
        </div>
      </section>

      <section className='why-us'>
        <div>
          <img src={WhyUsPic} alt="" />
        </div>
        <div className="why-text">
          <h2>كوب واحد بيغيّر يومك</h2>
          <p>ما منحضّر قهوة بس. منحضّر اللحظة اللي بتوقف فيها شوي وتاخد نفس.<br /> تفضل, الكوب جاهز.</p>
          <div className="hero-btn">
            <Link to="/menu" className="btn-primary">المنيو</Link>
          </div>
        </div>
      </section>

      <section className='best-sellers'>
        <h2>الأكثر مبيعا لدينا</h2>
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