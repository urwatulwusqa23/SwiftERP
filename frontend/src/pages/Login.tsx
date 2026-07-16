import { useState } from "react";
import { motion } from "framer-motion";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { ActionButton, Field, inputStyle } from "../components/PageShell";
import { NetworkBackground } from "../components/NetworkBackground";

export function Login() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await login(email, password);
      navigate("/", { replace: true });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Login failed.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div
      style={{
        position: "relative",
        minHeight: "100vh",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
      }}
    >
      <NetworkBackground pulseSignal={null} />
      <div className="noise-vignette" />
      <div className="scanlines" />

      <motion.form
        onSubmit={handleSubmit}
        initial={{ opacity: 0, y: 16 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5 }}
        style={{
          position: "relative",
          zIndex: 2,
          width: 360,
          background: "rgba(8, 11, 10, 0.72)",
          backdropFilter: "blur(10px)",
          border: "1px solid var(--border)",
          borderRadius: "var(--radius)",
          padding: "2rem",
        }}
      >
        <div style={{ marginBottom: "1.6rem" }}>
          <span style={{ fontFamily: "var(--font-display)", fontSize: "1.4rem", fontWeight: 700 }}>
            SwiftERP
          </span>
          <div className="label" style={{ marginTop: "0.3rem" }}>
            Command Deck — Sign In
          </div>
        </div>

        <div style={{ display: "grid", gap: "0.9rem", marginBottom: "1.2rem" }}>
          <Field label="Email">
            <input
              style={{ ...inputStyle, width: "100%" }}
              type="email"
              autoComplete="username"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </Field>
          <Field label="Password">
            <input
              style={{ ...inputStyle, width: "100%" }}
              type="password"
              autoComplete="current-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </Field>
        </div>

        {error && (
          <p style={{ color: "var(--danger)", fontSize: "0.8rem", marginBottom: "1rem" }}>{error}</p>
        )}

        <ActionButton accent="var(--accent)" type="submit" disabled={submitting}>
          {submitting ? "Signing in…" : "Sign In"}
        </ActionButton>
      </motion.form>
    </div>
  );
}
