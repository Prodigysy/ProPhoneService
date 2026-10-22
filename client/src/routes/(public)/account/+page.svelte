<script lang="ts">
	import { goto } from '$app/navigation';
	import Eyebrow from '#lib/components/Eyebrow.svelte';
	import Button from '#lib/components/Button.svelte';
	import StatusBadge from '#lib/components/StatusBadge.svelte';
	import { session } from '#lib/auth/session.svelte.ts';
	import { showToast } from '#lib/components/toast.svelte.ts';
	import { fetchMyOrders, type AccountOrderDto } from '../../../services/api.ts';

	const ready = $derived(session.ready && !!session.user);

	let orders = $state<AccountOrderDto[]>([]);
	let loading = $state(true);
	let loadError = $state(false);
	let filter = $state<'all' | 'InProgress' | 'Ready'>('all');

	// Mockup chips: Все | В ремонте | Готово — «Выдана» counts as «Готово».
	const chips = $derived([
		{ id: 'all' as const, label: 'Все', count: orders.length },
		{ id: 'InProgress' as const, label: 'В ремонте', count: orders.filter((o) => o.status === 'InProgress').length },
		{ id: 'Ready' as const, label: 'Готово', count: orders.filter((o) => o.status !== 'InProgress').length }
	]);

	const visible = $derived(
		filter === 'all' ? orders : orders.filter((o) => (filter === 'Ready' ? o.status !== 'InProgress' : o.status === filter))
	);
	const currentRepair = $derived(orders.find((o) => o.status === 'InProgress') ?? null);

	const priceFormat = new Intl.NumberFormat('ru-RU');

	function formatDate(iso: string): string {
		return iso ? new Date(iso).toLocaleDateString('ru-RU', { day: '2-digit', month: '2-digit', year: 'numeric' }) : '—';
	}

	function initials(name: string): string {
		return name
			.split(/\s+/)
			.slice(0, 2)
			.map((part) => part[0]?.toUpperCase() ?? '')
			.join('');
	}

	$effect(() => {
		if (!session.ready) return;
		if (!session.user) {
			goto('/login?next=/account');
			return;
		}
		fetchMyOrders()
			.then((list) => (orders = list))
			.catch(() => {
				loadError = true;
				showToast('Не удалось загрузить ремонты. Попробуйте обновить страницу.', 'error');
			})
			.finally(() => (loading = false));
	});

	async function signOut(): Promise<void> {
		await session.logout();
		showToast('Вы вышли из кабинета');
		goto('/');
	}
</script>

<section class="container section account">
	{#if !ready}
		<p class="state">Загружаем кабинет…</p>
	{:else if session.user}
		<div class="account__head">
			<div>
				<Eyebrow>Личный кабинет</Eyebrow>
				<h1 class="account__title">{session.user.name.split(' ')[0]}, рады вас видеть</h1>
				<p class="account__lead">Ваши устройства, история обращений и всё, что нужно знать о текущем ремонте.</p>
			</div>
			<div class="profile">
				<span class="profile__avatar" aria-hidden="true">{initials(session.user.name)}</span>
				<div class="profile__info">
					<p class="profile__name">{session.user.name}</p>
					<p class="profile__contact">{session.user.phone ?? session.user.email ?? ''}</p>
				</div>
				<button class="profile__signout" type="button" onclick={signOut}>Выйти ↗</button>
			</div>
		</div>

		{#if currentRepair}
			<section class="current" aria-label="Сейчас в ремонте">
				<p class="current__label">Сейчас в ремонте</p>
				<div class="current__row">
					<h2 class="current__title">{currentRepair.device} · {currentRepair.repair}</h2>
					<a class="current__link" href="/account/{currentRepair.id}">Отслеживать ремонт ↗</a>
				</div>
				<p class="current__meta">
					{currentRepair.expectedReady ? `${currentRepair.expectedReady} · ` : ''}Итоговая цена {priceFormat.format(currentRepair.price)} ₽
				</p>
			</section>
		{/if}

		<div class="chips" role="group" aria-label="Фильтр по статусу">
			{#each chips as chip (chip.id)}
				<button class="chip" class:chip--active={filter === chip.id} aria-pressed={filter === chip.id} onclick={() => (filter = chip.id)}>
					{chip.label} · <span class="chip__count">{chip.count}</span>
				</button>
			{/each}
		</div>

		<section class="orders" aria-label="Мои заказы">
			<h2 class="orders__title">Мои заказы</h2>
			{#if loading}
				<div class="skeleton">
					{#each ['r1', 'r2', 'r3', 'r4'] as row (row)}
						<div class="skeleton__row"><span class="skeleton__line skeleton__line--lg"></span><span class="skeleton__line"></span></div>
					{/each}
				</div>
			{:else if loadError}
				<p class="state state--error">Ремонты не загрузились. Обновите страницу.</p>
			{:else if visible.length === 0}
				<div class="empty">
					<p class="empty__title">Заявок нет</p>
					<p class="empty__text">Сломался телефон? Оставьте заявку — история появится здесь.</p>
					<Button href="/request" size="md">Оставить заявку</Button>
				</div>
			{:else}
				<div class="table">
					<div class="table__head">
						<span>Устройство / ремонт</span><span>Дата</span><span>Статус</span><span>Цена</span>
					</div>
					{#each visible as order (order.id)}
						<a class="table__row" href="/account/{order.id}">
							<span class="table__device">
								<span class="table__device-name">{order.device}</span>
								<span class="table__repair">{order.repair}</span>
							</span>
							<span class="table__date">{formatDate(order.createdAt)}</span>
							<span class="table__status"><StatusBadge status={order.status} /></span>
							<span class="table__price">{priceFormat.format(order.price)} ₽</span>
						</a>
					{/each}
				</div>
			{/if}
		</section>

		<section class="cta">
			<h2 class="cta__title">Есть ещё устройство, которому нужна помощь?</h2>
			<p class="cta__text">Мы сохраним всю историю ремонта здесь, в вашем кабинете.</p>
			<Button href="/request" size="md">Оставить заявку</Button>
		</section>
	{/if}
</section>

<style>
	.account__head {
		display: flex;
		align-items: flex-start;
		justify-content: space-between;
		gap: 32px;
	}

	.account__title {
		font-size: clamp(26px, 3vw, 36px);
		font-weight: 700;
		letter-spacing: -0.02em;
	}

	.account__lead {
		color: var(--muted);
		font-size: 15px;
		margin-top: 8px;
	}

	.profile {
		display: flex;
		align-items: center;
		gap: 14px;
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 16px;
		padding: 14px 20px;
	}

	.profile__avatar {
		width: 44px;
		height: 44px;
		border-radius: 50%;
		background: var(--accent);
		color: #ffffff;
		display: inline-flex;
		align-items: center;
		justify-content: center;
		font-size: 15px;
		font-weight: 600;
	}

	.profile__name {
		font-size: 14px;
		font-weight: 600;
	}

	.profile__contact {
		font-size: 12px;
		color: var(--muted);
		margin-top: 2px;
	}

	.profile__signout {
		font-size: 13px;
		font-weight: 500;
		color: var(--accent);
		margin-left: 12px;
		white-space: nowrap;
	}

	.profile__signout:hover {
		text-decoration: underline;
		text-underline-offset: 3px;
	}

	.current {
		margin-top: 28px;
		background: var(--accent-soft);
		border: 1px solid var(--border);
		border-radius: 16px;
		padding: 22px 26px;
	}

	.current__label {
		font-size: 11px;
		font-weight: 600;
		letter-spacing: 0.08em;
		text-transform: uppercase;
		color: var(--accent);
	}

	.current__row {
		display: flex;
		align-items: baseline;
		justify-content: space-between;
		gap: 16px;
		margin-top: 8px;
	}

	.current__title {
		font-size: 19px;
		font-weight: 600;
	}

	.current__link {
		font-size: 13px;
		font-weight: 500;
		color: var(--accent);
		white-space: nowrap;
	}

	.current__meta {
		margin-top: 6px;
		font-size: 14px;
		color: var(--muted);
	}

	.chips {
		display: flex;
		gap: 10px;
		margin-top: 28px;
	}

	.chip {
		font-size: 13px;
		font-weight: 500;
		color: var(--muted);
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 999px;
		padding: 8px 16px;
	}

	.chip--active {
		background: var(--accent-soft);
		color: var(--accent);
		border-color: transparent;
	}

	.chip__count {
		font-weight: 600;
	}

	.orders {
		margin-top: 24px;
	}

	.orders__title {
		font-size: 20px;
		font-weight: 600;
		margin-bottom: 16px;
	}

	.table {
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 16px;
		overflow: hidden;
	}

	.table__head,
	.table__row {
		display: grid;
		grid-template-columns: 2fr 1fr 1fr 1fr;
		gap: 16px;
		align-items: center;
		padding: 14px 22px;
	}

	.table__head {
		font-size: 11px;
		font-weight: 600;
		letter-spacing: 0.06em;
		text-transform: uppercase;
		color: var(--muted);
		border-bottom: 1px solid var(--border);
		background: var(--bg);
	}

	.table__row {
		color: var(--text);
		text-decoration: none;
		transition: background-color 0.15s ease;
	}

	.table__row + .table__row {
		border-top: 1px solid var(--border);
	}

	.table__row:hover {
		background: var(--bg);
	}

	.table__device {
		display: flex;
		flex-direction: column;
		gap: 2px;
	}

	.table__device-name {
		font-size: 14px;
		font-weight: 600;
	}

	.table__repair {
		font-size: 13px;
		color: var(--muted);
	}

	.table__date {
		font-size: 13px;
		color: var(--muted);
	}

	.table__price {
		font-size: 14px;
		font-weight: 600;
		text-align: right;
	}

	.skeleton {
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 16px;
		padding: 8px 22px;
	}

	.skeleton__row {
		display: flex;
		justify-content: space-between;
		gap: 24px;
		padding: 16px 0;
	}

	.skeleton__row + .skeleton__row {
		border-top: 1px solid var(--border);
	}

	.skeleton__line {
		flex: 1;
		max-width: 220px;
		height: 14px;
		border-radius: 7px;
		background: linear-gradient(90deg, var(--bg) 25%, var(--surface) 50%, var(--bg) 75%);
		background-size: 200% 100%;
		animation: shimmer 1.4s ease-in-out infinite;
	}

	.skeleton__line--lg {
		max-width: 320px;
		height: 18px;
	}

	@keyframes shimmer {
		from {
			background-position: 200% 0;
		}
		to {
			background-position: -200% 0;
		}
	}

	.empty {
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 16px;
		padding: 40px;
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 10px;
		text-align: center;
	}

	.empty__title {
		font-size: 17px;
		font-weight: 600;
	}

	.empty__text {
		font-size: 14px;
		color: var(--muted);
		margin-bottom: 8px;
	}

	.cta {
		margin-top: 48px;
		background: var(--accent);
		color: #ffffff;
		border-radius: 24px;
		padding: 40px 48px;
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 24px;
	}

	.cta__title {
		font-size: clamp(22px, 2.6vw, 32px);
		font-weight: 700;
		letter-spacing: -0.02em;
	}

	.cta__text {
		font-size: 14px;
		opacity: 0.85;
		margin-top: 8px;
	}

	.cta :global(.btn--primary) {
		background: #ffffff;
		color: var(--accent);
		flex-shrink: 0;
	}

	.cta :global(.btn--primary:hover) {
		background: var(--bg);
	}

	@media (max-width: 1024px) {
		.account__head {
			flex-direction: column;
		}

		.table__head,
		.table__row {
			grid-template-columns: 1fr auto auto;
		}

		.table__date,
		.table__head > span:nth-child(2) {
			display: none;
		}
	}

	@media (max-width: 640px) {
		.cta {
			flex-direction: column;
			align-items: flex-start;
			padding: 28px 24px;
		}

		.current__row {
			flex-direction: column;
			gap: 6px;
		}
	}
</style>
