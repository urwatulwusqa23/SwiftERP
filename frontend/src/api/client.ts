import { clearStoredSession, getStoredToken } from "../auth/AuthContext";

// VITE_API_URL is injected at build time (see .env.production / Render's env vars) — falls back
// to the local dev Api port so `npm run dev` keeps working with no config.
const BASE_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5199";

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const token = getStoredToken();
  const response = await fetch(`${BASE_URL}${path}`, {
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
    ...options,
  });

  if (response.status === 401) {
    clearStoredSession();
    throw new Error("Session expired — please sign in again.");
  }

  if (!response.ok) {
    const body = await response.text().catch(() => "");
    throw new Error(`${options?.method ?? "GET"} ${path} failed (${response.status}): ${body}`);
  }

  if (response.status === 204) return undefined as T;
  return response.json() as Promise<T>;
}

export interface DashboardSummary {
  lowStockProductCount: number;
  draftSaleOrderCount: number;
  financeRunningBalance: number;
  activeEmployeeCount: number;
}

export interface Product {
  id: string;
  sku: string;
  name: string;
  quantityOnHand: number;
  reorderThreshold: number;
}

export interface SaleOrder {
  id: string;
  customerId: string;
  status: string;
  paymentStatus: string;
  total: number;
  lines: { productId: string; quantity: number; unitPrice: number }[];
}

export interface Employee {
  id: string;
}

export interface PayrollRunSummary {
  id: string;
}

export interface EmployeeSummary {
  id: string;
  fullName: string;
  email: string;
  status: string;
  jobTitle: string | null;
  department: string | null;
  managerId: string | null;
}

export interface EmployeeProfile {
  id: string;
  fullName: string;
  email: string;
  monthlySalary: number;
  hireDate: string;
  status: string;
  phoneNumber: string | null;
  address: string | null;
  dateOfBirth: string | null;
  jobTitle: string | null;
  department: string | null;
  managerId: string | null;
  managerName: string | null;
}

export interface OrgChartNode {
  id: string;
  fullName: string;
  jobTitle: string | null;
  department: string | null;
  reports: OrgChartNode[];
}

export interface EmployeeDocument {
  id: string;
  documentType: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  uploadedAtUtc: string;
}

export interface LeaveBalance {
  leaveType: string;
  year: number;
  totalDays: number;
  usedDays: number;
  availableDays: number;
}

export interface LeaveRequestView {
  id: string;
  employeeId: string;
  leaveType: string;
  startDate: string;
  endDate: string;
  totalDays: number;
  reason: string | null;
  status: string;
  requestedAtUtc: string;
}

export interface AttendanceRecordView {
  id: string;
  date: string;
  clockInUtc: string;
  clockOutUtc: string | null;
  workedHours: number | null;
  overtimeHours: number | null;
}

export interface ModulePermissionDto {
  module: string;
  accessLevel: string;
}

export interface RoleDto {
  id: string;
  name: string;
  isSystemRole: boolean;
  permissions: ModulePermissionDto[];
}

export interface UserDto {
  id: string;
  employeeId: string;
  employeeName: string;
  email: string;
  isActive: boolean;
  roleIds: string[];
  roleNames: string[];
}

export const api = {
  getDashboard: () => request<DashboardSummary>("/api/v1/dashboard"),

  getRoles: () => request<RoleDto[]>("/api/v1/admin/roles"),

  createRole: (name: string) =>
    request<{ id: string }>("/api/v1/admin/roles", { method: "POST", body: JSON.stringify({ name }) }),

  updateRolePermission: (roleId: string, module: string, accessLevel: string) =>
    request<void>(`/api/v1/admin/roles/${roleId}/permissions`, {
      method: "PUT",
      body: JSON.stringify({ module, accessLevel }),
    }),

  getUsers: () => request<UserDto[]>("/api/v1/admin/users"),

  createUserAccount: (body: { employeeId: string; email: string; password: string; roleIds: string[] }) =>
    request<{ id: string }>("/api/v1/admin/users", { method: "POST", body: JSON.stringify(body) }),

  assignRole: (userId: string, roleId: string) =>
    request<void>(`/api/v1/admin/users/${userId}/roles`, { method: "POST", body: JSON.stringify({ roleId }) }),

  removeRole: (userId: string, roleId: string) =>
    request<void>(`/api/v1/admin/users/${userId}/roles/${roleId}`, { method: "DELETE" }),

  getLowStockProducts: () => request<Product[]>("/api/v1/inventory/products/low-stock"),

  createProduct: (body: {
    sku: string;
    name: string;
    reorderThreshold: number;
    supplierId: string;
    initialQuantity: number;
  }) =>
    request<{ id: string }>("/api/v1/inventory/products", {
      method: "POST",
      body: JSON.stringify(body),
    }),

  adjustStock: (productId: string, newQuantity: number) =>
    request<void>(`/api/v1/inventory/products/${productId}/stock`, {
      method: "PUT",
      body: JSON.stringify({ newQuantity }),
    }),

  createSaleOrder: (body: {
    customerId: string;
    lines: { productId: string; quantity: number; unitPrice: number }[];
  }) =>
    request<{ id: string }>("/api/v1/sales/orders", {
      method: "POST",
      body: JSON.stringify(body),
    }),

  getSaleOrders: () => request<SaleOrder[]>("/api/v1/sales/orders"),

  getSaleOrder: (id: string) => request<SaleOrder>(`/api/v1/sales/orders/${id}`),

  confirmSaleOrder: (id: string) =>
    request<void>(`/api/v1/sales/orders/${id}/confirm`, { method: "POST" }),

  getFinanceBalance: () => request<{ balance: number }>("/api/v1/finance/ledger/balance"),

  hireEmployee: (body: { fullName: string; email: string; monthlySalary: number; hireDate: string }) =>
    request<{ id: string }>("/api/v1/hr/employees", {
      method: "POST",
      body: JSON.stringify(body),
    }),

  createPayrollRun: (body: { year: number; month: number }) =>
    request<{ id: string }>("/api/v1/hr/payroll-runs", {
      method: "POST",
      body: JSON.stringify(body),
    }),

  postPayrollRun: (id: string) =>
    request<void>(`/api/v1/hr/payroll-runs/${id}/post`, { method: "POST" }),

  getAllEmployees: () => request<EmployeeSummary[]>("/api/v1/hr/employees"),

  getEmployeeProfile: (id: string) => request<EmployeeProfile>(`/api/v1/hr/employees/${id}`),

  updateEmployee: (
    id: string,
    body: {
      phoneNumber: string | null;
      address: string | null;
      dateOfBirth: string | null;
      jobTitle: string | null;
      department: string | null;
      managerId: string | null;
    }
  ) =>
    request<void>(`/api/v1/hr/employees/${id}`, {
      method: "PUT",
      body: JSON.stringify(body),
    }),

  getOrgChart: () => request<OrgChartNode[]>("/api/v1/hr/org-chart"),

  getEmployeeDocuments: (id: string) => request<EmployeeDocument[]>(`/api/v1/hr/employees/${id}/documents`),

  uploadEmployeeDocument: async (id: string, documentType: string, file: File) => {
    const form = new FormData();
    form.append("file", file);
    form.append("documentType", documentType);
    const token = getStoredToken();
    const response = await fetch(`${BASE_URL}/api/v1/hr/employees/${id}/documents`, {
      method: "POST",
      headers: token ? { Authorization: `Bearer ${token}` } : undefined,
      body: form,
    });
    if (response.status === 401) {
      clearStoredSession();
      throw new Error("Session expired — please sign in again.");
    }
    if (!response.ok) throw new Error(`Upload failed (${response.status})`);
    return response.json() as Promise<{ id: string }>;
  },

  // A plain <a href> can't carry an Authorization header, and the download endpoint requires
  // one now, so downloading means fetching the bytes ourselves and opening a local blob URL.
  downloadEmployeeDocument: async (documentId: string, fileName: string) => {
    const token = getStoredToken();
    const response = await fetch(`${BASE_URL}/api/v1/hr/documents/${documentId}/download`, {
      headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    });
    if (response.status === 401) {
      clearStoredSession();
      throw new Error("Session expired — please sign in again.");
    }
    if (!response.ok) throw new Error(`Download failed (${response.status})`);
    const blob = await response.blob();
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = fileName;
    link.click();
    URL.revokeObjectURL(url);
  },

  getLeaveBalances: (employeeId: string, year: number) =>
    request<LeaveBalance[]>(`/api/v1/hr/employees/${employeeId}/leave-balances?year=${year}`),

  getLeaveRequests: (employeeId: string) =>
    request<LeaveRequestView[]>(`/api/v1/hr/employees/${employeeId}/leave-requests`),

  requestLeave: (body: { employeeId: string; leaveType: string; startDate: string; endDate: string; reason: string | null }) =>
    request<{ id: string }>("/api/v1/hr/leave-requests", {
      method: "POST",
      body: JSON.stringify(body),
    }),

  approveLeave: (id: string) => request<void>(`/api/v1/hr/leave-requests/${id}/approve`, { method: "POST" }),

  rejectLeave: (id: string, note: string | null) =>
    request<void>(`/api/v1/hr/leave-requests/${id}/reject`, {
      method: "POST",
      body: JSON.stringify({ note }),
    }),

  getAttendance: (employeeId: string) => request<AttendanceRecordView[]>(`/api/v1/hr/employees/${employeeId}/attendance`),

  clockIn: (employeeId: string) =>
    request<{ id: string }>(`/api/v1/hr/employees/${employeeId}/attendance/clock-in`, { method: "POST" }),

  clockOut: (employeeId: string) =>
    request<void>(`/api/v1/hr/employees/${employeeId}/attendance/clock-out`, { method: "POST" }),
};

export const NOTIFICATIONS_STREAM_URL = `${BASE_URL}/api/v1/notifications/stream`;
