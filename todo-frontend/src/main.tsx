// src/main.tsx
import React from 'react';
import ReactDOM from 'react-dom/client';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from 'react-hot-toast';
import App from './App';
import './index.css';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5,    // 5 minutes
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <App />
      <Toaster
        position="bottom-left"
        reverseOrder={false}
        gutter={8}
        toastOptions={{
          duration: 3000,
          style: {
            background: 'var(--color-bg-secondary)',
            color: 'var(--color-text-primary)',
            border: '1px solid var(--color-border)',
            borderRadius: 'var(--radius-lg)',
            fontFamily: 'var(--font-arabic)',
            fontSize: '0.875rem',
            boxShadow: 'var(--shadow-lg)',
            direction: 'rtl',
          },
          success: {
            iconTheme: { primary: 'var(--color-success)', secondary: 'white' },
          },
          error: {
            iconTheme: { primary: 'var(--color-danger)', secondary: 'white' },
          },
        }}
      />
    </QueryClientProvider>
  </React.StrictMode>
);
