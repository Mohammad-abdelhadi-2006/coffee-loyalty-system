import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],

  build: {
    // Straight into the API's wwwroot rather than the default ./dist, the same
    // way the dashboard builds into wwwroot/dashboard.
    //
    // The site owns the root: `/` is the home page, `/menu` and the rest are its
    // routes, and `/assets`, `/downloads` and the favicons sit beside them. The
    // dashboard keeps its own `/dashboard` subtree and the API keeps `/api`, so
    // the three never collide.
    //
    // `dotnet publish` picks wwwroot up on its own, so building here and then
    // publishing produces a folder that is already the whole deployment.
    outDir: '../backend/CoffeeLoyalty.Api/wwwroot',

    // NOT emptied. wwwroot also holds the dashboard's build, and clearing it
    // would delete that — the dashboard would 404 until someone rebuilt it and
    // nobody would know why. Stale asset files are cleaned by deleting wwwroot
    // by hand before a full redeploy; see docs/DEPLOYMENT.md.
    emptyOutDir: false,
  },

  server: {
    // اربط على كل المحوّلات — ما عاد في داعي لـ --host
    host: true,
    port: 5173,
    // اسمح لروابط أنفاق VS Code (Dev Tunnels) بالوصول
    allowedHosts: ['.devtunnels.ms'],
  },
})
