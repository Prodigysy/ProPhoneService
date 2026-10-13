<script lang="ts">
	import { goto } from '$app/navigation';
	import Eyebrow from '#lib/components/Eyebrow.svelte';
	import Button from '#lib/components/Button.svelte';
	import { showToast } from '#lib/components/toast.svelte.ts';
	import {
		devices,
		workshop,
		CUSTOM_DEVICE,
		serviceMeta,
		serviceOrderIndex,
		workshopWallTimeToUtcIso
	} from '../../../mocks/db.ts';
	import {
		ApiError,
		createOrder,
		fetchServices,
		type CreateOrderPayload,
		type CreateOrderResult,
		type ServiceDto
	} from '../../../services/api.ts';

	// ── Form state ─────────────────────────────────────────────────────────
	let clientName = $state('');
	let deviceId = $state('');
	let customManufacturer = $state('');
	let customDevice = $state('');
	let selectedServiceIds = $state<string[]>([]);
	let contact = $state('');
	let visitDate = $state('');
	let visitTime = $state('');
	let services = $state<ServiceDto[]>([]);
	let servicesFailed = $state(false);
	let touched = $state<Record<string, boolean>>({});
	let apiFieldErrors = $state<Record<string, string>>({});
	let submitting = $state(false);

	const device = $derived(devices.find((item) => item.id === deviceId) ?? null);
	const deviceManufacturer = $derived(
		device ? device.brand : deviceId === CUSTOM_DEVICE ? customManufacturer.trim() : ''
	);
	const deviceModel = $derived(
		device ? device.model : deviceId === CUSTOM_DEVICE ? customDevice.trim() : ''
	);
	const deviceName = $derived([deviceManufacturer, deviceModel].filter(Boolean).join(' '));

	const catalog = $derived(
		[...services].sort((a, b) => serviceOrderIndex(a.name) - serviceOrderIndex(b.name))
	);
	const selectedServices = $derived(services.filter((item) => selectedServiceIds.includes(item.id)));
	const estimateTotal = $derived(selectedServices.reduce((sum, item) => sum + item.price, 0));
	const selectedCount = $derived(selectedServices.length);

	$effect(() => {
		fetchServices()
			.then((items) => {
				services = items;
				servicesFailed = false;
			})
			.catch(() => {
				servicesFailed = true;
				showToast('Не удалось загрузить список услуг. Проверьте подключение к API.', 'error');
			});
	});

	// ── Validation: mirrors the backend WorkshopOptions ────────────────────
	const contactIsEmail = $derived(contact.includes('@'));
	const contactValue = $derived(contact.trim());
	const emailValid = $derived(/^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/.test(contactValue));
	const phoneDigits = $derived(contact.replace(/\D/g, ''));
	const phoneValid = $derived(
		/^\+7\d{10}$/.test(`+7${phoneDigits.slice(1)}`) && phoneDigits.length === 11
	);
	const contactValid = $derived(contactIsEmail ? emailValid : phoneValid);

	const visitTimes = $derived(
		Array.from(
			{ length: Number(workshop.closeAt.slice(0, 2)) - Number(workshop.openAt.slice(0, 2)) + 1 },
			(_, index) => `${String(Number(workshop.openAt.slice(0, 2)) + index).padStart(2, '0')}:00`
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
		if (
			visitDate < minimumVisitDate ||
			(visitDate === minimumVisitDate && visitTime && visitTime <= currentWorkshopTime)
		) {
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
		selectedServiceIds.length > 0 &&
		contactValid &&
		visitDate !== '' &&
		visitTime !== '' &&
		!visitError
	);

	function validateField(name: string): string | null {
		switch (name) {
			case 'name':
				return clientName.trim().length > 0 ? null : 'Укажите ваше имя';
			case 'device':
				return deviceName.length === 0 || (deviceId === CUSTOM_DEVICE && !deviceManufacturer)
					? 'Укажите производителя и модель устройства'
					: null;
			case 'services':
				return selectedServiceIds.length > 0 ? null : 'Выберите хотя бы одну услугу';
			case 'contact':
				return contactValid
					? null
					: contactIsEmail
						? 'Введите корректный email'
						: 'Введите телефон в формате +7 (XXX) XXX-XX-XX';
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

	// ── Contact mask: keeps emails as typed, masks phone numbers ───────────
	function formatPhone(raw: string): string {
		let digits = raw.replace(/\D/g, '');
		if (digits.startsWith('8')) digits = `7${digits.slice(1)}`;
		if (!digits.startsWith('7')) digits = `7${digits}`;
		digits = digits.slice(0, 11);
		const rest = digits.slice(1);
		let output = '+7';
		if (rest.length > 0) output += ` (${rest.slice(0, 3)}`;
		if (rest.length >= 3) output += ')';
		if (rest.length > 3) output += ` ${rest.slice(3, 6)}`;
		if (rest.length > 6) output += `-${rest.slice(6, 8)}`;
		if (rest.length > 8) output += `-${rest.slice(8, 10)}`;

		return output;
	}

	function onContactInput(event: Event): void {
		const raw = (event.target as HTMLInputElement).value;
		contact = /[A-Za-z@]/.test(raw) ? raw : formatPhone(raw);
	}

	function toggleService(id: string): void {
		selectedServiceIds = selectedServiceIds.includes(id)
			? selectedServiceIds.filter((item) => item !== id)
			: [...selectedServiceIds, id];
		touched.services = true;
	}

	// ── Estimate helpers ───────────────────────────────────────────────────
	const priceFormat = new Intl.NumberFormat('ru-RU');

	function priceText(price: number): string {
		return price === 0 ? 'Бесплатно' : `от ${priceFormat.format(price)} ₽`;
	}

	function plural(count: number, forms: [string, string, string]): string {
		const mod10 = count % 10;
		const mod100 = count % 100;
		if (mod10 === 1 && mod100 !== 11) return forms[0];
		if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) return forms[1];

		return forms[2];
	}

	async function submit(): Promise<void> {
		for (const field of ['name', 'device', 'services', 'contact', 'visit']) touched[field] = true;
		if (!formValid || submitting) return;

		submitting = true;
		let result: CreateOrderResult;
		try {
			const payload: CreateOrderPayload = {
				name: clientName.trim(),
				phone: contactIsEmail ? null : `+7${phoneDigits.slice(1)}`,
				email: contactIsEmail ? contactValue : null,
				deviceType: 'Phone',
				deviceManufacturer,
				deviceModel,
				preferredVisitAt: workshopWallTimeToUtcIso(visitDate, visitTime),
				serviceIds: selectedServiceIds
			};
			result = await createOrder(payload);
		} catch (error) {
			const fieldMap: Record<string, string> = {
				Name: 'name',
				Phone: 'contact',
				Email: 'contact',
				DeviceType: 'device',
				DeviceManufacturer: 'device',
				DeviceModel: 'device',
				ServiceIds: 'services',
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
			if (!mappedFieldError) {
				showToast(
					error instanceof Error ? error.message : 'Не удалось отправить заявку. Попробуйте ещё раз.',
					'error'
				);
			}
			submitting = false;

			return;
		}

		const query = new URLSearchParams({
			id: result.id,
			device: deviceName,
			services: selectedServices.map((item) => serviceMeta(item.name).displayName).join(', '),
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
					{#each devices as item (item.id)}
						<option value={item.id}>{item.brand} {item.model}</option>
					{/each}
					<option value={CUSTOM_DEVICE}>Нет модели в списке? Указать вручную</option>
				</select>
				{#if deviceId === CUSTOM_DEVICE}
					<label class="label" for="customManufacturer">Производитель</label>
					<input
						id="customManufacturer"
						class="input"
						class:input--error={!!fieldError('device')}
						placeholder="Например: Google"
						bind:value={customManufacturer}
						onblur={() => markTouched('device')}
					/>
					<label class="label" for="customDevice">Модель</label>
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
				<span class="label">Выберите услуги</span>
				<span class="hint">Можно выбрать несколько услуг</span>
				<ul class="services" class:services--error={!!fieldError('services')}>
					{#each catalog as service (service.id)}
						<li>
							<label class="service" class:service--selected={selectedServiceIds.includes(service.id)}>
								<input
									type="checkbox"
									checked={selectedServiceIds.includes(service.id)}
									onchange={() => toggleService(service.id)}
									onblur={() => markTouched('services')}
								/>
								<span class="service__name">{serviceMeta(service.name).displayName}</span>
								<span class="service__price">{priceText(service.price)}</span>
								<span class="service__check" aria-hidden="true">✓</span>
							</label>
						</li>
					{:else}
						<li class="service service--muted">
							{servicesFailed ? 'Список услуг недоступен.' : 'Загружаем услуги…'}
						</li>
					{/each}
				</ul>
				{#if fieldError('services')}
					<span class="error" role="alert">{fieldError('services')}</span>
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
					<span class="hint">Выберите удобный день для визита в мастерскую.</span>
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
				<label class="label" for="contact">Телефон или почта для связи</label>
				<input
					id="contact"
					class="input"
					class:input--error={!!fieldError('contact')}
					placeholder="+7 (___) ___-__-__ или mail@example.com"
					value={contact}
					oninput={onContactInput}
					onblur={() => markTouched('contact')}
				/>
				<span class="hint">Только для связи по ремонту. Никаких рекламных сообщений.</span>
				{#if fieldError('contact')}
					<span class="error" role="alert">{fieldError('contact')}</span>
				{/if}
			</div>

			<Button type="submit" size="lg" disabled={!formValid || submitting}>
				{submitting ? 'Отправляем…' : 'Отправить заявку'}
			</Button>

			<p class="consent">
				Нажимая «Отправить заявку», вы соглашаетесь с обработкой персональных данных и
				<button
					type="button"
					class="consent-link"
					onclick={() => showToast('Политика конфиденциальности появится позже')}
				>политикой конфиденциальности</button>.
			</p>
		</form>

		<!-- ─────────── Right column: estimate ─────────── -->
		<aside class="estimate">
			<div class="estimate__card">
				<h2 class="estimate__title">Ориентировочная стоимость</h2>

				{#if deviceName}
					<p class="estimate__device">{deviceName}</p>
				{/if}
				<p class="estimate__count">
					{selectedCount > 0
						? `${selectedCount} ${plural(selectedCount, ['выбранная услуга', 'выбранные услуги', 'выбранных услуг'])}`
						: 'Услуги не выбраны'}
				</p>

				<dl class="estimate__lines">
					{#each selectedServices as service (service.id)}
						<div>
							<dt>{serviceMeta(service.name).displayName}</dt>
							<dd>{priceText(service.price)}</dd>
						</div>
					{/each}
					<div>
						<dt>Диагностика</dt>
						<dd>Бесплатно</dd>
					</div>
				</dl>

				<div class="estimate__total">
					<span class="estimate__total-label">Предварительная оценка</span>
					<span class="estimate__total-value">
						{selectedCount > 0 ? `от ${priceFormat.format(estimateTotal)} ₽` : '—'}
					</span>
				</div>
				<p class="estimate__duration">Обычно от 40 минут после диагностики</p>
			</div>

			<div class="estimate__note">
				<strong>Пока не подтверждённая цена.</strong>
				После диагностики мастер подтвердит стоимость. Начнём ремонт только после вашего согласия.
			</div>

			<div class="estimate__next">
				<h3>Что будет после отправки?</h3>
				<ol class="next-steps">
					<li>Вы получите номер заявки для отслеживания.</li>
					<li>Приносите телефон на {workshop.addressShort}.</li>
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

	.hint {
		font-size: 12px;
		color: var(--muted);
	}

	.error {
		font-size: 12px;
		color: var(--danger);
	}

	/* ── Services multi-select ── */
	.services {
		display: flex;
		flex-direction: column;
		gap: 8px;
		margin-top: 6px;
		border: 1px solid var(--border);
		border-radius: 12px;
		padding: 8px;
		background: var(--surface);
	}

	.services--error {
		border-color: var(--danger);
	}

	.service {
		display: flex;
		align-items: center;
		gap: 12px;
		padding: 10px 12px;
		border-radius: 10px;
		cursor: pointer;
		transition: background-color 0.15s ease;
	}

	.service:hover {
		background: var(--accent-soft);
	}

	.service--selected {
		background: var(--accent-soft);
	}

	.service--muted {
		color: var(--muted);
		cursor: default;
	}

	.service input {
		position: absolute;
		opacity: 0;
		pointer-events: none;
	}

	.service__name {
		flex: 1;
		font-size: 14px;
	}

	.service__price {
		font-size: 13px;
		color: var(--muted);
		white-space: nowrap;
	}

	.service__check {
		color: var(--accent);
		font-size: 15px;
		font-weight: 600;
		opacity: 0;
	}

	.service--selected .service__check {
		opacity: 1;
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

	.estimate__count {
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
		gap: 12px;
	}

	.estimate__lines dt {
		font-size: 14px;
		color: var(--muted);
	}

	.estimate__lines dd {
		font-size: 14px;
		font-weight: 500;
		margin: 0;
		white-space: nowrap;
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
		white-space: nowrap;
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
