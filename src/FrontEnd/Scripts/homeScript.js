const apiUrl = 'https://localhost:7070/api';

const cryptos = ['BTC', 'ETH', 'USDC'];
const exchanges = ['Binance', 'Buenbit', 'SatoshiTango'];

document.addEventListener('DOMContentLoaded', async () => {
  await mostrarPrecios();
});

async function mostrarPrecios() {
  const container = document.getElementById('cryptoCards');
  container.innerHTML = '';

  for (const crypto of cryptos) {
    for (const exchange of exchanges) {
      let buy = '-';
      let sell = '-';
      try {
        // Fetch precio de compra
        const resBuy = await fetch(`${apiUrl}/Price/buy/${crypto}/${exchange}`);
        if (resBuy.ok) {
          const dataBuy = await resBuy.json();
          buy = dataBuy.price ?? '-';
        }
        // Fetch precio de venta
        const resSell = await fetch(`${apiUrl}/Price/sell/${crypto}/${exchange}`);
        if (resSell.ok) {
          const dataSell = await resSell.json();
          sell = dataSell.price ?? '-';
        }

        container.innerHTML += `
  <div class="crypto-card">
    <div class="crypto-header">
      <span class="crypto-symbol">${crypto}</span>
      <span class="exchange">${exchange}</span>
    </div>
    <div class="crypto-body">
      <div class="price-row">
        <span>Compra:</span>
        <span class="price-buy">$${buy}</span>
      </div>
      <div class="price-row">
        <span>Venta:</span>
        <span class="price-sell">$${sell}</span>
      </div>
      <div class="card-actions">
        <button class="btn-comprar" data-crypto="${crypto}" data-exchange="${exchange}">Comprar</button>
        <button class="btn-vender" data-crypto="${crypto}" data-exchange="${exchange}">Vender</button>
      </div>
    </div>
  </div>
`;
      } catch (err) {
        container.innerHTML += `
          <div class="crypto-card unavailable">
            <div class="crypto-header">
              <span class="crypto-symbol">${crypto}</span>
              <span class="exchange">${exchange}</span>
            </div>
            <div class="crypto-body">
              <div class="price-row">Precios no disponibles</div>
            </div>
          </div>
        `;
      }
    }
  }
  
setTimeout(() => {
  document.querySelectorAll('.btn-comprar').forEach(btn => {
    btn.addEventListener('click', (e) => {
      const crypto = btn.getAttribute('data-crypto');
      const exchange = btn.getAttribute('data-exchange');
      // Guardar selección en sessionStorage y redirigir
      sessionStorage.setItem('preselectedAction', 'purchase');
      sessionStorage.setItem('preselectedCrypto', crypto);
      sessionStorage.setItem('preselectedExchange', exchange);
      window.location.href = 'transaction.html';
    });
  });
  document.querySelectorAll('.btn-vender').forEach(btn => {
    btn.addEventListener('click', (e) => {
      const crypto = btn.getAttribute('data-crypto');
      const exchange = btn.getAttribute('data-exchange');
      sessionStorage.setItem('preselectedAction', 'sale');
      sessionStorage.setItem('preselectedCrypto', crypto);
      sessionStorage.setItem('preselectedExchange', exchange);
      window.location.href = 'transaction.html';
    });
  });
}, 100); 
}