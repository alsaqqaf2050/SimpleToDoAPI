// src/pages/TodosPage.tsx
import { useState, useCallback } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { motion, AnimatePresence } from 'framer-motion';
import toast from 'react-hot-toast';
import { Plus, Search, Trash2, Edit3, CheckCircle2, Circle, X, SlidersHorizontal } from 'lucide-react';
import { todoService } from '../services/todoService';
import { userService } from '../services/userService';
import type { Todo, CreateTodoDto, UpdateTodoDto } from '../types';

// ─── Todo Modal ───────────────────────────────────────────────
interface TodoModalProps {
  todo?: Todo;
  onClose: () => void;
}

function TodoModal({ todo, onClose }: TodoModalProps) {
  const queryClient = useQueryClient();
  const isEdit = !!todo;
  const [form, setForm] = useState({
    title: todo?.title ?? '',
    description: todo?.description ?? '',
    isCompleted: todo?.isCompleted ?? false,
    assignedToUserId: todo?.assignedToUserId ?? '',
  });

  const { data: usersData } = useQuery({
    queryKey: ['users', 'all'],
    queryFn: () => userService.getAll(1, 100).catch(() => null),
  });
  const users = usersData?.data ?? [];

  const createMutation = useMutation({
    mutationFn: (dto: CreateTodoDto) => todoService.create(dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['todos'] });
      toast.success('تم إنشاء المهمة بنجاح ✨');
      onClose();
    },
    onError: () => toast.error('فشل في إنشاء المهمة'),
  });

  const updateMutation = useMutation({
    mutationFn: (dto: UpdateTodoDto) => todoService.update(todo!.id, dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['todos'] });
      toast.success('تم تحديث المهمة');
      onClose();
    },
    onError: () => toast.error('فشل في تحديث المهمة'),
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.title.trim()) { toast.error('عنوان المهمة مطلوب'); return; }
    const payload = { ...form, assignedToUserId: form.assignedToUserId || undefined };
    if (isEdit) updateMutation.mutate(payload);
    else createMutation.mutate(payload);
  };

  const isPending = createMutation.isPending || updateMutation.isPending;

  return (
    <div className="modal-overlay" onClick={(e) => e.target === e.currentTarget && onClose()}>
      <div className="modal-content">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
          <h2 style={{ fontWeight: 700, fontSize: '1.2rem' }}>
            {isEdit ? '✏️ تعديل المهمة' : '✨ مهمة جديدة'}
          </h2>
          <button className="icon-btn" onClick={onClose}><X size={18} /></button>
        </div>

        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          <div className="form-group">
            <label className="form-label">العنوان *</label>
            <input
              type="text"
              className="form-input"
              placeholder="ما الذي تريد إنجازه؟"
              value={form.title}
              onChange={(e) => setForm({ ...form, title: e.target.value })}
              autoFocus
              maxLength={100}
            />
            <span style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)', textAlign: 'left' }}>
              {form.title.length}/100
            </span>
          </div>

          <div className="form-group">
            <label className="form-label">الوصف (اختياري)</label>
            <textarea
              className="form-textarea"
              placeholder="تفاصيل إضافية..."
              value={form.description}
              onChange={(e) => setForm({ ...form, description: e.target.value })}
              rows={3}
              maxLength={500}
            />
          </div>

          {users.length > 0 && (
            <div className="form-group">
              <label className="form-label">إسناد إلى موظف (اختياري)</label>
              <select
                className="form-input"
                value={form.assignedToUserId}
                onChange={(e) => setForm({ ...form, assignedToUserId: e.target.value })}
              >
                <option value="">-- أنا --</option>
                {users.map(u => (
                  <option key={u.id} value={u.id}>{u.fullName || u.email}</option>
                ))}
              </select>
            </div>
          )}

          {isEdit && (
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', padding: '0.75rem', borderRadius: 'var(--radius-md)', background: 'var(--color-bg-input)', border: '1px solid var(--color-border)', cursor: 'pointer' }}
              onClick={() => setForm({ ...form, isCompleted: !form.isCompleted })}>
              <div className={`custom-checkbox ${form.isCompleted ? 'checked' : ''}`}>
                {form.isCompleted && <span style={{ color: 'white', fontSize: '0.7rem' }}>✓</span>}
              </div>
              <span style={{ fontSize: '0.9rem' }}>تمييز كمنجزة</span>
            </div>
          )}

          <div style={{ display: 'flex', gap: '0.75rem', marginTop: '0.5rem' }}>
            <button type="button" className="btn btn-ghost" onClick={onClose} style={{ flex: 1 }}>
              إلغاء
            </button>
            <motion.button type="submit" className="btn btn-primary" disabled={isPending} whileTap={{ scale: 0.97 }} style={{ flex: 2 }}>
              {isPending
                ? <><span className="spinner" style={{ width: 16, height: 16 }} /> جارٍ الحفظ...</>
                : isEdit ? '💾 حفظ التغييرات' : '✨ إنشاء المهمة'
              }
            </motion.button>
          </div>
        </form>
      </div>
    </div>
  );
}

// ─── Main Todos Page ──────────────────────────────────────────
export default function TodosPage() {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const [filter, setFilter] = useState<'all' | 'pending' | 'completed'>('all');
  const [page, setPage] = useState(1);
  const [showModal, setShowModal] = useState(false);
  const [editTodo, setEditTodo] = useState<Todo | undefined>(undefined);

  const completed = filter === 'completed' ? true : filter === 'pending' ? false : undefined;

  const { data, isLoading } = useQuery({
    queryKey: ['todos', { search, completed, page }],
    queryFn: () => todoService.getAll({
      search: search || undefined,
      completed,
      pageNumber: page,
      pageSize: 10,
    }),
    placeholderData: (prev) => prev,
  });

  const todos = data?.data ?? [];
  const pagination = data?.pagination;

  const toggleMutation = useMutation({
    mutationFn: (todo: Todo) => todoService.toggleComplete(todo),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['todos'] }),
    onError: () => toast.error('فشل في تحديث الحالة'),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => todoService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['todos'] });
      toast.success('تم حذف المهمة');
    },
    onError: () => toast.error('فشل في حذف المهمة'),
  });

  const handleDelete = useCallback((todo: Todo) => {
    if (confirm(`هل أنت متأكد من حذف "${todo.title}"؟`)) {
      deleteMutation.mutate(todo.id);
    }
  }, [deleteMutation]);

  const handleEdit = (todo: Todo) => {
    setEditTodo(todo);
    setShowModal(true);
  };

  const closeModal = () => {
    setShowModal(false);
    setEditTodo(undefined);
  };

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearch(e.target.value);
    setPage(1);
  };

  return (
    <div className="fade-in">
      {/* Header */}
      <div className="page-header">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: '1rem' }}>
          <div>
            <h1>مهامي</h1>
            <p>
              {pagination
                ? `${pagination.totalRecords} مهمة إجمالاً`
                : 'قائمة مهامك'}
            </p>
          </div>
          <motion.button
            className="btn btn-primary"
            whileTap={{ scale: 0.95 }}
            onClick={() => { setEditTodo(undefined); setShowModal(true); }}
          >
            <Plus size={18} /> مهمة جديدة
          </motion.button>
        </div>
      </div>

      {/* Filters & Search */}
      <div style={{ display: 'flex', gap: '1rem', marginBottom: '1.5rem', flexWrap: 'wrap', alignItems: 'center' }}>
        {/* Search */}
        <div className="search-wrapper" style={{ flex: 1, minWidth: 200 }}>
          <Search size={16} className="search-icon" />
          <input
            type="text"
            className="search-input"
            placeholder="بحث في المهام..."
            value={search}
            onChange={handleSearchChange}
          />
        </div>

        {/* Filter Chips */}
        <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center' }}>
          <SlidersHorizontal size={16} style={{ color: 'var(--color-text-muted)' }} />
          {(['all', 'pending', 'completed'] as const).map((f) => (
            <span
              key={f}
              className={`chip ${filter === f ? 'active' : ''}`}
              onClick={() => { setFilter(f); setPage(1); }}
            >
              {f === 'all' ? 'الكل' : f === 'pending' ? '⏳ قيد التنفيذ' : '✅ منجزة'}
            </span>
          ))}
        </div>
      </div>

      {/* Todos List */}
      {isLoading ? (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          {[...Array(5)].map((_, i) => (
            <div key={i} className="skeleton" style={{ height: 80 }} />
          ))}
        </div>
      ) : todos.length === 0 ? (
        <motion.div
          className="empty-state glass-card"
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
        >
          <div className="empty-icon">{search ? '🔍' : '📋'}</div>
          <h3>{search ? 'لا توجد نتائج' : 'لا توجد مهام'}</h3>
          <p>{search ? `لا توجد مهام تطابق "${search}"` : 'ابدأ بإضافة مهمتك الأولى!'}</p>
          {!search && (
            <button className="btn btn-primary" onClick={() => setShowModal(true)} style={{ marginTop: '1rem' }}>
              <Plus size={16} /> إضافة مهمة
            </button>
          )}
        </motion.div>
      ) : (
        <AnimatePresence mode="popLayout">
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
            {todos.map((todo, i) => (
              <motion.div
                key={todo.id}
                className={`todo-item ${todo.isCompleted ? 'completed' : ''}`}
                initial={{ opacity: 0, y: 12 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0, x: -20, scale: 0.96 }}
                transition={{ delay: i * 0.05, duration: 0.3 }}
                layout
              >
                {/* Checkbox */}
                <div
                  className={`custom-checkbox ${todo.isCompleted ? 'checked' : ''}`}
                  onClick={() => toggleMutation.mutate(todo)}
                  style={{ cursor: 'pointer', flexShrink: 0 }}
                >
                  {todo.isCompleted && <span style={{ color: 'white', fontSize: '0.7rem' }}>✓</span>}
                </div>

                {/* Content */}
                <div style={{ flex: 1, minWidth: 0 }}>
                  <div className="todo-title">{todo.title}</div>
                  {todo.description && <div className="todo-desc">{todo.description}</div>}
                  {todo.assignedToUserName && (
                    <div style={{ fontSize: '0.8rem', color: 'var(--color-primary)', marginTop: '0.2rem', marginBottom: '0.2rem', display: 'flex', alignItems: 'center', gap: '0.25rem' }}>
                      👤 إسناد: {todo.assignedToUserName}
                    </div>
                  )}
                  <div className="todo-meta">
                    <span className={`badge ${todo.isCompleted ? 'badge-success' : 'badge-warning'}`}>
                      {todo.isCompleted
                        ? <><CheckCircle2 size={11} /> منجزة</>
                        : <><Circle size={11} /> قيد التنفيذ</>
                      }
                    </span>
                    <span style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>
                      {new Date(todo.createdDate).toLocaleDateString('ar-SA', {
                        year: 'numeric', month: 'short', day: 'numeric'
                      })}
                    </span>
                    {todo.updatedDate && (
                      <span style={{ fontSize: '0.7rem', color: 'var(--color-text-muted)' }}>
                        · عُدّلت {new Date(todo.updatedDate).toLocaleDateString('ar-SA')}
                      </span>
                    )}
                  </div>
                </div>

                {/* Actions */}
                <div className="todo-actions">
                  <button className="icon-btn success" onClick={() => toggleMutation.mutate(todo)} data-tooltip={todo.isCompleted ? 'إلغاء الإنجاز' : 'تمييز كمنجزة'}>
                    {todo.isCompleted ? <Circle size={16} /> : <CheckCircle2 size={16} />}
                  </button>
                  <button className="icon-btn" onClick={() => handleEdit(todo)} data-tooltip="تعديل">
                    <Edit3 size={16} />
                  </button>
                  <button className="icon-btn danger" onClick={() => handleDelete(todo)} data-tooltip="حذف"
                    disabled={deleteMutation.isPending}>
                    <Trash2 size={16} />
                  </button>
                </div>
              </motion.div>
            ))}
          </div>
        </AnimatePresence>
      )}

      {/* Pagination */}
      {pagination && pagination.totalPages > 1 && (
        <div style={{ display: 'flex', justifyContent: 'center', gap: '0.5rem', marginTop: '2rem', flexWrap: 'wrap' }}>
          <button className="btn btn-ghost btn-sm" onClick={() => setPage(p => p - 1)} disabled={page === 1}>
            السابق
          </button>
          {[...Array(pagination.totalPages)].map((_, i) => (
            <button
              key={i}
              className={`btn btn-sm ${page === i + 1 ? 'btn-primary' : 'btn-ghost'}`}
              onClick={() => setPage(i + 1)}
            >
              {i + 1}
            </button>
          ))}
          <button className="btn btn-ghost btn-sm" onClick={() => setPage(p => p + 1)} disabled={page === pagination.totalPages}>
            التالي
          </button>
        </div>
      )}

      {/* Modal */}
      <AnimatePresence>
        {showModal && (
          <TodoModal todo={editTodo} onClose={closeModal} />
        )}
      </AnimatePresence>
    </div>
  );
}
