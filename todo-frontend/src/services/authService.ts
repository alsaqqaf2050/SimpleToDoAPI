// src/services/authService.ts
import api from '../lib/api';
import type { LoginDto, RegisterDto, ApiResponse, AuthResult, CurrentUser } from '../types';

export const authService = {
  login: async (dto: LoginDto): Promise<AuthResult> => {
    const res = await api.post<AuthResult>('/auth/login', dto);
    return res.data;
  },

  register: async (dto: RegisterDto): Promise<AuthResult> => {
    const res = await api.post<AuthResult>('/auth/register', dto);
    return res.data;
  },

  logout: async (): Promise<void> => {
    await api.post('/auth/logout');
  },

  getMe: async (): Promise<CurrentUser> => {
    const res = await api.get<ApiResponse<CurrentUser>>('/auth/me');
    return res.data.data;
  },

  refreshToken: async (refreshToken: string) => {
    const res = await api.post('/auth/refresh', { refreshToken });
    return res.data;
  },
};
