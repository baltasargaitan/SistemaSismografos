import { useState, useMemo, useRef } from "react";
import { Search, RefreshCw } from "lucide-react";
import Spinner from "./SpinnerOverlay";
import { motion } from "framer-motion";

export default function OrdersTable({ data, selected, onSelect, onRefresh, loading }) {
  const [q, setQ] = useState("");
  const [sort, setSort] = useState({ key: "fechaInicio", asc: false });
  const tableRef = useRef(null);

  const filtered = useMemo(() => {
    const base = (data ?? []).filter((d) => {
      const s = `${d.estacion ?? ""} ${d.nroOrden ?? ""}`;
      return s.toLowerCase().includes(q.toLowerCase());
    });
    const dir = sort.asc ? 1 : -1;
    return base.sort((a, b) => {
      const ka = a[sort.key];
      const kb = b[sort.key];
      if (ka == null && kb == null) return 0;
      if (ka == null) return 1;
      if (kb == null) return -1;
      if (sort.key === "nroOrden") return (ka - kb) * dir;
      return String(ka).localeCompare(String(kb)) * dir;
    });
  }, [data, q, sort]);

  // 🧠 Bloquea foco accidental en contenedor
  const preventFocus = (e) => {
    if (e.target === tableRef.current) {
      e.preventDefault();
      tableRef.current.blur();
    }
  };

  return (
    <motion.div
      ref={tableRef}
      onMouseDown={preventFocus}
      className="flex flex-col gap-3 h-full text-white select-none"
      initial={{ opacity: 0, y: 10 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.6 }}
    >
      {/* 🔍 buscador y refresh */}
      <div className="flex items-center gap-2">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-cyan-400 opacity-70" />
          <input
            value={q}
            onChange={(e) => setQ(e.target.value)}
            placeholder="Buscar por #orden o estación..."
            className="w-full pl-9 pr-3 py-2 rounded-xl bg-white/5 border border-cyan-400/30 text-sm placeholder-gray-400 focus:outline-none focus:ring-1 focus:ring-cyan-400 select-text"
          />
        </div>
        <button
          onClick={onRefresh}
          className="inline-flex items-center gap-2 rounded-xl bg-cyan-600/10 border border-cyan-400/30 px-3 py-2 hover:bg-cyan-500/20 transition-all"
        >
          <RefreshCw className={`h-4 w-4 ${loading ? "animate-spin" : ""}`} />
          <span className="text-sm">Actualizar</span>
        </button>
      </div>

      {/* 🧾 tabla */}
      <div
        className="overflow-hidden rounded-2xl border border-cyan-400/20 bg-white/5 backdrop-blur-md cursor-default"
        onMouseDown={(e) => e.preventDefault()} // 🔒 evita foco de texto accidental
      >
        <table className="w-full text-sm">
          <thead className="bg-cyan-500/10 border-b border-cyan-400/30">
            <tr className="text-left text-cyan-300 select-none">
              <th className="px-4 py-2"># Orden</th>
              <th className="px-4 py-2">Estación</th>
              <th className="px-4 py-2">Estado</th>
              <th className="px-4 py-2">Inicio</th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr>
                <td colSpan={4} className="px-3 py-6 text-center">
                  <Spinner label="Cargando órdenes..." />
                </td>
              </tr>
            ) : filtered.length === 0 ? (
              <tr>
                <td colSpan={4} className="px-3 py-6 text-center text-gray-400">
                  Sin resultados
                </td>
              </tr>
            ) : (
              filtered.map((o) => (
                <tr
                  key={o.nroOrden}
                  className={`border-t border-cyan-400/10 hover:bg-cyan-500/10 cursor-pointer transition-all select-none ${
                    selected === o.nroOrden ? "bg-cyan-600/20" : ""
                  }`}
                  onClick={() => onSelect(o.nroOrden)}
                >
                  <td className="px-4 py-2">{o.nroOrden}</td>
                  <td className="px-4 py-2">{o.estacion ?? "—"}</td>
                  <td className="px-4 py-2 text-cyan-300">{o.estado ?? "—"}</td>
                  <td className="px-4 py-2">{o.fechaInicio ?? "—"}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </motion.div>
  );
}
