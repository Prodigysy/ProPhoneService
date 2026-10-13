<script lang="ts">
	interface Props {
		children?: import('svelte').Snippet;
		/** Green marker dot, as on the hero eyebrow in the mockup. */
		dot?: boolean;
		/** muted = gray label (hero), accent = blue label (sections). */
		tone?: 'accent' | 'muted';
	}
	let { children, dot = false, tone = 'accent' }: Props = $props();
</script>

<p class="eyebrow" class:eyebrow--muted={tone === 'muted'}>
	{#if dot}<span class="eyebrow__dot" aria-hidden="true"></span>{/if}{@render children?.()}
</p>

<style>
	.eyebrow {
		display: inline-flex;
		align-items: center;
		gap: 8px;
		font-size: 12px;
		font-weight: 600;
		letter-spacing: 0.08em;
		text-transform: uppercase;
		color: var(--accent);
		margin: 0 0 14px;
	}

	.eyebrow--muted {
		color: var(--muted);
	}

	.eyebrow__dot {
		width: 6px;
		height: 6px;
		border-radius: 50%;
		background: var(--success);
	}
</style>
