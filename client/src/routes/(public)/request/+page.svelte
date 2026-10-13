<script lang="ts">
	import { goto } from '$app/navigation';
	import Eyebrow from '#lib/components/Eyebrow.svelte';
	import Button from '#lib/components/Button.svelte';
	import { showToast } from '#lib/components/toast.svelte.ts';
	import { devices, problems, workshop, CUSTOM_DEVICE, workshopWallTimeToUtcIso } from '../../../mocks/db.ts';
	import {
		ApiError,
		createOrder,
		fetchServices,
		isMockMode,
		type CreateOrderPayload,
		type CreateOrderResult,
		type ServiceDto
	} from '../../../services/api.ts';

	// ── Form state ─────────────────────────────────────────────────────────
	let clientName = $state('');
	let deviceId = $state('');
	let customManufacturer = $state('');
	let customDevice = $state('');
	let problemId = $state('');
	let description = $state('');
	let visitTime = $state('');
	let phone = $state('');
	let visitDate = $state('');
	let services = $state<ServiceDto[]>([]);
	let servicesError = $state(false);
	let apiFieldErrors = $state<Record<string, string>>({});
	let touched = $state<Record<string, boolean>>({});
	let submitting = $state(false);

	const problem = $derived(problems.find((p) => p.id === problemId) ?? null);
	const apiService = $derived(services.find((service) => service.id === problemId) ?? null);
	const device = $derived(devices.find((d) => d.id === deviceId) ?? null);
	const deviceManufacturer = $derived(
		device ? device.brand : deviceId === CUSTOM_DEVICE ? customManufacturer.trim() : ''
	);
	const deviceModel = $derived(
		device ? device.model : deviceId === CUSTOM_DEVICE ? customDevice.trim() : ''
	);
	const deviceName = $derived([deviceManufacturer, deviceModel].filter(Boolean).join(' '));
	const problemLabel = $derived(isMockMode ? problem?.label ?? '' : apiService?.name ?? '');

	$effect(() => {
		if (isMockMode) return;
		fetchServices()
			.then((items) => {
				services = items;
				servicesError = false;
			})
			.catch(() => {
				servicesError = true;
				showToast('Не удалось загрузить список услуг. Проверьте подключение к API.', 'error');
			});
	});

	// ── Validation: mirrors the current backend WorkshopOptions ────────────
	const digitsOnly = $derived(phone.replace(/\D/g, ''));
	const phoneValid = $derived(/^\+7\d{10}$/.test(`+7${digitsOnly.slice(1)}`) && digitsOnly.length === 11);
	const descriptionValid = $derived(description.trim().length >= 10);
	const visitTimes = $derived(
		Array.from({ length: Number(workshop.closeAt.slice(0, 2)) - Number(workshop.openAt.slice(0, 2)) + 1 }, (_, index) =>
			`${String(Number(workshop.openAt.slice(0, 2)) + index).padStart(2, '0')}:00`
		)
	);
	const minimumVisitDate = $derived(
		new Intl.DateTimeFormat('en-CA', {
			timeZone: workshop.timeZone,
			year: 'numeric',
			month: '2-digit',
			day: '2-digit'
		}).format(new Date())
	);
	const currentWorkshopTime = $derived(
		new Intl.DateTimeFormat('en-GB', {
			timeZone: workshop.timeZone,
			hour: '2-digit',
			minute: '2-digit',
			hourCycle: 'h23'
		}).format(new Date())
	);
	const visitError = $derived((() => {
		if (!visitDate) return null;
		if (visitDate < minimumVisitDate || (visitDate === minimumVisitDate && visitTime && visitTime <= currentWorkshopTime)) {
			return 'Выбранное время визита уже прошло';
		}
		const weekday = new Date(`${visitDate}T12:00:00Z`).getUTCDay();
		const isoWeekday = weekday === 0 ? 7 : weekday;
		if (!workshop.workingDays.includes(isoWeekday)) return 'Мастерская в этот день закрыта (выходной)';
		return null;
	})());

	const formValid = $derived(
		clientName.trim().length > 0 &&
		deviceName.length > 0 &&
		(deviceId !== CUSTOM_DEVICE || deviceManufacturer.length > 0) &&
		problemId !== '' &&
		descriptionValid &&
		phoneValid && visitDate !== '' && visitTime !== '' && !visitError &&
		(isMockMode || (!servicesError && apiService !== null))
	);

	function validateField(name: string): string | null {
		switch (name) {
			case 'name':
				return clientName.trim().length > 0 ? null : 'Укажите ваше имя';
			case 'device':
				return deviceName.length === 0 || (deviceId === CUSTOM_DEVICE && !deviceManufacturer)
					? 'Укажите производителя и модель устройства'
					: null;
			case 'problem':
				return problemId === '' ? 'Выберите услугу' : null;
			case 'description':
				return descriptionValid ? null : 'Опишите проблему — минимум 10 символов';
			case 'phone':
				return phoneValid ? null : 'Введите телефон в формате +7 (XXX) XXX-XX-XX';
			case 'visit':
				if (!visitDate) return 'Выберите дату визита';
				if (!visitTime) return 'Выберите время визита';
				return visitError;
			default:
				return null;
		}
	}

	function fieldError(name: string): string | null {
		return touched[name] ? (apiFieldErrors[name] ?? validateField(name)) : null;
	}

	function markTouched(name: string): void {
		touched[name] = true;
		delete apiFieldErrors[name];
	}

	// ── Phone mask +7 (___) ___-__-__ ───────────────────────────────────────
	function formatPhone(raw: string): string {
		let d = raw.replace(/\D/g, '');
		if (d.startsWith('8')) d = `7${d.slice(1)}`;
		if (!d.startsWith('7')) d = `7${d}`;
		d = d.slice(0, 11);
		const p = d.slice(1);
		let out = '+7';
		if (p.length > 0) out += ` (${p.slice(0, 3)}`;
		if (p.length >= 3) out += ')';
		if (p.length > 3) out += ` ${p.slice(3, 6)}`;
		if (p.length > 6) out += `-${p.slice(6, 8)}`;
		if (p.length > 8) out += `-${p.slice(8, 10)}`;
		return out;
	}

	function onPhoneInput(event: Event): void {
		phone = formatPhone((event.target as HTMLInputElement).value);
	}

	// ── Estimate card ───────────────────────────────────────────────────────
	const partsPrice = $derived(isMockMode ? problem?.partsPrice ?? 0 : 0);
	const workPrice = $derived(isMockMode ? problem?.workPrice ?? 0 : apiService?.price ?? 0);
	const estimateTotal = $derived(partsPrice + workPrice);
	const selectedServiceIds = $derived(
		isMockMode ? (problem?.serviceId ? [problem.serviceId] : []) : (apiService ? [apiService.id] : [])
	);

	const priceFormat = new Intl.NumberFormat('ru-RU');

	async function submit(): Promise<void> {
		for (const field of ['name', 'device', 'problem', 'description', 'phone', 'visit']) touched[field] = true;
		if (!formValid || submitting) return;

		submitting = true;
		let result: CreateOrderResult;
		try {
			const payload: CreateOrderPayload = {
				name: clientName.trim(),
				phone: `+7${digitsOnly.slice(1)}`,
				email: null,
				deviceType: 'Phone',
				deviceManufacturer,
				deviceModel,
				preferredVisitAt: workshopWallTimeToUtcIso(visitDate, visitTime),
				serviceIds: selectedServiceIds,
				problemDescription: description.trim(),
				estimateTotal
			};
			result = await createOrder(payload);
		} catch (error) {
			const fieldMap: Record<string, string> = {
				Name: 'name',
				Phone: 'phone',
				DeviceType: 'device',
				DeviceManufacturer: 'device',
				DeviceModel: 'device',
				ServiceIds: 'problem',
				ProblemDescription: 'description',
				PreferredVisitAt: 'visit'
			};
			let mappedFieldError = false;
			if (error instanceof ApiError) {
				for (const [backendField, messages] of Object.entries(error.fieldErrors)) {
					const formField = fieldMap[backendField];
					if (!formField || !messages[0]) continue;
					apiFieldErrors[formField] = messages[0];
					touched[formField] = true;
					mappedFieldError = true;
				}
			}
			if (!mappedFieldError) showToast(error instanceof Error ? error.message : 'Не удалось отправить заявку. Попробуйте ещё раз.', 'error');
			submitting = false;
			return;
		}

		const query = new URLSearchParams({
			id: result.id,
			number: result.number,
			device: deviceName,
			problem: problemLabel,
			price: String(estimateTotal),
			date: visitDate,
			time: visitTime
		});
		await goto(`/request/success?${query}`);
	}
</script>

<section class="container section">
	<Eyebrow>Без звонков и лишних вопросов</Eyebrow>
	<h1 class="h2 heading">Давайте вернём телефон в строй</h1>
	<p class="lead">
		Расскажите, что случилось. Покажем ориентировочную цену и подготовимся к вашему визиту.
	</p>

	<div class="grid">
		<!-- ─────────── Left column: form ─────────── -->
		<form class="form" novalidate onsubmit={(e) => { e.preventDefault(); submit(); }}>
			<div class="form-head">
				<h2 class="form-title">Устройство и проблема</h2>
				<span class="required-note">Все поля обязательны</span>
			</div>

			<div class="field">
				<label class="label" for="clientName">Ваше имя</label>
				<input
					id="clientName"
					class="input"
					class:input--error={!!fieldError('name')}
					autocomplete="name"
					bind:value={clientName}
					onblur={() => markTouched('name')}
				/>
				{#if fieldError('name')}<span class="error" role="alert">{fieldError('name')}</span>{/if}
			</div>

			<div class="field">
				<label class="label" for="device">Модель устройства</label>
				<select
					id="device"
					class="input"
					class:input--error={!!fieldError('device')}
					bind:value={deviceId}
					onblur={() => markTouched('device')}
				>
					<option value="" disabled selected>Выберите модель</option>
					{#each devices as d (d.id)}
						<option value={d.id}>{d.brand} {d.model}</option>
					{/each}
					<option value={CUSTOM_DEVICE}>Нет модели в списке? Указать вручную</option>
				</select>
				{#if deviceId === CUSTOM_DEVICE}
					<label class="label" for="customManufacturer">Производитель</label>
					<input
						id="customManufacturer"
						class="input"
						class:input--error={!!fieldError('device')}
						autocomplete="organization"
						placeholder="Например: Google"
						bind:value={customManufacturer}
						onblur={() => markTouched('device')}
					/>
					<label class="label" for="customDevice">Модель устройства</label>
					<input
						id="customDevice"
						class="input"
						class:input--error={!!fieldError('device')}
						placeholder="Например: Pixel 8"
						bind:value={customDevice}
						onblur={() => markTouched('device')}
					/>
				{/if}
				{#if fieldError('device')}
					<span class="error" role="alert">{fieldError('device')}</span>
				{/if}
			</div>

			<div class="field">
				<label class="label" for="problem">Что случилось?</label>
				<select
					id="problem"
					class="input"
					class:input--error={!!fieldError('problem')}
					bind:value={problemId}
					onblur={() => markTouched('problem')}
					disabled={!isMockMode && services.length === 0}
				>
					<option value="" disabled selected>
						{servicesError ? 'Список услуг недоступен' : isMockMode || services.length ? 'Выберите проблему или услугу' : 'Загружаем услуги…'}
					</option>
					{#if isMockMode}
						{#each problems as p (p.id)}
							<option value={p.id}>{p.label}</option>
						{/each}
					{:else}
						{#each services as service (service.id)}
							<option value={service.id}>{service.name} · {service.price === 0 ? 'Бесплатно' : `${priceFormat.format(service.price)} ₽`}</option>
						{/each}
					{/if}
				</select>
				{#if fieldError('problem')}
					<span class="error" role="alert">{fieldError('problem')}</span>
				{/if}
				<p class="hint">{isMockMode ? 'Другие популярные проблемы' : 'Выберите услугу из действующего прайс-листа'}</p>
				<div class="chips">
					{#if isMockMode}
						{#each problems.filter((item) => item.id !== 'other') as item (item.id)}
							<button type="button" class="chip" class:chip--active={problemId === item.id} onclick={() => { problemId = item.id; touched.problem = true; }}>
								{item.label.split(' · ')[0]}
							</button>
						{/each}
					{:else}
						{#each services.slice(0, 5) as service (service.id)}
							<button type="button" class="chip" class:chip--active={problemId === service.id} onclick={() => { problemId = service.id; touched.problem = true; }}>
								{service.name}
							</button>
						{/each}
					{/if}
				</div>
			</div>

			<div class="field">
				<label class="label" for="description">Описание проблемы</label>
				<textarea
					id="description"
					class="input textarea"
					class:input--error={!!fieldError('description')}
					placeholder="Например: телефон быстро разряжается и иногда выключается на 20%."
					bind:value={description}
					onblur={() => markTouched('description')}
				></textarea>
				{#if fieldError('description')}
					<span class="error" role="alert">{fieldError('description')}</span>
				{/if}
			</div>

			<div class="field-row">
				<div class="field">
					<label class="label" for="visit">Дата визита</label>
					<input
						id="visit"
						class="input"
						class:input--error={!!fieldError('visit')}
						type="date"
						min={minimumVisitDate}
						bind:value={visitDate}
						onblur={() => markTouched('visit')}
					/>
					<span class="hint">Рабочие дни: {workshop.hours}.</span>
					{#if fieldError('visit')}
						<span class="error" role="alert">{fieldError('visit')}</span>
					{/if}
				</div>
				<div class="field">
					<label class="label" for="visitTime">Время визита</label>
					<select id="visitTime" class="input" bind:value={visitTime} onblur={() => markTouched('visit')}>
						<option value="" disabled selected>Выберите время</option>
						{#each visitTimes as time (time)}<option value={time}>{time}</option>{/each}
					</select>
				</div>
			</div>

			<div class="field">
				<label class="label" for="phone">Телефон для связи</label>
				<input
					id="phone"
					class="input"
					class:input--error={!!fieldError('phone')}
					type="tel"
					autocomplete="tel"
					placeholder="+7 (___) ___-__-__"
					value={phone}
					oninput={onPhoneInput}
					onblur={() => markTouched('phone')}
				/>
				<span class="hint">Только для связи по ремонту. Никаких рекламных сообщений.</span>
				{#if fieldError('phone')}
					<span class="error" role="alert">{fieldError('phone')}</span>
				{/if}
			</div>

			<Button type="submit" size="lg" disabled={!formValid || submitting}>
				{submitting ? 'Отправляем…' : 'Отправить заявку'}
			</Button>

			<p class="consent">
				Нажимая «Отправить заявку», вы соглашаетесь с обработкой персональных данных и
				<button type="button" class="consent-link" onclick={() => showToast('Политика конфиденциальности появится позже')}>политикой конфиденциальности</button>.
			</p>
		</form>

		<!-- ─────────── Right column: estimate ─────────── -->
		<aside class="estimate">
			<div class="estimate__card">
				<h2 class="estimate__title">Ориентировочная стоимость</h2>

				{#if deviceName}
					<p class="estimate__device">{deviceName}</p>
				{/if}
				{#if problemLabel}
					<p class="estimate__service">{problemLabel}</p>
				{/if}

				<dl class="estimate__lines">
					{#if isMockMode}
						<div>
							<dt>Запчасть</dt>
							<dd>{problem ? `${priceFormat.format(partsPrice)} ₽` : '—'}</dd>
						</div>
						<div>
							<dt>Работа мастера</dt>
							<dd>{problem ? `${priceFormat.format(workPrice)} ₽` : '—'}</dd>
						</div>
						<div><dt>Диагностика</dt><dd>Бесплатно</dd></div>
					{:else}
						<div>
							<dt>{apiService?.name ?? 'Услуга'}</dt>
							<dd>{apiService ? `${priceFormat.format(apiService.price)} ₽` : '—'}</dd>
						</div>
					{/if}
				</dl>

				<div class="estimate__total">
					<span class="estimate__total-label">Предварительная оценка</span>
					<span class="estimate__total-value">
						{problemLabel ? `${priceFormat.format(estimateTotal)} ₽` : '—'}
					</span>
				</div>
				<p class="estimate__duration">
					{isMockMode ? 'Обычно от 40 минут после диагностики' : 'Срок подтвердит мастер после диагностики'}
				</p>
			</div>

			<div class="estimate__note">
				<strong>Пока не подтверждённая цена.</strong>
				После диагностики мастер подтвердит стоимость. Начнём ремонт только после вашего согласия.
			</div>

			<div class="estimate__next">
				<h3>Что будет после отправки?</h3>
				<ol class="next-steps">
					<li>Вы получите номер заявки для отслеживания.</li>
					<li>Приносите телефон на {workshop.address.split(', ').slice(-1)[0]}.</li>
					<li>Мастер проведёт бесплатную диагностику.</li>
				</ol>
			</div>
		</aside>
	</div>
</section>

<style>
	.heading {
		margin-bottom: 12px;
	}

	.lead {
		color: var(--muted);
		max-width: 560px;
	}

	.grid {
		display: grid;
		grid-template-columns: 1.5fr 1fr;
		gap: 40px;
		margin-top: 40px;
		align-items: start;
	}

	.grid > :global(*) {
		/* let form/estimate shrink inside grid tracks on narrow screens */
		min-width: 0;
	}

	/* ── Form ── */
	.form-head {
		display: flex;
		align-items: baseline;
		justify-content: space-between;
		gap: 12px;
		margin-bottom: 20px;
	}

	.form-title {
		font-size: 20px;
		font-weight: 600;
	}

	.required-note {
		font-size: 12px;
		color: var(--muted);
	}

	.field {
		display: flex;
		flex-direction: column;
		gap: 6px;
		margin-bottom: 20px;
	}

	.field-row {
		display: grid;
		grid-template-columns: 1fr 1fr;
		gap: 16px;
	}

	.label {
		font-size: 14px;
		font-weight: 500;
	}

	.input {
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 10px;
		padding: 12px 14px;
		font-size: 15px;
		transition: border-color 0.15s ease;
	}

	.input:focus {
		border-color: var(--accent);
		outline: none;
	}

	.input--error {
		border-color: var(--danger);
	}


	select.input {
		appearance: none;
		background-image: linear-gradient(45deg, transparent 49%, var(--muted) 50%),
			linear-gradient(-45deg, transparent 49%, var(--muted) 50%);
		background-size: 6px 6px;
		background-position:
			right 18px center,
			right 13px center;
		background-repeat: no-repeat;
		padding-right: 36px;
	}

	.textarea {
		min-height: 110px;
		resize: vertical;
	}

	.hint {
		font-size: 12px;
		color: var(--muted);
	}

	.error {
		font-size: 12px;
		color: var(--danger);
	}

	.chips {
		display: flex;
		flex-wrap: wrap;
		gap: 8px;
		margin-top: 8px;
	}

	.chip {
		font-size: 13px;
		color: var(--muted);
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 999px;
		padding: 8px 16px;
		transition:
			color 0.15s ease,
			border-color 0.15s ease,
			background-color 0.15s ease;
	}

	.chip:hover {
		color: var(--text);
	}

	.chip--active {
		color: var(--accent);
		border-color: var(--accent);
		background: var(--accent-soft);
	}

	.consent {
		margin-top: 16px;
		font-size: 12px;
		color: var(--muted);
		max-width: 480px;
	}

	.consent-link {
		color: var(--accent);
		text-decoration: underline;
		text-underline-offset: 3px;
	}

	/* ── Estimate ── */
	.estimate {
		position: sticky;
		top: 96px;
		display: flex;
		flex-direction: column;
		gap: 16px;
	}

	.estimate__card {
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 16px;
		padding: 24px;
	}

	.estimate__title {
		font-size: 17px;
		font-weight: 600;
		margin-bottom: 16px;
	}

	.estimate__device {
		font-size: 15px;
		font-weight: 500;
	}

	.estimate__service {
		font-size: 13px;
		color: var(--muted);
		margin-top: 2px;
		margin-bottom: 12px;
	}

	.estimate__lines {
		display: flex;
		flex-direction: column;
		gap: 10px;
	}

	.estimate__lines div {
		display: flex;
		justify-content: space-between;
	}

	.estimate__lines dt {
		font-size: 14px;
		color: var(--muted);
	}

	.estimate__lines dd {
		font-size: 14px;
		font-weight: 500;
		margin: 0;
	}

	.estimate__total {
		display: flex;
		justify-content: space-between;
		align-items: baseline;
		gap: 12px;
		border-top: 1px solid var(--border);
		margin-top: 16px;
		padding-top: 16px;
	}

	.estimate__total-label {
		font-size: 11px;
		font-weight: 600;
		letter-spacing: 0.08em;
		text-transform: uppercase;
		color: var(--muted);
	}

	.estimate__total-value {
		font-size: 28px;
		font-weight: 600;
	}

	.estimate__duration {
		font-size: 12px;
		color: var(--muted);
		margin-top: 8px;
	}

	.estimate__note {
		background: var(--warning-soft);
		border-radius: 12px;
		padding: 16px 18px;
		font-size: 13px;
		line-height: 1.55;
	}

	.estimate__next {
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 16px;
		padding: 24px;
	}

	.estimate__next h3 {
		font-size: 15px;
		font-weight: 600;
		margin-bottom: 12px;
	}

	.next-steps {
		display: flex;
		flex-direction: column;
		gap: 10px;
		counter-reset: step;
	}

	.next-steps li {
		position: relative;
		font-size: 13px;
		color: var(--muted);
		padding-left: 32px;
	}

	.next-steps li::before {
		counter-increment: step;
		content: counter(step, decimal-leading-zero);
		position: absolute;
		left: 0;
		top: 1px;
		font-size: 12px;
		font-weight: 600;
		color: var(--accent);
	}

	/* ── Responsive ── */
	@media (max-width: 1024px) {
		.grid {
			grid-template-columns: 1fr;
		}

		.estimate {
			position: static;
		}
	}

	@media (max-width: 600px) {
		.field-row {
			grid-template-columns: 1fr;
		}
	}
</style>
