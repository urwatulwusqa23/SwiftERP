import { useEffect, useRef, useState } from "react";
import { motion } from "framer-motion";
import {
  api,
  type AttendanceRecordView,
  type EmployeeDocument,
  type EmployeeProfile,
  type EmployeeSummary,
  type LeaveBalance,
  type LeaveRequestView,
} from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { ActionButton, Field, FieldRow, inputStyle, PageShell, Panel } from "../components/PageShell";

const ACCENT = "var(--mod-hr)";
const CURRENT_YEAR = new Date().getFullYear();

export function Portal() {
  const { user, hasAccess } = useAuth();
  // Only HR-permissioned users can list every employee (used for the "manager" picker below) —
  // a plain employee viewing their own portal simply won't get that dropdown populated.
  const [employees, setEmployees] = useState<EmployeeSummary[]>([]);

  useEffect(() => {
    if (hasAccess("HR", "View")) {
      api.getAllEmployees().then(setEmployees).catch(() => {});
    }
  }, [hasAccess]);

  if (!user) return null;

  return (
    <PageShell
      moduleLabel="Employee Self-Service"
      title="My Portal"
      accent={ACCENT}
      description="Your own profile, attendance, leave, and documents — visible to you regardless of what HR module access your role grants."
    >
      <PortalContent employeeId={user.employeeId} employees={employees} />
    </PageShell>
  );
}

function PortalContent({ employeeId, employees }: { employeeId: string; employees: EmployeeSummary[] }) {
  return (
    <div style={{ display: "grid", gap: "1rem" }}>
      <ProfileSection employeeId={employeeId} employees={employees} />
      <AttendanceSection employeeId={employeeId} />
      <LeaveSection employeeId={employeeId} />
      <DocumentsSection employeeId={employeeId} />
    </div>
  );
}

function ProfileSection({ employeeId, employees }: { employeeId: string; employees: EmployeeSummary[] }) {
  const [profile, setProfile] = useState<EmployeeProfile | null>(null);
  const [form, setForm] = useState({ phoneNumber: "", address: "", dateOfBirth: "", jobTitle: "", department: "", managerId: "" });
  const [message, setMessage] = useState<string | null>(null);

  const load = () => api.getEmployeeProfile(employeeId).then((p) => {
    setProfile(p);
    setForm({
      phoneNumber: p.phoneNumber ?? "",
      address: p.address ?? "",
      dateOfBirth: p.dateOfBirth ?? "",
      jobTitle: p.jobTitle ?? "",
      department: p.department ?? "",
      managerId: p.managerId ?? "",
    });
  });

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [employeeId]);

  const handleSave = async () => {
    setMessage(null);
    try {
      await api.updateEmployee(employeeId, {
        phoneNumber: form.phoneNumber || null,
        address: form.address || null,
        dateOfBirth: form.dateOfBirth || null,
        jobTitle: form.jobTitle || null,
        department: form.department || null,
        managerId: form.managerId || null,
      });
      setMessage("Profile updated.");
      load();
    } catch (e) {
      setMessage(String(e));
    }
  };

  if (!profile) return <Panel>Loading profile…</Panel>;

  return (
    <Panel>
      <div className="label" style={{ marginBottom: "0.9rem" }}>
        Profile — {profile.fullName}
      </div>
      <p style={{ fontSize: "0.85rem", color: "var(--text-dim)", marginBottom: "0.9rem" }}>
        {profile.email} · Hired {profile.hireDate} · ${profile.monthlySalary.toLocaleString()}/mo
        {profile.managerName && <> · Reports to {profile.managerName}</>}
      </p>
      <FieldRow>
        <Field label="Phone">
          <input style={inputStyle} value={form.phoneNumber} onChange={(e) => setForm({ ...form, phoneNumber: e.target.value })} />
        </Field>
        <Field label="Address">
          <input style={{ ...inputStyle, width: 220 }} value={form.address} onChange={(e) => setForm({ ...form, address: e.target.value })} />
        </Field>
        <Field label="Date of Birth">
          <input style={inputStyle} type="date" value={form.dateOfBirth} onChange={(e) => setForm({ ...form, dateOfBirth: e.target.value })} />
        </Field>
      </FieldRow>
      <div style={{ height: "0.6rem" }} />
      <FieldRow>
        <Field label="Job Title">
          <input style={inputStyle} value={form.jobTitle} onChange={(e) => setForm({ ...form, jobTitle: e.target.value })} />
        </Field>
        <Field label="Department">
          <input style={inputStyle} value={form.department} onChange={(e) => setForm({ ...form, department: e.target.value })} />
        </Field>
        <Field label="Manager">
          <select style={inputStyle} value={form.managerId} onChange={(e) => setForm({ ...form, managerId: e.target.value })}>
            <option value="">None</option>
            {employees.filter((e) => e.id !== employeeId).map((e) => (
              <option key={e.id} value={e.id}>{e.fullName}</option>
            ))}
          </select>
        </Field>
        <ActionButton accent={ACCENT} onClick={handleSave}>Save</ActionButton>
      </FieldRow>
      {message && <p style={{ marginTop: "0.7rem", fontSize: "0.8rem", color: "var(--text-dim)" }}>{message}</p>}
    </Panel>
  );
}

function AttendanceSection({ employeeId }: { employeeId: string }) {
  const [records, setRecords] = useState<AttendanceRecordView[]>([]);
  const [message, setMessage] = useState<string | null>(null);

  const load = () => api.getAttendance(employeeId).then(setRecords);

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [employeeId]);

  const openRecord = records.find((r) => !r.clockOutUtc);

  const handleClockIn = async () => {
    setMessage(null);
    try {
      await api.clockIn(employeeId);
      load();
    } catch (e) {
      setMessage(String(e));
    }
  };

  const handleClockOut = async () => {
    setMessage(null);
    try {
      await api.clockOut(employeeId);
      load();
    } catch (e) {
      setMessage(String(e));
    }
  };

  return (
    <Panel>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "0.9rem" }}>
        <span className="label">Attendance</span>
        <div style={{ display: "flex", gap: "0.5rem" }}>
          <ActionButton accent={ACCENT} onClick={handleClockIn} disabled={!!openRecord}>Clock In</ActionButton>
          <ActionButton accent={ACCENT} onClick={handleClockOut} disabled={!openRecord}>Clock Out</ActionButton>
        </div>
      </div>
      {message && <p style={{ fontSize: "0.8rem", color: "var(--danger)", marginBottom: "0.6rem" }}>{message}</p>}
      <div style={{ display: "flex", flexDirection: "column", gap: "0.4rem" }}>
        {records.length === 0 && <p style={{ color: "var(--text-faint)", fontSize: "0.85rem" }}>No attendance records yet.</p>}
        {records.slice(0, 6).map((r) => (
          <div key={r.id} style={rowStyle}>
            <span className="mono" style={{ fontSize: "0.8rem" }}>{r.date}</span>
            <span className="mono" style={{ fontSize: "0.8rem", color: "var(--text-dim)" }}>
              {new Date(r.clockInUtc).toLocaleTimeString()} → {r.clockOutUtc ? new Date(r.clockOutUtc).toLocaleTimeString() : "…"}
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

function LeaveSection({ employeeId }: { employeeId: string }) {
  const [balances, setBalances] = useState<LeaveBalance[]>([]);
  const [requests, setRequests] = useState<LeaveRequestView[]>([]);
  const [form, setForm] = useState({ leaveType: "Annual", startDate: "", endDate: "", reason: "" });
  const [message, setMessage] = useState<string | null>(null);

  const load = () => {
    api.getLeaveBalances(employeeId, CURRENT_YEAR).then(setBalances);
    api.getLeaveRequests(employeeId).then(setRequests);
  };

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [employeeId]);

  const handleRequest = async () => {
    setMessage(null);
    if (!form.startDate || !form.endDate) {
      setMessage("Pick a start and end date.");
      return;
    }
    try {
      await api.requestLeave({
        employeeId,
        leaveType: form.leaveType,
        startDate: form.startDate,
        endDate: form.endDate,
        reason: form.reason || null,
      });
      setMessage("Leave requested.");
      setForm({ ...form, startDate: "", endDate: "", reason: "" });
      load();
    } catch (e) {
      setMessage(String(e));
    }
  };

  const handleApprove = async (id: string) => {
    await api.approveLeave(id);
    load();
  };

  const handleReject = async (id: string) => {
    await api.rejectLeave(id, "Rejected via portal");
    load();
  };

  return (
    <Panel>
      <div className="label" style={{ marginBottom: "0.9rem" }}>Leave</div>

      <div style={{ display: "grid", gridTemplateColumns: "repeat(3, 1fr)", gap: "0.6rem", marginBottom: "1rem" }}>
        {balances.map((b) => (
          <div key={b.leaveType} style={{ ...rowStyle, flexDirection: "column", alignItems: "flex-start", gap: 4 }}>
            <span className="label">{b.leaveType}</span>
            <span className="mono" style={{ fontSize: "1.1rem" }}>{b.availableDays} / {b.totalDays}</span>
          </div>
        ))}
      </div>

      <FieldRow>
        <Field label="Type">
          <select style={inputStyle} value={form.leaveType} onChange={(e) => setForm({ ...form, leaveType: e.target.value })}>
            <option value="Annual">Annual</option>
            <option value="Sick">Sick</option>
            <option value="Casual">Casual</option>
          </select>
        </Field>
        <Field label="Start">
          <input style={inputStyle} type="date" value={form.startDate} onChange={(e) => setForm({ ...form, startDate: e.target.value })} />
        </Field>
        <Field label="End">
          <input style={inputStyle} type="date" value={form.endDate} onChange={(e) => setForm({ ...form, endDate: e.target.value })} />
        </Field>
        <Field label="Reason">
          <input style={{ ...inputStyle, width: 180 }} value={form.reason} onChange={(e) => setForm({ ...form, reason: e.target.value })} />
        </Field>
        <ActionButton accent={ACCENT} onClick={handleRequest}>Request</ActionButton>
      </FieldRow>
      {message && <p style={{ marginTop: "0.6rem", fontSize: "0.8rem", color: "var(--text-dim)" }}>{message}</p>}

      <div style={{ marginTop: "1rem", display: "flex", flexDirection: "column", gap: "0.4rem" }}>
        {requests.map((r, i) => (
          <motion.div key={r.id} initial={{ opacity: 0 }} animate={{ opacity: 1 }} transition={{ delay: i * 0.03 }} style={rowStyle}>
            <span className="mono" style={{ fontSize: "0.8rem" }}>{r.leaveType}</span>
            <span className="mono" style={{ fontSize: "0.8rem", color: "var(--text-dim)" }}>{r.startDate} → {r.endDate} ({r.totalDays}d)</span>
            <div style={{ display: "flex", alignItems: "center", gap: "0.5rem" }}>
              <StatusChip status={r.status} />
              {r.status === "Pending" && (
                <>
                  <ActionButton accent="var(--success)" onClick={() => handleApprove(r.id)}>Approve</ActionButton>
                  <ActionButton accent="var(--danger)" onClick={() => handleReject(r.id)}>Reject</ActionButton>
                </>
              )}
            </div>
          </motion.div>
        ))}
      </div>
    </Panel>
  );
}

function DocumentsSection({ employeeId }: { employeeId: string }) {
  const [documents, setDocuments] = useState<EmployeeDocument[]>([]);
  const [documentType, setDocumentType] = useState("Contract");
  const [message, setMessage] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const load = () => api.getEmployeeDocuments(employeeId).then(setDocuments);

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [employeeId]);

  const handleUpload = async () => {
    setMessage(null);
    const file = fileInputRef.current?.files?.[0];
    if (!file) {
      setMessage("Choose a file first.");
      return;
    }
    try {
      await api.uploadEmployeeDocument(employeeId, documentType, file);
      setMessage("Document uploaded.");
      if (fileInputRef.current) fileInputRef.current.value = "";
      load();
    } catch (e) {
      setMessage(String(e));
    }
  };

  const handleDownload = async (documentId: string, fileName: string) => {
    setMessage(null);
    try {
      await api.downloadEmployeeDocument(documentId, fileName);
    } catch (e) {
      setMessage(String(e));
    }
  };

  return (
    <Panel>
      <div className="label" style={{ marginBottom: "0.9rem" }}>Documents</div>
      <FieldRow>
        <Field label="Type">
          <select style={inputStyle} value={documentType} onChange={(e) => setDocumentType(e.target.value)}>
            <option value="Contract">Contract</option>
            <option value="IdProof">ID Proof</option>
            <option value="Certificate">Certificate</option>
            <option value="Other">Other</option>
          </select>
        </Field>
        <Field label="File">
          <input ref={fileInputRef} type="file" style={{ ...inputStyle, padding: "0.4rem" }} />
        </Field>
        <ActionButton accent={ACCENT} onClick={handleUpload}>Upload</ActionButton>
      </FieldRow>
      {message && <p style={{ marginTop: "0.6rem", fontSize: "0.8rem", color: "var(--text-dim)" }}>{message}</p>}

      <div style={{ marginTop: "1rem", display: "flex", flexDirection: "column", gap: "0.4rem" }}>
        {documents.length === 0 && <p style={{ color: "var(--text-faint)", fontSize: "0.85rem" }}>No documents uploaded.</p>}
        {documents.map((d) => (
          <div key={d.id} style={rowStyle}>
            <span className="mono" style={{ fontSize: "0.8rem", color: ACCENT }}>{d.documentType}</span>
            <button
              onClick={() => handleDownload(d.id, d.fileName)}
              style={{ background: "none", border: "none", color: "inherit", fontSize: "0.85rem", textDecoration: "underline", cursor: "pointer", padding: 0 }}
            >
              {d.fileName}
            </button>
            <span className="mono" style={{ fontSize: "0.72rem", color: "var(--text-faint)" }}>
              {(d.sizeBytes / 1024).toFixed(1)} KB
            </span>
          </div>
        ))}
      </div>
    </Panel>
  );
}

function StatusChip({ status }: { status: string }) {
  const color = status === "Approved" ? "var(--success)" : status === "Rejected" ? "var(--danger)" : "var(--text-faint)";
  return (
    <span className="mono" style={{ fontSize: "0.68rem", color, border: `1px solid ${color}`, borderRadius: "var(--radius)", padding: "0.15rem 0.5rem" }}>
      {status}
    </span>
  );
}

const rowStyle: React.CSSProperties = {
  display: "flex",
  alignItems: "center",
  justifyContent: "space-between",
  padding: "0.6rem 0.8rem",
  background: "var(--bg-elevated)",
  borderRadius: "var(--radius)",
  borderLeft: `2px solid ${ACCENT}`,
};
