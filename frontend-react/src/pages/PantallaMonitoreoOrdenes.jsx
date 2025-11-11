import { useEffect, useState } from "react";
import { motion } from "framer-motion";
import { Activity, RefreshCcw } from "lucide-react";
import { getEventosMonitoreo } from "../api/cierreOrden";

export default function PantallaMonitoreoOrdenes() {
  const [eventos, setEventos] = useState([]);
  const [refreshing, setRefreshing] = useState(false);
    
  async function cargarEventos() {
    try {
      setRefreshing(true);
      const ev = await getEventosMonitoreo();
      setEventos(ev);
    } catch (err) {
      console.error("Error cargando eventos del monitoreo:", err);
    } finally {
      setRefreshing(false);
    }
  }

  useEffect(() => {
    cargarEventos();
    const id = setInterval(cargarEventos, 5000);
    return () => clearInterval(id);
  }, []);

  return (
    <div
      className="relative min-h-screen bg-gradient-to-b from-gray-950 via-black to-gray-900 text-white overflow-hidden select-none pointer-events-none cursor-none"
    >
      {/* Fondo animado */}
      <motion.div
        className="absolute inset-0 bg-[radial-gradient(circle_at_center,rgba(0,255,255,0.06),transparent_70%)]"
        animate={{ backgroundPosition: ["0% 0%", "100% 100%", "0% 0%"] }}
        transition={{ duration: 18, repeat: Infinity, ease: "easeInOut" }}
      />

      {/* Cabecera */}
      <motion.header
        className="relative z-10 text-center py-10"
        initial={{ opacity: 0, y: -30 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.8 }}
      >
        <h1 className="text-4xl md:text-5xl font-bold tracking-tight bg-gradient-to-r from-cyan-400 via-blue-400 to-purple-500 bg-clip-text text-transparent">
          Monitor del Sistema
        </h1>
        <p className="text-cyan-400 text-sm mt-2 uppercase tracking-widest">
          SISGRAFOS — Sistema de Observación Sísmica
        </p>
      </motion.header>

      {/* Panel del monitor */}
      <motion.main
        className="relative z-10 max-w-3xl mx-auto p-6"
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 0.3, duration: 0.8 }}
      >
        <motion.div
          className="bg-white/5 backdrop-blur-lg border border-green-500/20 rounded-3xl shadow-lg p-6"
          initial={{ y: 10, opacity: 0 }}
          animate={{ y: 0, opacity: 1 }}
          transition={{ delay: 0.4 }}
        >
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-xl font-semibold text-green-400 flex items-center gap-2">
              <Activity className="h-5 w-5" /> Eventos Recientes
            </h2>
            <div className="flex items-center gap-2 text-xs text-gray-400 italic">
              <RefreshCcw
                className={`h-4 w-4 ${
                  refreshing ? "animate-spin text-cyan-400" : "text-gray-500"
                }`}
              />
              Refresca cada 5 s automáticamente
            </div>
          </div>

          <div className="h-[500px] overflow-y-auto font-mono text-xs space-y-1 bg-black/60 border border-green-500/10 rounded-xl p-3 shadow-inner">
            {eventos.length > 0 ? (
              eventos.map((e, i) => (
                <motion.div
                  key={i}
                  initial={{ opacity: 0, x: -5 }}
                  animate={{ opacity: 1, x: 0 }}
                  transition={{ delay: i * 0.02 }}
                  className="text-green-300/90"
                >
                  ▶ {e}
                </motion.div>
              ))
            ) : (
              <div className="text-gray-500 text-center mt-12">
                Sin eventos registrados aún…
              </div>
            )}
          </div>
        </motion.div>
      </motion.main>

      {/* Footer */}
      <motion.footer
        className="relative z-10 text-center text-gray-500 text-xs tracking-widest py-6"
        initial={{ opacity: 0 }}
        animate={{ opacity: 0.8 }}
        transition={{ delay: 1 }}
      >
        Universidad Tecnológica Nacional — Proyecto Red Sísmica 2025
      </motion.footer>
    </div>
  );
}
