import { useState, useRef } from "react";
import { ChevronDown } from "lucide-react";
import { motion, AnimatePresence } from "framer-motion";

export default function SimpleSelect({
  value,
  onChange,
  options,
  placeholder = "Seleccionar...",
  label,
  disabled,
  name,
  error,
}) {
  const [open, setOpen] = useState(false);
  const btnRef = useRef(null);

  return (
    <div className="w-full relative text-white" style={{ zIndex: open ? 600 : 1 }}>
      {label && (
        <label
          className="block text-sm mb-1 font-medium text-cyan-200"
          htmlFor={name}
        >
          {label}
        </label>
      )}

      <button
        ref={btnRef}
        id={name}
        type="button"
        disabled={disabled}
        onClick={() => setOpen((v) => !v)}
        className={`w-full inline-flex items-center justify-between rounded-xl border px-3 py-2 text-left text-sm bg-white/10 backdrop-blur-sm hover:bg-white/20 transition-all ${
          error ? "border-red-500" : "border-cyan-400/30"
        }`}
      >
        <span className={value ? "" : "text-gray-400"}>
          {value || placeholder}
        </span>
        <ChevronDown className="h-4 w-4 opacity-70 text-cyan-300" />
      </button>

      {error && <p className="text-red-500 text-xs mt-1">{error}</p>}

      <AnimatePresence>
        {open && (
          <motion.ul
            initial={{ opacity: 0, y: -5 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -5 }}
            className="absolute z-500 mt-1 w-full max-h-40 overflow-auto rounded-xl border border-blue-400/50 shadow-2xl"
            style={{ 
              backgroundColor: '#0a0e12',
              backdropFilter: 'none'
            }}
          >
            {options.length === 0 && (
              <li className="px-3 py-2 text-sm text-gray-500 bg-[#0a0e12]">
                No hay opciones
              </li>
            )}
            {options.map((opt) => (
              <li key={opt} style={{ backgroundColor: '#0a0e12' }}>
                <button
                  type="button"
                  onClick={() => {
                    onChange(opt);
                    setOpen(false);
                    btnRef.current?.focus();
                  }}
                  className={`w-full text-left px-3 py-2 text-sm transition-all ${
                    opt === value 
                      ? "bg-blue-600 text-white font-medium" 
                      : "text-gray-200 hover:bg-blue-900"
                  }`}
                >
                  {opt}
                </button>
              </li>
            ))}
          </motion.ul>
        )}
      </AnimatePresence>
    </div>
  );
}
