// src/services/todoService.ts
import api from '../lib/api';
import type { ApiResponse, Todo, CreateTodoDto, UpdateTodoDto, TodoQueryParams } from '../types';

export const todoService = {
  getAll: async (params?: TodoQueryParams) => {
    const res = await api.get<ApiResponse<Todo[]>>('/todos', { params });
    return res.data;
  },

  getById: async (id: number) => {
    const res = await api.get<ApiResponse<Todo>>(`/todos/${id}`);
    return res.data.data;
  },

  create: async (dto: CreateTodoDto) => {
    const res = await api.post<ApiResponse<Todo>>('/todos', dto);
    return res.data.data;
  },

  update: async (id: number, dto: UpdateTodoDto) => {
    const res = await api.put<ApiResponse<Todo>>(`/todos/${id}`, dto);
    return res.data.data;
  },

  delete: async (id: number) => {
    await api.delete(`/todos/${id}`);
  },

  toggleComplete: async (todo: Todo) => {
    const res = await api.put<ApiResponse<Todo>>(`/todos/${todo.id}`, {
      title: todo.title,
      description: todo.description,
      isCompleted: !todo.isCompleted,
    });
    return res.data.data;
  },
};
