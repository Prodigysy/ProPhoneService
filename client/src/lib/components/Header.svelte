<script lang="ts">
	import { theme } from '#lib/theme/theme.svelte.ts';

	interface Props {
		class?: string;
	}

	let { class: className = '' }: Props = $props();
</script>

<header class="header {className}">
	<div class="container header__inner">
		<a class="header__logo" href="/" aria-label="про телефон — на главную">
			<span class="header__logo-mark">про</span>телефон
		</a>

		<nav class="header__nav" aria-label="Основная навигация">
			<a class="header__link" href="/">Главная</a>
			<a class="header__link" href="/price">Прайс-лист</a>
			<button
				class="header__link header__link--disabled"
				type="button"
				aria-disabled="true"
				title="Личный кабинет появится после авторизации"
			>
				Личный кабинет
			</button>
		</nav>

		<div class="header__actions">
			<button
				class="theme-toggle"
				type="button"
				aria-label={theme.value === 'dark' ? 'Включить светлую тему' : 'Включить тёмную тему'}
				onclick={() => theme.toggle()}
			>
				{#if theme.value === 'dark'}
					<svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden="true">
						<circle cx="12" cy="12" r="4.5" stroke="currentColor" stroke-width="2" />
						<path
							d="M12 2.5v2M12 19.5v2M2.5 12h2M19.5 12h2M5.3 5.3l1.4 1.4M17.3 17.3l1.4 1.4M18.7 5.3l-1.4 1.4M6.7 17.3l-1.4 1.4"
							stroke="currentColor"
							stroke-width="2"
							stroke-linecap="round"
						/>
					</svg>
					<span>Светлая</span>
				{:else}
					<svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden="true">
						<path
							d="M20 13.5A8 8 0 1 1 10.5 4a6.6 6.6 0 0 0 9.5 9.5Z"
							stroke="currentColor"
							stroke-width="2"
							stroke-linejoin="round"
						/>
					</svg>
					<span>Тёмная</span>
				{/if}
			</button>
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
		font-size: 20px;
		font-weight: 600;
		letter-spacing: -0.02em;
		display: inline-flex;
		gap: 4px;
	}

	.header__logo-mark {
		background: var(--accent);
		color: #ffffff;
		border-radius: 8px;
		padding: 0 6px;
	}

	.header__nav {
		display: flex;
		gap: 28px;
	}

.header__link {
	font-size: 14px;
	font-weight: 500;
	color: var(--muted);
}

.header__link:hover {
	color: var(--text);
}

	.header__link--disabled {
		cursor: not-allowed;
	}

	.header__link--disabled:hover {
		color: var(--muted);
	}

	.header__actions {
		display: flex;
		align-items: center;
		gap: 16px;
	}

	.theme-toggle {
		display: inline-flex;
		align-items: center;
		gap: 8px;
		font-size: 12px;
		font-weight: 500;
		color: var(--muted);
		border: 1px solid var(--border);
		border-radius: 999px;
		padding: 8px 14px;
		transition:
			color 0.15s ease,
			border-color 0.15s ease;
	}

	.theme-toggle:hover {
		color: var(--text);
		border-color: var(--accent);
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

		.theme-toggle span {
			display: none;
		}

		.theme-toggle {
			padding: 10px;
		}
	}
</style>
