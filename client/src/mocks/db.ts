// Deterministic mock data mirroring the V2 mockup (docs/Макет.V2.pdf).
// Used by src/services/api.ts when VITE_API_URL is not set.

export interface WorkshopInfo {
	name: string;
	address: string;
	/** Street and building only, for copy such as "Приносите телефон на …". */
	addressShort: string;
	metro: string;
	metroDistance: string;
	phone: string;
	hours: string;
	timeZone: string;
	workingDays: number[]; // ISO-8601: Monday=1 … Sunday=7
	openAt: string;
	closeAt: string;
	telegramUrl: string;
	warrantyDays: number;
	diagnosticsPrice: number;
}

export const workshop: WorkshopInfo = {
	name: 'про телефон',
	address: 'Самара, просп. Ленина, 12А',
	addressShort: 'проспект Ленина, 12А',
	metro: 'Алабинская',
	metroDistance: '5 минут от метро «Алабинская»',
	phone: '+7 (495) 120-45-67',
	hours: 'Пн–Пт · 10:00–19:00',
	timeZone: 'Europe/Samara',
	workingDays: [1, 2, 3, 4, 5],
	openAt: '10:00',
	closeAt: '19:00',
	telegramUrl: 'https://t.me/prophoneservice',
	warrantyDays: 90,
	diagnosticsPrice: 0
};

export interface WorkshopStatus {
	isOpen: boolean;
	message: string;
}

export function getWorkshopStatus(now = new Date()): WorkshopStatus {
	const parts = new Intl.DateTimeFormat('en-US', {
		timeZone: workshop.timeZone,
		weekday: 'short',
		hour: '2-digit',
		minute: '2-digit',
		hourCycle: 'h23'
	}).formatToParts(now);
	const values = Object.fromEntries(parts.map((part) => [part.type, part.value]));
	const weekdayByName: Record<string, number> = {
		Mon: 1,
		Tue: 2,
		Wed: 3,
		Thu: 4,
		Fri: 5,
		Sat: 6,
		Sun: 7
	};
	const weekday = weekdayByName[values.weekday] ?? 0;
	const minuteOfDay = Number(values.hour) * 60 + Number(values.minute);
	const openMinute = Number(workshop.openAt.slice(0, 2)) * 60 + Number(workshop.openAt.slice(3));
	const closeMinute = Number(workshop.closeAt.slice(0, 2)) * 60 + Number(workshop.closeAt.slice(3));
	const isWorkingDay = workshop.workingDays.includes(weekday);

	if (!isWorkingDay) {
		return { isOpen: false, message: `Выходной · ${workshop.hours}` };
	}
	if (minuteOfDay < openMinute) {
		return { isOpen: false, message: `Откроемся в ${workshop.openAt}` };
	}
	if (minuteOfDay >= closeMinute) {
		return {
			isOpen: false,
			message: weekday === 5 ? 'Закрыто · откроемся в понедельник в 10:00' : `Закрыто · откроемся завтра в ${workshop.openAt}`
		};
	}
	return { isOpen: true, message: `Сегодня открыты до ${workshop.closeAt}` };
}

const WORKSHOP_TIME_PARTS = new Intl.DateTimeFormat('en-US', {
	timeZone: workshop.timeZone,
	year: 'numeric',
	month: '2-digit',
	day: '2-digit',
	hour: '2-digit',
	minute: '2-digit',
	hourCycle: 'h23'
});

/**
 * Convert a wall-clock date/time in the workshop timezone into an ISO UTC instant.
 * The backend requires `PreferredVisitAt` as `DateTimeOffset`, and the visit slots
 * are shown to the client in workshop-local time.
 */
export function workshopWallTimeToUtcIso(date: string, time: string): string {
	const [year, month, day] = date.split('-').map(Number);
	const [hour, minute] = time.split(':').map(Number);
	const asUtc = Date.UTC(year, month - 1, day, hour, minute);
	const parts = Object.fromEntries(
		WORKSHOP_TIME_PARTS.formatToParts(new Date(asUtc)).map((part) => [part.type, part.value])
	);
	const shownAsUtc = Date.UTC(
		Number(parts.year),
		Number(parts.month) - 1,
		Number(parts.day),
		Number(parts.hour),
		Number(parts.minute)
	);

	return new Date(asUtc - (shownAsUtc - asUtc)).toISOString();
}

export interface DeviceModel {
	id: string;
	brand: string;
	model: string;
	category: 'apple' | 'samsung' | 'xiaomi';
}

export const devices: DeviceModel[] = [
	{ id: 'iphone-11', brand: 'Apple', model: 'iPhone 11', category: 'apple' },
	{ id: 'iphone-13', brand: 'Apple', model: 'iPhone 13', category: 'apple' },
	{ id: 'iphone-14', brand: 'Apple', model: 'iPhone 14', category: 'apple' },
	{ id: 'iphone-15', brand: 'Apple', model: 'iPhone 15', category: 'apple' },
	{ id: 'galaxy-s23', brand: 'Samsung', model: 'Galaxy S23', category: 'samsung' },
	{ id: 'galaxy-a52', brand: 'Samsung', model: 'Galaxy A52', category: 'samsung' },
	{ id: 'redmi-note12', brand: 'Xiaomi', model: 'Redmi Note 12', category: 'xiaomi' }
];

export const CUSTOM_DEVICE = 'custom';

export interface ServiceMeta {
	name: string;
	/** Short label from the mockup ("Диагностика" instead of the API's long name). */
	displayName: string;
	description: string;
	duration: string;
}

/**
 * V2 mockup metadata for the seeded services: display order, duration and the
 * short description of the price list. The API returns only name and price, so
 * this reference table fills the remaining columns of the layout.
 */
export const serviceCatalog: ServiceMeta[] = [
	{ name: 'Диагностика (выявление неисправности)', displayName: 'Диагностика', description: 'Найдём причину неисправности', duration: 'от 20 мин' },
	{ name: 'Замена дисплейного модуля', displayName: 'Замена дисплейного модуля', description: 'Изображение, сенсор и стекло', duration: '2–4 часа' },
	{ name: 'Замена разъема зарядки', displayName: 'Замена разъема зарядки', description: 'Не заряжается или отходит кабель', duration: '1–2 часа' },
	{ name: 'Замена аккумулятора', displayName: 'Замена аккумулятора', description: 'Быстро разряжается или выключается', duration: 'от 40 мин' },
	{ name: 'Замена микрофона', displayName: 'Замена микрофона', description: 'Плохая слышимость во время звонков', duration: '1–2 часа' },
	{ name: 'Замена динамика', displayName: 'Замена динамика', description: 'Не слышно звука или есть помехи', duration: '1–2 часа' },
	{ name: 'Замена кнопки', displayName: 'Замена кнопки', description: 'Не нажимается или застряла', duration: '1–2 часа' },
	{ name: 'Замена камеры', displayName: 'Замена камеры', description: 'Не фокусируется или не снимает', duration: '1–2 часа' },
	{ name: 'Профилактика', displayName: 'Профилактика', description: 'Проверка и подготовка устройства', duration: 'от 30 мин' },
	{ name: 'Чистка сеток и разъемов', displayName: 'Чистка сеток и разъемов', description: 'Удаление пыли и мусора', duration: 'от 20 мин' }
];

/** Catalog entry for a service name, with a neutral fallback for unknown services. */
export function serviceMeta(name: string): ServiceMeta {
	return (
		serviceCatalog.find((item) => item.name.toLowerCase() === name.toLowerCase()) ?? {
			name,
			displayName: name,
			description: 'Описание уточним после диагностики',
			duration: 'уточним после диагностики'
		}
	);
}

/** Catalog position of a service, used to keep the mockup's row order. */
export function serviceOrderIndex(name: string): number {
	const index = serviceCatalog.findIndex((item) => item.name.toLowerCase() === name.toLowerCase());
	return index === -1 ? serviceCatalog.length : index;
}

export const priceUpdatedAt = '1 октября 2026';

export interface ReviewItem {
	id: string;
	author: string;
	initials: string;
	date: string;
	text: string;
}

export const reviews: ReviewItem[] = [
	{
		id: 'elena',
		author: 'Елена К.',
		initials: 'ЕК',
		date: '24 сентября 2026',
		text: 'Поменяли экран на iPhone за пару часов. Цену назвали сразу, никаких сюрпризов. Очень аккуратная работа!'
	},
	{
		id: 'dmitry',
		author: 'Дмитрий В.',
		initials: 'ДВ',
		date: '19 сентября 2026',
		text: 'Телефон перестал заряжаться. Мастер объяснил, в чём дело, и заменил разъём. Теперь всё работает отлично.'
	},
	{
		id: 'anna',
		author: 'Анна С.',
		initials: 'АС',
		date: '14 августа 2026',
		text: 'Удобно, что можно оставить заявку без звонка и посмотреть статус. Ребята внимательные, вернусь при необходимости.'
	}
];

export const workshopStats = {
	years: '8 лет',
	yearsLabel: 'чиним телефоны',
	saved: '4 800+',
	savedLabel: 'устройств спасено',
	rating: '4,9 / 5',
	ratingLabel: 'оценка на Яндекс',
	reviewsCount: '218'
};

export const masterProfile = {
	name: 'Алексей Морозов',
	initials: 'АМ',
	role: 'Основатель и мастер',
	promise: 'Лично отвечаю за качество.'
};

export const aboutText =
	'С 2018 года помогаем соседям на Покровке оставаться на связи. Объясняем поломку простыми словами, бережём ваши данные и не начинаем ремонт без согласования цены.';

export const heroTrust = [
	{ title: 'Диагностика — 0 ₽', subtitle: null },
	{ title: 'Гарантия 90 дней', subtitle: null }
];

export const heroBadge = {
	title: 'В надёжных руках',
	subtitle: 'Ремонтируем, а не усложняем'
};

export const steps = [
	{
		num: '01',
		icon: '<svg width="20" height="20" viewBox="0 0 24 24" fill="none"><path d="M4 5h16v11H8l-4 4V5Z" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round"/></svg>',
		title: 'Расскажите о поломке',
		text: 'Выберите устройство и проблему. Заявка займёт всего пару минут.'
	},
	{
		num: '02',
		icon: '<svg width="20" height="20" viewBox="0 0 24 24" fill="none"><path d="M4 8V4h4M16 4h4v4M20 16v4h-4M8 20H4v-4" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/><rect x="8.5" y="8.5" width="7" height="7" rx="1.5" stroke="currentColor" stroke-width="1.8"/></svg>',
		title: 'Принесите телефон',
		text: 'Проведём диагностику и согласуем точную цену до начала ремонта.'
	},
	{
		num: '03',
		icon: '<svg width="20" height="20" viewBox="0 0 24 24" fill="none"><path d="M14.5 6.5a3.5 3.5 0 0 1 4.9-4.3l-2.6 2.6 2.4 2.4L21.8 4.6a3.5 3.5 0 0 1-4.9 4.9L8 18.4a2 2 0 1 1-2.8-2.8l9.3-9.1Z" stroke="currentColor" stroke-width="1.7" stroke-linejoin="round"/></svg>',
		title: 'Мы всё исправим',
		text: 'Мастер займётся устройством. Следите за этапами ремонта онлайн.'
	},
	{
		num: '04',
		icon: '<svg width="20" height="20" viewBox="0 0 24 24" fill="none"><circle cx="12" cy="12" r="9" stroke="currentColor" stroke-width="1.8"/><path d="m8.5 12.2 2.3 2.3 4.7-5" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/></svg>',
		title: 'Заберите как новый',
		text: 'Проверим телефон вместе с вами и выдадим гарантию на 90 дней.'
	}
];
