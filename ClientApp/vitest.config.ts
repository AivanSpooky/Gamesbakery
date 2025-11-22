import { defineConfig } from 'vitest/config';
import vue from '@vitejs/plugin-vue';
import { fileURLToPath } from 'url';
import path from 'path';

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  test: {
    environment: 'jsdom',
    coverage: {
      provider: 'v8',
      reporter: ['text', 'html'],
      thresholds: { lines: 60 },
      include: ['src/services/**/*.ts'],
      exclude: [
        'node_modules/**',
        'dist/**',
        'tests/**',
        'src/services/ApiService.ts'
      ],
    },
  },
});
