<script lang="ts">
	import StatusBadge from './StatusBadge.svelte';
	import {
		statusTransitions,
		statusLabels,
		type RepairStatus
	} from '../../services/api.ts';

	interface Props {
		status: RepairStatus;
		onchange: (next: RepairStatus) => void;
		disabled?: boolean;
	}

	let { status, onchange, disabled = false }: Props = $props();

	const options = $derived(statusTransitions[status]);
</script>

<div class="status-select">
	{#if options.length > 0}
		<label class="status-select__label">
			<StatusBadge {status} />
			<svg class="status-select__chevron" width="10" height="6" viewBox="0 0 10 6" aria-hidden="true">
				<path d="M1 1l4 4 4-4" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" />
			</svg>
			<select
				class="status-select__native"
				aria-label="Сменить статус заявки"
				{disabled}
				onchange={(e) => {
					const value = (e.currentTarget as HTMLSelectElement).value as RepairStatus;
					if (value) onchange(value);
					e.currentTarget.value = '';
				}}
			>
				<option value=""></option>
				{#each options as next (next)}
					<option value={next}>{statusLabels[next]}</option>
				{/each}
			</select>
		</label>
	{:else}
		<StatusBadge {status} />
	{/if}
</div>
<style>
	.status-select {
		display: inline-flex;
		align-items: center;
	}

	.status-select__label {
		position: relative;
		display: inline-flex;
		align-items: center;
		gap: 10px;
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 10px;
		padding: 5px 12px;
		cursor: pointer;
		transition: border-color 0.15s ease;
	}

	.status-select__label:hover {
		border-color: var(--accent);
	}

	.status-select__chevron {
		color: var(--muted);
		flex-shrink: 0;
	}

	.status-select__native {
		/* Transparent layer over the badge: click anywhere opens the native menu */
		position: absolute;
		inset: 0;
		width: 100%;
		height: 100%;
		opacity: 0;
		cursor: pointer;
		border: none;
		appearance: none;
	}
</style>
