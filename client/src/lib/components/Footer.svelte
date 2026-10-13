<script lang="ts">
	import { workshop } from '../../mocks/db.ts';
	import { showToast } from './toast.svelte.ts';
</script>

<footer class="footer">
	<div class="container footer__inner">
		<div class="footer__brand">
			<span class="footer__logo" aria-hidden="true">
				<svg width="16" height="16" viewBox="0 0 24 24" fill="none">
					<rect x="7" y="3" width="10" height="18" rx="2.5" stroke="currentColor" stroke-width="2" />
					<path d="M11 18h2" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
				</svg>
			</span>
			<span class="footer__logo-name">про телефон</span>
		</div>

		<div class="footer__place">
			<address class="footer__address">{workshop.address}</address>
			<p class="footer__hours">{workshop.hours}</p>
		</div>

		<div class="footer__contacts">
			<a class="footer__phone" href="tel:+74951204567">{workshop.phone}</a>
			<a class="footer__telegram" href={workshop.telegramUrl} target="_blank" rel="noreferrer">
				Написать в Telegram ↗
			</a>
		</div>
	</div>

	<div class="container footer__bottom">
		<p class="footer__copyright">© 2026 «Про телефон». Возвращаем связь с важным.</p>
		<button
			class="footer__policy"
			type="button"
			onclick={() => showToast('Политика конфиденциальности появится позже')}
		>
			Политика конфиденциальности
		</button>
	</div>
</footer>

<style>
	.footer {
		border-top: 1px solid var(--border);
		background: var(--surface);
	}

	.footer__inner {
		display: grid;
		grid-template-columns: 1fr 1fr 1fr;
		gap: 40px;
		align-items: start;
		padding-block: 44px 36px;
	}

	.footer__brand {
		display: flex;
		align-items: center;
		gap: 10px;
	}

	.footer__logo {
		width: 34px;
		height: 34px;
		border-radius: 10px;
		background: var(--accent);
		color: #ffffff;
		display: inline-flex;
		align-items: center;
		justify-content: center;
	}

	.footer__logo-name {
		font-size: 17px;
		font-weight: 700;
		letter-spacing: -0.02em;
	}

	.footer__place {
		display: flex;
		flex-direction: column;
		gap: 6px;
	}

	.footer__address {
		font-style: normal;
		font-size: 14px;
		font-weight: 500;
	}

	.footer__hours {
		font-size: 13px;
		color: var(--muted);
	}

	.footer__contacts {
		display: flex;
		flex-direction: column;
		align-items: flex-start;
		gap: 8px;
	}

	.footer__phone {
		font-size: 17px;
		font-weight: 600;
	}

	.footer__telegram {
		font-size: 13px;
		font-weight: 500;
		color: var(--accent);
	}

	.footer__bottom {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 24px;
		padding-block: 18px 24px;
		border-top: 1px solid var(--border);
	}

	.footer__copyright {
		font-size: 12px;
		color: var(--muted);
	}

	.footer__policy {
		font-size: 12px;
		color: var(--muted);
	}

	.footer__policy:hover {
		color: var(--text);
	}

	@media (max-width: 768px) {
		.footer__inner {
			grid-template-columns: 1fr;
			gap: 24px;
			padding-block: 32px 24px;
		}

		.footer__bottom {
			flex-direction: column;
			align-items: flex-start;
			gap: 10px;
		}
	}
</style>
