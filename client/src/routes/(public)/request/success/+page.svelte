<script lang="ts">
	import { goto } from '$app/navigation';
	import { page } from '$app/state';
	import Eyebrow from '#lib/components/Eyebrow.svelte';
	import Button from '#lib/components/Button.svelte';
	import StatusBadge from '#lib/components/StatusBadge.svelte';
	import type { RepairOrderDto } from '../../../../services/api.ts';
	import { fetchOrderById } from '../../../../services/api.ts';
	import { workshop } from '../../../../mocks/db.ts';

	// The identifier stays internal: the client sees the recap, not the order id.
	const orderId = $derived(page.url.searchParams.get('id') ?? '');
	const device = $derived(page.url.searchParams.get('device') ?? '');
	const services = $derived(page.url.searchParams.get('services') ?? '');
	const price = $derived(page.url.searchParams.get('price') ?? '—');
	const date = $derived(page.url.searchParams.get('date') ?? '');
	const time = $derived(page.url.searchParams.get('time') ?? '');

	let order = $state<RepairOrderDto | null>(null);
	let notFound = $state(false);

	$effect(() => {
		if (!orderId) {
			goto('/request');
			return;
		}
		fetchOrderById(orderId)
			.then((result) => (order = result))
			.catch(() => (notFound = true));
	});

	const dateFormatted = $derived(
		date
			? new Date(`${date}T12:00:00Z`).toLocaleDateString('ru-RU', {
				day: 'numeric',
				month: 'long',
				timeZone: workshop.timeZone
			})
			: ''
	);
</script>

{#if orderId}
	<section class="container section success">
		<div class="card">
			<div class="check" aria-hidden="true">
				<svg width="32" height="32" viewBox="0 0 24 24" fill="none">
					<path d="M5 12.5 10 17.5 19 7" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" />
				</svg>
			</div>

			<Eyebrow>Заявка отправлена</Eyebrow>
			<h1 class="h2 title">Заявка принята</h1>

			<p class="text">
				Мы сохранили заявку. Мастер проведёт бесплатную диагностику и подтвердит стоимость.
				Начнём ремонт только после вашего согласия.
			</p>

			<dl class="recap">
				{#if device}
					<div><dt>Устройство</dt><dd>{device}</dd></div>
				{/if}
				{#if services}
					<div><dt>Услуги</dt><dd>{services}</dd></div>
				{/if}
				{#if dateFormatted}
					<div><dt>Дата визита</dt><dd>{dateFormatted}{time ? `, ${time}` : ''}</dd></div>
				{/if}
				<div>
					<dt>Ориентировочная цена</dt>
					<dd>{price === '—' ? 'Уточнит мастер' : `от ${price} ₽`}</dd>
				</div>
			</dl>

			<div class="status-line">
				{#if order}
					<span class="status-label">Статус заявки:</span>
					<StatusBadge status={order.status} />
				{:else if notFound}
					<span class="status-label status-label--warn">Статус появится, когда мастерская обработает заявку.</span>
				{:else}
					<span class="status-label">Проверяем статус…</span>
				{/if}
			</div>

			<div class="actions">
				<Button href="/">На главную</Button>
			</div>
		</div>
	</section>
{/if}

<style>
	.success {
		display: flex;
		justify-content: center;
	}

	.card {
		max-width: 640px;
		width: 100%;
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 24px;
		padding: 48px;
		text-align: center;
	}

	.check {
		width: 64px;
		height: 64px;
		border-radius: 50%;
		background: var(--accent);
		color: #ffffff;
		display: inline-flex;
		align-items: center;
		justify-content: center;
		margin-bottom: 24px;
	}

	.title {
		margin-bottom: 8px;
	}

	.text {
		color: var(--muted);
		font-size: 15px;
		line-height: 1.6;
	}

	.recap {
		display: flex;
		flex-wrap: wrap;
		gap: 12px 32px;
		justify-content: center;
		margin-top: 24px;
		padding: 20px;
		background: var(--bg);
		border-radius: 16px;
	}

	.recap div {
		display: flex;
		flex-direction: column;
		gap: 2px;
	}

	.recap dt {
		font-size: 11px;
		font-weight: 600;
		letter-spacing: 0.06em;
		text-transform: uppercase;
		color: var(--muted);
	}

	.recap dd {
		margin: 0;
		font-size: 14px;
		font-weight: 500;
	}

	.status-line {
		margin-top: 24px;
		display: flex;
		align-items: center;
		justify-content: center;
		gap: 10px;
		flex-wrap: wrap;
	}

	.status-label {
		font-size: 13px;
		color: var(--muted);
	}

	.status-label--warn {
		color: var(--warning);
	}

	.actions {
		margin-top: 28px;
		display: flex;
		gap: 12px;
		justify-content: center;
		flex-wrap: wrap;
	}

	@media (max-width: 600px) {
		.card {
			padding: 28px 20px;
		}
	}
</style>
