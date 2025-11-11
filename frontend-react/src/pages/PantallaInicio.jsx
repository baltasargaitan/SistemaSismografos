import { motion } from "framer-motion";
import { useNavigate } from "react-router-dom";
import { AlertTriangle, Activity, Compass } from "lucide-react";

export default function PantallaInicio() {
  const navigate = useNavigate();

  const handleClick = (e, path) => {
    // Clic izquierdo (0) o clic medio (1)
    if (e.button === 0 || e.button === 1) {
      e.preventDefault();
      if (e.button === 1) {
        // Abrir en nueva pestaña si es clic medio
        window.open(path, "_blank");
      } else {
        // Navegación interna normal
        navigate(path);
      }
    }
  };

  return (
<div className="relative min-h-screen overflow-hidden bg-linear-to-b from-gray-950 via-black to-gray-900 text-white select-none pointer-events-auto cursor-default caret-transparent">
      {/* Fondo con energía sísmica */}
      <motion.div
        className="absolute inset-0 bg-[radial-gradient(ellipse_at_bottom_right,rgba(0,255,255,0.1),transparent_70%)]"
        animate={{
          backgroundPosition: ["0% 0%", "100% 100%", "0% 0%"],
        }}
        transition={{ duration: 15, repeat: Infinity, ease: "easeInOut" }}
      />

      {/* Línea sísmica animada */}
      <motion.svg
        className="absolute bottom-1/4 left-0 w-full h-16 opacity-40"
        viewBox="0 0 1200 200"
        preserveAspectRatio="none"
      >
        
        <motion.path
          d="M0,100 Q100,80 200,100 T400,100 T600,100 T800,100 T1000,100 T1200,100"
          stroke="cyan"
          strokeWidth="2"
          fill="transparent"
          animate={{
            d: [
              "M0,100 Q100,80 200,100 T400,100 T600,100 T800,100 T1000,100 T1200,100",
              "M0,100 Q100,120 200,100 T400,100 T600,80 T800,120 T1000,100 T1200,100",
            ],
          }}
          transition={{ repeat: Infinity, duration: 2, ease: "easeInOut" }}
        />
      </motion.svg>

      {/* Título central */}
      <motion.div
        className="relative z-10 text-center mt-24"
        initial={{ opacity: 0, y: -30 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 1 }}
      >
        <h1 className="text-6xl font-extrabold tracking-tight">
          SISGRAFOS
        </h1>
        <p className="text-cyan-400 text-xl mt-2 font-light tracking-widest">
          Sistema de Monitoreo y Gestión Sísmica
        </p>
        <motion.div
          className="mt-4 h-1 w-40 mx-auto bg-linear-to-r from-cyan-400 to-blue-600 rounded-full"
          initial={{ width: 0 }}
          animate={{ width: "10rem" }}
          transition={{ duration: 1.2 }}
        />
      </motion.div>

      {/* Cards */}
      <div className="relative z-10 mt-20 grid grid-cols-1 md:grid-cols-2 gap-10 px-8 max-w-6xl mx-auto select-none">
        {/* Card 1 */}
        <motion.div
          onMouseDown={(e) => handleClick(e, "/cerrar")}
          whileHover={{ scale: 1.05, y: -5 }}
          className="cursor-pointer bg-linear-to-br from-blue-800/20 to-cyan-700/10 backdrop-blur-lg border border-cyan-400/30 rounded-3xl p-8 shadow-lg hover:shadow-cyan-500/30 transition-all"
        >
          <div className="flex items-center gap-4 mb-3">
            <div className="p-3 rounded-2xl bg-cyan-500/20">
              <Activity className="w-7 h-7 text-cyan-400" />
            </div>
            <h2 className="text-2xl font-semibold text-cyan-300">
              Cerrar Orden de Inspección
            </h2>
          </div>
          <p className="text-gray-300 leading-relaxed">
            Gestioná el cierre de órdenes de inspección con trazabilidad total
            y registro auditado de cambios.
          </p>
          <div className="mt-6 text-sm text-cyan-400 uppercase tracking-widest">
            Ingresar →
          </div>
        </motion.div>

        {/* Card 2 */}
        <motion.div
          onMouseDown={(e) => handleClick(e, "/monitoreo")}
          whileHover={{ scale: 1.05, y: -5 }}
          className="cursor-pointer bg-linear-to-br from-emerald-800/20 to-green-700/10 backdrop-blur-lg border border-emerald-400/30 rounded-3xl p-8 shadow-lg hover:shadow-emerald-500/30 transition-all"
        >
          <div className="flex items-center gap-4 mb-3">
            <div className="p-3 rounded-2xl bg-emerald-500/20">
              <Compass className="w-7 h-7 text-emerald-400" />
            </div>
            <h2 className="text-2xl font-semibold text-emerald-300">
              Monitoreo en Tiempo Real
            </h2>
          </div>
          <p className="text-gray-300 leading-relaxed">
            Observá en tiempo real los estados de estaciones y notificaciones
            del sistema de observadores.
          </p>
          <div className="mt-6 text-sm text-emerald-400 uppercase tracking-widest">
            Ingresar →
          </div>
        </motion.div>
      </div>

      {/* Footer */}
      <motion.footer
        className="absolute bottom-6 w-full text-center text-gray-500 text-xs tracking-widest"
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 1.5 }}
      >
        Universidad Tecnológica Nacional — Red Sísmica PPAI 2025
      </motion.footer>

      {/* Icono decorativo */}
      <motion.div
        className="absolute bottom-0 right-0 text-cyan-500/10"
        animate={{ rotate: [0, 10, -10, 0] }}
        transition={{ repeat: Infinity, duration: 20 }}
      >
        <AlertTriangle className="w-64 h-64" />
      </motion.div>
    </div>
  );
}
