import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],

  // The dashboard lives under /dashboard, not at the site root — the root is reserved for the
  // public site that comes later. This prefixes every asset URL Vite writes into index.html;
  // without it the built page asks for /assets/index-*.js, which under /dashboard resolves
  // against the root and 404s. The trailing slash is required.
  base: '/dashboard/',

  build: {
    // Straight into the API's wwwroot rather than the default ./dist.
    //
    // In production the dashboard and the API are one site on MonsterASP: `dotnet publish`
    // picks wwwroot up on its own, so `npm run build` followed by a publish produces a folder
    // that is already the whole deployment. A ./dist plus a copy step is one step someone
    // forgets, and a stale dashboard against a fresh API is a confusing thing to debug.
    //
    // The path mirrors the URL — wwwroot/dashboard for /dashboard — so the static file
    // middleware maps one onto the other with no rewriting.
    //
    // emptyOutDir is explicit because the directory sits outside the Vite project root, where
    // Vite refuses to clear it silently — without this the folder accumulates the hashed
    // assets of every past build.
    outDir: '../backend/CoffeeLoyalty.Api/wwwroot/dashboard',
    emptyOutDir: true,
  },

  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      // Development only. In production the API serves these pages itself, so `/api` is
      // already same-origin and no proxy is involved.
      '/api': 'http://localhost:5286',
    },
  },
})
