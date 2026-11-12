/// <reference types="vitest" />
import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import path from 'path'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  server: {
    // يجعل السيرفر قابلاً للوصول من الهاتف عبر الشبكة المحلية
    host: '0.0.0.0',
    port: 5173,
    // HTTPS مطلوب لتفعيل GPS على الهاتف (Geolocation API)
    // شغّل: npx vite --https
    // أو استخدم: npm run dev:mobile  (انظر scripts في package.json)
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/test/setup.ts',
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      exclude: [
        'node_modules/',
        'src/test/',
        'src/**/*.d.ts',
        'src/main.tsx',
      ],
    },
  },
})
