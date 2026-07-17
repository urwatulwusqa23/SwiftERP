import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from "react";

export type Module = "Inventory" | "Sales" | "Finance" | "HR";
export type AccessLevel = "None" | "View" | "Edit" | "Full";

const ACCESS_RANK: Record<AccessLevel, number> = { None: 0, View: 1, Edit: 2, Full: 3 };

export interface AuthUser {
  userId: string;
  employeeId: string;
  email: string;
  roles: string[];
  isSystemAdmin: boolean;
  permissions: Record<Module, AccessLevel>;
}

interface StoredSession {
  token: string;
  expiresAtUtc: string;
  user: AuthUser;
}

interface AuthContextValue {
  token: string | null;
  user: AuthUser | null;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  hasAccess: (module: Module, minLevel: AccessLevel) => boolean;
}

const STORAGE_KEY = "swifterp:session";
const AuthContext = createContext<AuthContextValue | null>(null);

function userFromLoginResponse(body: {
  userId: string;
  employeeId: string;
  email: string;
  roles: string[];
  permissions: Record<string, string>;
}): AuthUser {
  return {
    userId: body.userId,
    employeeId: body.employeeId,
    email: body.email,
    roles: body.roles,
    isSystemAdmin: body.roles.includes("Admin"),
    permissions: body.permissions as Record<Module, AccessLevel>,
  };
}

function loadSession(): StoredSession | null {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) return null;
  try {
    const parsed = JSON.parse(raw) as StoredSession;
    if (new Date(parsed.expiresAtUtc).getTime() <= Date.now()) {
      localStorage.removeItem(STORAGE_KEY);
      return null;
    }
    return parsed;
  } catch {
    return null;
  }
}

const BASE_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5199";

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<StoredSession | null>(() => loadSession());

  const login = useCallback(async (email: string, password: string) => {
    const response = await fetch(`${BASE_URL}/api/v1/auth/login`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, password }),
    });

    if (!response.ok) {
      const body = await response.json().catch(() => ({ error: "Login failed." }));
      throw new Error(body.error ?? "Login failed.");
    }

    const body = await response.json();
    const next: StoredSession = {
      token: body.token,
      expiresAtUtc: body.expiresAtUtc,
      user: userFromLoginResponse(body),
    };
    localStorage.setItem(STORAGE_KEY, JSON.stringify(next));
    setSession(next);
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem(STORAGE_KEY);
    setSession(null);
  }, []);

  const hasAccess = useCallback(
    (module: Module, minLevel: AccessLevel) => {
      if (!session) return false;
      const level = session.user.permissions[module] ?? "None";
      return ACCESS_RANK[level] >= ACCESS_RANK[minLevel];
    },
    [session]
  );

  const value = useMemo<AuthContextValue>(
    () => ({ token: session?.token ?? null, user: session?.user ?? null, login, logout, hasAccess }),
    [session, login, logout, hasAccess]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within an AuthProvider");
  return ctx;
}

export function getStoredToken(): string | null {
  return loadSession()?.token ?? null;
}

// Exported so client.ts can react to a 401 (expired/invalid token) without importing React.
export function clearStoredSession() {
  localStorage.removeItem(STORAGE_KEY);
  window.dispatchEvent(new Event("swifterp:session-expired"));
}
