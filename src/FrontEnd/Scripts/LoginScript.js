const apiUrl = 'https://localhost:7070/api';

// Alternar entre login y registro
function toggleForms() {
  const loginForm = document.getElementById('loginForm');
  const registerForm = document.getElementById('registerForm');
  const title = document.getElementById('formTitle');
  const toggleText = document.getElementById('toggleText');

  if (loginForm.classList.contains('active')) {
    loginForm.classList.remove('active');
    registerForm.classList.add('active');
    title.textContent = 'Regístrate';
    toggleText.innerHTML = '¿Ya tienes una cuenta? <a onclick="toggleForms()">Inicia sesión aquí</a>';
  } else {
    registerForm.classList.remove('active');
    loginForm.classList.add('active');
    title.textContent = 'Inicia sesión';
    toggleText.innerHTML = '¿No tienes una cuenta? <a onclick="toggleForms()">Regístrate aquí</a>';
  }
}

// --- FUNCIONES MODULARIZADAS PARA TESTING ---

async function registrarUsuario(data) {
  const res = await fetch(`${apiUrl}/User/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data)
  });
  const result = await res.json();
  return { ok: res.ok, result };
}

async function loginUsuario(data) {
  const res = await fetch(`${apiUrl}/User/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data)
  });
  const result = await res.json();
  return { ok: res.ok, result };
}

// --- EVENT LISTENERS USANDO LAS FUNCIONES MODULARIZADAS ---

const registerForm = document.getElementById('registerForm');
if (registerForm) {
  registerForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    const form = e.target;
    if (form.password.value.length < 8) {
      alert('La contraseña debe tener al menos 8 caracteres.');
      return;
    }
    const data = {
      username: form.username.value,
      email: form.email.value,
      passwordHash: form.password.value,
      createdAt: new Date().toISOString()
    };

    try {
      const res = await registrarUsuario(data);
      if (res.ok) {
        alert('Usuario registrado con ID: ' + res.result.id);
        toggleForms();
      } else {
        alert('Error: ' + (res.result.message || JSON.stringify(res.result)));
      }
    } catch (err) {
      console.error(err);
      alert('Error al registrar usuario: ' + err);
    }
  });
}

const loginForm = document.getElementById('loginForm');
if (loginForm) {
  loginForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    const form = e.target;

    const data = {
      username: form.username.value,
      passwordHash: form.password.value
    };

    try {
      const res = await loginUsuario(data);
      if (res.ok) {
        sessionStorage.setItem('jwt', res.result.token);
        window.location.href = "index.html";
      } else {
        alert(res.result.message || 'Error al iniciar sesión');
      }
    } catch (err) {
      console.error(err);
      alert('Error al iniciar sesión');
    }
  });
}

function togglePassword(inputId, eye) {
  const input = document.getElementById(inputId);
  if (input.type === "password") {
    input.type = "text";
    eye.textContent = "🙈";
  } else {
    input.type = "password";
    eye.textContent = "👁️";
  }
}

// Exportar funciones para testing
if (typeof module !== 'undefined') {
  module.exports = { toggleForms, registrarUsuario, loginUsuario };
}