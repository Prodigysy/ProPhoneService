<script lang="ts">
	interface Props {
		variant?: 'primary' | 'secondary' | 'ghost';
		href?: string;
		type?: 'button' | 'submit';
		size?: 'md' | 'lg';
		disabled?: boolean;
		onclick?: (e: MouseEvent) => void;
		children?: import('svelte').Snippet;
	}

	let {
		variant = 'primary',
		href,
		type = 'button',
		size = 'md',
		disabled = false,
		onclick,
		children
	}: Props = $props();
</script>

{#if href}
	<a class="btn btn--{variant} btn--{size}" {href} role="button" aria-disabled={disabled}>
		{@render children?.()}
	</a>
{:else}
	<button class="btn btn--{variant} btn--{size}" {type} {disabled} {onclick}>
		{@render children?.()}
	</button>
{/if}

<style>
	.btn {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		gap: 8px;
		font-weight: 500;
		border-radius: 12px;
		cursor: pointer;
		text-decoration: none;
		white-space: nowrap;
		transition:
			background-color 0.15s ease,
			border-color 0.15s ease,
			color 0.15s ease,
			opacity 0.15s ease;
	}

	.btn--md {
		font-size: 14px;
		padding: 12px 20px;
	}

	.btn--lg {
		font-size: 16px;
		padding: 16px 28px;
	}

	.btn--primary {
		background: var(--accent);
		color: #ffffff;
	}

	.btn--primary:hover {
		background: var(--accent-hover);
	}

	.btn--secondary {
		background: var(--surface);
		color: var(--text);
		border: 1px solid var(--border);
	}

	.btn--secondary:hover {
		border-color: var(--accent);
		color: var(--accent);
	}

	.btn--ghost {
		background: transparent;
		color: var(--accent);
	}

	.btn--ghost:hover {
		background: var(--accent-soft);
	}

	.btn:disabled,
	.btn[aria-disabled='true'] {
		opacity: 0.55;
		cursor: not-allowed;
		pointer-events: none;
	}

	@media (max-width: 480px) {
		.btn--lg {
			width: 100%;
		}
	}
</style>
