import { useState } from 'react'
import Login from './pages/login.jsx'
import Order from './pages/Order.jsx'
import Products from './pages/Products.jsx'
import Employees from './pages/Employees.jsx'
import Returns from './pages/Returns.jsx'
import Sidebar from './components/Sidebar.jsx'
import { loadSession, clearSession } from './api/client.js'
import OrderLookup from './pages/OrderLookup.jsx'

export default function App() {
  const [session, setSession] = useState(loadSession)
  const [screen, setScreen] = useState('order')
  const [askOut, setAskOut] = useState(false)

  if (!session) return <Login onDone={setSession} />

  const logout = () => {
    clearSession()
    setSession(null)
    setAskOut(false)
    setScreen('order')
  }

  return (
    <div className="shell">

      <Sidebar session={session} screen={screen}
        onChange={setScreen} onLogout={() => setAskOut(true)} />

      <main className="main">
        {screen === 'order' && <Order />}
        {screen === 'returns' && <Returns />}
        {screen === 'lookup' && <OrderLookup />}
        {screen === 'products' && <Products session={session} />}
        {screen === 'employees' && <Employees session={session} />}
      </main>

      {askOut && (
        <div className="modal-bg">
          <div className="modal">
            <h3>تسجيل الخروج</h3>
            <p className="hint">
              متأكد بدك تطلع؟ أي طلب لسا ما تأكّد رح يضيع.
            </p>
            <div className="wact">
              <button type="button" className="danger-button" onClick={logout}>
                خروج
              </button>
              <button type="button" className="wcancel" onClick={() => setAskOut(false)}>
                إلغاء
              </button>
            </div>
          </div>
        </div>
      )}

    </div>
  )
}