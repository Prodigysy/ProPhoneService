<script lang="ts">
	import { showToast } from '#lib/components/toast.svelte.ts';
	import {
		fetchMasterServices,
		createService,
		updateService,
		hideService,
		type ServiceDto
	} from '../../../../services/api.ts';

	let services = $state<ServiceDto[]>([]);
	let loading = $state(true);

	// Editing state: null = list view, 'new' = create, id = edit that service.
	let editing = $state<'new' | string | null>(null);
	let formName = $state('');
	let formPrice = $state('');
	let formDescription = $state('');
	let formError = $state('');
	let busy = $state(false);
	let hidingId = $state<string | null>(null);

	const priceFormat = new Intl.NumberFormat('ru-RU');

	(window as unknown as { __svcMount?: string }).__svcMount = `mounted:${Date.now()}`;

	$effect(() => {
		reload();
	});

	async function reload(): Promise<void> {
		try {
			const list = await fetchMasterServices();
			services = list;
		} catch {
			showToast('Не удалось загрузить прайс. Попробуйте обновить страницу.', 'error');
		} finally {
			loading = false;
		}
	}

	function openCreate(): void {
		editing = 'new';
		formName = '';
		formPrice = '';
		formDescription = '';
		formError = '';
	}

	function openEdit(service: ServiceDto): void {
		editing = service.id;
		formName = service.name;
		formPrice = String(service.price);
		formDescription = service.description ?? '';
		formError = '';
	}

	function closeForm(): void {
		editing = null;
		formError = '';
	}

	function validate(): boolean {
		if (!formName.trim()) {
			formError = 'Укажите название услуги.';
			return false;
		}
		// Svelte 5 bind:value on a number input yields a number, not a string.
		const price = Number(String(formPrice).replace(/,/g, '.'));
		if (!Number.isFinite(price) || price < 0) {
			formError = 'Цена — неотрицательное число.';
			return false;
		}
		return true;
	}

	async function saveService(): Promise<void> {
		if (busy || !validate()) return;
		busy = true;
		const input = {
			name: formName.trim(),
			price: Number(String(formPrice).replace(/,/g, '.')),
			description: formDescription.trim() || null
		};
		try {
			if (editing === 'new') {
				await createService(input);
				showToast('Услуга добавлена');
			} else if (editing) {
				await updateService(editing, input);
				showToast('Услуга обновлена');
			}
			editing = null;
			await reload();
		} catch (error) {
			showToast(error instanceof Error ? error.message : 'Не удалось сохранить услугу', 'error');
		} finally {
			busy = false;
		}
	}

	async function hide(service: ServiceDto): Promise<void> {
		if (hidingId) return;
		hidingId = service.id;
		try {
			await hideService(service.id);
			showToast(`«${service.name}» скрыта из прайса`);
			await reload();
		} catch (error) {
			showToast(error instanceof Error ? error.message : 'Не удалось скрыть услугу', 'error');
		} finally {
			hidingId = null;
		}
	}
</script>

<div class="services">
	<header class="services__head">
		<div>
			<h1 class="services__title">Прайс-лист</h1>
			<p class="services__note">Добавляйте, правьте и скрывайте услуги — сайт обновится сразу после сохранения.</p>
		</div>
		<button class="services__add" type="button" onclick={openCreate}>+ Добавить услугу</button>
	</header>

	<div class="editor" class:editor--open={editing !== null}>
		<h2 class="editor__title">{editing === 'new' ? 'Новая услуга' : 'Правка услуги'}</h2>
		<div class="editor__grid">
			<label class="field">
				<span class="label">Название</span>
				<input class="input" type="text" placeholder="Замена дисплейного модуля" bind:value={formName} />
			</label>
			<label class="field">
				<span class="label">Цена, ₽</span>
				<input class="input" type="number" min="0" step="100" placeholder="1500" bind:value={formPrice} />
			</label>
			<label class="field field--wide">
				<span class="label">Описание (что ремонтируем)</span>
				<input class="input" type="text" placeholder="Изображение, сенсор и стекло" bind:value={formDescription} />
			</label>
		</div>
		{#if formError}
			<p class="form-error" role="alert">{formError}</p>
		{/if}
		<div class="editor__actions">
			<button class="editor__cancel" type="button" onclick={closeForm}>Отмена</button>
			<button class="editor__save" type="button" onclick={() => saveService()}>
				{busy ? 'Сохраняем…' : 'Сохранить'}
			</button>
		</div>
	</div>

	{#if loading}
		<p class="state">Загружаем услуги…</p>
	{:else}
		<section class="table" aria-label="Услуги и цены">
			<div class="table__head">
				<span>Услуга</span><span>Цена</span><span>Описание</span><span>Статус</span><span class="table__head-ops">Правки</span>
			</div>
			{#each services as service (service.id)}
				<div class="table__row" class:table__row--hidden={!service.isActive}>
					<span class="table__name">{service.name}</span>
					<span class="table__price">
						{service.price === 0 ? '0 ₽' : `${priceFormat.format(service.price)} ₽`}
					</span>
					<span class="table__desc">{service.description ?? '—'}</span>
					<span class="table__status">
						{#if service.isActive}
							<span class="tag tag--active">Активна</span>
						{:else}
							<span class="tag tag--hidden">Скрыта</span>
						{/if}
					</span>
					<span class="table__ops">
						<button class="op" type="button" onclick={() => openEdit(service)}>Править</button>
						{#if service.isActive}
							<button class="op op--danger" type="button" disabled={hidingId !== null} onclick={() => hide(service)}>
								{hidingId === service.id ? 'Скрываем…' : 'Скрыть'}
							</button>
						{/if}
					</span>
				</div>
			{/each}
		</section>
	{/if}
</div>

<style>
	.services__head {
		display: flex;
		align-items: flex-start;
		justify-content: space-between;
		gap: 24px;
		margin-bottom: 24px;
	}

	.services__title {
		font-size: 26px;
		font-weight: 700;
		letter-spacing: -0.02em;
	}

	.services__note {
		margin-top: 6px;
		font-size: 14px;
		color: var(--muted);
	}

	.editor {
		display: none;
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 16px;
		padding: 24px;
		margin-bottom: 24px;
	}

	.editor--open {
		display: block;
	}

	.services__add {
		font-size: 14px;
		font-weight: 500;
		background: var(--accent);
		color: #ffffff;
		border-radius: 12px;
		padding: 12px 20px;
		white-space: nowrap;
	}

	.services__add:hover {
		background: var(--accent-hover);
	}

	.editor__title {
		font-size: 17px;
		font-weight: 600;
		margin-bottom: 16px;
	}

	.editor__grid {
		display: grid;
		grid-template-columns: 2fr 1fr 2fr;
		gap: 16px;
	}

	.field {
		display: flex;
		flex-direction: column;
		gap: 6px;
	}

	.field--wide {
		grid-column: 1 / -1;
	}

	.label {
		font-size: 13px;
		font-weight: 500;
	}

	.input {
		background: var(--bg);
		border: 1px solid var(--border);
		border-radius: 10px;
		padding: 11px 14px;
		font-size: 14px;
	}

	.input:focus {
		border-color: var(--accent);
		outline: none;
	}

	.form-error {
		margin-top: 12px;
		font-size: 13px;
		color: var(--danger);
	}

	.editor__actions {
		margin-top: 18px;
		display: flex;
		align-items: center;
		gap: 14px;
	}

	.editor__save {
		font-size: 14px;
		font-weight: 500;
		background: var(--accent);
		color: #ffffff;
		border-radius: 12px;
		padding: 12px 22px;
	}

	.editor__save:hover {
		background: var(--accent-hover);
	}

	.editor__save:disabled {
		opacity: 0.55;
		cursor: not-allowed;
	}

	.editor__cancel {
		font-size: 13px;
		color: var(--muted);
	}

	.editor__cancel:hover {
		color: var(--text);
	}

	.state {
		color: var(--muted);
		font-size: 14px;
	}

	.table {
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 16px;
		overflow: hidden;
	}

	.table__head,
	.table__row {
		display: grid;
		grid-template-columns: 2.2fr 0.9fr 2fr 1fr 1.2fr;
		gap: 16px;
		align-items: center;
		padding: 13px 22px;
	}

	.table__head {
		font-size: 11px;
		font-weight: 600;
		letter-spacing: 0.06em;
		text-transform: uppercase;
		color: var(--muted);
		background: var(--bg);
	}

	.table__head-ops {
		text-align: right;
	}

	.table__row + .table__row {
		border-top: 1px solid var(--border);
	}

	.table__row--hidden .table__name,
	.table__row--hidden .table__desc,
	.table__row--hidden .table__price {
		opacity: 0.45;
	}

	.table__name {
		font-size: 14px;
		font-weight: 500;
	}

	.table__price {
		font-size: 14px;
		font-weight: 600;
	}

	.table__desc {
		font-size: 13px;
		color: var(--muted);
	}

	.tag {
		font-size: 12px;
		font-weight: 500;
		border-radius: 999px;
		padding: 4px 10px;
	}

	.tag--active {
		background: var(--success-soft);
		color: var(--success);
	}

	.tag--hidden {
		background: var(--bg);
		color: var(--muted);
	}

	.table__ops {
		display: flex;
		gap: 8px;
		justify-content: flex-end;
	}

	.op {
		font-size: 13px;
		font-weight: 500;
		color: var(--accent);
		padding: 6px 12px;
		border-radius: 8px;
	}

	.op:hover {
		background: var(--accent-soft);
	}

	.op--danger {
		color: var(--danger);
	}

	.op--danger:hover {
		background: var(--danger-soft);
	}

	@media (max-width: 1024px) {
		.editor__grid {
			grid-template-columns: 1fr;
		}

		.table__head,
		.table__row {
			grid-template-columns: 2fr 1fr 1.4fr;
		}

		.table__desc,
		.table__head > span:nth-child(3) {
			display: none;
		}

		.table__ops {
			grid-column: 2 / -1;
			justify-content: flex-start;
			margin-top: 8px;
		}

		.table__head,
		.table__row {
			grid-template-areas:
				'name status'
				'ops ops';
			grid-template-columns: 1fr auto;
		}

		.table__name {
			grid-area: name;
		}

		.table__status {
			grid-area: status;
		}

		.table__ops {
			grid-area: ops;
		}

		.table__price {
			display: none;
		}
	}
</style>
