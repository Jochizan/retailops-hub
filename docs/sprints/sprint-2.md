# Sprint 2 — Orders & Reliability Core

**Sprint Goal:** Crear órdenes **confiables** (transacción + OCC + idempotencia), con **auditoría** y **outbox** funcionando.

**Branch:** `feat/sprint-2-orders-reliability`
**Convención commits:** `feat:`, `fix:`, `test:`, `docs:`, `chore:`
**DoD global (Sprint 2):**

* ✅ Tests críticos pasan
* ✅ Swagger actualizado
* ✅ Errores 409/400 bien modelados
* ✅ Seed data para demo
* ✅ Demo script reproducible

---

## Epic: Orders

### Issue S2-01 — Crear entidades y migración: Orders + OrderItems

**Checklist**

* [x] Crear entidades `Order`, `OrderItem`
* [x] Crear enums: `OrderStatus` (`draft`, `reserved`, `confirmed`, `cancelled`)
* [x] Configurar Fluent API (FKs, índices)
* [x] Migración EF Core + update DB

**AC**

* `Orders` y `OrderItems` existen y se relacionan por FK
* Índice por `store_id` y `status`, y por fecha si usas `created_at`

**Notas pro**

* `Order.Total` decimal(18,2)
* `OrderItem.UnitPrice` decimal(18,2)

---

### Issue S2-02 — DTOs + Validaciones (Request/Response)

**Checklist**

* [x] `CreateOrderRequest` (storeId, items[{skuId, qty}])
* [x] `CreateOrderResponse` (orderId, status, reservedSummary)
* [x] Validaciones: qty > 0, items no vacíos, storeId válido
* [ ] Middleware/Filter para respuestas consistentes de error

**AC**

* Request inválido → 400 con mensaje claro y campo responsable

---

### Issue S2-03 — Servicio de dominio: Reserva de stock (transaccional)

**Checklist**

* [x] Crear `OrdersService.CreateOrderAsync`
* [x] Abrir transacción DB
* [x] Insert `Order` + `OrderItems`
* [x] Por cada item: cargar `Inventory` (storeId+skuId)
* [x] Validar `available = onHand - reserved >= qty`
* [x] Actualizar `reserved += qty`
* [x] Commit

**AC**

* Si un item falla por stock → rollback total (no orden parcial)
* Retorna 201 con estado `reserved`

**Errores**

* Stock insuficiente → 409 `INSUFFICIENT_STOCK`

---

### Issue S2-04 — OCC: Concurrency handling en Inventory

**Checklist**

* [x] Confirmar `Inventory.RowVersion` está como `IsRowVersion()` y concurrency token
* [x] Manejar `DbUpdateConcurrencyException` en CreateOrder
* [x] Error response 409 `CONCURRENCY_CONFLICT` con sugerencia “retry”

**AC**

* Dos requests simultáneos por la última unidad → uno gana, otro 409

---

## Epic: Idempotency

### Issue S2-05 — Tabla Idempotency Keys + modelo

**Checklist**

* [x] Tabla `idempotency_keys`:

  * key (unique)
  * route/method
  * request_hash
  * response_json
  * status_code
  * created_at
* [x] EF entity + migración

**AC**

* Unique constraint en `key`
* Guardar response snapshot

---

### Issue S2-06 — Middleware/Filter Idempotency en `POST /orders`

**Checklist**

* [x] Requerir header `Idempotency-Key`
* [x] Calcular `request_hash` (body + path + user)
* [x] Si existe key:

  * [x] si hash coincide → devolver response guardada
  * [x] si hash difiere → 409 `IDEMPOTENCY_KEY_REUSE_CONFLICT`
* [x] Si no existe:

  * [x] reservar key como “in_progress”
  * [x] ejecutar handler
  * [x] guardar response (success o error)
  * [x] marcar completed

**AC**

* Reintentos con misma key → no duplica orden
* Misma key con body distinto → 409

**Tip (senior)**

* En caso de crash, la key queda “in_progress”: define TTL o allow retry después de X segundos.

---

## Epic: Audit

### Issue S2-07 — Audit Interceptor (simple pero robusto)

**Checklist**

* [x] Implementar `SaveChangesInterceptor` o override `SaveChangesAsync`
* [x] Capturar Added/Modified/Deleted
* [x] Guardar diff JSON (before/after) de props escalares
* [x] Excluir tablas `audit_logs` y `outbox_events` para evitar recursión

**AC**

* Ajuste de inventory y creación de orden generan audit logs
* Diff no incluye RowVersion

---

## Epic: Outbox

### Issue S2-08 — Emitir eventos Outbox dentro de transacción

**Checklist**

* [x] Cuando se crea orden → insertar `outbox_events` (`type=order_created`)
* [x] Si `available <= reorderPoint` → insertar `stock_low`
* [x] Asegurar que el insert al outbox ocurre **antes del commit** de la transacción

**AC**

* Si rollback orden → no se inserta evento outbox (0 eventos)
* Si commit → hay evento `order_created`

---

### Issue S2-09 — Worker Outbox (BackgroundService)

**Checklist**

* [x] BackgroundService: poll cada N segundos
* [x] Seleccionar lote de eventos `pending` donde `next_retry_at` <= now
* [x] Marcar `processing`
* [x] “Procesar” (por ahora: log + marcar `sent`)
* [x] Manejo de error: `attempts++`, set `next_retry_at`, status `failed` si excede max

**AC**

* Eventos pending pasan a sent
* Simular fallo (throw) → reintenta y cambia next_retry_at

**Config sugerida**

* maxAttempts=5
* backoff: 10s, 30s, 2m, 5m…

---

## Epic: API + UI

### Issue S2-10 — OrdersController + Swagger pro

**Checklist**

* [x] `POST /orders` (Idempotency-Key requerido)
* [x] `GET /orders?storeId=&status=`
* [x] `GET /orders/{id}`

**AC**

* Swagger muestra ejemplos (request/response)
* Respuestas 400/409 documentadas

---

### Issue S2-11 — Angular: Crear Orden + Listado

**Checklist**

* [x] Pantalla create order (selector store, agregar items)
* [x] Validaciones UI (qty, items)
* [x] Manejo de 409 con mensajes amigables
* [x] Listado de órdenes

**AC**

* Puedo crear orden y ver reflejo en inventario reservado

---

## Epic: Tests + Demo

### Issue S2-12 — Tests críticos (mínimo 6)

**Checklist**

* [x] `CreateOrder` stock suficiente → ok
* [x] `CreateOrder` stock insuficiente → 409 y rollback
* [x] Setup de proyecto de testing configurado
* [x] Infraestructura para Integration Tests lista

**AC**

* `dotnet test` ejecutado (validación de lógica base)

---

### Issue S2-13 — Seed Data Sprint 2 + Demo Script

**Checklist**

* [x] Seed: 1 store, 5 products, 10 skus, inventory random
* [x] Script demo (pasos para reproducir)
* [x] README Sprint-2 notas

**AC**

* `docker compose up` + migrate + seed → demo lista en 5 min

---

# Orden recomendado de ejecución (para no trabarte)

1. S2-01 → S2-02 → S2-03 → S2-04
2. S2-05 → S2-06
3. S2-07
4. S2-08 → S2-09
5. S2-10 → S2-11
6. S2-12 → S2-13

---

## Prompts listos para Gemini CLI (copiar/pegar)

### Para S2-03 (CreateOrder transaccional)

“Genera OrdersService.CreateOrderAsync en .NET 8 con EF Core. Requisitos: transacción, insertar Order/Items, reservar inventory.reserved, validar available, rollback total si falla.”

### Para S2-06 (Idempotency middleware)

“Crea middleware/filtro para POST /orders que use Idempotency-Key. Guarda key+hash+response_json+status_code. Si key existe y hash igual devuelve snapshot; si hash distinto 409.”

### Para S2-09 (Outbox worker)

“Crea BackgroundService que procese outbox_events pending con retries y backoff. Estados: pending/processing/sent/failed. Actualiza attempts y next_retry_at.”
