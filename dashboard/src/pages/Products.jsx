import { useState, useEffect } from 'react'
import { Plus, Pencil, Trash2, Keyboard as KeyboardIcon } from 'lucide-react'
import Keyboard from '../components/keyboard.jsx'
import { FIELDS } from '../constants/fields.js'
import { CATEGORIES, UNITS } from '../constants/catalog.js'
import { CATEGORY_ICON } from '../constants/categoryIcons.js'
import {
  getProducts, createProduct, updateProduct, deleteProduct, setAvailability,
} from '../api/products.js'

const EMPTY = { name: '', price: '', unitType: 'Piece', category: 'HotCoffee' }

const TABS = [{ key: 'all', label: 'الكل' }, ...CATEGORIES]

export default function Products({ session }) {
  const isAdmin = session.role === 'admin'

  const [list, setList]       = useState([])
  const [loading, setLoading] = useState(true)
  const [err, setErr]         = useState('')

  const [cat, setCat] = useState('all')

  const [form, setForm]       = useState(null)
  const [editId, setEditId]   = useState(null)
  const [formErr, setFormErr] = useState('')
  const [saving, setSaving]   = useState(false)

  const [confirmDel, setConfirmDel] = useState(null)
  const [openField, setOpenField]   = useState(null)

  const load = async () => {
    try {
      setList(await getProducts())
    } catch (e) {
      setErr(e.message)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  const setField = (k, v) => setForm({ ...form, [k]: v })

  const inputs = {
    productName:  { value: form?.name  ?? '', set: v => setField('name', v) },
    productPrice: { value: form?.price ?? '', set: v => setField('price', v) },
  }

  const visible = cat === 'all' ? list : list.filter(p => p.category === cat)

  const labelOf = (arr, k) => arr.find(x => x.key === k)?.label ?? k

  const openAdd = () => {
    setForm(EMPTY); setEditId(null); setFormErr('')
  }

  const openEdit = (p) => {
    setForm({ name: p.name, price: String(p.price), unitType: p.unitType, category: p.category })
    setEditId(p.id); setFormErr('')
  }

  const save = async () => {
    const price = Number(form.price)
    if (!form.name.trim()) return setFormErr('لازم تكتب اسم المنتج')
    if (!(price > 0))      return setFormErr('السعر لازم يكون أكبر من صفر')

    setFormErr(''); setSaving(true)
    try {
      const body = {
        name: form.name.trim(),
        price,
        unitType: form.unitType,
        category: form.category,
      }
      if (editId) await updateProduct(editId, body)
      else        await createProduct(body)
      setForm(null); setEditId(null)
      await load()
    } catch (e) {
      setFormErr(e.message)
    } finally {
      setSaving(false)
    }
  }

  const remove = async () => {
    const id = confirmDel.id
    setConfirmDel(null)
    try {
      await deleteProduct(id)
      await load()
    } catch (e) {
      setErr(e.message)
    }
  }

  const toggle = async (p) => {
    try {
      const updated = await setAvailability(p.id, !p.isAvailable)
      setList(list.map(x => x.id === p.id ? updated : x))
    } catch (e) {
      setErr(e.message)
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
        <h1 className="page-title">المنتجات</h1>
        {isAdmin && (
          <button type="button" className="mini big-mini" onClick={openAdd}>
            <Plus size={18} /> منتج جديد
          </button>
        )}
      </div>

      <div className="tabs">
        {TABS.map(c => (
          <button key={c.key} type="button"
                  className={cat === c.key ? 'tab on' : 'tab'}
                  onClick={() => setCat(c.key)}>{c.label}</button>
        ))}
      </div>

      {err && <p className="login-error">{err}</p>}
      {loading && <p className="hint">جاري التحميل...</p>}
      {!loading && visible.length === 0 && <p className="hint">ما في منتجات بهاي الفئة</p>}

      {visible.length > 0 && (
        <div className="tablewrap">
          <table className="tbl">
            <thead>
              <tr>
                <th>المنتج</th>
                <th>الفئة</th>
                <th>الوحدة</th>
                <th>السعر</th>
                <th>التوفّر</th>
                {isAdmin && <th>إجراءات</th>}
              </tr>
            </thead>
            <tbody>
              {visible.map(p => {
                const Icon = CATEGORY_ICON[p.category]
                return (
                  <tr key={p.id}>
                    <td>
                      <div className="cellname">
                        <span className="minithumb">{Icon && <Icon size={18} />}</span>
                        {p.name}
                      </div>
                    </td>
                    <td>{labelOf(CATEGORIES, p.category)}</td>
                    <td>{labelOf(UNITS, p.unitType)}</td>
                    <td className="num">{p.price.toFixed(3)} د</td>
                    <td>
                      <button type="button"
                              className={p.isAvailable ? 'pill on' : 'pill'}
                              onClick={() => toggle(p)}>
                        {p.isAvailable ? 'متوفّر' : 'غير متوفّر'}
                      </button>
                    </td>
                    {isAdmin && (
                      <td>
                        <div className="line-qty">
                          <button type="button" onClick={() => openEdit(p)}><Pencil size={16} /></button>
                          <button type="button" className="del" onClick={() => setConfirmDel(p)}><Trash2 size={16} /></button>
                        </div>
                      </td>
                    )}
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      )}

      {form && (
        <div className="modal-bg">
          <div className="modal">
            <h3>{editId ? 'تعديل منتج' : 'منتج جديد'}</h3>

            <span className="wlabel">اسم المنتج</span>
            <div className="input-row">
              <input type="text" value={form.name}
                     onChange={e => setField('name', e.target.value)} />
              {kbdBtn('productName')}
            </div>

            <span className="wlabel">السعر (د.أ)</span>
            <div className="input-row">
              <input type="text" value={form.price}
                     onChange={e => setField('price', e.target.value)} />
              {kbdBtn('productPrice')}
            </div>

            <span className="wlabel">الفئة</span>
            <select className="sel" value={form.category}
                    onChange={e => setField('category', e.target.value)}>
              {CATEGORIES.map(c => <option key={c.key} value={c.key}>{c.label}</option>)}
            </select>

            <span className="wlabel">وحدة البيع</span>
            <select className="sel" value={form.unitType}
                    onChange={e => setField('unitType', e.target.value)}>
              {UNITS.map(u => <option key={u.key} value={u.key}>{u.label}</option>)}
            </select>

            {formErr && <p className="login-error">{formErr}</p>}

            <div className="wact">
              <button type="button" className="login-button" disabled={saving} onClick={save}>
                {saving ? 'جاري الحفظ...' : 'حفظ'}
              </button>
              <button type="button" className="wcancel" onClick={() => setForm(null)}>إلغاء</button>
            </div>
          </div>
        </div>
      )}

      {confirmDel && (
        <div className="modal-bg">
          <div className="modal">
            <h3>حذف منتج</h3>
            <p className="hint">
              رح ينحذف «{confirmDel.name}» من القائمة. الطلبات القديمة بتضل محفوظة زي ما هي.
            </p>
            <div className="wact">
              <button type="button" className="danger-button" onClick={remove}>حذف</button>
              <button type="button" className="wcancel" onClick={() => setConfirmDel(null)}>إلغاء</button>
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