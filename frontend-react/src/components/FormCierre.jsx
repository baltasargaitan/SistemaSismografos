import { useState, useEffect, useRef, useCallback } from "react";
import { createPortal } from "react-dom";
import SimpleSelect from "./SimpleSelect";
import SpinnerOverlay from "./SpinnerOverlay";
import { motion, AnimatePresence } from "framer-motion";
import { Trash2, Plus, HelpCircle, CheckCircle2, AlertCircle } from "lucide-react";

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
      <div className="p-8 border-2 border-blue-400/30 rounded-2xl text-center bg-linear-to-br from-blue-50/10 to-gray-50/10 backdrop-blur-sm select-none">
        <div className="flex flex-col items-center gap-4">
          <div className="w-16 h-16 rounded-full bg-blue-500/20 flex items-center justify-center">
            <AlertCircle className="w-8 h-8 text-blue-400" />
          </div>
          <div>
            <h3 className="text-lg font-semibold text-gray-200 mb-2">
              Ninguna orden seleccionada
            </h3>
            <p className="text-sm text-gray-400">
              Seleccioná una orden de la lista para comenzar el proceso de cierre
            </p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <>
    <form
      ref={formRef}
      onSubmit={validarYConfirmar}
      className="space-y-4 text-white"
    >
      {/* Información de la orden */}
      <div className="border-2 border-green-500/30 rounded-2xl p-4 bg-linear-to-br from-green-50/10 to-blue-50/10 backdrop-blur-sm space-y-2 text-sm">
        <div className="flex items-center gap-2 mb-3 pb-2 border-b border-green-400/20">
          <CheckCircle2 className="w-5 h-5 text-green-400" />
          <h3 className="font-semibold text-green-300">Información de la Orden</h3>
        </div>
        <p className="flex justify-between">
          <span className="text-gray-400">Orden:</span>
          <span className="font-semibold text-white">#{orden.nroOrden}</span>
        </p>
        <p className="flex justify-between">
          <span className="text-gray-400">Estación:</span>
          <span className="font-medium text-white">{orden.estacion ?? "—"}</span>
        </p>
        <p className="flex justify-between">
          <span className="text-gray-400">Estado:</span>
          <span className="px-2 py-1 rounded-lg bg-green-500/20 text-green-300 text-xs font-semibold">
            {orden.estado ?? "—"}
          </span>
        </p>
      </div>

      {/* Observación con ayuda contextual */}
      <div>
        <div className="flex items-center gap-2 mb-2">
          <label className="text-sm font-medium text-blue-200">
            Observación General
          </label>
          <div 
            className="group relative cursor-help"
            data-tooltip="Describí brevemente el resultado de la inspección y el estado general del equipo"
          >
            <HelpCircle className="w-4 h-4 text-blue-400/60 hover:text-blue-400 transition-colors" />
          </div>
        </div>
        <textarea
          placeholder="Ejemplo: 'Inspección completada. Se detectaron problemas de conectividad y fallas eléctricas intermitentes en el sismógrafo...'"
          className={`w-full rounded-xl p-3 mt-1 bg-white/10 text-white border-2 ${
            intentoEnvio && !observacion.trim()
              ? "border-red-500 focus:ring-2 focus:ring-red-400"
              : "border-blue-400/30 focus:ring-2 focus:ring-blue-400"
          } focus:outline-none transition-all placeholder:text-gray-500`}
          rows="4"
          value={observacion}
          onChange={(e) => {
            setObservacion(e.target.value);
            setIntentoEnvio(false);
            setBloquearModal(true);
          }}
        />
        {intentoEnvio && !observacion.trim() && (
          <motion.p 
            initial={{ opacity: 0, y: -10 }}
            animate={{ opacity: 1, y: 0 }}
            className="text-red-400 text-xs mt-2 flex items-center gap-1"
          >
            <AlertCircle className="w-3 h-3" />
            La observación es obligatoria para cerrar la orden
          </motion.p>
        )}
        {observacion.trim() && (
          <motion.p 
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            className="text-green-400 text-xs mt-2 flex items-center gap-1"
          >
            <CheckCircle2 className="w-3 h-3" />
            Observación completa
          </motion.p>
        )}
      </div>

      {/* Motivos con ayuda contextual */}
      <div className="space-y-3 relative">
        <div className="flex items-center gap-2 mb-3">
          <h3 className="text-sm font-medium text-yellow-200">
            Motivos de Falla del Sismógrafo
          </h3>
          <div 
            className="group relative cursor-help"
            data-tooltip="Seleccioná los motivos técnicos que causaron la baja del equipo. Podés agregar múltiples motivos si es necesario"
          >
            <HelpCircle className="w-4 h-4 text-yellow-400/60 hover:text-yellow-400 transition-colors" />
          </div>
        </div>
        
        {motivosList.map((m, i) => (
          <motion.div
            key={i}
            initial={{ opacity: 0, scale: 0.95 }}
            animate={{ opacity: 1, scale: 1 }}
            exit={{ opacity: 0, scale: 0.95 }}
            className="relative border-2 border-yellow-400/30 bg-yellow-50/5 rounded-xl p-4 backdrop-blur-sm hover:border-yellow-400/50 transition-all mb-3"
            style={{ zIndex: motivosList.length - i }}
          >
            {motivosList.length > 1 && (
              <motion.button
                type="button"
                onClick={() => quitarMotivo(i)}
                whileHover={{ scale: 1.1 }}
                whileTap={{ scale: 0.9 }}
                className="absolute top-2 right-2 p-1.5 rounded-lg bg-red-600/30 text-red-300 hover:bg-red-600/50 hover:text-red-200 transition-all z-10"
                data-tooltip="Eliminar este motivo"
              >
                <Trash2 className="h-4 w-4" />
              </motion.button>
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
                  ? "⚠️ Seleccioná un motivo de la lista"
                  : null
              }
            />
            <div className="mt-2">
              <div className="flex items-center gap-2 mb-1">
                <label className="text-xs text-gray-400">Comentario adicional (opcional)</label>
                <div 
                  className="group relative cursor-help"
                  data-tooltip="Agregá detalles específicos sobre este motivo si es necesario"
                >
                  <HelpCircle className="w-3 h-3 text-gray-500 hover:text-gray-400 transition-colors" />
                </div>
              </div>
              <input
                placeholder="Ej: 'Cable principal dañado en sector norte...'"
                className="w-full border-2 border-gray-400/20 bg-white/5 rounded-lg p-2 text-sm text-white placeholder:text-gray-600 focus:outline-none focus:ring-2 focus:ring-yellow-400 focus:border-transparent transition-all"
                value={m.comentario}
                onChange={(e) =>
                  actualizarMotivo(i, "comentario", e.target.value)
                }
                type="text"
              />
            </div>
          </motion.div>
        ))}
        
        {/* Botón al final con margen grande para evitar que los dropdowns lo toquen */}
        <div className="pt-4">
          <motion.button
            type="button"
            whileHover={{ scale: 1.02 }}
            whileTap={{ scale: 0.98 }}
            className="relative w-full flex items-center justify-center gap-2 px-4 py-3 text-sm font-medium bg-yellow-600/30 text-yellow-200 border-2 border-yellow-400/40 rounded-xl hover:bg-yellow-600/40 hover:border-yellow-400/60 transition-all shadow-lg"
            style={{ backgroundColor: 'rgba(202, 138, 4, 0.3)', zIndex: 1 }}
            onClick={agregarMotivo}
            data-tooltip="Podés registrar múltiples motivos si el sismógrafo tiene varios problemas"
          >
            <Plus className="h-5 w-5" /> Agregar otro motivo
          </motion.button>
        </div>
      </div>

      {/* Botón de acción con feedback visual */}
      <motion.button
        type="submit"
        disabled={busy}
        whileHover={{ scale: 1.02 }}
        whileTap={{ scale: 0.98 }}
        className={`w-full px-6 py-3 rounded-xl font-semibold shadow-lg transition-all ${
          busy 
            ? "bg-gray-600 cursor-not-allowed" 
            : "bg-linear-to-r from-green-600 to-blue-600 hover:from-green-500 hover:to-blue-500"
        } text-white`}
      >
        {busy ? (
          <span className="inline-flex items-center gap-2">
            <motion.span
              className="w-5 h-5 border-2 border-t-transparent border-white rounded-full"
              animate={{ rotate: 360 }}
              transition={{ repeat: Infinity, duration: 0.8, ease: "linear" }}
            />
            Procesando cierre de orden...
          </span>
        ) : (
          <span className="flex items-center justify-center gap-2">
            <CheckCircle2 className="w-5 h-5" />
            Cerrar Orden de Inspección
          </span>
        )}
      </motion.button>

      {busy && <SpinnerOverlay label="⏳ Cerrando orden, notificando responsables y actualizando estado del sismógrafo..." />}
    </form>
    
    {/* Modal de confirmación - USANDO PORTAL para renderizar en el body */}
    {mostrarModal && !bloquearModal && createPortal(
      <motion.div
        className="fixed inset-0 flex items-center justify-center p-4"
        style={{ 
          zIndex: 9999,
          backgroundColor: 'rgba(0, 0, 0, 0.90)',
          backdropFilter: 'blur(10px)'
        }}
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        exit={{ opacity: 0 }}
        onClick={(e) => {
          // Cerrar modal si hacen click en el backdrop
          if (e.target === e.currentTarget) {
            setMostrarModal(false);
          }
        }}
      >
        <motion.div
          className="rounded-2xl p-8 shadow-2xl max-w-lg w-full select-none"
          style={{
            background: 'linear-gradient(to bottom right, #1a1d23, #0f1115)',
            border: '2px solid rgba(59, 130, 246, 0.5)'
          }}
          initial={{ scale: 0.9, opacity: 0, y: 20 }}
          animate={{ scale: 1, opacity: 1, y: 0 }}
            exit={{ scale: 0.9, opacity: 0, y: 20 }}
          >
            <div className="flex items-center gap-3 mb-4">
              <div className="w-12 h-12 rounded-full bg-yellow-500/20 flex items-center justify-center">
                <AlertCircle className="w-6 h-6 text-yellow-400" />
              </div>
              <h2 className="text-2xl font-bold text-blue-300">
                Confirmar Cierre de Orden
              </h2>
            </div>
            
            <div className="bg-blue-500/10 border border-blue-400/30 rounded-lg p-4 mb-4">
              <p className="text-sm text-gray-200 mb-3">
                Estás a punto de cerrar la orden{" "}
                <span className="font-bold text-green-400">
                  #{orden.nroOrden}
                </span>{" "}
                de la estación{" "}
                <span className="font-bold text-green-400">
                  {orden.estacion}
                </span>
              </p>
              <div className="text-xs text-gray-400 space-y-1">
                <p className="flex items-center gap-2">
                  <CheckCircle2 className="w-3 h-3 text-green-400" />
                  El sismógrafo será marcado como fuera de servicio
                </p>
                <p className="flex items-center gap-2">
                  <CheckCircle2 className="w-3 h-3 text-green-400" />
                  Se notificará a todos los responsables de reparación
                </p>
                <p className="flex items-center gap-2">
                  <AlertCircle className="w-3 h-3 text-yellow-400" />
                  Esta acción no se puede revertir
                </p>
              </div>
            </div>

            <p className="text-sm text-gray-400 mb-6 italic">
              ¿Estás seguro de que deseas continuar?
            </p>

            <div className="flex justify-end gap-3">
              <motion.button
                type="button"
                onClick={() => setMostrarModal(false)}
                whileHover={{ scale: 1.05 }}
                whileTap={{ scale: 0.95 }}
                className="px-5 py-2.5 rounded-lg border-2 border-gray-600 text-gray-300 hover:bg-gray-700 hover:border-gray-500 transition-all font-medium"
              >
                ✕ Cancelar
              </motion.button>
              <motion.button
                type="button"
                onClick={confirmarCierre}
                whileHover={{ scale: 1.05 }}
                whileTap={{ scale: 0.95 }}
                className="px-5 py-2.5 rounded-lg bg-linear-to-r from-green-600 to-blue-600 hover:from-green-500 hover:to-blue-500 text-white font-bold shadow-lg transition-all flex items-center gap-2"
              >
                <CheckCircle2 className="w-4 h-4" />
                Sí, cerrar orden
              </motion.button>
            </div>
          </motion.div>
        </motion.div>,
      document.body
    )}
    </>
  );
}
// Force reload
