<script lang="ts">
	import Eyebrow from '#lib/components/Eyebrow.svelte';
	import Button from '#lib/components/Button.svelte';
	import { priceUpdatedAt, serviceMeta, serviceOrderIndex } from '../../../mocks/db.ts';
	import { fetchServices, type ServiceDto } from '../../../services/api.ts';

	let services = $state<ServiceDto[]>([]);
	let loading = $state(true);
	let failed = $state(false);

	const priceFormat = new Intl.NumberFormat('ru-RU');

	// The API returns services in database order; the mockup defines this order.
	const rows = $derived(
		[...services].sort((a, b) => serviceOrderIndex(a.name) - serviceOrderIndex(b.name))
	);

	$effect(() => {
		fetchServices()
			.then((items) => {
				services = items;
				failed = false;
			})
			.catch(() => (failed = true))
			.finally(() => (loading = false));
	});

	function priceText(price: number): string {
		return price === 0 ? 'Бесплатно' : `от ${priceFormat.format(price)} ₽`;
	}
</script>

<section class="container section">
	<Eyebrow>Понятно до ремонта</Eyebrow>
	<h1 class="h2 heading">Хороший ремонт. Честная цена.</h1>
	<p class="lead">
		Популярные работы и ориентировочная стоимость. Запчасти и работа мастера уже включены.
	</p>

	<div class="toolbar">
		<span class="filter filter--active">Общий прайс</span>
		<p class="updated">Цены обновлены {priceUpdatedAt}</p>
	</div>

	{#if failed}
		<p class="load-error" role="alert">
			Не удалось загрузить прайс. Проверьте подключение к API и обновите страницу.
		</p>
	{/if}

	<!-- Desktop: one row per service -->
	<div class="table-wrap">
		<table class="price-table">
			<thead>
				<tr>
					<th scope="col">Услуга</th>
					<th scope="col">Цена</th>
					<th scope="col">Срок</th>
				</tr>
			</thead>
			<tbody>
				{#each rows as service (service.id)}
					<tr>
						<td>
							<span class="row-name">{serviceMeta(service.name).displayName}</span>
							<span class="row-desc">
								{service.description ?? serviceMeta(service.name).description}
							</span>
						</td>
						<td class="price">{priceText(service.price)}</td>
						<td class="duration">{serviceMeta(service.name).duration}</td>
					</tr>
				{:else}
					<tr>
						<td colspan="3" class="duration">
							{loading ? 'Загружаем услуги…' : (failed ? 'Прайс недоступен.' : 'Активных услуг пока нет.')}
						</td>
					</tr>
				{/each}
			</tbody>
		</table>
	</div>

	<!-- Mobile: cards -->
	<div class="cards">
		{#each rows as service (service.id)}
			<article class="card">
				<header class="card__head">
					<h2 class="card__name">{serviceMeta(service.name).displayName}</h2>
					<p class="card__desc">{service.description ?? serviceMeta(service.name).description}</p>
				</header>
				<p class="card__single-price">Стоимость {priceText(service.price).toLowerCase()}</p>
				<p class="card__duration">Срок: {serviceMeta(service.name).duration}</p>
			</article>
		{:else}
			<p class="card__desc">
				{loading ? 'Загружаем услуги…' : (failed ? 'Прайс недоступен.' : 'Активных услуг пока нет.')}
			</p>
		{/each}
	</div>

	<div class="note" role="note">
		<strong>Это ориентир, а не окончательный счёт.</strong>
		Цена зависит от модели и сложности поломки. Точную стоимость подтвердим после бесплатной
		диагностики и согласуем с вами. Никаких дополнительных работ без вашего согласия.
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

	.filter {
		font-size: 13px;
		font-weight: 500;
		border-radius: 999px;
		padding: 9px 18px;
		border: 1px solid var(--border);
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

	.load-error {
		margin-top: 16px;
		font-size: 14px;
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

	.card__single-price {
		font-size: 16px;
		font-weight: 600;
		margin: 14px 0 0;
	}

	.card__duration {
		margin-top: 6px;
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
