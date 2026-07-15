import { motion } from "framer-motion";
import { NavLink } from "react-router-dom";

const LINKS = [
  { to: "/", label: "Deck" },
  { to: "/inventory", label: "Inventory" },
  { to: "/sales", label: "Sales" },
  { to: "/finance", label: "Finance" },
  { to: "/hr", label: "HR" },
  { to: "/portal", label: "Portal" },
];

export function Nav() {
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

      <div style={{ display: "flex", gap: "0.3rem" }}>
        {LINKS.map((link) => (
          <NavLink
            key={link.to}
            to={link.to}
            end={link.to === "/"}
            style={({ isActive }) => ({
              padding: "0.4rem 0.85rem",
              borderRadius: "var(--radius)",
              fontSize: "0.85rem",
              color: isActive ? "var(--bg)" : "var(--text-dim)",
              background: isActive ? "var(--accent)" : "transparent",
              transition: "all 0.2s",
            })}
          >
            {link.label}
          </NavLink>
        ))}
      </div>
    </motion.nav>
  );
}
