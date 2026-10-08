<script lang="ts">
	import { workshop } from '../../mocks/db.ts';
	import { showToast } from './toast.svelte.ts';
</script>

<footer class="footer">
	<div class="container footer__inner">
		<div class="footer__brand">
			<span class="footer__logo"><span class="footer__logo-mark">про</span>телефон</span>
			<p class="footer__tagline">Возвращаем связь с важным.</p>
			<p class="footer__copyright">© 2026 «про телефон».</p>
			<button
				class="footer__policy"
				type="button"
				onclick={() => showToast('Политика конфиденциальности появится позже')}
			>
				Политика конфиденциальности
			</button>
		</div>

		<div class="footer__contacts">
			<address class="footer__address">{workshop.address}</address>
			<p class="footer__hours">{workshop.hours}</p>
			<a class="footer__phone" href="tel:+74951204567">{workshop.phone}</a>
			<a class="footer__telegram" href={workshop.telegramUrl} target="_blank" rel="noreferrer">
				Написать в Telegram ↗
			</a>
		</div>

		<nav class="footer__nav" aria-label="Разделы">
			<a href="/">Главная</a>
			<a href="/price">Прайс-лист</a>
			<button type="button" aria-disabled="true" title="Скоро">Личный кабинет</button>
		</nav>
	</div>
</footer>

<style>
	.footer {
		border-top: 1px solid var(--border);
		background: var(--surface);
	}

	.footer__inner {
		display: grid;
		grid-template-columns: 1.4fr 1fr 0.7fr;
		gap: 40px;
		padding-block: 48px 40px;
	}

	.footer__logo {
		font-size: 19px;
		font-weight: 600;
		display: inline-flex;
		gap: 4px;
	}

	.footer__logo-mark {
		background: var(--accent);
		color: #ffffff;
		border-radius: 8px;
		padding: 0 6px;
	}

	.footer__tagline {
		margin-top: 10px;
		font-size: 15px;
	}

	.footer__copyright {
		margin-top: 18px;
		font-size: 12px;
		color: var(--muted);
	}

	.footer__policy {
		display: block;
		margin-top: 6px;
		font-size: 12px;
		color: var(--muted);
		text-decoration: underline;
		text-underline-offset: 3px;
	}

	.footer__policy:hover {
		color: var(--text);
	}

	.footer__contacts {
		display: flex;
		flex-direction: column;
		gap: 10px;
		font-style: normal;
	}

	.footer__address {
		font-size: 14px;
	}

	.footer__hours {
		font-size: 12px;
		color: var(--muted);
	}

	.footer__phone {
		font-size: 18px;
		font-weight: 500;
	}

	.footer__telegram {
		font-size: 12px;
		color: var(--accent);
	}

	.footer__nav {
		display: flex;
		flex-direction: column;
		gap: 10px;
		font-size: 14px;
	}

	.footer__nav a {
		color: var(--muted);
	}

	.footer__nav a:hover {
		color: var(--text);
	}

	.footer__nav button {
		text-align: left;
		color: var(--muted);
		cursor: not-allowed;
	}

	@media (max-width: 768px) {
		.footer__inner {
			grid-template-columns: 1fr;
			gap: 28px;
			padding-block: 32px;
		}
	}
</style>
