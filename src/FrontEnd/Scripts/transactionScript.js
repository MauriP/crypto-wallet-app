const apiUrl = 'https://localhost:7070/api';
let token = sessionStorage.getItem('jwt');
let userId = null;
let saldoPesos = 0;
let saldoCripto = 0;

// Proteger la página
if (!token) {
  window.location.href = "login.html";
}

// Mostrar saldo en pesos
async function mostrarSaldo() {
  try {
    const res = await fetch(`${apiUrl}/UserPesosBalance/${userId}`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error('No se pudo obtener el saldo');
    const data = await res.json();
    saldoPesos = data.pesosBalance ?? data.balance ?? 0;
    document.getElementById('saldoPesos').textContent = saldoPesos.toFixed(2);
  } catch (err) {
    document.getElementById('saldoPesos').textContent = '0.00';
    saldoPesos = 0;
  }
}

// Mostrar saldo de la cripto seleccionada (solo para venta)
async function mostrarSaldoCripto() {
  const operation = document.getElementById('action').value;
  const crypto = document.getElementById('crypto').value;
  const saldoCriptoSpan = document.getElementById('saldoCripto');
  if (operation !== 'sale' || !crypto || !userId) {
    saldoCriptoSpan.textContent = '';
    saldoCripto = 0;
    return;
  }
  try {
    const res = await fetch(`${apiUrl}/Transactions/balance/${userId}/${crypto}`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!res.ok) throw new Error('No se pudo obtener el saldo de la cripto');
    const data = await res.json();
    saldoCripto = data.balance ?? 0;
    saldoCriptoSpan.textContent = `Disponible: ${saldoCripto} ${crypto}`;
  } catch (err) {
    saldoCriptoSpan.textContent = 'Disponible: 0';
    saldoCripto = 0;
  }
}

// Modal para agregar saldo en pesos
function toggleModalSaldo(show) {
  document.getElementById('modalSaldo').style.display = show ? 'flex' : 'none';
  if (!show) document.getElementById('mensajeSaldo').textContent = '';
}

document.addEventListener('DOMContentLoaded', async () => {
  // Obtener usuario actual y mostrar saldo
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
    await mostrarSaldo();
    await mostrarSaldoCripto();
  } catch (err) {
    console.error(err);
    window.location.href = "login.html";
  }

  // Preselección desde sessionStorage
  const preselectedAction = sessionStorage.getItem('preselectedAction');
  const preselectedCrypto = sessionStorage.getItem('preselectedCrypto');
  const preselectedExchange = sessionStorage.getItem('preselectedExchange');

  if (preselectedAction && document.getElementById('action')) {
    document.getElementById('action').value = preselectedAction;
    sessionStorage.removeItem('preselectedAction');
  }
  if (preselectedCrypto && document.getElementById('crypto')) {
    document.getElementById('crypto').value = preselectedCrypto;
    sessionStorage.removeItem('preselectedCrypto');
  }
  if (preselectedExchange && document.getElementById('exchange')) {
    document.getElementById('exchange').value = preselectedExchange;
    sessionStorage.removeItem('preselectedExchange');
  }

  // Si se preseleccionó algo, actualiza los campos dependientes
  if (preselectedAction || preselectedCrypto || preselectedExchange) {
    updateMoneyField();
    if (typeof mostrarSaldoCripto === 'function') mostrarSaldoCripto();
  }

  // Setea fecha/hora local por defecto
  const now = new Date();
  if (document.getElementById('datetime')) {
    document.getElementById('datetime').value = now.toISOString().slice(0, 16);
  }

  // Botón para abrir el modal
  if (document.getElementById('btnAgregarSaldo')) {
    document.getElementById('btnAgregarSaldo').addEventListener('click', () => toggleModalSaldo(true));
  }
  // Botón para cerrar el modal
  if (document.getElementById('cerrarModalSaldo')) {
    document.getElementById('cerrarModalSaldo').addEventListener('click', () => toggleModalSaldo(false));
  }
  // Formulario para agregar saldo
  if (document.getElementById('formAgregarSaldo')) {
    document.getElementById('formAgregarSaldo').addEventListener('submit', async (e) => {
      e.preventDefault();
      const monto = parseFloat(document.getElementById('inputSaldo').value);
      if (isNaN(monto) || monto <= 0) {
        document.getElementById('mensajeSaldo').textContent = 'Ingrese un monto válido.';
        return;
      }
      try {
        const res = await fetch(`${apiUrl}/UserPesosBalance/add`, {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify({ UserId: parseInt(userId), PesosBalance: monto })
});
        if (res.ok) {
          document.getElementById('mensajeSaldo').textContent = 'Saldo cargado correctamente.';
          await mostrarSaldo();
          setTimeout(() => toggleModalSaldo(false), 1000);
        } else {
          const result = await res.json();
          document.getElementById('mensajeSaldo').textContent = result.message || 'Error al cargar saldo.';
        }
      } catch (err) {
        document.getElementById('mensajeSaldo').textContent = 'Error al cargar saldo.';
      }
    });
  }

  // Listeners para el formulario de transacción y saldo cripto
  if (document.getElementById('action')) {
    document.getElementById('action').addEventListener('change', () => {
      updateMoneyField();
      mostrarSaldoCripto();
    });
  }
  if (document.getElementById('crypto')) {
    document.getElementById('crypto').addEventListener('change', () => {
      updateMoneyField();
      mostrarSaldoCripto();
    });
  }
  if (document.getElementById('exchange')) {
    document.getElementById('exchange').addEventListener('change', updateMoneyField);
  }
  if (document.getElementById('amount')) {
    document.getElementById('amount').addEventListener('input', updateMoneyField);
  }
  if (document.getElementById('transactionForm')) {
    document.getElementById('transactionForm').addEventListener('submit', submitTransaction);
  }
});

async function updateMoneyField() {
  const operation = document.getElementById('action').value;
  const crypto = document.getElementById('crypto').value;
  const exchange = document.getElementById('exchange').value;
  const amount = parseFloat(document.getElementById('amount').value);

  const moneyGroup = document.getElementById('moneyGroup');
  const moneyLabel = document.getElementById('moneyLabel');
  const moneyInput = document.getElementById('money');

  if (!operation || !crypto || !exchange || isNaN(amount) || amount <= 0) {
    moneyGroup.style.display = 'none';
    return;
  }

  try {
    const amountStr = amount.toFixed(8);
    const endpoint = `${apiUrl}/Price/total/${operation}/${crypto}/${exchange}?amount=${amountStr}`;
    console.log('endpoint:', endpoint, { operation, crypto, exchange, amount });
    const response = await fetch(endpoint, {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    let data = {};
    try {
      data = await response.json();
    } catch {}

    if (!response.ok) {
      console.error('Respuesta backend:', data);
      throw new Error(data.message || 'Error al obtener total');
    }

    const total = data.total;
    moneyInput.value = total.toFixed(2);
    moneyLabel.textContent = operation === 'purchase'
      ? 'Monto a pagar (ARS)'
      : 'Monto a recibir (ARS)';
    moneyGroup.style.display = 'block';

    // Validación usando saldoPesos para compra y saldoCripto para venta
    if (operation === 'purchase' && total > saldoPesos) {
      moneyLabel.textContent += ' (Saldo insuficiente)';
      moneyLabel.style.color = 'red';
      document.querySelector('button[type="submit"]').disabled = true;
    } else if (operation === 'sale' && amount > saldoCripto) {
      moneyLabel.textContent += ' (No tienes suficiente cripto)';
      moneyLabel.style.color = 'red';
      document.querySelector('button[type="submit"]').disabled = true;
    } else {
      moneyLabel.style.color = '';
      document.querySelector('button[type="submit"]').disabled = false;
    }
  } catch (error) {
    console.error('Error en updateMoneyField:', error);
    moneyGroup.style.display = 'none';
    alert('Error al calcular el monto. Verifique los datos e intente nuevamente.');
    document.querySelector('button[type="submit"]').disabled = true;
  }
}

async function submitTransaction(e) {
  e.preventDefault();

  if (!userId) {
    alert('Usuario no autenticado');
    window.location.href = "login.html";
    return;
  }

  const operation = document.getElementById('action').value;
  const crypto = document.getElementById('crypto').value;
  const exchange = document.getElementById('exchange').value;
  const amount = parseFloat(document.getElementById('amount').value);
  const date = document.getElementById('datetime').value;

  const data = {
    UserId: parseInt(userId),
    CryptoCode: crypto,
    ExchangeCode: exchange,
    Action: operation,
    CryptoAmount: amount,
    DateTime: date
  };

  // LOG para depuración
  console.log("Datos enviados a /api/Transactions:", data);

  try {
    const res = await fetch(`${apiUrl}/Transactions`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify(data)
    });

    let result = {};  
    let errorText = '';
    try {
      result = await res.json();
    } catch {
      try {
        errorText = await res.text();
      } catch {}
    }

    if (res.ok) {
      document.getElementById('mensaje').textContent = 'Transacción guardada correctamente.';
      setTimeout(() => {
        window.location.href = "historial.html";
      }, 1500);
    } else {
      console.error("Respuesta backend:", result, errorText);
      document.getElementById('mensaje').textContent = 'Error: ' + (result.message || errorText || 'No se pudo guardar la transacción');
    }
  } catch (err) {
    document.getElementById('mensaje').textContent = 'Error al guardar la transacción';
    console.error("Error en fetch:", err);
  }
}