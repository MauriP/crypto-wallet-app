// Proteger la página: si no hay token, redirige a login.html
if (!sessionStorage.getItem('jwt')) {
  window.location.href = "login.html";
}

// Función para cerrar sesión
function logout() {
  sessionStorage.removeItem('jwt');
  window.location.href = "login.html";
}
