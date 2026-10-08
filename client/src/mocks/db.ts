// Deterministic mock data mirroring the Figma/PDF mockup (docs/Макет.pdf).
// Used by src/services/api.ts when VITE_API_URL is not set.

export interface WorkshopInfo {
	name: string;
	address: string;
	metro: string;
	metroDistance: string;
	phone: string;
	hours: string;
	openAt: string; // 'HH:mm' local Moscow time
	closeAt: string;
	telegramUrl: string;
	warrantyDays: number;
	diagnosticsPrice: number; // 0 ₽
}

export const workshop: WorkshopInfo = {
	name: 'про телефон',
	address: 'Москва, ул. Покровка, 17',
	metro: 'Чистые пруды',
	metroDistance: '7 минут от метро «Чистые пруды»',
	phone: '+7 (495) 120-45-67',
	hours: 'Ежедневно · 10:00–20:00',
	openAt: '10:00',
	closeAt: '20:00',
	telegramUrl: 'https://t.me/prophoneservice',
	warrantyDays: 90,
	diagnosticsPrice: 0
};

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

export interface PriceRow {
	id: string;
	name: string;
	description: string;
	apple: number | null; // null = free
	samsung: number | null;
	xiaomi: number | null;
	duration: string;
}

export const priceRows: PriceRow[] = [
	{
		id: 'display',
		name: 'Замена дисплея',
		description: 'Изображение, сенсор и стекло',
		apple: 9900,
		samsung: 6900,
		xiaomi: 4490,
		duration: '2–4 часа'
	},
	{
		id: 'battery',
		name: 'Замена аккумулятора',
		description: 'Быстро разряжается или выключается',
		apple: 3490,
		samsung: 2990,
		xiaomi: 2490,
		duration: 'от 40 мин'
	},
	{
		id: 'charging-port',
		name: 'Разъём зарядки',
		description: 'Не заряжается или отходит кабель',
		apple: 3490,
		samsung: 2490,
		xiaomi: 2490,
		duration: '1–2 часа'
	},
	{
		id: 'back-glass',
		name: 'Замена заднего стекла',
		description: 'Трещины и повреждения корпуса',
		apple: 5900,
		samsung: 3490,
		xiaomi: 2490,
		duration: '2–3 часа'
	},
	{
		id: 'camera',
		name: 'Ремонт камеры',
		description: 'Не фокусируется или не снимает',
		apple: 4490,
		samsung: 3490,
		xiaomi: 2990,
		duration: '1–2 часа'
	},
	{
		id: 'water',
		name: 'После попадания воды',
		description: 'Чистка и восстановление устройства',
		apple: 2990,
		samsung: 2990,
		xiaomi: 2490,
		duration: 'от 1 дня'
	},
	{
		id: 'diagnostics',
		name: 'Диагностика',
		description: 'Найдём причину неисправности',
		apple: null,
		samsung: null,
		xiaomi: null,
		duration: 'от 20 мин'
	}
];

export const priceUpdatedAt = '1 октября 2026';

export interface ProblemOption {
	id: string;
	label: string;
	serviceId: string | null;
	partsPrice: number; // запчасть
	workPrice: number; // работа мастера
}

export const problems: ProblemOption[] = [
	{ id: 'battery', label: 'Быстро разряжается · замена аккумулятора', serviceId: 'battery', partsPrice: 2490, workPrice: 1000 },
	{ id: 'screen', label: 'Разбит экран', serviceId: 'display', partsPrice: 3490, workPrice: 1500 },
	{ id: 'charging', label: 'Не заряжается', serviceId: 'charging-port', partsPrice: 1490, workPrice: 1000 },
	{ id: 'water', label: 'Попала вода', serviceId: 'water', partsPrice: 990, workPrice: 2000 },
	{ id: 'camera', label: 'Камера не фокусируется', serviceId: 'camera', partsPrice: 2490, workPrice: 1000 },
	{ id: 'other', label: 'Другая проблема', serviceId: null, partsPrice: 0, workPrice: 0 }
];

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
	rating: '4,9 /',
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
	{ title: 'В надёжных руках', subtitle: 'Ремонтируем, а не усложняем' },
	{ title: 'Диагностика — 0 ₽', subtitle: null },
	{ title: 'Гарантия 90 дней', subtitle: null }
];

export const steps = [
	{
		num: '01',
		title: 'Расскажите о поломке',
		text: 'Выберите устройство и проблему. Заявка займёт всего пару минут.'
	},
	{
		num: '02',
		title: 'Принесите телефон',
		text: 'Проведём диагностику и согласуем точную цену до начала ремонта.'
	},
	{
		num: '03',
		title: 'Мы всё исправим',
		text: 'Мастер займётся устройством. Следите за этапами ремонта онлайн.'
	},
	{
		num: '04',
		title: 'Заберите как новый',
		text: 'Проверим телефон вместе с вами и выдадим гарантию на 90 дней.'
	}
];
