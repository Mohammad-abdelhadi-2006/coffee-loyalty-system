import { useEffect, useState } from 'react'
import { motion } from 'motion/react'
import { getMenuCategoryIcon } from '../../constants/menuCategoryIcons.js'
import { useLanguage } from '../context/LanguageContext.jsx'

const getCategoryKey = (category, index = -1) =>
  category?.key ??
  category?.id ??
  category?.slug ??
  category?.category ??
  category?.title ??
  `cat-${index}`

function Menu() {
  const { translations } = useLanguage()
  const categories = translations.menu.categories || []
  const [activeCategory, setActiveCategory] = useState('all')

  useEffect(() => {
    const activeStillExists =
      activeCategory === 'all' ||
      categories.some(
        (category, index) =>
          getCategoryKey(category, index) === activeCategory,
      )

    if (!activeStillExists) setActiveCategory('all')
  }, [activeCategory, categories])

  const visibleCategories =
    activeCategory === 'all'
      ? categories
      : categories.filter(
          (category, index) =>
            getCategoryKey(category, index) === activeCategory,
        )

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
        <nav className="menu-category-nav" aria-label="اختيار الصنف">
          <button
            type="button"
            className={`menu-category-button${
              activeCategory === 'all' ? ' active' : ''
            }`}
            aria-pressed={activeCategory === 'all'}
            onClick={() => setActiveCategory('all')}
          >
            <span>الكل</span>
          </button>

          {categories.map((category, index) => {
            const key = getCategoryKey(category, index)
            const Icon = getMenuCategoryIcon(category, index)
            const isActive = activeCategory === key

            return (
              <button
                key={key}
                type="button"
                className={`menu-category-button${isActive ? ' active' : ''}`}
                aria-pressed={isActive}
                onClick={() => setActiveCategory(key)}
              >
                <Icon size={17} strokeWidth={1.7} />
                <span>{category.title}</span>
              </button>
            )
          })}
        </nav>

        {visibleCategories.map((category) => {
          const categoryIndex = categories.indexOf(category)
          const CategoryIcon = getMenuCategoryIcon(category, categoryIndex)
          const categoryKey = getCategoryKey(category, categoryIndex)

          return (
            <div className="menu-category" key={categoryKey}>
              <div className="category-heading">
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
                    <span className="menu-item-icon" aria-hidden="true">
                      <CategoryIcon size={29} strokeWidth={1.5} />
                    </span>

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
          )
        })}
      </section>
    </main>
  )
}

export default Menu