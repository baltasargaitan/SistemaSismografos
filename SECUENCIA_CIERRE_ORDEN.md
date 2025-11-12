# Secuencia de Ejecución: Cierre de Orden de Inspección

## 📋 Descripción General

Este documento detalla paso a paso cómo funciona el proceso de cierre de una orden de inspección en el sistema, desde que el usuario hace clic en "Cerrar Orden" hasta que se notifica a los responsables de reparación y se actualiza el estado del sismógrafo.

---

## 🔄 Flujo Completo de Ejecución

### **FASE 1: Interfaz de Usuario (Frontend React)**

#### 1.1 Carga Inicial de Datos
**Componente:** `PantallaCierreInspeccion.jsx`

**Secuencia:**
1. Se ejecuta el `useEffect` al montar el componente
2. Se llama a la función asíncrona `init()`
3. Se ejecutan en paralelo:
   - `getOrdenesCerrables()` → API: `GET /api/CierreOrden/ordenes-cerrables`
   - `getMotivos()` → API: `GET /api/CierreOrden/motivos`

**Estado resultante:**
```javascript
ordenes: [
  { nroOrden: 1006, estacion: "Estación 4", estado: "Completada", ... },
  { nroOrden: 1005, estacion: "Estación 2", estado: "Completada", ... }
]

motivos: ["Falla eléctrica", "Sin conectividad", "Mantenimiento programado", ...]
```

---

#### 1.2 Selección de Orden
**Componente:** `OrdersTable.jsx`

**Secuencia:**
1. Usuario hace clic en una fila de la tabla
2. Se ejecuta `onSelect(orden.nroOrden)`
3. Se actualiza el estado `selected` en `PantallaCierreInspeccion`
4. Se recalcula `ordenSel` usando `useMemo`:
   ```javascript
   ordenSel = ordenes.find(o => o.nroOrden === selected)
   ```
5. Se pasa `ordenSel` como prop a `FormCierre`

---

#### 1.3 Llenado del Formulario
**Componente:** `FormCierre.jsx`

**Estado del componente:**
```javascript
const [observacion, setObservacion] = useState("");
const [motivosList, setMotivosList] = useState([
  { motivo: "", comentario: "" }
]);
const [mostrarModal, setMostrarModal] = useState(false);
const [intentoEnvio, setIntentoEnvio] = useState(false);
const [bloquearModal, setBloquearModal] = useState(true);
```

**Acciones del usuario:**
1. Escribe en el campo "Observación General"
   - `onChange={(e) => setObservacion(e.target.value)}`
   
2. Selecciona motivo(s) del dropdown
   - `onChange={(v) => actualizarMotivo(i, "motivo", v)}`
   - **Método:** `actualizarMotivo(index, campo, valor)`
   
3. (Opcional) Agrega comentarios a cada motivo
   - `onChange={(e) => actualizarMotivo(i, "comentario", e.target.value)}`

4. (Opcional) Agrega más motivos
   - Click en "Agregar otro motivo" → `onClick={agregarMotivo}`
   - **Método:** `agregarMotivo()` → agrega `{ motivo: "", comentario: "" }` al array

---

#### 1.4 Validación y Confirmación
**Componente:** `FormCierre.jsx`

**Secuencia al hacer submit:**
1. Usuario hace clic en "Cerrar Orden de Inspección"
2. Se ejecuta `onSubmit={validarYConfirmar}`
3. **Método:** `validarYConfirmar(e)`
   ```javascript
   e.preventDefault();
   setIntentoEnvio(true);
   
   // Validaciones:
   const observacionValida = observacion.trim() !== "";
   const motivosValidos = motivosList.every(m => m.motivo.trim() !== "");
   
   if (observacionValida && motivosValidos) {
     setBloquearModal(false);
     setMostrarModal(true);  // Muestra modal de confirmación
   }
   ```

4. Se muestra el **Modal de Confirmación** con:
   - Número de orden
   - Estación
   - Checklist de acciones:
     - ✅ Sismógrafo será marcado como fuera de servicio
     - ✅ Se notificará a responsables de reparación
     - ⚠️ Esta acción no se puede revertir

---

#### 1.5 Confirmación Final
**Componente:** `FormCierre.jsx`

**Secuencia:**
1. Usuario hace clic en "Sí, cerrar orden"
2. Se ejecuta `onClick={confirmarCierre}`
3. **Método:** `confirmarCierre()`
   ```javascript
   setMostrarModal(false);
   setBloquearModal(true);
   
   // Construye el payload
   const payload = {
     nroOrden: orden.nroOrden,
     observacion: observacion.trim(),
     motivos: motivosList
       .filter(m => m.motivo.trim() !== "")
       .map(m => ({
         motivo: m.motivo.trim(),
         comentario: m.comentario.trim()
       }))
   };
   
   // Llama al callback del padre
   onSubmit(payload);
   ```

4. Se ejecuta `cerrarOrden(payload)` en `PantallaCierreInspeccion`

---

### **FASE 2: Llamada a la API (Frontend → Backend)**

#### 2.1 Petición HTTP
**Archivo:** `src/api/cierreOrden.js`

**Método:** `postCerrarOrden(payload)`
```javascript
export async function postCerrarOrden(payload) {
  const response = await fetch(`${API_URL}/CierreOrden/cerrar`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  });
  
  if (!response.ok) {
    throw new Error(`Error ${response.status}: ${response.statusText}`);
  }
  
  return await response.text();
}
```

**Payload enviado:**
```json
{
  "nroOrden": 1006,
  "observacion": "Inspección completada. Se detectaron problemas de conectividad...",
  "motivos": [
    {
      "motivo": "Falla eléctrica",
      "comentario": "Cable principal dañado en sector norte"
    },
    {
      "motivo": "Sin conectividad",
      "comentario": ""
    }
  ]
}
```

---

### **FASE 3: Backend API (.NET 8)**

#### 3.1 Controlador Recibe la Petición
**Archivo:** `Api/Controllers/CierreOrdenController.cs`

**Clase:** `CierreOrdenController`

**Endpoint:**
```csharp
[HttpPost("cerrar")]
public async Task<IActionResult> CerrarOrden([FromBody] CierreOrdenRequestDTO request)
```

**Atributos del DTO:**
```csharp
public class CierreOrdenRequestDTO
{
    public int NroOrden { get; set; }
    public string Observacion { get; set; }
    public List<MotivoDTO> Motivos { get; set; }
}

public class MotivoDTO
{
    public string Motivo { get; set; }
    public string Comentario { get; set; }
}
```

**Secuencia:**
1. Se valida el modelo (ModelState)
2. Se llama al caso de uso:
   ```csharp
   var resultado = await _gestorCierreInspeccion.CerrarOrdenInspeccion(
       request.NroOrden,
       request.Observacion,
       request.Motivos
   );
   ```

---

### **FASE 4: Caso de Uso - GestorCierreInspeccion**

#### 4.1 Clase Principal
**Archivo:** `Aplicacion/UseCases/GestorCierreInspeccion.cs`

**Clase:** `GestorCierreInspeccion` (Implementa `ISujetoOrdenInspeccion`)

**Atributos privados:**
```csharp
private readonly IRepositorioOrdenInspeccion _repoOrden;
private readonly IRepositorioEstado _repoEstado;
private readonly IRepositorioSismografo _repoSismografo;
private readonly IRepositorioEmpleado _repoEmpleado;
private readonly IRepositorioMotivoFueraServicio _repoMotivo;
private readonly IRepositorioMotivoTipo _repoMotivoTipo;
private readonly IEnumerable<IObserverNotificacionCierre> _observers;

// Campos para capturar datos antes de notificar
private int _idSismografo;
private string _nombreEstado;
private DateTime _fechaHoraCierre;
private List<string> _motivosDescripciones;
private List<string> _motivosComentarios;
private List<string> _mailsResponsablesReparacion;
```

---

#### 4.2 Método Principal: CerrarOrdenInspeccion
**Secuencia detallada (7 pasos según diagrama UML):**

##### **Paso 1: Obtener la Orden**
```csharp
var orden = await _repoOrden.ObtenerPorIdAsync(nroOrden);
if (orden == null)
    throw new InvalidOperationException($"No existe orden con ID {nroOrden}");
```

**Clase:** `OrdenDeInspeccion`
**Atributos relevantes:**
- `NroOrden` (int)
- `Observacion` (string)
- `FechaHoraInicio` (DateTime)
- `FechaHoraFin` (DateTime?)
- `Sismografo` (navegación)
- `Estado` (navegación)
- `CambiosEstado` (colección)

---

##### **Paso 2: Obtener Estado "Cerrada"**
```csharp
var estadoCerrada = await _repoEstado.ObtenerPorNombreAsync("Cerrada");
if (estadoCerrada == null)
    throw new InvalidOperationException("No existe el estado 'Cerrada'");

SetNombreEstado(estadoCerrada.Nombre);
```

**Clase:** `Estado`
**Atributos:**
- `IdEstado` (int, PK)
- `Nombre` (string)

**Método privado:**
```csharp
private void SetNombreEstado(string nombre) 
{
    _nombreEstado = nombre;
}
```

---

##### **Paso 3: Registrar Cambio de Estado**
```csharp
var cambioEstado = new CambioEstado
{
    FechaHoraCambio = DateTime.Now,
    OrdenInspeccion = orden,
    EstadoNuevo = estadoCerrada
};

SetFechaHoraCierre(cambioEstado.FechaHoraCambio);

orden.CambiosEstado.Add(cambioEstado);
```

**Clase:** `CambioEstado`
**Atributos:**
- `IdCambioEstado` (int, PK)
- `FechaHoraCambio` (DateTime)
- `OrdenInspeccion` (navegación)
- `EstadoNuevo` (navegación)

**Método privado:**
```csharp
private void SetFechaHoraCierre(DateTime fechaHora) 
{
    _fechaHoraCierre = fechaHora;
}
```

---

##### **Paso 4: Actualizar la Orden**
```csharp
orden.Observacion = observacion;
orden.FechaHoraFin = DateTime.Now;
orden.Estado = estadoCerrada;

await _repoOrden.ActualizarAsync(orden);
```

---

##### **Paso 5: Obtener y Actualizar Sismógrafo**
```csharp
var sismografo = await _repoSismografo.ObtenerPorIdAsync(orden.Sismografo.IdSismografo);
var estadoFueraServicio = await _repoEstado.ObtenerPorNombreAsync("FueraDeServicio");

sismografo.Estado = estadoFueraServicio;
await _repoSismografo.ActualizarAsync(sismografo);

await ActualizarIdSismografo(sismografo.IdSismografo, sismografo.EstacionSismologica.IdEstacionSismologica);
```

**Clase:** `Sismografo`
**Atributos:**
- `IdSismografo` (int, PK)
- `Codigo` (string)
- `Modelo` (string)
- `FechaInstalacion` (DateTime)
- `Estado` (navegación)
- `EstacionSismologica` (navegación)

**Método público (porque lo llama el controlador según UML):**
```csharp
public async Task ActualizarIdSismografo(int idSismografo, int idEstacionSismologica)
{
    var sismografo = await _repoSismografo.ObtenerPorIdAsync(idSismografo);
    if (sismografo?.EstacionSismologica?.IdEstacionSismologica != idEstacionSismologica)
    {
        throw new InvalidOperationException("El sismógrafo no pertenece a la estación");
    }

    _idSismografo = idSismografo;

    // CAPTURA de datos antes de notificar
    var estadoActual = await _repoEstado.ObtenerPorIdAsync(sismografo.Estado.IdEstado);
    SetNombreEstado(estadoActual.Nombre);

    var mails = await ObtenerMailsResponsablesReparacion();
    SetMailsResponsablesReparacion(mails);

    // NOTIFICACIÓN a todos los observers
    Notificar();
}
```

---

##### **Paso 6: Registrar Motivos de Falla**
```csharp
var motivosDescripciones = new List<string>();
var motivosComentarios = new List<string>();

foreach (var motivoDTO in motivos)
{
    var motivoTipo = await _repoMotivoTipo.ObtenerPorDescripcionAsync(motivoDTO.Motivo);
    if (motivoTipo == null)
        throw new InvalidOperationException($"Motivo '{motivoDTO.Motivo}' no existe");

    var motivoFueraServicio = new MotivoFueraServicio
    {
        Comentario = motivoDTO.Comentario,
        MotivoTipo = motivoTipo,
        Sismografo = sismografo
    };

    await _repoMotivo.AgregarAsync(motivoFueraServicio);
    
    motivosDescripciones.Add(motivoTipo.Descripcion);
    motivosComentarios.Add(motivoDTO.Comentario ?? "");
}

SetMotivos(motivosDescripciones);
SetComentarios(motivosComentarios);
```

**Clase:** `MotivoFueraServicio`
**Atributos:**
- `IdMotivoFueraServicio` (int, PK)
- `Comentario` (string)
- `MotivoTipo` (navegación)
- `Sismografo` (navegación)

**Clase:** `MotivoTipo`
**Atributos:**
- `IdMotivoTipo` (int, PK)
- `Descripcion` (string)

**Métodos privados:**
```csharp
private void SetMotivos(List<string> motivos) 
{
    _motivosDescripciones = motivos;
}

private void SetComentarios(List<string> comentarios) 
{
    _motivosComentarios = comentarios;
}
```

---

##### **Paso 7: Obtener Emails de Responsables**
```csharp
var mails = await ObtenerMailsResponsablesReparacion();
SetMailsResponsablesReparacion(mails);
```

**Método privado:**
```csharp
private async Task<List<string>> ObtenerMailsResponsablesReparacion()
{
    var empleados = await _repoEmpleado.ObtenerTodosAsync();
    
    return empleados
        .Where(e => e.Roles.Any(r => r.Nombre == "ResponsableReparacion"))
        .Select(e => e.Mail)
        .ToList();
}

private void SetMailsResponsablesReparacion(List<string> mails) 
{
    _mailsResponsablesReparacion = mails;
}
```

**Clase:** `Empleado`
**Atributos:**
- `IdEmpleado` (int, PK)
- `Legajo` (string)
- `Mail` (string)
- `Nombre` (string)
- `Apellido` (string)
- `Roles` (colección many-to-many)

**Clase:** `Rol`
**Atributos:**
- `IdRol` (int, PK)
- `Nombre` (string) // "ResponsableReparacion", "ResponsableInspeccion"

---

#### 4.3 Patrón Observer - Notificación

**Método del Subject:**
```csharp
public void Notificar()
{
    foreach (var observer in _observers)
    {
        observer.Actualizar(
            _idSismografo,
            _nombreEstado,
            _fechaHoraCierre,
            _motivosDescripciones,
            _motivosComentarios,
            _mailsResponsablesReparacion
        );
    }
}
```

**Interfaz:** `IObserverNotificacionCierre`
```csharp
public interface IObserverNotificacionCierre
{
    void Actualizar(
        int idSismografo,
        string nombreEstado,
        DateTime fechaHoraCierre,
        List<string> motivos,
        List<string> comentarios,
        List<string> mailsResponsablesReparacion
    );
}
```

---

### **FASE 5: Observers - Notificaciones**

#### 5.1 PantallaCCRS (Sistema CCRS)
**Archivo:** `Aplicacion/Servicios/PantallaCCRS.cs`

**Clase:** `PantallaCCRS : IObserverNotificacionCierre`

**Método:**
```csharp
public void Actualizar(
    int idSismografo,
    string nombreEstado,
    DateTime fechaHoraCierre,
    List<string> motivos,
    List<string> comentarios,
    List<string> mailsResponsablesReparacion)
{
    Console.WriteLine("══════════════════════════════════════════════════════");
    Console.WriteLine("📊 NOTIFICACIÓN AL SISTEMA CCRS");
    Console.WriteLine($"🔧 Sismógrafo ID: {idSismografo}");
    Console.WriteLine($"📍 Estado: {nombreEstado}");
    Console.WriteLine($"📅 Fecha/Hora: {fechaHoraCierre:dd/MM/yyyy HH:mm:ss}");
    Console.WriteLine("📋 Motivos:");
    for (int i = 0; i < motivos.Count; i++)
    {
        Console.WriteLine($"   {i + 1}. {motivos[i]}");
        if (!string.IsNullOrWhiteSpace(comentarios[i]))
            Console.WriteLine($"      💬 {comentarios[i]}");
    }
    Console.WriteLine("══════════════════════════════════════════════════════");
}
```

---

#### 5.2 InterfazNotificacionMail (Emails)
**Archivo:** `Aplicacion/Servicios/Notificaciones/InterfazNotificacionMail.cs`

**Clase:** `InterfazNotificacionMail : IObserverNotificacionCierre`

**Método:**
```csharp
public void Actualizar(
    int idSismografo,
    string nombreEstado,
    DateTime fechaHoraCierre,
    List<string> motivos,
    List<string> comentarios,
    List<string> mailsResponsablesReparacion)
{
    Console.WriteLine("══════════════════════════════════════════════════════");
    Console.WriteLine("📧 NOTIFICACIÓN POR EMAIL");
    
    foreach (var mail in mailsResponsablesReparacion)
    {
        Console.WriteLine($"✉️  Enviando a: {mail}");
        Console.WriteLine($"   Asunto: Sismógrafo #{idSismografo} - {nombreEstado}");
        Console.WriteLine($"   Fecha: {fechaHoraCierre:dd/MM/yyyy HH:mm:ss}");
        Console.WriteLine("   Motivos:");
        for (int i = 0; i < motivos.Count; i++)
        {
            Console.WriteLine($"      • {motivos[i]}");
            if (!string.IsNullOrWhiteSpace(comentarios[i]))
                Console.WriteLine($"        Comentario: {comentarios[i]}");
        }
    }
    Console.WriteLine("══════════════════════════════════════════════════════");
}
```

---

#### 5.3 ObservadorWebMonitor (Monitoreo Web)
**Archivo:** `Aplicacion/Servicios/Notificaciones/ObservadorWebMonitor.cs`

**Clase:** `ObservadorWebMonitor` (Clase estática - no inyectada)

**Método estático:**
```csharp
public static void Actualizar(
    int idSismografo,
    string nombreEstado,
    DateTime fechaHoraCierre,
    List<string> motivos,
    List<string> comentarios,
    List<string> mailsResponsablesReparacion)
{
    Console.WriteLine("══════════════════════════════════════════════════════");
    Console.WriteLine("🌐 ACTUALIZACIÓN DEL MONITOR WEB");
    Console.WriteLine($"📡 Sismógrafo #{idSismografo} → Estado: {nombreEstado}");
    Console.WriteLine($"🕐 {fechaHoraCierre:HH:mm:ss}");
    Console.WriteLine($"👥 {mailsResponsablesReparacion.Count} responsables notificados:");
    foreach (var mail in mailsResponsablesReparacion)
    {
        Console.WriteLine($"   - {mail}");
    }
    Console.WriteLine("══════════════════════════════════════════════════════");
}
```

**Nota:** Se invoca manualmente desde `ActualizarIdSismografo` después de `Notificar()`:
```csharp
ObservadorWebMonitor.Actualizar(_idSismografo, _nombreEstado, _fechaHoraCierre, 
    _motivosDescripciones, _motivosComentarios, _mailsResponsablesReparacion);
```

---

### **FASE 6: Persistencia en Base de Datos**

#### 6.1 Repositorios (Entity Framework Core)

**DbContext:** `AppDbContext`
**Base de datos:** SQL Server Express (localhost\SQLEXPRESS)
**Database:** SistemaSismografosDB

**Entidades guardadas:**

1. **OrdenDeInspeccion** (actualizada):
   - `Observacion` = nuevo valor
   - `FechaHoraFin` = DateTime.Now
   - `Estado` = "Cerrada"

2. **CambioEstado** (nuevo registro):
   - `FechaHoraCambio` = DateTime.Now
   - `OrdenInspeccion` = referencia a orden
   - `EstadoNuevo` = "Cerrada"

3. **Sismografo** (actualizado):
   - `Estado` = "FueraDeServicio"

4. **MotivoFueraServicio** (1 o más registros nuevos):
   - `Comentario` = comentario del usuario
   - `MotivoTipo` = referencia (ej: "Falla eléctrica")
   - `Sismografo` = referencia al sismógrafo

---

### **FASE 7: Respuesta al Frontend**

#### 7.1 Controlador Retorna Resultado
```csharp
return Ok(resultado);
```

**Mensaje de éxito:** `"Orden cerrada exitosamente"`

---

#### 7.2 Frontend Procesa la Respuesta
**Componente:** `PantallaCierreInspeccion.jsx`

**Método:** `cerrarOrden(payload)`
```javascript
try {
  const msg = await postCerrarOrden(payload);
  
  setToast({
    kind: "success",
    msg: "✅ ¡Orden cerrada exitosamente! Los responsables de reparación han sido notificados y el sismógrafo está marcado como fuera de servicio."
  });
  
  await fetchOrdenes();  // Refresca la lista
  setSelected(null);     // Limpia la selección
} catch (e) {
  setToast({ 
    kind: "error", 
    msg: `⚠️ Error al cerrar la orden: ${e.message}` 
  });
}
```

---

## 📊 Diagrama de Secuencia Resumido

```
Usuario → FormCierre → PantallaCierreInspeccion → API Controller → GestorCierreInspeccion
                                                                            ↓
                                                        1. Obtiene Orden (Repo)
                                                        2. Obtiene Estado "Cerrada" (Repo)
                                                        3. Crea CambioEstado
                                                        4. Actualiza Orden
                                                        5. Actualiza Sismografo → "FueraDeServicio"
                                                        6. Registra Motivos
                                                        7. Obtiene Emails Responsables
                                                                            ↓
                                                                    Notificar() → Observers
                                                                            ↓
                                            ┌──────────────────────────────┼──────────────────────────────┐
                                            ↓                              ↓                              ↓
                                    PantallaCCRS              InterfazNotificacionMail        ObservadorWebMonitor
                                    (Console log)              (Emails a 3 responsables)      (Console log)
```

---

## 🔑 Clases y Métodos Clave

### **Frontend (React)**
| Componente/Archivo | Método Principal | Responsabilidad |
|---|---|---|
| `PantallaCierreInspeccion.jsx` | `cerrarOrden(payload)` | Orquesta el flujo completo |
| `FormCierre.jsx` | `validarYConfirmar(e)` | Valida datos del formulario |
| `FormCierre.jsx` | `confirmarCierre()` | Construye payload y ejecuta submit |
| `cierreOrden.js` | `postCerrarOrden(payload)` | Llama a la API REST |

### **Backend (.NET 8)**
| Clase | Método Principal | Responsabilidad |
|---|---|---|
| `CierreOrdenController` | `CerrarOrden(request)` | Endpoint HTTP POST |
| `GestorCierreInspeccion` | `CerrarOrdenInspeccion()` | Caso de uso principal (7 pasos) |
| `GestorCierreInspeccion` | `ActualizarIdSismografo()` | Captura datos + notifica observers |
| `GestorCierreInspeccion` | `Notificar()` | Dispara el patrón Observer |
| `PantallaCCRS` | `Actualizar()` | Observer - notifica a CCRS |
| `InterfazNotificacionMail` | `Actualizar()` | Observer - envía emails |
| `ObservadorWebMonitor` | `Actualizar()` | Observer estático - log web |

### **Entidades (EF Core)**
| Entidad | Propósito | Relaciones |
|---|---|---|
| `OrdenDeInspeccion` | Orden de inspección | → Sismografo, Estado, CambiosEstado |
| `CambioEstado` | Historial de cambios | → OrdenInspeccion, EstadoNuevo |
| `Sismografo` | Equipo sismográfico | → Estado, EstacionSismologica, MotivosFueraServicio |
| `MotivoFueraServicio` | Motivo de baja | → Sismografo, MotivoTipo |
| `Empleado` | Responsables | → Roles (many-to-many) |
| `Estado` | Estados posibles | Usado por Orden y Sismografo |

---

## ✅ Validaciones Implementadas

1. **Frontend:**
   - Observación no vacía
   - Al menos un motivo seleccionado
   - Cada motivo debe tener descripción

2. **Backend:**
   - Orden existe
   - Estado "Cerrada" existe
   - Estado "FueraDeServicio" existe
   - Motivos existen en catálogo
   - Sismógrafo pertenece a la estación correcta

---

## 🎯 Resultado Final

Al completar exitosamente el flujo:
- ✅ Orden marcada como "Cerrada"
- ✅ Sismógrafo marcado como "FueraDeServicio"
- ✅ Motivos registrados en BD
- ✅ 3 responsables de reparación notificados por email
- ✅ Sistema CCRS actualizado
- ✅ Monitor web actualizado
- ✅ Historial de cambios de estado guardado
- ✅ Usuario ve mensaje de confirmación
- ✅ Lista de órdenes refrescada (la orden cerrada ya no aparece)

---

**Última actualización:** 12 de noviembre de 2025  
**Sistema:** SistemaSismografos - Monitoreo Sísmico
