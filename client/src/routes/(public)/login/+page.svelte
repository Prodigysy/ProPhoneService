<script lang="ts">
	import { goto } from '$app/navigation';
	import { page } from '$app/state';
	import Button from '#lib/components/Button.svelte';
	import Eyebrow from '#lib/components/Eyebrow.svelte';
	import { session } from '#lib/auth/session.svelte.ts';
	import { isMockMode } from '../../../services/api.ts';
	import { ApiError } from '../../../services/api.ts';

	// ?next=/account after an expired session guard redirect.
	const next = $derived(page.url.searchParams.get('next') ?? '/account');

	let mode = $state<'login' | 'register'>('login');
	let identity = $state('');
	let name = $state('');
	let password = $state('');
	let formError = $state('');
	let fieldError = $state<'identity' | 'name' | 'password' | null>(null);
	let busy = $state(false);

	function validate(): boolean {
		fieldError = null;
		formError = '';
		if (mode === 'register' && !name.trim()) {
			fieldError = 'name';
			formError = 'Укажите имя.';
			return false;
		}
		if (!identity.trim()) {
			fieldError = 'identity';
			formError = 'Укажите телефон или почту.';
			return false;
		}
		if (password.length < 6) {
			fieldError = 'password';
			formError = 'Пароль — минимум 6 символов.';
			return false;
		}
		return true;
	}

	async function submit(event: SubmitEvent): Promise<void> {
		event.preventDefault();
		if (busy || !validate()) return;
		busy = true;
		formError = '';
		try {
			if (mode === 'login') await session.login(identity.trim(), password);
			else await session.register(name.trim(), identity.trim(), password);
			goto(next);
		} catch (error) {
			fieldError = null;
			formError = error instanceof ApiError ? error.message : 'Не удалось выполнить запрос. Попробуйте ещё раз.';
		} finally {
			busy = false;
		}
	}

	function switchMode(): void {
		mode = mode === 'login' ? 'register' : 'login';
		formError = '';
		fieldError = null;
		password = '';
	}
</script>

<section class="container section auth">
	<div class="card">
		<Eyebrow>{mode === 'login' ? 'Личный кабинет' : 'Регистрация'}</Eyebrow>
		<h1 class="title">{mode === 'login' ? 'Вход в кабинет' : 'Создать аккаунт'}</h1>
		<p class="lead">
			{mode === 'login'
				? 'История ремонтов, статусы и цены — в одном месте.'
				: 'Один аккаунт — и все ваши ремонты всегда под рукой.'}
		</p>

		<div class="tabs" role="tablist" aria-label="Вход или регистрация">
			<button class="tab" class:tab--active={mode === 'login'} type="button" role="tab" aria-selected={mode === 'login'} onclick={() => mode !== 'login' && switchMode()}>
				Вход
			</button>
			<button class="tab" class:tab--active={mode === 'register'} type="button" role="tab" aria-selected={mode === 'register'} onclick={() => mode !== 'register' && switchMode()}>
				Регистрация
			</button>
		</div>

		<form onsubmit={submit} novalidate>
			{#if mode === 'register'}
				<div class="field">
					<label class="label" for="name">Ваше имя</label>
					<input
						id="name"
						class="input"
						class:input--error={fieldError === 'name'}
						type="text"
						placeholder="Анна Смирнова"
						bind:value={name}
					/>
				</div>
			{/if}
			<div class="field">
				<label class="label" for="identity">Телефон или почта</label>
				<input
					id="identity"
					class="input"
					class:input--error={fieldError === 'identity'}
					type="text"
					inputmode="email"
					placeholder="+7 (916) 234-56-78 или anna@mail.ru"
					bind:value={identity}
				/>
			</div>
			<div class="field">
				<label class="label" for="password">Пароль</label>
				<input
					id="password"
					class="input"
					class:input--error={fieldError === 'password'}
					type="password"
					placeholder="Минимум 6 символов"
					bind:value={password}
				/>
			</div>

			{#if formError}
				<p class="form-error" role="alert">{formError}</p>
			{/if}

			<div class="submit">
				<Button type="submit" size="lg" disabled={busy}>
					{busy ? 'Отправляем…' : mode === 'login' ? 'Войти' : 'Зарегистрироваться'}
				</Button>
				<button class="switch" type="button" onclick={switchMode}>
					{mode === 'login' ? 'Нет аккаунта? Зарегистрируйтесь' : 'Уже есть аккаунт? Войдите'}
				</button>
			</div>
		</form>

		{#if isMockMode}
			<p class="demo-note">Демо-режим: подойдёт любой телефон или почта и пароль от 6 символов.</p>
		{/if}
	</div>
</section>

<style>
	.auth {
		display: flex;
		justify-content: center;
		padding-block: 48px 72px;
	}

	.card {
		max-width: 480px;
		width: 100%;
		background: var(--surface);
		border: 1px solid var(--border);
		border-radius: 24px;
		padding: 40px;
	}

	.title {
		font-size: 26px;
		font-weight: 700;
		letter-spacing: -0.02em;
	}

	.lead {
		color: var(--muted);
		font-size: 14px;
		line-height: 1.6;
		margin-top: 8px;
	}

	.tabs {
		display: inline-flex;
		background: var(--bg);
		border: 1px solid var(--border);
		border-radius: 999px;
		padding: 4px;
		gap: 4px;
		margin-block: 24px 8px;
	}

	.tab {
		font-size: 13px;
		font-weight: 500;
		color: var(--muted);
		padding: 8px 18px;
		border-radius: 999px;
	}

	.tab--active {
		background: var(--surface);
		color: var(--text);
		box-shadow: var(--shadow-card);
	}

	.field {
		display: flex;
		flex-direction: column;
		gap: 6px;
		margin-top: 16px;
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

	.form-error {
		margin-top: 14px;
		font-size: 14px;
		color: var(--danger);
		background: var(--danger-soft);
		border-radius: 10px;
		padding: 10px 14px;
	}

	.submit {
		margin-top: 20px;
		display: flex;
		align-items: center;
		gap: 16px;
		flex-wrap: wrap;
	}

	.switch {
		font-size: 13px;
		color: var(--accent);
	}

	.switch:hover {
		text-decoration: underline;
		text-underline-offset: 3px;
	}

	.demo-note {
		margin-top: 18px;
		font-size: 12px;
		color: var(--muted);
	}

	@media (max-width: 480px) {
		.card {
			padding: 28px 20px;
		}
	}
</style>
