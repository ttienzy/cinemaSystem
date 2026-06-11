import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  envDir: '..',
  plugins: [react()],
  server: {
    port: 19876,
    strictPort: true,
  },
  preview: {
    port: 19876,
    strictPort: true,
  },
});
