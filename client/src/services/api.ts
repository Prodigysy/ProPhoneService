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

// Master panel counters from the mockup (until the backend provides real stats).
export const masterStats = {
	todayCount: 12,
	todayDelta: '+3 к предыдущему дню',
	inWork: 8,
	inWorkNote: 'Ещё 3 — на диагностике',
	revenueMonth: '24 890 ₽',
	revenueNote: 'Октябрь 2026 · 4 ремонта',
	clientsCount: 12,
	totalOrders: 57
};

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
