import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // اربط على كل المحوّلات — ما عاد في داعي لـ --host
    host: true,
    port: 5173,
    // اسمح لروابط أنفاق VS Code (Dev Tunnels) بالوصول
    allowedHosts: ['.devtunnels.ms'],
  },
})
