// ─────────────────────────────────────────────────────────────────────────────
// Service layer — the ONLY place in the app that talks to the backend.
// While VITE_API_URL is unset, works in deterministic mock mode against
// src/mocks/db.ts. When the ASP.NET Core backend is ready, set VITE_API_URL
// (e.g. http://localhost:8080) and only this file needs endpoint mapping.
//
// Contracts mirror server/ProPhoneService.Domain models:
//   RepairOrder: Id, ClientId, DeviceId, Status, TotalPrice, CreatedAt, PreferredVisitAt
//   RepairStatus enum: Created, InDiagnostic, Agreed, InProgress, Ready, Completed, Cancelled
//   RepairStatusTransitions — allowed transitions matrix (same rules below)
// ─────────────────────────────────────────────────────────────────────────────

import { problems } from '../mocks/db.ts';

export type RepairStatus =
	| 'Created'
	| 'InDiagnostic'
	| 'Agreed'
	| 'InProgress'
	| 'Ready'
	| 'Completed'
	| 'Cancelled';

export interface RepairOrderDto {
	/** Guid */
	id: string;
	/** Human-readable order number, e.g. 'ПТ-1048' */
	number: string;
	clientName: string;
	deviceName: string;
	serviceName: string;
	/** null → price not confirmed yet (render '—' / 'Цена' in master panel) */
	price: number | null;
	/** 'from' prefix for unconfirmed prices in the mockup */
	priceFrom: boolean;
	status: RepairStatus;
	/** ISO datetime */
	createdAt: string;
	preferredVisitAt: string | null;
}

export interface CreateOrderPayload {
	deviceName: string;
	problemDescription: string;
	phone: string;
	preferredVisitAt: string | null;
	/** Selected problem/option label, if picked from the list */
	problemId: string | null;
	/** Preliminary estimate parts+work shown to the client, ₽ */
	estimateTotal: number;
}

export class ApiError extends Error {
	constructor(
		message: string,
		readonly status?: number
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

// Master panel stats, computed from the mock store (mirrors the mockup cards).
// Backend will expose real counters later; shape matches what the UI needs.

export interface MasterStats {
	/** New orders today */
	todayCount: number;
	todayDelta: string;
	/** InProgress + Agreed */
	inWork: number;
	inWorkNote: string;
	/** Sum of Completed order prices this month, formatted */
	revenueMonth: string;
	revenueNote: string;
	/** Distinct clients in the store */
	clientsCount: number;
	/** Total orders in the store */
	totalOrders: number;
	/** Count per status, for filter chips */
	statusCounts: Record<RepairStatus, number>;
}

const priceFormat = new Intl.NumberFormat('ru-RU');

function computeMasterStats(now = new Date()): MasterStats {
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

	for (const order of orders) {
		statusCounts[order.status]++;
		clients.add(order.clientName);
		const created = new Date(order.createdAt);
		if (sameUtcDay(created, now)) todayCount++;
		if (sameUtcDay(created, new Date(now.getTime() - 86_400_000))) yesterdayCount++;
		if (order.status === 'InProgress' || order.status === 'Agreed') inWork++;
		if (order.status === 'InDiagnostic') inDiagnostic++;
		if (
			order.status === 'Completed' &&
			order.price !== null &&
			new Date(order.createdAt).getTime() >= monthStart
		) {
			monthRevenue += order.price;
			monthCompleted++;
		}
	}

	const monthName = now.toLocaleDateString('ru-RU', { month: 'long', timeZone: 'UTC' });
	const delta = todayCount - yesterdayCount;
	return {
		todayCount,
		todayDelta:
			delta === 0
				? 'как вчера'
				: `${delta > 0 ? '+' : ''}${delta} к предыдущему дню`,
		inWork,
		inWorkNote: inDiagnostic > 0 ? `Ещё ${inDiagnostic} — на диагностике` : 'Все вне диагностики',
		revenueMonth: `${priceFormat.format(monthRevenue)} ₽`,
		revenueNote: `${monthName} ${now.getUTCFullYear()} · ${monthCompleted} ${pluralize(monthCompleted, ['ремонт', 'ремонта', 'ремонтов'])}`,
		clientsCount: clients.size,
		totalOrders: orders.length,
		statusCounts
	};
}

function sameUtcDay(a: Date, b: Date): boolean {
	return (
		a.getUTCFullYear() === b.getUTCFullYear() &&
		a.getUTCMonth() === b.getUTCMonth() &&
		a.getUTCDate() === b.getUTCDate()
	);
}

function pluralize(n: number, forms: [string, string, string]): string {
	const mod10 = n % 10;
	const mod100 = n % 100;
	if (mod10 === 1 && mod100 !== 11) return forms[0];
	if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) return forms[1];
	return forms[2];
}

let nextOrderNumber = 1049;

// ─── API functions ──────────────────────────────────────────────────────────

const baseUrl = import.meta.env.VITE_API_URL as string | undefined;
const useMocks = !baseUrl;

async function request<T>(path: string, init?: RequestInit): Promise<T> {
	const response = await fetch(`${baseUrl}${path}`, {
		headers: { 'Content-Type': 'application/json', ...init?.headers },
		...init
	});
	if (!response.ok) {
		throw new ApiError(`API error: ${response.status}`, response.status);
	}
	return (await response.json()) as T;
}

const delay = (ms = 250) => new Promise((resolve) => setTimeout(resolve, ms));

export async function fetchOrders(): Promise<RepairOrderDto[]> {
	if (useMocks) {
		await delay();
		return [...orders];
	}
	return request<RepairOrderDto[]>('/orders');
}

export async function fetchMasterStats(): Promise<MasterStats> {
	if (useMocks) {
		await delay(150);
		return computeMasterStats();
	}
	return request<MasterStats>('/master/stats');
}

export async function fetchOrderByNumber(number: string): Promise<RepairOrderDto> {
	if (useMocks) {
		await delay();
		const order = orders.find((o) => o.number.toLowerCase() === number.toLowerCase());
		if (!order) throw new ApiError('Заявка не найдена', 404);
		return { ...order };
	}
	return request<RepairOrderDto>(`/orders/${encodeURIComponent(number)}`);
}

export async function createOrder(
	payload: CreateOrderPayload
): Promise<{ number: string; status: RepairStatus }> {
	if (useMocks) {
		await delay(600);
		const number = `ПТ-${nextOrderNumber++}`;
		orders.unshift({
			id: crypto.randomUUID(),
			number,
			clientName: 'Новый клиент',
			deviceName: payload.deviceName,
			serviceName: payload.problemId ? problemLabel(payload.problemId) : 'Диагностика',
			price: payload.estimateTotal || null,
			priceFrom: !payload.estimateTotal,
			status: 'Created',
			createdAt: new Date().toISOString(),
			preferredVisitAt: payload.preferredVisitAt
		});
		return { number, status: 'Created' };
	}
	return request<{ number: string; status: RepairStatus }>('/orders', {
		method: 'POST',
		body: JSON.stringify(payload)
	});
}

export async function changeOrderStatus(
	orderId: string,
	next: RepairStatus,
	comment?: string
): Promise<void> {
	if (useMocks) {
		await delay(300);
		const order = orders.find((o) => o.id === orderId);
		if (!order) throw new ApiError('Заявка не найдена', 404);
		const allowed = statusTransitions[order.status];
		if (!allowed.includes(next)) {
			throw new ApiError(`Недопустимый переход статуса: ${statusLabels[order.status]} → ${statusLabels[next]}`, 409);
		}
		order.status = next;
		return;
	}
	await request<void>(`/orders/${orderId}/status`, {
		method: 'PATCH',
		body: JSON.stringify({ status: next, comment })
	});
}

// Problem labels from the mockup (form select) — used by mock createOrder.

function problemLabel(problemId: string): string {
	return problems.find((p) => p.id === problemId)?.label ?? 'Диагностика';
}
