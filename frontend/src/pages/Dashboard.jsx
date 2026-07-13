import React, { useEffect, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { FileText, Clock, Bell, ArrowRight, UserCheck, Folder, Check, X, AlertTriangle } from 'lucide-react';
import { Link } from 'react-router-dom';
import api from '../services/api';
import './Dashboard.css';

const Dashboard = () => {
  const { user } = useAuth();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const currentYear = new Date().getFullYear();
  const [selectedYear, setSelectedYear] = useState(currentYear);
  
  // Dashboard stats
  const [stats, setStats] = useState({
    primary: 0, // Employees (Admin) / Managed Pending (Manager) / Remaining Leave (Employee)
    secondary: 0, // Total Requests (Admin) / Total Managed (Manager) / Active Requests (Employee)
    notificationsCount: 0 // Unread Notifications (All)
  });

  // Data lists based on role
  const [allEmployees, setAllEmployees] = useState([]);
  const [pendingApprovals, setPendingApprovals] = useState([]);
  const [notifications, setNotifications] = useState([]);
  const [leaveBalance, setLeaveBalance] = useState(null);

  useEffect(() => {
    fetchDashboardData();
  }, [user, selectedYear]);

  const getDurationInDays = (startDateStr, endDateStr) => {
    const start = new Date(startDateStr);
    const end = new Date(endDateStr);
    start.setHours(0,0,0,0);
    end.setHours(0,0,0,0);
    const diffTime = end.getTime() - start.getTime();
    if (diffTime < 0) return 0;
    return Math.floor(diffTime / (1000 * 60 * 60 * 24)) + 1;
  };

  const fetchDashboardData = async () => {
    if (!user) return;
    try {
      setLoading(true);
      setError('');

      // 1. Fetch notifications for all roles
      const notifRes = await api.get(`/notifications/user/${user.id}`);
      const unreadNotifs = notifRes.data.filter(n => !n.readStatus);
      setNotifications(notifRes.data);

      if (user.role === 'Admin') {
        // Fetch Admin data
        const [empRes, reqRes, , balanceRes] = await Promise.all([
          api.get('/employees'),
          api.get('/requests'),
          api.get('/departments'),
          api.get(`/leavebalances/${user.id}/year/${selectedYear}`).catch(() => null)
        ]);
        
        setAllEmployees(empRes.data);
        const getEmpRole = (empId) => {
          const emp = empRes.data.find(e => e.id === empId);
          return emp ? emp.role : 'Employee';
        };
        const adminPending = reqRes.data.filter(r => 
          r.status === 'PendingAdminApproval' || 
          (r.status === 'Pending' && getEmpRole(r.employeeId) === 'Manager')
        );
        setPendingApprovals(adminPending);
        
        // Calculate dynamic used days
        const userApprovedLeaves = reqRes.data.filter(r => 
          r.employeeId === user.id &&
          r.status === 'Approved' && 
          (r.type === 1 || r.type === 'Leave') && 
          r.leaveType === 1 && 
          new Date(r.startDate).getFullYear() === selectedYear
        );
        let usedDays = 0;
        userApprovedLeaves.forEach(r => {
          usedDays += getDurationInDays(r.startDate, r.endDate);
        });

        const balanceData = balanceRes ? balanceRes.data : null;
        if (!balanceData) {
          try {
            const seedRes = await api.post('/leavebalances', {
              employeeId: user.id,
              year: selectedYear,
              totalDays: 20,
              carriedOverDays: 0,
              expirationDate: new Date(Date.UTC(selectedYear, 5, 30, 0, 0, 0)).toISOString()
            });
            const seeded = {
              ...seedRes.data,
              usedDays: usedDays,
              remainingDays: seedRes.data.totalDays + seedRes.data.carriedOverDays - usedDays
            };
            setLeaveBalance(seeded);
          } catch (seedErr) {
            console.error("Auto-seeding leave balance failed for admin", seedErr);
            setLeaveBalance(null);
          }
        } else {
          const updated = {
            ...balanceData,
            usedDays: usedDays,
            remainingDays: balanceData.totalDays + balanceData.carriedOverDays - usedDays
          };
          setLeaveBalance(updated);
        }
        
        setStats({
          primary: empRes.data.length,
          secondary: reqRes.data.length,
          notificationsCount: unreadNotifs.length
        });
      } 
      else if (user.role === 'Manager') {
        // Fetch Manager data
        const [empRes, reqRes, balanceRes] = await Promise.all([
          api.get('/employees'),
          api.get('/requests'),
          api.get(`/leavebalances/${user.id}/year/${selectedYear}`).catch(() => null)
        ]);

        const managerEmp = empRes.data.find(e => e.id === user.id);
        const managerDept = managerEmp?.department;

        const managedEmployees = empRes.data.filter(emp => {
          if (emp.id === user.id) return false;
          if (emp.role === 'Admin') return false;
          if (emp.role === 'Manager') return false;
          return (emp.department === managerDept || emp.managerId === user.id);
        });
        const managedEmpIds = managedEmployees.map(emp => emp.id);

        const managerPending = reqRes.data.filter(r => 
          r.status === 'Pending' && managedEmpIds.includes(r.employeeId)
        );
        
        setAllEmployees(empRes.data);
        setPendingApprovals(managerPending);

        // Calculate dynamic used days
        const userApprovedLeaves = reqRes.data.filter(r => 
          r.employeeId === user.id &&
          r.status === 'Approved' && 
          (r.type === 1 || r.type === 'Leave') && 
          r.leaveType === 1 && 
          new Date(r.startDate).getFullYear() === selectedYear
        );
        let usedDays = 0;
        userApprovedLeaves.forEach(r => {
          usedDays += getDurationInDays(r.startDate, r.endDate);
        });

        const balanceData = balanceRes ? balanceRes.data : null;
        if (!balanceData) {
          try {
            const seedRes = await api.post('/leavebalances', {
              employeeId: user.id,
              year: selectedYear,
              totalDays: 20,
              carriedOverDays: 0,
              expirationDate: new Date(Date.UTC(selectedYear, 5, 30, 0, 0, 0)).toISOString()
            });
            const seeded = {
              ...seedRes.data,
              usedDays: usedDays,
              remainingDays: seedRes.data.totalDays + seedRes.data.carriedOverDays - usedDays
            };
            setLeaveBalance(seeded);
          } catch (seedErr) {
            console.error("Auto-seeding leave balance failed for manager", seedErr);
            setLeaveBalance(null);
          }
        } else {
          const updated = {
            ...balanceData,
            usedDays: usedDays,
            remainingDays: balanceData.totalDays + balanceData.carriedOverDays - usedDays
          };
          setLeaveBalance(updated);
        }

        setStats({
          primary: managerPending.length,
          secondary: managedEmployees.length,
          notificationsCount: unreadNotifs.length
        });
      } 
      else {
        // Fetch Employee data
        const [reqRes, balanceRes] = await Promise.all([
          api.get(`/requests/employee/${user.id}`),
          api.get(`/leavebalances/${user.id}/year/${selectedYear}`).catch(() => null)
        ]);

        const activeRequests = reqRes.data.filter(r => r.status === 'Pending' || r.status === 'PendingAdminApproval');
        
        // Calculate dynamic used days
        const userApprovedLeaves = reqRes.data.filter(r => 
          r.employeeId === user.id &&
          r.status === 'Approved' && 
          (r.type === 1 || r.type === 'Leave') && 
          r.leaveType === 1 && 
          new Date(r.startDate).getFullYear() === selectedYear
        );
        let usedDays = 0;
        userApprovedLeaves.forEach(r => {
          usedDays += getDurationInDays(r.startDate, r.endDate);
        });

        const balanceData = balanceRes ? balanceRes.data : null;
        if (!balanceData) {
          try {
            const seedRes = await api.post('/leavebalances', {
              employeeId: user.id,
              year: selectedYear,
              totalDays: 20,
              carriedOverDays: 0,
              expirationDate: new Date(Date.UTC(selectedYear, 5, 30, 0, 0, 0)).toISOString()
            });
            const seeded = {
              ...seedRes.data,
              usedDays: usedDays,
              remainingDays: seedRes.data.totalDays + seedRes.data.carriedOverDays - usedDays
            };
            setLeaveBalance(seeded);
            setStats({
              primary: seeded.remainingDays,
              secondary: activeRequests.length,
              notificationsCount: unreadNotifs.length
            });
          } catch (seedErr) {
            console.error("Auto-seeding leave balance failed for employee", seedErr);
            setLeaveBalance(null);
            setStats({
              primary: 20 - usedDays,
              secondary: activeRequests.length,
              notificationsCount: unreadNotifs.length
            });
          }
        } else {
          const updated = {
            ...balanceData,
            usedDays: usedDays,
            remainingDays: balanceData.totalDays + balanceData.carriedOverDays - usedDays
          };
          setLeaveBalance(updated);
          setStats({
            primary: updated.remainingDays,
            secondary: activeRequests.length,
            notificationsCount: unreadNotifs.length
          });
        }
      }
    } catch (err) {
      console.error("Dashboard fetch failed", err);
      setError("Failed to synchronize dashboard with C# services.");
    } finally {
      setLoading(false);
    }
  };

  const handleApprove = async (requestId) => {
    try {
      // API expects X-Approver-Role header to route approval logic
      await api.put(`/requests/${requestId}/approve`, null, {
        headers: { 'X-Approver-Role': user.role }
      });
      
      // Auto-trigger a success notification on the backend for the employee
      const req = pendingApprovals.find(r => r.id === requestId);
      if (req) {
        await api.post('/notifications', {
          recipientId: req.employeeId,
          message: `Your request #${requestId} (${req.description}) has been approved by ${user.role}.`
        });
      }

      const getTypeName = (t) => {
        if (t === 1 || t === 'Leave') return 'Leave';
        if (t === 2 || t === 'TravelOrder') return 'Travel Order';
        if (t === 3 || t === 'Internship') return 'Internship';
        return 'Education';
      };

      if (user.role === 'Manager') {
        const admin = allEmployees.find(e => e.role === 'Admin');
        if (admin && req) {
          await api.post('/notifications', {
            recipientId: admin.id,
            message: `Manager has approved the request for ${getTypeName(req.type)} (ID #${requestId}) for employee ${getEmployeeName(req.employeeId)}.`
          });
        }
      }

      fetchDashboardData();
    } catch (err) {
      console.error("Approval failed", err);
      alert("Failed to process approval.");
    }
  };

  const handleReject = async (requestId) => {
    try {
      await api.put(`/requests/${requestId}/reject`);
      
      const req = pendingApprovals.find(r => r.id === requestId);
      if (req) {
        await api.post('/notifications', {
          recipientId: req.employeeId,
          message: `Your request #${requestId} (${req.description}) has been rejected.`
        });
      }

      fetchDashboardData();
    } catch (err) {
      console.error("Rejection failed", err);
      alert("Failed to process rejection.");
    }
  };

  const handleMarkAsRead = async (notifId) => {
    try {
      await api.patch(`/notifications/${notifId}/read`);
      fetchDashboardData();
    } catch (err) {
      console.error("Mark read failed", err);
    }
  };

  const handleDeleteNotification = async (notifId) => {
    try {
      await api.delete(`/notifications/${notifId}`);
      fetchDashboardData();
    } catch (err) {
      console.error("Delete failed", err);
    }
  };

  // Helper to get employee name from ID
  const getEmployeeName = (empId) => {
    const emp = allEmployees.find(e => e.id === empId);
    return emp ? `${emp.firstName} ${emp.lastName}` : `Employee #${empId}`;
  };

  // Expiration check logic for June 30th
  const isCarriedOverExpired = () => {
    if (!leaveBalance) return false;
    const expirationDate = new Date(leaveBalance.expirationDate);
    return new Date() > expirationDate;
  };

  return (
    <div className="dashboard-container">
      <header className="page-header">
        <div>
          <h1>Welcome, {user?.name}</h1>
          <p className="subtitle">Role: <strong className="text-primary">{user?.role}</strong> | Real-time microservice interface.</p>
        </div>
      </header>

      {error && <div className="error-banner flex-align-center gap-sm"><AlertTriangle className="text-danger" /><span>{error}</span></div>}

      {loading ? (
        <div className="loader">Synchronizing with API Gateway...</div>
      ) : (
        <>
          <div className="stats-grid">
            <div className="stat-card glass notification-card">
              <div className="stat-icon-wrapper notification-icon">
                <Bell size={24} />
              </div>
              <div className="stat-info">
                <span className="stat-value">{stats.notificationsCount}</span>
                <span className="stat-label">Unread Notifications</span>
              </div>
            </div>

            {user.role === 'Admin' && (
              <>
                <div className="stat-card glass employee-card-stat">
                  <div className="stat-icon-wrapper employee-icon-stat">
                    <UserCheck size={24} />
                  </div>
                  <div className="stat-info">
                    <span className="stat-value">{stats.primary}</span>
                    <span className="stat-label">Total Employees</span>
                  </div>
                </div>
                <div className="stat-card glass request-card">
                  <div className="stat-icon-wrapper request-icon">
                    <FileText size={24} />
                  </div>
                  <div className="stat-info">
                    <span className="stat-value">{stats.secondary}</span>
                    <span className="stat-label">Total Requests</span>
                  </div>
                </div>
              </>
            )}

            {user.role === 'Manager' && (
              <>
                <div className="stat-card glass request-card">
                  <div className="stat-icon-wrapper request-icon">
                    <FileText size={24} />
                  </div>
                  <div className="stat-info">
                    <span className="stat-value">{stats.primary}</span>
                    <span className="stat-label">Pending Your Approval</span>
                  </div>
                </div>
                <div className="stat-card glass employee-card-stat">
                  <div className="stat-icon-wrapper employee-icon-stat">
                    <UserCheck size={24} />
                  </div>
                  <div className="stat-info">
                    <span className="stat-value">{stats.secondary}</span>
                    <span className="stat-label">Employees Managed</span>
                  </div>
                </div>
              </>
            )}

            {user.role === 'Employee' && (
              <>
                <div className="stat-card glass leave-card-stat">
                  <div className="stat-icon-wrapper leave-icon-stat">
                    <Clock size={24} />
                  </div>
                  <div className="stat-info">
                    <span className="stat-value">{stats.primary}</span>
                    <span className="stat-label">Available Leave Days</span>
                  </div>
                </div>
                <div className="stat-card glass request-card">
                  <div className="stat-icon-wrapper request-icon">
                    <FileText size={24} />
                  </div>
                  <div className="stat-info">
                    <span className="stat-value">{stats.secondary}</span>
                    <span className="stat-label">Active Pending Requests</span>
                  </div>
                </div>
              </>
            )}
          </div>

          <div className="dashboard-grid-content">
            {/* Left Column: Approvals or personal Leave Balance */}
            <div className="dashboard-main-left">
              {(user.role === 'Admin' || user.role === 'Manager') && (
                <div className="recent-activity glass">
                  <div className="section-header">
                    <h3>Pending Request Approvals ({pendingApprovals.length})</h3>
                  </div>
                  
                  {pendingApprovals.length === 0 ? (
                    <div className="empty-state">No requests awaiting your approval.</div>
                  ) : (
                    <div className="approval-list">
                      {pendingApprovals.map(req => (
                        <div key={req.id} className="approval-item glass">
                          <div className="approval-info">
                            <span className="employee-name">{getEmployeeName(req.employeeId)}</span>
                            <p className="request-desc">
                              <strong>{req.type === 1 ? 'Leave' : req.type === 2 ? 'Travel Order' : req.type === 3 ? 'Internship' : 'Education'}:</strong> {req.description}
                            </p>
                            <span className="request-dates">
                              {new Date(req.startDate).toLocaleDateString()} - {new Date(req.endDate).toLocaleDateString()}
                            </span>
                            {req.type === 2 && req.travelOrder && (
                              <div className="sub-detail">Dest: {req.travelOrder.destination} | Cost: ${req.travelOrder.costs}</div>
                            )}
                          </div>
                          <div className="approval-actions">
                            <button className="btn-approve btn-icon-only" onClick={() => handleApprove(req.id)} title="Approve">
                              <Check size={18} />
                            </button>
                            <button className="btn-reject btn-icon-only" onClick={() => handleReject(req.id)} title="Reject">
                              <X size={18} />
                            </button>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              )}

              {/* Universal Leave Balance Details Card (User Story 13) */}
              <div className={`leave-balance-container glass ${user.role !== 'Employee' ? 'mt-lg' : ''}`}>
                <div className="section-header flex-header">
                  <h3>Your Leave Balance Details</h3>
                  <div className="year-selector">
                    <label htmlFor="year-select" style={{marginRight: '0.5rem', fontSize: '0.9rem', color: 'var(--text-muted)'}}>Year: </label>
                    <select 
                      id="year-select" 
                      className="form-input year-select-dropdown" 
                      value={selectedYear} 
                      onChange={e => setSelectedYear(parseInt(e.target.value))}
                      style={{width: '90px', display: 'inline-block', padding: '0.25rem', height: 'auto'}}
                    >
                      <option value={currentYear - 1}>{currentYear - 1}</option>
                      <option value={currentYear}>{currentYear}</option>
                      <option value={currentYear + 1}>{currentYear + 1}</option>
                    </select>
                  </div>
                </div>
                {leaveBalance ? (
                  <div className="leave-details-grid">
                    <div className="balance-item">
                      <span className="label">Total Annual Leave Days</span>
                      <span className="value">{leaveBalance.totalDays} days</span>
                    </div>
                    <div className="balance-item">
                      <span className="label">Carried Over (Previous Year)</span>
                      <span className={`value ${isCarriedOverExpired() ? 'text-muted line-through' : 'text-primary'}`}>
                        {leaveBalance.carriedOverDays} days
                      </span>
                    </div>
                    <div className="balance-item">
                      <span className="label">Used Days</span>
                      <span className="value text-warning">{leaveBalance.usedDays} days</span>
                    </div>
                    <div className="balance-item highlight">
                      <span className="label">Net Remaining Balance</span>
                      <span className="value text-success">{leaveBalance.remainingDays} days</span>
                    </div>
                    
                    <div className="alert-box warning flex-align-center gap-sm">
                      <AlertTriangle size={20} />
                      <div>
                        <strong>Carried Over Expiration:</strong> {new Date(leaveBalance.expirationDate).toLocaleDateString()}
                        <p className="sub-text">
                          {isCarriedOverExpired() 
                            ? "Carried over days from previous year have expired." 
                            : "Make sure to use carried over days before the expiration date."}
                        </p>
                      </div>
                    </div>
                  </div>
                ) : (
                  <div className="empty-state">No active leave balance configured.</div>
                )}
              </div>
            </div>

            {/* Right Column: Notifications list */}
            <div className="dashboard-main-right">
              <div className="notifications-container glass">
                <div className="section-header">
                  <h3>Notifications ({notifications.filter(n => !n.readStatus).length} unread)</h3>
                </div>
                
                {notifications.length === 0 ? (
                  <div className="empty-state">No notifications.</div>
                ) : (
                  <div className="notification-list">
                    {notifications.map(notif => (
                      <div key={notif.notificationId} className={`notification-item glass ${notif.readStatus ? 'read' : 'unread'}`}>
                        <div className="notification-main">
                          <p className="notification-msg">{notif.message}</p>
                          <span className="notification-date">{new Date(notif.date).toLocaleString()}</span>
                        </div>
                        <div className="notification-actions">
                          {!notif.readStatus && (
                            <button className="icon-btn check-icon" onClick={() => handleMarkAsRead(notif.notificationId)} title="Mark as read">
                              <Check size={16} />
                            </button>
                          )}
                          <button className="icon-btn delete-icon text-danger" onClick={() => handleDeleteNotification(notif.notificationId)} title="Delete">
                            <X size={16} />
                          </button>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          </div>
        </>
      )}
    </div>
  );
};

export default Dashboard;
