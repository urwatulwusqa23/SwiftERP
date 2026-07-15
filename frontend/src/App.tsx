import { HashRouter, Route, Routes } from "react-router-dom";
import { Nav } from "./components/Nav";
import { Dashboard } from "./pages/Dashboard";
import { Inventory } from "./pages/Inventory";
import { Sales } from "./pages/Sales";
import { Finance } from "./pages/Finance";
import { HR } from "./pages/HR";
import { Portal } from "./pages/Portal";

function App() {
  return (
    <HashRouter>
      <Nav />
      <Routes>
        <Route path="/" element={<Dashboard />} />
        <Route path="/inventory" element={<Inventory />} />
        <Route path="/sales" element={<Sales />} />
        <Route path="/finance" element={<Finance />} />
        <Route path="/hr" element={<HR />} />
        <Route path="/portal" element={<Portal />} />
      </Routes>
    </HashRouter>
  );
}

export default App;
