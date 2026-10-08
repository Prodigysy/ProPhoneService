<script lang="ts">
	import { goto } from '$app/navigation';
	import { page } from '$app/state';
	import Eyebrow from '#lib/components/Eyebrow.svelte';
	import Button from '#lib/components/Button.svelte';
	import StatusBadge from '#lib/components/StatusBadge.svelte';
	import type { RepairOrderDto } from '../../../../services/api.ts';
	import { fetchOrderByNumber } from '../../../../services/api.ts';

	// Order recap comes from query params (works after refresh).
	const number = $derived(page.url.searchParams.get('number'));
	const device = $derived(page.url.searchParams.get('device') ?? '');
	const problem = $derived(page.url.searchParams.get('problem') ?? '');
	const price = $derived(page.url.searchParams.get('price') ?? '—');
	const date = $derived(page.url.searchParams.get('date') ?? '');

	let order = $state<RepairOrderDto | null>(null);
	let notFound = $state(false);

	$effect(() => {
		if (!number) {
			goto('/request');
			return;
		}
		fetchOrderByNumber(number)
			.then((o) => (order = o))
			.catch(() => (notFound = true));
	});

	const dateFormatted = $derived(
		date ? new Date(`${date}T12:00:00`).toLocaleDateString('ru-RU', {
			day: 'numeric',
			month: 'long'
		}) : ''
	);
</script>

{#if number}
	<section class="container section success">
		<div class="card">
			<div class="check" aria-hidden="true">
				<svg width="32" height="32" viewBox="0 0 24 24" fill="none">
					<path d="M5 12.5 10 17.5 19 7" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" />
				</svg>
			</div>

			<Eyebrow>Заявка отправлена</Eyebrow>
			<h1 class="h2 title">Заявка № {number} принята</h1>
			<p class="number" aria-label="Номер заявки">{number}</p>

			<p class="text">
				Мы сохранили заявку. Мастер проведёт бесплатную диагностику и подтвердит стоимость.
				Начнём ремонт только после вашего согласия.
			</p>

			<dl class="recap">
				{#if device}
					<div><dt>Устройство</dt><dd>{device}</dd></div>
				{/if}
				{#if problem}
					<div><dt>Проблема</dt><dd>{problem}</dd></div>
				{/if}
				{#if dateFormatted}
					<div><dt>Дата визита</dt><dd>{dateFormatted}</dd></div>
				{/if}
				<div>
					<dt>Ориентировочная цена</dt>
					<dd>{price} ₽</dd>
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
				<Button href="/" variant="secondary">На главную</Button>
				<Button href="/price">Посмотреть цены</Button>
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

	.number {
		font-size: 44px;
		font-weight: 600;
		letter-spacing: 0.02em;
		color: var(--accent);
		margin-bottom: 16px;
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

		.number {
			font-size: 34px;
		}
	}
</style>
