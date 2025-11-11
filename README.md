# 🌐 Sistema Sismógrafos — Rama OBSERVER

### 🛰️ Proyecto académico — UTN FRC | PPAI 2025  
**Materia:** Diseño de Sistemas de Información  
**Caso de uso principal:** *Dar cierre a orden de inspección de estación sismológica*  
**Patrón aplicado:** **Observer** — Monitoreo en tiempo real de notificaciones del sistema

---

## 📘 Descripción general

**SistemaSismografos** es una aplicación full stack para la gestión de órdenes de inspección y monitoreo de estaciones sismológicas.  
La rama **`observer`** implementa el **patrón de diseño Observer**, permitiendo que el módulo de monitoreo reciba notificaciones automáticas sobre eventos relevantes del sistema (cierres de órdenes, cambios de estado, etc.).

---

## 🧩 Arquitectura general

| Capa | Tecnología | Descripción |
|------|-------------|-------------|
| **Backend (API REST)** | ASP.NET Core 8 + EF Core | Expone endpoints para gestión de órdenes, sismógrafos, estados y notificaciones. |
| **Persistencia** | SQL Server + Entity Framework | Repositorios concretos e implementación de patrón Unit of Work. |
| **Frontend** | React + Vite + TailwindCSS | UI moderna, responsiva y modular. Usa Framer Motion para animaciones. |
| **Comunicación** | HTTP + JSON | El cliente React interactúa con la API vía fetch/Axios. |

---

## 🧠 Patrón de diseño aplicado

### 🔹 Observer
- **Sujeto:** `GestorCierreInspeccion`
- **Observadores:** componentes de monitoreo que muestran los eventos en tiempo real.
- **Notificación:** cada vez que una orden se cierra o cambia de estado, el gestor notifica a los observadores (pantalla de monitoreo o log del sistema).

**Ventajas:**
- Desacopla la lógica de notificación.
- Permite agregar nuevos observadores sin modificar el gestor.
- Facilita el monitoreo simultáneo desde múltiples interfaces.

---

## 🗂️ Estructura de carpetas

```bash
SistemaSismografos/
├── Api/                        # Proyecto ASP.NET Core
│   ├── Controllers/
│   ├── appsettings.json
│   ├── Program.cs
│   └── ...
│
├── Infraestructura/
│   ├── Persistencia/
│   │   ├── AppDbContext.cs
│   │   ├── AppDbContextFactory.cs
│   │   └── ...
│   └── Repositorios/
│
├── Dominio/
│   ├── Entidades/
│   ├── Interfaces/
│   ├── Servicios/
│   └── ...
│
├── Aplicacion/
│   ├── DTOs/
│   ├── UseCases/
│   └── ...
│
└── frontend-react/
    ├── src/pages/
    │   ├── PantallaInicio.jsx
    │   ├── PantallaCierreInspeccion.jsx
    │   └── PantallaMonitoreoOrdenes.jsx
    └── ...
