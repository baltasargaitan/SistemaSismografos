import { BrowserRouter, Routes, Route } from "react-router-dom";
import PantallaCierreInspeccion from "./pages/PantallaCierreInspeccion";

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<PantallaCierreInspeccion />} />
      </Routes>
    </BrowserRouter>
  );
}
