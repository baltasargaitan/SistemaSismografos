# 🌐 Sistema Sismógrafos
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

El proyecto sigue una arquitectura en capas que promueve la separación de responsabilidades y facilita el mantenimiento:

### **Dominio**
Contiene las entidades principales y la lógica de negocio, totalmente independiente de frameworks o servicios externos.

### **Aplicación**
Define los casos de uso, los DTOs y las interfaces de los servicios que articulan la interacción entre las capas.

### **Infraestructura**
Implementa la capa de persistencia mediante Entity Framework Core 9.0.10, gestionando la conexión con la base de datos y los repositorios.

### **API**
Expone los controladores REST y configura el backend para la comunicación con el frontend.

### **Frontend**
Desarrollado con **React 19 + Vite 7**, encargado de la interfaz de usuario y la interacción con los servicios del backend. Utiliza **TailwindCSS** para el diseño responsivo y **Framer Motion** para animaciones fluidas.

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

│   └── ...
│
├── Aplicacion/
│   ├── DTOs/
│   ├── Servicios/
│   ├── UseCases/
│   └── ...
│
└── frontend-react/
    ├── src/pages/
    │   ├── PantallaInicio.jsx
    │   ├── PantallaCierreInspeccion.jsx
    │   └── PantallaMonitoreoOrdenes.jsx
    └── ...
