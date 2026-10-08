export interface Toast {
	id: number;
	text: string;
	kind: 'info' | 'error';
}

let toasts = $state<Toast[]>([]);
let nextId = 0;

export const toastList = {
	get value(): Toast[] {
		return toasts;
	}
};

export function showToast(text: string, kind: Toast['kind'] = 'info'): void {
	const id = ++nextId;
	toasts.push({ id, text, kind });
	setTimeout(() => {
		toasts = toasts.filter((t) => t.id !== id);
	}, 3200);
}
