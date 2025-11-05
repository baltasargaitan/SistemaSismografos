import { useState, useEffect } from "react";
import SimpleSelect from "./SimpleSelect";
import Spinner from "./Spinner";
import Toast from "./Toast";

export default function FormCierre({ orden, motivos, onSubmit, busy }) {
  const [observacion, setObservacion] = useState("");
  const [motivosList, setMotivosList] = useState([{ motivo: "", comentario: "" }]);
  const [confirmar, setConfirmar] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    setObservacion("");
    setMotivosList([{ motivo: "", comentario: "" }]);
    setConfirmar(false);
    setError(null);
  }, [orden?.nroOrden]);

  if (!orden)
    return (
      <div className="p-6 border rounded-xl text-center text-gray-500">
        Seleccioná una orden para cerrarla.
      </div>
    );

  function agregarMotivo() {
    setMotivosList([...motivosList, { motivo: "", comentario: "" }]);
  }

  function actualizarMotivo(index, field, value) {
    const nuevos = [...motivosList];
    nuevos[index][field] = value;
    setMotivosList(nuevos);
  }

  async function enviar(e) {
    e.preventDefault();
    if (!observacion.trim()) return setError("Debe ingresar una observación.");
    if (!confirmar) return setError("Debes confirmar el cierre.");
    if (motivosList.some((m) => !m.motivo)) return setError("Selecciona al menos un motivo.");

    const motivosTipos = motivosList.map((m) => m.motivo);
    const comentarios = motivosList.map((m) => m.comentario);

    await onSubmit({
      NroOrden: orden.nroOrden,
      Observacion: observacion,
      Confirmar: confirmar,
      MotivosTipo: motivosTipos,
      Comentarios: comentarios,
    });
  }

  return (
    <form onSubmit={enviar} className="space-y-4" tabIndex={-1}>
      {error && <Toast kind="error" msg={error} onClose={() => setError(null)} />}

      <div className="border rounded-2xl p-4 space-y-2 text-sm">
        <p>
          <strong>Orden:</strong> {orden.nroOrden}
        </p>
        <p>
          <strong>Estación:</strong> {orden.estacion ?? "—"}
        </p>
        <p>
          <strong>Estado:</strong> {orden.estado ?? "—"}
        </p>
      </div>

      <div>
        <label className="text-sm font-medium">Observación</label>
        <textarea
          className="w-full border rounded-xl p-2 mt-1"
          rows="4"
          value={observacion}
          onChange={(e) => setObservacion(e.target.value)}
        />
      </div>

      <div className="space-y-3">
        {motivosList.map((m, i) => (
          <div key={i} className="border rounded-xl p-3">
            <SimpleSelect
              label={`Motivo ${i + 1}`}
              value={m.motivo}
              onChange={(v) => actualizarMotivo(i, "motivo", v)}
              options={motivos}
            />
            <input
              placeholder="Comentario (opcional)"
              className="w-full border rounded-xl p-2 mt-2"
              value={m.comentario}
              onChange={(e) => actualizarMotivo(i, "comentario", e.target.value)}
            />
          </div>
        ))}
        <button
          type="button"
          className="text-sm text-blue-600 underline"
          onClick={agregarMotivo}
        >
          + Agregar otro motivo
        </button>
      </div>

      <label className="flex items-center gap-2 text-sm">
        <input
          type="checkbox"
          checked={confirmar}
          onChange={(e) => setConfirmar(e.target.checked)}
        />
        Confirmo el cierre definitivo de la orden.
      </label>

      <button
        type="submit"
        disabled={busy}
        className="bg-black text-white px-4 py-2 rounded-xl hover:opacity-90"
      >
        {busy ? "Cerrando..." : "Cerrar orden"}
      </button>
      {busy && <Spinner label="Procesando cierre" />}
    </form>
  );
}
