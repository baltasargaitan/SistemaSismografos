import { Check, AlertTriangle } from "lucide-react";

export default function Toast({ kind, msg, onClose }) {
  const bg =
    kind === "success"
      ? "bg-green-50 border-green-300 text-green-700"
      : kind === "error"
      ? "bg-red-50 border-red-300 text-red-700"
      : "bg-blue-50 border-blue-300 text-blue-700";

  return (
    <div className={`border rounded-xl px-4 py-3 text-sm flex items-start gap-2 ${bg}`}>
      {kind === "success" ? (
        <Check className="h-4 w-4 mt-0.5" />
      ) : (
        <AlertTriangle className="h-4 w-4 mt-0.5" />
      )}
      <span className="flex-1">{msg}</span>
      {onClose && (
        <button
          onClick={onClose}
          className="text-xs underline underline-offset-2 hover:opacity-70"
        >
          Cerrar
        </button>
      )}
    </div>
  );
}
