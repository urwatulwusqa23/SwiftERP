import { useCallback, useEffect, useRef, useState } from "react";
import { motion } from "framer-motion";
import { api, type DashboardSummary } from "../api/client";
import { NetworkBackground } from "../components/NetworkBackground";
import { ModuleLabels } from "../components/ModuleLabels";
import { StatCard } from "../components/StatCard";
import { ActivityFeed } from "../components/ActivityFeed";
import { useNotificationStream, type LiveNotification } from "../hooks/useNotificationStream";
import type { ModuleKey } from "../components/networkLayout";

export function Dashboard() {
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [pulseSignal, setPulseSignal] = useState<{ key: ModuleKey; nonce: number } | null>(null);
  const nonceRef = useRef(0);

  const refresh = useCallback(() => {
    api.getDashboard().then(setSummary).catch(() => {});
  }, []);

  useEffect(() => {
    refresh();
  }, [refresh]);

  const handleEvent = useCallback(
    (n: LiveNotification) => {
      nonceRef.current += 1;
      setPulseSignal({ key: n.module, nonce: nonceRef.current });
      refresh();
    },
    [refresh]
  );

  const { notifications, connected } = useNotificationStream(handleEvent);

  return (
    <div style={{ position: "relative", minHeight: "calc(100vh - 68px)" }}>
      <NetworkBackground pulseSignal={pulseSignal} />
      <ModuleLabels />
      <div className="noise-vignette" />
      <div className="scanlines" />

      <div
        style={{
          position: "relative",
          zIndex: 2,
          maxWidth: 1180,
          margin: "0 auto",
          padding: "4rem 2rem 3rem",
        }}
      >
        <motion.div
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6 }}
          style={{ marginBottom: "3rem", maxWidth: 640 }}
        >
          <div className="label" style={{ marginBottom: "0.8rem" }}>
            Live System Status
          </div>
          <h1 style={{ fontSize: "2.6rem", lineHeight: 1.1 }}>
            Four modules.
            <br />
            One transaction boundary.
          </h1>
        </motion.div>

        <div
          style={{
            display: "grid",
            gridTemplateColumns: "repeat(4, 1fr)",
            gap: "1rem",
            marginBottom: "1.5rem",
          }}
        >
          <StatCard
            label="Low Stock Products"
            value={summary?.lowStockProductCount ?? 0}
            accent="var(--mod-inventory)"
            index={0}
          />
          <StatCard
            label="Draft Sale Orders"
            value={summary?.draftSaleOrderCount ?? 0}
            accent="var(--mod-sales)"
            index={1}
          />
          <StatCard
            label="Finance Balance"
            value={summary?.financeRunningBalance ?? 0}
            accent="var(--mod-finance)"
            format={(n) => `$${n.toLocaleString(undefined, { maximumFractionDigits: 0 })}`}
            index={2}
          />
          <StatCard
            label="Active Employees"
            value={summary?.activeEmployeeCount ?? 0}
            accent="var(--mod-hr)"
            index={3}
          />
        </div>

        <div style={{ display: "grid", gridTemplateColumns: "2fr 1fr", gap: "1rem" }}>
          <motion.div
            initial={{ opacity: 0, y: 16 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.4, duration: 0.5 }}
            style={{
              background: "var(--surface)",
              border: "1px solid var(--border)",
              borderRadius: "var(--radius)",
              padding: "1.4rem 1.5rem",
            }}
          >
            <div className="label" style={{ marginBottom: "0.8rem" }}>
              Cross-Module Transactions
            </div>
            <p style={{ color: "var(--text-dim)", fontSize: "0.88rem", lineHeight: 1.6 }}>
              Confirming a sale decrements stock and posts a ledger entry in one atomic transaction.
              Same pattern powers payroll.
            </p>
          </motion.div>

          <ActivityFeed notifications={notifications} connected={connected} />
        </div>
      </div>
    </div>
  );
}
