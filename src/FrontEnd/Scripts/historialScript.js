const apiUrl = 'https://localhost:7070/api';
let token = sessionStorage.getItem('jwt');
let userId = null;

// Proteger la página
if (!token) {
  window.location.href = "login.html";
}

document.addEventListener('DOMContentLoaded', async () => {
  // Obtener usuario actual
  try {
    const res = await fetch(`${apiUrl}/User/me`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) {
      window.location.href = "login.html";
      return;
    }
    const result = await res.json();
    userId = result.userId;
    await cargarHistorial();
  } catch (err) {
    console.error(err);
    window.location.href = "login.html";
  }
});

async function cargarHistorial() {
  const tabla = document.querySelector('#tablaHistorial tbody');
  const mensaje = document.getElementById('mensaje');
  tabla.innerHTML = '';
  mensaje.textContent = '';

  try {
    const res = await fetch(`${apiUrl}/Transactions/user/${userId}`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error('No se pudo obtener el historial');
    const transacciones = await res.json();

    if (transacciones.length === 0) {
      mensaje.textContent = 'No hay transacciones registradas.';
      return;
    }

    transacciones.forEach(tx => {
    const montoClass = tx.action === 'purchase' ? 'monto-compra' : 'monto-venta';
  const fila = document.createElement('tr');
  fila.innerHTML = `
    <td>${new Date(tx.dateTime).toLocaleString()}</td>
    <td>${tx.action === 'purchase' ? 'Compra' : 'Venta'}</td>
    <td>${tx.cryptoCode}</td>
    <td>${tx.exchangeCode}</td>
    <td>${tx.cryptoAmount}</td>
    <td>${tx.action === 'purchase' ? '-' : '+'}${parseFloat(tx.money).toFixed(2)}</td>
  `;
  tabla.appendChild(fila);
});
  } catch (err) {
    mensaje.textContent = 'Error al cargar el historial.';
  }
}