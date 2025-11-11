const API_BASE = import.meta.env.VITE_API_BASE;

export async function getOrdenesCerrables() {
  const res = await fetch(`${API_BASE}/api/CierreOrden/cerrables`);
  if (!res.ok) throw new Error("Error al obtener las órdenes cerrables");
  return res.json();
}

export async function postCerrarOrden(payload) {
  const res = await fetch(`${API_BASE}/api/CierreOrden/cerrar`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
  const text = await res.text();
  if (!res.ok) throw new Error(text);
  return text;
}


export async function getMotivos() {
  const res = await fetch(`${API_BASE}/api/CierreOrden/motivos`);
  if (!res.ok) throw new Error("Error al obtener motivos");
  return res.json();
}




/* 👇👇 NUEVO: trae lo que dejó el Observer en el backend */
export async function getEventosMonitoreo() {
  const res = await fetch(`${API_BASE}/api/CierreOrden/monitoreo`);
  if (!res.ok) throw new Error("Error al obtener eventos de monitoreo");
  return res.json();
}
