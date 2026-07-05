# Comparador de precios de criptomonedas

Aplicación web que consulta el precio de una criptomoneda en distintos
exchanges (por ejemplo Binance) para identificar dónde conviene comprar
o vender. Permite simular operaciones de compra/venta y calcula el
balance del portfolio a partir del historial de transacciones, sin
ejecutar operaciones reales.

## Motivación

Comparar manualmente el precio de una cripto entre varios exchanges es
lento y hay que abrir varias pestañas. Esta app centraliza esa consulta
y además permite simular una cartera para ver cómo hubiera evolucionado
sin arriesgar dinero real.

## Funcionalidades

- Consulta de precios de criptomonedas en tiempo real vía la API de
  [CryptoYa](https://criptoya.com/api).
- Comparación de precio de compra/venta entre distintos exchanges para
  la misma criptomoneda.
- Simulación de operaciones de compra y venta (no ejecuta transacciones
  reales, solo las registra).
- Cálculo dinámico del balance del portfolio: en vez de guardar un
  campo de saldo fijo, el balance se calcula sumando compras y restando
  ventas del historial completo (ver sección Base de datos).
- Historial de transacciones por usuario.

## Stack

- **Backend:** C# / .NET
- **Frontend:** HTML, CSS, JavaScript (sin framework)
- **Base de datos:** MySQL
- **API externa:** CryptoYa (precios de criptomonedas por exchange)

## Base de datos

El modelo separa criptomonedas, exchanges y precios en tablas
normalizadas, y calcula el estado de la cartera mediante vistas SQL en
vez de un campo de balance que podría desincronizarse:

- `users`, `cryptocurrencies`, `exchanges`: catálogos base.
- `crypto_prices`: último precio de compra/venta por exchange y
  criptomoneda.
- `transactions`: historial de operaciones simuladas (compra/venta),
  con restricción `crypto_amount > 0` a nivel de base de datos.
- `wallet_status` (vista): tenencia actual de cada usuario por
  criptomoneda, calculada sumando compras y restando ventas.
- `v_wallet_summary` (vista): valor actual del portfolio, cruzando
  `wallet_status` con el último precio disponible.
- `v_transaction_history` (vista): historial de transacciones con los
  nombres de criptomoneda y exchange ya resueltos (sin necesidad de
  hacer los JOIN desde la aplicación).

## Cómo ejecutar localmente

> ⚠️ Completar esta sección con la estructura real del proyecto — no
> pude inspeccionar el contenido de `src/` para confirmar nombres
> exactos de archivos y carpetas.

### Requisitos

- .NET SDK (verificar versión en el `.csproj` dentro de `src/`)
- MySQL Server accesible localmente
- Una cuenta / acceso a la API de CryptoYa (verificar si requiere API key)

### Pasos

1. Cloná el repositorio:
```bash
   git clone https://github.com/MauriP/crypto-wallet-app.git
   cd crypto-wallet-app
```
2. Creá la base de datos en MySQL y ejecutá el script de creación de
   tablas src/BaseDeDatos.txt
3. Configurá la cadena de conexión a MySQL 
   `appsettings.json`
4. Restauré dependencias y corré el proyecto:
```bash
   cd src/<NOMBRE_DEL_PROYECTO>
   dotnet restore
   dotnet run
```
5. Abrí `http://localhost:<PUERTO>` en el navegador (completar puerto
   real).

## Estado del proyecto

Proyecto individual, funcional, corre en entorno local. Aún no
desplegado en un hosting público.
