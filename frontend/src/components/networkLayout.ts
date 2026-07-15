export type ModuleKey = "inventory" | "sales" | "finance" | "hr";

export interface ModuleNode {
  key: ModuleKey;
  label: string;
  x: number; // 0..1 fraction of viewport width
  y: number; // 0..1 fraction of viewport height
  color: string;
}

// Positioned near the corners, deliberately outside the centered ~1180px content
// column, so the network graph reads as ambient background rather than fighting
// the headline/copy for the same screen real estate.
export const MODULE_NODES: ModuleNode[] = [
  { key: "inventory", label: "INVENTORY", x: 0.05, y: 0.12, color: "var(--mod-inventory)" },
  { key: "sales", label: "SALES", x: 0.95, y: 0.14, color: "var(--mod-sales)" },
  { key: "finance", label: "FINANCE", x: 0.93, y: 0.9, color: "var(--mod-finance)" },
  { key: "hr", label: "HR & PAYROLL", x: 0.07, y: 0.92, color: "var(--mod-hr)" },
];

export const MODULE_EDGES: [ModuleKey, ModuleKey][] = [
  ["inventory", "sales"],
  ["sales", "finance"],
  ["hr", "finance"],
  ["inventory", "hr"],
  ["inventory", "finance"],
  ["sales", "hr"],
];

export function resolveColor(cssVar: string): string {
  if (typeof window === "undefined") return "#ffffff";
  if (!cssVar.startsWith("var(")) return cssVar;
  const name = cssVar.slice(4, -1).trim();
  return getComputedStyle(document.documentElement).getPropertyValue(name).trim() || "#ffffff";
}
