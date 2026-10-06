import api from './api';

export interface MaintenanceStatus {
  isSuccess: boolean;
  isMaintenanceMode: boolean;
  message?: string;
}

export const systemService = {
  getMaintenanceStatus: async (): Promise<MaintenanceStatus> => {
    const response = await api.get<MaintenanceStatus>('/system/maintenance/status');
    return response.data;
  },

  toggleMaintenanceMode: async (isMaintenanceMode: boolean): Promise<MaintenanceStatus> => {
    const response = await api.post<MaintenanceStatus>('/system/maintenance/toggle', { isMaintenanceMode });
    return response.data;
  }
};
