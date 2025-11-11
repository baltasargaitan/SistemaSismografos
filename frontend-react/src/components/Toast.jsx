import { motion } from "framer-motion";
import { Check, AlertTriangle, Info } from "lucide-react";

export default function Toast({ kind = "info", msg, onClose }) {
  // Colores coherentes con el resto de la app
  const styles =
    kind === "success"
      ? "from-emerald-500/30 to-green-700/20 border-emerald-400/40 text-emerald-300"
      : kind === "error"
      ? "from-red-600/30 to-red-800/20 border-red-400/40 text-red-300"
      : "from-cyan-600/30 to-blue-800/20 border-cyan-400/40 text-cyan-300";

  const Icon =
    kind === "success" ? Check : kind === "error" ? AlertTriangle : Info;

  return (
    <motion.div
      initial={{ opacity: 0, y: -10 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -10 }}
      className={`relative z-50 flex items-center gap-3 border rounded-2xl px-4 py-3 text-sm 
        bg-linear-to-r ${styles} backdrop-blur-md shadow-[0_0_15px_rgba(0,0,0,0.4)]`}
    >
      <Icon className="h-5 w-5 shrink-0" />
      <span className="flex-1 font-medium tracking-wide">{msg}</span>
      {onClose && (
        <button
          onClick={onClose}
          className="text-xs text-gray-400 hover:text-white hover:underline underline-offset-4 transition-all"
        >
          Cerrar
        </button>
      )}
    </motion.div>
  );
}
