import { motion } from "framer-motion";
import { NavLink, useNavigate } from "react-router-dom";
import { useAuth, type AccessLevel, type Module } from "../auth/AuthContext";

const MODULE_LINKS: { to: string; label: string; module: Module; minLevel: AccessLevel }[] = [
  { to: "/inventory", label: "Inventory", module: "Inventory", minLevel: "View" },
  { to: "/sales", label: "Sales", module: "Sales", minLevel: "View" },
  { to: "/finance", label: "Finance", module: "Finance", minLevel: "View" },
  { to: "/hr", label: "HR", module: "HR", minLevel: "View" },
];

export function Nav() {
  const { user, hasAccess, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/login", { replace: true });
  };

  return (
    <motion.nav
      initial={{ opacity: 0, y: -12 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.5 }}
      style={{
        position: "sticky",
        top: 0,
        zIndex: 10,
        display: "flex",
        alignItems: "center",
        justifyContent: "space-between",
        padding: "1.1rem 2rem",
        borderBottom: "1px solid var(--border)",
        background: "rgba(8, 11, 10, 0.72)",
        backdropFilter: "blur(10px)",
      }}
    >
      <div style={{ display: "flex", alignItems: "baseline", gap: "0.6rem" }}>
        <span style={{ fontFamily: "var(--font-display)", fontSize: "1.15rem", fontWeight: 700 }}>
          SwiftERP
        </span>
        <span className="label">Command Deck</span>
      </div>

      <div style={{ display: "flex", alignItems: "center", gap: "0.3rem" }}>
        <NavLinkItem to="/" label="Deck" end />
        {MODULE_LINKS.filter((link) => hasAccess(link.module, link.minLevel)).map((link) => (
          <NavLinkItem key={link.to} to={link.to} label={link.label} />
        ))}
        <NavLinkItem to="/portal" label="My Portal" />
        {user?.isSystemAdmin && <NavLinkItem to="/admin" label="Admin" />}

        <div style={{ display: "flex", alignItems: "center", gap: "0.7rem", marginLeft: "1rem" }}>
          <span className="mono" style={{ fontSize: "0.75rem", color: "var(--text-faint)" }}>
            {user?.email}
          </span>
          <button
            onClick={handleLogout}
            style={{
              background: "none",
              border: "1px solid var(--border)",
              borderRadius: "var(--radius)",
              color: "var(--text-dim)",
              padding: "0.35rem 0.7rem",
              fontSize: "0.75rem",
              cursor: "pointer",
            }}
          >
            Sign Out
          </button>
        </div>
      </div>
    </motion.nav>
  );
}

function NavLinkItem({ to, label, end }: { to: string; label: string; end?: boolean }) {
  return (
    <NavLink
      to={to}
      end={end}
      style={({ isActive }) => ({
        padding: "0.4rem 0.85rem",
        borderRadius: "var(--radius)",
        fontSize: "0.85rem",
        color: isActive ? "var(--bg)" : "var(--text-dim)",
        background: isActive ? "var(--accent)" : "transparent",
        transition: "all 0.2s",
      })}
    >
      {label}
    </NavLink>
  );
}
