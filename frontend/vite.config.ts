import { fileURLToPath, URL } from 'node:url'

import { defineConfig, type Plugin } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

/**
 * 正式版的 Content-Security-Policy（XSS 防護的一環）
 * script-src 'self'：只執行本站的 js 檔，就算有惡意字串被插進畫面，inline script 與外部 script 也不會執行
 * style-src 允許 'unsafe-inline'：Element Plus 元件會動態寫入 inline style
 * connect-src 'self'：API 只能呼叫同網域的 /api
 */
const contentSecurityPolicy = [
  "default-src 'self'",
  "script-src 'self'",
  "style-src 'self' 'unsafe-inline'",
  "img-src 'self' data:",
  "font-src 'self' data:",
  "connect-src 'self'",
  "object-src 'none'",
  "base-uri 'self'",
  "form-action 'self'",
].join('; ')

/**
 * 在 build 出來的 index.html 加上 CSP meta 標籤
 * 只套用在 npm run build：開發模式的熱更新（websocket）與 Vue DevTools 會被 CSP 擋下
 * @returns Vite 外掛
 */
function cspMetaPlugin(): Plugin {
  return {
    name: 'csp-meta',
    apply: 'build',
    transformIndexHtml() {
      return [
        {
          tag: 'meta',
          attrs: { 'http-equiv': 'Content-Security-Policy', content: contentSecurityPolicy },
          injectTo: 'head',
        },
      ]
    },
  }
}

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vueDevTools(),
    cspMetaPlugin(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5123',
        changeOrigin: true,
      },
    },
  },
})
