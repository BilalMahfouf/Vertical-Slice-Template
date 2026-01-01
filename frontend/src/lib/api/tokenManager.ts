import api from './api';

let accessToken: string | null = null;
let isRefreshing = false;
let refreshSubscribers: ((token: string) => void)[] = [];

// Notify all queued requests with the new token
const onRefreshed = (token: string) => {
  refreshSubscribers.forEach((callback) => callback(token));
  refreshSubscribers = [];
};

// Add request to queue
const addRefreshSubscriber = (callback: (token: string) => void) => {
  refreshSubscribers.push(callback);
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
      console.log("Refreshing access token...");
      console.log("cockies:", document.cookie);
      const response = await api.post('/auth/refresh-token', {});
      const newAccessToken = response.data.value.token;
      
      tokenManager.setAccessToken(newAccessToken);
      return newAccessToken;
    } catch (error) {
      tokenManager.clearTokens();
      // Redirect to login or emit event
      window.location.href = '/login';
      return null;
    }
  },
};

// Request interceptor - attach access token
api.interceptors.request.use(
  (config) => {
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
  async (error) => {
    const originalRequest = error.config;

    // If 401 and not already retried
    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        // Queue this request until refresh completes
        return new Promise((resolve) => {
          addRefreshSubscriber((token: string) => {
            originalRequest.headers.Authorization = `Bearer ${token}`;
            resolve(api(originalRequest));
          });
        });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const newToken = await tokenManager.refreshAccessToken();
        
        if (newToken) {
          isRefreshing = false;
          onRefreshed(newToken);
          
          originalRequest.headers.Authorization = `Bearer ${newToken}`;
          return api(originalRequest);
        }
      } catch (refreshError) {
        isRefreshing = false;
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);