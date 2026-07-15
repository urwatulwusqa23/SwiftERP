import { useEffect, useRef } from "react";
import { MODULE_EDGES, MODULE_NODES, resolveColor, type ModuleKey } from "./networkLayout";

interface Particle {
  x: number;
  y: number;
  vx: number;
  vy: number;
}

interface Ring {
  key: ModuleKey;
  x: number;
  y: number;
  startedAt: number;
  color: string;
}

interface Props {
  pulseSignal: { key: ModuleKey; nonce: number } | null;
}

const PARTICLE_COUNT = 46;
const PARTICLE_LINK_DIST = 130;
const RING_DURATION_MS = 1400;

export function NetworkBackground({ pulseSignal }: Props) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const particlesRef = useRef<Particle[]>([]);
  const ringsRef = useRef<Ring[]>([]);
  const lastPulseNonce = useRef<number>(-1);

  useEffect(() => {
    if (!pulseSignal || pulseSignal.nonce === lastPulseNonce.current) return;
    lastPulseNonce.current = pulseSignal.nonce;

    const node = MODULE_NODES.find((n) => n.key === pulseSignal.key);
    if (!node) return;

    ringsRef.current.push({
      key: pulseSignal.key,
      x: node.x * window.innerWidth,
      y: node.y * window.innerHeight,
      startedAt: performance.now(),
      color: resolveColor(node.color),
    });
  }, [pulseSignal]);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    let width = window.innerWidth;
    let height = window.innerHeight;
    let dpr = Math.min(window.devicePixelRatio || 1, 2);

    function resize() {
      width = window.innerWidth;
      height = window.innerHeight;
      dpr = Math.min(window.devicePixelRatio || 1, 2);
      canvas!.width = width * dpr;
      canvas!.height = height * dpr;
      canvas!.style.width = `${width}px`;
      canvas!.style.height = `${height}px`;
      ctx!.setTransform(dpr, 0, 0, dpr, 0, 0);
    }
    resize();
    window.addEventListener("resize", resize);

    particlesRef.current = Array.from({ length: PARTICLE_COUNT }, () => ({
      x: Math.random() * width,
      y: Math.random() * height,
      vx: (Math.random() - 0.5) * 0.18,
      vy: (Math.random() - 0.5) * 0.18,
    }));

    const nodeColors = MODULE_NODES.map((n) => ({ ...n, resolved: resolveColor(n.color) }));
    const borderColor = resolveColor("var(--border-bright)");

    let raf = 0;
    let startTime = performance.now();

    function frame(now: number) {
      const t = now - startTime;
      ctx!.clearRect(0, 0, width, height);

      // Ambient particle field: soft drifting dots, linked when close, gently
      // pulled toward the nearest module hub — reads as "the whole system is
      // quietly alive" rather than a decorative loop.
      const particles = particlesRef.current;
      for (const p of particles) {
        p.x += p.vx;
        p.y += p.vy;
        if (p.x < 0 || p.x > width) p.vx *= -1;
        if (p.y < 0 || p.y > height) p.vy *= -1;
      }

      ctx!.lineWidth = 1;
      for (let i = 0; i < particles.length; i++) {
        for (let j = i + 1; j < particles.length; j++) {
          const a = particles[i];
          const b = particles[j];
          const dist = Math.hypot(a.x - b.x, a.y - b.y);
          if (dist < PARTICLE_LINK_DIST) {
            ctx!.strokeStyle = `rgba(94, 122, 110, ${0.14 * (1 - dist / PARTICLE_LINK_DIST)})`;
            ctx!.beginPath();
            ctx!.moveTo(a.x, a.y);
            ctx!.lineTo(b.x, b.y);
            ctx!.stroke();
          }
        }
      }

      ctx!.fillStyle = "rgba(140, 160, 150, 0.35)";
      for (const p of particles) {
        ctx!.beginPath();
        ctx!.arc(p.x, p.y, 1.1, 0, Math.PI * 2);
        ctx!.fill();
      }

      // Module edges: the six connections between Inventory/Sales/Finance/HR,
      // with a traveling highlight showing data "flowing" between modules —
      // a direct visualization of the cross-module transactions in the API.
      const nodePositions = new Map(
        nodeColors.map((n) => [n.key, { x: n.x * width, y: n.y * height, color: n.resolved }])
      );

      MODULE_EDGES.forEach(([from, to], edgeIndex) => {
        const a = nodePositions.get(from)!;
        const b = nodePositions.get(to)!;
        const isPrimary = edgeIndex < 4;

        ctx!.strokeStyle = isPrimary ? "rgba(90, 120, 108, 0.28)" : "rgba(70, 90, 82, 0.12)";
        ctx!.lineWidth = isPrimary ? 1.2 : 1;
        ctx!.beginPath();
        ctx!.moveTo(a.x, a.y);
        ctx!.lineTo(b.x, b.y);
        ctx!.stroke();

        if (isPrimary) {
          const speed = 0.00022;
          const phase = (t * speed + edgeIndex * 0.31) % 1;
          const px = a.x + (b.x - a.x) * phase;
          const py = a.y + (b.y - a.y) * phase;
          const grad = ctx!.createRadialGradient(px, py, 0, px, py, 7);
          grad.addColorStop(0, "rgba(255, 210, 140, 0.9)");
          grad.addColorStop(1, "rgba(255, 210, 140, 0)");
          ctx!.fillStyle = grad;
          ctx!.beginPath();
          ctx!.arc(px, py, 7, 0, Math.PI * 2);
          ctx!.fill();
        }
      });

      // Module hubs: a soft breathing glow so each node reads as "live"
      // infrastructure, not a static icon.
      for (const n of nodeColors) {
        const x = n.x * width;
        const y = n.y * height;
        const breathe = 0.5 + 0.5 * Math.sin(t * 0.0012 + n.x * 10);
        const radius = 26 + breathe * 6;

        const grad = ctx!.createRadialGradient(x, y, 0, x, y, radius);
        grad.addColorStop(0, hexToRgba(n.resolved, 0.22 + breathe * 0.1));
        grad.addColorStop(1, hexToRgba(n.resolved, 0));
        ctx!.fillStyle = grad;
        ctx!.beginPath();
        ctx!.arc(x, y, radius, 0, Math.PI * 2);
        ctx!.fill();

        ctx!.fillStyle = n.resolved;
        ctx!.beginPath();
        ctx!.arc(x, y, 3.5, 0, Math.PI * 2);
        ctx!.fill();

        ctx!.strokeStyle = borderColor;
        ctx!.lineWidth = 1;
        ctx!.beginPath();
        ctx!.arc(x, y, 6.5, 0, Math.PI * 2);
        ctx!.stroke();
      }

      // Expanding rings for live pulse events (a stock-low or payroll-processed
      // notification arriving through the SSE stream).
      ringsRef.current = ringsRef.current.filter((r) => now - r.startedAt < RING_DURATION_MS);
      for (const r of ringsRef.current) {
        const progress = (now - r.startedAt) / RING_DURATION_MS;
        const radius = 8 + progress * 70;
        const alpha = 1 - progress;
        ctx!.strokeStyle = hexToRgba(r.color, alpha * 0.8);
        ctx!.lineWidth = 2;
        ctx!.beginPath();
        ctx!.arc(r.x, r.y, radius, 0, Math.PI * 2);
        ctx!.stroke();
      }

      raf = requestAnimationFrame(frame);
    }

    raf = requestAnimationFrame(frame);

    return () => {
      cancelAnimationFrame(raf);
      window.removeEventListener("resize", resize);
    };
  }, []);

  return <canvas ref={canvasRef} style={{ position: "fixed", inset: 0, zIndex: 0 }} />;
}

function hexToRgba(hex: string, alpha: number): string {
  const clean = hex.replace("#", "");
  const bigint = parseInt(clean.length === 3 ? clean.split("").map((c) => c + c).join("") : clean, 16);
  const r = (bigint >> 16) & 255;
  const g = (bigint >> 8) & 255;
  const b = bigint & 255;
  return `rgba(${r}, ${g}, ${b}, ${alpha})`;
}
