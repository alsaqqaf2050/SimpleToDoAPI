// src/pages/LoginPage.tsx
import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { motion } from 'framer-motion';
import { Mail, Lock, LogIn, Eye, EyeOff } from 'lucide-react';
import { authService } from '../services/authService';
import { useAuthStore } from '../store/authStore';

export default function LoginPage() {
  const navigate = useNavigate();
  const setAuth = useAuthStore((s) => s.setAuth);
  const [showPassword, setShowPassword] = useState(false);
  const [form, setForm] = useState({ userName: '', password: '' });

  const loginMutation = useMutation({
    mutationFn: () => authService.login(form),
    onSuccess: async (data) => {
      if (!data.isSuccess || !data.token) {
        toast.error(data.message || 'فشل تسجيل الدخول');
        return;
      }
      // Fetch user profile
      const { default: api } = await import('../lib/api');
      localStorage.setItem('access_token', data.token);
      localStorage.setItem('refresh_token', data.refreshToken ?? '');
      try {
        const meRes = await api.get('/auth/me');
        const user = meRes.data.data;
        setAuth(user, data.token, data.refreshToken ?? '');
        toast.success(`مرحباً ${user.fullName}! 👋`);
        navigate('/dashboard');
      } catch {
        toast.error('تعذر جلب بيانات المستخدم');
      }
    },
    onError: (error: { response?: { data?: { message?: string } } }) => {
      toast.error(error.response?.data?.message || 'بيانات الدخول غير صحيحة');
    },
  });

  return (
    <div className="auth-page">
      {/* Background orbs */}
      <div style={{ position: 'fixed', top: '20%', right: '15%', width: 300, height: 300, background: 'radial-gradient(circle, rgba(99,102,241,0.15) 0%, transparent 70%)', borderRadius: '50%', pointerEvents: 'none' }} />
      <div style={{ position: 'fixed', bottom: '20%', left: '15%', width: 250, height: 250, background: 'radial-gradient(circle, rgba(6,182,212,0.1) 0%, transparent 70%)', borderRadius: '50%', pointerEvents: 'none' }} />

      <motion.div
        initial={{ opacity: 0, y: 24 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5, ease: [0.34, 1.56, 0.64, 1] }}
        className="auth-card"
      >
        <div className="auth-logo">
          <div style={{ fontSize: '3rem', marginBottom: '0.5rem' }}>✅</div>
          <h1>مرحباً بك</h1>
          <p>سجّل دخولك لإدارة مهامك</p>
        </div>

        <form
          onSubmit={(e) => { e.preventDefault(); loginMutation.mutate(); }}
          style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}
        >
          {/* Username / Email */}
          <div className="form-group">
            <label className="form-label">البريد الإلكتروني أو اسم المستخدم</label>
            <div style={{ position: 'relative' }}>
              <input
                type="text"
                className="form-input"
                placeholder="admin@todo.com أو admin"
                value={form.userName}
                onChange={(e) => setForm({ ...form, userName: e.target.value })}
                style={{ paddingRight: '2.75rem' }}
                required
              />
              <Mail size={16} style={{ position: 'absolute', right: '0.875rem', top: '50%', transform: 'translateY(-50%)', color: 'var(--color-text-muted)' }} />
            </div>
          </div>

          {/* Password */}
          <div className="form-group">
            <label className="form-label">كلمة المرور</label>
            <div style={{ position: 'relative' }}>
              <input
                type={showPassword ? 'text' : 'password'}
                className="form-input"
                placeholder="••••••••"
                value={form.password}
                onChange={(e) => setForm({ ...form, password: e.target.value })}
                style={{ paddingRight: '2.75rem', paddingLeft: '2.75rem' }}
                required
              />
              <Lock size={16} style={{ position: 'absolute', right: '0.875rem', top: '50%', transform: 'translateY(-50%)', color: 'var(--color-text-muted)' }} />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                style={{ position: 'absolute', left: '0.875rem', top: '50%', transform: 'translateY(-50%)', background: 'none', border: 'none', color: 'var(--color-text-muted)', cursor: 'pointer' }}
              >
                {showPassword ? <EyeOff size={16} /> : <Eye size={16} />}
              </button>
            </div>
          </div>

          {/* Hint */}
          <div style={{ padding: '0.75rem', borderRadius: 'var(--radius-md)', background: 'rgba(99,102,241,0.08)', border: '1px solid rgba(99,102,241,0.2)', fontSize: '0.8rem', color: 'var(--color-text-secondary)' }}>
            💡 للتجربة: <strong style={{ color: 'var(--color-primary-light)' }}>admin@todo.com</strong> / <strong style={{ color: 'var(--color-primary-light)' }}>Admin@123</strong>
          </div>

          {/* Submit */}
          <motion.button
            type="submit"
            className="btn btn-primary btn-lg"
            disabled={loginMutation.isPending}
            whileTap={{ scale: 0.97 }}
            style={{ width: '100%', marginTop: '0.5rem' }}
          >
            {loginMutation.isPending ? (
              <><span className="spinner" style={{ width: 18, height: 18 }} /> جارٍ الدخول...</>
            ) : (
              <><LogIn size={18} /> تسجيل الدخول</>
            )}
          </motion.button>
        </form>

        <p style={{ textAlign: 'center', marginTop: '1.5rem', fontSize: '0.875rem', color: 'var(--color-text-secondary)' }}>
          ليس لديك حساب؟{' '}
          <Link to="/register" style={{ color: 'var(--color-primary-light)', fontWeight: 600 }}>
            إنشاء حساب جديد
          </Link>
        </p>
      </motion.div>
    </div>
  );
}
