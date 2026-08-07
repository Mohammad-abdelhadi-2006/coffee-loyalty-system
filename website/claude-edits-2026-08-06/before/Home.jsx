import HeroPic from './assets/images/HeroPic.jpg'

function Hero() {
  return (
    <section id="hero">
      <div>
        <img src={HeroPic} alt="" />
      </div>
      <div id="hero-text">
      <h1>قهوة طازجة لكل مزاج</h1>
      <p>نكهات غنية وأجواء دافئة كل يوم.</p>
      <div className="hero-btn">
  <button id='menubtn'>المنيو</button>
  <button id='location-btn'>موقعنا</button>
</div>
      </div>
    </section>

  )
}

export default Hero