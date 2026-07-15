import { motion } from "framer-motion";
import { MODULE_NODES } from "./networkLayout";

export function ModuleLabels() {
  return (
    <div style={{ position: "fixed", inset: 0, zIndex: 1, pointerEvents: "none" }}>
      {MODULE_NODES.map((node, i) => (
        <motion.div
          key={node.key}
          initial={{ opacity: 0, y: 8 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 + i * 0.12, duration: 0.6, ease: "easeOut" }}
          style={{
            position: "absolute",
            left: `${node.x * 100}%`,
            top: `${node.y * 100}%`,
            transform: "translate(-50%, 14px)",
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            gap: 2,
          }}
        >
          <span
            className="label"
            style={{
              color: node.color,
              fontSize: "0.65rem",
              opacity: 0.85,
              whiteSpace: "nowrap",
              textShadow: "0 2px 12px rgba(0,0,0,0.8)",
            }}
          >
            {node.label}
          </span>
        </motion.div>
      ))}
    </div>
  );
}
