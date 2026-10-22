<script lang="ts">
	import { theme } from '#lib/theme/theme.svelte.ts';
	import { session } from '#lib/auth/session.svelte.ts';
	import { page } from '$app/state';

	interface Props {
		class?: string;
	}

	let { class: className = '' }: Props = $props();

	const isActive = (href: string, exact = false) =>
		exact ? page.url.pathname === href : page.url.pathname.startsWith(href);
</script>

<header class="header {className}">
	<div class="container header__inner">
		<a class="header__logo" href="/" aria-label="про телефон — на главную">
			<span class="header__logo-mark" aria-hidden="true">
				<svg width="14" height="14" viewBox="0 0 24 24" fill="none">
					<rect x="7" y="3" width="10" height="18" rx="2.5" stroke="currentColor" stroke-width="2" />
					<path d="M11 18h2" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
				</svg>
			</span>
			<span class="header__logo-text">
				<span class="header__logo-name">про телефон</span>
				<span class="header__logo-caption" aria-hidden="true">Мастерская рядом</span>
			</span>
		</a>

		<nav class="header__nav" aria-label="Основная навигация">
			<a class="header__link" class:header__link--active={isActive('/', true)} aria-current={isActive('/', true) ? 'page' : undefined} href="/">Главная</a>
			<a class="header__link" class:header__link--active={isActive('/price')} aria-current={isActive('/price') ? 'page' : undefined} href="/price">Прайс-лист</a>
			<a
				class="header__link"
				class:header__link--active={isActive('/account') || isActive('/login')}
				aria-current={isActive('/account') ? 'page' : undefined}
				href={session.user ? '/account' : '/login'}
			>
				Личный кабинет
			</a>
		</nav>

		<div class="header__actions">
			<div class="header__theme" role="group" aria-label="Тема оформления">
				<button
					class="theme-btn"
					class:theme-btn--active={theme.value === 'light'}
					type="button"
					aria-pressed={theme.value === 'light'}
					onclick={() => theme.set('light')}
				>
					<svg width="14" height="14" viewBox="0 0 24 24" fill="none" aria-hidden="true">
						<circle cx="12" cy="12" r="4.5" stroke="currentColor" stroke-width="2" />
						<path
							d="M12 2.5v2M12 19.5v2M2.5 12h2M19.5 12h2M5.3 5.3l1.4 1.4M17.3 17.3l1.4 1.4M18.7 5.3l-1.4 1.4M6.7 17.3l-1.4 1.4"
							stroke="currentColor"
							stroke-width="2"
							stroke-linecap="round"
						/>
					</svg>
					Светлая
				</button>
				<button
					class="theme-btn"
					class:theme-btn--active={theme.value === 'dark'}
					type="button"
					aria-pressed={theme.value === 'dark'}
					onclick={() => theme.set('dark')}
				>
					<svg width="14" height="14" viewBox="0 0 24 24" fill="none" aria-hidden="true">
						<path d="M20 13.5A8 8 0 1 1 10.5 4a6.6 6.6 0 0 0 9.5 9.5Z" stroke="currentColor" stroke-width="2" stroke-linejoin="round" />
					</svg>
					Тёмная
				</button>
			</div>
			<a class="header__cta" href="/request">Оставить заявку</a>
		</div>
	</div>
</header>

<style>
	.header {
		position: sticky;
		top: 0;
		z-index: 100;
		background: var(--surface);
		border-bottom: 1px solid var(--border);
	}

	.header__inner {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 24px;
		height: 72px;
	}

	.header__logo {
		display: inline-flex;
		align-items: center;
		gap: 10px;
		text-decoration: none;
	}

	.header__logo-mark {
		width: 34px;
		height: 34px;
		border-radius: 10px;
		background: var(--accent);
		color: #ffffff;
		display: inline-flex;
		align-items: center;
		justify-content: center;
	}

	.header__logo-text {
		display: flex;
		flex-direction: column;
		line-height: 1.15;
	}

	.header__logo-name {
		font-size: 17px;
		font-weight: 700;
		letter-spacing: -0.02em;
		color: var(--text);
	}

	.header__logo-caption {
		font-size: 9px;
		font-weight: 500;
		letter-spacing: 0.1em;
		text-transform: uppercase;
		color: var(--muted);
	}

	.header__nav {
		display: flex;
		gap: 8px;
	}

	.header__link {
		font-size: 14px;
		font-weight: 500;
		color: var(--muted);
		padding: 8px 14px;
		border-radius: 999px;
	}
	.header__link:hover {
		color: var(--text);
	}

	.header__link--active {
		background: var(--accent-soft);
		color: var(--accent);
	}

	.header__link--active:hover {
		color: var(--accent);
	}

	.header__actions {
		display: flex;
		align-items: center;
		gap: 16px;
	}

	.header__theme {
		display: inline-flex;
		background: var(--bg);
		border: 1px solid var(--border);
		border-radius: 999px;
		padding: 3px;
		gap: 2px;
	}

	.theme-btn {
		display: inline-flex;
		align-items: center;
		gap: 6px;
		font-size: 12px;
		font-weight: 500;
		color: var(--muted);
		border-radius: 999px;
		padding: 6px 12px;
		transition:
			color 0.15s ease,
			background-color 0.15s ease,
			box-shadow 0.15s ease;
	}

	.theme-btn:hover {
		color: var(--text);
	}

	.theme-btn--active {
		background: var(--surface);
		color: var(--text);
		box-shadow: var(--shadow-card);
	}

	.header__cta {
		font-size: 14px;
		font-weight: 500;
		background: var(--accent);
		color: #ffffff;
		border-radius: 12px;
		padding: 10px 18px;
		transition: background-color 0.15s ease;
	}

	.header__cta:hover {
		background: var(--accent-hover);
	}

	@media (max-width: 768px) {
		.header__nav {
			display: none;
		}

		.header__inner {
			gap: 12px;
		}

		.header__theme {
			display: none;
		}
	}
</style>
