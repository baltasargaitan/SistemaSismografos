import { motion } from "framer-motion";

export default function SpinnerOverlay({ label = "Procesando..." }) {
  return (
    <div className="fixed inset-0 z-50 flex flex-col items-center justify-center bg-black/70 backdrop-blur-sm select-none">
      {/* Anillo doble giratorio con efecto sísmico */}
      <motion.div
        className="relative w-24 h-24"
        animate={{ rotate: 360 }}
        transition={{ repeat: Infinity, duration: 1.6, ease: "linear" }}
      >
        {/* Anillo exterior */}
        <div className="absolute inset-0 rounded-full border-t-4 border-cyan-400/80 border-solid shadow-[0_0_20px_rgba(34,211,238,0.4)]" />
        {/* Anillo interior */}
        <div className="absolute inset-3 rounded-full border-t-4 border-blue-500/70 border-solid opacity-70 shadow-[0_0_15px_rgba(59,130,246,0.3)]" />
      </motion.div>

      {/* Ondas expansivas tipo sismo */}
      <motion.div
        className="absolute rounded-full border border-cyan-400/30"
        style={{ width: 120, height: 120 }}
        initial={{ scale: 1, opacity: 0.7 }}
        animate={{
          scale: [1, 1.5, 2],
          opacity: [0.7, 0.3, 0],
        }}
        transition={{ repeat: Infinity, duration: 2, ease: "easeOut" }}
      />

      {/* Texto de estado */}
      <motion.p
        className="mt-8 text-cyan-300 text-sm tracking-widest font-medium text-center"
        initial={{ opacity: 0, y: 8 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.3, duration: 0.8 }}
      >
        {label}
      </motion.p>
    </div>
  );
}
