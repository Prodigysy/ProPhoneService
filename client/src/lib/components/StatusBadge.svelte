<script lang="ts">
	import type { RepairStatus } from '../../services/api.ts';

	interface Props {
		status: RepairStatus | null;
	}

	let { status }: Props = $props();
	const statusKey = $derived(status ?? 'Unknown');

	const labels: Record<RepairStatus | 'Unknown', string> = {
		Created: 'Принята',
		InDiagnostic: 'Диагностика',
		Agreed: 'Согласована',
		InProgress: 'В ремонте',
		Ready: 'Готово',
		Completed: 'Выдана',
		Cancelled: 'Отменена',
		Unknown: 'Статус не задан'
	};

	const toneClass: Record<RepairStatus | 'Unknown', string> = {
		Created: 'badge--info',
		InDiagnostic: 'badge--warning',
		Agreed: 'badge--accent',
		InProgress: 'badge--accent',
		Ready: 'badge--success',
		Completed: 'badge--success',
		Cancelled: 'badge--muted',
		Unknown: 'badge--muted'
	};
</script>

<span class="badge {toneClass[statusKey]}">
	<span class="badge__dot" aria-hidden="true"></span>
	{labels[statusKey]}
</span>

<style>
	.badge {
		display: inline-flex;
		align-items: center;
		gap: 7px;
		font-size: 12px;
		font-weight: 500;
		padding: 5px 12px;
		border-radius: 999px;
		white-space: nowrap;
	}

	.badge__dot {
		width: 6px;
		height: 6px;
		border-radius: 50%;
		background: currentColor;
	}

	.badge--accent {
		color: var(--accent);
		background: var(--accent-soft);
	}

	.badge--success {
		color: var(--success);
		background: var(--success-soft);
	}

	.badge--warning {
		color: var(--warning);
		background: var(--warning-soft);
	}

	.badge--info {
		color: var(--info);
		background: var(--info-soft);
	}

	.badge--muted {
		color: var(--muted);
		background: var(--bg);
	}
</style>
