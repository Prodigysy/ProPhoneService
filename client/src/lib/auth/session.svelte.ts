import { browser } from '$app/env';
import * as auth from '../../services/auth.ts';
import type { SessionUser } from '../../services/auth.ts';

let current = $state<SessionUser | null>(null);
let restored = $state(false);

const STORAGE_KEY = 'pps-user';

function persist(user: SessionUser | null): void {
	if (!browser) return;
	if (user) localStorage.setItem(STORAGE_KEY, JSON.stringify(user));
	else localStorage.removeItem(STORAGE_KEY);
}

async function restoreOnce(): Promise<void> {
	if (restored) return;
	restored = true;
	// Mock/localStorage first: keeps the session across reloads without a backend.
	if (browser) {
		const raw = localStorage.getItem(STORAGE_KEY);
		if (raw) {
			try {
				current = JSON.parse(raw) as SessionUser;
			} catch {
				localStorage.removeItem(STORAGE_KEY);
			}
		}
	}
	if (current) return;
	try {
		current = await auth.me();
		persist(current);
	} catch {
		// Backend unreachable — stay signed out.
	}
}

export const session = {
	/** Signed-in user, or null. Reactive. */
	get user(): SessionUser | null {
		return current;
	},
	/** True after the initial restore attempt (guards wait for it). */
	get ready(): boolean {
		return restored;
	},
	login(identity: string, password: string): Promise<SessionUser> {
		return auth.login(identity, password).then((user) => {
			current = user;
			persist(user);
			return user;
		});
	},
	register(name: string, identity: string, password: string): Promise<SessionUser> {
		return auth.register(name, identity, password).then((user) => {
			current = user;
			persist(user);
			return user;
		});
	},
	async logout(): Promise<void> {
		await auth.logout();
		current = null;
		persist(null);
	}
};

/** Call once from the root layout so a reload keeps the session. */
export function initSession(): void {
	if (!restored) void restoreOnce();
}

/** Clears the local session (e.g. on 401 from a protected API). */
export function dropSession(): void {
	current = null;
	persist(null);
}
