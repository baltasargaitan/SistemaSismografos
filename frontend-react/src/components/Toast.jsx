import { motion } from "framer-motion";
import { Check, AlertTriangle, Info } from "lucide-react";

export default function Toast({ kind = "info", msg, onClose }) {
  // Colores tierra alineados con el diseño general - reducen cansancio visual
  const styles =
    kind === "success"
      ? "from-[var(--earth-green-500)]/30 to-[var(--earth-green-700)]/20 border-[var(--earth-green-400)]/40 text-[var(--earth-green-100)]"
      : kind === "error"
      ? "from-[var(--earth-red-500)]/30 to-[var(--earth-red-700)]/20 border-[var(--earth-red-400)]/40 text-[var(--earth-red-100)]"
      : "from-[var(--earth-blue-500)]/30 to-[var(--earth-blue-700)]/20 border-[var(--earth-blue-400)]/40 text-[var(--earth-blue-100)]";

  const Icon =
    kind === "success" ? Check : kind === "error" ? AlertTriangle : Info;

  return (
    <motion.div
      initial={{ opacity: 0, y: -10 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -10 }}
      className={`relative z-50 flex items-center gap-3 border rounded-2xl px-5 py-3.5 text-sm 
        bg-linear-to-r ${styles} backdrop-blur-md shadow-[0_0_20px_rgba(0,0,0,0.3)]`}
    >
      <div className={`rounded-full p-1.5 ${
        kind === "success" 
          ? "bg-(--earth-green-500)/20" 
          : kind === "error"
          ? "bg-(--earth-red-500)/20"
          : "bg-(--earth-blue-500)/20"
      }`}>
        <Icon className="h-5 w-5 shrink-0" />
      </div>
      <span className="flex-1 font-medium tracking-wide leading-relaxed">{msg}</span>
      {onClose && (
        <button
          onClick={onClose}
          className="text-xs text-(--earth-gray-400) hover:text-(--earth-gray-100) 
            hover:underline underline-offset-4 transition-all duration-200 px-2 py-1 rounded
            hover:bg-white/10"
        >
          Cerrar
        </button>
      )}
    </motion.div>
  );
}
