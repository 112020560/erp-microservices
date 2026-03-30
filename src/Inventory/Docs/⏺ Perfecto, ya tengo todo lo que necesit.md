⏺ Perfecto, ya tengo todo lo que necesito. Te explico el flujo completo:

  ---
  Flujo de carga de inventario desde cero

  El microservicio de Inventario no vive solo — depende del microservicio de Catálogos para saber qué productos existen. El flujo tiene 4 pasos obligatorios en orden:

  ---
  Paso 1 — Crear los productos en Catálogos

  (esto ya lo hace el microservicio de Catálogos, no Inventory)

  Cuando creas un producto en Catalogs, este publica automáticamente un evento IProductCreated vía RabbitMQ.

  ---
  Paso 2 — Inventory recibe el evento y crea el ProductSnapshot

  (automático, no requiere llamada manual)

  El consumer ProductCreatedConsumer escucha ese evento y crea un ProductSnapshot en la DB de Inventory. Este snapshot es una copia local del producto con solo los datos que le interesan al inventario: SKU, nombre,
  categoría, marca.

  Catalogs publica IProductCreated
      → ProductCreatedConsumer lo recibe
      → Crea ProductSnapshot con TrackingType=None, MinStock=0, ReorderPoint=0

  El producto existe en Inventory pero sin configuración de inventario todavía.

  ---
  Paso 3 — Configurar el producto para inventario

  (llamada manual: POST /api/v1/catalog/products/{id}/register)

  Con esto le dices al sistema cómo quieres rastrear ese producto:

  {
    "trackingType": 1,    // 0=None, 1=Quantity, 2=Lot
    "minimumStock": 10,   // alerta de stock bajo
    "reorderPoint": 20    // punto de reorden
  }

  ---
  Paso 4 — Crear el almacén

  (llamada manual: POST /api/v1/warehouses)

  {
    "code": "BODEGA-01",
    "name": "Bodega Principal",
    "description": "Almacén central"
  }

  Opcionalmente puedes agregar ubicaciones físicas dentro del almacén (pasillos, estantes, niveles) con POST /api/v1/warehouses/{id}/locations.

  ---
  Paso 5 — Cargar el inventario inicial (Goods Receipt)

  (llamada manual: POST /api/v1/stock/receive)

  Aquí es donde realmente entran las unidades al sistema. Este es el punto clave:

  {
    "warehouseId": "...",
    "reference": "APERTURA-2026",
    "notes": "Inventario inicial",
    "date": "2026-03-30T00:00:00Z",
    "lines": [
      {
        "productId": "...",
        "locationId": "...",
        "lotId": null,
        "quantity": 100,
        "unitCost": 25.50
      }
    ]
  }

  Lo que hace el handler internamente:
  1. Genera un número de movimiento automático (GR-000001)
  2. Por cada línea busca si ya existe un StockEntry para ese producto+almacén+ubicación
  3. Si no existe, lo crea con cantidad 0 y lo inicializa
  4. Llama a stockEntry.ReceiveStock(qty, cost) que suma la cantidad y recalcula el costo promedio
  5. Confirma el movimiento (queda en estado Confirmed, no se puede modificar)
  6. Publica el evento IStockMovementConfirmed
  7. Si la cantidad queda por debajo del mínimo, publica ILowStockDetected

  ---
  Resumen visual del flujo

  [Catalogs] Crear Producto
          ↓ (evento automático vía RabbitMQ)
  [Inventory] ProductSnapshot creado
          ↓ (manual: POST /catalog/products/{id}/register)
  [Inventory] Configurar tracking + min stock + reorder point
          ↓ (manual: POST /warehouses)
  [Inventory] Crear almacén
          ↓ (manual: POST /stock/receive)
  [Inventory] StockEntry creado con cantidad real y costo promedio