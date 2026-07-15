import { useEffect, useState } from "react";
import { api } from "../api/client";
import { PageShell, Panel } from "../components/PageShell";
import { StatCard } from "../components/StatCard";

const ACCENT = "var(--mod-finance)";

export function Finance() {
  const [balance, setBalance] = useState(0);

  useEffect(() => {
    const load = () => api.getFinanceBalance().then((r) => setBalance(r.balance));
    load();
    const interval = setInterval(load, 5000);
    return () => clearInterval(interval);
  }, []);

  return (
    <PageShell
      moduleLabel="Finance & Ledger"
      title="Running Balance"
      accent={ACCENT}
      description="Every confirmed sale posts a SaleRevenue entry; every posted payroll run posts a PayrollExpense entry. The balance below is the signed sum across all of them."
    >
      <div style={{ display: "grid", gridTemplateColumns: "1fr", gap: "1rem", maxWidth: 360 }}>
        <StatCard
          label="Ledger Balance"
          value={balance}
          accent={ACCENT}
          format={(n) => `$${n.toLocaleString(undefined, { maximumFractionDigits: 2 })}`}
          index={0}
        />
      </div>

      <Panel style={{ marginTop: "1rem" }}>
        <div className="label" style={{ marginBottom: "0.6rem" }}>
          Ledger entry types
        </div>
        <div style={{ display: "flex", flexDirection: "column", gap: "0.5rem", fontSize: "0.85rem", color: "var(--text-dim)" }}>
          <div>
            <span className="mono" style={{ color: "var(--success)" }}>
              + SaleRevenue
            </span>{" "}
            — posted by Sales.ConfirmSaleOrder
          </div>
          <div>
            <span className="mono" style={{ color: "var(--danger)" }}>
              − PayrollExpense
            </span>{" "}
            — posted by HR.PostPayrollRun
          </div>
          <div>
            <span className="mono" style={{ color: "var(--danger)" }}>
              − PurchaseExpense
            </span>{" "}
            — reserved for a future Inventory→Finance transaction
          </div>
        </div>
      </Panel>
    </PageShell>
  );
}
