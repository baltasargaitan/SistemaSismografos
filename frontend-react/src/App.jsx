import { BrowserRouter, Routes, Route, useLocation } from "react-router-dom";
import { useEffect } from "react";
import PantallaCierreInspeccion from "./pages/PantallaCierreInspeccion";
import PantallaMonitoreoOrdenes from "./pages/PantallaMonitoreoOrdenes";
import PantallaInicio from "./pages/PantallaInicio";

function CambiarTituloRuta() {
  const location = useLocation();

  useEffect(() => {
    const nombres = {
      "/": "Inicio",
      "/cerrar": "Cerrar Ordenes",
      "/monitoreo": "Monitoreo",
    };

    const nombre = nombres[location.pathname] || "SISGRAFOS";
    document.title = `${nombre} | SISGRAFOS`; // título visible en la pestaña
  }, [location.pathname]);

  return null; // no renderiza nada en pantalla
}

export default function App() {
  return (
    <BrowserRouter>
      {/* 🔹 Este componente se ejecuta siempre y actualiza el título */}
      <CambiarTituloRuta />

      <Routes>
        <Route path="/" element={<PantallaInicio />} />
        <Route path="/cerrar" element={<PantallaCierreInspeccion />} />
        <Route path="/monitoreo" element={<PantallaMonitoreoOrdenes />} />
      </Routes>
    </BrowserRouter>
  );
}
