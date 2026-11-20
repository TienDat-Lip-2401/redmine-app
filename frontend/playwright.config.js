import process from 'node:process'
import { defineConfig, devices } from '@playwright/test'

// Chạy với stack Docker (frontend :3000, API :5164). Đổi bằng biến môi trường nếu dùng dev server.
const BASE_URL = process.env.E2E_BASE_URL || 'http://localhost:3000'
export const API_URL = process.env.E2E_API_URL || 'http://localhost:5164/api'

export default defineConfig({
  testDir: './e2e',
  timeout: 30 * 1000,
  expect: { timeout: 5000 },
  forbidOnly: !!process.env.CI,
  retries: 0,
  workers: 1,
  reporter: [['list'], ['html', { open: 'never' }]],
  use: {
    baseURL: BASE_URL,
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    headless: true,
  },
  // Không dùng webkit: backend trong Docker chạy http nên Safari bỏ qua cookie Secure.
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
})
