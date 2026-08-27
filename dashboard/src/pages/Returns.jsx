import { useState } from 'react'
import { Search, Keyboard as KeyboardIcon } from 'lucide-react'
import Keyboard from '../components/keyboard.jsx'
import { FIELDS } from '../constants/fields.js'
import { findByPhone, getCustomerOrders } from '../api/customers.js'
import { cancelOrder, returnItems } from '../api/orders.js'

const STATUS = {
  Completed: 'مكتمل',
  Returned:  'فيه مرتجعات',
  Cancelled: 'ملغي',
}

const fmtDate = (iso) => new Date(iso).toLocaleString('ar-JO', {
  year: 'numeric', month: '2-digit', day: '2-digit',
  hour: '2-digit', minute: '2-digit',
})

export default function Returns() {
  const [phone, setPhone]         = useState('')
  const [customer, setCustomer]   = useState(null)
  const [custErr, setCustErr]     = useState('')
  const [searching, setSearching] = useState(false)

  const [orders, setOrders]   = useState([])
  const [selected, setSelected] = useState(null)

  const [qty, setQty]         = useState({})
  const [busy, setBusy]       = useState(false)
  const [actErr, setActErr]   = useState('')
  const [result, setResult]   = useState(null)

  const [openField, setOpenField] = useState(null)
  const [qtyItem, setQtyItem]     = useState(null)

  const inputs = {
    customerPhone: { value: phone, set: setPhone },
    qty: { value: qty[qtyItem] ?? '', set: v => setQty({ ...qty, [qtyItem]: v }) },
  }

  const search = async () => {
    if (!phone.trim()) return
    setCustErr(''); setSearching(true)
    setOrders([]); setSelected(null); setQty({}); setActErr('')
    try {
      const c = await findByPhone(phone.trim())
      setCustomer(c)
      setOrders(await getCustomerOrders(c.id))
    } catch (e) {
      setCustomer(null)
      setCustErr(e.message)
    } finally {
      setSearching(false)
    }
  }

  const refresh = async () => {
    const list = await getCustomerOrders(customer.id)
    setOrders(list)
    setSelected(list.find(o => o.orderId === selected?.orderId) ?? null)
    setQty({})
  }

  const pick = (o) => {
    setSelected(o); setQty({}); setActErr('')
  }

  const remaining = (it) => it.quantity - it.returnedQuantity

  const canCancel = selected
    && selected.status === 'Completed'
    && selected.items.every(it => it.returnedQuantity === 0)

  const canReturn = selected
    && selected.status !== 'Cancelled'
    && selected.pointsRedeemed === 0
    && selected.items.some(it => remaining(it) > 0)

  const doCancel = async () => {
    setActErr(''); setBusy(true)
    try {
      setResult({ kind: 'cancel', ...await cancelOrder(selected.orderId) })
      await refresh()
    } catch (e) {
      setActErr(e.message)
    } finally {
      setBusy(false)
    }
  }

  const doReturn = async () => {
    const items = Object.entries(qty)
      .map(([id, v]) => ({ orderItemId: Number(id), quantity: Number(v) }))
      .filter(x => x.quantity > 0)

    if (items.length === 0) return setActErr('حدّد كمية للإرجاع')

    for (const x of items) {
      const it = selected.items.find(i => i.orderItemId === x.orderItemId)
      if (x.quantity > remaining(it)) {
        return setActErr(`«${it.productName}»: أقصى كمية للإرجاع ${remaining(it)}`)
      }
    }

    setActErr(''); setBusy(true)
    try {
      setResult({ kind: 'return', ...await returnItems(selected.orderId, items) })
      await refresh()
    } catch (e) {
      setActErr(e.message)
    } finally {
      setBusy(false)
    }
  }

  const kbdBtn = (name, item = null) => (
    <button type="button" className="kbd-btn" tabIndex={-1}
            onClick={() => { setQtyItem(item); setOpenField(name) }}>
      <KeyboardIcon size={20} />
    </button>
  )

  return (
    <div className="order">

      <section className="catalog">
        <h1 className="page-title">المرتجعات</h1>

        <div className="input-row">
          <input type="text" placeholder="رقم هاتف الزبون" value={phone}
                 onChange={e => setPhone(e.target.value)}
                 onKeyDown={e => e.key === 'Enter' && search()} />
          {kbdBtn('customerPhone')}
          <button type="button" className="kbd-btn" onClick={search} disabled={searching}>
            <Search size={20} />
          </button>
        </div>

        {custErr && <p className="login-error">{custErr}</p>}

        {customer && (
          <div className="cust">
            <strong>{customer.fullName}</strong>
            <span>الرصيد: {customer.pointsBalance} نقطة</span>
          </div>
        )}

        {customer && orders.length === 0 && <p className="hint">ما في طلبات لهاد الزبون</p>}

        <div className="grid onegrid">
          {orders.map(o => (
            <button key={o.orderId} type="button"
                    className={selected?.orderId === o.orderId ? 'ocard on' : 'ocard'}
                    onClick={() => pick(o)}>
              <span className="ohead">
                <strong>طلب #{o.orderId}</strong>
                <span className={o.status === 'Completed' ? 'pill on' : 'pill'}>
                  {STATUS[o.status] ?? o.status}
                </span>
              </span>
              <span className="ometa">{fmtDate(o.createdAt)}</span>
              <span className="ometa">
                {o.items.length} أصناف · نقاط مكتسبة {o.pointsEarned}
                {o.pointsRedeemed > 0 && ` · مستبدلة ${o.pointsRedeemed}`}
              </span>
              <span className="pprice">{o.total.toFixed(3)} د.أ</span>
            </button>
          ))}
        </div>
      </section>

      <aside className="bill">
        {!selected && <p className="hint">اختر طلبًا من اليمين</p>}

        {selected && (
          <>
            <div className="bill-top">
              <div className="row big"><span>طلب #{selected.orderId}</span></div>
              <div className="row"><span>الحالة</span><strong>{STATUS[selected.status]}</strong></div>
              <div className="row"><span>المجموع</span><strong>{selected.total.toFixed(3)} د</strong></div>
            </div>

            <div className="bill-items">
              {selected.items.map(it => (
                <div key={it.orderItemId} className="line coll">
                  <div className="line-info">
                    <strong>{it.productName}</strong>
                    <small>
                      {it.quantity} × {it.unitPriceSnapshot.toFixed(3)} د
                      {it.returnedQuantity > 0 && ` · مرتجع ${it.returnedQuantity}`}
                    </small>
                  </div>
                  {canReturn && remaining(it) > 0 && (
                    <div className="input-row">
                      <input type="text" placeholder={`للإرجاع (بحد أقصى ${remaining(it)})`}
                             value={qty[it.orderItemId] ?? ''}
                             onChange={e => setQty({ ...qty, [it.orderItemId]: e.target.value })} />
                      {kbdBtn('qty', it.orderItemId)}
                    </div>
                  )}
                </div>
              ))}
            </div>

            <div className="bill-foot">
              {actErr && <p className="login-error">{actErr}</p>}

              {selected.pointsRedeemed > 0 && selected.status !== 'Cancelled' && (
                <p className="hint">
                  هاد الطلب مدفوع بنقاط — الإرجاع الجزئي ممنوع، الإلغاء الكامل فقط.
                </p>
              )}

              <button type="button" className="login-button"
                      disabled={!canReturn || busy} onClick={doReturn}>
                إرجاع المحدد
              </button>

              <button type="button" className="danger-button"
                      disabled={!canCancel || busy} onClick={doCancel}>
                إلغاء الطلب كامل
              </button>
            </div>
          </>
        )}
      </aside>

      {result && (
        <div className="modal-bg">
          <div className="modal">
            <h3>{result.kind === 'cancel' ? 'تم إلغاء الطلب' : 'تم تسجيل الإرجاع'}</h3>
            {result.kind === 'return' && (
              <div className="row"><span>المبلغ المرتجع</span><strong>{result.refundAmount.toFixed(3)} د</strong></div>
            )}
            <div className="row"><span>نقاط مسحوبة</span><strong>{result.pointsClawedBack}</strong></div>
            {result.kind === 'cancel' && (
              <div className="row"><span>نقاط مُعادة</span><strong>{result.pointsRestored}</strong></div>
            )}
            <div className="row big"><span>رصيد الزبون</span><strong>{result.newBalance}</strong></div>
            <button type="button" className="login-button" onClick={() => setResult(null)}>تمام</button>
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