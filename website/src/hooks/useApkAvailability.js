import { useEffect, useState } from 'react'
import { apkAvailability } from '../constants/download.js'

/**
 * Whether the Android build is downloadable, as one of three states.
 *
 * `'checking' | 'ready' | 'pending'`.
 *
 * Every surface that offers the app calls this — the download page's button and
 * the home page's store badge — so the two cannot contradict each other. The
 * underlying probe runs once per page load and its result is shared; this hook
 * only subscribes to it.
 */
export function useApkAvailability() {
  const [status, setStatus] = useState('checking')

  useEffect(() => {
    let cancelled = false

    apkAvailability().then((available) => {
      if (cancelled) return

      setStatus(available ? 'ready' : 'pending')
    })

    return () => {
      cancelled = true
    }
  }, [])

  return status
}
