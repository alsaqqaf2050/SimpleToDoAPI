// src/pages/RegisterPage.tsx
import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { motion } from 'framer-motion';
import { Mail, Lock, User, UserPlus, Eye, EyeOff } from 'lucide-react';
import { authService } from '../services/authService';

export default function RegisterPage() {
  const navigate = useNavigate();
  const [showPassword, setShowPassword] = useState(false);
  const [form, setForm] = useState({
    fullName: '',
    email: '',
    password: '',
    confirmPassword: '',
  });
  const [errors, setErrors] = useState<Record<string, string>>({});

  const validate = () => {
    const errs: Record<string, string> = {};
    if (!form.fullName.trim()) errs.fullName = 'الاسم مطلوب';
    if (!form.email.trim()) errs.email = 'البريد مطلوب';
    if (form.password.length < 6) errs.password = 'كلمة المرور 6 أحرف على الأقل';
    if (form.password !== form.confirmPassword) errs.confirmPassword = 'كلمتا المرور غير متطابقتين';
    setErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const registerMutation = useMutation({
    mutationFn: () => authService.register({ ...form, userName: form.email.split('@')[0] }),
    onSuccess: (data) => {
      if (!data.isSuccess) {
        toast.error(data.message || 'فشل إنشاء الحساب');
        return;
      }
      toast.success('تم إنشاء الحساب بنجاح! يمكنك تسجيل الدخول الآن');
      navigate('/login');
    },
    onError: (error: { response?: { data?: { message?: string } } }) => {
      toast.error(error.response?.data?.message || 'فشل إنشاء الحساب');
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (validate()) registerMutation.mutate();
  };

  return (
    <div className="auth-page">
      <div style={{ position: 'fixed', top: '15%', left: '10%', width: 300, height: 300, background: 'radial-gradient(circle, rgba(99,102,241,0.12) 0%, transparent 70%)', borderRadius: '50%', pointerEvents: 'none' }} />

      <motion.div
        initial={{ opacity: 0, y: 24 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5, ease: [0.34, 1.56, 0.64, 1] }}
        className="auth-card"
      >
        <div className="auth-logo">
          <div style={{ fontSize: '2.5rem', marginBottom: '0.5rem' }}>🚀</div>
          <h1>إنشاء حساب جديد</h1>
          <p>انضم وابدأ تنظيم مهامك اليوم</p>
        </div>

        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          {/* Full Name */}
          <div className="form-group">
            <label className="form-label">الاسم الكامل</label>
            <div style={{ position: 'relative' }}>
              <input type="text" className={`form-input ${errors.fullName ? 'error' : ''}`}
                placeholder="محمد أحمد"
                value={form.fullName}
                onChange={(e) => setForm({ ...form, fullName: e.target.value })}
                style={{ paddingRight: '2.75rem' }}
              />
              <User size={16} style={{ position: 'absolute', right: '0.875rem', top: '50%', transform: 'translateY(-50%)', color: 'var(--color-text-muted)' }} />
            </div>
            {errors.fullName && <span className="form-error">⚠ {errors.fullName}</span>}
          </div>

          {/* Email */}
          <div className="form-group">
            <label className="form-label">البريد الإلكتروني</label>
            <div style={{ position: 'relative' }}>
              <input type="email" className={`form-input ${errors.email ? 'error' : ''}`}
                placeholder="user@example.com"
                value={form.email}
                onChange={(e) => setForm({ ...form, email: e.target.value })}
                style={{ paddingRight: '2.75rem' }}
              />
              <Mail size={16} style={{ position: 'absolute', right: '0.875rem', top: '50%', transform: 'translateY(-50%)', color: 'var(--color-text-muted)' }} />
            </div>
            {errors.email && <span className="form-error">⚠ {errors.email}</span>}
          </div>

          {/* Password */}
          <div className="form-group">
            <label className="form-label">كلمة المرور</label>
            <div style={{ position: 'relative' }}>
              <input type={showPassword ? 'text' : 'password'} className={`form-input ${errors.password ? 'error' : ''}`}
                placeholder="••••••••"
                value={form.password}
                onChange={(e) => setForm({ ...form, password: e.target.value })}
                style={{ paddingRight: '2.75rem', paddingLeft: '2.75rem' }}
              />
              <Lock size={16} style={{ position: 'absolute', right: '0.875rem', top: '50%', transform: 'translateY(-50%)', color: 'var(--color-text-muted)' }} />
              <button type="button" onClick={() => setShowPassword(!showPassword)}
                style={{ position: 'absolute', left: '0.875rem', top: '50%', transform: 'translateY(-50%)', background: 'none', border: 'none', color: 'var(--color-text-muted)', cursor: 'pointer' }}>
                {showPassword ? <EyeOff size={16} /> : <Eye size={16} />}
              </button>
            </div>
            {errors.password && <span className="form-error">⚠ {errors.password}</span>}
          </div>

          {/* Confirm Password */}
          <div className="form-group">
            <label className="form-label">تأكيد كلمة المرور</label>
            <div style={{ position: 'relative' }}>
              <input type={showPassword ? 'text' : 'password'} className={`form-input ${errors.confirmPassword ? 'error' : ''}`}
                placeholder="••••••••"
                value={form.confirmPassword}
                onChange={(e) => setForm({ ...form, confirmPassword: e.target.value })}
                style={{ paddingRight: '2.75rem' }}
              />
              <Lock size={16} style={{ position: 'absolute', right: '0.875rem', top: '50%', transform: 'translateY(-50%)', color: 'var(--color-text-muted)' }} />
            </div>
            {errors.confirmPassword && <span className="form-error">⚠ {errors.confirmPassword}</span>}
          </div>

          <motion.button type="submit" className="btn btn-primary btn-lg"
            disabled={registerMutation.isPending}
            whileTap={{ scale: 0.97 }}
            style={{ width: '100%', marginTop: '0.5rem' }}>
            {registerMutation.isPending
              ? <><span className="spinner" style={{ width: 18, height: 18 }} /> جارٍ الإنشاء...</>
              : <><UserPlus size={18} /> إنشاء الحساب</>
            }
          </motion.button>
        </form>

        <p style={{ textAlign: 'center', marginTop: '1.5rem', fontSize: '0.875rem', color: 'var(--color-text-secondary)' }}>
          لديك حساب بالفعل؟{' '}
          <Link to="/login" style={{ color: 'var(--color-primary-light)', fontWeight: 600 }}>
            تسجيل الدخول
          </Link>
        </p>
      </motion.div>
    </div>
  );
}
