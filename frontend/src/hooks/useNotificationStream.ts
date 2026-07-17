import { useEffect, useRef, useState } from "react";
import { NOTIFICATIONS_STREAM_URL } from "../api/client";
import { getStoredToken } from "../auth/AuthContext";
import type { ModuleKey } from "../components/networkLayout";

export interface LiveNotification {
  id: number;
  type: string;
  occurredAtUtc: string;
  payload: Record<string, unknown>;
  module: ModuleKey;
}

const TYPE_TO_MODULE: Record<string, ModuleKey> = {
  "stock.low": "inventory",
  "payroll.processed": "hr",
};

let counter = 0;

export function useNotificationStream(onEvent?: (n: LiveNotification) => void) {
  const [notifications, setNotifications] = useState<LiveNotification[]>([]);
  const [connected, setConnected] = useState(false);
  const onEventRef = useRef(onEvent);
  onEventRef.current = onEvent;

  useEffect(() => {
    const token = getStoredToken();
    if (!token) return;

    // EventSource can't set an Authorization header, so the token rides along as a query
    // param — the Api's JWT bearer handler is configured to read it from there for this path.
    const source = new EventSource(`${NOTIFICATIONS_STREAM_URL}?access_token=${encodeURIComponent(token)}`);

    source.onopen = () => setConnected(true);
    source.onerror = () => setConnected(false);

    source.onmessage = (event) => {
      try {
        const parsed = JSON.parse(event.data);
        const notification: LiveNotification = {
          id: ++counter,
          type: parsed.type,
          occurredAtUtc: parsed.occurredAtUtc,
          payload: parsed.payload ?? {},
          module: TYPE_TO_MODULE[parsed.type] ?? "finance",
        };
        setNotifications((prev) => [notification, ...prev].slice(0, 20));
        onEventRef.current?.(notification);
      } catch {
        // Ignore malformed/comment frames (the initial ": connected" keep-alive).
      }
    };

    return () => source.close();
  }, []);

  return { notifications, connected };
}
