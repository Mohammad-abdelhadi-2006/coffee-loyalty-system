import { useState, useEffect } from 'react'
import { UserPlus, Keyboard as KeyboardIcon } from 'lucide-react'
import Keyboard from '../components/keyboard.jsx'
import { FIELDS } from '../constants/fields.js'
import { getEmployees, createEmployee, setEmployeeStatus } from '../api/employees.js'

const EMPTY = { fullName: '', username: '', password: '', role: 'cashier' }

const ROLES = [
  { key: 'cashier', label: 'cashier' },
  { key: 'admin',   label: 'admin' },
]

export default function Employees({ session }) {
  const [list, setList]       = useState([])
  const [loading, setLoading] = useState(true)
  const [err, setErr]         = useState('')

  const [form, setForm]       = useState(null)
  const [formErr, setFormErr] = useState('')
  const [saving, setSaving]   = useState(false)

  const [openField, setOpenField] = useState(null)

  const load = async () => {
    try {
      setList(await getEmployees())
    } catch (e) {
      setErr(e.message)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  const setField = (k, v) => setForm({ ...form, [k]: v })

  const inputs = {
    employeeName: { value: form?.fullName ?? '', set: v => setField('fullName', v) },
    newUsername:  { value: form?.username ?? '', set: v => setField('username', v) },
    newPassword:  { value: form?.password ?? '', set: v => setField('password', v) },
  }

  const isMe = (e) => e.username === session.username

  const save = async () => {
    if (!form.fullName.trim())   return setFormErr('لازم تكتب الاسم الكامل')
    if (!form.username.trim())   return setFormErr('لازم تكتب اسم المستخدم')
    if (form.password.length < 8) return setFormErr('كلمة المرور لازم ٨ محارف فأكثر')

    setFormErr(''); setSaving(true)
    try {
      await createEmployee({
        fullName: form.fullName.trim(),
        username: form.username.trim(),
        password: form.password,
        role: form.role,
      })
      setForm(null)
      await load()
    } catch (e) {
      setFormErr(e.message)
    } finally {
      setSaving(false)
    }
  }

  const toggle = async (e) => {
    try {
      const updated = await setEmployeeStatus(e.id, !e.isActive)
      setList(list.map(x => x.id === e.id ? updated : x))
    } catch (ex) {
      setErr(ex.message)
    }
  }

  const kbdBtn = (name) => (
    <button type="button" className="kbd-btn" tabIndex={-1}
            onClick={() => setOpenField(name)}>
      <KeyboardIcon size={20} />
    </button>
  )

  return (
    <div className="page">

      <div className="page-head">
        <h1 className="page-title">الموظفين</h1>
        <button type="button" className="mini big-mini"
                onClick={() => { setForm(EMPTY); setFormErr('') }}>
          <UserPlus size={18} /> موظف جديد
        </button>
      </div>

      {err && <p className="login-error">{err}</p>}
      {loading && <p className="hint">جاري التحميل...</p>}

      {!loading && list.length > 0 && (
        <div className="tablewrap">
          <table className="tbl">
            <thead>
              <tr>
                <th>الاسم</th>
                <th className="mid">اسم المستخدم</th>
                <th>الدور</th>
                <th>الحالة</th>
              </tr>
            </thead>
            <tbody>
              {list.map(e => (
                <tr key={e.id}>
                  <td>
                    <div className="cellname">
                      {e.fullName}
                      {isMe(e) && <span className="metag">أنت</span>}
                    </div>
                  </td>
                  <td className="ltr mid">{e.username}</td>
                  <td>{e.role === 'admin' ? 'admin' : 'cashier'}</td>
                  <td>
                    <button type="button"
                            className={e.isActive ? 'pill on' : 'pill'}
                            disabled={isMe(e)}
                            onClick={() => toggle(e)}>
                      {e.isActive ? 'مفعّل' : 'معطّل'}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {form && (
        <div className="modal-bg">
          <div className="modal">
            <h3>موظف جديد</h3>

            <span className="wlabel">الاسم الكامل</span>
            <div className="input-row">
              <input type="text" value={form.fullName}
                     onChange={ev => setField('fullName', ev.target.value)} />
              {kbdBtn('employeeName')}
            </div>

            <span className="wlabel">اسم المستخدم</span>
            <div className="input-row">
              <input type="text" className="ltr" value={form.username}
                     onChange={ev => setField('username', ev.target.value)} />
              {kbdBtn('newUsername')}
            </div>

            <span className="wlabel">كلمة المرور</span>
            <div className="input-row">
              <input type="text" className="ltr" value={form.password}
                     onChange={ev => setField('password', ev.target.value)} />
              {kbdBtn('newPassword')}
            </div>
            <p className="hint">٨ محارف فأكثر. اكتبها للموظف قبل ما تسكّر النافذة.</p>

            <span className="wlabel">الدور</span>
            <select className="sel" value={form.role}
                    onChange={ev => setField('role', ev.target.value)}>
              {ROLES.map(r => <option key={r.key} value={r.key}>{r.label}</option>)}
            </select>

            {formErr && <p className="login-error">{formErr}</p>}

            <div className="wact">
              <button type="button" className="login-button" disabled={saving} onClick={save}>
                {saving ? 'جاري الإنشاء...' : 'إنشاء'}
              </button>
              <button type="button" className="wcancel" onClick={() => setForm(null)}>إلغاء</button>
            </div>
          </div>
        </div>
      )}

      {openField && (
        <Keyboard value={inputs[openField].value}
                  onChange={inputs[openField].set}
                  config={FIELDS[openField]}
                  onClose={() => setOpenField(null)} />
      )}

    </div>
  )
}