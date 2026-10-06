import api from './api';

export interface Role {
  id: string;
  name: string;
}

export interface RoleResponse {
  isSuccess: boolean;
  message: string;
  data: Role[];
  pagination: any;
}

export const roleService = {
  getRoles: async (): Promise<RoleResponse> => {
    const response = await api.get<RoleResponse>('/roles?PageNumber=1&PageSize=50');
    return response.data;
  }
};
