import { browser } from '$app/env';

export type Theme = 'light' | 'dark';

function initialTheme(): Theme {
	if (!browser) return 'light';
	const stored = localStorage.getItem('pps-theme');
	if (stored === 'light' || stored === 'dark') return stored;
	return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
}

let current = $state<Theme>(initialTheme());

export const theme = {
	get value(): Theme {
		return current;
	},
	set(value: Theme) {
		current = value;
		if (browser) {
			localStorage.setItem('pps-theme', value);
			document.documentElement.dataset.theme = value;
		}
	},
	toggle() {
		this.set(current === 'dark' ? 'light' : 'dark');
	}
};

if (browser) {
	// Sync in case documentElement theme was set by the inline script in app.html
	document.documentElement.dataset.theme = current;
}
