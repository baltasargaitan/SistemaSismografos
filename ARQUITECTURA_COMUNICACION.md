# 🏗️ ARQUITECTURA DE COMUNICACIÓN FRONTEND-BACKEND
## Sistema de Gestión de Sismógrafos

---

## 📋 ÍNDICE
1. [Visión General](#visión-general)
2. [Flujo Completo de Cierre de Orden](#flujo-completo)
3. [DTOs - Data Transfer Objects](#dtos)
4. [Endpoints REST](#endpoints)
5. [Patrón Observer en Acción](#patrón-observer)
6. [Diagrama de Secuencia Completo](#diagrama-de-secuencia)

---

## 🌐 VISIÓN GENERAL

### Arquitectura de 3 Capas

```
┌─────────────────────────────────────────────────────────────┐
│                    FRONTEND (React)                         │
│  - PantallaCierreInspeccion.jsx                            │
│  - FormCierre.jsx                                          │
│  - API Client (cierreOrden.js)                             │
└────────────────────┬────────────────────────────────────────┘
                     │ HTTP/JSON
                     │ (DTOs)
┌────────────────────▼────────────────────────────────────────┐
│                  BACKEND API (.NET)                         │
│  - CierreOrdenController.cs                                │
│  - Validación & Mapeo de DTOs                              │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────┐
│              CAPA DE APLICACIÓN                             │
│  - GestorCierreInspeccion (Sujeto del Observer)           │
│  - DTOs (CierreOrdenRequest, MotivoDTO, etc.)              │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────┐
│                 CAPA DE DOMINIO                             │
│  - Entidades: OrdenDeInspeccion, Sismografo, etc.          │
│  - Lógica de Negocio                                       │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────┐
│               CAPA DE INFRAESTRUCTURA                       │
│  - Repositorios (EF Core)                                  │
│  - Base de Datos (SQLite)                                  │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔄 FLUJO COMPLETO DE CIERRE DE ORDEN

### PASO 1: Usuario Interactúa con el Frontend

**Archivo: `PantallaCierreInspeccion.jsx`**

```javascript
// Usuario hace click en "Cerrar Orden"
const cerrarOrden = async (payload) => {
  setBusy(true);
  try {
    // Llama a la función de API
    const msg = await postCerrarOrden(payload);
    setToast({ kind: "success", msg: "✅ ¡Orden cerrada exitosamente!" });
    await fetchOrdenes(); // Recargar lista
  } catch (e) {
    setToast({ kind: "error", msg: `⚠️ Error: ${e.message}` });
  } finally {
    setBusy(false);
  }
};
```

**¿Qué es `payload`?**
Un objeto JavaScript que el frontend enviará al backend:

```javascript
{
  NroOrden: 1001,
  Observacion: "Inspección completada. Falla eléctrica detectada.",
  Confirmar: true,
  MotivosTipo: ["Falla eléctrica", "Sin conectividad"],
  Comentarios: ["Cortocircuito en panel", "Cable dañado"]
}
```

---

### PASO 2: API Client - Preparación de la Petición HTTP

**Archivo: `frontend-react/src/api/cierreOrden.js`**

```javascript
export async function postCerrarOrden(payload) {
  console.log("🌐 Enviando a:", `${API_BASE}/api/CierreOrden/cerrar`);
  console.log("📦 Payload:", payload);
  
  const res = await fetchWithTimeout(`${API_BASE}/api/CierreOrden/cerrar`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload), // ⬅️ Convierte el objeto JS a JSON
  });
  
  console.log("📡 Status:", res.status);
  const text = await res.text();
  console.log("📥 Respuesta:", text);
  
  if (!res.ok) {
    throw new Error(text || `Error ${res.status}`);
  }
  return text;
}
```

**Transformación:**
```
JavaScript Object → JSON.stringify() → String JSON → HTTP Body
```

**HTTP Request enviado:**
```http
POST http://localhost:5001/api/CierreOrden/cerrar
Content-Type: application/json

{
  "NroOrden": 1001,
  "Observacion": "Inspección completada. Falla eléctrica detectada.",
  "Confirmar": true,
  "MotivosTipo": ["Falla eléctrica", "Sin conectividad"],
  "Comentarios": ["Cortocircuito en panel", "Cable dañado"]
}
```

---

### PASO 3: Backend Recibe la Petición - Controller

**Archivo: `Api/Controllers/CierreOrdenController.cs`**

```csharp
[ApiController]
[Route("api/[controller]")]
public class CierreOrdenController : ControllerBase
{
    private readonly GestorCierreInspeccion _gestor;

    public CierreOrdenController(GestorCierreInspeccion gestor)
    {
        _gestor = gestor; // ⬅️ Inyección de Dependencias
    }

    // Endpoint: POST /api/CierreOrden/cerrar
    [HttpPost("cerrar")]
    public async Task<IActionResult> CerrarOrden(
        [FromBody] CierreOrdenRequest request) // ⬅️ DTO recibido
    {
        try
        {
            // Delega al Gestor (Capa de Aplicación)
            var resultado = await _gestor.CierreOrdenInspeccion(request);
            return Ok(resultado); // ⬅️ Retorna string de éxito
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al cerrar la orden: {ex.Message}");
        }
    }
}
```

**¿Qué hace ASP.NET Core automáticamente?**
1. **Deserialización JSON → DTO**: El string JSON se convierte en un objeto C# `CierreOrdenRequest`
2. **Model Binding**: Mapea automáticamente las propiedades JSON a las propiedades del DTO
3. **Validación**: Si el DTO tiene atributos de validación (como `[Required]`), los valida

---

## 📦 DTOs - DATA TRANSFER OBJECTS

### ¿Qué es un DTO?

Un **DTO (Data Transfer Object)** es un objeto simple que transporta datos entre procesos. 
No contiene lógica de negocio, solo propiedades.

**Propósito:**
- ✅ Desacoplar el frontend del modelo de dominio
- ✅ Controlar qué datos se exponen externamente
- ✅ Evitar enviar entidades completas (con relaciones, lógica, etc.)
- ✅ Versionar la API sin romper el dominio

---

### DTO de Entrada: `CierreOrdenRequest`

**Archivo: `Aplicacion/DTOs/CierreOrdenRequestDTO.cs`**

```csharp
namespace Aplicacion.DTOs
{
    public class CierreOrdenRequest
    {
        public int NroOrden { get; set; }           // ⬅️ Identificador de la orden
        public string Observacion { get; set; }     // ⬅️ Texto libre del inspector
        public List<string> MotivosTipo { get; set; } = new(); // ⬅️ Tipos de fallas
        public List<string> Comentarios { get; set; } = new(); // ⬅️ Detalles de cada falla
        public bool Confirmar { get; set; }         // ⬅️ Confirmación del usuario
    }
}
```

**Mapeo JSON ↔ C#:**

```json
{
  "NroOrden": 1001,              → int NroOrden
  "Observacion": "Texto...",     → string Observacion
  "Confirmar": true,             → bool Confirmar
  "MotivosTipo": ["Falla..."],   → List<string> MotivosTipo
  "Comentarios": ["Detalles..."] → List<string> Comentarios
}
```

---

### DTO de Salida: Motivos

**Archivo: `Aplicacion/DTOs/MotivoDTO.cs`**

```csharp
namespace Aplicacion.DTOs
{
    public class MotivoDTO
    {
        public string TipoMotivo { get; set; }    // ⬅️ ID del tipo (ej: "1")
        public string Descripcion { get; set; }   // ⬅️ Nombre legible (ej: "Falla eléctrica")
    }
}
```

**Uso en el Controller:**

```csharp
[HttpGet("motivos")]
public async Task<IActionResult> ObtenerMotivos()
{
    var motivos = await _gestor.ObtenerMotivosAsync();
    
    // Mapeo: Entidad de Dominio → DTO
    var resultado = motivos.Select(m => new
    {
        tipoMotivo = m.TipoMotivo,
        descripcion = m.Descripcion
    });
    
    return Ok(resultado); // ⬅️ Se serializa a JSON automáticamente
}
```

**JSON retornado al frontend:**

```json
[
  {
    "tipoMotivo": "1",
    "descripcion": "Falla eléctrica"
  },
  {
    "tipoMotivo": "2",
    "descripcion": "Sin conectividad"
  }
]
```

---

## 🌐 ENDPOINTS REST - API Completa

### Mapa de Endpoints

```
┌────────────────────────────────────────────────────────────┐
│              API: /api/CierreOrden                         │
├────────────────────────────────────────────────────────────┤
│                                                            │
│  GET  /cerrables                                           │
│  ↳ Retorna: List<OrdenResumenDTO>                         │
│  ↳ Filtro: Empleado logueado + Estado "Completada"        │
│                                                            │
│  GET  /motivos                                             │
│  ↳ Retorna: List<MotivoDTO>                               │
│  ↳ Catálogo de motivos técnicos disponibles               │
│                                                            │
│  POST /cerrar                                              │
│  ↳ Recibe: CierreOrdenRequest (JSON)                      │
│  ↳ Retorna: string (mensaje de éxito/error)               │
│  ↳ Dispara: Patrón Observer (notificaciones)              │
│                                                            │
│  GET  /monitoreo                                           │
│  ↳ Retorna: List<EventoMonitoreo>                         │
│  ↳ Registro de eventos del Observer                       │
│                                                            │
└────────────────────────────────────────────────────────────┘
```

---

### Endpoint 1: Obtener Órdenes Cerrables

**Request:**
```http
GET http://localhost:5001/api/CierreOrden/cerrables
```

**Backend:**
```csharp
[HttpGet("cerrables")]
public async Task<IActionResult> ObtenerOrdenesCerrables()
{
    // 1. Llama al Gestor (Capa de Aplicación)
    var ordenes = await _gestor.BuscarOrdenesDeInspeccion();

    // 2. Mapeo: Entidad de Dominio → DTO anónimo
    var resultado = ordenes.Select(o => new
    {
        nroOrden = o.GetNroOrden(),
        estacion = o.GetEstacion()?.GetNombre(),
        estado = o.GetEstado()?.NombreEstado,
        fechaInicio = o.GetFechaHoraInicio()
    });

    // 3. Serializa automáticamente a JSON
    return Ok(resultado);
}
```

**Response JSON:**
```json
[
  {
    "nroOrden": 1001,
    "estacion": "Estación 1",
    "estado": "Completada",
    "fechaInicio": "2024-11-01T10:30:00"
  },
  {
    "nroOrden": 1005,
    "estacion": "Estación 2",
    "estado": "Completada",
    "fechaInicio": "2024-11-05T14:15:00"
  }
]
```

---

### Endpoint 2: Cerrar Orden (con Observer)

**Request:**
```http
POST http://localhost:5001/api/CierreOrden/cerrar
Content-Type: application/json

{
  "NroOrden": 1001,
  "Observacion": "Inspección completada. Falla eléctrica detectada.",
  "Confirmar": true,
  "MotivosTipo": ["Falla eléctrica", "Sin conectividad"],
  "Comentarios": ["Cortocircuito en panel", "Cable dañado"]
}
```

**Backend:**
```csharp
[HttpPost("cerrar")]
public async Task<IActionResult> CerrarOrden(
    [FromBody] CierreOrdenRequest request)
{
    try
    {
        // Llama al método que ejecuta el patrón Observer
        var resultado = await _gestor.CierrarOrdenInspeccion(request);
        
        return Ok(resultado);
        // ⬅️ Retorna: "Orden 1001 cerrada correctamente. Notificaciones enviadas."
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Error al cerrar la orden: {ex.Message}");
    }
}
```

---

## 🎯 PATRÓN OBSERVER EN ACCIÓN

### Arquitectura del Observer

```
┌──────────────────────────────────────────────────────────┐
│            SUJETO CONCRETO                               │
│        GestorCierreInspeccion                           │
│                                                          │
│  - Lista estática de observadores                       │
│  - Método: IniC1() → Suscribe observadores             │
│  - Método: Notificar() → Informa a todos               │
└────────────┬─────────────────────────────────────────────┘
             │
             │ Notifica a:
             ├────────────────────────┬────────────────────┐
             │                        │                    │
┌────────────▼──────────┐  ┌─────────▼─────────┐  ┌───────▼────────┐
│  OBSERVADOR 1         │  │  OBSERVADOR 2      │  │  OBSERVADOR 3  │
│  PantallaCCRS         │  │  InterfazMail      │  │  WebMonitor    │
│                       │  │                    │  │                │
│  Actualizar():        │  │  Actualizar():     │  │  Actualizar(): │
│  - Imprime consola    │  │  - Envía emails    │  │  - Registra log│
└───────────────────────┘  └────────────────────┘  └────────────────┘
```

---

### Secuencia del Patrón Observer

#### 1. INICIALIZACIÓN (al arrancar la aplicación)

**Archivo: `Api/Program.cs`**

```csharp
// ----------------------------------------------------------
//  INICIALIZACIÓN DEL GESTOR (al iniciar app)
// ----------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var gestor = scope.ServiceProvider.GetRequiredService<GestorCierreInspeccion>();
    
    // ⬇️ Llama al método de inicialización del Observer
    gestor.IniC1();
    
    Console.WriteLine("✅ GestorCierreInspeccion inicializado con observadores.");
}
```

**¿Qué hace `IniC1()`?**

**Archivo: `Aplicacion/Servicios/Notificaciones/GestorCierreInspeccion.cs`**

```csharp
public void IniC1()
{
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    Console.WriteLine("🔧 Inicializando GestorCierreInspeccion...");
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    
    // PASO 1: Crear observador PantallaCCRS
    Console.WriteLine("\n[PASO 1] Creando PantallaCCRS...");
    var pantallaCCRS = CrearPantallaCCRS(); // ⬅️ Factory Method
    
    // PASO 2: Crear observador InterfazNotificacionMail
    Console.WriteLine("\n[PASO 2] Creando InterfazNotificacionMail...");
    var interfazMail = CrearPantallasNotificacionMail(); // ⬅️ Factory Method
    
    // PASO 3: Suscribir ambos observadores
    Console.WriteLine("\n[PASO 3] Suscribiendo observadores...");
    Suscribir(new IObserverNotificacionCierre[] { pantallaCCRS, interfazMail });
    
    Console.WriteLine($"\n✅ Gestor inicializado con {_observadoresGlobales.Count} observadores.");
}
```

**Lista estática de observadores:**

```csharp
// ⚠️ IMPORTANTE: Lista estática compartida entre todas las instancias
// Los observadores se crean UNA SOLA VEZ al iniciar la app
private static readonly List<IObserverNotificacionCierre> _observadoresGlobales = new();

public void Suscribir(IObserverNotificacionCierre[] observadores)
{
    foreach (var observador in observadores)
    {
        _observadoresGlobales.Add(observador);
        Console.WriteLine($"✅ Observador '{observador.GetType().Name}' suscrito.");
    }
}
```

---

#### 2. EJECUCIÓN DEL CIERRE (cuando el usuario confirma)

**Archivo: `GestorCierreInspeccion.cs` → Método `CerrarOrdenInspeccion()`**

```csharp
public async Task<string> CerrarOrdenInspeccion(CierreOrdenRequest request)
{
    // ═══════════════════════════════════════════════════════════════
    // PASO 1: VALIDACIONES PREVIAS
    // ═══════════════════════════════════════════════════════════════
    var usuario = _sesionService.ObtenerUsuarioLogueado();
    if (usuario == null) return "No hay usuario logueado.";

    var ordenEntidad = await _ordenRepo.BuscarPorNroAsync(request.NroOrden);
    if (ordenEntidad == null) return $"No se encontró la orden {request.NroOrden}.";

    if (string.IsNullOrWhiteSpace(request.Observacion))
        return "Debe ingresar una observación.";

    if (!request.Confirmar)
        return "Cierre cancelado por el usuario.";

    // ═══════════════════════════════════════════════════════════════
    // PASO 2: BUSCAR ESTADO "CERRADA"
    // ═══════════════════════════════════════════════════════════════
    var estadoCerrado = BuscarEstadoCerradoParaOrdenInspeccion();

    // ═══════════════════════════════════════════════════════════════
    // PASO 3: CERRAR LA ORDEN
    // ═══════════════════════════════════════════════════════════════
    try
    {
        ordenEntidad.Cerrar(request.Observacion, estadoCerrado);
    }
    catch (InvalidOperationException ex)
    {
        return ex.Message;
    }

    // ═══════════════════════════════════════════════════════════════
    // PASO 4: ACTUALIZAR SISMÓGRAFO (cambiar a "Fuera de Servicio")
    // ═══════════════════════════════════════════════════════════════
    var estacion = ordenEntidad.GetEstacion();
    var sismografo = estacion?.ObtenerIdSismografo();

    if (sismografo != null && estacion != null)
    {
        await RegistrarFallaSismografo(
            sismografo.GetIdentificadorSismografo(),
            estacion,
            request.MotivosTipo,
            request.Comentarios,
            await _motivoTipoRepo.ObtenerTodosAsync()
        );
    }

    // ═══════════════════════════════════════════════════════════════
    // PASO 5: GUARDAR CAMBIOS EN BASE DE DATOS
    // ═══════════════════════════════════════════════════════════════
    _ordenRepo.Actualizar(ordenEntidad);
    await _ordenRepo.GuardarCambiosAsync();

    // ═══════════════════════════════════════════════════════════════
    // PASO 6: OBTENER EMAILS DE RESPONSABLES DE REPARACIÓN
    // ═══════════════════════════════════════════════════════════════
    var empleados = await _empleadoRepo.ObtenerTodosAsync() ?? new List<Empleado>();
    var mailsResp = ObtenerMailsResponsablesReparacion(empleados);
    _mailsResponsablesReparacion = mailsResp;

    // ═══════════════════════════════════════════════════════════════
    // PASO 7: 🔔 NOTIFICAR A TODOS LOS OBSERVADORES (PATRÓN OBSERVER)
    // ═══════════════════════════════════════════════════════════════
    Notificar(); // ⬅️⬅️⬅️ AQUÍ SE EJECUTA EL PATRÓN OBSERVER

    return $"Orden {ordenEntidad.GetNroOrden()} cerrada correctamente. Notificaciones enviadas.";
}
```

---

#### 3. NOTIFICACIÓN A OBSERVADORES

**Método: `Notificar()`**

```csharp
public void Notificar()
{
    Console.WriteLine($"\n🔔 [NOTIFICAR] Iniciando notificación a {_observadoresGlobales.Count} observadores...");
    Console.WriteLine($"   - ID Sismógrafo: {_idSismografo}");
    Console.WriteLine($"   - Estado: {_nombreEstado}");
    Console.WriteLine($"   - Motivos: {string.Join(", ", _motivos)}");
    Console.WriteLine($"   - Emails responsables reparación: {string.Join(", ", _mailsResponsablesReparacion)}");
    
    // Loop: Recorrer todos los observadores estáticos y notificar uno por uno
    int contador = 1;
    foreach (var observador in _observadoresGlobales)
    {
        try
        {
            Console.WriteLine($"\n   [{contador}/{_observadoresGlobales.Count}] Notificando a {observador.GetType().Name}...");
            
            // ⬇️ Llamar al método Actualizar() de cada observador
            observador.Actualizar(
                idSismografo: _idSismografo,
                nombreEstado: _nombreEstado,
                fechaHoraCierre: _fechaHoraCierre,
                motivos: _motivos,
                comentarios: _comentarios,
                mailsResponsablesReparacion: _mailsResponsablesReparacion.ToArray()
            );
            
            Console.WriteLine($"   ✅ {observador.GetType().Name} notificado correctamente.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Error al notificar {observador.GetType().Name}: {ex.Message}");
        }
        contador++;
    }
    
    Console.WriteLine($"\n✅ Notificación completada a todos los observadores.\n");
}
```

---

#### 4. OBSERVADOR 1: PantallaCCRS (Consola)

**Archivo: `Aplicacion/Servicios/Notificaciones/PantallaCCRS.cs`**

```csharp
public class PantallaCCRS : IObserverNotificacionCierre
{
    // Atributos internos para almacenar datos de la notificación
    private int _idSismografo;
    private string _nombreEstado = string.Empty;
    private DateTime _fechaHoraCierre;
    private string[] _motivos = Array.Empty<string>();
    private string[] _comentarios = Array.Empty<string>();
    private string[] _mailsResponsables = Array.Empty<string>();

    public void Actualizar(
        int idSismografo,
        string nombreEstado,
        DateTime fechaHoraCierre,
        string[] motivos,
        string[] comentarios,
        string[] mailsResponsablesReparacion)
    {
        // Guardar datos recibidos
        _idSismografo = idSismografo;
        _nombreEstado = nombreEstado;
        _fechaHoraCierre = fechaHoraCierre;
        _motivos = motivos;
        _comentarios = comentarios;
        _mailsResponsables = mailsResponsablesReparacion;

        // Mostrar en consola
        MostrarEnPantalla();
    }

    private void MostrarEnPantalla()
    {
        Console.WriteLine("\n╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  📋 PANTALLA CCRS - CIERRE DE ORDEN                      ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
        Console.WriteLine($"🔧 Sismógrafo ID: {_idSismografo}");
        Console.WriteLine($"📊 Nuevo Estado: {_nombreEstado}");
        Console.WriteLine($"⏰ Fecha/Hora Cierre: {_fechaHoraCierre:G}");
        Console.WriteLine($"⚠️ Motivos: {string.Join(", ", _motivos)}");
        Console.WriteLine($"💬 Comentarios: {string.Join(", ", _comentarios)}");
        Console.WriteLine($"📧 Responsables notificados: {string.Join(", ", _mailsResponsables)}");
        Console.WriteLine("═══════════════════════════════════════════════════════════\n");
    }
}
```

**Salida en consola:**
```
╔═══════════════════════════════════════════════════════════╗
║  📋 PANTALLA CCRS - CIERRE DE ORDEN                      ║
╚═══════════════════════════════════════════════════════════╝
🔧 Sismógrafo ID: 1
📊 Nuevo Estado: FueraDeServicio
⏰ Fecha/Hora Cierre: 13/11/2025 14:30:45
⚠️ Motivos: Falla eléctrica, Sin conectividad
💬 Comentarios: Cortocircuito en panel, Cable dañado
📧 Responsables notificados: marcos.pomenich@empresa.com, carla.rodriguez@empresa.com, luis.fernandez@empresa.com
═══════════════════════════════════════════════════════════
```

---

#### 5. OBSERVADOR 2: InterfazNotificacionMail (Email)

**Archivo: `Aplicacion/Servicios/Notificaciones/InterfazNotificacionMail.cs`**

```csharp
public class InterfazNotificacionMail : IObserverNotificacionCierre
{
    private string _cuerpoEmail = string.Empty;
    private readonly SmtpSettings _smtpSettings;

    public void Actualizar(
        int idSismografo,
        string nombreEstado,
        DateTime fechaHoraCierre,
        string[] motivos,
        string[] comentarios,
        string[] mailsResponsablesReparacion)
    {
        // 1. Generar cuerpo del email
        _cuerpoEmail = GenerarCuerpoEmail(
            idSismografo,
            nombreEstado,
            fechaHoraCierre,
            motivos,
            comentarios
        );

        // 2. Enviar mail a cada responsable (LOOP)
        if (mailsResponsablesReparacion != null && mailsResponsablesReparacion.Length > 0)
        {
            foreach (var mail in mailsResponsablesReparacion)
            {
                EnviarMail(mail, _cuerpoEmail);
            }
        }
    }

    private void EnviarMail(string mailResponsableReparacion, string cuerpoEmail)
    {
        try
        {
            // Crear mensaje con MimeKit
            var mensaje = new MimeKit.MimeMessage();
            mensaje.From.Add(new MimeKit.MailboxAddress(
                _smtpSettings.FromName,
                _smtpSettings.FromAddress
            ));
            mensaje.To.Add(new MimeKit.MailboxAddress("", mailResponsableReparacion));
            mensaje.Subject = "Notificación de Cierre de Orden de Inspección";

            var bodyBuilder = new MimeKit.BodyBuilder
            {
                TextBody = cuerpoEmail
            };
            mensaje.Body = bodyBuilder.ToMessageBody();

            // Enviar con MailKit
            using var client = new MailKit.Net.Smtp.SmtpClient();
            client.Connect(_smtpSettings.Host, _smtpSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
            client.Authenticate(_smtpSettings.User, _smtpSettings.Password);
            client.Send(mensaje);
            client.Disconnect(true);

            Console.WriteLine($"[InterfazNotificacionMail] ✅ Email enviado a: {mailResponsableReparacion}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[InterfazNotificacionMail] ❌ Error: {ex.Message}");
        }
    }

    private string GenerarCuerpoEmail(
        int idSismografo,
        string nombreEstado,
        DateTime fechaHoraCierre,
        string[] motivos,
        string[] comentarios)
    {
        return $"Estimado/a Responsable de Reparación,\n\n" +
               $"Se ha registrado el cierre de una orden de inspección:\n\n" +
               $"• Sismógrafo ID: {idSismografo}\n" +
               $"• Nuevo Estado: {nombreEstado}\n" +
               $"• Fecha y Hora: {fechaHoraCierre:G}\n" +
               $"• Motivos: {string.Join(", ", motivos)}\n" +
               $"• Comentarios: {string.Join(", ", comentarios)}\n\n" +
               $"Por favor, tome las acciones necesarias.\n\n" +
               $"Sistema de Gestión de Sismógrafos";
    }
}
```

---

## 📊 DIAGRAMA DE SECUENCIA COMPLETO

```
┌─────────┐   ┌──────────┐   ┌──────────┐   ┌─────────┐   ┌───────────┐   ┌──────────┐
│ Usuario │   │ Frontend │   │ API      │   │ Gestor  │   │Repositorio│   │Observer  │
│         │   │ (React)  │   │Controller│   │ (Sujeto)│   │ (EF Core) │   │          │
└────┬────┘   └────┬─────┘   └────┬─────┘   └────┬────┘   └─────┬─────┘   └────┬─────┘
     │             │                │              │              │              │
     │ 1. Click   │                │              │              │              │
     │ "Cerrar"   │                │              │              │              │
     ├───────────>│                │              │              │              │
     │            │                │              │              │              │
     │            │ 2. POST        │              │              │              │
     │            │ /cerrar        │              │              │              │
     │            │ + DTO          │              │              │              │
     │            ├───────────────>│              │              │              │
     │            │                │              │              │              │
     │            │                │ 3. Validar  │              │              │
     │            │                │    DTO       │              │              │
     │            │                │──────────────┘              │              │
     │            │                │              │              │              │
     │            │                │ 4. Llamar   │              │              │
     │            │                │ CerrarOrden()│              │              │
     │            │                ├─────────────>│              │              │
     │            │                │              │              │              │
     │            │                │              │ 5. Buscar   │              │
     │            │                │              │    Orden    │              │
     │            │                │              ├─────────────>│              │
     │            │                │              │<─────────────┤              │
     │            │                │              │  OrdenEntity │              │
     │            │                │              │              │              │
     │            │                │              │ 6. Cerrar() │              │
     │            │                │              │──────────────┘              │
     │            │                │              │              │              │
     │            │                │              │ 7. Actualizar│              │
     │            │                │              │  Sismografo  │              │
     │            │                │              ├─────────────>│              │
     │            │                │              │              │              │
     │            │                │              │ 8. SaveChanges()           │
     │            │                │              ├─────────────>│              │
     │            │                │              │<─────────────┤              │
     │            │                │              │              │              │
     │            │                │              │ 9. Notificar()              │
     │            │                │              ├────────────────────────────>│
     │            │                │              │              │              │
     │            │                │              │              │  10. Loop:   │
     │            │                │              │              │  Actualizar()│
     │            │                │              │              │  cada        │
     │            │                │              │              │  observador  │
     │            │                │              │              │              │
     │            │                │              │              │  11a. Consola│
     │            │                │              │              │  11b. Email  │
     │            │                │              │              │  11c. Log    │
     │            │                │              │<─────────────────────────────┤
     │            │                │              │              │              │
     │            │                │ 12. Return  │              │              │
     │            │                │    mensaje   │              │              │
     │            │                │<─────────────┤              │              │
     │            │                │              │              │              │
     │            │ 13. HTTP 200   │              │              │              │
     │            │    + string    │              │              │              │
     │            │<───────────────┤              │              │              │
     │            │                │              │              │              │
     │ 14. Toast  │                │              │              │              │
     │    Success │                │              │              │              │
     │<───────────┤                │              │              │              │
     │            │                │              │              │              │
```

---

## 🔑 PUNTOS CLAVE

### 1. **DTOs como Frontera**
- Los DTOs son la **interfaz entre frontend y backend**
- Evitan exponer el modelo de dominio completo
- Permiten versionar la API sin afectar el dominio

### 2. **Controller como Adaptador**
- El Controller **recibe** DTOs del frontend
- **Valida** y **mapea** a entidades de dominio
- **Delega** la lógica de negocio al Gestor (capa de aplicación)

### 3. **Gestor como Sujeto del Observer**
- El `GestorCierreInspeccion` es el **Sujeto Concreto**
- Mantiene una lista estática de observadores
- Llama a `Notificar()` después de completar la lógica de negocio

### 4. **Observadores Independientes**
- Cada observador implementa `IObserverNotificacionCierre`
- Reciben la misma información pero actúan diferente:
  - `PantallaCCRS`: Imprime en consola
  - `InterfazNotificacionMail`: Envía emails
  - `ObservadorWebMonitor`: Registra en log

### 5. **Desacoplamiento**
- El Gestor **no sabe** qué hacen los observadores
- Los observadores **no saben** de dónde vienen los datos
- Puedes agregar/quitar observadores sin modificar el Gestor

---

## 📝 RESUMEN EJECUTIVO

**Flujo completo en 7 pasos:**

1. **Usuario** → Click "Cerrar Orden" en React
2. **Frontend** → Envía DTO como JSON via HTTP POST
3. **Controller** → Recibe y deserializa JSON a DTO
4. **Gestor** → Ejecuta lógica de negocio (cerrar orden, actualizar sismógrafo)
5. **Repositorio** → Persiste cambios en base de datos
6. **Gestor** → Llama a `Notificar()` (Patrón Observer)
7. **Observadores** → Reciben notificación y ejecutan acciones (consola, email, log)

**Tecnologías involucradas:**
- **Frontend**: React, JavaScript, fetch API
- **Transporte**: HTTP/REST, JSON
- **Backend**: ASP.NET Core, C#
- **Patrón**: Observer (Gang of Four)
- **Persistencia**: Entity Framework Core, SQLite
- **Email**: MailKit, MimeKit

---

**Archivo generado:** `ARQUITECTURA_COMUNICACION.md`
**Fecha:** 13 de noviembre de 2025
**Sistema:** Red Sísmica - Gestión de Sismógrafos
