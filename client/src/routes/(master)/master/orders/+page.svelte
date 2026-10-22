<script lang="ts">
	import StatusSelect from '#lib/components/StatusSelect.svelte';
	import { showToast } from '#lib/components/toast.svelte.ts';
	import { workshop } from '../../../../mocks/db.ts';
	import {
		fetchOrders,
		deriveMasterStats,
		isMockMode,
		changeOrderStatus,
		statusLabels,
		type RepairOrderDto,
		type RepairStatus,
		type MasterStats
	} from '../../../../services/api.ts';

	// ── Data ───────────────────────────────────────────────────────────────
	let orders = $state<RepairOrderDto[]>([]);
	let stats = $state<MasterStats | null>(null);
	let loading = $state(true);
	let loadError = $state(false);

	// ── Filters / search / sort ─────────────────────────────────────────────
	type StatusFilter = 'all' | RepairStatus;
	let filter = $state<StatusFilter>('all');
	let query = $state('');
	let sort = $state<'new' | 'old'>('new');

	const PAGE_SIZE = 7; // mockup shows 7 rows per page
	let page = $state(1);

	const priceFormat = new Intl.NumberFormat('ru-RU');

	const filtered = $derived(
		orders
			.filter((o) => filter === 'all' || o.status === filter)
			.filter((o) => {
				if (!dateFilterActive) return true;
				const created = Date.parse(o.createdAt);
				const from = dateFrom ? Date.parse(`${dateFrom}T00:00:00`) : -Infinity;
				const to = dateTo ? Date.parse(`${dateTo}T23:59:59.999`) : Infinity;
				return created >= from && created <= to;
			})
			.filter((o) => {
				const q = query.trim().toLowerCase();
				if (!q) return true;
				return (
					o.number.toLowerCase().includes(q) ||
					(o.clientName?.toLowerCase().includes(q) ?? false) ||
					o.deviceName.toLowerCase().includes(q)
				);
			})
			.toSorted((a, b) => {
				const diff = Date.parse(b.createdAt) - Date.parse(a.createdAt);
				return sort === 'new' ? diff : -diff;
			})
	);

	const totalPages = $derived(Math.max(1, Math.ceil(filtered.length / PAGE_SIZE)));
	const safePage = $derived(Math.min(page, totalPages));
	const pageOrders = $derived(filtered.slice((safePage - 1) * PAGE_SIZE, safePage * PAGE_SIZE));

	const chips = $derived<{ id: StatusFilter; label: string; count: number }[]>([
		{ id: 'all', label: 'Все', count: orders.length },
		{ id: 'Created', label: 'Принята', count: stats?.statusCounts.Created ?? 0 },
		{ id: 'InDiagnostic', label: 'Диагностика', count: stats?.statusCounts.InDiagnostic ?? 0 },
		{ id: 'InProgress', label: 'В ремонте', count: stats?.statusCounts.InProgress ?? 0 },
		{ id: 'Ready', label: 'Готово', count: stats?.statusCounts.Ready ?? 0 }
	]);

	// ── Date filter (sprint-2 card 15) ─────────────────────────────────────
	let dateFrom = $state('');
	let dateTo = $state('');

	async function loadOrders(): Promise<void> {
		try {
			const list = await fetchOrders({ from: dateFrom || undefined, to: dateTo || undefined });
			orders = list;
			stats = deriveMasterStats(list, !isMockMode);
		} catch {
			showToast('Не удалось загрузить заявки. Попробуйте обновить страницу.', 'error');
		} finally {
			loading = false;
		}
	}

	// ── Load ────────────────────────────────────────────────────────────────
	$effect(() => {
		loadOrders();
	});

	// Apply the date filter server-side (card 7 contract); re-apply on change.
	$effect(() => {
		void dateFrom;
		void dateTo;
		if (!dateFrom && !dateTo) return;
		if (isMockMode) return; // filtered client-side below
		page = 1;
		loadOrders();
	});

	// Client-side double-filter: exact today even before backend card 7 ships;
	// once it filters server-side the client pass is a no-op on the same range.
	const dateFilterActive = $derived(!!dateFrom || !!dateTo);

	function resetDateFilter(): void {
		dateFrom = '';
		dateTo = '';
		if (!isMockMode) {
			page = 1;
			loadOrders();
		}
	}

	// Reset page when filters change
	$effect(() => {
		void filter;
		void query;
		page = 1;
	});

	// ── Status change (per row) ─────────────────────────────────────────────
	let changing = $state<string | null>(null);

	async function onStatusChange(order: RepairOrderDto, next: RepairStatus): Promise<void> {
		if (changing || !order.status) return;
		changing = order.id;
		const previous = order.status;
		order.status = next;
		try {
			await changeOrderStatus(order.id, next);
			stats = deriveMasterStats(orders, !isMockMode);
			showToast(`Заявка ${displayReference(order)}: статус изменён на «${statusLabels[next]}»`);
		} catch (error) {
			order.status = previous;
			showToast(error instanceof Error ? error.message : 'Не удалось изменить статус', 'error');
		} finally {
			changing = null;
		}
	}

	function displayReference(order: RepairOrderDto): string {
		return order.id === order.number ? order.id.slice(0, 8).toUpperCase() : order.number;
	}

	// ── Formatting ──────────────────────────────────────────────────────────
	function formatDate(iso: string): string {
		return new Date(iso).toLocaleDateString('ru-RU', {
			day: '2-digit',
			month: '2-digit',
			timeZone: workshop.timeZone
		});
	}

	function formatVisit(iso: string): string {
		return new Date(iso).toLocaleString('ru-RU', {
			dateStyle: 'short',
			timeStyle: 'short',
			timeZone: workshop.timeZone
		});
	}

	function price(order: RepairOrderDto): string {
		if (order.price === null) return '—';
		return `${order.priceFrom ? 'от ' : ''}${priceFormat.format(order.price)} ₽`;
	}

	const todayLabel = $derived(
		new Date().toLocaleDateString('ru-RU', {
			weekday: 'long',
			day: 'numeric',
			month: 'long',
			timeZone: workshop.timeZone
		})
	);
</script>

<div class="orders">
	<header class="orders__head">
		<div>
			<h1 class="orders__title">Заявки на ремонт</h1>
			<p class="orders__date">{todayLabel} · Всё важное в одной очереди</p>
		</div>
		<a class="orders__new" href="/request">+ Новая заявка</a>
	</header>

	<!-- ── Stats ── -->
	{#if stats}
		<section class="stats" aria-label="Показатели мастерской">
			<article class="stat">
				<p class="stat__label">Заявок сегодня</p>
				<p class="stat__value">{stats.todayCount}</p>
				<p class="stat__note stat__note--success">{stats.todayDelta}</p>
			</article>
			<article class="stat">
				<p class="stat__label">В работе</p>
				<p class="stat__value">{stats.inWork}</p>
				<p class="stat__note">{stats.inWorkNote}</p>
			</article>
			<article class="stat">
				<p class="stat__label">Выручка за месяц</p>
				<p class="stat__value">{stats.revenueMonth ?? '—'}</p>
				<p class="stat__note">{stats.revenueNote}</p>
			</article>
			<article class="stat">
				<p class="stat__label">Клиенты</p>
				<p class="stat__value">{stats.clientsCount ?? '—'}</p>
				<p class="stat__note">{stats.clientsNote}</p>
			</article>
		</section>
	{/if}

	<!-- ── Search + sort ── -->
	<div class="toolbar">
		<div class="search">
			<svg class="search__icon" width="16" height="16" viewBox="0 0 24 24" fill="none" aria-hidden="true">
				<circle cx="11" cy="11" r="7" stroke="currentColor" stroke-width="2" />
				<path d="M20 20l-3.5-3.5" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
			</svg>
			<input
				class="search__input"
				type="search"
				placeholder={isMockMode ? 'Номер заявки, клиент или устройство' : 'Имя клиента или устройство'}
				aria-label="Поиск по заявкам"
				bind:value={query}
			/>
		</div>
		<select class="sort" aria-label="Сортировка" bind:value={sort}>
			<option value="new">Сначала новые</option>
			<option value="old">Сначала старые</option>
		</select>
	</div>

	<!-- ── Date filter (card 15) + status chips ── -->
	<div class="filters">
		<div class="chips" role="group" aria-label="Фильтр по статусу">
			{#each chips as chip (chip.id)}
				<button
					class="chip"
					class:chip--active={filter === chip.id}
					aria-pressed={filter === chip.id}
					onclick={() => (filter = chip.id)}
				>
					{chip.label}
					<span class="chip__count">{chip.count}</span>
				</button>
			{/each}
		</div>
		<div class="daterange">
			<input
				class="daterange__input"
				type="date"
				aria-label="Заявки с даты"
				max={dateTo || undefined}
				bind:value={dateFrom}
				onchange={() => (page = 1)}
			/>
			<span class="daterange__dash" aria-hidden="true">—</span>
			<input
				class="daterange__input"
				type="date"
				aria-label="Заявки по дату"
				min={dateFrom || undefined}
				bind:value={dateTo}
				onchange={() => (page = 1)}
			/>
			{#if dateFrom || dateTo}
				<button class="daterange__reset" type="button" onclick={resetDateFilter}>Сбросить</button>
			{/if}
		</div>
	</div>

	<!-- ── Orders list ── -->
	{#if loading}
		<p class="state">Загружаем заявки…</p>
	{:else if loadError}
		<p class="state state--error">Заявки не загрузились. Обновите страницу.</p>
	{:else if pageOrders.length === 0}
		<p class="state">{dateFilterActive ? 'Ничего не найдено за выбранный период.' : 'Заявок по фильтру нет.'}</p>
	{:else}
		<section class="table" aria-label="Список заявок">
			<!-- Desktop table -->
			<div class="table__scroll">
				<table class="table__grid">
					<thead>
						{#if isMockMode}
							<tr>
								<th scope="col">№ заявки</th><th scope="col">Клиент</th><th scope="col">Устройство</th><th scope="col">Услуга</th><th scope="col">Цена</th><th scope="col">Статус</th><th scope="col">Дата</th>
							</tr>
						{:else}
							<tr>
								<th scope="col">Клиент</th><th scope="col">Устройство</th><th scope="col">Желаемый визит</th><th scope="col">Создана</th><th scope="col">Статус</th>
							</tr>
						{/if}
					</thead>
					<tbody>
						{#each pageOrders as order (order.id)}
							<tr class:row--changing={changing === order.id}>
								{#if isMockMode}
									<td><span class="row__number">{displayReference(order)}</span></td>
									<td class="row__client">{order.clientName ?? '—'}</td>
									<td class="row__device">{order.deviceName}</td>
									<td class="row__service">{order.serviceName ?? '—'}</td>
									<td class="row__price">{price(order)}</td>
									<td><StatusSelect status={order.status} disabled={changing !== null} onchange={(next) => onStatusChange(order, next)} /></td>
									<td class="row__date">{formatDate(order.createdAt)}</td>
								{:else}
									<td class="row__client">{order.clientName ?? '—'}</td>
									<td class="row__device">{order.deviceName}</td>
									<td class="row__date">{order.preferredVisitAt ? formatVisit(order.preferredVisitAt) : '—'}</td>
									<td class="row__date">{formatDate(order.createdAt)}</td>
									<td><StatusSelect status={order.status} disabled={changing !== null} onchange={(next) => onStatusChange(order, next)} /></td>
								{/if}
							</tr>
						{/each}
					</tbody>
				</table>
			</div>

			<!-- Mobile cards -->
			<ul class="cards">
				{#each pageOrders as order (order.id)}
					<li class="card">
						<div class="card__head">
							<span class="card__number" title={isMockMode ? order.id : undefined}>{isMockMode ? displayReference(order) : (order.clientName ?? 'Клиент без имени')}</span>
							<span class="card__date">{formatDate(order.createdAt)}</span>
						</div>
						<p class="card__device">{order.deviceName}</p>
						{#if isMockMode}
							<p class="card__client">{order.clientName ?? '—'}</p>
							<p class="card__service">{order.serviceName ?? '—'}</p>
						{/if}
						{#if !isMockMode}
							<p class="card__service">Желаемый визит: {order.preferredVisitAt ? formatVisit(order.preferredVisitAt) : '—'}</p>
						{/if}
						<div class="card__bottom">
							{#if isMockMode}
								<div class="card__price">
									<span class="card__price-label">Цена</span>
									<span class="card__price-value">{price(order)}</span>
								</div>
							{/if}
							<StatusSelect status={order.status} disabled={changing !== null} onchange={(next) => onStatusChange(order, next)} />
						</div>
					</li>
				{/each}
			</ul>
		</section>

		<footer class="table__footer">
			<p class="table__shown">Показано {pageOrders.length} из {filtered.length} заявок</p>
			{#if totalPages > 1}
				<nav class="pager" aria-label="Страницы">
					<button
						class="pager__btn"
						type="button"
						disabled={safePage === 1}
						aria-label="Предыдущая страница"
						onclick={() => (page = safePage - 1)}
					>
						‹
					</button>
					{#each Array.from({ length: totalPages }, (_, i) => i + 1) as p (p)}
						<button
							class="pager__btn"
							class:pager__btn--active={p === safePage}
							aria-current={p === safePage ? 'page' : undefined}
							onclick={() => (page = p)}
						>
							{p}
						</button>
					{/each}
					<button
						class="pager__btn"
						type="button"
						disabled={safePage === totalPages}
						aria-label="Следующая страница"
						onclick={() => (page = safePage + 1)}
					>
						›
					</button>
				</nav>
			{/if}
		</footer>
	{/if}

	<p class="orders__hint">
		Измените статус в строке заявки — клиент увидит обновление на странице своего заказа.
	</p>
</div>

<style>
	.orders {
		display: flex;
		flex-direction: column;
		gap: 24px;
	}

	/* ── Header ── */
	.orders__head {
		display: flex;
		align-items: flex-start;
		justify-content: space-between;
		gap: 16px;
	}

	.orders__title {
		font-size: 28px;
		font-weight: 600;
		letter-spacing: -0.015em;
	}

	.orders__date {
		margin-top: 6px;
		font-size: 14px;
		color: var(--muted);
	}

	.orders__new {
		flex-shrink: 0;
		font-size: 14px;
		font-weight: 500;
		background: var(--accent);
		color: #ffffff;
		border-radius: 12px;
		padding: 13px 20px;
		transition: background-color 0.15s ease;
	}

	.orders__new:hover {
		background: var(--accent-hover);
	}

	/* ── Stats ── */
	.stats {
		display: grid;
		grid-template-columns: repeat(4, minmax(0, 1fr));
		gap: 16px;
	}

	.stat {
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 16px;
		padding: 20px 24px;
	}

	.stat__label {
		font-size: 13px;
		color: var(--muted);
	}

	.stat__value {
		margin-top: 6px;
		font-size: 28px;
		font-weight: 600;
		letter-spacing: -0.01em;
	}

	.stat__note {
		margin-top: 6px;
		font-size: 12px;
		color: var(--muted);
	}

	.stat__note--success {
		color: var(--success);
	}

	/* ── Toolbar ── */
	.toolbar {
		display: flex;
		gap: 12px;
	}

	.search {
		position: relative;
		flex: 1;
		max-width: 460px;
	}

	.search__icon {
		position: absolute;
		left: 14px;
		top: 50%;
		transform: translateY(-50%);
		color: var(--muted);
		pointer-events: none;
	}

	.search__input {
		width: 100%;
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 10px;
		padding: 12px 14px 12px 40px;
		font-size: 14px;
		transition: border-color 0.15s ease;
	}

	.search__input:focus {
		border-color: var(--accent);
		outline: none;
	}

	.sort {
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 10px;
		padding: 0 14px;
		font-size: 14px;
		color: var(--text);
		appearance: none;
		background-image: linear-gradient(45deg, transparent 49%, var(--muted) 50%),
			linear-gradient(-45deg, transparent 49%, var(--muted) 50%);
		background-size: 6px 6px;
		background-position: right 14px center, right 9px center;
		background-repeat: no-repeat;
		padding-right: 30px;
		cursor: pointer;
	}

	/* ── Filters row (chips + dates) ── */
	.filters {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 16px;
		flex-wrap: wrap;
		margin-top: 16px;
	}

	.daterange {
		display: flex;
		align-items: center;
		gap: 8px;
	}

	.daterange__input {
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 10px;
		padding: 9px 12px;
		font-size: 13px;
		color: var(--text);
	}

	.daterange__input:focus {
		border-color: var(--accent);
		outline: none;
	}

	.daterange__dash {
		color: var(--muted);
	}

	.daterange__reset {
		font-size: 13px;
		font-weight: 500;
		color: var(--accent);
		padding: 9px 10px;
		border-radius: 10px;
	}

	.daterange__reset:hover {
		background: var(--accent-soft);
	}

	/* ── Chips ── */
	.chips {
		display: flex;
		flex-wrap: wrap;
		gap: 8px;
	}

	.chip {
		display: inline-flex;
		align-items: center;
		gap: 8px;
		font-size: 13px;
		font-weight: 500;
		color: var(--muted);
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 10px;
		padding: 9px 14px;
		transition:
			color 0.15s ease,
			border-color 0.15s ease,
			background-color 0.15s ease;
	}

	.chip:hover {
		color: var(--text);
		border-color: var(--accent);
	}

	.chip--active {
		color: var(--accent);
		border-color: var(--accent);
		background: var(--accent-soft);
	}

	.chip__count {
		font-size: 12px;
		color: var(--muted);
		background: var(--bg);
		border-radius: 999px;
		padding: 1px 8px;
	}

	.chip--active .chip__count {
		color: #ffffff;
		background: var(--accent);
	}

	/* ── Table ── */
	.table {
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 16px;
		overflow: hidden;
	}

	.table__scroll {
		overflow-x: auto;
	}

	.table__grid {
		width: 100%;
		border-collapse: collapse;
		font-size: 14px;
	}

	.table__grid thead th {
		text-align: left;
		font-size: 12px;
		font-weight: 500;
		color: var(--muted);
		background: var(--bg);
		padding: 14px 20px;
		white-space: nowrap;
	}

	.table__grid tbody td {
		padding: 16px 20px;
		border-top: 1px solid var(--border);
		vertical-align: middle;
		white-space: nowrap;
	}

	.table__grid tbody tr:first-child td {
		border-top: none;
	}

	.table__grid tbody tr:hover {
		background: var(--accent-soft);
	}

	.row--changing {
		opacity: 0.6;
	}

	.row__number {
		display: block;
		font-weight: 600;
	}

	.row__client {
		display: block;
		font-size: 12px;
		color: var(--muted);
		margin-top: 2px;
	}

	.row__device,
	.row__service {
		color: var(--text);
	}

	.row__price {
		font-weight: 500;
	}

	.row__date {
		font-size: 12px;
		color: var(--muted);
	}

	/* ── Mobile cards ── */
	.cards {
		display: none;
		flex-direction: column;
		gap: 12px;
		padding: 16px;
	}

	.card {
		background: var(--bg);
		border: 1px solid var(--border);
		border-radius: 16px;
		padding: 16px;
		display: flex;
		flex-direction: column;
		gap: 4px;
	}

	.card__head {
		display: flex;
		justify-content: space-between;
		gap: 12px;
	}

	.card__number {
		font-weight: 600;
		font-size: 15px;
	}

	.card__date {
		font-size: 12px;
		color: var(--muted);
	}

	.card__client {
		margin-top: 6px;
		font-size: 14px;
	}

	.card__device {
		font-size: 14px;
		color: var(--muted);
	}

	.card__service {
		font-size: 13px;
		color: var(--muted);
	}

	.card__bottom {
		display: flex;
		justify-content: space-between;
		align-items: center;
		gap: 12px;
		margin-top: 12px;
	}

	.card__price {
		display: flex;
		flex-direction: column;
	}

	.card__price-label {
		font-size: 11px;
		color: var(--muted);
	}

	.card__price-value {
		font-size: 16px;
		font-weight: 600;
	}

	/* ── Footer / pager ── */
	.table__footer {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 16px;
	}

	.table__shown {
		font-size: 13px;
		color: var(--muted);
	}

	.pager {
		display: flex;
		gap: 6px;
	}

	.pager__btn {
		min-width: 34px;
		height: 34px;
		display: inline-flex;
		align-items: center;
		justify-content: center;
		font-size: 14px;
		color: var(--muted);
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 10px;
	}

	.pager__btn:hover:not(:disabled):not(.pager__btn--active) {
		color: var(--text);
		border-color: var(--accent);
	}

	.pager__btn--active {
		color: var(--accent);
		border-color: var(--accent);
		background: var(--accent-soft);
	}

	.pager__btn:disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}

	/* ── States ── */
	.state {
		padding: 48px 20px;
		text-align: center;
		color: var(--muted);
		background: var(--surface);
		border: 1px dashed var(--border);
		border-radius: 16px;
	}

	.state--error {
		color: var(--danger);
	}

	.orders__hint {
		font-size: 12px;
		color: var(--muted);
	}

	/* ── Responsive ── */
	@media (max-width: 900px) {
		.table__scroll,
		.table__grid {
			display: none;
		}

		.cards {
			display: flex;
		}

		.stats {
			grid-template-columns: repeat(2, minmax(0, 1fr));
		}

		.toolbar {
			flex-direction: column;
		}

		.sort {
			width: 100%;
		}

		.orders__new {
			padding: 11px 16px;
		}
	}
</style>
