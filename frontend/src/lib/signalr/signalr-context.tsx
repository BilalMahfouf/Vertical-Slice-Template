import {
  useEffect,
  useState,
  type PropsWithChildren,
} from "react";
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";
import { useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { tokenManager } from "../api/tokenManager";
import { SignalRContext } from "./SignalRContext";

// ==================== Helper ====================

function buildConnection(): HubConnection | null{

    let accessToken = getValidAccessToken();
    if (!accessToken) {
        console.log("refresh access Token");
        accessToken = getValidAccessToken();
    }
    if(!accessToken) {
        console.log("no access token available, SignalR connection will not be established");
        return null;
    }
 const baseUrl = import.meta.env.VITE_API_URL || window.location.origin;
    const hubUrl = new URL("/hubs/notification", baseUrl).toString();

 const connection = new HubConnectionBuilder()
    .withUrl(hubUrl, {
      accessTokenFactory: () => accessToken || "",
    })
    .withAutomaticReconnect([ 2000, 10000, 30000]) // Custom retry delays: immediate, 2s, 10s, 30s
    .configureLogging(LogLevel.Warning)
    .build();
    console.log("SignalR connection instance created with URL:", hubUrl);
    return connection;

}
function getValidAccessToken(): string | null {
    const token = tokenManager.getAccessToken();
    if (!token) {
        tokenManager.refreshAccessToken().catch((error) => {
            console.error("Failed to refresh access token:", error);
        });
    }
    return tokenManager.getAccessToken();
}

// ==================== Provider ====================

export const SignalRProvider = ({ children }: PropsWithChildren) => {
  const queryClient = useQueryClient();
  // Build connection instance once using lazy initializer
  const [connection] = useState<HubConnection | null>(() => {
      return buildConnection();
  });

  useEffect(() => {
    // 1. Event Handlers
    const foo = ()=>{
        if(!connection) return;
    connection.on("ReceiveNotification", (data: { notification?: { title?: string; body?: string } }) => {
        console.log("Received notification via SignalR:", data);
      console.log("Invalidating notifications query...");
      queryClient.invalidateQueries({ queryKey: ["notifications","unread"] });

      // Show toast notification
      if (data?.notification) {
        toast(data.notification.title || "New Notification", {
          description: data.notification.body,
        });
      } else {
        toast("New notification received");
      }
    });

    // 2. Connection state handlers
    connection.onreconnecting((error) => {
      console.warn("SignalR reconnecting...", error);
    });

    connection.onreconnected((connectionId) => {
      console.log("SignalR reconnected:", connectionId);
    });

    connection.onclose((error) => {
      console.error("SignalR connection closed:", error);
    });

    // 3. Start connection
    connection.start()
      .then(() => console.log("SignalR Connected"))
      .catch((err) => console.error("SignalR Connection Error:", err));

    // 4. Cleanup on unmount
    return () => {
      connection.off("ReceiveNotification");
      if (connection.state !== HubConnectionState.Disconnected) {
        connection.stop().catch((err) => {
          console.error("SignalR stop error:", err);
        });
      }
    };
 
    }
    foo();
 }, [connection, queryClient]);

  return (
    <SignalRContext.Provider value={connection}>
      {children}
    </SignalRContext.Provider>
  );
};