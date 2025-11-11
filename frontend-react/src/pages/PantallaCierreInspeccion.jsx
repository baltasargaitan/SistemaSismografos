import { useState, useEffect, useMemo } from "react";
import {
  getOrdenesCerrables,
  postCerrarOrden,
  getMotivos,
} from "../api/cierreOrden";
import OrdersTable from "../components/OrdersTable";
import FormCierre from "../components/FormCierre";
import Toast from "../components/Toast";

export default function PantallaCierreInspeccion() {
  const [ordenes, setOrdenes] = useState([]);
  const [selected, setSelected] = useState(null);
  const [motivos, setMotivos] = useState([]);
  const [loading, setLoading] = useState(false);
  const [busy, setBusy] = useState(false);
  const [toast, setToast] = useState(null);
  const [errorShown, setErrorShown] = useState(false); // evita mostrar repetidos

  // ------- Cargar órdenes de inspección -------
  async function fetchOrdenes() {
    setLoading(true);
    try {
      const data = await getOrdenesCerrables();
      setOrdenes(data);
    } catch (e) {
      if (!errorShown) {
        setToast({ kind: "error", msg: "Error al cargar órdenes" });
        setErrorShown(true);
      }
      console.error("Error cargando órdenes:", e);
    } finally {
      setLoading(false);
    }
  }

  // ------- Cargar motivos desde la API -------
  async function fetchMotivos() {
    try {
      const data = await getMotivos();
      // se espera formato [{ tipoMotivo, descripcion }]
      setMotivos(data.map((m) => m.descripcion));
    } catch (e) {
      if (!errorShown) {
        setToast({ kind: "error", msg: "Error al cargar motivos" });
        setErrorShown(true);
      }
      console.error("Error cargando motivos:", e);
    }
  }

  // ------- Cerrar una orden -------
//   async function cerrarOrden(payload) {
//     // validar antes de enviar
//     if (!payload || !payload.NroOrden || !payload.MotivoTipo || !payload.Observacion) {
//       if (!errorShown) {
//         setToast({ kind: "error", msg: "Faltan datos obligatorios para cerrar la orden." });
//         setErrorShown(true);
//       }
//       return;
//     }

async function cerrarOrden(payload) {
  setBusy(true);
  try {
    const msg = await postCerrarOrden(payload);
    setToast({ kind: "success", msg: "Orden cerrada correctamente. Mail enviado y monitor actualizado." });
    setErrorShown(false);
    await fetchOrdenes();
    setSelected(null);
  } catch (e) {
    if (!errorShown) {
      setToast({ kind: "error", msg: e.message || "Error al cerrar la orden." });
      setErrorShown(true);
    }
    console.error("Error cerrando orden:", e);
  } finally {
    setBusy(false);
  }
}

  // ------- Cargar datos al montar -------
  useEffect(() => {
    async function init() {
      setLoading(true);
      try {
        const [ordenesData, motivosData] = await Promise.all([
          getOrdenesCerrables(),
          getMotivos(),
        ]);
        setOrdenes(ordenesData);
        setMotivos(motivosData.map((m) => m.descripcion));
      } catch (e) {
        if (!errorShown) {
          setToast({ kind: "error", msg: "Error inicial al cargar datos." });
          setErrorShown(true);
        }
      } finally {
        setLoading(false);
      }
    }
    init();
  }, []);

  const ordenSel = useMemo(
    () => ordenes.find((o) => o.nroOrden === selected),
    [ordenes, selected]
  );

  return (
    <div className="max-w-6xl mx-auto p-6 space-y-4" tabIndex={-1}>
      <h1 className="text-2xl font-semibold">Cerrar orden de inspección</h1>

      {toast && (
        <Toast
          kind={toast.kind}
          msg={toast.msg}
          onClose={() => {
            setToast(null);
            setErrorShown(false); // al cerrar el toast, permitir mostrar otro más adelante
          }}
        />
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        <OrdersTable
          data={ordenes}
          selected={selected}
          onSelect={setSelected}
          onRefresh={fetchOrdenes}
          loading={loading}
        />

        <FormCierre
          orden={ordenSel}
          motivos={motivos}
          onSubmit={cerrarOrden}
          busy={busy}
        />
      </div>
    </div>
  );
}
