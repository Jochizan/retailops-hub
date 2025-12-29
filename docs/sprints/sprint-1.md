# Sprint 1: Fundamentos y Arquitectura

**Estado:** Completado
**Fecha:** 28 de Diciembre, 2025

## Objetivos Alcanzados

### 1. Inicialización del Proyecto
- **Backend (.NET 9):**
  - Solución creada con arquitectura limpia (Clean Architecture).
  - Capas: `Api`, `Application`, `Domain`, `Infrastructure`, `Tests`.
- **Frontend (Angular 19):**
  - Proyecto inicializado y estructura base lista.
- **Control de Versiones:**
  - Repositorio Git inicializado.
  - `.gitignore` configurado para .NET, Node.js y archivos de sistema.

### 2. Base de Datos y Persistencia
- **Motor:** Migración exitosa de SQLite a **SQL Server**.
- **ORM:** Entity Framework Core 9 configurado.
- **Conexión:** Cadena de conexión establecida en `appsettings.json` apuntando a `localhost` (TrustServerCertificate=true).

### 3. Modelado de Dominio (Core)
Se definieron las entidades principales con una estrategia robusta para manejo de atributos dinámicos:

- **Entidades:**
  - `Product`: Contiene datos fijos e indexables (Nombre, Categoría, **Marca**).
  - `Sku`: Variantes del producto. Contiene `Code` (único) y `AttributesJson`.
  - `Store`, `Inventory`, `AuditLog`, `OutboxEvent`.
  - `AttributeType`: Catálogo maestro de atributos permitidos.

### 4. Arquitectura "Enterprise" de Atributos
Se implementó una solución híbrida Relacional + JSON para evitar duplicidad de datos y garantizar integridad:

- **Entidad `AttributeType`:**
  - Propiedad `Scope` para diferenciar atributos:
    - `PRODUCT`: Viven en columnas de la tabla Product (ej. 'MARCA').
    - `SKUJSON`: Viven dentro del JSON del SKU (ej. 'FABRICANTE', 'CONTENIDO').
- **Validación Hard-Core (SQL Trigger):**
  - Se implementó el trigger `TR_skus_validate_attributesjson`.
  - **Reglas:**
    1. El campo `AttributesJson` debe ser JSON válido.
    2. Solo permite claves que existan en `attribute_types` con scope `SKUJSON`.
    3. Bloquea intentos de insertar 'MARCA' en el JSON (forzando el uso de `Product.Brand`).

### 5. Infraestructura
- Inyección de Dependencias configurada en `Program.cs`.
- Migraciones de EF Core aplicadas correctamente:
  - `InitialSqlServer`: Tablas base.
  - `AddAttributeTypesAndTrigger`: Tablas de atributos y Trigger de validación.
