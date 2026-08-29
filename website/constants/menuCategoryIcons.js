import { createElement } from 'react'
import {
  Apple,
  CakeSlice,
  Cookie,
  Leaf,
  Sandwich,
  Soup,
  Utensils,
} from 'lucide-react'

const element = (type, props, ...children) =>
  createElement(type, props, ...children)

const svgProps = (size = 32, strokeWidth = 1.7) => ({
  width: size,
  height: size,
  viewBox: '0 0 32 32',
  fill: 'none',
  xmlns: 'http://www.w3.org/2000/svg',
  'aria-hidden': true,
  stroke: 'currentColor',
  strokeWidth,
  strokeLinecap: 'round',
  strokeLinejoin: 'round',
})

// فنجان قهوة ساخنة يخرج منه بخار
export function HotCoffeeIcon({ size = 32, strokeWidth = 1.7 }) {
  return element(
    'svg',
    svgProps(size, strokeWidth),
    element('path', {
      d: 'M7 13h14v6.2A4.8 4.8 0 0 1 16.2 24h-4.4A4.8 4.8 0 0 1 7 19.2V13Z',
    }),
    element('path', {
      d: 'M21 15h1.2a3.3 3.3 0 0 1 0 6.6H20',
    }),
    element('path', { d: 'M10 9.5c-1.2-1.5 1.8-2.1.6-4' }),
    element('path', { d: 'M15 9.5c-1.2-1.5 1.8-2.1.6-4' }),
    element('path', { d: 'M5 27h20' }),
  )
}

// كوب آيس كوفي مع مكعبات ثلج وشفاطة
export function IcedCoffeeIcon({ size = 32, strokeWidth = 1.7 }) {
  return element(
    'svg',
    svgProps(size, strokeWidth),
    element('path', { d: 'm9 10 1.3 15h11.4L23 10' }),
    element('path', { d: 'M8 10h16' }),
    element('path', { d: 'm17 10 1.8-5.8L22 3' }),
    element('path', { d: 'm12 14 2 2 2-2 2 2 2-2' }),
    element('path', { d: 'M10 7.5c2.2 1 7.8 1 11 0' }),
    element('path', { d: 'M7 27h18' }),
  )
}

// كاسة ميلك شيك بغطاء وشفاطة
export function MilkshakeIcon({ size = 32, strokeWidth = 1.7 }) {
  return element(
    'svg',
    svgProps(size, strokeWidth),
    element('path', { d: 'M9 10h14l-1.5 15h-11L9 10Z' }),
    element('path', {
      d: 'M8 10c.6-2.3 2.9-3.7 8-3.7s7.4 1.4 8 3.7H8Z',
    }),
    element('path', { d: 'M16 6.2V3' }),
    element('path', { d: 'm16 3 4-1' }),
    element('path', { d: 'M11 14h10' }),
    element('path', { d: 'M12 18h8' }),
    element('path', { d: 'M7 27h18' }),
  )
}

const normalize = (value) =>
  String(value ?? '')
    .toLowerCase()
    .trim()
    .replace(/[\s_-]+/g, '')

const rules = [
  {
    icon: MilkshakeIcon,
    names: [
      'milkshake',
      'milkshakes',
      'shake',
      'ميلكشيك',
      'ميلك شيك',
      'ميلك',
      'شيك',
      'ميلكشيكات',
    ],
  },
  {
    icon: IcedCoffeeIcon,
    names: [
      'cold',
      'colddrinks',
      'iced',
      'icedcoffee',
      'icecoffee',
      'coldcoffee',
      'بارد',
      'باردة',
      'مشروباتباردة',
      'قهوةباردة',
      'مثلج',
      'مثلجة',
    ],
  },
  {
    icon: HotCoffeeIcon,
    names: [
      'hot',
      'hotdrinks',
      'coffee',
      'espresso',
      'tea',
      'ساخن',
      'ساخنة',
      'مشروباتساخنة',
      'قهوة',
      'قهوةساخنة',
      'شاي',
    ],
  },
  {
    icon: CakeSlice,
    names: [
      'dessert',
      'desserts',
      'sweets',
      'cake',
      'حلويات',
      'حلو',
      'كيك',
      'معجناتحلوة',
    ],
  },
  {
    icon: Sandwich,
    names: [
      'food',
      'foods',
      'sandwich',
      'sandwiches',
      'meal',
      'وجبات',
      'طعام',
      'ساندويش',
      'ساندويشات',
      'مأكولات',
    ],
  },
  {
    icon: Soup,
    names: ['breakfast', 'soup', 'فطور', 'شوربة', 'شوربات'],
  },
  {
    icon: Apple,
    names: ['juice', 'juices', 'fresh', 'عصائر', 'عصير', 'فريش'],
  },
  {
    icon: Leaf,
    names: ['healthy', 'salad', 'herbal', 'صحي', 'سلطات', 'أعشاب'],
  },
  {
    icon: Cookie,
    names: ['bakery', 'baked', 'cookies', 'مخبوزات', 'كوكيز', 'بسكويت'],
  },
]

const fallbackIcons = [HotCoffeeIcon, Utensils, Leaf, Apple, Cookie, Soup, Sandwich]

export const getMenuCategoryIcon = (category, index = 0) => {
  const value = normalize(
    category?.key ??
      category?.id ??
      category?.slug ??
      category?.category ??
      category?.title,
  )

  const matchingRule = rules.find((rule) =>
    rule.names.some((name) => value.includes(normalize(name))),
  )

  return matchingRule?.icon || fallbackIcons[index % fallbackIcons.length]
}