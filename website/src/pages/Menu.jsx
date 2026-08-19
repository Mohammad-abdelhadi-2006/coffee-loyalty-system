import { motion } from 'motion/react';

const hotCoffee = [
  { id: 1, name: 'سبانش لاتيه', price: '1.50 JD', about: 'إسبريسو وحليب مبخّر مع لمسة حليب مكثّف محلّى.' },
  { id: 2, name: 'لاتيه', price: '1.50 JD', about: 'إسبريسو مع حليب مبخّر وطبقة رغوة خفيفة.' },
  { id: 3, name: 'كابتشينو', price: '1.50 JD', about: 'ثلث إسبريسو، ثلث حليب، وثلث رغوة كثيفة.' },
  { id: 4, name: 'اميركانو', price: '1.00 JD', about: 'إسبريسو مخفّف بالماء الساخن — أخف وأطول.' },
  { id: 5, name: 'V60', price: '1.50 JD', about: 'تقطير يدوي بفلتر ورقي، نكهة صافية وخفيفة.' },
  { id: 6, name: 'اسبريسو', price: '0.75 JD', about: 'شوت مركّز من البن المطحون طازجاً.' },
  { id: 7, name: 'دبل شوت اسبريسو', price: '1.00 JD', about: 'ضعف كمية الإسبريسو لطعم أقوى وأعمق.' },
  { id: 8, name: 'موكا دارك', price: '2.00 JD', about: 'إسبريسو وشوكولاتة داكنة مع حليب مبخّر.' },
  { id: 9, name: 'فلات وايت', price: '1.50 JD', about: 'إسبريسو مزدوج مع حليب مخملي ورغوة رقيقة.' },
  { id: 10, name: 'قهوة تركي', price: '0.50 JD', about: 'بن مطحون ناعم يُغلى ببطء برغوة دافئة.' },
  { id: 11, name: 'RED EYE', price: '1.50 JD', about: 'قهوة مفلترة مع شوت إسبريسو — طاقة مضاعفة.' },
]

const coldCoffee = [
  { id: 1, name: 'ايس سبانش لاتيه', price: '1.50 JD', about: 'إسبريسو وحليب بارد مع حليب مكثّف على ثلج.' },
  { id: 2, name: 'ايس بستاشيو لاتيه', price: '1.50 JD', about: 'إسبريسو وحليب بارد بنكهة الفستق على ثلج.' },
  { id: 3, name: 'ايس لاتيه', price: '1.50 JD', about: 'إسبريسو وحليب بارد على ثلج — خفيف ومنعش.' },
  { id: 4, name: 'ايس موكا', price: '2.00 JD', about: 'إسبريسو وشوكولاتة وحليب بارد على ثلج.' },
  { id: 5, name: 'ايس اميركانو', price: '1.00 JD', about: 'إسبريسو مع ماء بارد وثلج — منعش وقوي.' },
  { id: 6, name: 'ايس V60', price: '1.50 JD', about: 'قهوة مقطّرة يدوياً تُقدّم باردة على ثلج.' },
  { id: 7, name: 'فرابتشينو', price: '2.00 JD', about: 'قهوة وحليب وثلج مخفوقة بقوام كريمي.' },
]

const mojito = [
  { id: 1, name: 'بلو كارساو', price: '1.75 JD', about: 'نعنع وليمون مع شراب البرتقال الأزرق على ثلج.' },
  { id: 2, name: 'موهيتو', price: '1.75 JD', about: 'نعنع طازج وليمون على ثلج مجروش.' },
  { id: 3, name: 'فراولة', price: '1.75 JD', about: 'نعنع وليمون مع الفراولة — حلو ومنعش.' },
  { id: 4, name: 'مكس بيري', price: '1.75 JD', about: 'خليط توت أحمر مع نعنع وليمون على ثلج.' },
  { id: 5, name: 'بطيخ', price: '1.75 JD', about: 'بطيخ منعش مع نعنع ولمسة ليمون.' },
  { id: 6, name: 'مانجو', price: '1.75 JD', about: 'مانجو استوائي مع نعنع وليمون على ثلج.' },
  { id: 7, name: 'باشن فروت', price: '1.75 JD', about: 'باشن فروت حامض مع نعنع طازج وثلج.' },
]

const milkshake = [
  { id: 1, name: 'باشينجو', price: '2.00 JD', about: 'باشن فروت ومانجو مخفوقان مع الحليب.' },
  { id: 2, name: 'ميلينجو', price: '2.00 JD', about: 'مانجو وحليب مخفوق بقوام مخملي كثيف.' },
  { id: 3, name: 'فراولة', price: '2.00 JD', about: 'فراولة وحليب مخفوقان مع لمسة آيس كريم.' },
  { id: 4, name: 'مكس بيري', price: '2.00 JD', about: 'خليط التوت مع الحليب المخفوق — حلو وحامض.' },
  { id: 5, name: 'بطيخ', price: '2.00 JD', about: 'بطيخ طازج مخفوق مع الحليب والثلج.' },
  { id: 6, name: 'مانجو', price: '2.00 JD', about: 'مانجو وحليب مخفوقان لقوام كريمي غني.' },
  { id: 7, name: 'باشن فروت', price: '2.00 JD', about: 'باشن فروت وحليب مخفوق — منعش ومختلف.' },
]

const desserts = [
  { id: 1, name: 'كوكيز', price: '0.50 JD', about: 'كوكيز طري بقطع الشوكولاتة، يُخبز يومياً.' },
  { id: 2, name: 'كرواسون', price: '1.00 JD', about: 'عجينة زبدة مورّقة، مقرمشة من الخارج.' },
  { id: 3, name: 'دونات', price: '1.00 JD', about: 'دونات طرية بطبقة سكرية — من بلانِت دونات.' },
]
function Menu() {
  return (
    <main>
      <div className="menu-header">
        <h1>قائمة قهوتنا المميزة</h1>
      </div>

      <section className="menu-section">
        <h2>قهوة ساخنة</h2>
        <ul className="items">
          {hotCoffee.map((item, index) => (
            <motion.li
            key={item.id}
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.4, delay: index * 0.08 }}
>
              <div className="item-info">
                <div className="namePrice">
                  <span>{item.name}</span>
                  <span className="price">{item.price}</span>
                </div>

                <p>{item.about}</p>
              </div>
            </motion.li>
          ))}
        </ul>

        <h2>قهوة باردة</h2>
        <ul className="items">
          {coldCoffee.map((item, index) => (
            <motion.li
            key={item.id}
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.4, delay: index * 0.08 }}
>
              <div className="item-info">
                <div className="namePrice">
                  <span>{item.name}</span>
                  <span className="price">{item.price}</span>
                </div>

                <p>{item.about}</p>
              </div>
            </motion.li>
          ))}
        </ul>

        <h2>موهيتو</h2>
        <ul className="items">
          {mojito.map((item, index) => (
            <motion.li
            key={item.id}
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.4, delay: index * 0.08 }}
>
              <div className="item-info">
                <div className="namePrice">
                  <span>{item.name}</span>
                  <span className="price">{item.price}</span>
                </div>

                <p>{item.about}</p>
              </div>
            </motion.li>
          ))}
        </ul>

        <h2>ميلك شيك</h2>
        <ul className="items">
          {milkshake.map((item, index) => (
            <motion.li
            key={item.id}
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.4, delay: index * 0.08 }}
>
              <div className="item-info">
                <div className="namePrice">
                  <span>{item.name}</span>
                  <span className="price">{item.price}</span>
                </div>

                <p>{item.about}</p>
              </div>
            </motion.li>
          ))}
        </ul>

        <h2>حلويات</h2>
        <ul className="items">
          {desserts.map((item, index) => (
            <motion.li
            key={item.id}
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.4, delay: index * 0.08 }}
>
              <div className="item-info">
                <div className="namePrice">
                  <span>{item.name}</span>
                  <span className="price">{item.price}</span>
                </div>

                <p>{item.about}</p>
              </div>
            </motion.li>
          ))}
        </ul>
        
      </section>

    </main>
  )
}

export default Menu