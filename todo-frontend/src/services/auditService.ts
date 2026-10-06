import api from '../lib/api';
import type { ApiResponse, AuditLog } from '../types';

export const auditService = {
  getLogs: async (params?: { userId?: string; pageNumber?: number; pageSize?: number; action?: string }) => {
    const res = await api.get<ApiResponse<AuditLog[]>>('/audit', { params });
    return res.data;
  },
};
