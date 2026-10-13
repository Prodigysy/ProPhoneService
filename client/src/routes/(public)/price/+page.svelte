<script lang="ts">
	import Eyebrow from '#lib/components/Eyebrow.svelte';
	import Button from '#lib/components/Button.svelte';
	import { priceRows, priceUpdatedAt } from '../../../mocks/db.ts';
	import { fetchServices, isMockMode, type ServiceDto } from '../../../services/api.ts';

	type Category = 'all' | 'apple' | 'samsung' | 'xiaomi';

	let category = $state<Category>('all');
	let services = $state<ServiceDto[]>([]);
	let servicesLoading = $state(!isMockMode);
	let servicesError = $state(false);

	const filters: { id: Category; label: string }[] = [
		{ id: 'all', label: 'Все устройства' },
		{ id: 'apple', label: 'Apple' },
		{ id: 'samsung', label: 'Samsung' },
		{ id: 'xiaomi', label: 'Xiaomi / Redmi' }
	];

	const priceFormat = new Intl.NumberFormat('ru-RU');

	$effect(() => {
		if (isMockMode) return;
		fetchServices()
			.then((items) => {
				services = items;
				servicesError = false;
			})
			.catch(() => (servicesError = true))
			.finally(() => (servicesLoading = false));
	});

	function priceFor(row: (typeof priceRows)[number], key: Exclude<Category, 'all'>): string {
		const value = row[key];
		if (value === null) return 'Бесплатно';
		return `от ${priceFormat.format(value)} ₽`;
	}
</script>

<section class="container section">
	<Eyebrow>Понятно до ремонта</Eyebrow>
	<h1 class="h2 heading">Хороший ремонт. Честная цена.</h1>
	<p class="lead">
		{isMockMode
			? 'Популярные работы и ориентировочная стоимость. Запчасти и работа мастера уже включены.'
			: 'Активные услуги и цены из базы мастерской. Итоговая стоимость подтверждается после диагностики.'}
	</p>

	<div class="toolbar">
		{#if isMockMode}
			<div class="filters" role="group" aria-label="Фильтр по производителю">
				{#each filters as filter (filter.id)}
					<button
						class="filter"
						class:filter--active={category === filter.id}
						aria-pressed={category === filter.id}
						onclick={() => (category = filter.id)}
					>
						{filter.label}
					</button>
				{/each}
			</div>
		{/if}
		<p class="updated">{isMockMode ? `Цены обновлены ${priceUpdatedAt}` : 'Актуальные цены из прайс-листа мастерской'}</p>
	</div>

	{#if !isMockMode && servicesError}
		<p class="load-error" role="alert">Не удалось загрузить прайс. Проверьте подключение к API и обновите страницу.</p>
	{/if}
	{#if !isMockMode && servicesLoading}
		<p class="service-loading" role="status">Загружаем актуальный прайс…</p>
	{/if}

	<!-- Desktop: model comparison in mock mode, actual service DTOs in API mode. -->
	<div class="table-wrap">
		<table class="price-table">
			{#if isMockMode}
				<thead>
					<tr><th scope="col">Тип ремонта</th><th scope="col">iPhone 11–15</th><th scope="col">Samsung Galaxy</th><th scope="col">Xiaomi / Redmi</th><th scope="col">Срок</th></tr>
				</thead>
				<tbody>
					{#each priceRows as row (row.id)}
						<tr>
							<td><span class="row-name">{row.name}</span><span class="row-desc">{row.description}</span></td>
							<td class="price">{priceFor(row, 'apple')}</td>
							<td class="price">{priceFor(row, 'samsung')}</td>
							<td class="price">{priceFor(row, 'xiaomi')}</td>
							<td class="duration">{row.duration}</td>
						</tr>
					{/each}
				</tbody>
			{:else}
				<thead><tr><th scope="col">Услуга</th><th scope="col">Описание</th><th scope="col">Цена</th></tr></thead>
				<tbody>
					{#each services as service (service.id)}
						<tr>
							<td><span class="row-name">{service.name}</span></td>
							<td class="row-desc">{service.description ?? 'Описание не указано'}</td>
							<td class="price">{service.price === 0 ? 'Бесплатно' : `${priceFormat.format(service.price)} ₽`}</td>
						</tr>
					{:else}
						<tr><td colspan="3" class="duration">{servicesLoading ? 'Загружаем услуги…' : 'Активных услуг пока нет.'}</td></tr>
					{/each}
				</tbody>
			{/if}
		</table>
	</div>

	<!-- Mobile cards -->
	<div class="cards">
		{#if isMockMode}
			{#each priceRows as row (row.id)}
				<article class="card">
					<header class="card__head"><h2 class="card__name">{row.name}</h2><p class="card__desc">{row.description}</p></header>
					{#if category === 'all'}
						<dl class="card__prices">
							<div><dt>iPhone 11–15</dt><dd>{priceFor(row, 'apple')}</dd></div>
							<div><dt>Samsung Galaxy</dt><dd>{priceFor(row, 'samsung')}</dd></div>
							<div><dt>Xiaomi / Redmi</dt><dd>{priceFor(row, 'xiaomi')}</dd></div>
						</dl>
					{:else}<p class="card__single-price">{priceFor(row, category)}</p>{/if}
					<p class="card__duration">Срок: {row.duration}</p>
				</article>
			{/each}
		{:else}
			{#each services as service (service.id)}
				<article class="card">
					<header class="card__head"><h2 class="card__name">{service.name}</h2><p class="card__desc">{service.description ?? 'Описание не указано'}</p></header>
					<p class="card__single-price">{service.price === 0 ? 'Бесплатно' : `${priceFormat.format(service.price)} ₽`}</p>
					<p class="card__duration">Итог подтвердим после диагностики.</p>
				</article>
			{:else}<p class="card__desc">{servicesLoading ? 'Загружаем услуги…' : 'Активных услуг пока нет.'}</p>{/each}
		{/if}
	</div>

	<div class="note" role="note">
		<strong>{isMockMode ? 'Это ориентир, а не окончательный счёт.' : 'Окончательную стоимость подтвердит мастер.'}</strong>
		{isMockMode
			? ' Цена зависит от модели и сложности поломки. Точную стоимость подтвердим после бесплатной диагностики и согласуем с вами. Никаких дополнительных работ без вашего согласия.'
			: ' Цена в списке — цена услуги из базы. Не начинаем дополнительные работы без согласия клиента.'}
	</div>

	<div class="cta">
		<h2 class="cta__title">Не нашли свою проблему?</h2>
		<p class="cta__text">Опишите, что случилось. Поможем разобраться и оценим ремонт.</p>
		<Button href="/request">Оставить заявку</Button>
	</div>
</section>

<style>
	.heading {
		margin-bottom: 12px;
	}

	.lead {
		color: var(--muted);
		max-width: 560px;
	}

	.toolbar {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 16px;
		flex-wrap: wrap;
		margin-top: 32px;
	}

	.filters {
		display: flex;
		gap: 8px;
		flex-wrap: wrap;
	}

	.filter {
		font-size: 13px;
		font-weight: 500;
		color: var(--muted);
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 999px;
		padding: 9px 18px;
		transition:
			color 0.15s ease,
			border-color 0.15s ease,
			background-color 0.15s ease;
	}

	.filter:hover {
		color: var(--text);
	}

	.filter--active {
		color: var(--accent);
		border-color: var(--accent);
		background: var(--accent-soft);
	}

	.updated {
		font-size: 12px;
		color: var(--muted);
	}

	.service-loading,
	.load-error {
		margin-top: 16px;
		font-size: 14px;
		color: var(--muted);
	}

	.load-error {
		color: var(--danger);
	}

	/* ---------- Desktop table ---------- */
	.table-wrap {
		margin-top: 24px;
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 16px;
		overflow: hidden;
	}

	.price-table {
		width: 100%;
		border-collapse: collapse;
	}

	.price-table th {
		text-align: left;
		font-size: 11px;
		font-weight: 600;
		letter-spacing: 0.08em;
		text-transform: uppercase;
		color: var(--muted);
		padding: 14px 20px;
		border-bottom: 1px solid var(--border);
	}

	.price-table td {
		padding: 18px 20px;
		border-bottom: 1px solid var(--border);
		vertical-align: top;
	}

	.price-table tbody tr:last-child td {
		border-bottom: none;
	}

	.price-table tbody tr {
		transition: background-color 0.15s ease;
	}

	.price-table tbody tr:hover {
		background: var(--accent-soft);
	}

	.row-name {
		display: block;
		font-size: 15px;
		font-weight: 500;
	}

	.row-desc {
		display: block;
		font-size: 12px;
		color: var(--muted);
		margin-top: 3px;
	}

	.price {
		font-size: 15px;
		white-space: nowrap;
	}

	.duration {
		font-size: 13px;
		color: var(--muted);
		white-space: nowrap;
	}

	/* ---------- Mobile cards ---------- */
	.cards {
		display: none;
		margin-top: 24px;
		flex-direction: column;
		gap: 12px;
	}

	.card {
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 16px;
		padding: 20px;
	}

	.card__name {
		font-size: 17px;
		font-weight: 600;
		margin: 0;
	}

	.card__desc {
		font-size: 13px;
		color: var(--muted);
		margin-top: 3px;
	}

	.card__prices {
		display: flex;
		flex-direction: column;
		gap: 10px;
		margin: 16px 0 0;
	}

	.card__prices div {
		display: flex;
		justify-content: space-between;
		gap: 12px;
	}

	.card__prices dt {
		font-size: 13px;
		color: var(--muted);
	}

	.card__prices dd {
		font-size: 14px;
		font-weight: 500;
		margin: 0;
	}

	.card__single-price {
		font-size: 20px;
		font-weight: 600;
		margin: 16px 0 0;
	}

	.card__duration {
		margin-top: 12px;
		font-size: 12px;
		color: var(--muted);
	}

	/* ---------- Note ---------- */
	.note {
		margin-top: 24px;
		background: var(--warning-soft);
		border-radius: 16px;
		padding: 20px 24px;
		font-size: 14px;
		line-height: 1.6;
	}

	/* ---------- CTA ---------- */
	.cta {
		margin-top: 64px;
		text-align: center;
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 12px;
	}

	.cta__title {
		font-size: 24px;
		font-weight: 600;
	}

	.cta__text {
		color: var(--muted);
	}

	.cta :global(.btn) {
		margin-top: 8px;
	}

	/* ---------- Responsive ---------- */
	@media (max-width: 900px) {
		.table-wrap {
			display: none;
		}

		.cards {
			display: flex;
		}
	}
</style>
