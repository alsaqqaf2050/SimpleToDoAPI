// src/services/dashboardService.ts
import api from '../lib/api';
import type { DashboardResponse } from '../types';

export const dashboardService = {
  getStats: async () => {
    const res = await api.get<DashboardResponse>('/Dashboard/stats');
    return res.data;
  },
};
