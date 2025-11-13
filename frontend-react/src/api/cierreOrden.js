const API_BASE = import.meta.env.VITE_API_BASE;

// Helper para agregar timeout a las peticiones
async function fetchWithTimeout(url, options = {}, timeout = 30000) {
  const controller = new AbortController();
  const id = setTimeout(() => controller.abort(), timeout);
  
  try {
    const response = await fetch(url, {
      ...options,
      signal: controller.signal
    });
    clearTimeout(id);
    return response;
  } catch (error) {
    clearTimeout(id);
    if (error.name === 'AbortError') {
      throw new Error('La petición excedió el tiempo de espera (30s)');
    }
    throw error;
  }
}

export async function getOrdenesCerrables() {
  const url = `${API_BASE}/api/CierreOrden/cerrables`;
  console.log("🌐 Llamando a:", url);
  console.log("🌐 API_BASE:", API_BASE);
  
  const res = await fetchWithTimeout(url);
  console.log("📡 Status:", res.status);
  
  if (!res.ok) throw new Error("Error al obtener las órdenes cerrables");
  
  const data = await res.json();
  console.log("📦 Datos recibidos:", data);
  return data;
}

export async function postCerrarOrden(payload) {
  console.log("Enviando a:", `${API_BASE}/api/CierreOrden/cerrar`);
  console.log("Payload:", payload);
  
  const res = await fetchWithTimeout(`${API_BASE}/api/CierreOrden/cerrar`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
  
  console.log("Status de respuesta:", res.status);
  const text = await res.text();
  console.log("Respuesta del servidor:", text);
  
  if (!res.ok) {
    throw new Error(text || `Error ${res.status}: ${res.statusText}`);
  }
  return text;
}


export async function getMotivos() {
  const res = await fetchWithTimeout(`${API_BASE}/api/CierreOrden/motivos`);
  if (!res.ok) throw new Error("Error al obtener motivos");
  return res.json();
}




/* 👇👇 NUEVO: trae lo que dejó el Observer en el backend */
export async function getEventosMonitoreo() {
  const res = await fetchWithTimeout(`${API_BASE}/api/CierreOrden/monitoreo`);
  if (!res.ok) throw new Error("Error al obtener eventos de monitoreo");
  return res.json();
}
