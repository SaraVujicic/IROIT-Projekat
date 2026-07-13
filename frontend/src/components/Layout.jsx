import React from 'react';
import { Outlet, Navigate, Link, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Users, LayoutDashboard, Calendar, FileText, Briefcase, LogOut, Plane, BookOpen, Clock } from 'lucide-react';
import './Layout.css';

const Layout = () => {
  const { user, logout } = useAuth();
  const location = useLocation();

  if (!user) {
    return <Navigate to="/login" />;
  }

  const navItems = [
    { name: 'Dashboard', path: '/', icon: <LayoutDashboard size={20} />, roles: ['Admin', 'Manager', 'Employee'] },
    { name: 'Employees', path: '/employees', icon: <Users size={20} />, roles: ['Admin'] },
    { name: 'Requests', path: '/requests', icon: <FileText size={20} />, roles: ['Admin', 'Manager', 'Employee'] },
    { name: 'Absences', path: '/absences', icon: <Clock size={20} />, roles: ['Employee'] },
    { name: 'Travel Orders', path: '/travel', icon: <Plane size={20} />, roles: ['Employee'] },
    { name: 'Internship & Education', path: '/internships', icon: <BookOpen size={20} />, roles: ['Admin', 'Employee'] },
    { name: 'Calendar', path: '/calendar', icon: <Calendar size={20} />, roles: ['Employee'] },
  ];

  const allowedItems = navItems.filter(item => item.roles.includes(user?.role || 'Employee'));

  return (
    <div className="app-container">
      {/* Sidebar Navigation */}
      <aside className="sidebar glass">
        <div className="sidebar-header">
          <Briefcase className="brand-icon" size={28} />
          <h2>EmpManage</h2>
        </div>
        
        <nav className="sidebar-nav">
          {allowedItems.map((item) => (
            <Link 
              key={item.path} 
              to={item.path}
              className={`nav-item ${location.pathname === item.path ? 'active' : ''}`}
            >
              {item.icon}
              <span>{item.name}</span>
            </Link>
          ))}
        </nav>

        <div className="sidebar-footer">
          <div className="user-profile">
            <div className="avatar">{(user?.name || 'User').charAt(0)}</div>
            <div className="user-info">
              <span className="user-name">{user?.name || 'User'}</span>
              <span className="user-role">{user?.role || 'Employee'}</span>
            </div>
          </div>
          <button className="logout-btn" onClick={logout}>
            <LogOut size={18} />
            <span>Logout</span>
          </button>
        </div>
      </aside>

      {/* Main Content Area */}
      <main className="main-content">
        <Outlet />
      </main>
    </div>
  );
};

export default Layout;
