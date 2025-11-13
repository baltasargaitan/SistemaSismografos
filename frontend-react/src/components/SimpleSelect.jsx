import { useState, useRef, useEffect } from "react";
import { createPortal } from "react-dom";
import { ChevronDown } from "lucide-react";
import { motion, AnimatePresence } from "framer-motion";

export default function SimpleSelect({
  value,
  onChange,
  options = [],
  placeholder = "Seleccionar...",
  label,
  disabled = false,
  name,
  error,
}) {
  const [isOpen, setIsOpen] = useState(false);
  const [dropdownPosition, setDropdownPosition] = useState({ top: 0, left: 0, width: 0 });
  const buttonRef = useRef(null);
  const dropdownRef = useRef(null);

  // Calcular posición del dropdown
  useEffect(() => {
    if (isOpen && buttonRef.current) {
      const rect = buttonRef.current.getBoundingClientRect();
      setDropdownPosition({
        top: rect.bottom + window.scrollY + 4,
        left: rect.left + window.scrollX,
        width: rect.width,
      });
    }
  }, [isOpen]);

  // Cerrar dropdown al hacer click fuera
  useEffect(() => {
    if (!isOpen) return;

    const handleClickOutside = (e) => {
      if (
        buttonRef.current &&
        !buttonRef.current.contains(e.target) &&
        dropdownRef.current &&
        !dropdownRef.current.contains(e.target)
      ) {
        setIsOpen(false);
      }
    };

    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [isOpen]);

  const handleSelect = (option) => {
    onChange(option);
    setIsOpen(false);
  };

  // Renderizar el dropdown
  const dropdown = isOpen && createPortal(
    <AnimatePresence>
      <motion.div
        ref={dropdownRef}
        initial={{ opacity: 0, y: -8 }}
        animate={{ opacity: 1, y: 0 }}
        exit={{ opacity: 0, y: -8 }}
        transition={{ duration: 0.15 }}
        style={{
          position: "absolute",
          top: `${dropdownPosition.top}px`,
          left: `${dropdownPosition.left}px`,
          width: `${dropdownPosition.width}px`,
          zIndex: 99999,
        }}
        className="bg-gray-900 border-2 border-cyan-400/50 rounded-xl shadow-2xl overflow-hidden"
      >
        <ul className="max-h-60 overflow-y-auto">
          {options.length === 0 ? (
            <li className="px-4 py-3 text-sm text-gray-400">
              No hay opciones disponibles
            </li>
          ) : (
            options.map((option, index) => (
              <li key={`${option}-${index}`}>
                <button
                  type="button"
                  onClick={() => handleSelect(option)}
                  className={`w-full text-left px-4 py-2.5 text-sm transition-colors ${
                    value === option
                      ? "bg-cyan-600 text-white font-medium"
                      : "text-gray-200 hover:bg-cyan-900/50"
                  }`}
                >
                  {option}
                </button>
              </li>
            ))
          )}
        </ul>
      </motion.div>
    </AnimatePresence>,
    document.body
  );

  return (
    <div className="w-full">
      {label && (
        <label
          htmlFor={name}
          className="block text-sm mb-1.5 font-medium text-cyan-200"
        >
          {label}
        </label>
      )}

      <button
        ref={buttonRef}
        id={name}
        type="button"
        disabled={disabled}
        onClick={() => !disabled && setIsOpen(!isOpen)}
        className={`w-full flex items-center justify-between rounded-xl border-2 px-4 py-2.5 text-left text-sm bg-white/10 backdrop-blur-sm transition-all ${
          disabled
            ? "opacity-50 cursor-not-allowed"
            : "hover:bg-white/20 cursor-pointer"
        } ${error ? "border-red-500 focus:ring-2 focus:ring-red-400" : "border-cyan-400/30 focus:ring-2 focus:ring-cyan-400"} focus:outline-none`}
      >
        <span className={value ? "text-white" : "text-gray-400"}>
          {value || placeholder}
        </span>
        <ChevronDown
          className={`h-4 w-4 text-cyan-300 transition-transform ${
            isOpen ? "rotate-180" : ""
          }`}
        />
      </button>

      {error && (
        <p className="text-red-400 text-xs mt-1.5 flex items-center gap-1">
          {error}
        </p>
      )}

      {dropdown}
    </div>
  );
}
