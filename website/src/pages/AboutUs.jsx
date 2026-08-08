import aboutUsPic from '../assets/images/aboutUsPic.png'
function AboutUs() {
  return (
    <main>
      <div className='story-title'>
        <p className='tag'>قصتنا</p>
        <h2>حكاية بدأت من فكرة بسيطة</h2>
        <p className='story-p'>مقهى حي صغير تحوّل إلى بيت ثانٍ لعشاق القهوة.</p>
      </div>
      <div className='story-container'>
        <div className='story-text'>
          <p className='text-tag'>كيف بدأنا</p>
          <h2>من زاوية صغيرة إلى وجهة يومية<br/> لعشاق القهوة</h2>
          <p>بدأت نكهة فنجان بحلم بسيط: تقديم كوب قهوة يشعر فيه الزائر بالراحة والاهتمام في كل تفصيلة. افتتحنا أول فرع لنا في زاوية هادئة من المدينة، بمعدات متواضعة وشغف كبير بحبوب البن المحمصة بعناية.</p>
          <p>مع كل كوب قدّمناه، كبرت قصتنا بفضل ثقة زوارنا. اليوم أصبحت نكهة فنجان مساحة يجتمع فيها الأصدقاء، ويعمل فيها الطلاب، ويبدأ فيها الكثيرون صباحهم — وما زلنا نحمّص كل حبة بنفس الحب الذي بدأنا به.</p>
        </div>

        <div className='story-pic'>
          <img src={aboutUsPic} alt="" />
        </div>

      </div>

      <div className='founder-text'>
        <h4>"كل ما بنيناه هنا بدأ بفكرة واحدة: أن نجعل فنجان القهوة لحظة يستحقها كل زائر."</h4>
        <p>- مؤسس نكهة فنجان</p>
      </div>
    </main>
  )
}

export default AboutUs