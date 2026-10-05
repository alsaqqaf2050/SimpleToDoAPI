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
};
