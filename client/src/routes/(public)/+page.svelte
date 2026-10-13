<script lang="ts">
	import Eyebrow from '#lib/components/Eyebrow.svelte';
	import Button from '#lib/components/Button.svelte';
	import { workshop, workshopStats, masterProfile, aboutText, steps, reviews, heroTrust, heroBadge, getWorkshopStatus } from '../../mocks/db.ts';

	const workshopStatus = getWorkshopStatus();
	const isOpen = workshopStatus.isOpen;
	const openStatus = workshopStatus.message;
</script>

<!-- ================= Hero ================= -->
<section class="hero">
	<div class="container hero__inner">
		<div class="hero__content">
			<Eyebrow dot tone="muted">Ремонт телефонов на Покровке</Eyebrow>
			<h1 class="hero__title">Телефон снова в деле.<br /><span class="hero__title-accent">Вы — на связи.</span></h1>
			<p class="hero__subtitle">
				Сломался телефон? Оставьте заявку без звонка. Узнайте цену заранее и отслеживайте ремонт онлайн.
			</p>
			<div class="hero__actions">
				<Button href="/request" size="lg">Оставить заявку ↗</Button>
				<Button href="/price" variant="secondary" size="lg">Посмотреть цены</Button>
			</div>
			<ul class="hero__trust">
				{#each heroTrust as item (item.title)}
					<li class="trust-item">
						<span class="trust-item__check" aria-hidden="true">
							<svg width="16" height="16" viewBox="0 0 24 24" fill="none">
								<path
									d="m5 12.5 4.5 4.5L19 7.5"
									stroke="currentColor"
									stroke-width="2.4"
									stroke-linecap="round"
									stroke-linejoin="round"
								/>
							</svg>
						</span>
						{item.title}
					</li>
				{/each}
			</ul>
		</div>
		<div class="hero__photo">
			<img src="/hero.jpg" alt="Мастер ремонтирует телефон в мастерской" />
			<div class="hero__badge">
				<span class="hero__badge-icon" aria-hidden="true">
					<svg width="20" height="20" viewBox="0 0 24 24" fill="none">
						<path
							d="M12 3 5 5.5V12c0 4.4 3 7.4 7 9 4-1.6 7-4.6 7-9V5.5L12 3Z"
							stroke="currentColor"
							stroke-width="1.8"
							stroke-linejoin="round"
						/>
						<path d="m9 12 2.2 2.2L15.5 9.7" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" />
					</svg>
				</span>
				<div class="hero__badge-text">
					<p class="hero__badge-title">{heroBadge.title}</p>
					<p class="hero__badge-subtitle">{heroBadge.subtitle}</p>
				</div>
			</div>
		</div>
	</div>

	<div class="container hero__location">
		<div class="strip-card">
			<div class="location">
				<span class="location__pin" aria-hidden="true">
					<svg width="18" height="18" viewBox="0 0 24 24" fill="none">
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
					<svg width="18" height="18" viewBox="0 0 24 24" fill="none">
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
			<a class="location__link" href="https://yandex.ru/maps/?text={encodeURIComponent(workshop.address)}" target="_blank" rel="noreferrer">
				Как нас найти ↗
			</a>
		</div>
	</div>
</section>

<!-- ================= About ================= -->
<section class="section about">
	<div class="container about__inner">
		<div class="about__text">
			<Eyebrow>О мастерской</Eyebrow>
			<h2 class="h2">Не просто техника.<br />Ваши планы, фото и связь.</h2>
			<p class="about__paragraph">{aboutText}</p>
		</div>
		<div class="about__side">
			<dl class="about__stats">
				<div class="stat">
					<dd class="stat__value">{workshopStats.years}</dd>
					<dt class="stat__label">{workshopStats.yearsLabel}</dt>
				</div>
				<div class="stat">
					<dd class="stat__value">{workshopStats.saved}</dd>
					<dt class="stat__label">{workshopStats.savedLabel}</dt>
				</div>
				<div class="stat">
					<dd class="stat__value">{workshopStats.rating}</dd>
					<dt class="stat__label">{workshopStats.ratingLabel}</dt>
				</div>
			</dl>
			<div class="about__master">
				<span class="about__master-avatar" aria-hidden="true">{masterProfile.initials}</span>
				<div class="about__master-info">
					<p class="about__master-name">{masterProfile.name}</p>
					<p class="about__master-bio">{masterProfile.role}. {masterProfile.promise}</p>
				</div>
			</div>
		</div>
	</div>
</section>

<!-- ================= Steps ================= -->
<section class="section steps-section">
	<div class="container">
		<div class="steps-head">
			<h2 class="h2">Как это работает</h2>
			<p class="steps-head__note">Четыре шага — и снова на связи</p>
		</div>
		<ol class="steps">
			{#each steps as step (step.num)}
				<li class="step">
					<div class="step__top">
						<span class="step__num">{step.num}</span>
						<!-- eslint-disable-next-line svelte/no-at-html-tags -- static inline SVG constants from src/mocks/db.ts, not user input -->
						<span class="step__icon" aria-hidden="true">{@html step.icon}</span>
					</div>
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
		<div class="reviews-head">
			<h2 class="h2">Нас советуют своим</h2>
			<p class="reviews-head__rating">
				<span class="reviews-head__stars" aria-hidden="true">★</span>
				{workshopStats.rating.replace(' / 5', '')} · {workshopStats.reviewsCount} отзывов на Яндекс ↗
			</p>
		</div>
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

<!-- ================= CTA ================= -->
<section class="section cta-section">
	<div class="container cta-section__inner">
		<div class="cta-section__copy">
			<h2 class="cta-section__title">Починим телефон. Освободим ваш день.</h2>
			<p class="cta-section__text">Оставьте заявку — всё остальное возьмём на себя.</p>
		</div>
		<Button href="/request" size="lg">Оставить заявку ↗</Button>
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
		font-size: clamp(34px, 4.6vw, 62px);
		font-weight: 800;
		line-height: 1.12;
		letter-spacing: -0.03em;
		margin: 16px 0 20px;
		max-width: 620px;
	}

	.hero__title-accent {
		color: var(--accent);
	}

	.hero__subtitle {
		font-size: 17px;
		line-height: 1.55;
		color: var(--muted);
		max-width: 440px;
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
		display: inline-flex;
		align-items: center;
		gap: 8px;
		font-size: 14px;
		font-weight: 500;
	}

	.trust-item__check {
		color: var(--success);
		display: inline-flex;
	}

	.hero__photo {
		position: relative;
	}

	.hero__photo img {
		width: 100%;
		height: 100%;
		object-fit: cover;
		border-radius: 20px;
	}

	.hero__badge {
		position: absolute;
		left: -12px;
		bottom: 24px;
		display: flex;
		align-items: center;
		gap: 12px;
		background: var(--surface);
		border-radius: 14px;
		box-shadow: var(--shadow-card);
		padding: 14px 18px;
		max-width: 320px;
	}

	.hero__badge-icon {
		width: 40px;
		height: 40px;
		border-radius: 10px;
		background: var(--accent-soft);
		color: var(--accent);
		display: inline-flex;
		align-items: center;
		justify-content: center;
		flex-shrink: 0;
	}

	.hero__badge-title {
		font-size: 14px;
		font-weight: 600;
	}

	.hero__badge-subtitle {
		font-size: 12px;
		color: var(--muted);
		margin-top: 2px;
	}

	.hero__location {
		display: grid;
		grid-template-columns: 1fr 1fr auto;
		gap: 32px;
		align-items: center;
		padding-block: 0 48px;
	}

	.strip-card {
		grid-column: 1 / -1;
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 16px;
		padding: 20px 28px;
		display: grid;
		grid-template-columns: 1fr 1fr auto;
		gap: 24px;
		align-items: center;
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
		grid-template-columns: 1.15fr 1fr;
		gap: 64px;
		align-items: center;
	}

	.about__paragraph {
		margin-top: 20px;
		font-size: 15px;
		line-height: 1.7;
		color: var(--muted);
		max-width: 520px;
	}

	.about__stats {
		display: grid;
		grid-template-columns: repeat(3, 1fr);
		gap: 24px;
		border-bottom: 1px solid var(--border);
		padding-bottom: 28px;
	}

	.stat__value {
		font-size: 34px;
		font-weight: 700;
		letter-spacing: -0.02em;
		margin: 0;
		white-space: nowrap;
	}

	.stat__label {
		font-size: 13px;
		color: var(--muted);
		margin-top: 6px;
	}

	.about__master {
		display: flex;
		align-items: center;
		gap: 16px;
		padding-top: 28px;
	}

	.about__master-avatar {
		width: 48px;
		height: 48px;
		border-radius: 50%;
		background: var(--accent);
		color: #ffffff;
		display: inline-flex;
		align-items: center;
		justify-content: center;
		font-size: 17px;
		font-weight: 600;
		flex-shrink: 0;
	}

	.about__master-name {
		font-size: 16px;
		font-weight: 600;
	}

	.about__master-bio {
		font-size: 14px;
		color: var(--muted);
		margin-top: 4px;
	}

	/* ---------- Steps ---------- */
	.steps-section {
		background: var(--bg-muted, var(--bg));
	}

	.steps-head {
		display: flex;
		align-items: baseline;
		justify-content: space-between;
		gap: 24px;
		margin-bottom: 36px;
	}

	.steps-head__note {
		font-size: 14px;
		color: var(--muted);
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

	.step__top {
		display: flex;
		align-items: center;
		justify-content: space-between;
	}

	.step__num {
		font-size: 15px;
		font-weight: 600;
		color: var(--accent);
	}

	.step__icon {
		color: var(--accent);
		display: inline-flex;
	}

	.step__title {
		font-size: 18px;
		font-weight: 600;
		margin-top: 16px;
	}

	.step__text {
		font-size: 14px;
		line-height: 1.6;
		color: var(--muted);
		margin-top: 10px;
	}

	/* ---------- Reviews ---------- */
	.reviews-section {
		background: var(--bg-muted, var(--bg));
	}

	.reviews-head {
		display: flex;
		align-items: baseline;
		justify-content: space-between;
		gap: 24px;
		margin-bottom: 32px;
	}

	.reviews-head__rating {
		font-size: 14px;
		font-weight: 500;
		color: var(--accent);
	}

	.reviews-head__stars {
		color: var(--accent);
	}

	.reviews {
		display: grid;
		grid-template-columns: repeat(3, 1fr);
		gap: 24px;
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
		color: #d97706;
		letter-spacing: 3px;
		font-size: 15px;
		margin: 0;
	}

	.review__text {
		font-size: 14px;
		line-height: 1.65;
	}

	.review__footer {
		margin-top: auto;
		display: flex;
		align-items: center;
		gap: 12px;
	}

	.review__avatar {
		width: 40px;
		height: 40px;
		border-radius: 50%;
		background: var(--bg-muted, var(--bg));
		color: var(--muted);
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

	/* ---------- CTA ---------- */
	.cta-section {
		padding-block: 40px 72px;
	}

	.cta-section__inner {
		background: var(--accent);
		color: #ffffff;
		border-radius: 24px;
		padding: 48px 56px;
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 32px;
	}

	.cta-section__title {
		font-size: clamp(26px, 3vw, 38px);
		font-weight: 700;
		letter-spacing: -0.02em;
	}

	.cta-section__text {
		font-size: 15px;
		opacity: 0.85;
		margin-top: 10px;
	}

	.cta-section :global(.btn--primary) {
		background: #ffffff;
		color: var(--accent);
		flex-shrink: 0;
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
		.hero__inner {
			grid-template-columns: 1fr;
		}

		.hero__photo {
			display: none;
		}

		.strip-card {
			grid-template-columns: 1fr;
			gap: 18px;
		}

		.steps-head,
		.reviews-head {
			flex-direction: column;
			align-items: flex-start;
			gap: 8px;
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

		.cta-section__inner {
			padding: 32px 24px;
			flex-direction: column;
			align-items: flex-start;
		}

		.hero__title br {
			display: none;
		}
	}
</style>
