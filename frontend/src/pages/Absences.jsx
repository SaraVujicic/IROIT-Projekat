import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { Plus, X, Calendar, FileText, Check, AlertCircle, Eye } from 'lucide-react';
import api from '../services/api';
import './Absences.css';

const Absences = () => {
  const { user } = useAuth();
  const [absences, setAbsences] = useState([]);
  const [employees, setEmployees] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  
  // Create absence states
  const [isOpen, setIsOpen] = useState(false);
  const [type, setType] = useState('Annual'); // Annual, Sick, DayOff
  const [description, setDescription] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [medicalDocument, setMedicalDocument] = useState(false);
  const [selectedFile, setSelectedFile] = useState(null);
  const [singleDate, setSingleDate] = useState('');
  const [targetEmployeeId, setTargetEmployeeId] = useState(user?.id || '');

  useEffect(() => {
    fetchAbsences();
    if (user.role === 'Admin' || user.role === 'Manager') {
      fetchEmployees();
    }
    if (user?.id) {
      setTargetEmployeeId(user.id);
    }
  }, [user]);

  const resetForm = () => {
    setDescription('');
    setStartDate('');
    setEndDate('');
    setSingleDate('');
    setMedicalDocument(false);
    setSelectedFile(null);
    if (user?.id) setTargetEmployeeId(user.id);
  };

  const handleClose = () => {
    setIsOpen(false);
    resetForm();
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

  const fetchAbsences = async () => {
    try {
      setLoading(true);
      setError('');
      if (user.role === 'Admin') {
        const res = await api.get('/absences');
        setAbsences(res.data);
      } else if (user.role === 'Manager') {
        const [absRes, empRes] = await Promise.all([
          api.get('/absences'),
          api.get('/employees')
        ]);
        
        const managerEmp = empRes.data.find(e => e.id === user.id);
        const managerDept = managerEmp?.department;
        
        const filtered = absRes.data.filter(abs => {
          if (abs.employeeId === user.id) return true;
          
          const emp = empRes.data.find(e => e.id === abs.employeeId);
          if (!emp) return false;
          
          if (emp.role === 'Admin') return false;
          if (emp.role === 'Manager' && emp.id !== user.id) return false;
          
          return (emp.department === managerDept || emp.managerId === user.id);
        });
        setAbsences(filtered);
      } else {
        const res = await api.get('/absences');
        const filtered = res.data.filter(abs => abs.employeeId === user.id);
        setAbsences(filtered);
      }
    } catch (err) {
      console.error("Failed to load absences", err);
      setError("Failed to load absences from server.");
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (!description.trim()) {
      setError("Description is required.");
      return;
    }

    try {
      if (type === 'Annual') {
        if (!startDate || !endDate) return setError("Start and end date are required.");
        await api.post('/absences/annual', {
          employeeId: targetEmployeeId,
          description,
          startDate: new Date(startDate).toISOString(),
          endDate: new Date(endDate).toISOString()
        });
      } else if (type === 'Sick') {
        if (!startDate || !endDate) return setError("Start and end date are required.");
        const res = await api.post('/absences/sick', {
          employeeId: targetEmployeeId,
          description,
          startDate: new Date(startDate).toISOString(),
          endDate: new Date(endDate).toISOString(),
          medicalDocument: !!selectedFile || medicalDocument
        });
        const absenceId = res.data;

        if (selectedFile) {
          const formData = new FormData();
          formData.append('file', selectedFile);
          await api.post(`/absences/${absenceId}/document`, formData, {
            headers: {
              'Content-Type': 'multipart/form-data'
            }
          });
        }
      } else {
        if (!singleDate) return setError("Date is required.");
        await api.post('/absences/dayoff', {
          employeeId: targetEmployeeId,
          description,
          date: new Date(singleDate).toISOString()
        });
      }

      resetForm();
      setIsOpen(false);
      fetchAbsences();
    } catch (err) {
      console.error("Absence submission failed", err);
      setError(err.response?.data?.message || "Overlap check failed or invalid dates provided.");
    }
  };

  const getAbsenceBadge = (typeStr) => {
    switch (typeStr) {
      case 'AnnualLeave':
      case 'Annual':
        return <span className="badge badge-success">Annual Leave</span>;
      case 'SickLeave':
      case 'Sick':
        return <span className="badge badge-danger">Sick Leave</span>;
      default:
        return <span className="badge badge-warning">Day Off</span>;
    }
  };

  return (
    <div className="absences-container">
      <div className="page-header flex-header">
        <div>
          <h1>Absences Register</h1>
          <p className="subtitle">Official list of sick leaves, day-offs, and vacation days.</p>
        </div>
        {(user.role === 'Employee' || user.role === 'Manager' || user.role === 'Admin') && (
          <button className="btn-primary" onClick={() => setIsOpen(true)}>
            <Plus size={18} />
            <span>Record Absence</span>
          </button>
        )}
      </div>

      {error && <div className="error-banner flex-align-center gap-sm"><AlertCircle className="text-danger" /><span>{error}</span></div>}

      <div className="table-container glass mt-lg">
        {loading ? (
          <div className="loader">Loading absences...</div>
        ) : (
          <table className="premium-table">
            <thead>
              <tr>
                <th>ID</th>
                {(user.role === 'Admin' || user.role === 'Manager') && <th>Employee</th>}
                <th>Type</th>
                <th>Description</th>
                <th>Dates</th>
                <th>Medical Document</th>
              </tr>
            </thead>
            <tbody>
              {absences.map(abs => (
                <tr key={abs.id}>
                  <td><code>#{abs.id}</code></td>
                  {(user.role === 'Admin' || user.role === 'Manager') && (
                    <td><strong>{getEmployeeName(abs.employeeId)}</strong></td>
                  )}
                  <td>{getAbsenceBadge(abs.type)}</td>
                  <td>{abs.description}</td>
                  <td>
                    {abs.type === 'DayOff' || abs.date ? (
                      new Date(abs.date || abs.startDate).toLocaleDateString()
                    ) : (
                      `${new Date(abs.startDate).toLocaleDateString()} - ${new Date(abs.endDate).toLocaleDateString()}`
                    )}
                  </td>
                  <td>
                    {abs.medicalDocument !== null ? (
                      abs.medicalDocument ? (
                        <span className="text-success flex-align-center gap-xs" style={{display: 'inline-flex', alignItems: 'center'}}>
                          <Check size={16} /> Yes
                          <a 
                            href={`${api.defaults.baseURL}/absences/${abs.id}/document`} 
                            target="_blank" 
                            rel="noopener noreferrer" 
                            className="text-primary flex-align-center gap-xs ml-sm"
                            style={{textDecoration: 'underline', fontSize: '0.85rem', display: 'inline-flex', marginLeft: '0.5rem'}}
                            title="View/Download Document"
                          >
                            <Eye size={14} /> View
                          </a>
                        </span>
                      ) : (
                        <span className="text-muted">No</span>
                      )
                    ) : (
                      <span className="text-muted">-</span>
                    )}
                  </td>
                </tr>
              ))}
              {absences.length === 0 && (
                <tr>
                  <td colSpan={(user.role === 'Admin' || user.role === 'Manager') ? "6" : "5"} className="text-center py-xl">No absences recorded.</td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {/* Record Absence Modal */}
      {isOpen && (
        <div className="modal-overlay">
          <div className="modal-card glass">
            <div className="modal-header">
              <h2>Record Absence</h2>
              <button type="button" className="icon-btn" onClick={handleClose}><X size={20} /></button>
            </div>
            
            <form onSubmit={handleSubmit} className="modal-form">
              {(user.role === 'Admin' || user.role === 'Manager') && (
                <div className="form-group">
                  <label>For Employee</label>
                  <select 
                    className="form-input" 
                    value={targetEmployeeId} 
                    onChange={e => setTargetEmployeeId(parseInt(e.target.value))}
                  >
                    {user.role === 'Admin' ? (
                      employees.map(emp => (
                        <option key={emp.id} value={emp.id}>
                          {emp.firstName} {emp.lastName} ({emp.role})
                        </option>
                      ))
                    ) : (
                      employees
                        .filter(emp => {
                          if (emp.id === user.id) return true;
                          if (emp.role === 'Admin') return false;
                          if (emp.role === 'Manager') return false;
                          const managerEmp = employees.find(e => e.id === user.id);
                          return emp.department === managerEmp?.department || emp.managerId === user.id;
                        })
                        .map(emp => (
                          <option key={emp.id} value={emp.id}>
                            {emp.firstName} {emp.lastName} ({emp.role})
                          </option>
                        ))
                    )}
                  </select>
                </div>
              )}

              <div className="form-group">
                <label>Absence Type</label>
                <select className="form-input" value={type} onChange={e => setType(e.target.value)}>
                  <option value="Annual">Annual Leave</option>
                  <option value="Sick">Sick Leave</option>
                  <option value="DayOff">Day Off (Single Day)</option>
                </select>
              </div>

              <div className="form-group">
                <label>Description / Note</label>
                <textarea className="form-input text-area" value={description} onChange={e => setDescription(e.target.value)} required placeholder="E.g., Dental appointment, annual vacation, etc." />
              </div>

              {type !== 'DayOff' ? (
                <div className="form-row">
                  <div className="form-group">
                    <label>Start Date</label>
                    <input type="date" className="form-input" value={startDate} onChange={e => setStartDate(e.target.value)} required />
                  </div>
                  <div className="form-group">
                    <label>End Date</label>
                    <input type="date" className="form-input" value={endDate} onChange={e => setEndDate(e.target.value)} required />
                  </div>
                </div>
              ) : (
                <div className="form-group">
                  <label>Date</label>
                  <input type="date" className="form-input" value={singleDate} onChange={e => setSingleDate(e.target.value)} required />
                </div>
              )}

              {type === 'Sick' && (
                <>
                  <div className="form-group flex-align-center" style={{paddingTop: '1rem'}}>
                    <input 
                      type="checkbox" 
                      id="med-doc" 
                      checked={medicalDocument} 
                      onChange={e => {
                        setMedicalDocument(e.target.checked);
                        if (!e.target.checked) setSelectedFile(null);
                      }} 
                    />
                    <label htmlFor="med-doc" style={{marginLeft: '0.5rem'}}>Medical Document / Note Attached</label>
                  </div>
                  {medicalDocument && (
                    <div className="form-group" style={{paddingTop: '0.5rem'}}>
                      <label>Upload Document (PDF, Image, etc.)</label>
                      <input 
                        type="file" 
                        className="form-input" 
                        onChange={e => setSelectedFile(e.target.files[0])} 
                      />
                    </div>
                  )}
                </>
              )}

              <div className="modal-actions">
                <button type="button" className="btn-secondary" onClick={handleClose}>Cancel</button>
                <button type="submit" className="btn-primary" disabled={!description.trim() || (type !== 'DayOff' ? (!startDate || !endDate) : !singleDate) || (type === 'Sick' && medicalDocument && !selectedFile)}>Record</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default Absences;
