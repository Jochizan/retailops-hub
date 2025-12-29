# Sprint 3 — Ops Dashboard & Alerts (+ Tailwind)

**Sprint Goal:** que en 20 segundos se entienda: *inventario crítico, alertas accionables y trazabilidad visible (audit/outbox)*, con UI enterprise.

**Branch:** `feat/sprint-3-ops-dashboard-alerts`

## DoD global Sprint 3

* ✅ Dashboard funcional con datos reales (por tienda)
* ✅ Alertas `open/ack/resolved` + flujo completo desde orden/stock_low
* ✅ Pantallas Audit + Outbox (wow)
* CI pasa + `docker compose up` levanta
* README + demo script (ideal: video 60–90s)

---

## S3-00 — Angular 21: configurar TailwindCSS (Must)

### Checklist

* [x] Instalar dependencias
* [x] Generar configs `tailwind.config.*` y `postcss.config.*`
* [x] Configurar `content` para HTML/TS
* [x] Importar Tailwind en `src/styles.*`
* [x] Verificar build + clase Tailwind en una vista

---

# Epic: Reporting (API)

## S3-01 — Report: Stock crítico por tienda (Must)

**Endpoint:** `GET /reports/stock-critical?storeId=&limit=`

### Checklist

* [x] Query eficiente: `available = onHand - reserved`
* [x] Condición: `available <= reorderPoint`
* [x] Orden: menor available
* [x] Incluir: store, skuCode, productName, brand, available, reorderPoint

**AC**

* Devuelve Top N y coincide con lo que se ve en Inventory.

---

## S3-02 — Report: Resumen de órdenes (Must)

**Endpoint:** `GET /reports/orders-summary?storeId=&from=&to=`

### Checklist

* [x] Conteos por status
* [x] Total órdenes y total items
* [x] (Opcional) total monto

**AC**

* Responde en < 500ms con seed demo.

---

## S3-03 — Report: Top SKUs (Should)

**Endpoint:** `GET /reports/top-skus?storeId=&from=&to=&limit=`

### Checklist

* [x] Sum qty por sku en rango
* [x] Orden desc

**AC**

* Devuelve lista top con qty total.

---

# Epic: Alerts

## S3-04 — Modelo `StockAlert` + migración (Must)

### Checklist

* [x] Tabla `StockAlerts` (Id, StoreId, SkuId, Type, Status, Message, CreatedAt, UpdatedAt)
* [x] Evitar duplicados open: unique `(StoreId,SkuId,Type,Status)` donde Status='open' (si no haces filtered index, lo manejas en lógica)

**AC**

* No se duplica alerta open para mismo sku/tienda.

---

## S3-05 — Generar alertas desde Outbox Worker (Must)

### Checklist

* [x] Al procesar evento `stock_low`: upsert alerta open
* [x] Mensaje incluye valores: available/reorderPoint
* [x] Audit log del cambio (si aplica)

**AC**

* Orden → stock baja → aparece alerta open.

---

## S3-06 — Endpoints de Alertas (Must)

* `GET /alerts?storeId=&status=`
* `POST /alerts/{id}/ack`
* `POST /alerts/{id}/resolve`

### Checklist

* [x] Validar transiciones (open → ack → resolved)
* [x] Respuestas claras

**AC**

* UI puede ack/resolve y persiste.

---

# Epic: Ops Visibility (Audit + Outbox)

## S3-07 — Audit API filtros (Must)

**Endpoint:** `GET /audit?entity=&actor=&from=&to=&take=`

### Checklist

* [x] Paginación simple
* [x] Orden por fecha desc
* [x] Buen formato del diff JSON

**AC**

* Puedo filtrar Inventory u Orders y ver cambios.

---

## S3-08 — Outbox API (Must)

* `GET /outbox?status=&take=`
* `GET /outbox/{id}`
* (Opcional) `POST /outbox/{id}/retry`

### Checklist

* [x] Mostrar attempts/nextRetryAt

**AC**

* Puedo inspeccionar eventos y payloads desde UI.

---

# Epic: Frontend Angular (Tailwind)

## S3-09 — Store Picker global (Must)

### Checklist

* [x] Selector de tienda en navbar
* [x] Persistir en localStorage
* [x] Service central `CurrentStoreService`

**AC**

* Cambiar store refresca dashboard/alerts/reportes.

---

## S3-10 — Dashboard UI (Must)

### Checklist

* [x] KPIs Orders Summary
* [x] Tabla Stock Crítico
* [x] Top SKUs (si S3-03 listo)
* [x] Estados: loading/empty/error

**AC**

* Demo “20 segundos” ✅

---

## S3-11 — Alerts UI (Must)

### Checklist

* [x] Listado con tabs: open/ack/resolved
* [x] Botones ACK/RESOLVE
* [x] Link hacia inventory/sku (mínimo: filtro aplicado)

**AC**

* Flujo completo operativo.

---

## S3-12 — Audit UI (Wow)

### Checklist

* [x] Tabla audit con filtros básicos
* [x] Modal/Drawer para diff pretty-print
* [x] Botón copiar

---

## S3-13 — Outbox UI (Wow)

### Checklist

* [x] Tabla por status
* [x] Ver payload
* [x] (Opcional) retry failed

---

# Epic: Release-grade

## S3-14 — CI (Must)

### Checklist

* [x] GitHub Actions: restore/build/test backend
* [x] (Opcional) build frontend

---

## S3-15 — Docker compose + README + demo script (Must)

### Checklist

* [x] `docker compose up` levanta DB + API
* [x] README: run, endpoints, arquitectura (OCC + outbox + idempotency)
* [x] Demo script (pasos exactos)

---

## Orden recomendado (para ejecutar sin trabarte)

1. **S3-00 Tailwind**
2. S3-01 + S3-02 (Reports API)
3. S3-04 + S3-05 + S3-06 (Alerts end-to-end)
4. S3-07 + S3-08 (Audit/Outbox API)
5. S3-09 + S3-10 + S3-11 (UI core)
6. S3-12 + S3-13 (wow)
7. S3-14 + S3-15 (release)
