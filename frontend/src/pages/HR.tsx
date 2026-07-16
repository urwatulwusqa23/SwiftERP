import { useEffect, useState } from "react";
import { motion } from "framer-motion";
import { api, type AttendanceRecordView, type EmployeeSummary, type OrgChartNode } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { ActionButton, Field, FieldRow, inputStyle, PageShell, Panel } from "../components/PageShell";

const ACCENT = "var(--mod-hr)";

interface RunEntry {
  id: string;
  year: number;
  month: number;
  status: "Draft" | "Posted";
}

function AttendancePanel() {
  const [employees, setEmployees] = useState<EmployeeSummary[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [records, setRecords] = useState<AttendanceRecordView[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    api.getAllEmployees().then((list) => {
      setEmployees(list);
      if (list.length > 0) setSelectedId(list[0].id);
    });
  }, []);

  useEffect(() => {
    if (!selectedId) return;
    setLoading(true);
    api
      .getAttendance(selectedId)
      .then(setRecords)
      .finally(() => setLoading(false));
  }, [selectedId]);

  const openRecord = records.find((r) => !r.clockOutUtc);

  return (
    <Panel>
      <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: "0.9rem", flexWrap: "wrap", gap: "0.6rem" }}>
        <div>
          <div className="label">Attendance</div>
          <p style={{ fontSize: "0.78rem", color: "var(--text-faint)", marginTop: "0.2rem" }}>
            Read-only — clock in/out from the Portal.
          </p>
        </div>
        <select style={{ ...inputStyle, minWidth: 220 }} value={selectedId} onChange={(e) => setSelectedId(e.target.value)}>
          {employees.length === 0 && <option value="">No employees yet</option>}
          {employees.map((e) => (
            <option key={e.id} value={e.id}>
              {e.fullName} {e.jobTitle ? `— ${e.jobTitle}` : ""}
            </option>
          ))}
        </select>
      </div>

      {selectedId && (
        <div style={{ display: "flex", alignItems: "center", gap: "0.5rem", marginBottom: "0.8rem" }}>
          <span
            className="mono"
            style={{
              fontSize: "0.68rem",
              color: openRecord ? "var(--success)" : "var(--text-faint)",
              border: `1px solid ${openRecord ? "var(--success)" : "var(--border)"}`,
              borderRadius: "var(--radius)",
              padding: "0.2rem 0.6rem",
            }}
          >
            {openRecord ? "Currently clocked in" : "Not clocked in"}
          </span>
        </div>
      )}

      <div style={{ display: "flex", flexDirection: "column", gap: "0.4rem" }}>
        {loading && <p style={{ color: "var(--text-faint)", fontSize: "0.85rem" }}>Loading…</p>}
        {!loading && records.length === 0 && (
          <p style={{ color: "var(--text-faint)", fontSize: "0.85rem" }}>No attendance records for this employee yet.</p>
        )}
        {!loading &&
          records.slice(0, 10).map((r) => (
            <div
              key={r.id}
              style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                padding: "0.6rem 0.8rem",
                background: "var(--bg-elevated)",
                borderRadius: "var(--radius)",
                borderLeft: `2px solid ${ACCENT}`,
              }}
            >
              <span className="mono" style={{ fontSize: "0.8rem" }}>
                {r.date}
              </span>
              <span className="mono" style={{ fontSize: "0.8rem", color: "var(--text-dim)" }}>
                {new Date(r.clockInUtc).toLocaleTimeString()} →{" "}
                {r.clockOutUtc ? new Date(r.clockOutUtc).toLocaleTimeString() : "…"}
              </span>
              <span className="mono" style={{ fontSize: "0.8rem" }}>
                {r.workedHours != null ? `${r.workedHours}h` : "—"}
                {r.overtimeHours ? ` (+${r.overtimeHours}h OT)` : ""}
              </span>
            </div>
          ))}
      </div>
    </Panel>
  );
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
  const { hasAccess } = useAuth();
  const canHire = hasAccess("HR", "Edit");
  const canPayroll = hasAccess("HR", "Full");
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
      description="Employees, attendance, org chart, and payroll."
    >
      <div style={{ display: "grid", gap: "1rem" }}>
        {canHire && (
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
        )}

        <AttendancePanel />

        <OrgChartPanel />

        {canPayroll && (
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
        )}

        {canPayroll && runs.length > 0 && (
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
