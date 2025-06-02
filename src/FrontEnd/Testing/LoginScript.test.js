/**
 * @jest-environment jsdom
 */
const { loginUsuario, registrarUsuario, toggleForms } = require('../Scripts/LoginScript');

beforeAll(() => {
  global.fetch = require('jest-fetch-mock');
});

describe('loginUsuario', () => {
  beforeEach(() => {
    fetch.resetMocks();
  });

  test('login exitoso', async () => {
    fetch.mockResponseOnce(JSON.stringify({ token: 'abc123' }), { status: 200 });
    const data = { username: 'test', passwordHash: '12345678' };
    const res = await loginUsuario(data);
    expect(res.ok).toBe(true);
    expect(res.result.token).toBe('abc123');
  });

  test('login con error', async () => {
    fetch.mockResponseOnce(JSON.stringify({ message: 'Credenciales inválidas' }), { status: 401 });
    const data = { username: 'test', passwordHash: 'wrongpass' };
    const res = await loginUsuario(data);
    expect(res.ok).toBe(false);
    expect(res.result.message).toBe('Credenciales inválidas');
  });
});

describe('registrarUsuario', () => {
  beforeEach(() => {
    fetch.resetMocks();
  });

  test('registro exitoso', async () => {
    fetch.mockResponseOnce(JSON.stringify({ id: 1 }), { status: 200 });
    const data = { username: 'nuevo', email: 'nuevo@mail.com', passwordHash: '12345678', createdAt: '2025-01-01T00:00:00Z' };
    const res = await registrarUsuario(data);
    expect(res.ok).toBe(true);
    expect(res.result.id).toBe(1);
  });

  test('registro con error', async () => {
    fetch.mockResponseOnce(JSON.stringify({ message: 'Usuario ya existe' }), { status: 400 });
    const data = { username: 'nuevo', email: 'nuevo@mail.com', passwordHash: '12345678', createdAt: '2025-01-01T00:00:00Z' };
    const res = await registrarUsuario(data);
    expect(res.ok).toBe(false);
    expect(res.result.message).toBe('Usuario ya existe');
  });
});

describe('toggleForms', () => {
  beforeEach(() => {
    document.body.innerHTML = `
      <form id="loginForm" class="active"></form>
      <form id="registerForm"></form>
      <h2 id="formTitle"></h2>
      <div id="toggleText"></div>
    `;
  });

  test('debe alternar de login a registro', () => {
    toggleForms();
    expect(document.getElementById('loginForm').classList.contains('active')).toBe(false);
    expect(document.getElementById('registerForm').classList.contains('active')).toBe(true);
    expect(document.getElementById('formTitle').textContent).toBe('Regístrate');
  });

  test('debe alternar de registro a login', () => {
    toggleForms(); // a registro
    toggleForms(); // a login
    expect(document.getElementById('loginForm').classList.contains('active')).toBe(true);
    expect(document.getElementById('registerForm').classList.contains('active')).toBe(false);
    expect(document.getElementById('formTitle').textContent).toBe('Inicia sesión');
  });
});