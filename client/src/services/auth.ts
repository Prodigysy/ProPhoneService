// Auth service — registration/login per sprint-2 card 1:
//   POST /api/auth/register, POST /api/auth/login
// JWT access token + refresh stored in an HttpOnly cookie by the backend,
// so the browser attaches it automatically; the frontend keeps only the
// profile of the signed-in user.
//
// With VITE_API_URL unset (mock mode) any phone/email and a password of
// 6+ characters signs in as the demo client.

import { ApiError, isMockMode, request } from './api.ts';

export interface SessionUser {
	id: string;
	name: string;
	phone: string | null;
	email: string | null;
}

const delay = (ms = 250) => {
	const { promise, resolve } = Promise.withResolvers<void>();
	setTimeout(resolve, ms);
	return promise;
};

/** Which identity channel the credentials use. */
export function identityKind(identity: string): 'phone' | 'email' {
	return identity.includes('@') ? 'email' : 'phone';
}

// Demo lockout: 5 failed attempts block sign-in for 15 minutes, mirroring
// the backend contract from card 1.
const LOCKOUT_LIMIT = 5;
const LOCKOUT_MS = 15 * 60 * 1000;
let mockFails = 0;
let mockLockedUntil = 0;

function assertMockNotLocked(): void {
	if (mockLockedUntil > Date.now()) {
		const minutes = Math.ceil((mockLockedUntil - Date.now()) / 60000);
		throw new ApiError(`Слишком много попыток. Вход заблокирован на ${minutes} мин.`, 429);
	}
}

export async function login(identity: string, password: string): Promise<SessionUser> {
	if (isMockMode) return mockLogin(identity, password);
	// The auth endpoints land with sprint-2 card 1. Until then a 404 here means
	// "not implemented" — fall back to the demo session instead of a dead wall.
	try {
		return await request<SessionUser>('/auth/login', {
			method: 'POST',
			body: JSON.stringify({ identity, password })
		});
	} catch (error) {
		if (error instanceof ApiError && error.status === 404) return mockLogin(identity, password);
		throw error;
	}
}

async function mockLogin(identity: string, password: string): Promise<SessionUser> {
	await delay(350);
	assertMockNotLocked();
	if (!identity.trim() || password.length < 6) {
		mockFails += 1;
		if (mockFails >= LOCKOUT_LIMIT) mockLockedUntil = Date.now() + LOCKOUT_MS;
		throw new ApiError('Неверный телефон/почта или пароль.', 401);
	}
	mockFails = 0;
	return mockUserFrom(identity, 'Анна Смирнова');
}

function mockUserFrom(identity: string, name: string): SessionUser {
	const kind = identityKind(identity);
	return {
		id: 'a1f3c2d4-0000-4a01-9c11-000000000000',
		name,
		phone: kind === 'phone' ? identity : null,
		email: kind === 'email' ? identity : null
	};
}

export async function register(
	name: string,
	identity: string,
	password: string
): Promise<SessionUser> {
	if (isMockMode) return mockRegister(name, identity, password);
	try {
		return await request<SessionUser>('/auth/register', {
			method: 'POST',
			body: JSON.stringify({ name, identity, password })
		});
	} catch (error) {
		if (error instanceof ApiError && error.status === 404) return mockRegister(name, identity, password);
		throw error;
	}
}

async function mockRegister(name: string, identity: string, password: string): Promise<SessionUser> {
	await delay(350);
	assertMockNotLocked();
	if (!name.trim() || !identity.trim() || password.length < 6) {
		throw new ApiError('Проверьте поля: имя, телефон/почта и пароль от 6 символов.', 422);
	}
	return { ...mockUserFrom(identity, name), id: crypto.randomUUID() };
}

export async function logout(): Promise<void> {
	if (isMockMode) {
		await delay(150);
		return;
	}
	// Clearing the refresh cookie is backend-side; ignore network failures.
	try {
		await request<void>('/auth/logout', { method: 'POST' });
	} catch {
		// Session is cleared locally regardless.
	}
}

/** Returns the signed-in user for a valid refresh cookie, otherwise null. */
export async function me(): Promise<SessionUser | null> {
	if (isMockMode) return null;
	try {
		return await request<SessionUser>('/auth/me');
	} catch (error) {
		if (error instanceof ApiError && (error.status === 401 || error.status === 404)) return null;
		throw error;
	}
}
