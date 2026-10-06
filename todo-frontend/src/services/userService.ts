// src/services/userService.ts
import api from '../lib/api';
import type { ApiResponse, AppUser } from '../types';

export const userService = {
  getAll: async (pageNumber = 1, pageSize = 10) => {
    const res = await api.get<ApiResponse<AppUser[]>>('/users', {
      params: { pageNumber, pageSize },
    });
    return res.data;
  },

  delete: async (id: string) => {
    await api.delete(`/users/${id}`);
  },

  update: async (id: string, data: { fullName: string; email: string; isActive: boolean }) => {
    const res = await api.put<ApiResponse<AppUser>>(`/users/${id}`, data);
    return res.data;
  },

  assignRole: async (userId: string, roleName: string) => {
    const res = await api.post<ApiResponse<any>>('/roles/assign-user', { userId, roleName });
    return res.data;
  },
};
