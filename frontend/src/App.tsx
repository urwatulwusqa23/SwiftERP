import { useEffect } from "react";
import { HashRouter, Navigate, Route, Routes, useNavigate } from "react-router-dom";
import { Nav } from "./components/Nav";
import { Dashboard } from "./pages/Dashboard";
import { Inventory } from "./pages/Inventory";
import { Sales } from "./pages/Sales";
import { Finance } from "./pages/Finance";
import { HR } from "./pages/HR";
import { Portal } from "./pages/Portal";
import { Admin } from "./pages/Admin";
import { Login } from "./pages/Login";
import { AuthProvider, useAuth, type AccessLevel, type Module } from "./auth/AuthContext";

function RequireAuth({ children }: { children: React.ReactNode }) {
  const { user } = useAuth();
  if (!user) return <Navigate to="/login" replace />;
  return <>{children}</>;
}

function RequireModule({
  module,
  minLevel,
  children,
}: {
  module: Module;
  minLevel: AccessLevel;
  children: React.ReactNode;
}) {
  const { hasAccess } = useAuth();
  if (!hasAccess(module, minLevel)) return <Navigate to="/" replace />;
  return <>{children}</>;
}

function RequireAdmin({ children }: { children: React.ReactNode }) {
  const { user } = useAuth();
  if (!user?.isSystemAdmin) return <Navigate to="/" replace />;
  return <>{children}</>;
}

// A session that expires mid-session (401 from any API call) dispatches this event from
// client.ts — listening here bounces the user back to /login instead of leaving them staring
// at pages full of failed requests.
function SessionExpiryWatcher() {
  const navigate = useNavigate();
  useEffect(() => {
    const handler = () => navigate("/login", { replace: true });
    window.addEventListener("swifterp:session-expired", handler);
    return () => window.removeEventListener("swifterp:session-expired", handler);
  }, [navigate]);
  return null;
}

function AppShell() {
  const { user } = useAuth();

  return (
    <>
      <SessionExpiryWatcher />
      {user && <Nav />}
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/" element={<RequireAuth><Dashboard /></RequireAuth>} />
        <Route
          path="/inventory"
          element={
            <RequireAuth>
              <RequireModule module="Inventory" minLevel="View"><Inventory /></RequireModule>
            </RequireAuth>
          }
        />
        <Route
          path="/sales"
          element={
            <RequireAuth>
              <RequireModule module="Sales" minLevel="View"><Sales /></RequireModule>
            </RequireAuth>
          }
        />
        <Route
          path="/finance"
          element={
            <RequireAuth>
              <RequireModule module="Finance" minLevel="View"><Finance /></RequireModule>
            </RequireAuth>
          }
        />
        <Route
          path="/hr"
          element={
            <RequireAuth>
              <RequireModule module="HR" minLevel="View"><HR /></RequireModule>
            </RequireAuth>
          }
        />
        <Route path="/portal" element={<RequireAuth><Portal /></RequireAuth>} />
        <Route path="/admin" element={<RequireAuth><RequireAdmin><Admin /></RequireAdmin></RequireAuth>} />
      </Routes>
    </>
  );
}

function App() {
  return (
    <HashRouter>
      <AuthProvider>
        <AppShell />
      </AuthProvider>
    </HashRouter>
  );
}

export default App;
