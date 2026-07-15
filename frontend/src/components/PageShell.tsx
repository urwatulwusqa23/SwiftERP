import type { ReactNode } from "react";
import { motion } from "framer-motion";

interface Props {
  moduleLabel: string;
  title: string;
  accent: string;
  description: string;
  children: ReactNode;
}

export function PageShell({ moduleLabel, title, accent, description, children }: Props) {
  return (
    <div style={{ maxWidth: 1000, margin: "0 auto", padding: "3.2rem 2rem 4rem" }}>
      <motion.div
        initial={{ opacity: 0, y: 12 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5 }}
        style={{ marginBottom: "2.4rem" }}
      >
        <div className="label" style={{ color: accent, marginBottom: "0.6rem" }}>
          {moduleLabel}
        </div>
        <h1 style={{ fontSize: "2.1rem", marginBottom: "0.7rem" }}>{title}</h1>
        <p style={{ color: "var(--text-dim)", fontSize: "0.9rem", maxWidth: 560, lineHeight: 1.6 }}>
          {description}
        </p>
      </motion.div>
      {children}
    </div>
  );
}

export function Panel({ children, style }: { children: ReactNode; style?: React.CSSProperties }) {
  return (
    <div
      style={{
        background: "var(--surface)",
        border: "1px solid var(--border)",
        borderRadius: "var(--radius)",
        padding: "1.4rem 1.5rem",
        ...style,
      }}
    >
      {children}
    </div>
  );
}

export function FieldRow({ children }: { children: ReactNode }) {
  return <div style={{ display: "flex", gap: "0.7rem", flexWrap: "wrap", alignItems: "flex-end" }}>{children}</div>;
}

export function Field({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div style={{ display: "flex", flexDirection: "column", gap: "0.35rem" }}>
      <span className="label">{label}</span>
      {children}
    </div>
  );
}

export const inputStyle: React.CSSProperties = {
  background: "var(--bg-elevated)",
  border: "1px solid var(--border)",
  borderRadius: "var(--radius)",
  color: "var(--text)",
  padding: "0.5rem 0.7rem",
  fontFamily: "var(--font-mono)",
  fontSize: "0.85rem",
  outline: "none",
};

export function ActionButton({
  children,
  onClick,
  accent,
  disabled,
  type = "button",
}: {
  children: ReactNode;
  onClick?: () => void;
  accent: string;
  disabled?: boolean;
  type?: "button" | "submit";
}) {
  return (
    <motion.button
      type={type}
      onClick={onClick}
      disabled={disabled}
      whileHover={disabled ? undefined : { y: -1, filter: "brightness(1.1)" }}
      whileTap={disabled ? undefined : { scale: 0.97 }}
      style={{
        background: disabled ? "var(--border)" : accent,
        color: "#08120f",
        border: "none",
        borderRadius: "var(--radius)",
        padding: "0.55rem 1.1rem",
        fontFamily: "var(--font-mono)",
        fontSize: "0.78rem",
        fontWeight: 600,
        letterSpacing: "0.03em",
        opacity: disabled ? 0.5 : 1,
        cursor: disabled ? "not-allowed" : "pointer",
      }}
    >
      {children}
    </motion.button>
  );
}
