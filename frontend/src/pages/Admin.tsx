import { useEffect, useState } from "react";
import { api, type RoleDto, type UserDto } from "../api/client";
import { ActionButton, Field, FieldRow, inputStyle, PageShell, Panel } from "../components/PageShell";

const ACCENT = "var(--accent)";
const MODULES = ["Inventory", "Sales", "Finance", "HR"] as const;
const LEVELS = ["None", "View", "Edit", "Full"] as const;

export function Admin() {
  return (
    <PageShell
      moduleLabel="Access Control"
      title="Admin Portal"
      accent={ACCENT}
      description="Manage role permissions and user accounts."
    >
      <div style={{ display: "grid", gap: "1rem" }}>
        <RolesPanel />
        <UsersPanel />
      </div>
    </PageShell>
  );
}

function RolesPanel() {
  const [roles, setRoles] = useState<RoleDto[]>([]);
  const [newRoleName, setNewRoleName] = useState("");
  const [message, setMessage] = useState<string | null>(null);

  const load = () => api.getRoles().then(setRoles);

  useEffect(() => {
    load();
  }, []);

  const handleCreateRole = async () => {
    if (!newRoleName.trim()) return;
    setMessage(null);
    try {
      await api.createRole(newRoleName.trim());
      setNewRoleName("");
      load();
    } catch (e) {
      setMessage(String(e));
    }
  };

  const handleChangeLevel = async (roleId: string, module: string, accessLevel: string) => {
    setMessage(null);
    try {
      await api.updateRolePermission(roleId, module, accessLevel);
      load();
    } catch (e) {
      setMessage(String(e));
    }
  };

  return (
    <Panel>
      <div className="label" style={{ marginBottom: "0.9rem" }}>
        Roles &amp; Permission Matrix
      </div>

      <div style={{ display: "grid", gap: "0.8rem", marginBottom: "1.2rem" }}>
        {roles.map((role) => (
          <div
            key={role.id}
            style={{
              display: "grid",
              gridTemplateColumns: "160px repeat(4, 1fr)",
              alignItems: "center",
              gap: "0.6rem",
              padding: "0.7rem 0.9rem",
              background: "var(--bg-elevated)",
              borderRadius: "var(--radius)",
              borderLeft: `2px solid ${ACCENT}`,
            }}
          >
            <div>
              <div style={{ fontSize: "0.9rem", fontWeight: 600 }}>{role.name}</div>
              {role.isSystemRole && (
                <span className="mono" style={{ fontSize: "0.65rem", color: "var(--text-faint)" }}>
                  system role
                </span>
              )}
            </div>
            {MODULES.map((module) => {
              const current = role.permissions.find((p) => p.module === module)?.accessLevel ?? "None";
              return (
                <select
                  key={module}
                  style={{ ...inputStyle, fontSize: "0.75rem" }}
                  value={current}
                  onChange={(e) => handleChangeLevel(role.id, module, e.target.value)}
                >
                  {LEVELS.map((level) => (
                    <option key={level} value={level}>
                      {module}: {level}
                    </option>
                  ))}
                </select>
              );
            })}
          </div>
        ))}
      </div>

      <FieldRow>
        <Field label="New Role Name">
          <input style={inputStyle} value={newRoleName} onChange={(e) => setNewRoleName(e.target.value)} />
        </Field>
        <ActionButton accent={ACCENT} onClick={handleCreateRole}>
          Create Role
        </ActionButton>
      </FieldRow>
      {message && <p style={{ marginTop: "0.7rem", fontSize: "0.8rem", color: "var(--danger)" }}>{message}</p>}
    </Panel>
  );
}

function UsersPanel() {
  const [users, setUsers] = useState<UserDto[]>([]);
  const [roles, setRoles] = useState<RoleDto[]>([]);
  const [form, setForm] = useState({ employeeId: "", email: "", password: "", roleId: "" });
  const [message, setMessage] = useState<string | null>(null);

  const load = () => {
    api.getUsers().then(setUsers);
    api.getRoles().then(setRoles);
  };

  useEffect(() => {
    load();
  }, []);

  const handleCreateUser = async () => {
    setMessage(null);
    if (!form.employeeId || !form.email || !form.password) {
      setMessage("Employee ID, email, and password are required.");
      return;
    }
    try {
      await api.createUserAccount({
        employeeId: form.employeeId,
        email: form.email,
        password: form.password,
        roleIds: form.roleId ? [form.roleId] : [],
      });
      setForm({ employeeId: "", email: "", password: "", roleId: "" });
      load();
    } catch (e) {
      setMessage(String(e));
    }
  };

  const handleAssignRole = async (userId: string, roleId: string) => {
    if (!roleId) return;
    await api.assignRole(userId, roleId);
    load();
  };

  const handleRemoveRole = async (userId: string, roleId: string) => {
    await api.removeRole(userId, roleId);
    load();
  };

  return (
    <Panel>
      <div className="label" style={{ marginBottom: "0.9rem" }}>
        User Accounts
      </div>

      <div style={{ display: "flex", flexDirection: "column", gap: "0.5rem", marginBottom: "1.2rem" }}>
        {users.length === 0 && <p style={{ color: "var(--text-faint)", fontSize: "0.85rem" }}>No user accounts yet.</p>}
        {users.map((u) => (
          <div
            key={u.id}
            style={{
              display: "flex",
              alignItems: "center",
              justifyContent: "space-between",
              padding: "0.6rem 0.8rem",
              background: "var(--bg-elevated)",
              borderRadius: "var(--radius)",
              borderLeft: `2px solid ${ACCENT}`,
              flexWrap: "wrap",
              gap: "0.5rem",
            }}
          >
            <div>
              <div style={{ fontSize: "0.85rem" }}>{u.employeeName}</div>
              <span className="mono" style={{ fontSize: "0.72rem", color: "var(--text-faint)" }}>
                {u.email} {!u.isActive && "· deactivated"}
              </span>
            </div>
            <div style={{ display: "flex", alignItems: "center", gap: "0.4rem", flexWrap: "wrap" }}>
              {u.roleIds.map((roleId, i) => (
                <span
                  key={roleId}
                  className="mono"
                  style={{ fontSize: "0.7rem", border: "1px solid var(--border)", borderRadius: "var(--radius)", padding: "0.15rem 0.5rem", display: "flex", gap: "0.3rem", alignItems: "center" }}
                >
                  {u.roleNames[i]}
                  <button
                    onClick={() => handleRemoveRole(u.id, roleId)}
                    style={{ background: "none", border: "none", color: "var(--danger)", cursor: "pointer", fontSize: "0.75rem", padding: 0 }}
                  >
                    ×
                  </button>
                </span>
              ))}
              <select
                style={{ ...inputStyle, fontSize: "0.72rem" }}
                value=""
                onChange={(e) => handleAssignRole(u.id, e.target.value)}
              >
                <option value="">+ assign role</option>
                {roles.filter((r) => !u.roleIds.includes(r.id)).map((r) => (
                  <option key={r.id} value={r.id}>{r.name}</option>
                ))}
              </select>
            </div>
          </div>
        ))}
      </div>

      <div className="label" style={{ marginBottom: "0.7rem" }}>
        Create User Account for Existing Employee
      </div>
      <FieldRow>
        <Field label="Employee ID">
          <input
            style={{ ...inputStyle, width: 260 }}
            placeholder="uuid"
            value={form.employeeId}
            onChange={(e) => setForm({ ...form, employeeId: e.target.value })}
          />
        </Field>
        <Field label="Email">
          <input style={inputStyle} value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
        </Field>
        <Field label="Password">
          <input
            style={inputStyle}
            type="password"
            value={form.password}
            onChange={(e) => setForm({ ...form, password: e.target.value })}
          />
        </Field>
        <Field label="Initial Role">
          <select style={inputStyle} value={form.roleId} onChange={(e) => setForm({ ...form, roleId: e.target.value })}>
            <option value="">None</option>
            {roles.map((r) => (
              <option key={r.id} value={r.id}>{r.name}</option>
            ))}
          </select>
        </Field>
        <ActionButton accent={ACCENT} onClick={handleCreateUser}>
          Create Account
        </ActionButton>
      </FieldRow>
      {message && <p style={{ marginTop: "0.7rem", fontSize: "0.8rem", color: "var(--danger)" }}>{message}</p>}
      <p style={{ marginTop: "0.7rem", fontSize: "0.75rem", color: "var(--text-faint)" }}>
        Find an employee's ID on the HR page's org chart / employee list.
      </p>
    </Panel>
  );
}
