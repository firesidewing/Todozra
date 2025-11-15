import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import tailwindcss from "@tailwindcss/vite";

export default defineConfig({
  plugins: [vue(), tailwindcss()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5202',
        changeOrigin: true,
        secure: false
      }
    }
  },
  build: {
    outDir: process.env.DOCKER_BUILD ? 'dist' : '../Todozra.Api/wwwroot'
  }
});