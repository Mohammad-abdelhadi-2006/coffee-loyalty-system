import { useState } from 'react'
import { Search, Keyboard as KeyboardIcon } from 'lucide-react'
import Keyboard from '../components/keyboard.jsx'
import { FIELDS } from '../constants/fields.js'
import { getOrder } from '../api/orders.js'

const STATUS = {
  Completed: 'مكتمل',
  Returned:  'فيه مرتجعات',
  Cancelled: 'ملغي',
}

const fmtDate = (iso) => new Date(iso).toLocaleString('ar-JO', {
  year: 'numeric', month: '2-digit', day: '2-digit',
  hour: '2-digit', minute: '2-digit',
})

export default function OrderLookup() {
  const [num, setNum]         = useState('')
  const [order, setOrder]     = useState(null)
  const [err, setErr]         = useState('')
  const [loading, setLoading] = useState(false)

  const [openField, setOpenField] = useState(null)

  const inputs = {
    orderNumber: { value: num, set: setNum },
  }

  const find = async () => {
    const id = Number(num.trim())
    if (!(id > 0)) return setErr('اكتب رقم طلب صحيح')

    setErr(''); setOrder(null); setLoading(true)
    try {
      setOrder(await getOrder(id))
    } catch (e) {
      setErr(e.message)
    } finally {
      setLoading(false)
    }
  }

  const cashPaid = order ? order.total - order.pointsRedeemed / 100 : 0

  return (
    <div className="page">

      <div className="page-head">
        <h1 className="page-title">بحث عن طلب</h1>
      </div>

      <div className="input-row lookup">
        <input type="text" placeholder="رقم الطلب" value={num}
               onChange={e => setNum(e.target.value)}
               onKeyDown={e => e.key === 'Enter' && find()} />
        <button type="button" className="kbd-btn" tabIndex={-1}
                onClick={() => setOpenField('orderNumber')}>
          <KeyboardIcon size={20} />
        </button>
        <button type="button" className="kbd-btn" onClick={find} disabled={loading}>
          <Search size={20} />
        </button>
      </div>

      {err && <p className="login-error">{err}</p>}
      {loading && <p className="hint">جاري البحث...</p>}
      {!order && !err && !loading &&
        <p className="hint">اكتب رقم الطلب المطبوع على الفاتورة</p>}

      {order && (
        <div className="tablewrap invoice">

          <div className="inv-head">
            <div>
              <h2 className="h2">طلب #{order.orderId}</h2>
              <span className="ometa">{fmtDate(order.createdAt)}</span>
            </div>
            <span className={order.status === 'Completed' ? 'pill on' : 'pill'}>
              {STATUS[order.status] ?? order.status}
            </span>
          </div>

          <div className="inv-who">
            <div className="inv-cell">
              <small>الزبون</small>
              <strong>{order.customerName}</strong>
            </div>
            <div className="inv-cell">
              <small>الكاشير</small>
              <strong>{order.employeeName}</strong>
            </div>
          </div>

          <table className="tbl">
            <thead>
              <tr>
                <th>الصنف</th>
                <th>الكمية</th>
                <th>سعر الوحدة</th>
                <th>المرتجع</th>
                <th>الإجمالي</th>
              </tr>
            </thead>
            <tbody>
              {order.items.map(it => (
                <tr key={it.orderItemId}>
                  <td>{it.productName}</td>
                  <td>{it.quantity}</td>
                  <td>{it.unitPriceSnapshot.toFixed(3)} د</td>
                  <td>{it.returnedQuantity > 0 ? it.returnedQuantity : '—'}</td>
                  <td className="num">{(it.quantity * it.unitPriceSnapshot).toFixed(3)} د</td>
                </tr>
              ))}
            </tbody>
          </table>

          <div className="inv-foot">
            <div className="row">
              <span>المجموع</span><strong>{order.total.toFixed(3)} د</strong>
            </div>
            {order.pointsRedeemed > 0 && (
              <div className="row minus">
                <span>خصم النقاط ({order.pointsRedeemed} نقطة)</span>
                <strong>− {(order.pointsRedeemed / 100).toFixed(3)} د</strong>
              </div>
            )}
            <div className="row">
              <span>نقاط مكتسبة</span><strong>{order.pointsEarned}</strong>
            </div>
            <div className="row big">
              <span>المدفوع نقدًا</span><strong>{cashPaid.toFixed(3)} د</strong>
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