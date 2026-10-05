// src/types/index.ts

export interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data: T;
  pagination?: PaginationMetadata;
}

export interface PaginationMetadata {
  pageNumber: number;
  pageSize: number;
  totalRecords: number;
  totalPages: number;
}

export interface ApiErrorResponse {
  isSuccess: false;
  message: string;
  errors?: Record<string, string[]>;
}

// ─── Auth ───
export interface LoginDto {
  userName: string;
  password: string;
}

export interface RegisterDto {
  fullName: string;
  userName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface AuthResult {
  isSuccess: boolean;
  message: string;
  token?: string;
  refreshToken?: string;
  expireDate?: string;
}

export interface CurrentUser {
  id: string;
  email: string;
  fullName: string;
  roles: string[];
  permissions: string[];
}

// ─── Todo ───
export interface Todo {
  id: number;
  title: string;
  description: string;
  isCompleted: boolean;
  assignedToUserId?: string;
  assignedToUserName?: string;
  createdDate: string;
  updatedDate?: string;
}

export interface CreateTodoDto {
  title: string;
  description?: string;
  isCompleted?: boolean;
  assignedToUserId?: string;
}

export interface UpdateTodoDto {
  title?: string;
  description?: string;
  isCompleted?: boolean;
  assignedToUserId?: string;
}

export interface TodoQueryParams {
  pageNumber?: number;
  pageSize?: number;
  search?: string;
  completed?: boolean;
}

// ─── Role ───
export interface Role {
  id: string;
  name: string;
  permissions?: string[];
}

// ─── User ───
export interface AppUser {
  id: string;
  fullName: string;
  email: string;
  isActive: boolean;
  createdDate: string;
  roles: string[];
}

// ─── Audit ───
export interface AuditLog {
  id: number;
  tableName: string;
  action: string;
  oldValues?: string;
  newValues?: string;
  userId: string;
  userName: string;
  timestamp: string;
}

// ─── Dashboard ───
export interface EmployeeStats {
  userId: string;
  fullName: string;
  totalTasks: number;
  completedTasks: number;
  pendingTasks: number;
}

export interface DashboardResponse {
  totalTasks: number;
  completedTasks: number;
  pendingTasks: number;
  employeeStats: EmployeeStats[];
}
