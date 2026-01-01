import api from './api';
import { tokenManager } from './tokenManager';

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface LoginResponse {
  value: {
    token: string;
  };
}

export const authApi = {
  login: async (credentials: LoginCredentials) => {
    const response = await api.post<LoginResponse>('/auth/login', credentials);
    
    // Store access token in memory
    tokenManager.setAccessToken(response.data.value.token);
    
    // refreshToken is automatically stored in httpOnly cookie by backend
    return response.data;
  },

  logout: async () => {
    try {
      await api.post('/auth/logout');
    } finally {
      tokenManager.clearTokens();
     // window.location.href = '/login';
    }
  },

  // Manual refresh if needed
  refresh: async () => {
    return tokenManager.refreshAccessToken();
  },
};