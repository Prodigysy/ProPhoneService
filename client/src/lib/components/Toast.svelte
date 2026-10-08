<script lang="ts">
	import { toastList } from './toast.svelte.ts';
</script>

<div class="toast-region" aria-live="polite">
	{#each toastList.value as toast (toast.id)}
		<div class="toast" class:toast--error={toast.kind === 'error'} role="status">
			{toast.text}
		</div>
	{/each}
</div>

<style>
	.toast-region {
		position: fixed;
		bottom: 24px;
		left: 50%;
		transform: translateX(-50%);
		z-index: 1000;
		display: flex;
		flex-direction: column;
		gap: 8px;
		align-items: center;
		pointer-events: none;
	}

	.toast {
		background: var(--surface);
		color: var(--text);
		border: 1px solid var(--border);
		border-radius: 12px;
		box-shadow: var(--shadow-pop);
		padding: 12px 20px;
		font-size: 14px;
		max-width: min(90vw, 420px);
		animation: toast-in 0.18s ease-out;
	}

	.toast--error {
		border-color: var(--danger);
		color: var(--danger);
	}

	@keyframes toast-in {
		from {
			opacity: 0;
			transform: translateY(8px);
		}
		to {
			opacity: 1;
			transform: translateY(0);
		}
	}
</style>
