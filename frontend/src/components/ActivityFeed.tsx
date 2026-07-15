import { AnimatePresence, motion } from "framer-motion";
import type { LiveNotification } from "../hooks/useNotificationStream";
import { MODULE_NODES } from "./networkLayout";

interface Props {
  notifications: LiveNotification[];
  connected: boolean;
}

function describe(n: LiveNotification): string {
  if (n.type === "stock.low") {
    return `${n.payload.sku} dropped to ${n.payload.quantityOnHand} units (threshold ${n.payload.reorderThreshold})`;
  }
  if (n.type === "payroll.processed") {
    return `Payroll ${n.payload.year}-${String(n.payload.month).padStart(2, "0")} posted — $${Number(
      n.payload.total
    ).toLocaleString()}`;
  }
  return JSON.stringify(n.payload);
}

export function ActivityFeed({ notifications, connected }: Props) {
  return (
    <div
      style={{
        background: "var(--surface)",
        border: "1px solid var(--border)",
        borderRadius: "var(--radius)",
        padding: "1.25rem 1.4rem",
        display: "flex",
        flexDirection: "column",
        minHeight: 260,
      }}
    >
      <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: "0.9rem" }}>
        <span className="label">Live Activity</span>
        <span style={{ display: "flex", alignItems: "center", gap: 6 }}>
          <motion.span
            animate={{ opacity: connected ? [1, 0.3, 1] : 1 }}
            transition={{ repeat: connected ? Infinity : 0, duration: 1.8 }}
            style={{
              width: 6,
              height: 6,
              borderRadius: "50%",
              background: connected ? "var(--success)" : "var(--text-faint)",
              display: "inline-block",
            }}
          />
          <span className="mono" style={{ fontSize: "0.68rem", color: "var(--text-faint)" }}>
            {connected ? "LIVE" : "CONNECTING"}
          </span>
        </span>
      </div>

      <div style={{ display: "flex", flexDirection: "column", gap: "0.5rem", overflowY: "auto", flex: 1 }}>
        <AnimatePresence initial={false}>
          {notifications.length === 0 && (
            <motion.p
              key="empty"
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              style={{ color: "var(--text-faint)", fontSize: "0.85rem", padding: "1rem 0" }}
            >
              Waiting for events — adjust stock below a reorder threshold or post a payroll run to see one land here.
            </motion.p>
          )}
          {notifications.map((n) => {
            const node = MODULE_NODES.find((m) => m.key === n.module);
            return (
              <motion.div
                key={n.id}
                layout
                initial={{ opacity: 0, x: -12, height: 0 }}
                animate={{ opacity: 1, x: 0, height: "auto" }}
                exit={{ opacity: 0, height: 0 }}
                transition={{ duration: 0.35, ease: "easeOut" }}
                style={{
                  display: "flex",
                  gap: "0.65rem",
                  alignItems: "flex-start",
                  padding: "0.6rem 0.7rem",
                  background: "var(--bg-elevated)",
                  borderRadius: "var(--radius)",
                  borderLeft: `2px solid ${node?.color ?? "var(--accent)"}`,
                }}
              >
                <div style={{ flex: 1 }}>
                  <div className="mono" style={{ fontSize: "0.7rem", color: node?.color, marginBottom: 2 }}>
                    {n.type}
                  </div>
                  <div style={{ fontSize: "0.82rem", color: "var(--text-dim)", lineHeight: 1.4 }}>
                    {describe(n)}
                  </div>
                </div>
                <span className="mono" style={{ fontSize: "0.68rem", color: "var(--text-faint)", whiteSpace: "nowrap" }}>
                  {new Date(n.occurredAtUtc).toLocaleTimeString()}
                </span>
              </motion.div>
            );
          })}
        </AnimatePresence>
      </div>
    </div>
  );
}
