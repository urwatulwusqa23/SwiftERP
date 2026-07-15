import { useEffect, useState } from "react";
import { motion } from "framer-motion";
import { api, type Product } from "../api/client";
import { ActionButton, Field, FieldRow, inputStyle, PageShell, Panel } from "../components/PageShell";

const ACCENT = "var(--mod-inventory)";

export function Inventory() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(false);
  const [form, setForm] = useState({ sku: "", name: "", reorderThreshold: "5", initialQuantity: "20" });
  const [message, setMessage] = useState<string | null>(null);

  const refresh = () => {
    setLoading(true);
    api
      .getLowStockProducts()
      .then(setProducts)
      .finally(() => setLoading(false));
  };

  useEffect(refresh, []);

  const handleCreate = async () => {
    setMessage(null);
    try {
      await api.createProduct({
        sku: form.sku || `SKU-${Date.now()}`,
        name: form.name || "New Product",
        reorderThreshold: Number(form.reorderThreshold),
        supplierId: crypto.randomUUID(),
        initialQuantity: Number(form.initialQuantity),
      });
      setMessage("Product created.");
      setForm({ sku: "", name: "", reorderThreshold: "5", initialQuantity: "20" });
      refresh();
    } catch (e) {
      setMessage(String(e));
    }
  };

  const handleAdjust = async (productId: string, delta: number) => {
    const product = products.find((p) => p.id === productId);
    if (!product) return;
    const next = Math.max(0, product.quantityOnHand + delta);
    await api.adjustStock(productId, next);
    refresh();
  };

  return (
    <PageShell
      moduleLabel="Inventory & Procurement"
      title="Stock & Reorder Monitor"
      accent={ACCENT}
      description="Products at or below their reorder threshold. Crossing the threshold fires a stock.low event, visible live on the Deck."
    >
      <div style={{ display: "grid", gap: "1rem" }}>
        <Panel>
          <div className="label" style={{ marginBottom: "0.9rem" }}>
            Create Product
          </div>
          <FieldRow>
            <Field label="SKU">
              <input
                style={inputStyle}
                placeholder="auto-generated"
                value={form.sku}
                onChange={(e) => setForm({ ...form, sku: e.target.value })}
              />
            </Field>
            <Field label="Name">
              <input
                style={inputStyle}
                placeholder="Product name"
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
              />
            </Field>
            <Field label="Reorder Threshold">
              <input
                style={{ ...inputStyle, width: 90 }}
                type="number"
                value={form.reorderThreshold}
                onChange={(e) => setForm({ ...form, reorderThreshold: e.target.value })}
              />
            </Field>
            <Field label="Initial Qty">
              <input
                style={{ ...inputStyle, width: 90 }}
                type="number"
                value={form.initialQuantity}
                onChange={(e) => setForm({ ...form, initialQuantity: e.target.value })}
              />
            </Field>
            <ActionButton accent={ACCENT} onClick={handleCreate}>
              Create
            </ActionButton>
          </FieldRow>
          {message && (
            <p style={{ marginTop: "0.7rem", fontSize: "0.8rem", color: "var(--text-dim)" }}>{message}</p>
          )}
        </Panel>

        <Panel>
          <div style={{ display: "flex", justifyContent: "space-between", marginBottom: "0.9rem" }}>
            <span className="label">Low Stock ({products.length})</span>
            <button
              onClick={refresh}
              style={{ background: "none", border: "none", color: "var(--text-faint)", fontSize: "0.75rem" }}
            >
              {loading ? "refreshing…" : "refresh"}
            </button>
          </div>

          {products.length === 0 && !loading && (
            <p style={{ color: "var(--text-faint)", fontSize: "0.85rem" }}>Nothing below threshold right now.</p>
          )}

          <div style={{ display: "flex", flexDirection: "column", gap: "0.5rem" }}>
            {products.map((p, i) => (
              <motion.div
                key={p.id}
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
                  <div className="mono" style={{ fontSize: "0.85rem" }}>
                    {p.name}
                  </div>
                  <div className="mono" style={{ fontSize: "0.7rem", color: "var(--text-faint)" }}>
                    {p.sku}
                  </div>
                </div>
                <div style={{ display: "flex", alignItems: "center", gap: "0.8rem" }}>
                  <span className="mono" style={{ fontSize: "0.85rem", color: "var(--danger)" }}>
                    {p.quantityOnHand} / {p.reorderThreshold}
                  </span>
                  <button
                    onClick={() => handleAdjust(p.id, -1)}
                    style={ghostBtn}
                  >
                    −1
                  </button>
                  <button onClick={() => handleAdjust(p.id, 5)} style={ghostBtn}>
                    +5
                  </button>
                </div>
              </motion.div>
            ))}
          </div>
        </Panel>
      </div>
    </PageShell>
  );
}

const ghostBtn: React.CSSProperties = {
  background: "var(--surface-hover)",
  border: "1px solid var(--border)",
  color: "var(--text-dim)",
  borderRadius: "var(--radius)",
  padding: "0.3rem 0.55rem",
  fontFamily: "var(--font-mono)",
  fontSize: "0.72rem",
};
