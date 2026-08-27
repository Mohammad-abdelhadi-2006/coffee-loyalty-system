import { useState } from 'react'
import { Keyboard as KeyboardIcon } from 'lucide-react'
import logo from '../assets/logo.png'
import Keyboard from '../components/keyboard.jsx'
import { FIELDS } from '../constants/fields.js'
import { login } from '../api/auth.js'

export default function Login({ onDone }) {
  const [username, setUsername]   = useState('')
  const [password, setPassword]   = useState('')
  const [openField, setOpenField] = useState(null)
  const [error, setError]         = useState('')
  const [loading, setLoading]     = useState(false)

  const submit = async (e) => {
    e.preventDefault()

    if (!username.trim() || !password) {
      setError('لازم تعبّي اسم المستخدم وكلمة المرور')
      return
    }

    setError('')
    setLoading(true)

    try {
      const session = await login(username.trim(), password)
      onDone(session)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className={openField ? 'login-page kbd-open' : 'login-page'}>

      <form className="login-card" onSubmit={submit}>

        <img src={logo} alt="نكهة فنجان" className="login-logo" />
        <h1 className="login-title">نكهة فنجان</h1>
        <p className="login-subtitle">نظام نقاط البيع</p>

        <div className="field">
          <label htmlFor="username">اسم المستخدم</label>
          <div className="input-row">
            <input id="username" type="text" value={username}
                   onChange={e => setUsername(e.target.value)} />
            <button type="button"
                    className={openField === 'username' ? 'kbd-btn on' : 'kbd-btn'}
                    onClick={() => setOpenField('username')}>
              <KeyboardIcon size={22} />
            </button>
          </div>
        </div>

        <div className="field">
          <label htmlFor="password">كلمة المرور</label>
          <div className="input-row">
            <input id="password" type="password" value={password}
                   onChange={e => setPassword(e.target.value)} />
            <button type="button"
                    className={openField === 'password' ? 'kbd-btn on' : 'kbd-btn'}
                    onClick={() => setOpenField('password')}>
              <KeyboardIcon size={22} />
            </button>
          </div>
        </div>

        {error && <p className="login-error">{error}</p>}

        <button type="submit" className="login-button" disabled={loading}>
          {loading ? 'جاري الدخول...' : 'دخول'}
        </button>

      </form>

      {openField === 'username' && (
        <Keyboard value={username} onChange={setUsername}
                  config={FIELDS.username} onClose={() => setOpenField(null)} />
      )}

      {openField === 'password' && (
        <Keyboard value={password} onChange={setPassword}
                  config={FIELDS.password} onClose={() => setOpenField(null)} />
      )}

    </div>
  )
}