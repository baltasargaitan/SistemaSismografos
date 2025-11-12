import { useState, useEffect, useMemo, useRef } from "react";
import { motion } from "framer-motion";
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
  const [errorShown, setErrorShown] = useState(false);
  const pageRef = useRef(null);

  // Evita foco accidental en el contenedor principal
  const preventFocus = (e) => {
    if (e.target === pageRef.current) {
      e.preventDefault();
      pageRef.current.blur();
    }
  };

  // ------- Cargar órdenes -------
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

  // ------- Cargar motivos -------
  async function fetchMotivos() {
    try {
      const data = await getMotivos();
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
  async function cerrarOrden(payload) {
    setBusy(true);
    try {
      console.log("Enviando petición de cierre:", payload);
      const msg = await postCerrarOrden(payload);
      console.log("Respuesta del servidor:", msg);
      setToast({
        kind: "success",
        msg: "Orden cerrada correctamente. Mail enviado y monitor actualizado.",
      });
      setErrorShown(false);
      await fetchOrdenes();
      setSelected(null);
    } catch (e) {
      console.error("Error completo cerrando orden:", e);
      if (!errorShown) {
        setToast({ kind: "error", msg: e.message || "Error al cerrar la orden." });
        setErrorShown(true);
      }
    } finally {
      console.log("Finalizando proceso de cierre");
      setBusy(false);
    }
  }

  // ------- Inicialización -------
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

  // ------- Render -------
  return (
    <div
      ref={pageRef}
      onMouseDown={preventFocus}
      className="relative h-screen bg-linear-to-b from-gray-950 via-black to-gray-900 text-white overflow-hidden select-none cursor-default flex flex-col"
    >
      {/* 🌌 Fondo animado */}
      <motion.div
        className="absolute inset-0 bg-[radial-gradient(circle_at_top_left,rgba(0,255,255,0.05),transparent_70%)] pointer-events-none"
        animate={{ backgroundPosition: ["0% 0%", "100% 100%", "0% 0%"] }}
        transition={{ duration: 20, repeat: Infinity, ease: "easeInOut" }}
      />

      {/* 🧭 Cabecera */}
      <motion.header
        className="relative z-10 text-center py-8 select-none shrink-0"
        initial={{ opacity: 0, y: -30 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.8 }}
      >
        <h1 className="text-4xl md:text-5xl font-bold tracking-tight">
          Cierre de Órdenes de Inspección
        </h1>
        <p className="text-cyan-400 text-sm mt-2 uppercase tracking-widest">
          SISGRAFOS — Sistema de Monitoreo Sísmico
        </p>
      </motion.header>

      {/* 🧩 Contenido principal */}
      <motion.main
        className="relative z-10 flex-1 overflow-hidden px-6 pb-6"
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 0.3, duration: 0.8 }}
      >
        {toast && (
          <Toast
            kind={toast.kind}
            msg={toast.msg}
            onClose={() => {
              setToast(null);
              setErrorShown(false);
            }}
          />
        )}

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 h-full max-w-7xl mx-auto">
          {/* 📋 Panel izquierdo — Tabla */}
          <motion.div
            className="h-full overflow-y-auto bg-white/5 backdrop-blur-md border border-cyan-500/20 rounded-3xl shadow-lg p-4 hover:shadow-cyan-500/20 transition-all cursor-default"
            initial={{ y: 10, opacity: 0 }}
            animate={{ y: 0, opacity: 1 }}
            transition={{ delay: 0.2 }}
            onMouseDown={(e) => e.preventDefault()}
          >
            <OrdersTable
              data={ordenes}
              selected={selected}
              onSelect={setSelected}
              onRefresh={fetchOrdenes}
              loading={loading}
            />
          </motion.div>

        {/* 🧠 Panel derecho — Formulario */}
        <motion.div
          className="h-full overflow-y-auto bg-white/5 backdrop-blur-md border border-cyan-500/20 rounded-3xl shadow-lg p-6 hover:shadow-cyan-500/20 transition-all"
          initial={{ y: 10, opacity: 0 }}
          animate={{ y: 0, opacity: 1 }}
          transition={{ delay: 0.4 }}
        >
          <FormCierre
            orden={ordenSel}
            motivos={motivos}
            onSubmit={cerrarOrden}
            busy={busy}
          />
        </motion.div>

        </div>
      </motion.main>

      {/* 🧩 Footer */}
      <motion.footer
        className="relative z-10 text-center text-gray-500 text-xs tracking-widest py-4 select-none shrink-0"
        initial={{ opacity: 0 }}
        animate={{ opacity: 0.8 }}
        transition={{ delay: 1 }}
      >
        Universidad Tecnológica Nacional — Proyecto Red Sísmica 2025
      </motion.footer>

      {/* 🌊 Onda inferior */}
      <motion.svg
        className="absolute bottom-0 left-0 w-full opacity-10 pointer-events-none"
        viewBox="0 0 1200 200"
        preserveAspectRatio="none"
      >
        <path
          d="M0,100 Q100,80 200,100 T400,100 T600,100 T800,100 T1000,100 T1200,100"
          stroke="cyan"
          strokeWidth="2"
          fill="transparent"
        >
          <animate
            attributeName="d"
            dur="2s"
            repeatCount="indefinite"
            values="
              M0,100 Q100,80 200,100 T400,100 T600,100 T800,100 T1000,100 T1200,100;
              M0,100 Q100,120 200,100 T400,100 T600,80 T800,120 T1000,100 T1200,100;
              M0,100 Q100,80 200,100 T400,100 T600,100 T800,100 T1000,100 T1200,100
            "
          />
        </path>
      </motion.svg>
    </div>
  );
}
