import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import path from 'path';

export default defineConfig({
  assetsInclude: ['**/*.ttf'],
  plugins: [vue()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'), // @ points to src/
    },
  },
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:8080',
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/api/, ''), // Strip /api prefix
      },
      '/swagger': {
        target: 'http://localhost:8080',
        changeOrigin: true,
      },
    },
    hmr: {
      overlay: true,
    },
  },
});
