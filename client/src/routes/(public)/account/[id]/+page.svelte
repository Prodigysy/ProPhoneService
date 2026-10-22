<script lang="ts">
	import { goto } from '$app/navigation';
	import Eyebrow from '#lib/components/Eyebrow.svelte';
	import Button from '#lib/components/Button.svelte';
	import StatusBadge from '#lib/components/StatusBadge.svelte';
	import { showToast } from '#lib/components/toast.svelte.ts';
	import { session } from '#lib/auth/session.svelte.ts';
	import {
		fetchMyOrderById,
		createReview,
		type AccountOrderDto
	} from '../../../../services/api.ts';

	const orderId = $derived(pageUrlId());

	let order = $state<AccountOrderDto | null>(null);
	let notFound = $state(false);

	import { page } from '$app/state';
	function pageUrlId(): string {
		return page.params.id ?? '';
	}

	let rating = $state(0);
	let text = $state('');
	let busy = $state(false);
	let alreadyReviewed = $state(false);

	const priceFormat = new Intl.NumberFormat('ru-RU');

	$effect(() => {
		if (!session.ready) return;
		if (!session.user) {
			goto('/login?next=/account');
			return;
		}
		if (!orderId) {
			goto('/account');
			return;
		}
		fetchMyOrderById(orderId)
			.then((result) => (order = result))
			.catch(() => (notFound = true));
	});

	async function submitReview(event: SubmitEvent): Promise<void> {
		event.preventDefault();
		if (!order || busy) return;
		if (rating < 1) {
			showToast('Поставьте оценку от 1 до 5 звёзд', 'error');
			return;
		}
		if (text.trim().length < 10) {
			showToast('Расскажите чуть подробнее — хотя бы 10 символов', 'error');
			return;
		}
		busy = true;
		try {
			await createReview(order.id, rating, text.trim(), session.user?.name ?? 'Клиент');
			alreadyReviewed = true;
			showToast('Спасибо! Отзыв опубликован');
		} catch (error) {
			const message = error instanceof Error ? error.message : 'Не удалось отправить отзыв';
			if (message.includes('уже')) alreadyReviewed = true;
			showToast(message, 'error');
		} finally {
			busy = false;
		}
	}
</script>

<section class="container section repair">
	{#if !session.ready || !session.user}
		<p class="state">Загружаем кабинет…</p>
	{:else if notFound}
		<p class="state">Ремонт не найден. <a class="repair__back" href="/account">Вернуться в кабинет</a></p>
	{:else if order}
		<a class="repair__back" href="/account">← Все ремонты</a>
		<Eyebrow>Карточка ремонта</Eyebrow>
		<div class="head">
			<h1 class="head__title">{order.device} · {order.repair}</h1>
			<StatusBadge status={order.status} />
		</div>

		<div class="grid">
			<div class="summary">
				<div class="summary__row">
					<span class="summary__label">Дата обращения</span>
					<span class="summary__value">{order.createdAt ? new Date(order.createdAt).toLocaleDateString('ru-RU', { day: '2-digit', month: 'long', year: 'numeric' }) : '—'}</span>
				</div>
				<div class="summary__row">
					<span class="summary__label">Услуги</span>
					<span class="summary__value">{order.repair}</span>
				</div>
				<div class="summary__row">
					<span class="summary__label">Стоимость</span>
					<span class="summary__value summary__value--price">{priceFormat.format(order.price)} ₽</span>
				</div>
				{#if order.expectedReady}
					<div class="summary__row">
						<span class="summary__label">Готовность</span>
						<span class="summary__value">{order.expectedReady.replace('Ожидаемая готовность: ', '')}</span>
					</div>
				{/if}
			</div>

			<div class="timeline-card">
				<p class="timeline-card__title">Этапы ремонта</p>
				<ol class="timeline">
					{#each order.timeline as step, i (step.label + i)}
						<li class="timeline__item" class:timeline__item--last={i === order.timeline.length - 1}>
							<span class="timeline__dot" aria-hidden="true"></span>
							<span class="timeline__label">{step.label}</span>
							<span class="timeline__date">{step.at}</span>
						</li>
					{/each}
				</ol>
			</div>
		</div>

		{#if order.status === 'Completed'}
			<section class="review" aria-label="Отзыв о ремонте">
				<h2 class="review__title">Как всё прошло?</h2>
				{#if alreadyReviewed}
					<p class="review__done">Отзыв уже оставлен — спасибо, что помогаете мастерской стать лучше.</p>
				{:else}
					<form class="review__form" onsubmit={submitReview}>
						<div class="review__stars" role="radiogroup" aria-label="Оценка от 1 до 5">
							{#each Array.from({ length: 5 }, (_, i) => i + 1) as star (star)}
								<button
									class="review__star"
									class:review__star--on={star <= rating}
									type="button"
									role="radio"
									aria-checked={rating === star}
									aria-label={`${star} из 5`}
									onclick={() => (rating = star)}
								>
									★
								</button>
							{/each}
						</div>
						<textarea
							class="review__input"
							rows="4"
							placeholder="Что понравилось? Что можно улучшить?"
							bind:value={text}
						></textarea>
						<div class="review__submit">
							<Button type="submit" disabled={busy}>{busy ? 'Отправляем…' : 'Отправить отзыв'}</Button>
						</div>
					</form>
				{/if}
			</section>
		{/if}
	{/if}
</section>

<style>
	.repair__back {
		display: inline-block;
		font-size: 13px;
		font-weight: 500;
		color: var(--muted);
		margin-bottom: 16px;
	}

	.repair__back:hover {
		color: var(--text);
	}

	.head {
		display: flex;
		align-items: center;
		gap: 16px;
		margin-bottom: 28px;
	}

	.head__title {
		font-size: clamp(22px, 2.6vw, 30px);
		font-weight: 700;
		letter-spacing: -0.02em;
	}

	.grid {
		display: grid;
		grid-template-columns: 1fr 1fr;
		gap: 24px;
		align-items: start;
	}

	.summary,
	.timeline-card {
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 16px;
		padding: 26px;
	}

	.summary__row {
		display: flex;
		justify-content: space-between;
		gap: 24px;
		padding-block: 10px;
	}

	.summary__row + .summary__row {
		border-top: 1px solid var(--border);
	}

	.summary__label {
		font-size: 13px;
		color: var(--muted);
	}

	.summary__value {
		font-size: 14px;
		font-weight: 500;
		text-align: right;
	}

	.summary__value--price {
		font-size: 16px;
		font-weight: 700;
	}

	.timeline-card__title {
		font-size: 16px;
		font-weight: 600;
		margin-bottom: 16px;
	}

	.timeline {
		display: flex;
		flex-direction: column;
	}

	.timeline__item {
		position: relative;
		display: flex;
		align-items: baseline;
		gap: 10px;
		padding-bottom: 18px;
		padding-left: 24px;
	}

	.timeline__item::before {
		content: '';
		position: absolute;
		left: 5px;
		top: 16px;
		bottom: -2px;
		width: 2px;
		background: var(--border);
	}

	.timeline__item--last::before {
		display: none;
	}

	.timeline__dot {
		position: absolute;
		left: 0;
		top: 6px;
		width: 12px;
		height: 12px;
		border-radius: 50%;
		background: var(--accent);
	}

	.timeline__label {
		font-size: 14px;
		font-weight: 500;
	}

	.timeline__date {
		font-size: 12px;
		color: var(--muted);
		margin-left: auto;
		white-space: nowrap;
	}

	.review {
		margin-top: 28px;
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 16px;
		padding: 26px;
	}

	.review__title {
		font-size: 18px;
		font-weight: 600;
	}

	.review__form {
		margin-top: 16px;
		display: flex;
		flex-direction: column;
		gap: 14px;
	}

	.review__stars {
		display: flex;
		gap: 4px;
	}

	.review__star {
		font-size: 28px;
		line-height: 1;
		color: var(--border);
		transition:
			color 0.15s ease,
			transform 0.15s ease;
	}

	.review__star:hover {
		transform: scale(1.1);
	}

	.review__star--on {
		color: #d97706;
	}

	.review__input {
		background: var(--bg);
		border: 1px solid var(--border);
		border-radius: 10px;
		padding: 12px 14px;
		font-size: 14px;
		resize: vertical;
	}

	.review__input:focus {
		border-color: var(--accent);
		outline: none;
	}

	.review__submit {
		display: flex;
	}

	.review__done {
		margin-top: 12px;
		font-size: 14px;
		color: var(--muted);
	}

	@media (max-width: 900px) {
		.grid {
			grid-template-columns: 1fr;
		}
	}
</style>
