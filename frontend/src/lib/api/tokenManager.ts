import api from './api';
import i18n from '../i18n';
import i18nKeyContainer from '../i18n/keyContainer';
import type { AxiosError } from 'axios';

let accessToken: string | null = null;
let isRefreshing = false;
let refreshSubscribers: Array<{
  resolve: (token: string) => void;
  reject: (error: unknown) => void;
}> = [];

// Notify all queued requests with the new token
const onRefreshed = (token: string) => {
  refreshSubscribers.forEach((subscriber) => subscriber.resolve(token));
  refreshSubscribers = [];
};

// Notify all queued requests that refresh failed
const onRefreshFailed = (error: unknown) => {
  refreshSubscribers.forEach((subscriber) => subscriber.reject(error));
  refreshSubscribers = [];
};

// Add request to queue
const addRefreshSubscriber = (
  resolve: (token: string) => void,
  reject: (error: unknown) => void
) => {
  refreshSubscribers.push({ resolve, reject });
};

export const tokenManager = {
  getAccessToken: () => accessToken,
  
  setAccessToken: (token: string | null) => {
    accessToken = token;
  },
  
  clearTokens: () => {
    accessToken = null;
  },

  refreshAccessToken: async (): Promise<string | null> => {
    try {
      // refreshToken is sent automatically via httpOnly cookie
    //   console.log('Refreshing access token...');
      const response = await api.post('/auth/refresh-token', {}, {
        skipAuthRefresh: true
      });
    //   console.log('Refresh token response status:', response.status);   
      if(response.status !== 200) {
        return null;
      }
      const newAccessToken = response.data.value.token;
      console.log('Access token refreshed.');
      
      tokenManager.setAccessToken(newAccessToken);
      return newAccessToken;
    } catch(error)  {
      tokenManager.clearTokens();
      throw error;
    }
  },
};

// Request interceptor - attach access token
api.interceptors.request.use(
  (config) => {
    // Skip token attachment for endpoints that don't require auth
    if (config.skipAuthRefresh) {
      return config;
    }
    const token = tokenManager.getAccessToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor - handle 401 with token refresh
api.interceptors.response.use(
  (response) => response, 
  async (error: AxiosError) => {
    const originalRequest = error.config;
    if(!originalRequest) {
        return Promise.reject(error);
    }
    if(originalRequest.skipAuthRefresh) {
        return Promise.reject(error);
    }

    // If 401 and not already retried
    if (error.response?.status === 401 && !originalRequest._retry) {
      console.log('401 Unauthorized - attempting token refresh');
      originalRequest._retry = true;
      
      if (isRefreshing) {
        console.log('Token refresh already in progress, queuing request');
        // Queue this request until refresh completes
        return new Promise((resolve, reject) => {
          addRefreshSubscriber(
            (token: string) => {
              originalRequest.headers.Authorization = `Bearer ${token}`;
              resolve(api(originalRequest));
            },
            (err: unknown) => {
              reject(err);
            }
          );
        });
      }

      console.log('Refreshing token for 401 response');
      isRefreshing = true;

      try {
        const newToken = await tokenManager.refreshAccessToken();
        
        if (newToken) {
          onRefreshed(newToken);
          originalRequest.headers.Authorization = `Bearer ${newToken}`;
          return api(originalRequest);
        } else {
          // Token refresh failed - notify subscribers and redirect to login
          tokenManager.clearTokens();
          onRefreshFailed(error);
          const message = i18n.t(i18nKeyContainer.sessionExpiredMessage);
          window.confirm(message);
          window.location.href = '/login';
          return Promise.reject(error);
        }
      } catch (refreshError) {
        // Clear tokens, notify subscribers on error and redirect to login
        tokenManager.clearTokens();
        onRefreshFailed(refreshError);
        const message = i18n.t(i18nKeyContainer.sessionExpiredMessage);
        window.confirm(message);
        window.location.href = '/login';
        return Promise.reject(refreshError);
      } finally {
        // Always reset the flag
        isRefreshing = false;
      }
    }
    return Promise.reject(error);
  }
);