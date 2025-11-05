import { useState, useMemo } from "react";
import { Search, RefreshCw, ChevronDown } from "lucide-react";
import Spinner from "./Spinner";

export default function OrdersTable({ data, selected, onSelect, onRefresh, loading }) {
  const [q, setQ] = useState("");
  const [sort, setSort] = useState({ key: "fechaInicio", asc: false });

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

  return (
    <div className="flex flex-col gap-3 h-full">
      <div className="flex items-center gap-2">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 opacity-60" />
          <input
            value={q}
            onChange={(e) => setQ(e.target.value)}
            placeholder="Buscar por #orden o estación..."
            className="w-full pl-9 pr-3 py-2 rounded-xl border bg-background"
          />
        </div>
        <button
          onClick={onRefresh}
          className="inline-flex items-center gap-2 rounded-xl border px-3 py-2 hover:bg-gray-100"
        >
          <RefreshCw className={`h-4 w-4 ${loading ? "animate-spin" : ""}`} />
          <span className="text-sm">Actualizar</span>
        </button>
      </div>

      <div className="overflow-hidden rounded-2xl border">
        <table className="w-full text-sm">
          <thead className="bg-gray-50">
            <tr className="text-left">
              <th className="px-3 py-2"># Orden</th>
              <th className="px-3 py-2">Estación</th>
              <th className="px-3 py-2">Estado</th>
              <th className="px-3 py-2">Inicio</th>
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
                <td colSpan={4} className="px-3 py-6 text-center text-gray-500">
                  Sin resultados
                </td>
              </tr>
            ) : (
              filtered.map((o) => (
                <tr
                  key={o.nroOrden}
                  className={`border-t hover:bg-gray-100 cursor-pointer ${
                    selected === o.nroOrden ? "bg-purple-100" : ""
                  }`}
                  onClick={() => onSelect(o.nroOrden)}
                >
                  <td className="px-3 py-2">{o.nroOrden}</td>
                  <td className="px-3 py-2">{o.estacion ?? "—"}</td>
                  <td className="px-3 py-2">{o.estado ?? "—"}</td>
                  <td className="px-3 py-2">{o.fechaInicio ?? "—"}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
