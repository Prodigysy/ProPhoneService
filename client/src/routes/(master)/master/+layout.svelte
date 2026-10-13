<script lang="ts">
	import Toast from '#lib/components/Toast.svelte';
	import { showToast } from '#lib/components/toast.svelte.ts';
	import { theme } from '#lib/theme/theme.svelte.ts';
	import { masterProfile, workshop } from '../../../mocks/db.ts';

	let { children } = $props();

	const navItems = [
		{ id: 'orders', label: 'Заявки' },
		{ id: 'clients', label: 'Клиенты' },
		{ id: 'price', label: 'Прайс-лист' },
		{ id: 'reports', label: 'Отчёты' }
	];

	const ordersCount = 12;
</script>

<div class="workspace">
	<aside class="sidebar">
		<div class="sidebar__top">
			<a class="sidebar__logo" href="/">про телефон</a>
			<p class="sidebar__sub">Рабочая мастерская</p>
			<p class="sidebar__address">{workshop.addressShort}</p>
		</div>

		<nav class="sidebar__nav" aria-label="Разделы кабинета">
			{#each navItems as item (item.id)}
				{#if item.id === 'orders'}
					<a class="sidebar__item sidebar__item--active" href="/master/orders" aria-current="page">
						{item.label}
						<span class="sidebar__count">{ordersCount}</span>
					</a>
				{:else}
					<button
						class="sidebar__item"
						type="button"
						onclick={() => showToast('Раздел в разработке')}
					>
						{item.label}
					</button>
				{/if}
			{/each}
		</nav>

		<div class="sidebar__bottom">
			<a class="sidebar__profile" href="/master/orders">
				<span class="sidebar__avatar" aria-hidden="true">{masterProfile.initials}</span>
				<span class="sidebar__profile-info">
					<span class="sidebar__name">{masterProfile.name}</span>
					<span class="sidebar__role">Владелец · мастер</span>
				</span>
			</a>
			<a class="sidebar__back" href="/">На сайт мастерской ↗</a>
		</div>
	</aside>

	<div class="workspace__right">
		<header class="topbar">
			<nav class="topbar__crumbs" aria-label="Раздел">
				<span class="topbar__crumb">Рабочий кабинет</span>
				<span class="topbar__crumb-sep" aria-hidden="true">·</span>
				<span class="topbar__crumb topbar__crumb--current">
					{navItems.find((i) => i.id === 'orders')?.label ?? 'Заявки'}
				</span>
			</nav>

			<div class="topbar__actions">
				<div class="topbar__theme" role="group" aria-label="Тема оформления">
					<button
						class="theme-btn"
						class:theme-btn--active={theme.value === 'light'}
						type="button"
						aria-pressed={theme.value === 'light'}
						onclick={() => theme.set('light')}
					>
						Светлая
					</button>
					<button
						class="theme-btn"
						class:theme-btn--active={theme.value === 'dark'}
						type="button"
						aria-pressed={theme.value === 'dark'}
						onclick={() => theme.set('dark')}
					>
						Тёмная
					</button>
				</div>
				<span class="topbar__avatar" aria-hidden="true">{masterProfile.initials}</span>
			</div>
		</header>

		<main class="workspace__main">
			{@render children()}
		</main>
	</div>
</div>
<Toast />

<style>
	.workspace {
		display: grid;
		grid-template-columns: 240px 1fr;
		min-height: 100vh;
	}

	/* ── Sidebar ── */
	.sidebar {
		display: flex;
		flex-direction: column;
		gap: 24px;
		padding: 24px 16px;
		border-right: 1px solid var(--border);
		background: var(--surface);
	}

	.sidebar__logo {
		font-size: 18px;
		font-weight: 600;
	}

	.sidebar__sub {
		font-size: 11px;
		font-weight: 600;
		letter-spacing: 0.08em;
		text-transform: uppercase;
		color: var(--muted);
		margin-top: 4px;
	}

	.sidebar__address {
		font-size: 12px;
		color: var(--muted);
		margin-top: 2px;
	}

	.sidebar__nav {
		display: flex;
		flex-direction: column;
		gap: 4px;
	}

	.sidebar__item {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 8px;
		width: 100%;
		padding: 10px 12px;
		font-size: 14px;
		font-weight: 500;
		border-radius: 10px;
		color: var(--muted);
		transition:
			background-color 0.15s ease,
			color 0.15s ease;
	}

	.sidebar__item:hover {
		background: var(--accent-soft);
		color: var(--text);
	}

	.sidebar__item--active {
		background: var(--accent-soft);
		color: var(--accent);
	}

	.sidebar__count {
		font-size: 12px;
		background: var(--bg);
		border-radius: 999px;
		padding: 2px 8px;
	}

	.sidebar__item--active .sidebar__count {
		background: var(--accent);
		color: #ffffff;
	}

	.sidebar__bottom {
		margin-top: auto;
		display: flex;
		flex-direction: column;
		gap: 12px;
	}

	.sidebar__profile {
		display: flex;
		align-items: center;
		gap: 10px;
		text-decoration: none;
	}

	.sidebar__avatar {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		width: 36px;
		height: 36px;
		border-radius: 50%;
		background: var(--accent);
		color: #ffffff;
		font-size: 13px;
		font-weight: 600;
		flex-shrink: 0;
	}

	.sidebar__profile-info {
		display: flex;
		flex-direction: column;
		line-height: 1.25;
	}

	.sidebar__name {
		font-size: 13px;
		font-weight: 500;
	}

	.sidebar__role {
		font-size: 11px;
		color: var(--muted);
	}

	.sidebar__back {
		font-size: 13px;
		color: var(--accent);
	}

	/* ── Right column: topbar + main ── */
	.workspace__right {
		display: flex;
		flex-direction: column;
		min-width: 0;
	}

	.topbar {
		position: sticky;
		top: 0;
		z-index: 100;
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 16px;
		min-height: 64px;
		padding: 0 32px;
		background: var(--surface);
		border-bottom: 1px solid var(--border);
	}

	.topbar__crumbs {
		display: flex;
		align-items: center;
		gap: 8px;
		font-size: 14px;
	}

	.topbar__crumb {
		color: var(--muted);
	}

	.topbar__crumb--current {
		color: var(--text);
		font-weight: 500;
	}

	.topbar__crumb-sep {
		color: var(--muted);
	}

	.topbar__actions {
		display: flex;
		align-items: center;
		gap: 16px;
	}

	.topbar__theme {
		display: inline-flex;
		border: 1px solid var(--border);
		border-radius: 999px;
		padding: 3px;
		gap: 2px;
	}

	.theme-btn {
		font-size: 12px;
		font-weight: 500;
		color: var(--muted);
		border-radius: 999px;
		padding: 6px 14px;
		transition:
			background-color 0.15s ease,
			color 0.15s ease;
	}

	.theme-btn--active {
		background: var(--accent);
		color: #ffffff;
	}


	.topbar__avatar {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		width: 36px;
		height: 36px;
		border-radius: 50%;
		background: var(--accent-soft);
		color: var(--accent);
		font-size: 13px;
		font-weight: 600;
	}

	.workspace__main {
		padding: 32px 40px;
		min-width: 0;
	}

	@media (max-width: 768px) {
		.workspace {
			grid-template-columns: 1fr;
		}

		.sidebar {
			flex-direction: row;
			align-items: center;
			gap: 12px;
			padding: 12px 16px;
			min-width: 0;
			flex-wrap: wrap;
		}

		.sidebar__top {
			display: flex;
			align-items: baseline;
			gap: 8px;
			flex-shrink: 0;
		}

		.sidebar__nav {
			flex-direction: row;
			flex-wrap: wrap;
			min-width: 0;
		}

		.sidebar__item {
			padding: 8px 10px;
		}

		.sidebar__bottom {
			margin-top: 0;
		}

		.sidebar__back,
		.sidebar__profile-info,
		.sidebar__address,
		.sidebar__sub {
			display: none;
		}

		.topbar {
			padding: 0 16px;
			gap: 10px;
		}

		.topbar__crumbs {
			min-width: 0;
			overflow: hidden;
			text-overflow: ellipsis;
			white-space: nowrap;
		}

		.topbar__crumb:not(.topbar__crumb--current),
		.topbar__crumb-sep {
			display: none;
		}

		.topbar__actions {
			gap: 8px;
			min-width: 0;
		}

		.topbar__theme,
		.topbar__avatar {
			display: none;
		}

		.workspace__main {
			padding: 20px 16px;
		}
	}
</style>
