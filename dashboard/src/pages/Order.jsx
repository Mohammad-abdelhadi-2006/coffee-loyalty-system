import { useState, useEffect } from 'react'
import { Search, Plus, Minus, Trash2, UserPlus, Coffee, Keyboard as KeyboardIcon } from 'lucide-react'
import Keyboard from '../components/keyboard.jsx'
import { FIELDS } from '../constants/fields.js'
import { getProducts } from '../api/products.js'
import { createOrder } from '../api/orders.js'
import { CATEGORY_ICON } from '../constants/categoryIcons.js'
import { findByPhone, createCustomer } from '../api/customers.js'
import { CATEGORIES } from '../constants/catalog.js'

const TABS = [{ key: 'all', label: 'الكل' }, ...CATEGORIES]

export default function Order() {
    const [products, setProducts] = useState([])
    const [loading, setLoading] = useState(true)
    const [loadErr, setLoadErr] = useState('')

    const [category, setCategory] = useState('all')
    const [search, setSearch] = useState('')

    const [phone, setPhone] = useState('')
    const [customer, setCustomer] = useState(null)
    const [custErr, setCustErr] = useState('')
    const [searching, setSearching] = useState(false)

    const [cart, setCart] = useState([])
    const [redeem, setRedeem] = useState('')
    const [weighing, setWeighing] = useState(null)
    const [wKg, setWKg] = useState('')
    const [wAmount, setWAmount] = useState('')
    const [wEdit, setWEdit] = useState(false)

    const [newCust, setNewCust] = useState(false)
    const [nName, setNName] = useState('')
    const [nPhone, setNPhone] = useState('')
    const [nErr, setNErr] = useState('')
    const [nSaving, setNSaving] = useState(false)

    const [sending, setSending] = useState(false)
    const [sendErr, setSendErr] = useState('')
    const [result, setResult] = useState(null)

    const [openField, setOpenField] = useState(null)


    useEffect(() => {
        async function load() {
            try {
                setProducts(await getProducts())
            } catch (e) {
                setLoadErr(e.message)
            } finally {
                setLoading(false)
            }
        }
        load()
    }, [])

    const visible = products.filter(p =>
        (category === 'all' || p.category === category) &&
        p.name.includes(search.trim())
    )

    const total = cart.reduce((sum, i) => sum + i.price * i.qty, 0)
    const points = Number(redeem) || 0
    const discount = points / 100
    const cashPaid = Math.max(0, total - discount)

    const searchCustomer = async () => {
        if (!phone.trim()) return
        setCustErr('')
        setSearching(true)
        try {
            setCustomer(await findByPhone(phone.trim()))
        } catch (e) {
            setCustomer(null)
            setCustErr(e.message)
        } finally {
            setSearching(false)
        }
    }

    const onKg = (v) => {
        setWKg(v)
        const n = Number(v)
        setWAmount(n > 0 ? (n * weighing.price).toFixed(3) : '')
    }

    const onAmount = (v) => {
        setWAmount(v)
        const n = Number(v)
        setWKg(n > 0 ? (n / weighing.price).toFixed(3) : '')
    }

    const openWeigh = (p, currentQty = null) => {
        setWeighing(p)
        setWEdit(currentQty !== null)
        setWKg(currentQty !== null ? String(currentQty) : '')
        setWAmount(currentQty !== null ? (currentQty * p.price).toFixed(3) : '')
    }

    const confirmWeigh = () => {
        const qty = Number(wKg)
        if (!(qty > 0)) return
        const p = weighing
        const found = cart.find(i => i.id === p.id)
        if (found) {
            setCart(cart.map(i => i.id === p.id
                ? { ...i, qty: wEdit ? qty : +(i.qty + qty).toFixed(3) }
                : i))
        } else {
            setCart([...cart, { id: p.id, name: p.name, price: p.price, unitType: 'Kg', qty }])
        }
        setWeighing(null)
    }

    const openNewCust = () => {
        setNName('')
        setNPhone(phone.trim())
        setNErr('')
        setNewCust(true)
    }

    const saveNewCust = async () => {
        if (!nName.trim() || !nPhone.trim()) {
            setNErr('لازم تعبّي الاسم ورقم الهاتف')
            return
        }
        setNErr('')
        setNSaving(true)
        try {
            const c = await createCustomer(nName.trim(), nPhone.trim())
            setCustomer(c)
            setCustErr('')
            setPhone(c.phoneNumber)
            setNewCust(false)
        } catch (e) {
            setNErr(e.message)
        } finally {
            setNSaving(false)
        }
    }

    const inputs = {
        customerPhone: { value: phone, set: setPhone },
        customerName: { value: nName, set: setNName },
        newCustPhone: { value: nPhone, set: setNPhone },
        productSearch: { value: search, set: setSearch },
        redeemPoints: { value: redeem, set: setRedeem },
        weightKg: { value: wKg, set: onKg },
        weightAmount: { value: wAmount, set: onAmount },
    }

    const addToCart = (p) => {
        if (!p.isAvailable) return
        if (p.unitType === 'Kg') return openWeigh(p)
        const found = cart.find(i => i.id === p.id)
        if (found) {
            setCart(cart.map(i => i.id === p.id ? { ...i, qty: i.qty + 1 } : i))
        } else {
            setCart([...cart, { id: p.id, name: p.name, price: p.price, unitType: p.unitType, qty: 1 }])
        }
    }

    const changeQty = (id, dir) => {
        setCart(cart
            .map(i => i.id === id ? { ...i, qty: i.qty + dir } : i)
            .filter(i => i.qty > 0))
    }
    const removeItem = (id) => setCart(cart.filter(i => i.id !== id))

    const reset = () => {
        setCart([]); setRedeem(''); setResult(null); setSendErr('')
        setPhone(''); setCustomer(null); setCustErr('')
        setNewCust(false); setNName(''); setNPhone(''); setNErr('')
    }

    const submit = async () => {
        setSendErr('')
        setSending(true)
        try {
            const items = cart.map(i => ({ productId: i.id, quantity: i.qty }))
            setResult(await createOrder(customer.id, items, points))
            setCart([]); setRedeem('')
        } catch (e) {
            setSendErr(e.message)
        } finally {
            setSending(false)
        }
    }

    const kbdBtn = (name) => (
        <button type="button" className="kbd-btn" tabIndex={-1}
            onClick={() => setOpenField(name)}>
            <KeyboardIcon size={20} />
        </button>
    )

    return (
        <div className="order">

            <section className="catalog">

                <div className="input-row">
                    <input type="text" placeholder="بحث عن منتج" value={search}
                        onChange={e => setSearch(e.target.value)} />
                    {kbdBtn('productSearch')}
                </div>

                <div className="tabs">
                    {TABS.map(c => (
                        <button key={c.key} type="button"
                            className={category === c.key ? 'tab on' : 'tab'}
                            onClick={() => setCategory(c.key)}>{c.label}</button>
                    ))}
                </div>

                {loading && <p className="hint">جاري تحميل المنتجات...</p>}
                {loadErr && <p className="login-error">{loadErr}</p>}
                {!loading && !loadErr && visible.length === 0 &&
                    <p className="hint">ما في منتجات مطابقة</p>}
                <div className="meta">
                    <h2 className="h2">
                        {TABS.find(c => c.key === category).label}
                    </h2>
                    <span className="count">{visible.length} صنف</span>
                </div>

                <div className="grid">
                    {visible.map(p => {
                        const Icon = CATEGORY_ICON[p.category] || Coffee
                        const inCart = cart.some(i => i.id === p.id)
                        return (
                            <button key={p.id} type="button"
                                className={`pcard${inCart ? ' picked' : ''}${p.isAvailable ? '' : ' off'}`}
                                disabled={!p.isAvailable}
                                onClick={() => addToCart(p)}>
                                <span className="thumb"><Icon size={30} strokeWidth={1.5} /></span>
                                <span className="pinfo">
                                    <span className="pname">{p.name}</span>
                                    <span className="punit">
                                        {p.unitType === 'Kg' ? 'يُباع بالكيلو' : 'يُباع بالقطعة'}
                                    </span>
                                    <span className="pprice">{p.price.toFixed(3)} د.أ</span>
                                </span>
                                <span className={inCart ? 'add' : 'add ghost'}>
                                    {p.isAvailable ? (inCart ? 'بالسلة ✓' : 'إضافة') : 'غير متوفر'}
                                </span>
                            </button>
                        )
                    })}
                </div>

            </section>

            <aside className="bill">

                <div className="bill-top">
                    <div className="input-row">
                        <input type="text" placeholder="رقم هاتف الزبون" value={phone}
                            onChange={e => setPhone(e.target.value)}
                            onKeyDown={e => e.key === 'Enter' && searchCustomer()} />
                        {kbdBtn('customerPhone')}
                        <button type="button" className="kbd-btn go" onClick={searchCustomer}
                            disabled={searching}>
                            <Search size={20} />
                        </button>
                    </div>

                    {customer && (
                        <div className="cust">
                            <strong>{customer.fullName}</strong>
                            <span>الرصيد: {customer.pointsBalance} نقطة</span>
                        </div>
                    )}

                    {custErr && (
                        <div className="cust-err">
                            <span>{custErr}</span>
                            <button type="button" className="mini" onClick={openNewCust}>
                                <UserPlus size={16} /> زبون جديد
                            </button>
                        </div>
                    )}
                </div>

                <div className="bill-items">
                    {cart.length === 0 && <p className="hint">السلة فارغة</p>}
                    {cart.map(i => (
                        <div key={i.id} className="line">
                            <div className="line-info">
                                <strong>{i.name}</strong>
                                <small>{(i.price * i.qty).toFixed(3)} د</small>
                            </div>

                            {i.unitType === 'Kg' ? (
                                <div className="line-qty">
                                    <button type="button" className="wbtn"
                                        onClick={() => openWeigh(products.find(p => p.id === i.id), i.qty)}>
                                        {i.qty} كغم
                                    </button>
                                    <button type="button" className="del" onClick={() => removeItem(i.id)}><Trash2 size={16} /></button>
                                </div>
                            ) : (
                                <div className="line-qty">
                                    <button type="button" onClick={() => changeQty(i.id, -1)}><Minus size={16} /></button>
                                    <span>{i.qty}</span>
                                    <button type="button" onClick={() => changeQty(i.id, +1)}><Plus size={16} /></button>
                                    <button type="button" className="del" onClick={() => removeItem(i.id)}><Trash2 size={16} /></button>
                                </div>
                            )}
                        </div>
                    ))}
                </div>

                <div className="bill-foot">

                    {customer && customer.pointsBalance >= 200 && (
                        <div className="input-row">
                            <input type="text" placeholder="نقاط للاستبدال (٢٠٠ فأكثر)" value={redeem}
                                onChange={e => setRedeem(e.target.value)} />
                            {kbdBtn('redeemPoints')}
                        </div>
                    )}

                    <div className="row"><span>المجموع</span><strong>{total.toFixed(3)} د</strong></div>
                    {points > 0 && (
                        <div className="row minus"><span>خصم النقاط</span><strong>− {discount.toFixed(3)} د</strong></div>
                    )}
                    <div className="row big"><span>المطلوب</span><strong>{cashPaid.toFixed(3)} د</strong></div>

                    {sendErr && <p className="login-error">{sendErr}</p>}

                    <button type="button" className="login-button"
                        disabled={!customer || cart.length === 0 || sending}
                        onClick={submit}>
                        {sending ? 'جاري الحفظ...' : 'تأكيد الطلب'}
                    </button>

                </div>
            </aside>
            {newCust && (
                <div className="modal-bg">
                    <div className="modal">
                        <h3>تسجيل زبون جديد</h3>

                        <span className="wlabel">الاسم الكامل</span>
                        <div className="input-row">
                            <input type="text" value={nName}
                                onChange={e => setNName(e.target.value)} />
                            {kbdBtn('customerName')}
                        </div>

                        <span className="wlabel">رقم الهاتف</span>
                        <div className="input-row">
                            <input type="text" value={nPhone}
                                onChange={e => setNPhone(e.target.value)} />
                            {kbdBtn('newCustPhone')}
                        </div>

                        <p className="hint">مثال: 0791234567</p>

                        {nErr && <p className="login-error">{nErr}</p>}

                        <div className="wact">
                            <button type="button" className="login-button"
                                disabled={nSaving} onClick={saveNewCust}>
                                {nSaving ? 'جاري الحفظ...' : 'تسجيل'}
                            </button>
                            <button type="button" className="wcancel"
                                onClick={() => setNewCust(false)}>إلغاء</button>
                        </div>
                    </div>
                </div>
            )}

            {weighing && (
                <div className="modal-bg">
                    <div className="modal">
                        <h3>{weighing.name}</h3>
                        <p className="hint">{weighing.price.toFixed(3)} د.أ للكيلو الواحد</p>

                        <span className="wlabel">الوزن المطلوب (كغم)</span>
                        <div className="input-row">
                            <input type="text" value={wKg} onChange={e => onKg(e.target.value)} />
                            {kbdBtn('weightKg')}
                        </div>
                        <div className="quick">
                            {[0.25, 0.5, 0.75, 1].map(v => (
                                <button key={v} type="button" onClick={() => onKg(String(v))}>{v} كغم</button>
                            ))}
                        </div>

                        <span className="wlabel">أو حدّد المبلغ (د.أ)</span>
                        <div className="input-row">
                            <input type="text" value={wAmount} onChange={e => onAmount(e.target.value)} />
                            {kbdBtn('weightAmount')}
                        </div>
                        <div className="quick">
                            {[1, 2, 5, 10].map(v => (
                                <button key={v} type="button" onClick={() => onAmount(String(v))}>{v} د</button>
                            ))}
                        </div>

                        <div className="row big">
                            <span>{Number(wKg) > 0 ? `${Number(wKg)} كغم` : '—'}</span>
                            <strong>{(Number(wKg) * weighing.price).toFixed(3)} د.أ</strong>
                        </div>

                        <div className="wact">
                            <button type="button" className="login-button"
                                disabled={!(Number(wKg) > 0)} onClick={confirmWeigh}>
                                {wEdit ? 'تحديث' : 'إضافة للسلة'}
                            </button>
                            <button type="button" className="wcancel" onClick={() => setWeighing(null)}>إلغاء</button>
                        </div>
                    </div>
                </div>
            )}
            {result && (
                <div className="modal-bg">
                    <div className="modal">
                        <h3>تم تسجيل الطلب #{result.orderId}</h3>
                        <div className="row"><span>المجموع</span><strong>{result.total.toFixed(3)} د</strong></div>
                        <div className="row"><span>المدفوع نقدًا</span><strong>{result.cashPaid.toFixed(3)} د</strong></div>
                        <div className="row"><span>نقاط مستبدلة</span><strong>{result.pointsRedeemed}</strong></div>
                        <div className="row"><span>نقاط مكتسبة</span><strong>{result.pointsEarned}</strong></div>
                        <div className="row big"><span>رصيد الزبون</span><strong>{result.newBalance}</strong></div>
                        <button type="button" className="login-button" onClick={reset}>طلب جديد</button>
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