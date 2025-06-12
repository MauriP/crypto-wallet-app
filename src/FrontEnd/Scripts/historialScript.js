const apiUrl = 'https://localhost:7070/api';
let token = sessionStorage.getItem('jwt');
let userId = null;

// Proteger la página
if (!token) {
  window.location.href = "login.html";
}

document.addEventListener('DOMContentLoaded', async () => {
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
      const fila = document.createElement('tr');
      fila.dataset.txId = tx.id;
      fila.innerHTML = `
        <td>${new Date(tx.dateTime).toLocaleString()}</td>
        <td>${tx.action === 'purchase' ? 'Compra' : 'Venta'}</td>
        <td>${tx.cryptoCode}</td>
        <td>${tx.cryptoAmount}</td>
      `;
      fila.addEventListener('click', () => mostrarDetallesTransaccion(tx.id));
      tabla.appendChild(fila);
    });

  } catch (err) {
    mensaje.textContent = 'Error al cargar el historial.';
  }
}

async function mostrarDetallesTransaccion(id) {
  try {
    const res = await fetch(`${apiUrl}/Transactions/${id}`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error('Error al obtener detalles');
    const tx = await res.json();

    document.getElementById('detalleFecha').textContent = new Date(tx.dateTime).toLocaleString();
    document.getElementById('detalleTipo').textContent = tx.action === 'purchase' ? 'Compra' : 'Venta';
    document.getElementById('detalleCripto').textContent = tx.cryptoCode;
    document.getElementById('detalleExchange').textContent = tx.exchangeCode;
    document.getElementById('detalleCantidad').textContent = tx.cryptoAmount;
    document.getElementById('detalleMonto').textContent = parseFloat(tx.money).toFixed(2);

    document.getElementById('modalTransaccion').style.display = 'block';

  } catch (err) {
    alert('Error al cargar los detalles de la transacción.');
  }
}

// Cierre del modal
document.querySelector('.cerrar').onclick = function () {
  document.getElementById('modalTransaccion').style.display = 'none';
};

window.onclick = function (event) {
  const modal = document.getElementById('modalTransaccion');
  if (event.target === modal) {
    modal.style.display = 'none';
  }
};
