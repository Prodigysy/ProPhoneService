import adapter from '@sveltejs/adapter-static';
import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig, loadEnv } from 'vite';

export default defineConfig(({ mode }) => {
	const env = loadEnv(mode, '.', 'VITE_');

	return {
		plugins: [
			sveltekit({
				adapter: adapter({
					fallback: 'index.html'
				})
			})
		],
		server: {
			proxy: {
				'/api': {
					target: env.VITE_API_PROXY_TARGET || 'http://localhost:8080',
					changeOrigin: true
				}
			}
		}
	};
});
