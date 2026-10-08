<script lang="ts">
	interface Props {
		children?: import('svelte').Snippet;
	}
	let { children }: Props = $props();
</script>

<p class="eyebrow">{@render children?.()}</p>

<style>
	.eyebrow {
		font-size: 12px;
		font-weight: 600;
		letter-spacing: 0.08em;
		text-transform: uppercase;
		color: var(--muted);
		margin: 0 0 14px;
	}
</style>
