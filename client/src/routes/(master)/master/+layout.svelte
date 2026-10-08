<script lang="ts">
	import Toast from '#lib/components/Toast.svelte';
	import { showToast } from '#lib/components/toast.svelte.ts';
	import { masterProfile } from '../../../mocks/db.ts';

	let { children } = $props();

	const navItems = [
		{ id: 'orders', label: 'Заявки', count: '12', active: true },
		{ id: 'clients', label: 'Клиенты', count: null, active: false },
		{ id: 'price', label: 'Прайс-лист', count: null, active: false },
		{ id: 'reports', label: 'Отчёты', count: null, active: false }
	];
</script>

<div class="workspace">
	<aside class="sidebar">
		<div class="sidebar__top">
			<a class="sidebar__logo" href="/">про телефон</a>
			<p class="sidebar__sub">Рабочий кабинет</p>
			<p class="sidebar__address">Покровка, 17</p>
		</div>

		<nav class="sidebar__nav" aria-label="Разделы кабинета">
			{#each navItems as item (item.id)}
				{#if item.active}
					<a class="sidebar__item sidebar__item--active" href="/master/orders" aria-current="page">
						{item.label}
						{#if item.count}<span class="sidebar__count">{item.count}</span>{/if}
					</a>
				{:else}
					<button
						class="sidebar__item"
						type="button"
						onclick={() => showToast('Раздел в разработке')}
					>
						{item.label}
						{#if item.count}<span class="sidebar__count">{item.count}</span>{/if}
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

	<main class="workspace__main">
		{@render children()}
	</main>
</div>
<Toast />

<style>
	.workspace {
		display: grid;
		grid-template-columns: 240px 1fr;
		min-height: 100vh;
	}

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
		font-size: 13px;
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
			overflow-x: auto;
			gap: 16px;
			padding: 12px 16px;
		}

		.sidebar__top {
			display: flex;
			align-items: baseline;
			gap: 8px;
		}

		.sidebar__nav {
			flex-direction: row;
		}

		.sidebar__bottom {
			margin-top: 0;
			margin-left: auto;
		}

		.sidebar__back,
		.sidebar__profile-info,
		.sidebar__address,
		.sidebar__sub {
			display: none;
		}

		.workspace__main {
			padding: 20px 16px;
		}
	}
</style>
