import { ShoppingCart, RotateCcw, Receipt, Package, Users, LogOut } from 'lucide-react'
import logo from '../assets/logo.png' 

/* Configuration array mapping screen keys, labels, icons, and authorized roles */
const SCREENS = [
  { key: 'order',     label: 'طلب جديد',   Icon: ShoppingCart, roles: ['cashier', 'admin'] },
  { key: 'returns',   label: 'المرتجعات', Icon: RotateCcw,    roles: ['cashier', 'admin'] },
  { key: 'lookup',    label: 'بحث عن طلب', Icon: Receipt,      roles: ['cashier', 'admin'] },
  { key: 'products',  label: 'المنتجات',  Icon: Package,      roles: ['cashier', 'admin'] },
  { key: 'employees', label: 'الموظفين',  Icon: Users,        roles: ['admin'] },
]

/* Sidebar navigation component that renders menu options filtered by user session role */
export default function Sidebar({ session, screen, onChange, onLogout }) {
  /* Filters available screens based on the current user's role (admin or cashier) */
  const allowed = SCREENS.filter(s => s.roles.includes(session.role))

  return (
    <aside className="side">

      <div className="side-brand">
        <img src={logo} alt="" className="side-logo" />
        <span className="side-name">نكهة فنجان</span>
      </div>

      <nav className="side-nav">
        {allowed.map(({ key, label, Icon }) => (
          <button key={key} type="button"
                  className={screen === key ? 'side-item on' : 'side-item'}
                  onClick={() => onChange(key)}>
            <Icon size={20} />
            <span>{label}</span>
          </button>
        ))}
      </nav>

      <div className="side-foot">
        <div className="side-user">
          <strong>{session.fullName}</strong>
          <small>{session.role === 'admin' ? 'admin' : 'cashier'}</small>
        </div>
        <button type="button" className="side-out" onClick={onLogout}>
          <LogOut size={18} />
          <span>خروج</span>
        </button>
      </div>

    </aside>
  )
}