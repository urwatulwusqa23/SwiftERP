import { useEffect, useState } from "react";
import { motion } from "framer-motion";
import { api, type OrgChartNode } from "../api/client";
import { ActionButton, Field, FieldRow, inputStyle, PageShell, Panel } from "../components/PageShell";

const ACCENT = "var(--mod-hr)";

interface RunEntry {
  id: string;
  year: number;
  month: number;
  status: "Draft" | "Posted";
}

function OrgChartTree({ node, depth = 0 }: { node: OrgChartNode; depth?: number }) {
  return (
    <div style={{ marginLeft: depth * 20 }}>
      <div
        style={{
          display: "flex",
          alignItems: "center",
          gap: "0.5rem",
          padding: "0.4rem 0.6rem",
          borderLeft: `2px solid ${ACCENT}`,
          marginBottom: "0.3rem",
        }}
      >
        <span style={{ fontSize: "0.85rem" }}>{node.fullName}</span>
        {node.jobTitle && (
          <span className="mono" style={{ fontSize: "0.7rem", color: "var(--text-faint)" }}>
            {node.jobTitle}
          </span>
        )}
      </div>
      {node.reports.map((child) => (
        <OrgChartTree key={child.id} node={child} depth={depth + 1} />
      ))}
    </div>
  );
}

function OrgChartPanel() {
  const [nodes, setNodes] = useState<OrgChartNode[]>([]);

  useEffect(() => {
    api.getOrgChart().then(setNodes);
  }, []);

  return (
    <Panel>
      <div className="label" style={{ marginBottom: "0.9rem" }}>
        Org Chart
      </div>
      {nodes.length === 0 && (
        <p style={{ color: "var(--text-faint)", fontSize: "0.85rem" }}>No employees yet.</p>
      )}
      {nodes.map((n) => (
        <OrgChartTree key={n.id} node={n} />
      ))}
    </Panel>
  );
}

export function HR() {
  const [employeeForm, setEmployeeForm] = useState({ fullName: "", email: "", monthlySalary: "4000" });
  const [runForm, setRunForm] = useState({ year: "2026", month: "7" });
  const [runs, setRuns] = useState<RunEntry[]>([]);
  const [message, setMessage] = useState<string | null>(null);

  const handleHire = async () => {
    setMessage(null);
    try {
      await api.hireEmployee({
        fullName: employeeForm.fullName || `Employee ${Date.now()}`,
        email: employeeForm.email || `employee-${Date.now()}@example.com`,
        monthlySalary: Number(employeeForm.monthlySalary),
        hireDate: new Date().toISOString().slice(0, 10),
      });
      setMessage("Employee hired.");
      setEmployeeForm({ fullName: "", email: "", monthlySalary: "4000" });
    } catch (e) {
      setMessage(String(e));
    }
  };

  const handleCreateRun = async () => {
    setMessage(null);
    try {
      const created = await api.createPayrollRun({ year: Number(runForm.year), month: Number(runForm.month) });
      setRuns((prev) => [{ id: created.id, year: Number(runForm.year), month: Number(runForm.month), status: "Draft" }, ...prev]);
    } catch (e) {
      setMessage(String(e));
    }
  };

  const handlePost = async (id: string) => {
    setMessage(null);
    try {
      await api.postPayrollRun(id);
      setRuns((prev) => prev.map((r) => (r.id === id ? { ...r, status: "Posted" } : r)));
    } catch (e) {
      setMessage(`Post failed — ${e}`);
    }
  };

  return (
    <PageShell
      moduleLabel="HR & Payroll"
      title="Employees & Payroll Runs"
      accent={ACCENT}
      description="Posting a payroll run marks it Posted and records a Finance ledger expense atomically, using the same DbContext-sharing pattern as Sales."
    >
      <div style={{ display: "grid", gap: "1rem" }}>
        <Panel>
          <div className="label" style={{ marginBottom: "0.9rem" }}>
            Hire Employee
          </div>
          <FieldRow>
            <Field label="Full Name">
              <input
                style={inputStyle}
                value={employeeForm.fullName}
                onChange={(e) => setEmployeeForm({ ...employeeForm, fullName: e.target.value })}
              />
            </Field>
            <Field label="Email">
              <input
                style={inputStyle}
                value={employeeForm.email}
                onChange={(e) => setEmployeeForm({ ...employeeForm, email: e.target.value })}
              />
            </Field>
            <Field label="Monthly Salary">
              <input
                style={{ ...inputStyle, width: 110 }}
                type="number"
                value={employeeForm.monthlySalary}
                onChange={(e) => setEmployeeForm({ ...employeeForm, monthlySalary: e.target.value })}
              />
            </Field>
            <ActionButton accent={ACCENT} onClick={handleHire}>
              Hire
            </ActionButton>
          </FieldRow>
        </Panel>

        <OrgChartPanel />

        <Panel>
          <div className="label" style={{ marginBottom: "0.9rem" }}>
            Create Payroll Run
          </div>
          <FieldRow>
            <Field label="Year">
              <input
                style={{ ...inputStyle, width: 90 }}
                type="number"
                value={runForm.year}
                onChange={(e) => setRunForm({ ...runForm, year: e.target.value })}
              />
            </Field>
            <Field label="Month">
              <input
                style={{ ...inputStyle, width: 70 }}
                type="number"
                min={1}
                max={12}
                value={runForm.month}
                onChange={(e) => setRunForm({ ...runForm, month: e.target.value })}
              />
            </Field>
            <ActionButton accent={ACCENT} onClick={handleCreateRun}>
              Create Draft Run
            </ActionButton>
          </FieldRow>
          {message && (
            <p style={{ marginTop: "0.7rem", fontSize: "0.8rem", color: "var(--text-dim)" }}>{message}</p>
          )}
        </Panel>

        {runs.length > 0 && (
          <Panel>
            <div className="label" style={{ marginBottom: "0.9rem" }}>
              Runs This Session
            </div>
            <div style={{ display: "flex", flexDirection: "column", gap: "0.5rem" }}>
              {runs.map((r, i) => (
                <motion.div
                  key={r.id}
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
                  <span className="mono" style={{ fontSize: "0.85rem" }}>
                    {r.year}-{String(r.month).padStart(2, "0")}
                  </span>
                  <div style={{ display: "flex", alignItems: "center", gap: "0.7rem" }}>
                    <span
                      className="mono"
                      style={{
                        fontSize: "0.68rem",
                        color: r.status === "Posted" ? "var(--success)" : "var(--text-faint)",
                      }}
                    >
                      {r.status}
                    </span>
                    {r.status === "Draft" && (
                      <ActionButton accent={ACCENT} onClick={() => handlePost(r.id)}>
                        Post
                      </ActionButton>
                    )}
                  </div>
                </motion.div>
              ))}
            </div>
          </Panel>
        )}
      </div>
    </PageShell>
  );
}
