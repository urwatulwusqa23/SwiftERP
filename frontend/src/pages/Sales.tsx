import { useState } from "react";
import { motion } from "framer-motion";
import { api, type SaleOrder } from "../api/client";
import { ActionButton, Field, FieldRow, inputStyle, PageShell, Panel } from "../components/PageShell";

const ACCENT = "var(--mod-sales)";

export function Sales() {
  const [form, setForm] = useState({ productId: "", quantity: "1", unitPrice: "10" });
  const [orders, setOrders] = useState<SaleOrder[]>([]);
  const [message, setMessage] = useState<string | null>(null);

  const handleCreate = async () => {
    setMessage(null);
    if (!form.productId) {
      setMessage("Paste a product ID from the Inventory page first.");
      return;
    }
    try {
      const created = await api.createSaleOrder({
        customerId: crypto.randomUUID(),
        lines: [{ productId: form.productId, quantity: Number(form.quantity), unitPrice: Number(form.unitPrice) }],
      });
      const order = await api.getSaleOrder(created.id);
      setOrders((prev) => [order, ...prev]);
      setMessage("Draft sale order created.");
    } catch (e) {
      setMessage(String(e));
    }
  };

  const handleConfirm = async (id: string) => {
    setMessage(null);
    try {
      await api.confirmSaleOrder(id);
      const refreshed = await api.getSaleOrder(id);
      setOrders((prev) => prev.map((o) => (o.id === id ? refreshed : o)));
    } catch (e) {
      setMessage(`Confirm failed — ${e}`);
    }
  };

  return (
    <PageShell
      moduleLabel="Sales & Invoicing"
      title="Sale Orders"
      accent={ACCENT}
      description="Confirming a sale order decrements Inventory stock and posts a Finance ledger entry — atomically. Insufficient stock rolls back both."
    >
      <div style={{ display: "grid", gap: "1rem" }}>
        <Panel>
          <div className="label" style={{ marginBottom: "0.9rem" }}>
            New Draft Order
          </div>
          <FieldRow>
            <Field label="Product ID">
              <input
                style={{ ...inputStyle, width: 280 }}
                placeholder="paste from Inventory"
                value={form.productId}
                onChange={(e) => setForm({ ...form, productId: e.target.value })}
              />
            </Field>
            <Field label="Quantity">
              <input
                style={{ ...inputStyle, width: 80 }}
                type="number"
                value={form.quantity}
                onChange={(e) => setForm({ ...form, quantity: e.target.value })}
              />
            </Field>
            <Field label="Unit Price">
              <input
                style={{ ...inputStyle, width: 90 }}
                type="number"
                value={form.unitPrice}
                onChange={(e) => setForm({ ...form, unitPrice: e.target.value })}
              />
            </Field>
            <ActionButton accent={ACCENT} onClick={handleCreate}>
              Create Draft
            </ActionButton>
          </FieldRow>
          {message && (
            <p style={{ marginTop: "0.7rem", fontSize: "0.8rem", color: "var(--text-dim)" }}>{message}</p>
          )}
        </Panel>

        <Panel>
          <div className="label" style={{ marginBottom: "0.9rem" }}>
            Orders This Session ({orders.length})
          </div>
          {orders.length === 0 && (
            <p style={{ color: "var(--text-faint)", fontSize: "0.85rem" }}>
              No orders created yet. Copy a Product ID from the Inventory tab to get started.
            </p>
          )}
          <div style={{ display: "flex", flexDirection: "column", gap: "0.5rem" }}>
            {orders.map((o, i) => (
              <motion.div
                key={o.id}
                initial={{ opacity: 0, x: -10 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: i * 0.05 }}
                style={{
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "space-between",
                  padding: "0.7rem 0.9rem",
                  background: "var(--bg-elevated)",
                  borderRadius: "var(--radius)",
                  borderLeft: `2px solid ${ACCENT}`,
                }}
              >
                <div>
                  <div className="mono" style={{ fontSize: "0.78rem", color: "var(--text-faint)" }}>
                    {o.id.slice(0, 8)}…
                  </div>
                  <div className="mono" style={{ fontSize: "0.85rem" }}>
                    ${o.total.toLocaleString()} · {o.lines.length} line(s)
                  </div>
                </div>
                <div style={{ display: "flex", alignItems: "center", gap: "0.7rem" }}>
                  <StatusChip status={o.status} />
                  {o.status === "Draft" && (
                    <ActionButton accent={ACCENT} onClick={() => handleConfirm(o.id)}>
                      Confirm
                    </ActionButton>
                  )}
                </div>
              </motion.div>
            ))}
          </div>
        </Panel>
      </div>
    </PageShell>
  );
}

function StatusChip({ status }: { status: string }) {
  const color = status === "Confirmed" ? "var(--success)" : status === "Cancelled" ? "var(--danger)" : "var(--text-faint)";
  return (
    <span
      className="mono"
      style={{
        fontSize: "0.68rem",
        color,
        border: `1px solid ${color}`,
        borderRadius: "var(--radius)",
        padding: "0.15rem 0.5rem",
      }}
    >
      {status}
    </span>
  );
}
