# Sprint 4 — Release-Grade + Portfolio Mode (sin romper Sprint-3)

**Sprint Goal:** cerrar el ciclo de negocio + endurecer API/UI + dejar evidencia (docs, demo, deploy, story).

## DoD global Sprint-4

- ✅ Order lifecycle completo (confirm/cancel) con consistencia de inventario
- ✅ Reports/Alerts siguen correctos tras cambios
- ✅ Errores estandarizados (ProblemDetails) + validación sólida
- ✅ Auth/RBAC aplicado en endpoints clave
- ✅ UI pulida (tailwind, estados, toasts, responsive)
- ✅ README “senior” + diagrama + demo script + video
- ✅ Release tag `v1.0.0` + CI green + docker compose ok

---

## Epic A — Business Completeness (sin romper reportes/alertas)

### S4-01 — Order Lifecycle: Confirm / Cancel (Must)

**Checklist**

- [x] `POST /orders/{id}/confirm`
- [x] `POST /orders/{id}/cancel`
- [x] Validar transiciones
- [x] Emitir outbox events: `order_confirmed`, `order_cancelled`

---

### S4-02 — Inventory Movements (Kardex mínimo) (Should, pero muy valioso)

**Checklist**

- [x] Tabla `InventoryMovements`
- [x] Registrar movimiento en cada operación

---

## Epic B — API Hardening (la parte que huele a senior)

### S4-03 — ProblemDetails estándar + códigos de error (Must)

**Checklist**

- [x] Unificar 400/401/403/404/409/500 con `ProblemDetails`
- [x] Error codes consistentes
- [x] Mapear excepciones conocidas a 409/400 con mensajes claros

---

### S4-04 — Validación fuerte (Must)

**Checklist**

- [x] Validación reforzada en servicios
- [x] Validar cantidades y estados

---

### S4-05 — Auth/RBAC en endpoints críticos (Must)

**Checklist**

- [x] Roles: `admin`, `manager`, `clerk`
- [x] Policies: Admin, Manager
- [x] Bloquear audit/outbox a admin/manager

---

### S4-06 — Observabilidad mínima pro (Should)

**Checklist**

- [x] CorrelationId middleware
- [x] Health endpoint `/health` y “readiness” DB

---

## Epic C — UI Polish con Tailwind (Angular 21)

### S4-07 — UI Consistency Pack (Must)

**Checklist**

- [x] Layout final (sidebar/topbar), spacing consistente
- [x] Componentes base Tailwind
- [x] Empty states y loading skeletons
- [x] Toast notifications (usando alerts nativos/confirm por ahora)

**AC**

- Se siente “enterprise”, no demo escolar

---

### S4-08 — Flujos completos en UI (Must)

**Checklist**

- [x] Orders list + order detail
- [x] Botones Confirm/Cancel con confirm modal
- [x] Inventory view muestra available (vía Dashboard)
- [x] Alerts: ack/resolve con feedback visual

**AC**

- Demo fluye sin depender de Swagger

---

### S4-09 — Responsive mínimo + accesibilidad básica (Should)

**Checklist**

- [x] Sidebar/Navbar responsivo
- [x] Tablas: responsive con horizontal scroll
- [x] Focus styles / aria labels en acciones importantes

---

## Epic D — QA / Regression (para no romper Sprint-3)

### S4-10 — Regression suite (Must)

**Checklist**

- [x] Tests backend (Order Lifecycle Integration Tests)
- [x] CreateOrder ok
- [x] Confirm order actualiza inventory
- [x] Cancel libera reserved

**AC**

- Suite de pruebas automatizada pasando (3/3 verdes)

- [ ] (Opcional) Playwright smoke test: login + dashboard loads

**AC**

- Cualquier cambio no tumba dashboard/alerts

---

## Epic E — Release & Portfolio Storytelling

### S4-11 — README senior + diagrama (Must)

**Checklist**

- [x] README con secciones técnicas y de negocio
- [x] Patrones: OCC + Idempotency + Outbox + Audit
- [x] Diagrama simple (Mermaid) de flujo order→inventory→outbox→alerts

---

### S4-12 — Seed data + Demo Script “perfecto” (Must)

**Checklist**

- [x] Seed: 2 stores, 8 products, 12 skus
- [x] Demo script paso a paso (5 minutos)
- [x] “Demo scenarios” cubiertos

---

### S4-13 — Deploy + Release tag (Must)

**Checklist**

- [ ] Deploy API + DB (según tu plan actual)
- [ ] Deploy Angular
- [ ] Variables env documentadas
- [ ] Tag `v1.0.0` + GitHub Release notes

---

### S4-14 — Video demo 60–90s (Should pero top impacto)

**Checklist**

- [ ] 1 toma, sin edición compleja
- [ ] Guion:

  - “Qué problema resuelve”
  - “Patrones de confiabilidad”
  - “Dashboard/alerts”
  - “Audit/outbox”

- [ ] Subir a YouTube no listado o drive + link en README/LinkedIn

---

# Orden recomendado (para no romper Sprint-3)

1. **S4-01** (confirm/cancel) + tests rápidos
2. **S4-02** (movements)
3. **S4-03/04/05** (hardening: ProblemDetails + validation + RBAC)
4. **S4-07/08** (UI final)
5. **S4-10** (regresión)
6. **S4-11/12/13** (docs + seed + release)
7. **S4-14** (video)

---

## Nota crítica ligada al Sprint-3 (para que no te explote)

En Sprint-3 probablemente tus reportes (top skus / resumen órdenes) contaban **reserved**.
En Sprint-4, cuando metes confirm/cancel, lo correcto es:

- **Ventas/reportes** = `confirmed`
- **Operación** = `reserved` (pendientes)
  Eso lo hace real y te da un talking point brutal en entrevistas.
