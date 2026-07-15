import { useEffect, useRef } from "react";
import { animate, motion, useMotionValue, useTransform } from "framer-motion";

interface Props {
  label: string;
  value: number;
  accent: string;
  format?: (n: number) => string;
  suffix?: string;
  index: number;
}

export function StatCard({ label, value, accent, format, suffix, index }: Props) {
  const motionValue = useMotionValue(0);
  const rounded = useTransform(motionValue, (v) => (format ? format(v) : Math.round(v).toLocaleString()));
  const prevValue = useRef(0);

  useEffect(() => {
    const controls = animate(motionValue, value, {
      duration: 1.1,
      ease: [0.16, 1, 0.3, 1],
      delay: 0.15 * index,
    });
    prevValue.current = value;
    return controls.stop;
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [value]);

  return (
    <motion.div
      initial={{ opacity: 0, y: 16 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay: 0.1 * index, duration: 0.5, ease: "easeOut" }}
      whileHover={{ y: -3, borderColor: accent }}
      style={{
        background: "var(--surface)",
        border: "1px solid var(--border)",
        borderRadius: "var(--radius)",
        padding: "1.4rem 1.5rem",
        position: "relative",
        transition: "border-color 0.25s",
        minWidth: 0,
        containerType: "inline-size",
      } as React.CSSProperties}
    >
      <div
        style={{
          position: "absolute",
          top: 0,
          left: 0,
          right: 0,
          height: 2,
          background: accent,
          opacity: 0.7,
          borderTopLeftRadius: "var(--radius)",
          borderTopRightRadius: "var(--radius)",
        }}
      />
      <div className="label" style={{ marginBottom: "0.6rem" }}>
        {label}
      </div>
      <div style={{ display: "flex", alignItems: "baseline", gap: "0.35rem", minWidth: 0 }}>
        <motion.span
          className="mono"
          style={{
            fontSize: "clamp(1.15rem, 5cqw, 2.1rem)",
            fontWeight: 600,
            color: "var(--text)",
            whiteSpace: "nowrap",
          }}
        >
          {rounded}
        </motion.span>
        {suffix && (
          <span className="mono" style={{ fontSize: "0.9rem", color: "var(--text-dim)" }}>
            {suffix}
          </span>
        )}
      </div>
    </motion.div>
  );
}
