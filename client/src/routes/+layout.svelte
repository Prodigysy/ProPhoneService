<script lang="ts">
	import '../app.css';
	import { initSession } from '#lib/auth/session.svelte.ts';

	let { children } = $props();

	initSession();
</script>

{@render children()}
