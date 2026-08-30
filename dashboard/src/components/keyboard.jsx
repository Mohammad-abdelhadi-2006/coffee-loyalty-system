import { ARABIC, ENGLISH, NUMBERS } from './keyboardLayouts.js'
import { useState } from 'react'

/*
 * Virtual touch keyboard component designed for POS terminals.
 * Accepts target input value, change handler, mode configurations, and close callback.
 */
export default function Keyboard({ value, onChange, config, onClose }) {
  const [mode, setMode] = useState(config.mode)
  const [shift, setShift] = useState(false)

  /* Selects the appropriate character set array based on the active keyboard mode */
  let rows = NUMBERS
  if (mode === 'ar') rows = ARABIC
  if (mode === 'en') rows = ENGLISH

  /* Appends clicked character to input value and resets shift toggle */
  const press = (ch) => {
    onChange(value + ch)
    if (shift) setShift(false)
  }

  /* Deletes the last entered character from the string */
  const back  = () => onChange(value.slice(0, -1))

  /* Resets and clears the input value completely */
  const clear = () => onChange('')

  /* Prevents the input field from losing browser focus when pressing buttons */
  const keep = (e) => e.preventDefault()

  return (
    <div className="kbd" onMouseDown={keep}>

      <div className="kbd-head">
        <span className="kbd-title">{config.title}</span>

        <div className="kbd-modes">
          <button type="button" disabled={!config.allow.includes('num')}
                  className={mode === 'num' ? 'kbd-mode on' : 'kbd-mode'}
                  onClick={() => setMode('num')}>123</button>

          <button type="button" disabled={!config.allow.includes('ar')}
                  className={mode === 'ar' ? 'kbd-mode on' : 'kbd-mode'}
                  onClick={() => setMode('ar')}>أ ب ت</button>

          <button type="button" disabled={!config.allow.includes('en')}
                  className={mode === 'en' ? 'kbd-mode on' : 'kbd-mode'}
                  onClick={() => setMode('en')}>ABC</button>
        </div>

        <button type="button" className="kbd-close" onClick={onClose}>✕</button>
      </div>

      {rows.map((row, i) => (
        <div key={i} className="kbd-row">
          {row.map(ch => {
            const label = shift ? ch.toUpperCase() : ch
            return (
              <button key={ch} type="button" className="kbd-key"
                      onClick={() => press(label)}>{label}</button>
            )
          })}
        </div>
      ))}

      <div className="kbd-row">
        {mode === 'en' && (
          <button type="button"
                  className={shift ? 'kbd-key act on' : 'kbd-key act'}
                  onClick={() => setShift(!shift)}>⇧</button>
        )}

        <button type="button" className="kbd-key act" onClick={back}>حذف</button>

        {mode !== 'num'
          ? <button type="button" className="kbd-key space" onClick={() => press(' ')}>مسافة</button>
          : <button type="button" className="kbd-key act" onClick={clear}>مسح الكل</button>}

        <button type="button" className="kbd-key go" onClick={onClose}>تم</button>
      </div>

    </div>
  )
}