import React, { useState, useEffect } from 'react';
import { Plus, Search, Edit2, Trash2, X, Check } from 'lucide-react';
import api from '../services/api';
import './EmployeeList.css';

const EmployeeList = () => {
  const [employees, setEmployees] = useState([]);
  const [departments, setDepartments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  
  // Modal states
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingEmployee, setEditingEmployee] = useState(null);
  
  // Form states
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [phone, setPhone] = useState('');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [role, setRole] = useState('Employee');
  const [department, setDepartment] = useState('');
  const [managerId, setManagerId] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      setLoading(true);
      const [empRes, deptRes] = await Promise.all([
        api.get('/employees'),
        api.get('/departments')
      ]);
      setEmployees(empRes.data);
      setDepartments(deptRes.data);
      
      // Default department to first one if available
      if (deptRes.data.length > 0) {
        setDepartment(deptRes.data[0].name);
      }
    } catch (err) {
      console.error("Error fetching data", err);
      setError("Failed to load employee directory data.");
    } finally {
      setLoading(false);
    }
  };

  const handleOpenCreate = () => {
    setEditingEmployee(null);
    setFirstName('');
    setLastName('');
    setPhone('');
    setUsername('');
    setPassword('');
    setRole('Employee');
    if (departments.length > 0) {
      setDepartment(departments[0].name);
    } else {
      setDepartment('General');
    }
    setManagerId('');
    setError('');
    setIsModalOpen(true);
  };

  const handleOpenEdit = (emp) => {
    setEditingEmployee(emp);
    setFirstName(emp.firstName);
    setLastName(emp.lastName);
    setPhone(emp.phone);
    setUsername(''); // Don't show password hashes
    setPassword('');
    setRole(emp.role);
    setDepartment(emp.department || 'General');
    setManagerId(emp.managerId || '');
    setError('');
    setIsModalOpen(true);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (!firstName.trim() || !lastName.trim() || !phone.trim() || !department.trim()) {
      setError("All fields are required.");
      return;
    }

    try {
      if (editingEmployee) {
        // Update details
        await api.put(`/employees/${editingEmployee.id}`, {
          firstName,
          lastName,
          phone,
          department
        });

        // Update manager if changed
        if (managerId !== (editingEmployee.managerId || '')) {
          if (managerId) {
            await api.put(`/employees/${editingEmployee.id}/assign-manager`, {
              managerId: parseInt(managerId)
            });
          } else {
            // No assign manager endpoint for remove, assign to null
            // We can call assign-manager with 0 or delete relation if supported, 
            // for simplicity we just handle assignment
          }
        }
      } else {
        // Create new
        if (!username.trim() || !password.trim()) {
          setError("Username and password are required for new employees.");
          return;
        }

        await api.post('/employees', {
          firstName,
          lastName,
          phone,
          username,
          password,
          role,
          department,
          managerId: managerId ? parseInt(managerId) : null
        });
      }

      setIsModalOpen(false);
      fetchData();
    } catch (err) {
      console.error("Save failed", err);
      setError(err.response?.data?.message || "An error occurred while saving.");
    }
  };

  const handleDelete = async (id) => {
    if (window.confirm("Are you sure you want to delete this employee?")) {
      try {
        await api.delete(`/employees/${id}`);
        fetchData();
      } catch (err) {
        console.error("Delete failed", err);
        alert("Failed to delete employee.");
      }
    }
  };

  const managers = employees.filter(emp => emp.role === 'Manager');

  const filteredEmployees = employees.filter(emp => 
    `${emp.firstName} ${emp.lastName}`.toLowerCase().includes(searchTerm.toLowerCase()) ||
    (emp.department && emp.department.toLowerCase().includes(searchTerm.toLowerCase()))
  );

  return (
    <div className="employee-list-container">
      <div className="page-header flex-header">
        <div>
          <h1>Employee Directory</h1>
          <p className="subtitle">Manage all company employees, roles, and departments.</p>
        </div>
        <button className="btn-primary" onClick={handleOpenCreate}>
          <Plus size={18} />
          <span>Add Employee</span>
        </button>
      </div>

      <div className="table-controls glass">
        <div className="search-bar">
          <Search size={18} className="search-icon" />
          <input 
            type="text" 
            placeholder="Search by name or department..." 
            className="form-input search-input"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
      </div>

      <div className="table-container glass">
        {loading ? (
          <div className="loader">Loading dataset...</div>
        ) : (
          <table className="premium-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Name</th>
                <th>Phone</th>
                <th>Department</th>
                <th>Role</th>
                <th>Manager</th>
                <th className="text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              {filteredEmployees.map(emp => (
                <tr key={emp.id}>
                  <td><code>#{emp.id}</code></td>
                  <td>
                    <div className="flex-align-center gap-sm">
                      <div className="avatar micro">{(emp.firstName || '').charAt(0)}</div>
                      <strong>{emp.firstName} {emp.lastName}</strong>
                    </div>
                  </td>
                  <td>{emp.phone}</td>
                  <td><span className="badge badge-outline">{emp.department || 'General'}</span></td>
                  <td><span className={`badge ${emp.role === 'Admin' ? 'badge-danger' : emp.role === 'Manager' ? 'badge-primary' : 'badge-secondary'}`}>{emp.role}</span></td>
                  <td>
                    {emp.managerId ? (
                      (() => {
                        const m = employees.find(e => e.id === emp.managerId);
                        return m ? `${m.firstName} ${m.lastName}` : `ID: ${emp.managerId}`;
                      })()
                    ) : (
                      <span className="text-muted">None</span>
                    )}
                  </td>
                  <td>
                    <div className="action-buttons text-right">
                      <button className="icon-btn edit" onClick={() => handleOpenEdit(emp)}><Edit2 size={16} /></button>
                      <button className="icon-btn delete" onClick={() => handleDelete(emp.id)}><Trash2 size={16} /></button>
                    </div>
                  </td>
                </tr>
              ))}
              {filteredEmployees.length === 0 && (
                <tr>
                  <td colSpan="7" className="text-center py-xl">No employees found.</td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {/* Create/Edit Modal */}
      {isModalOpen && (
        <div className="modal-overlay">
          <div className="modal-card glass">
            <div className="modal-header">
              <h2>{editingEmployee ? 'Edit Employee' : 'Add New Employee'}</h2>
              <button className="icon-btn" onClick={() => setIsModalOpen(false)}><X size={20} /></button>
            </div>
            
            {error && <div className="error-banner"><span className="text-danger">{error}</span></div>}
            
            <form onSubmit={handleSubmit} className="modal-form">
              <div className="form-row">
                <div className="form-group">
                  <label>First Name</label>
                  <input type="text" className="form-input" value={firstName} onChange={e => setFirstName(e.target.value)} required />
                </div>
                <div className="form-group">
                  <label>Last Name</label>
                  <input type="text" className="form-input" value={lastName} onChange={e => setLastName(e.target.value)} required />
                </div>
              </div>

              <div className="form-group">
                <label>Phone Number</label>
                <input type="text" className="form-input" value={phone} onChange={e => setPhone(e.target.value.replace(/\D/g, ''))} required />
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label>Department</label>
                  <select className="form-input" value={department} onChange={e => setDepartment(e.target.value)} required>
                    {departments.map(d => (
                      <option key={d.departmentId} value={d.name}>{d.name}</option>
                    ))}
                    {departments.length === 0 && <option value="General">General</option>}
                  </select>
                </div>
                
                {!editingEmployee && (
                  <div className="form-group">
                    <label>Role</label>
                    <select className="form-input" value={role} onChange={e => setRole(e.target.value)}>
                      <option value="Employee">Employee</option>
                      <option value="Manager">Manager</option>
                      <option value="Admin">Admin</option>
                    </select>
                  </div>
                )}
              </div>

              {!editingEmployee && (
                <div className="form-row">
                  <div className="form-group">
                    <label>Username</label>
                    <input type="text" className="form-input" value={username} onChange={e => setUsername(e.target.value)} required />
                  </div>
                  <div className="form-group">
                    <label>Password</label>
                    <input type="password" className="form-input" value={password} onChange={e => setPassword(e.target.value)} required />
                  </div>
                </div>
              )}

              <div className="form-group">
                <label>Assign Manager</label>
                <select className="form-input" value={managerId} onChange={e => setManagerId(e.target.value)}>
                  <option value="">None (Top Level)</option>
                  {managers.map(m => (
                    <option key={m.id} value={m.id}>{m.firstName} {m.lastName}</option>
                  ))}
                </select>
              </div>

              <div className="modal-actions">
                <button type="button" className="btn-secondary" onClick={() => setIsModalOpen(false)}>Cancel</button>
                <button type="submit" className="btn-primary" disabled={!firstName.trim() || !lastName.trim() || !phone.trim() || !department.trim() || (!editingEmployee && (!username.trim() || !password.trim()))}>Save Changes</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default EmployeeList;
