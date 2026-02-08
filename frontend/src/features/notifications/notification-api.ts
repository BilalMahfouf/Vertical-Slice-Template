import api from "@/lib/api/api";
import type { CursorPagedList } from "@/components/tables/types";

// ==================== Types ====================

/**
 * Notification filter type matching backend.
 */
export const NotificationType = {
  All: 1,
  NotReaded: 2,
} as const;

export type NotificationType =
  (typeof NotificationType)[keyof typeof NotificationType];

/**
 * Notification response from the backend.
 */
export interface Notification {
  id: string;
  title: string;
  body: string;
  isRead: boolean;
  createdOnUtc: string;
}

/**
 * Request parameters for fetching notifications.
 */
export interface GetNotificationsParams {
  pageSize?: number;
  cursor?: string;
  direction?: "next" | "prev";
  type?: NotificationType;
}

// ==================== API ====================

const notificationApi = {
  /**
   * Get all notifications with cursor-based pagination.
   */
  getAllNotifications: async (
    params?: GetNotificationsParams
  ): Promise<CursorPagedList<Notification>> => {
    const response = await api.get<CursorPagedList<Notification>>(
      "/notifications",
      { params }
    );
    if (response.status !== 200) {
      throw new Error("Failed to fetch notifications");
    }
    return response.data;
  },

  /**
   * Mark a single notification as read.
   */
  markAsRead: async (notificationId: string): Promise<void> => {
    const response = await api.patch<void>(
      `/notifications/${notificationId}/mark-as-read`
    );
    if (response.status !== 204) {
      throw new Error("Failed to mark notification as read");
    }
  },

  /**
   * Mark all notifications as read.
   */
  markAllAsRead: async (): Promise<void> => {
    const response = await api.patch<void>("/notifications/mark-all-as-read");
    if (response.status !== 204) {
      throw new Error("Failed to mark all notifications as read");
    }
  },
};

export default notificationApi;