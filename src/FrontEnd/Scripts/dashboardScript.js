const apiUrl = 'https://localhost:7070/api';
const token = sessionStorage.getItem('jwt');
let userId = null;

document.addEventListener('DOMContentLoaded', async () => {
  if (!token) {
    document.getElementById('mensaje').textContent = 'Debes iniciar sesión para ver tu wallet.';
    return;
  }

  try {
    // Obtener userId desde el backend usando el token
    const resUser = await fetch(`${apiUrl}/User/me`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!resUser.ok) throw new Error('No autenticado');
    const userData = await resUser.json();
    userId = userData.userId;

    // Ahora sí, pedimos el balance
    const res = await fetch(`${apiUrl}/UserBalance/${userId}`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error('No se pudo obtener el análisis de tu wallet');
    const data = await res.json();

    const tbody = document.getElementById('wallet-tbody');
    tbody.innerHTML = '';
    let total = 0;

    if (data.cryptoBalances && data.cryptoBalances.length > 0) {
      data.cryptoBalances.forEach(c => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
          <td>${c.cryptoCode.toUpperCase()}</td>
          <td>${c.cryptoAmount}</td>
          <td>$${Number(c.currentPrice).toLocaleString('es-AR', {maximumFractionDigits:2})}</td>
          <td>$${Number(c.valueInARS).toLocaleString('es-AR', {maximumFractionDigits:2})}</td>
        `;
        tbody.appendChild(tr);
        total += Number(c.valueInARS);
      });
      document.getElementById('total-ars').textContent = `$${total.toLocaleString('es-AR', {maximumFractionDigits:2})}`;
    } else {
      tbody.innerHTML = `<tr><td colspan="4">No tienes criptomonedas en tu wallet.</td></tr>`;
      document.getElementById('total-ars').textContent = '$0.00';
    }
  } catch (err) {
    document.getElementById('mensaje').textContent = 'Error al cargar el análisis de tu wallet.';
    console.error(err);
  }
});