import { useState, useEffect } from "react";
import { createPortal } from "react-dom";
import SimpleSelect from "./SimpleSelect";
import SpinnerOverlay from "./SpinnerOverlay";
import Tooltip from "./Tooltip";
import { motion } from "framer-motion";
import { Trash2, Plus, HelpCircle, CheckCircle2, AlertCircle } from "lucide-react";

export default function FormCierre({ orden, motivos = [], onSubmit, busy }) {
  const [observacion, setObservacion] = useState("");
  const [motivosList, setMotivosList] = useState([{ motivo: "", comentario: "" }]);
  const [mostrarModal, setMostrarModal] = useState(false);
  const [mostrarExito, setMostrarExito] = useState(false);
  const [datosEnviados, setDatosEnviados] = useState(null);
  const [intentoEnvio, setIntentoEnvio] = useState(false);

  // Reset cuando cambia la orden seleccionada
  useEffect(() => {
    setObservacion("");
    setMotivosList([{ motivo: "", comentario: "" }]);
    setMostrarModal(false);
    setIntentoEnvio(false);
    // Resetear popup de éxito cuando cambiamos de orden
    setMostrarExito(false);
    setDatosEnviados(null);
  }, [orden?.nroOrden]);

  const agregarMotivo = () => {
    setMotivosList([...motivosList, { motivo: "", comentario: "" }]);
  };

  const quitarMotivo = (index) => {
    setMotivosList(motivosList.filter((_, i) => i !== index));
  };

  const actualizarMotivo = (index, field, value) => {
    const nuevos = [...motivosList];
    nuevos[index][field] = value;
    setMotivosList(nuevos);
    setIntentoEnvio(false);
  };

  const validarYMostrarModal = (e) => {
    e.preventDefault();
    setIntentoEnvio(true);

    // Validar campos obligatorios
    if (!observacion.trim()) return;
    if (motivosList.some((m) => !m.motivo)) return;

    // Mostrar modal de confirmación
    setMostrarModal(true);
  };

  const confirmarCierre = async () => {
    setMostrarModal(false);

    const payload = {
      NroOrden: orden.nroOrden,
      Observacion: observacion,
      Confirmar: true,
      MotivosTipo: motivosList.map((m) => m.motivo),
      Comentarios: motivosList.map((m) => m.comentario),
    };

    // Guardar los datos para mostrarlos en el popup de éxito
    const datosParaExito = {
      nroOrden: orden.nroOrden,
      estacion: orden.estacion,
      observacion: observacion,
      motivos: motivosList.filter(m => m.motivo).map(m => ({
        motivo: m.motivo,
        comentario: m.comentario
      }))
    };

    // Llamar al submit primero
    await onSubmit(payload);
    setIntentoEnvio(false);
    
    // Mostrar popup de éxito DESPUÉS del envío exitoso
    // Usamos setTimeout para asegurar que se muestre después de que el padre actualice el estado
    setTimeout(() => {
      setDatosEnviados(datosParaExito);
      setMostrarExito(true);
    }, 100);
  };

  const cancelarCierre = () => {
    // Resetear el formulario
    setObservacion("");
    setMotivosList([{ motivo: "", comentario: "" }]);
    setIntentoEnvio(false);
  };

  // Filtrar motivos ya seleccionados
  const getMotivosDisponibles = (currentIndex) => {
    return motivos.filter(
      (mot) => !motivosList.some((m, i) => i !== currentIndex && m.motivo === mot)
    );
  };

  // Mostrar mensaje si no hay orden seleccionada
  if (!orden) {
    return (
      <>
        <div className="p-8 border-2 border-blue-400/30 rounded-2xl text-center bg-linear-to-br from-blue-50/10 to-gray-50/10 backdrop-blur-sm">
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

        {/* Modal de éxito - Se muestra incluso sin orden seleccionada */}
        {mostrarExito && datosEnviados && createPortal(
          <div
            className="fixed inset-0 flex items-center justify-center p-4 bg-black/90 backdrop-blur-md"
            style={{ zIndex: 99999 }}
          >
            <motion.div
              initial={{ scale: 0.9, opacity: 0, y: 20 }}
              animate={{ scale: 1, opacity: 1, y: 0 }}
              className="bg-linear-to-br from-green-900 to-gray-800 rounded-2xl p-8 shadow-2xl max-w-2xl w-full border-2 border-green-400/50"
            >
              <div className="flex items-center gap-3 mb-6">
                <div className="w-14 h-14 rounded-full bg-green-500/20 flex items-center justify-center">
                  <CheckCircle2 className="w-8 h-8 text-green-400" />
                </div>
                <div>
                  <h2 className="text-2xl font-bold text-green-300">
                    ¡Orden Cerrada Exitosamente!
                  </h2>
                  <p className="text-sm text-gray-400 mt-1">
                    Notificaciones enviadas correctamente
                  </p>
                </div>
              </div>

              <div className="space-y-4">
                {/* Información de la orden */}
                <div className="bg-green-500/10 border border-green-400/30 rounded-lg p-4">
                  <h3 className="font-semibold text-green-300 mb-3 flex items-center gap-2">
                    <CheckCircle2 className="w-4 h-4" />
                    Información de la Orden Cerrada
                  </h3>
                  <div className="grid grid-cols-2 gap-3 text-sm">
                    <div>
                      <span className="text-gray-400">Orden N°:</span>
                      <span className="ml-2 font-bold text-white">#{datosEnviados.nroOrden}</span>
                    </div>
                    <div>
                      <span className="text-gray-400">Estación:</span>
                      <span className="ml-2 font-medium text-white">{datosEnviados.estacion}</span>
                    </div>
                  </div>
                  <div className="mt-3">
                    <span className="text-gray-400 text-sm">Observación:</span>
                    <p className="mt-1 text-white text-sm bg-black/20 rounded p-2">
                      {datosEnviados.observacion}
                    </p>
                  </div>
                </div>

                {/* Motivos registrados */}
                <div className="bg-yellow-500/10 border border-yellow-400/30 rounded-lg p-4">
                  <h3 className="font-semibold text-yellow-300 mb-3 flex items-center gap-2">
                    <AlertCircle className="w-4 h-4" />
                    Motivos Registrados ({datosEnviados.motivos.length})
                  </h3>
                  <div className="space-y-2">
                    {datosEnviados.motivos.map((m, i) => (
                      <div key={i} className="bg-black/20 rounded p-2 text-sm">
                        <div className="font-medium text-yellow-200">
                          {i + 1}. {m.motivo}
                        </div>
                        {m.comentario && (
                          <div className="text-gray-400 text-xs mt-1 ml-4">
                            💬 {m.comentario}
                          </div>
                        )}
                      </div>
                    ))}
                  </div>
                </div>

                {/* Acciones realizadas */}
                <div className="bg-blue-500/10 border border-blue-400/30 rounded-lg p-4">
                  <h3 className="font-semibold text-blue-300 mb-3">
                    ✅ Acciones Ejecutadas
                  </h3>
                  <div className="space-y-2 text-sm">
                    <p className="flex items-center gap-2 text-gray-300">
                      <CheckCircle2 className="w-4 h-4 text-green-400" />
                      Orden de inspección cerrada
                    </p>
                    <p className="flex items-center gap-2 text-gray-300">
                      <CheckCircle2 className="w-4 h-4 text-green-400" />
                      Sismógrafo marcado como "Fuera de Servicio"
                    </p>
                    <p className="flex items-center gap-2 text-gray-300">
                      <CheckCircle2 className="w-4 h-4 text-green-400" />
                      Notificación enviada a la consola CCRS
                    </p>
                    <p className="flex items-center gap-2 text-gray-300">
                      <CheckCircle2 className="w-4 h-4 text-green-400" />
                      Emails enviados a responsables de reparación
                    </p>
                    <p className="flex items-center gap-2 text-gray-300">
                      <CheckCircle2 className="w-4 h-4 text-green-400" />
                      Evento registrado en el monitor del sistema
                    </p>
                  </div>
                </div>
              </div>

              <div className="flex justify-end gap-3 mt-6">
                <button
                  type="button"
                  onClick={() => {
                    setMostrarExito(false);
                    setDatosEnviados(null);
                  }}
                  className="px-6 py-2.5 rounded-lg bg-linear-to-r from-green-600 to-blue-600 hover:from-green-500 hover:to-blue-500 text-white font-bold shadow-lg transition-all flex items-center gap-2"
                >
                  <CheckCircle2 className="w-4 h-4" />
                  Entendido
                </button>
              </div>
            </motion.div>
          </div>,
          document.body
        )}
      </>
    );
  }

  return (
    <>
      <form onSubmit={validarYMostrarModal} className="space-y-5 text-white">
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

        {/* Observación */}
        <div>
          <div className="flex items-center gap-2 mb-2">
            <label className="text-sm font-medium text-blue-200">
              Observación General
            </label>
            <Tooltip text="Describí brevemente el resultado de la inspección">
              <HelpCircle className="w-4 h-4 text-blue-400/60 cursor-help" />
            </Tooltip>
          </div>
          <textarea
            placeholder="Ejemplo: 'Inspección completada. Se detectaron problemas de conectividad y fallas eléctricas intermitentes...'"
            className={`w-full rounded-xl p-3 bg-white/10 text-white border-2 ${
              intentoEnvio && !observacion.trim()
                ? "border-red-500 focus:ring-2 focus:ring-red-400"
                : "border-blue-400/30 focus:ring-2 focus:ring-blue-400"
            } focus:outline-none transition-all placeholder:text-gray-500`}
            rows="4"
            value={observacion}
            onChange={(e) => {
              setObservacion(e.target.value);
              setIntentoEnvio(false);
            }}
          />
          {intentoEnvio && !observacion.trim() && (
            <motion.p
              initial={{ opacity: 0, y: -10 }}
              animate={{ opacity: 1, y: 0 }}
              className="text-red-400 text-xs mt-2 flex items-center gap-1"
            >
              <AlertCircle className="w-3 h-3" />
              La observación es obligatoria
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

        {/* Motivos */}
        <div className="space-y-4">
          <div className="flex items-center gap-2">
            <h3 className="text-sm font-medium text-yellow-200">
              Motivos de Falla del Sismógrafo
            </h3>
            <Tooltip text="Seleccioná los motivos técnicos que causaron la baja del equipo">
              <HelpCircle className="w-4 h-4 text-yellow-400/60 cursor-help" />
            </Tooltip>
          </div>

          {motivosList.map((m, i) => (
            <motion.div
              key={i}
              initial={{ opacity: 0, scale: 0.95 }}
              animate={{ opacity: 1, scale: 1 }}
              className="border-2 border-yellow-400/30 bg-yellow-50/5 rounded-xl p-4 space-y-3"
            >
              {motivosList.length > 1 && (
                <button
                  type="button"
                  onClick={() => quitarMotivo(i)}
                  className="float-right p-1.5 rounded-lg bg-red-600/30 text-red-300 hover:bg-red-600/50 transition-all"
                  title="Eliminar este motivo"
                >
                  <Trash2 className="h-4 w-4" />
                </button>
              )}

              <SimpleSelect
                label={`Motivo ${i + 1}`}
                value={m.motivo}
                onChange={(v) => actualizarMotivo(i, "motivo", v)}
                options={getMotivosDisponibles(i)}
                error={
                  intentoEnvio && !m.motivo
                    ? "⚠️ Seleccioná un motivo de la lista"
                    : null
                }
              />

              <div>
                <label className="text-xs text-gray-400 flex items-center gap-1 mb-1">
                  Comentario adicional (opcional)
                  <Tooltip text="Agregá detalles específicos sobre este motivo">
                    <HelpCircle className="w-3 h-3 text-gray-500 cursor-help" />
                  </Tooltip>
                </label>
                <input
                  type="text"
                  placeholder="Ej: 'Cable principal dañado en sector norte...'"
                  className="w-full border-2 border-gray-400/20 bg-white/5 rounded-lg p-2 text-sm text-white placeholder:text-gray-600 focus:outline-none focus:ring-2 focus:ring-yellow-400 transition-all"
                  value={m.comentario}
                  onChange={(e) => actualizarMotivo(i, "comentario", e.target.value)}
                />
              </div>
            </motion.div>
          ))}

          <button
            type="button"
            onClick={agregarMotivo}
            className="w-full flex items-center justify-center gap-2 px-4 py-3 text-sm font-medium bg-yellow-600/30 text-yellow-200 border-2 border-yellow-400/40 rounded-xl hover:bg-yellow-600/40 hover:border-yellow-400/60 transition-all"
          >
            <Plus className="h-5 w-5" /> Agregar otro motivo
          </button>
        </div>

        {/* Botones de acción */}
        <div className="flex gap-3">
          <button
            type="button"
            onClick={cancelarCierre}
            className="flex-1 px-6 py-3 rounded-xl font-semibold shadow-lg transition-all flex items-center justify-center gap-2 bg-gray-700 hover:bg-gray-600 text-white border-2 border-gray-600 hover:border-gray-500"
          >
            ✕ Cancelar y Limpiar
          </button>

          <button
            type="submit"
            disabled={busy}
            className={`flex-1 px-6 py-3 rounded-xl font-semibold shadow-lg transition-all flex items-center justify-center gap-2 ${
              busy
                ? "bg-gray-600 cursor-not-allowed"
                : "bg-linear-to-r from-green-600 to-blue-600 hover:from-green-500 hover:to-blue-500"
            } text-white`}
          >
          {busy ? (
            <>
              <motion.span
                className="w-5 h-5 border-2 border-t-transparent border-white rounded-full"
                animate={{ rotate: 360 }}
                transition={{ repeat: Infinity, duration: 0.8, ease: "linear" }}
              />
              Procesando cierre de orden...
            </>
          ) : (
            <>
              <CheckCircle2 className="w-5 h-5" />
              Cerrar Orden de Inspección
            </>
          )}
          </button>
        </div>

        {busy && (
          <SpinnerOverlay label="⏳ Cerrando orden, notificando responsables y actualizando estado del sismógrafo..." />
        )}
      </form>

      {/* Modal de éxito - Información enviada */}
      {mostrarExito && datosEnviados && createPortal(
        <div
          className="fixed inset-0 flex items-center justify-center p-4 bg-black/90 backdrop-blur-md"
          style={{ zIndex: 99999 }}
        >
          <motion.div
            initial={{ scale: 0.9, opacity: 0, y: 20 }}
            animate={{ scale: 1, opacity: 1, y: 0 }}
            className="bg-linear-to-br from-green-900 to-gray-800 rounded-2xl p-8 shadow-2xl max-w-2xl w-full border-2 border-green-400/50"
          >
            <div className="flex items-center gap-3 mb-6">
              <div className="w-14 h-14 rounded-full bg-green-500/20 flex items-center justify-center">
                <CheckCircle2 className="w-8 h-8 text-green-400" />
              </div>
              <div>
                <h2 className="text-2xl font-bold text-green-300">
                  ¡Orden Cerrada Exitosamente!
                </h2>
                <p className="text-sm text-gray-400 mt-1">
                  Notificaciones enviadas correctamente
                </p>
              </div>
            </div>

            <div className="space-y-4">
              {/* Información de la orden */}
              <div className="bg-green-500/10 border border-green-400/30 rounded-lg p-4">
                <h3 className="font-semibold text-green-300 mb-3 flex items-center gap-2">
                  <CheckCircle2 className="w-4 h-4" />
                  Información de la Orden Cerrada
                </h3>
                <div className="grid grid-cols-2 gap-3 text-sm">
                  <div>
                    <span className="text-gray-400">Orden N°:</span>
                    <span className="ml-2 font-bold text-white">#{datosEnviados.nroOrden}</span>
                  </div>
                  <div>
                    <span className="text-gray-400">Estación:</span>
                    <span className="ml-2 font-medium text-white">{datosEnviados.estacion}</span>
                  </div>
                </div>
                <div className="mt-3">
                  <span className="text-gray-400 text-sm">Observación:</span>
                  <p className="mt-1 text-white text-sm bg-black/20 rounded p-2">
                    {datosEnviados.observacion}
                  </p>
                </div>
              </div>

              {/* Motivos registrados */}
              <div className="bg-yellow-500/10 border border-yellow-400/30 rounded-lg p-4">
                <h3 className="font-semibold text-yellow-300 mb-3 flex items-center gap-2">
                  <AlertCircle className="w-4 h-4" />
                  Motivos Registrados ({datosEnviados.motivos.length})
                </h3>
                <div className="space-y-2">
                  {datosEnviados.motivos.map((m, i) => (
                    <div key={i} className="bg-black/20 rounded p-2 text-sm">
                      <div className="font-medium text-yellow-200">
                        {i + 1}. {m.motivo}
                      </div>
                      {m.comentario && (
                        <div className="text-gray-400 text-xs mt-1 ml-4">
                          💬 {m.comentario}
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              </div>

              {/* Acciones realizadas */}
              <div className="bg-blue-500/10 border border-blue-400/30 rounded-lg p-4">
                <h3 className="font-semibold text-blue-300 mb-3">
                  ✅ Acciones Ejecutadas
                </h3>
                <div className="space-y-2 text-sm">
                  <p className="flex items-center gap-2 text-gray-300">
                    <CheckCircle2 className="w-4 h-4 text-green-400" />
                    Orden de inspección cerrada
                  </p>
                  <p className="flex items-center gap-2 text-gray-300">
                    <CheckCircle2 className="w-4 h-4 text-green-400" />
                    Sismógrafo marcado como "Fuera de Servicio"
                  </p>
                  <p className="flex items-center gap-2 text-gray-300">
                    <CheckCircle2 className="w-4 h-4 text-green-400" />
                    Notificación enviada a la consola CCRS
                  </p>
                  <p className="flex items-center gap-2 text-gray-300">
                    <CheckCircle2 className="w-4 h-4 text-green-400" />
                    Emails enviados a responsables de reparación
                  </p>
                  <p className="flex items-center gap-2 text-gray-300">
                    <CheckCircle2 className="w-4 h-4 text-green-400" />
                    Evento registrado en el monitor del sistema
                  </p>
                </div>
              </div>
            </div>

            <div className="flex justify-end gap-3 mt-6">
              <button
                type="button"
                onClick={() => {
                  setMostrarExito(false);
                  setDatosEnviados(null);
                }}
                className="px-6 py-2.5 rounded-lg bg-linear-to-r from-green-600 to-blue-600 hover:from-green-500 hover:to-blue-500 text-white font-bold shadow-lg transition-all flex items-center gap-2"
              >
                <CheckCircle2 className="w-4 h-4" />
                Entendido
              </button>
            </div>
          </motion.div>
        </div>,
        document.body
      )}

      {/* Modal de confirmación */}
      {mostrarModal && createPortal(
        <div
          className="fixed inset-0 flex items-center justify-center p-4 bg-black/90 backdrop-blur-md"
          style={{ zIndex: 99999 }}
          onClick={(e) => {
            if (e.target === e.currentTarget) {
              setMostrarModal(false);
            }
          }}
        >
          <motion.div
            initial={{ scale: 0.9, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            className="bg-linear-to-br from-gray-900 to-gray-800 rounded-2xl p-8 shadow-2xl max-w-lg w-full border-2 border-blue-400/50"
          >
            <div className="flex items-center gap-3 mb-4">
              <div className="w-12 h-12 rounded-full bg-yellow-500/20 flex items-center justify-center">
                <AlertCircle className="w-6 h-6 text-yellow-400" />
              </div>
              <h2 className="text-2xl font-bold text-blue-300">
                Confirmar Cierre de Orden
              </h2>
            </div>

            <div className="bg-blue-500/10 border border-blue-400/30 rounded-lg p-4 mb-4 space-y-2">
              <p className="text-sm text-gray-200">
                Estás a punto de cerrar la orden{" "}
                <span className="font-bold text-green-400">#{orden.nroOrden}</span>
                {" "}de la estación{" "}
                <span className="font-bold text-green-400">{orden.estacion}</span>
              </p>
              <div className="text-xs text-gray-400 space-y-1 mt-3">
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
              <button
                type="button"
                onClick={() => setMostrarModal(false)}
                className="px-5 py-2.5 rounded-lg border-2 border-gray-600 text-gray-300 hover:bg-gray-700 hover:border-gray-500 transition-all font-medium"
              >
                ✕ Cancelar
              </button>
              <button
                type="button"
                onClick={confirmarCierre}
                className="px-5 py-2.5 rounded-lg bg-linear-to-r from-green-600 to-blue-600 hover:from-green-500 hover:to-blue-500 text-white font-bold shadow-lg transition-all flex items-center gap-2"
              >
                <CheckCircle2 className="w-4 h-4" />
                Sí, cerrar orden
              </button>
            </div>
          </motion.div>
        </div>,
        document.body
      )}
    </>
  );
}
