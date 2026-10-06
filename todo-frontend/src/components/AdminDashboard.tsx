import { useState } from 'react';
import { ListTodo, CheckCircle, Clock, GripHorizontal } from 'lucide-react';
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip as RechartsTooltip, Legend, BarChart, Bar, XAxis, YAxis, CartesianGrid } from 'recharts';
import {
  DndContext,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
} from '@dnd-kit/core';
import type { DragEndEvent } from '@dnd-kit/core';
import {
  arrayMove,
  SortableContext,
  sortableKeyboardCoordinates,
  useSortable,
  rectSortingStrategy,
} from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';

interface AdminDashboardProps {
  statsData: any;
}

interface SortableWidgetProps {
  id: string;
  children: React.ReactNode;
}

function SortableWidget({ id, children }: SortableWidgetProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : 1,
    position: 'relative' as const,
    zIndex: isDragging ? 100 : 1,
  };

  return (
    <div ref={setNodeRef} className="glass-card" style={{ ...style, padding: '1.5rem', display: 'flex', flexDirection: 'column', height: '100%' }}>
      <div 
        {...attributes} 
        {...listeners} 
        style={{ cursor: 'grab', display: 'flex', justifyContent: 'flex-end', marginBottom: '0.5rem', color: 'var(--color-text-muted)' }}
      >
        <GripHorizontal size={18} />
      </div>
      {children}
    </div>
  );
}

export default function AdminDashboard({ statsData }: AdminDashboardProps) {
  const [widgets, setWidgets] = useState([
    'totalTasks',
    'completedTasks',
    'pendingTasks',
    'taskStatus',
    'userPerformance'
  ]);

  const sensors = useSensors(
    useSensor(PointerSensor),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;

    if (over && active.id !== over.id) {
      setWidgets((items) => {
        const oldIndex = items.indexOf(active.id as string);
        const newIndex = items.indexOf(over.id as string);
        return arrayMove(items, oldIndex, newIndex);
      });
    }
  };

  const renderWidget = (id: string) => {
    switch (id) {
      case 'totalTasks':
        return (
          <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', flex: 1 }}>
            <div style={{ padding: '1rem', borderRadius: '50%', background: 'rgba(99, 102, 241, 0.1)', color: '#6366f1' }}>
              <ListTodo size={24} />
            </div>
            <div>
              <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>إجمالي المهام</p>
              <h3 style={{ fontSize: '1.5rem', fontWeight: 700, margin: 0 }}>{statsData.totalTasks}</h3>
            </div>
          </div>
        );
      case 'completedTasks':
        return (
          <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', flex: 1 }}>
            <div style={{ padding: '1rem', borderRadius: '50%', background: 'rgba(34, 197, 94, 0.1)', color: '#22c55e' }}>
              <CheckCircle size={24} />
            </div>
            <div>
              <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>المهام المنجزة</p>
              <h3 style={{ fontSize: '1.5rem', fontWeight: 700, margin: 0 }}>{statsData.completedTasks}</h3>
            </div>
          </div>
        );
      case 'pendingTasks':
        return (
          <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', flex: 1 }}>
            <div style={{ padding: '1rem', borderRadius: '50%', background: 'rgba(245, 158, 11, 0.1)', color: '#f59e0b' }}>
              <Clock size={24} />
            </div>
            <div>
              <p style={{ color: 'var(--color-text-secondary)', fontSize: '0.875rem' }}>قيد الانتظار</p>
              <h3 style={{ fontSize: '1.5rem', fontWeight: 700, margin: 0 }}>{statsData.pendingTasks}</h3>
            </div>
          </div>
        );
      case 'taskStatus':
        return (
          <div style={{ flex: 1 }}>
            <h3 style={{ fontSize: '1.1rem', marginBottom: '1rem' }}>حالة المهام</h3>
            <div style={{ height: 300 }}>
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={[
                      { name: 'منجزة', value: statsData.completedTasks },
                      { name: 'قيد الانتظار', value: statsData.pendingTasks }
                    ]}
                    cx="50%" cy="50%" innerRadius={60} outerRadius={80} paddingAngle={5} dataKey="value"
                  >
                    <Cell fill="#22c55e" />
                    <Cell fill="#f59e0b" />
                  </Pie>
                  <RechartsTooltip contentStyle={{ borderRadius: '8px', border: 'none', background: 'var(--color-bg-elevated)', color: 'var(--color-text)' }} />
                  <Legend />
                </PieChart>
              </ResponsiveContainer>
            </div>
          </div>
        );
      case 'userPerformance':
        return (
          <div style={{ flex: 1 }}>
            <h3 style={{ fontSize: '1.1rem', marginBottom: '1rem' }}>أداء المستخدمين</h3>
            <div style={{ height: 300 }}>
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={statsData.employeeStats?.slice(0, 5) || []}>
                  <CartesianGrid strokeDasharray="3 3" stroke="var(--color-border)" vertical={false} />
                  <XAxis dataKey="fullName" stroke="var(--color-text-muted)" fontSize={12} tickLine={false} axisLine={false} />
                  <YAxis stroke="var(--color-text-muted)" fontSize={12} tickLine={false} axisLine={false} />
                  <RechartsTooltip contentStyle={{ borderRadius: '8px', border: 'none', background: 'var(--color-bg-elevated)', color: 'var(--color-text)' }} cursor={{fill: 'var(--color-border)', opacity: 0.4}} />
                  <Legend />
                  <Bar dataKey="completedTasks" name="منجزة" fill="#22c55e" radius={[4, 4, 0, 0]} />
                  <Bar dataKey="pendingTasks" name="قيد الانتظار" fill="#f59e0b" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </div>
        );
      default:
        return null;
    }
  };

  return (
    <DndContext 
      sensors={sensors}
      collisionDetection={closestCenter}
      onDragEnd={handleDragEnd}
    >
      <SortableContext 
        items={widgets}
        strategy={rectSortingStrategy}
      >
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '1.5rem', marginBottom: '2rem' }}>
          {widgets.map(id => (
            <SortableWidget key={id} id={id}>
              {renderWidget(id)}
            </SortableWidget>
          ))}
        </div>
      </SortableContext>
    </DndContext>
  );
}
