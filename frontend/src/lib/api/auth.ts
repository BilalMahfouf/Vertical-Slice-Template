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

export interface ForgotPasswordRequest {
  email: string;
  clientUri: string;
}

export interface ResetPasswordRequest {
  email: string;
  token: string;
  password: string;
  confirmPassword: string;
}

export const authApi = {
  login: async (credentials: LoginCredentials) => {
    const response = await api.post<LoginResponse>('/auth/login', credentials);
    
    if(response.status !== 200) {
        console.log('Login failed with status:', response.status);
        return false;
    }
    tokenManager.setAccessToken(response.data.value.token);
    return true;
  },

  logout: async () => {
    try {
      await api.post('/auth/logout');
    } finally {
      tokenManager.clearTokens();
      window.location.href = '/login';
    }
  },

  // Manual refresh if needed
  refresh: async () => {
    return tokenManager.refreshAccessToken();
  },

  forgotPassword: async (data: ForgotPasswordRequest) => {
    return api.post('/auth/forget-password', data);
  },

  resetPassword: async (data: ResetPasswordRequest) => {
    // Backend endpoint has typo: reset-passowrd
    return api.put('/auth/reset-passowrd', data);
  },
};