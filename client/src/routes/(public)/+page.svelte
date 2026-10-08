<script lang="ts">
	import Eyebrow from '#lib/components/Eyebrow.svelte';
	import Button from '#lib/components/Button.svelte';
	import { workshop, workshopStats, masterProfile, aboutText, steps, reviews, heroTrust } from '../../mocks/db.ts';

	// Live open/closed status computed in Europe/Moscow (workshop timezone).
	const moscowNow = new Date(
		new Date().toLocaleString('en-US', { timeZone: 'Europe/Moscow' })
	);
	const hours = moscowNow.getHours();
	const opensAt = Number(workshop.openAt.slice(0, 2));
	const closesAt = Number(workshop.closeAt.slice(0, 2));
	const isOpen = hours >= opensAt && hours < closesAt;

	const openStatus = isOpen
		? `Сегодня открыты до ${workshop.closeAt}`
		: `Сейчас закрыто · откроем в ${workshop.openAt}`;
</script>

<!-- ================= Hero ================= -->
<section class="hero">
	<div class="container hero__inner">
		<div class="hero__content">
			<Eyebrow>Ремонт телефонов на Покровке</Eyebrow>
			<h1 class="hero__title">
				Телефон снова в деле.<br />Вы — на связи.
			</h1>
			<p class="hero__subtitle">
				Сломался телефон? Оставьте заявку без звонка. Узнайте цену заранее и отслеживайте ремонт онлайн.
			</p>
			<div class="hero__actions">
				<Button href="/request" size="lg">Оставить заявку</Button>
				<Button href="/price" variant="secondary" size="lg">Посмотреть цены</Button>
			</div>
			<ul class="hero__trust">
				{#each heroTrust as item (item.title)}
					<li class="trust-item">
						<span class="trust-item__title">{item.title}</span>
						{#if item.subtitle}
							<span class="trust-item__subtitle">{item.subtitle}</span>
						{/if}
					</li>
				{/each}
			</ul>
		</div>
		<div class="hero__photo">
			<img src="/hero.jpg" alt="Мастер ремонтирует телефон в мастерской" />
		</div>
	</div>

	<div class="container hero__location">
		<div class="location">
			<span class="location__pin" aria-hidden="true">
				<svg width="16" height="16" viewBox="0 0 24 24" fill="none">
					<path
						d="M12 21s-7-5.1-7-11a7 7 0 1 1 14 0c0 5.9-7 11-7 11Z"
						stroke="currentColor"
						stroke-width="2"
					/>
					<circle cx="12" cy="10" r="2.5" stroke="currentColor" stroke-width="2" />
				</svg>
			</span>
			<div class="location__info">
				<strong>{workshop.address}</strong>
				<span class="location__metro">{workshop.metroDistance}</span>
			</div>
		</div>
		<div class="location">
			<span class="location__pin" aria-hidden="true">
				<svg width="16" height="16" viewBox="0 0 24 24" fill="none">
					<circle cx="12" cy="12" r="9" stroke="currentColor" stroke-width="2" />
					<path d="M12 7v5l3 3" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
				</svg>
			</span>
			<div class="location__info">
				<strong>{workshop.hours}</strong>
				<span class="location__status" class:location__status--open={isOpen} class:location__status--closed={!isOpen}>
					{openStatus}
				</span>
			</div>
		</div>
		<a class="location__link" href="#map">Как нас найти ↗</a>
	</div>
</section>

<!-- ================= About ================= -->
<section class="section about">
	<div class="container about__inner">
		<div class="about__text">
			<Eyebrow>О мастерской</Eyebrow>
			<h2 class="h2">Не просто техника. Ваши планы, фото и связь.</h2>
			<p class="about__paragraph">{aboutText}</p>
			<dl class="about__stats">
				<div class="stat">
					<dt class="stat__label">{workshopStats.yearsLabel}</dt>
					<dd class="stat__value">{workshopStats.years}</dd>
				</div>
				<div class="stat">
					<dt class="stat__label">{workshopStats.savedLabel}</dt>
					<dd class="stat__value">{workshopStats.saved}</dd>
				</div>
				<div class="stat">
					<dt class="stat__label">{workshopStats.ratingLabel}</dt>
					<dd class="stat__value">{workshopStats.rating}</dd>
				</div>
			</dl>
		</div>
		<div class="about__master">
			<div class="about__master-photo" aria-hidden="true">{masterProfile.initials}</div>
			<p class="about__master-name">{masterProfile.name}</p>
			<p class="about__master-role">{masterProfile.role}</p>
			<p class="about__master-promise">«{masterProfile.promise}»</p>
		</div>
	</div>
</section>

<!-- ================= Steps ================= -->
<section class="section steps-section">
	<div class="container">
		<Eyebrow>Четыре шага — и снова на связи</Eyebrow>
		<h2 class="h2 steps-section__title">Как это работает</h2>
		<ol class="steps">
			{#each steps as step (step.num)}
				<li class="step">
					<span class="step__num">{step.num}</span>
					<h3 class="step__title">{step.title}</h3>
					<p class="step__text">{step.text}</p>
				</li>
			{/each}
		</ol>
	</div>
</section>

<!-- ================= Reviews ================= -->
<section class="section reviews-section">
	<div class="container">
		<Eyebrow>Нас советуют своим</Eyebrow>
		<h2 class="h2 reviews-section__title">Отзывы клиентов</h2>
		<p class="reviews-section__rating">
			<span class="reviews-section__stars" aria-hidden="true">★★★★★</span>
			{workshopStats.rating.replace(' /', '')} · {workshopStats.reviewsCount} отзывов на Яндекс ↗
		</p>
		<div class="reviews">
			{#each reviews as review (review.id)}
				<article class="review">
					<p class="review__stars" aria-label="Оценка 5 из 5">★★★★★</p>
					<p class="review__text">{review.text}</p>
					<footer class="review__footer">
						<span class="review__avatar" aria-hidden="true">{review.initials}</span>
						<div class="review__meta">
							<span class="review__author">{review.author}</span>
							<span class="review__date">{review.date}</span>
						</div>
					</footer>
				</article>
			{/each}
		</div>
	</div>
</section>

<!-- ================= Map & hours ================= -->
<section class="section map-section" id="map">
	<div class="container">
		<Eyebrow>Как нас найти</Eyebrow>
		<h2 class="h2 map-section__title">Карта и часы работы</h2>
		<div class="map-section__grid">
			<div class="map-frame">
				<iframe
					src="https://yandex.ru/map-widget/v1/?text=Москва, Покровка 17&z=17"
					title="Карта: Москва, ул. Покровка, 17"
					loading="lazy"
					allowfullscreen
				></iframe>
				<a class="map-frame__fallback" href="https://yandex.ru/maps/?text=Москва, улица Покровка, 17" target="_blank" rel="noreferrer">
					Открыть в Яндекс Картах ↗
				</a>
			</div>
			<div class="hours-card">
				<p class="hours-card__label">{workshop.hours}</p>
				<p class="hours-card__status" class:hours-card__status--open={isOpen} class:hours-card__status--closed={!isOpen}>
					{openStatus}
				</p>
				<address class="hours-card__address">{workshop.address}</address>
				<p class="hours-card__metro">{workshop.metroDistance}</p>
				<a class="hours-card__phone" href="tel:+74951204567">{workshop.phone}</a>
			</div>
		</div>
	</div>
</section>

<!-- ================= CTA ================= -->
<section class="section cta-section">
	<div class="container cta-section__inner">
		<h2 class="h2 cta-section__title">Починим телефон.<br />Освободим ваш день.</h2>
		<p class="cta-section__text">Оставьте заявку — всё остальное возьмём на себя.</p>
		<Button href="/request" size="lg">Оставить заявку</Button>
	</div>
</section>

<style>
	/* ---------- Hero ---------- */
	.hero {
		background: var(--bg);
		padding-top: 48px;
	}

	.hero__inner {
		display: grid;
		grid-template-columns: 1fr 1.05fr;
		gap: 48px;
		align-items: center;
	}

	.hero__title {
		font-size: clamp(34px, 5vw, 64px);
		font-weight: 600;
		line-height: 1.08;
		letter-spacing: -0.02em;
		margin: 16px 0 20px;
	}

	.hero__subtitle {
		font-size: 18px;
		color: var(--muted);
		max-width: 480px;
	}

	.hero__actions {
		display: flex;
		gap: 12px;
		margin-top: 28px;
	}

	.hero__trust {
		display: flex;
		flex-wrap: wrap;
		gap: 10px;
		margin-top: 32px;
	}

	.trust-item {
		display: flex;
		flex-direction: column;
		gap: 2px;
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 12px;
		padding: 12px 16px;
	}

	.trust-item__title {
		font-size: 13px;
		font-weight: 500;
	}

	.trust-item__subtitle {
		font-size: 11px;
		color: var(--muted);
	}

	.hero__photo img {
		width: 100%;
		height: 100%;
		object-fit: cover;
		border-radius: 20px;
	}

	.hero__location {
		display: flex;
		align-items: center;
		gap: 32px;
		flex-wrap: wrap;
		padding-block: 24px 48px;
	}

	.location {
		display: flex;
		align-items: center;
		gap: 10px;
	}

	.location__pin {
		color: var(--accent);
		display: inline-flex;
	}

	.location__info {
		display: flex;
		flex-direction: column;
		line-height: 1.3;
	}

	.location__info strong {
		font-size: 15px;
		font-weight: 500;
	}

	.location__metro {
		font-size: 13px;
		color: var(--muted);
	}

	.location__status {
		font-size: 13px;
		color: var(--muted);
	}

	.location__status--open {
		color: var(--success);
	}

	.location__status--closed {
		color: var(--warning);
	}

	.location__link {
		font-size: 14px;
		font-weight: 500;
		color: var(--accent);
	}

	/* ---------- About ---------- */
	.about__inner {
		display: grid;
		grid-template-columns: 1.5fr 1fr;
		gap: 56px;
		align-items: start;
	}

	.about__paragraph {
		margin-top: 16px;
		font-size: 15px;
		color: var(--muted);
		max-width: 560px;
	}

	.about__stats {
		display: flex;
		gap: 48px;
		margin-top: 32px;
	}

	.stat__value {
		font-size: 32px;
		font-weight: 600;
		margin: 0;
	}

	.stat__label {
		font-size: 12px;
		color: var(--muted);
		margin-bottom: 4px;
	}

	.about__master {
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 16px;
		padding: 32px;
	}

	.about__master-photo {
		width: 56px;
		height: 56px;
		border-radius: 50%;
		background: var(--accent);
		color: #ffffff;
		display: flex;
		align-items: center;
		justify-content: center;
		font-size: 20px;
		font-weight: 600;
		margin-bottom: 16px;
	}

	.about__master-name {
		font-size: 17px;
		font-weight: 600;
	}

	.about__master-role {
		font-size: 13px;
		color: var(--muted);
		margin-top: 2px;
	}

	.about__master-promise {
		margin-top: 12px;
		font-size: 14px;
	}

	/* ---------- Steps ---------- */
	.steps-section__title {
		margin-bottom: 32px;
	}

	.steps {
		display: grid;
		grid-template-columns: repeat(4, 1fr);
		gap: 24px;
	}

	.step {
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 16px;
		padding: 24px;
	}

	.step__num {
		font-size: 14px;
		font-weight: 600;
		color: var(--accent);
	}

	.step__title {
		font-size: 18px;
		font-weight: 600;
		margin-top: 12px;
	}

	.step__text {
		font-size: 14px;
		color: var(--muted);
		margin-top: 8px;
	}

	/* ---------- Reviews ---------- */
	.reviews-section__title {
		margin-bottom: 8px;
	}

	.reviews-section__rating {
		font-size: 15px;
		color: var(--muted);
	}

	.reviews-section__stars {
		color: var(--warning);
		letter-spacing: 2px;
	}

	.reviews {
		display: grid;
		grid-template-columns: repeat(3, 1fr);
		gap: 24px;
		margin-top: 28px;
	}

	.review {
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 16px;
		padding: 24px;
		display: flex;
		flex-direction: column;
		gap: 14px;
	}

	.review__stars {
		color: var(--warning);
		letter-spacing: 2px;
		font-size: 15px;
		margin: 0;
	}

	.review__text {
		font-size: 14px;
		line-height: 1.6;
	}

	.review__footer {
		margin-top: auto;
		display: flex;
		align-items: center;
		gap: 10px;
	}

	.review__avatar {
		width: 36px;
		height: 36px;
		border-radius: 50%;
		background: var(--accent-soft);
		color: var(--accent);
		display: inline-flex;
		align-items: center;
		justify-content: center;
		font-size: 12px;
		font-weight: 600;
	}

	.review__meta {
		display: flex;
		flex-direction: column;
	}

	.review__author {
		font-size: 13px;
		font-weight: 500;
	}

	.review__date {
		font-size: 11px;
		color: var(--muted);
	}

	/* ---------- Map ---------- */
	.map-section__title {
		margin-bottom: 24px;
	}

	.map-section__grid {
		display: grid;
		grid-template-columns: 1.6fr 1fr;
		gap: 24px;
	}

	.map-frame {
		position: relative;
		border-radius: 16px;
		overflow: hidden;
		border: 1px solid var(--border);
		min-height: 360px;
	}

	.map-frame iframe {
		width: 100%;
		height: 100%;
		min-height: 360px;
		border: 0;
	}

	.map-frame__fallback {
		position: absolute;
		left: 16px;
		bottom: 16px;
		background: var(--surface);
		border-radius: 12px;
		box-shadow: var(--shadow-card);
		padding: 10px 16px;
		font-size: 13px;
		font-weight: 500;
		color: var(--accent);
	}

	.hours-card {
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 16px;
		padding: 28px;
		display: flex;
		flex-direction: column;
		gap: 10px;
	}

	.hours-card__label {
		font-size: 17px;
		font-weight: 600;
	}

	.hours-card__status {
		font-size: 14px;
	}

	.hours-card__status--open {
		color: var(--success);
	}

	.hours-card__status--closed {
		color: var(--warning);
	}

	.hours-card__address {
		font-style: normal;
		font-size: 14px;
	}

	.hours-card__metro {
		font-size: 13px;
		color: var(--muted);
	}

	.hours-card__phone {
		font-size: 16px;
		font-weight: 500;
		margin-top: 8px;
	}

	/* ---------- CTA ---------- */
	.cta-section {
		padding-block: 40px;
	}

	.cta-section__inner {
		background: var(--accent);
		color: #ffffff;
		border-radius: 24px;
		padding: 56px 64px;
		display: flex;
		flex-direction: column;
		align-items: flex-start;
		gap: 12px;
	}

	.cta-section__title {
		font-size: clamp(28px, 3.4vw, 40px);
	}

	.cta-section__text {
		font-size: 16px;
		opacity: 0.9;
		margin-bottom: 16px;
	}

	.cta-section :global(.btn--primary) {
		background: #ffffff;
		color: var(--accent);
	}

	.cta-section :global(.btn--primary:hover) {
		background: var(--bg);
	}

	/* ---------- Responsive ---------- */
	@media (max-width: 1024px) {
		.steps {
			grid-template-columns: repeat(2, 1fr);
		}

		.reviews {
			grid-template-columns: 1fr;
		}

		.about__inner,
		.hero__inner,
		.map-section__grid {
			grid-template-columns: 1fr;
		}

		.hero__photo {
			display: none;
		}
	}

	@media (max-width: 480px) {
		.hero {
			padding-top: 24px;
		}

		.hero__actions {
			flex-direction: column;
			width: 100%;
		}

		.steps {
			grid-template-columns: 1fr;
		}

		.about__stats {
			gap: 24px;
			flex-wrap: wrap;
		}

		.cta-section__inner {
			padding: 32px 24px;
		}
	}
</style>
