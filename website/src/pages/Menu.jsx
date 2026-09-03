import { useEffect, useState } from 'react'
import { getMenuCategoryIcon } from '../../constants/menuCategoryIcons.js'
import { getMenuImage } from '../constants/menuImages.js'
import { useMenuCardMotion } from '../hooks/useMenuCardMotion.js'
import { useLanguage } from '../context/LanguageContext.jsx'

const getCategoryKey = (category, index = -1) =>
  category?.key ??
  category?.id ??
  category?.slug ??
  category?.category ??
  category?.title ??
  `cat-${index}`

/**
 * One product.
 *
 * The photo slot is always drawn, whether or not a photo exists — an item with
 * no picture yet gets a tinted panel carrying its category mark, so the grid
 * stays even and the page reads as finished rather than half-loaded. See
 * `src/constants/menuImages.js` for where the files go.
 *
 * The card carries no animation state of its own: `registerCard` hands the node
 * to the observer, and everything after that is a class and two custom
 * properties read by CSS.
 */
function MenuCard({ name, price, about, image, Icon, registerCard }) {
  return (
    <li className="menu-card" ref={registerCard}>
      <div className="menu-card-media">
        {image ? (
          <img src={image} alt={name} loading="lazy" />
        ) : (
          <span className="menu-card-media-mark" aria-hidden="true">
            <Icon size={38} strokeWidth={1.4} />
          </span>
        )}
      </div>

      <div className="menu-card-body">
        <div className="menu-card-title">
          <h3>{name}</h3>
          {/* Hidden while the price is unknown rather than showing an empty
              chip — a blank price reads as a bug, a missing one reads as
              "ask". See the TODOs in translations.js. */}
          {price && <span className="menu-card-price">{price}</span>}
        </div>

        <p>{about}</p>
      </div>
    </li>
  )
}

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

  // Keyed on the filter, so switching category rebuilds the observer and the
  // incoming cards stagger in rather than appearing already settled.
  const registerCard = useMenuCardMotion(activeCategory)

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
        {/* Scrolls sideways rather than wrapping, so the row stays one line on a
            phone and the selected chip can be scrolled back into view. */}
        <nav className="menu-category-nav" aria-label={translations.menu.filterLabel}>
          <button
            type="button"
            className={`menu-category-button${
              activeCategory === 'all' ? ' active' : ''
            }`}
            aria-pressed={activeCategory === 'all'}
            onClick={() => setActiveCategory('all')}
          >
            <span>{translations.menu.all}</span>
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

          // The original index is carried along because it is what the photo
          // table is keyed on — splitting the list must not renumber anything.
          const entries = category.items.map(([name, price, about], index) => ({
            name,
            price,
            about,
            index,
            image: getMenuImage(categoryIndex, index),
          }))
          const withPhoto = entries.filter((entry) => entry.image)
          const withoutPhoto = entries.filter((entry) => !entry.image)

          return (
            // The filter is part of the key on purpose: going from "all" to a
            // single category otherwise reuses these nodes, and the new set
            // would swap in with no transition at all.
            <div
              className="menu-category"
              key={`${activeCategory}-${categoryKey}`}
            >
              <div className="category-heading">
                <h2>{category.title}</h2>
                <span className="category-line" />
                <span className="category-count">
                  {category.items.length} {translations.menu.itemsWord}
                </span>
              </div>

              {/* Two shapes for one category: anything with a photo gets a
                  card, and the rest are listed underneath. A grid of empty
                  stand-ins for a third of the menu looked unfinished; a list is
                  a deliberate way to show an item that simply has no picture
                  yet. */}
              {withPhoto.length > 0 && (
                <ul className="menu-grid">
                  {withPhoto.map(({ name, price, about, index }) => (
                    <MenuCard
                      key={`${categoryKey}-${name}`}
                      name={name}
                      price={price}
                      about={about}
                      image={getMenuImage(categoryIndex, index)}
                      Icon={CategoryIcon}
                      registerCard={registerCard}
                    />
                  ))}
                </ul>
              )}

              {withoutPhoto.length > 0 && (
                <ul className="menu-plain-list">
                  {withoutPhoto.map(({ name, price, about }) => (
                    <li className="menu-plain-item" key={`${categoryKey}-${name}`}>
                      <span className="menu-plain-mark" aria-hidden="true">
                        <CategoryIcon size={20} strokeWidth={1.6} />
                      </span>

                      <div className="menu-plain-body">
                        <div className="menu-plain-title">
                          <h3>{name}</h3>
                          {price && (
                            <span className="menu-card-price">{price}</span>
                          )}
                        </div>

                        {about && <p>{about}</p>}
                      </div>
                    </li>
                  ))}
                </ul>
              )}
            </div>
          )
        })}
      </section>
    </main>
  )
}

export default Menu
