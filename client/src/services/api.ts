// Service layer — the ONLY place in the app that talks to the backend.
// With VITE_API_URL unset, deterministic mock data is used.
// For local API mode set VITE_API_URL=/api; vite.config.ts proxies it to :8080.
//
// API contracts are based on Darowiin/ProPhoneService feat/backend-foundation:
//   GET /api/orders?page=N, POST /api/orders, PATCH /api/orders/{id}/status
//   GET /api/services
// Order creation sends name, contact, device, visit time and the selected
// service ids; the API computes the total price from those ids.

import { serviceMeta, workshop } from '../mocks/db.ts';

export type RepairStatus =
	| 'Created'
	| 'InDiagnostic'
	| 'Agreed'
	| 'InProgress'
	| 'Ready'
	| 'Completed'
	| 'Cancelled';

export interface RepairOrderDto {
	id: string;
	/** The API exposes the Guid as the order identifier; mocks use the design's ПТ- number. */
	number: string;
	clientName: string | null;
	deviceName: string;
	serviceName: string | null;
	price: number | null;
	priceFrom: boolean;
	status: RepairStatus | null;
	createdAt: string;
	preferredVisitAt: string | null;
	statusTimeline?: RepairStatusHistoryDto[];
}

export interface RepairStatusHistoryDto {
	status: RepairStatus;
	changedAt: string;
	comment: string | null;
}

export interface ServiceDto {
	id: string;
	name: string;
	price: number;
	description: string | null;
}

/**
 * Mirrors POST /api/orders. Phone or email is required by the backend, so exactly
 * one of them carries the value the client typed into the single contact field.
 */
export interface CreateOrderPayload {
	name: string;
	phone: string | null;
	email: string | null;
	deviceType: 'Phone' | 'Tablet' | 'Laptop';
	deviceManufacturer: string;
	deviceModel: string;
	preferredVisitAt: string;
	serviceIds: string[];
}

export interface CreateOrderResult {
	id: string;
	number: string;
	status: RepairStatus;
}

export class ApiError extends Error {
	constructor(
		message: string,
		readonly status?: number,
		readonly fieldErrors: Record<string, string[]> = {}
	) {
		super(message);
		this.name = 'ApiError';
	}
}

// ─── Status transitions (mirrors RepairStatusTransitions from Domain.Shared) ──

export const statusTransitions: Record<RepairStatus, RepairStatus[]> = {
	Created: ['InDiagnostic', 'Cancelled'],
	InDiagnostic: ['Agreed', 'Cancelled'],
	Agreed: ['InProgress', 'Cancelled'],
	InProgress: ['Ready', 'Cancelled'],
	Ready: ['Completed'],
	Completed: [],
	Cancelled: []
};

export const statusLabels: Record<RepairStatus, string> = {
	Created: 'Принята',
	InDiagnostic: 'Диагностика',
	Agreed: 'Согласована',
	InProgress: 'В ремонте',
	Ready: 'Готово',
	Completed: 'Выдана',
	Cancelled: 'Отменена'
};

const mockServices: ServiceDto[] = [
	{ id: '6f0d2a71-3f2e-4b1e-9d1f-0a1b2c3d4e01', name: 'Диагностика (выявление неисправности)', price: 0, description: null },
	{ id: '6f0d2a71-3f2e-4b1e-9d1f-0a1b2c3d4e02', name: 'Замена дисплейного модуля', price: 1500, description: null },
	{ id: '6f0d2a71-3f2e-4b1e-9d1f-0a1b2c3d4e03', name: 'Замена разъема зарядки', price: 1200, description: null },
	{ id: '6f0d2a71-3f2e-4b1e-9d1f-0a1b2c3d4e04', name: 'Замена аккумулятора', price: 1500, description: null },
	{ id: '6f0d2a71-3f2e-4b1e-9d1f-0a1b2c3d4e05', name: 'Замена микрофона', price: 1500, description: null },
	{ id: '6f0d2a71-3f2e-4b1e-9d1f-0a1b2c3d4e06', name: 'Замена динамика', price: 1100, description: null },
	{ id: '6f0d2a71-3f2e-4b1e-9d1f-0a1b2c3d4e07', name: 'Замена кнопки', price: 1200, description: null },
	{ id: '6f0d2a71-3f2e-4b1e-9d1f-0a1b2c3d4e08', name: 'Замена камеры', price: 1500, description: null },
	{ id: '6f0d2a71-3f2e-4b1e-9d1f-0a1b2c3d4e09', name: 'Профилактика', price: 1000, description: null },
	{ id: '6f0d2a71-3f2e-4b1e-9d1f-0a1b2c3d4e10', name: 'Чистка сеток и разъемов', price: 700, description: null }
];

// ─── Mock store (module-level, so changes persist during the session) ───────

const orders: RepairOrderDto[] = [
	{
		id: 'a1f3c2d4-0001-4a01-9c11-111111111111',
		number: 'ПТ-1048',
		clientName: 'Анна Смирнова',
		deviceName: 'iPhone 13',
		serviceName: 'Замена аккумулятора',
		price: 3490,
		priceFrom: false,
		status: 'InProgress',
		createdAt: '2026-10-01T10:24:00Z',
		preferredVisitAt: '2026-10-01T11:00:00Z'
	},
	{
		id: 'a1f3c2d4-0002-4a01-9c11-222222222222',
		number: 'ПТ-1047',
		clientName: 'Дмитрий Волков',
		deviceName: 'Samsung Galaxy S23',
		serviceName: 'Замена дисплея',
		price: 9900,
		priceFrom: true,
		status: 'InDiagnostic',
		createdAt: '2026-10-01T09:10:00Z',
		preferredVisitAt: null
	},
	{
		id: 'a1f3c2d4-0003-4a01-9c11-333333333333',
		number: 'ПТ-1046',
		clientName: 'Мария Соколова',
		deviceName: 'Xiaomi Redmi Note 12',
		serviceName: 'Не заряжается',
		price: 2490,
		priceFrom: true,
		status: 'Created',
		createdAt: '2026-10-01T08:45:00Z',
		preferredVisitAt: '2026-10-01T15:00:00Z'
	},
	{
		id: 'a1f3c2d4-0004-4a01-9c11-444444444444',
		number: 'ПТ-1045',
		clientName: 'Илья Козлов',
		deviceName: 'iPhone 14',
		serviceName: 'Замена заднего стекла',
		price: 5900,
		priceFrom: false,
		status: 'InProgress',
		createdAt: '2026-09-30T16:20:00Z',
		preferredVisitAt: null
	},
	{
		id: 'a1f3c2d4-0005-4a01-9c11-555555555555',
		number: 'ПТ-1021',
		clientName: 'Анна Смирнова',
		deviceName: 'iPhone 13',
		serviceName: 'Замена дисплея',
		price: 9900,
		priceFrom: false,
		status: 'Completed',
		createdAt: '2026-08-12T14:05:00Z',
		preferredVisitAt: null
	},
	{
		id: 'a1f3c2d4-0006-4a01-9c11-666666666666',
		number: 'ПТ-0986',
		clientName: 'Анна Смирнова',
		deviceName: 'Samsung Galaxy A52',
		serviceName: 'Замена разъёма зарядки',
		price: 2490,
		priceFrom: false,
		status: 'Completed',
		createdAt: '2026-05-24T12:00:00Z',
		preferredVisitAt: null
	},
	{
		id: 'a1f3c2d4-0007-4a01-9c11-777777777777',
		number: 'ПТ-0912',
		clientName: 'Анна Смирнова',
		deviceName: 'iPhone 11',
		serviceName: 'Замена аккумулятора',
		price: 2990,
		priceFrom: false,
		status: 'Completed',
		createdAt: '2026-02-03T10:30:00Z',
		preferredVisitAt: null
	}
];

// Master panel stats derived from the loaded orders. The current backend DTO
// returns no revenue or client fields, so those cards render placeholders live.

export interface MasterStats {
	todayCount: number;
	todayDelta: string;
	inWork: number;
	inWorkNote: string;
	revenueMonth: string | null;
	revenueNote: string;
	clientsCount: number | null;
	clientsNote: string;
	totalOrders: number;
	statusCounts: Record<RepairStatus, number>;
}

const priceFormat = new Intl.NumberFormat('ru-RU');

export function deriveMasterStats(
	source: RepairOrderDto[],
	backendMode = false,
	now = new Date()
): MasterStats {
	const statusCounts: Record<RepairStatus, number> = {
		Created: 0,
		InDiagnostic: 0,
		Agreed: 0,
		InProgress: 0,
		Ready: 0,
		Completed: 0,
		Cancelled: 0
	};
	let todayCount = 0;
	let yesterdayCount = 0;
	let inWork = 0;
	let inDiagnostic = 0;
	const clients = new Set<string>();
	const nowMonth = now.getUTCMonth();
	const nowYear = now.getUTCFullYear();
	const monthStart = Date.UTC(nowYear, nowMonth, 1);
	let monthRevenue = 0;
	let monthCompleted = 0;

	for (const order of source) {
		if (order.status) statusCounts[order.status]++;
		if (order.clientName) clients.add(order.clientName);
		const created = new Date(order.createdAt);
		if (sameWorkshopDay(created, now)) todayCount++;
		if (sameWorkshopDay(created, new Date(now.getTime() - 86_400_000))) yesterdayCount++;
		if (order.status === 'InProgress' || order.status === 'Agreed') inWork++;
		if (order.status === 'InDiagnostic') inDiagnostic++;
		if (
			!backendMode && order.status === 'Completed' && order.price !== null &&
			created.getTime() >= monthStart
		) {
			monthRevenue += order.price;
			monthCompleted++;
		}
	}

	const monthName = now.toLocaleDateString('ru-RU', { month: 'long', timeZone: workshop.timeZone });
	const delta = todayCount - yesterdayCount;
	return {
		todayCount,
		todayDelta:
			delta === 0
				? 'как вчера'
				: `${delta > 0 ? '+' : ''}${delta} к предыдущему дню`,
		inWork,
		inWorkNote: inDiagnostic > 0 ? `Ещё ${inDiagnostic} — на диагностике` : 'Все вне диагностики',
		revenueMonth: backendMode ? null : `${priceFormat.format(monthRevenue)} ₽`,
		revenueNote: backendMode
			? 'Итоговая стоимость не возвращается API'
			: `${monthName} ${now.getUTCFullYear()} · ${monthCompleted} ${pluralize(monthCompleted, ['ремонт', 'ремонта', 'ремонтов'])}`,
		clientsCount: backendMode ? null : clients.size,
		clientsNote: backendMode ? 'Клиенты не возвращаются API' : 'Клиенты в мок-данных',
		totalOrders: source.length,
		statusCounts
	};
}

function sameWorkshopDay(a: Date, b: Date): boolean {
	const format = new Intl.DateTimeFormat('en-CA', {
		timeZone: workshop.timeZone,
		year: 'numeric',
		month: '2-digit',
		day: '2-digit'
	});

	return format.format(a) === format.format(b);
}

function pluralize(n: number, forms: [string, string, string]): string {
	const mod10 = n % 10;
	const mod100 = n % 100;
	if (mod10 === 1 && mod100 !== 11) return forms[0];
	if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) return forms[1];
	return forms[2];
}

let nextOrderNumber = 1049;

// Set VITE_API_URL=/api to use the backend. Vite proxies /api to localhost:8080.
const apiBase = (import.meta.env.VITE_API_URL ?? '').replace(/\/$/, '');
export const isMockMode = apiBase.length === 0;
const apiPageSize = 25; // OrderWorkflowService.PageSize in feat/backend-foundation

interface BackendOrderDto {
	id: string;
	status: RepairStatus | null;
	device: { type: string; manufacturer: string; model: string };
	preferredVisitAt: string | null;
	createdAt: string;
	statusTimeline: RepairStatusHistoryDto[];
}

interface BackendCreateOrderResponse {
	id: string;
}

function asFrontendOrder(order: BackendOrderDto): RepairOrderDto {
	return {
		id: order.id,
		number: order.id,
		clientName: null,
		deviceName: [order.device.manufacturer, order.device.model].filter(Boolean).join(' '),
		serviceName: null,
		price: null,
		priceFrom: false,
		status: order.status,
		createdAt: order.createdAt,
		preferredVisitAt: order.preferredVisitAt,
		statusTimeline: order.statusTimeline
	};
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
	const headers = new Headers(init?.headers);
	if (init?.body && !headers.has('Content-Type')) headers.set('Content-Type', 'application/json');
	const response = await fetch(`${apiBase}${path}`, { ...init, headers });
	if (!response.ok) {
		let message = `API вернул ${response.status}`;
		let fieldErrors: Record<string, string[]> = {};
		try {
			const body = (await response.json()) as {
				title?: string;
				detail?: string;
				errors?: Record<string, string[]>;
			};
			fieldErrors = body.errors ?? {};
			message = Object.values(fieldErrors).flat()[0] ?? body.detail ?? body.title ?? message;
		} catch {
			// Some API errors, including 204 responses, have no JSON body.
		}
		throw new ApiError(message, response.status, fieldErrors);
	}
	if (response.status === 204) return undefined as T;
	return (await response.json()) as T;
}

const delay = (ms = 250) => {
	const { promise, resolve } = Promise.withResolvers<void>();
	setTimeout(resolve, ms);
	return promise;
};

export async function fetchOrders(): Promise<RepairOrderDto[]> {
	if (isMockMode) {
		await delay();
		return [...orders];
	}

	const result: RepairOrderDto[] = [];
	for (let page = 1; ; page++) {
		const items = await request<BackendOrderDto[]>(`/orders?page=${page}`);
		result.push(...items.map(asFrontendOrder));
		if (items.length < apiPageSize) break;
	}
	return result;
}

export async function fetchServices(): Promise<ServiceDto[]> {
	if (isMockMode) {
		await delay();
		return mockServices.map((service) => ({ ...service }));
	}
	return request<ServiceDto[]>('/services');
}

export async function fetchOrderById(id: string): Promise<RepairOrderDto> {
	if (isMockMode) {
		await delay();
		const order = orders.find((item) => item.id === id || item.number.toLowerCase() === id.toLowerCase());
		if (!order) throw new ApiError('Заявка не найдена', 404);
		return { ...order };
	}
	const order = (await fetchOrders()).find((item) => item.id.toLowerCase() === id.toLowerCase());
	if (!order) throw new ApiError('Заявка не найдена', 404);
	return order;
}

export async function createOrder(payload: CreateOrderPayload): Promise<CreateOrderResult> {
	if (isMockMode) {
		await delay(350);
		const number = `ПТ-${nextOrderNumber++}`;
		const selectedServices = mockServices.filter((service) => payload.serviceIds.includes(service.id));
		const orderId = crypto.randomUUID();
		orders.unshift({
			id: orderId,
			number,
			clientName: payload.name,
			deviceName: `${payload.deviceManufacturer} ${payload.deviceModel}`.trim(),
			serviceName: selectedServices.map((service) => serviceMeta(service.name).displayName).join(', ') || 'Диагностика',
			price: selectedServices.reduce((sum, service) => sum + service.price, 0),
			priceFrom: false,
			status: 'Created',
			createdAt: new Date().toISOString(),
			preferredVisitAt: payload.preferredVisitAt
		});
		return { id: orderId, number, status: 'Created' };
	}

	const result = await request<BackendCreateOrderResponse>('/orders', {
		method: 'POST',
		body: JSON.stringify({
			name: payload.name,
			phone: payload.phone,
			email: payload.email,
			deviceType: payload.deviceType,
			deviceManufacturer: payload.deviceManufacturer,
			deviceModel: payload.deviceModel,
			preferredVisitAt: payload.preferredVisitAt,
			serviceIds: payload.serviceIds
		})
	});
	return { id: result.id, number: result.id, status: 'Created' };
}

export async function changeOrderStatus(
	orderId: string,
	next: RepairStatus,
	comment?: string
): Promise<void> {
	if (isMockMode) {
		await delay(300);
		const order = orders.find((item) => item.id === orderId);
		if (!order) throw new ApiError('Заявка не найдена', 404);
		if (!order.status || !statusTransitions[order.status].includes(next)) {
			throw new ApiError(
				`Недопустимый переход статуса: ${order.status ? statusLabels[order.status] : 'не задан'} → ${statusLabels[next]}`,
				409
			);
		}
		order.status = next;
		order.statusTimeline = [
			...(order.statusTimeline ?? []),
			{ status: next, changedAt: new Date().toISOString(), comment: comment ?? null }
		];
		return;
	}
	await request<void>(`/orders/${encodeURIComponent(orderId)}/status`, {
		method: 'PATCH',
		body: JSON.stringify({ status: next, comment })
	});
}
