import { motion } from 'motion/react'
import { useLanguage } from '../context/LanguageContext.jsx'

function Menu() {
  const { translations } = useLanguage()

  return (
    <main className="menu-page">
      <header className="menu-header">
        <div className="menu-decoration menu-decoration-left" />
        <div className="menu-decoration menu-decoration-right" />

        <div className="menu-header-content">
          <span className="menu-symbol">✦</span>
          <h1>{translations.menu.title}</h1>
          <span className="menu-line" />
        </div>
      </header>

      <section className="menu-section">
        {translations.menu.categories.map((category, categoryIndex) => (
          <div className="menu-category" key={category.title}>
            <div className="category-heading">
              <span className="category-number">
                {String(categoryIndex + 1).padStart(2, '0')}
              </span>

              <h2>{category.title}</h2>

              <span className="category-line" />
            </div>

            <ul className="items">
              {category.items.map(([name, price, about], index) => (
                <motion.li
                  key={`${category.title}-${name}`}
                  className="menu-item"
                  initial={{ opacity: 0, y: 24 }}
                  whileInView={{ opacity: 1, y: 0 }}
                  whileHover={{ y: -7 }}
                  viewport={{ once: true, amount: 0.2 }}
                  transition={{
                    duration: 0.45,
                    delay: index * 0.08,
                  }}
                >
                  <div className="item-number">
                    {String(index + 1).padStart(2, '0')}
                  </div>

                  <div className="item-info">
                    <div className="name-price">
                      <h3>{name}</h3>
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