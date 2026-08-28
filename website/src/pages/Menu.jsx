import { motion } from 'motion/react';
import { useLanguage } from '../context/LanguageContext.jsx'

function Menu() {
  const { translations } = useLanguage()

  return (
    <main>
      <div className="menu-header">
        <h1>{translations.menu.title}</h1>
      </div>

      <section className="menu-section">
        {translations.menu.categories.map((category) => (
          <div key={category.title}>
            <h2>{category.title}</h2>
            <ul className="items">
          {category.items.map(([name, price, about], index) => (
            <motion.li
            key={name}
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.4, delay: index * 0.08 }}
>
              <div className="item-info">
                <div className="namePrice">
                  <span>{name}</span>
                  <span className="price">{price}</span>
                </div>

                <p>{about}</p>
              </div>
            </motion.li>
          ))}
            </ul>
          </div>
        ))}
        
      </section>

    </main>
  )
}

export default Menu