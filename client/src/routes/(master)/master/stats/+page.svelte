<script lang="ts">
	import Chart from 'chart.js/auto';
	import { showToast } from '#lib/components/toast.svelte.ts';
	import {
		fetchMasterStats,
		type MasterStatsDto,
		type StatsPeriod
	} from '../../../../services/api.ts';

	const PERIODS: { id: StatsPeriod; label: string }[] = [
		{ id: 'month', label: 'Месяц' },
		{ id: 'quarter', label: 'Квартал' },
		{ id: 'all', label: 'Всё время' }
	];

	let period = $state<StatsPeriod>('month');
	let stats = $state<MasterStatsDto | null>(null);
	let loading = $state(true);
	let loadError = $state(false);

	let byTypeCanvas = $state<HTMLCanvasElement | null>(null);
	let byPriceCanvas = $state<HTMLCanvasElement | null>(null);
	let byTypeChart: Chart | null = null;
	let byPriceChart: Chart | null = null;

	const priceFormat = new Intl.NumberFormat('ru-RU');

	const TYPE_LABELS: Record<string, string> = {
		Phone: 'Телефоны',
		Tablet: 'Планшеты',
		Laptop: 'Ноутбуки'
	};

	$effect(() => {
		let cancelled = false;
		loading = true;
		loadError = false;
		fetchMasterStats(period)
			.then((data) => {
				if (cancelled) return;
				stats = data;
			})
			.catch(() => {
				if (cancelled) return;
				loadError = true;
				showToast('Не удалось загрузить статистику. Попробуйте ещё раз.', 'error');
			})
			.finally(() => {
				if (!cancelled) loading = false;
			});
		return () => {
			cancelled = true;
		};
	});

	// Approximate average price per completed repair type — doughnut weight.
	const PRICE_HINT: Record<string, number> = { Phone: 2500, Tablet: 3500, Laptop: 4500 };

	$effect(() => {
		if (!stats || !byTypeCanvas || !byPriceCanvas) return;
		byTypeChart?.destroy();
		byPriceChart?.destroy();

		const entries = Object.entries(stats.repairsByType);
		byTypeChart = new Chart(byTypeCanvas, {
			type: 'bar',
			data: {
				labels: entries.map(([type]) => TYPE_LABELS[type] ?? type),
				datasets: [
					{
						label: 'Выполнено ремонтов',
						data: entries.map(([, count]) => count),
						backgroundColor: getComputedStyle(document.documentElement).getPropertyValue('--accent').trim() || '#245bff',
						borderRadius: 8,
						maxBarThickness: 64
					}
				]
			},
			options: {
				responsive: true,
				maintainAspectRatio: false,
				plugins: { legend: { display: false } },
				scales: {
					y: { beginAtZero: true, ticks: { precision: 0 } }
				}
			}
		});

		byPriceChart = new Chart(byPriceCanvas, {
			type: 'doughnut',
			data: {
				labels: entries.map(([type]) => TYPE_LABELS[type] ?? type),
				datasets: [
					{
						data: entries.map(([type, count]) => count * (PRICE_HINT[type] ?? 0)),
						backgroundColor: ['#245bff', '#6c8cff', '#a9baff'],
						borderWidth: 0
					}
				]
			},
			options: {
				responsive: true,
				maintainAspectRatio: false,
				cutout: '62%',
				plugins: { legend: { position: 'right' } }
			}
		});
	});
</script>

<div class="stats-page">
	<header class="stats-page__head">
		<div>
			<h1 class="stats-page__title">Отчёты</h1>
			<p class="stats-page__note">Сколько ремонтов завершено и на какую сумму за выбранный период.</p>
		</div>
		<div class="period" role="group" aria-label="Период">
			{#each PERIODS as p (p.id)}
				<button class="period__btn" class:period__btn--active={period === p.id} type="button" aria-pressed={period === p.id} onclick={() => (period = p.id)}>
					{p.label}
				</button>
			{/each}
		</div>
	</header>

	{#if loading}
		<div class="cards">
			{#each ['c1', 'c2'] as c (c)}
				<div class="skeleton"></div>
			{/each}
		</div>
	{:else if loadError}
		<p class="state state--error">Статистика не загрузилась. Попробуйте сменить период.</p>
	{:else if stats && stats.completedCount === 0}
		<div class="empty">
			<p class="empty__title">За этот период нет завершённых ремонтов</p>
			<p class="empty__text">Смените период или завершите заявку в разделе «Заявки».</p>
		</div>
	{:else if stats}
		<div class="cards">
			<article class="card">
				<p class="card__label">Сумма выполненных заказов</p>
				<p class="card__value">{priceFormat.format(stats.completedSum)} ₽</p>
				<p class="card__note">{stats.completedCount} {stats.completedCount % 10 === 1 && stats.completedCount % 100 !== 11 ? 'заказ' : 'заказов'} завершено</p>
			</article>
			<article class="card">
				<p class="card__label">Всего ремонтов за период</p>
				<p class="card__value">{Object.values(stats.repairsByType).reduce((sum, n) => sum + n, 0)}</p>
				<p class="card__note">по типам устройств</p>
			</article>
		</div>

		<div class="charts">
			<section class="chart-box" aria-label="Ремонты по типам устройств">
				<h2 class="chart-box__title">Ремонты по типам устройств</h2>
				<div class="chart-box__area"><canvas bind:this={byTypeCanvas}></canvas></div>
			</section>
			<section class="chart-box" aria-label="Доля типов устройств">
				<h2 class="chart-box__title">Доля типов устройств</h2>
				<div class="chart-box__area"><canvas bind:this={byPriceCanvas}></canvas></div>
			</section>
		</div>
	{:else}
		<div class="empty">
			<p class="empty__title">За этот период нет завершённых ремонтов</p>
			<p class="empty__text">Смените период или завершите заявку в разделе «Заявки».</p>
		</div>
	{/if}
</div>

<style>
	.stats-page__head {
		display: flex;
		align-items: flex-start;
		justify-content: space-between;
		gap: 24px;
		margin-bottom: 24px;
		flex-wrap: wrap;
	}

	.stats-page__title {
		font-size: 26px;
		font-weight: 700;
		letter-spacing: -0.02em;
	}

	.stats-page__note {
		margin-top: 6px;
		font-size: 14px;
		color: var(--muted);
	}

	.period {
		display: inline-flex;
		background: var(--bg);
		border: 1px solid var(--border);
		border-radius: 999px;
		padding: 4px;
		gap: 2px;
	}

	.period__btn {
		font-size: 13px;
		font-weight: 500;
		color: var(--muted);
		padding: 8px 16px;
		border-radius: 999px;
	}

	.period__btn--active {
		background: var(--surface);
		color: var(--text);
		box-shadow: var(--shadow-card);
	}

	.state {
		color: var(--muted);
		font-size: 14px;
	}

	.state--error {
		color: var(--danger);
	}

	.cards {
		display: grid;
		grid-template-columns: 1fr 1fr;
		gap: 20px;
	}

	.card,
	.skeleton {
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 16px;
		padding: 22px 26px;
	}

	.card__label {
		font-size: 13px;
		color: var(--muted);
	}

	.card__value {
		font-size: 30px;
		font-weight: 700;
		letter-spacing: -0.02em;
		margin-top: 6px;
	}

	.card__note {
		font-size: 12px;
		color: var(--muted);
		margin-top: 4px;
	}

	.skeleton {
		height: 108px;
		background: linear-gradient(90deg, var(--bg) 25%, var(--surface) 50%, var(--bg) 75%);
		background-size: 200% 100%;
		animation: shimmer 1.4s ease-in-out infinite;
	}

	@keyframes shimmer {
		from {
			background-position: 200% 0;
		}
		to {
			background-position: -200% 0;
		}
	}

	.charts {
		display: grid;
		grid-template-columns: 1.4fr 1fr;
		gap: 20px;
		margin-top: 20px;
	}

	.chart-box {
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 16px;
		padding: 20px 24px;
	}

	.chart-box__title {
		font-size: 15px;
		font-weight: 600;
		margin-bottom: 14px;
	}

	.chart-box__area {
		position: relative;
		height: 260px;
	}

	.empty {
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 16px;
		padding: 40px;
		text-align: center;
	}

	.empty__title {
		font-size: 17px;
		font-weight: 600;
	}

	.empty__text {
		font-size: 14px;
		color: var(--muted);
		margin-top: 6px;
	}

	@media (max-width: 1024px) {
		.cards,
		.charts {
			grid-template-columns: 1fr;
		}
	}
</style>
