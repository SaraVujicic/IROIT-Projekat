import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { Plus, Check, X, FileText, Calendar, Clock, AlertCircle } from 'lucide-react';
import api from '../services/api';
import './Requests.css';

const Requests = () => {
  const { user } = useAuth();
  const [requests, setRequests] = useState([]);
  const [employees, setEmployees] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  
  // Submit request states
  const [isSubmitOpen, setIsSubmitOpen] = useState(false);
  const [type, setType] = useState(1); // 1 = Leave, 2 = Travel, 3 = Internship, 4 = Education
  const [description, setDescription] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [leaveType, setLeaveType] = useState(1); // 1 = Annual, 2 = Sick, 3 = DayOff
  const [singleDate, setSingleDate] = useState('');

  // Details for specific requests
  const [destination, setDestination] = useState('');
  const [costs, setCosts] = useState(0);
  const [name, setName] = useState('');
  const [mentorFirstName, setMentorFirstName] = useState('');
  const [mentorLastName, setMentorLastName] = useState('');
  const [mentorPosition, setMentorPosition] = useState('');
  const [certificate, setCertificate] = useState(false);

  useEffect(() => {
    fetchRequests();
    if (user.role === 'Admin' || user.role === 'Manager') {
      fetchEmployees();
    }
  }, [user]);

  const fetchRequests = async () => {
    try {
      setLoading(true);
      setError('');
      if (user.role === 'Admin') {
        const res = await api.get('/requests');
        setRequests(res.data);
      } else if (user.role === 'Manager') {
        const [reqRes, empRes] = await Promise.all([
          api.get('/requests'),
          api.get('/employees')
        ]);
        
        const managerEmp = empRes.data.find(e => e.id === user.id);
        const managerDept = managerEmp?.department;
        
        const filtered = reqRes.data.filter(r => {
          if (r.employeeId === user.id) return true;
          
          const emp = empRes.data.find(e => e.id === r.employeeId);
          if (!emp) return false;
          
          if (emp.role === 'Admin') return false;
          if (emp.role === 'Manager' && emp.id !== user.id) return false;
          
          return (emp.department === managerDept || emp.managerId === user.id);
        });
        setRequests(filtered);
      } else {
        const res = await api.get(`/requests/employee/${user.id}`);
        setRequests(res.data);
      }
    } catch (err) {
      console.error("Failed to fetch requests", err);
      setError("Failed to synchronize requests with server.");
    } finally {
      setLoading(false);
    }
  };

  const fetchEmployees = async () => {
    try {
      const res = await api.get('/employees');
      setEmployees(res.data);
    } catch (err) {
      console.error("Failed to fetch employees", err);
    }
  };

  const getEmployeeName = (empId) => {
    const emp = employees.find(e => e.id === empId);
    return emp ? `${emp.firstName} ${emp.lastName}` : `Employee #${empId}`;
  };

  const getEmployeeRole = (empId) => {
    const emp = employees.find(e => e.id === empId);
    return emp ? emp.role : 'Employee';
  };

  const handleCreate = async (e) => {
    e.preventDefault();
    setError('');

    const isDayOff = (parseInt(type) === 1 && parseInt(leaveType) === 3);

    if (!description.trim()) {
      setError("Description is required.");
      return;
    }

    if (isDayOff) {
      if (!singleDate) {
        setError("Please choose a date.");
        return;
      }
    } else {
      if (!startDate || !endDate) {
        setError("Start and end date are required.");
        return;
      }
    }

    if (type === 2 && (!destination.trim() || !costs)) {
      setError("Destination and estimated costs are required.");
      return;
    }

    if (type === 3 && (!name.trim() || !mentorFirstName.trim() || !mentorLastName.trim() || !mentorPosition.trim())) {
      setError("All internship details and mentor details are required.");
      return;
    }

    if (type === 4 && !name.trim()) {
      setError("Education course name is required.");
      return;
    }

    try {
      const payload = {
        employeeId: user.id,
        description,
        startDate: isDayOff ? new Date(singleDate).toISOString() : new Date(startDate).toISOString(),
        endDate: isDayOff ? new Date(singleDate).toISOString() : new Date(endDate).toISOString(),
        type: parseInt(type),
        leaveType: parseInt(type) === 1 ? parseInt(leaveType) : null,
        destination: parseInt(type) === 2 ? destination : null,
        costs: parseInt(type) === 2 ? parseFloat(costs) : null,
        name: (parseInt(type) === 3 || parseInt(type) === 4) ? name : null,
        mentorFirstName: parseInt(type) === 3 ? mentorFirstName : null,
        mentorLastName: parseInt(type) === 3 ? mentorLastName : null,
        mentorPosition: parseInt(type) === 3 ? mentorPosition : null,
        certificate: parseInt(type) === 4 ? certificate : false
      };

      const res = await api.post('/requests/leave', payload); // Our generic mapped command
      const reqId = res.data;

      if (user.role === 'Admin') {
        // Auto-approve as Manager and Admin so status goes to 'Approved'
        await api.put(`/requests/${reqId}/approve`, null, { headers: { 'X-Approver-Role': 'Manager' } });
        await api.put(`/requests/${reqId}/approve`, null, { headers: { 'X-Approver-Role': 'Admin' } });
      }
      
      // Notify Admin/Manager
      // Send a general notification on the backend
      const managerId = employees.find(e => e.id === user.id)?.managerId;
      if (managerId) {
        await api.post('/notifications', {
          recipientId: managerId,
          message: `New request submitted by ${user.name}: ${description}`
        });
      }

      if (user.role === 'Manager') {
        const admin = employees.find(e => e.role === 'Admin');
        if (admin) {
          await api.post('/notifications', {
            recipientId: admin.id,
            message: `Manager ${user.name} has submitted a new request for ${getRequestTypeName(parseInt(type))}.`
          });
        }
      }

      setIsSubmitOpen(false);
      setDescription('');
      setStartDate('');
      setEndDate('');
      setSingleDate('');
      fetchRequests();
    } catch (err) {
      console.error("Failed to create request", err);
      setError(err.response?.data?.message || "Failed to create request. Ensure no overlapping dates exist.");
    }
  };

  const handleApprove = async (id) => {
    try {
      const req = requests.find(r => r.id === id);
      await api.put(`/requests/${id}/approve`, null, {
        headers: { 'X-Approver-Role': user.role }
      });

      if (user.role === 'Manager') {
        const admin = employees.find(e => e.role === 'Admin');
        if (admin && req) {
          await api.post('/notifications', {
            recipientId: admin.id,
            message: `Manager has approved the request for ${getRequestTypeName(req.type)} (ID #${id}) for employee ${getEmployeeName(req.employeeId)}.`
          });
        }
      }
      fetchRequests();
    } catch (err) {
      console.error("Approval failed", err);
    }
  };

  const handleReject = async (id) => {
    try {
      await api.put(`/requests/${id}/reject`);
      fetchRequests();
    } catch (err) {
      console.error("Rejection failed", err);
    }
  };

  const getStatusBadge = (status) => {
    switch (status) {
      case 'Approved':
        return <span className="badge badge-success">Approved</span>;
      case 'Rejected':
        return <span className="badge badge-danger">Rejected</span>;
      case 'PendingAdminApproval':
        return <span className="badge badge-primary">Pending Admin Approval</span>;
      default:
        return <span className="badge badge-warning">Pending</span>;
    }
  };

  const getRequestTypeName = (t) => {
    switch (t) {
      case 1:
      case 'Leave':
        return 'Leave';
      case 2:
      case 'TravelOrder':
        return 'Travel Order';
      case 3:
      case 'Internship':
        return 'Internship';
      case 4:
      case 'Education':
        return 'Education';
      default:
        return 'General';
    }
  };

  return (
    <div className="requests-container">
      <div className="page-header flex-header">
        <div>
          <h1>Requests Management</h1>
          <p className="subtitle">Track, submit, and review employee requests and travel orders.</p>
        </div>
        {(user.role === 'Employee' || user.role === 'Manager' || user.role === 'Admin') && (
          <button className="btn-primary" onClick={() => setIsSubmitOpen(true)}>
            <Plus size={18} />
            <span>New Request</span>
          </button>
        )}
      </div>

      {error && <div className="error-banner flex-align-center gap-sm"><AlertCircle className="text-danger" /><span>{error}</span></div>}

      <div className="table-container glass mt-lg">
        {loading ? (
          <div className="loader">Syncing requests...</div>
        ) : (
          <table className="premium-table">
            <thead>
              <tr>
                <th>ID</th>
                {(user.role === 'Admin' || user.role === 'Manager') && <th>Employee</th>}
                <th>Type</th>
                <th>Description</th>
                <th>Dates</th>
                <th>Status</th>
                {(user.role === 'Admin' || user.role === 'Manager') && <th className="text-right">Actions</th>}
              </tr>
            </thead>
            <tbody>
              {requests.map(req => (
                <tr key={req.id}>
                  <td><code>#{req.id}</code></td>
                  {(user.role === 'Admin' || user.role === 'Manager') && (
                    <td><strong>{getEmployeeName(req.employeeId)}</strong></td>
                  )}
                  <td><strong>{getRequestTypeName(req.type)}</strong></td>
                  <td>
                    {req.description}
                    {req.travelOrder && <div className="text-primary text-xs">Dest: {req.travelOrder.destination} | Cost: ${req.travelOrder.costs}</div>}
                    {req.internship && <div className="text-primary text-xs">Internship: {req.internship.name} (Mentor: {req.internship.mentor?.firstName} {req.internship.mentor?.lastName})</div>}
                    {req.education && <div className="text-primary text-xs">Education: {req.education.name} {req.education.certificate ? '(Certificate)' : ''}</div>}
                  </td>
                  <td>{new Date(req.startDate).toLocaleDateString()} - {new Date(req.endDate).toLocaleDateString()}</td>
                  <td>{getStatusBadge(req.status)}</td>
                  {(user.role === 'Admin' || user.role === 'Manager') && (
                    <td>
                      <div className="action-buttons text-right">
                        {((user.role === 'Manager' && req.status === 'Pending' && req.employeeId !== user.id) || 
                           (user.role === 'Admin' && (req.status === 'PendingAdminApproval' || (req.status === 'Pending' && getEmployeeRole(req.employeeId) === 'Manager')))) && (
                          <>
                            <button className="btn-approve btn-icon-only mr-sm" onClick={() => handleApprove(req.id)}>
                              <Check size={16} />
                            </button>
                            <button className="btn-reject btn-icon-only" onClick={() => handleReject(req.id)}>
                              <X size={16} />
                            </button>
                          </>
                        )}
                      </div>
                    </td>
                  )}
                </tr>
              ))}
              {requests.length === 0 && (
                <tr>
                  <td colSpan={(user.role === 'Admin' || user.role === 'Manager') ? 7 : 5} className="text-center py-xl">No requests found.</td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {/* Submit Request Modal */}
      {isSubmitOpen && (
        <div className="modal-overlay">
          <div className="modal-card glass">
            <div className="modal-header">
              <h2>Submit Request</h2>
              <button className="icon-btn" onClick={() => setIsSubmitOpen(false)}><X size={20} /></button>
            </div>
            
            <form onSubmit={handleCreate} className="modal-form">
              <div className="form-group">
                <label>Request Type</label>
                <select className="form-input" value={type} onChange={e => setType(parseInt(e.target.value))}>
                  <option value={1}>Leave / Absence</option>
                  <option value={2}>Travel Order</option>
                  <option value={3}>Internship</option>
                  <option value={4}>Education</option>
                </select>
              </div>

              <div className="form-group">
                <label>Description</label>
                <textarea className="form-input text-area" value={description} onChange={e => setDescription(e.target.value)} required placeholder="Reason for request..." />
              </div>

              {/* Render Absence Type right after Description if it is a Leave request */}
              {type === 1 && (
                <div className="form-group">
                  <label>Absence Type</label>
                  <select className="form-input" value={leaveType} onChange={e => setLeaveType(parseInt(e.target.value))}>
                    <option value={1}>Annual Leave</option>
                    <option value={2}>Sick Leave</option>
                    <option value={3}>Day Off</option>
                  </select>
                </div>
              )}

              {(parseInt(type) === 1 && parseInt(leaveType) === 3) ? (
                <div className="form-group">
                  <label>Date</label>
                  <input 
                    type="date" 
                    className="form-input" 
                    value={singleDate} 
                    onChange={e => setSingleDate(e.target.value)} 
                    required 
                  />
                </div>
              ) : (
                <div className="form-row">
                  <div className="form-group">
                    <label>Start Date</label>
                    <input 
                      type="date" 
                      className="form-input" 
                      value={startDate} 
                      onChange={e => setStartDate(e.target.value)} 
                      required 
                    />
                  </div>
                  <div className="form-group">
                    <label>End Date</label>
                    <input 
                      type="date" 
                      className="form-input" 
                      value={endDate} 
                      onChange={e => setEndDate(e.target.value)} 
                      required 
                    />
                  </div>
                </div>
              )}

              {type === 2 && (
                <div className="form-row">
                  <div className="form-group">
                    <label>Destination</label>
                    <input type="text" className="form-input" value={destination} onChange={e => setDestination(e.target.value)} required />
                  </div>
                  <div className="form-group">
                    <label>Estimated Costs ($)</label>
                    <input type="number" className="form-input" value={costs} onChange={e => setCosts(e.target.value)} required />
                  </div>
                </div>
              )}

              {type === 3 && (
                <>
                  <div className="form-group">
                    <label>Internship Name</label>
                    <input type="text" className="form-input" value={name} onChange={e => setName(e.target.value)} required />
                  </div>
                  <div className="form-row">
                    <div className="form-group">
                      <label>Mentor First Name</label>
                      <input type="text" className="form-input" value={mentorFirstName} onChange={e => setMentorFirstName(e.target.value)} required />
                    </div>
                    <div className="form-group">
                      <label>Mentor Last Name</label>
                      <input type="text" className="form-input" value={mentorLastName} onChange={e => setMentorLastName(e.target.value)} required />
                    </div>
                  </div>
                  <div className="form-group">
                    <label>Mentor Position</label>
                    <input type="text" className="form-input" value={mentorPosition} onChange={e => setMentorPosition(e.target.value)} required />
                  </div>
                </>
              )}

              {type === 4 && (
                <div className="form-row">
                  <div className="form-group">
                    <label>Education Course Name</label>
                    <input type="text" className="form-input" value={name} onChange={e => setName(e.target.value)} required />
                  </div>
                  <div className="form-group flex-align-center" style={{paddingTop: '2rem'}}>
                    <input type="checkbox" id="cert" checked={certificate} onChange={e => setCertificate(e.target.checked)} />
                    <label htmlFor="cert" style={{marginLeft: '0.5rem'}}>Requires Certificate</label>
                  </div>
                </div>
              )}

              <div className="modal-actions">
                <button type="button" className="btn-secondary" onClick={() => setIsSubmitOpen(false)}>Cancel</button>
                <button 
                  type="submit" 
                  className="btn-primary" 
                  disabled={
                    !description.trim() || 
                    ((parseInt(type) === 1 && parseInt(leaveType) === 3) ? !singleDate : (!startDate || !endDate)) || 
                    (type === 2 && (!destination.trim() || !costs)) || 
                    (type === 3 && (!name.trim() || !mentorFirstName.trim() || !mentorLastName.trim() || !mentorPosition.trim())) || 
                    (type === 4 && !name.trim())
                  }
                >
                  Submit Request
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default Requests;
