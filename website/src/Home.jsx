import HeroPic from './assets/images/HeroPic.jpg'

function Hero() {
  return (
    <section id="hero">
      <div>
        <img src={HeroPic} alt="فنجان قهوة ساخنة بجانب حبوب البن" />
      </div>
      <div id="hero-text">
        <h1>قهوة طازجة لكل مزاج</h1>
        <p>نكهات غنية وأجواء دافئة كل يوم.</p>
        <div className="hero-btn">
          <button className="btn-primary">المنيو</button>
          <button className="btn-outline">موقعنا</button>
        </div>
      </div>
    </section>

  )
}

export default Hero