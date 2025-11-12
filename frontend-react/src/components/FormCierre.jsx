import { useState, useEffect, useRef, useCallback } from "react";
import SimpleSelect from "./SimpleSelect";
import SpinnerOverlay from "./SpinnerOverlay";
import { motion, AnimatePresence } from "framer-motion";
import { Trash2, Plus } from "lucide-react";

export default function FormCierre({ orden, motivos, onSubmit, busy }) {
  const [observacion, setObservacion] = useState("");
  const [motivosList, setMotivosList] = useState([{ motivo: "", comentario: "" }]);
  const [mostrarModal, setMostrarModal] = useState(false);
  const [intentoEnvio, setIntentoEnvio] = useState(false);
  const [bloquearModal, setBloquearModal] = useState(false);
  const formRef = useRef(null);

  // 🔹 Reset cuando cambia la orden seleccionada
  useEffect(() => {
    if (!orden) return;
    setObservacion("");
    setMotivosList([{ motivo: "", comentario: "" }]);
    setMostrarModal(false);
    setIntentoEnvio(false);
    setBloquearModal(false);
  }, [orden?.nroOrden]);

  const agregarMotivo = () => {
    setMotivosList([...motivosList, { motivo: "", comentario: "" }]);
    setBloquearModal(true);
  };

  const quitarMotivo = (index) => {
    setMotivosList(motivosList.filter((_, i) => i !== index));
    setBloquearModal(true);
  };

  const actualizarMotivo = useCallback(
    (index, field, value) => {
      const nuevos = [...motivosList];
      nuevos[index][field] = value;
      setMotivosList(nuevos);
      setIntentoEnvio(false);
      setBloquearModal(true);
    },
    [motivosList]
  );

  async function confirmarCierre() {
    setMostrarModal(false);
    const motivosTipos = motivosList.map((m) => m.motivo);
    const comentarios = motivosList.map((m) => m.comentario);

    await onSubmit({
      NroOrden: orden.nroOrden,
      Observacion: observacion,
      Confirmar: true,
      MotivosTipo: motivosTipos,
      Comentarios: comentarios,
    });

    setIntentoEnvio(false);
    setBloquearModal(false);
  }

  function validarYConfirmar(e) {
    e.preventDefault();
    setIntentoEnvio(true);
    setBloquearModal(false);
    if (!observacion.trim()) return;
    if (motivosList.some((m) => !m.motivo)) return;
    setMostrarModal(true);
  }

  // 🔹 Render seguro: los hooks siempre se ejecutan
  if (!orden) {
    return (
      <div className="p-6 border border-cyan-400/20 rounded-2xl text-center text-gray-400 bg-white/5 backdrop-blur-sm select-none">
        Seleccioná una orden para cerrarla.
      </div>
    );
  }

  return (
    <form
      ref={formRef}
      onSubmit={validarYConfirmar}
      className="space-y-4 text-white"
    >
      {/* Información */}
      <div className="border border-cyan-400/30 rounded-2xl p-4 bg-white/5 backdrop-blur-sm space-y-2 text-sm">
        <p><strong>Orden:</strong> {orden.nroOrden}</p>
        <p><strong>Estación:</strong> {orden.estacion ?? "—"}</p>
        <p>
          <strong>Estado:</strong>{" "}
          <span className="text-cyan-300">{orden.estado ?? "—"}</span>
        </p>
      </div>

      {/* Observación */}
      <div>
        <label className="text-sm font-medium text-cyan-200">Observación</label>
        <textarea
          className={`w-full rounded-xl p-2 mt-1 bg-white/5 text-white border ${
            intentoEnvio && !observacion.trim()
              ? "border-red-500"
              : "border-cyan-400/30"
          } focus:ring-1 focus:ring-cyan-400`}
          rows="4"
          value={observacion}
          onChange={(e) => {
            setObservacion(e.target.value);
            setIntentoEnvio(false);
            setBloquearModal(true);
          }}
        />
        {intentoEnvio && !observacion.trim() && (
          <p className="text-red-500 text-xs mt-1">
            Debe ingresar una observación.
          </p>
        )}
      </div>

      {/* Motivos */}
      <div className="space-y-3">
        {motivosList.map((m, i) => (
          <motion.div
            key={i}
            initial={{ opacity: 0, scale: 0.95 }}
            animate={{ opacity: 1, scale: 1 }}
            exit={{ opacity: 0, scale: 0.95 }}
            className="border border-cyan-400/20 bg-white/5 rounded-xl p-3 backdrop-blur-sm relative"
          >
            {motivosList.length > 1 && (
              <button
                type="button"
                onClick={() => quitarMotivo(i)}
                className="absolute top-2 right-2 text-red-500 hover:text-red-400 transition-all"
              >
                <Trash2 className="h-4 w-4" />
              </button>
            )}
            <SimpleSelect
              label={`Motivo ${i + 1}`}
              value={m.motivo}
              onChange={(v) => actualizarMotivo(i, "motivo", v)}
              options={motivos.filter(
                (mot) =>
                  !motivosList.some((m2, j) => j !== i && m2.motivo === mot)
              )}
              error={
                intentoEnvio && !m.motivo
                  ? "Debe seleccionar un motivo."
                  : null
              }
            />
            <input
              placeholder="Comentario (opcional)"
              className="w-full border border-cyan-400/20 bg-white/10 rounded-xl p-2 mt-2 focus:ring-1 focus:ring-cyan-400"
              value={m.comentario}
              onChange={(e) =>
                actualizarMotivo(i, "comentario", e.target.value)
              }
              type="text"
            />
          </motion.div>
        ))}
        <button
          type="button"
          className="flex items-center gap-1 text-sm text-cyan-400 underline hover:text-cyan-300 transition-all"
          onClick={agregarMotivo}
        >
          <Plus className="h-4 w-4" /> Agregar otro motivo
        </button>
      </div>

      {/* Botón de acción */}
      <button
        type="submit"
        disabled={busy}
        className="w-full bg-linear-to-r from-cyan-600 to-blue-700 hover:from-cyan-500 hover:to-blue-600 text-white px-4 py-2 rounded-xl shadow-lg transition-all"
      >
        {busy ? (
          <span className="inline-flex items-center gap-2">
            <motion.span
              className="w-4 h-4 border-2 border-t-transparent border-white rounded-full"
              animate={{ rotate: 360 }}
              transition={{ repeat: Infinity, duration: 0.8, ease: "linear" }}
            />
            Procesando...
          </span>
        ) : (
          "Cerrar orden"
        )}
      </button>

      {busy && <SpinnerOverlay label="Procesando cierre de orden..." />}

      <AnimatePresence>
        {mostrarModal && !bloquearModal && (
          <motion.div
            className="fixed inset-0 bg-black/70 flex items-center justify-center z-50"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
          >
            <motion.div
              className="bg-gray-900 border border-cyan-500/30 rounded-2xl p-6 shadow-2xl max-w-md w-[90%] select-none"
              initial={{ scale: 0.9, opacity: 0 }}
              animate={{ scale: 1, opacity: 1 }}
              exit={{ scale: 0.9, opacity: 0 }}
            >
              <h2 className="text-xl font-semibold text-cyan-300 mb-2">
                Confirmar cierre de orden
              </h2>
              <p className="text-sm text-gray-300 mb-4">
                Estás a punto de cerrar definitivamente la orden{" "}
                <span className="text-cyan-400 font-semibold">
                  #{orden.nroOrden}
                </span>{" "}
                correspondiente a la estación{" "}
                <span className="text-cyan-400 font-semibold">
                  {orden.estacion}
                </span>.
              </p>
              <p className="text-sm text-gray-400 mb-4">
                Esta acción notificará al responsable y no podrá revertirse.
              </p>

              <div className="flex justify-end gap-3 mt-6">
                <button
                  type="button"
                  onClick={() => setMostrarModal(false)}
                  className="px-4 py-2 rounded-lg border border-gray-600 text-gray-300 hover:bg-gray-800 transition-all"
                >
                  Cancelar
                </button>
                <button
                  type="button"
                  onClick={confirmarCierre}
                  className="px-4 py-2 rounded-lg bg-cyan-600 hover:bg-cyan-500 text-white font-medium transition-all"
                >
                  Confirmar cierre
                </button>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </form>
  );
}
