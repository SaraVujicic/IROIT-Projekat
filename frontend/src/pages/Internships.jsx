import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { Plus, X, Award, Briefcase, BookOpen, AlertCircle, Calendar, User } from 'lucide-react';
import api from '../services/api';
import './Internships.css';

const Internships = () => {
  const { user } = useAuth();
  const [items, setItems] = useState([]);
  const [employees, setEmployees] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  
  // Modals
  const [isInternshipOpen, setIsInternshipOpen] = useState(false);
  const [isEducationOpen, setIsEducationOpen] = useState(false);

  // Internship states
  const [internName, setInternName] = useState('');
  const [internEmployeeId, setInternEmployeeId] = useState('');
  const [mentorFirstName, setMentorFirstName] = useState('');
  const [mentorLastName, setMentorLastName] = useState('');
  const [mentorPosition, setMentorPosition] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');

  // Education states
  const [eduName, setEduName] = useState('');
  const [eduStartDate, setEduStartDate] = useState('');
  const [eduEndDate, setEduEndDate] = useState('');
  const [certificate, setCertificate] = useState(false);

  useEffect(() => {
    fetchItems();
    if (user.role === 'Admin') {
      fetchEmployees();
    }
  }, [user]);

  const fetchItems = async () => {
    try {
      setLoading(true);
      setError('');
      let res;
      if (user.role === 'Admin') {
        res = await api.get('/requests');
      } else {
        res = await api.get(`/requests/employee/${user.id}`);
      }

      // Filter requests of type Internship (3) or Education (4)
      const filtered = res.data.filter(r => r.type === 3 || r.type === 4 || r.type === 'Internship' || r.type === 'Education');
      setItems(filtered);
    } catch (err) {
      console.error("Failed to load internships & education", err);
      setError("Failed to load records from server.");
    } finally {
      setLoading(false);
    }
  };

  const fetchEmployees = async () => {
    try {
      const res = await api.get('/employees');
      setEmployees(res.data);
    } catch (err) {
      console.error("Failed to load employees list", err);
    }
  };

  const getEmployeeName = (empId) => {
    const emp = employees.find(e => e.id === empId);
    return emp ? `${emp.firstName} ${emp.lastName}` : `Employee #${empId}`;
  };

  const handleRecordInternship = async (e) => {
    e.preventDefault();
    setError('');

    if (!internName || !internEmployeeId || !mentorFirstName || !mentorLastName || !mentorPosition || !startDate || !endDate) {
      setError("All fields are required.");
      return;
    }

    try {
      // Record internship for the specified employee (directly approved since created by Admin)
      const payload = {
        employeeId: parseInt(internEmployeeId),
        description: `Internship: ${internName}`,
        startDate: new Date(startDate).toISOString(),
        endDate: new Date(endDate).toISOString(),
        type: 3, // Internship
        name: internName,
        mentorFirstName,
        mentorLastName,
        mentorPosition
      };

      // Create Request
      const res = await api.post('/requests/leave', payload);
      const reqId = res.data;

      // Auto-approve by Manager and Admin since created by Admin
      await api.put(`/requests/${reqId}/approve`, null, { headers: { 'X-Approver-Role': 'Manager' } });
      await api.put(`/requests/${reqId}/approve`, null, { headers: { 'X-Approver-Role': 'Admin' } });

      setIsInternshipOpen(false);
      setInternName('');
      setInternEmployeeId('');
      setMentorFirstName('');
      setMentorLastName('');
      setMentorPosition('');
      setStartDate('');
      setEndDate('');
      fetchItems();
    } catch (err) {
      console.error("Failed to record internship", err);
      setError(err.response?.data?.message || "Failed to save internship record.");
    }
  };

  const handleApplyEducation = async (e) => {
    e.preventDefault();
    setError('');

    if (!eduName || !eduStartDate || !eduEndDate) {
      setError("All fields are required.");
      return;
    }

    try {
      const payload = {
        employeeId: user.id,
        description: `Education course: ${eduName}`,
        startDate: new Date(eduStartDate).toISOString(),
        endDate: new Date(eduEndDate).toISOString(),
        type: 4, // Education
        name: eduName,
        certificate
      };

      const res = await api.post('/requests/leave', payload);
      const reqId = res.data;

      if (user.role === 'Admin') {
        await api.put(`/requests/${reqId}/approve`, null, { headers: { 'X-Approver-Role': 'Manager' } });
        await api.put(`/requests/${reqId}/approve`, null, { headers: { 'X-Approver-Role': 'Admin' } });
      }

      if (user.role === 'Manager') {
        try {
          const empRes = await api.get('/employees');
          const admin = empRes.data.find(e => e.role === 'Admin');
          if (admin) {
            await api.post('/notifications', {
              recipientId: admin.id,
              message: `Manager ${user.name} has submitted a new request for Education.`
            });
          }
        } catch (err) {
          console.error("Failed to notify admin about manager education request", err);
        }
      }
      setIsEducationOpen(false);
      setEduName('');
      setEduStartDate('');
      setEduEndDate('');
      setCertificate(false);
      fetchItems();
    } catch (err) {
      console.error("Failed to apply for education", err);
      setError(err.response?.data?.message || "Failed to submit education request.");
    }
  };

  const getActiveStatus = (endDate) => {
    return new Date() < new Date(endDate) ? 'Active' : 'Completed';
  };

  const internships = items.filter(r => r.type === 3 || r.type === 'Internship');
  const educations = items.filter(r => r.type === 4 || r.type === 'Education');

  return (
    <div className="internships-container">
      <div className="page-header flex-header">
        <div>
          <h1>Internships & Education</h1>
          <p className="subtitle">Track professional education programs and internship mentorship logs.</p>
        </div>
        <div className="action-buttons-header">
          {user.role === 'Admin' && (
            <button className="btn-primary mr-sm" onClick={() => setIsInternshipOpen(true)}>
              <Plus size={18} />
              <span>Record Internship</span>
            </button>
          )}
          {(user.role === 'Employee' || user.role === 'Manager' || user.role === 'Admin') && (
            <button className="btn-primary" onClick={() => setIsEducationOpen(true)}>
              <Plus size={18} />
              <span>Apply for Education</span>
            </button>
          )}
        </div>
      </div>

      {error && <div className="error-banner flex-align-center gap-sm"><AlertCircle className="text-danger" /><span>{error}</span></div>}

      {loading ? (
        <div className="loader">Loading professional records...</div>
      ) : (
        <div className="programs-layout-grid">
          
          {/* Internships section */}
          <div className="programs-column glass">
            <h2><Briefcase size={22} className="text-primary inline-icon" /> Internships ({internships.length})</h2>
            <div className="programs-list mt-md">
              {internships.map(item => (
                <div key={item.id} className="program-item glass">
                  <div className="program-header">
                    <h4>{item.internship?.name || 'Internship'}</h4>
                    <span className={`badge ${getActiveStatus(item.endDate) === 'Active' ? 'badge-primary' : 'badge-outline'}`}>
                      {getActiveStatus(item.endDate)}
                    </span>
                  </div>
                  <div className="program-body">
                    {user.role === 'Admin' && (
                      <p className="detail"><User size={14} /> <strong>Intern:</strong> {getEmployeeName(item.employeeId)}</p>
                    )}
                    <p className="detail"><User size={14} /> <strong>Mentor:</strong> {item.internship?.mentor?.firstName} {item.internship?.mentor?.lastName} ({item.internship?.mentor?.position})</p>
                    <p className="detail"><Calendar size={14} /> {new Date(item.startDate).toLocaleDateString()} - {new Date(item.endDate).toLocaleDateString()}</p>
                  </div>
                </div>
              ))}
              {internships.length === 0 && <div className="empty-state">No recorded internships.</div>}
            </div>
          </div>

          {/* Education section */}
          <div className="programs-column glass">
            <h2><BookOpen size={22} className="text-warning inline-icon" /> Education & Skills ({educations.length})</h2>
            <div className="programs-list mt-md">
              {educations.map(item => (
                <div key={item.id} className="program-item glass">
                  <div className="program-header">
                    <h4>{item.education?.name || 'Education Course'}</h4>
                    <span className={`badge ${item.status === 'Approved' ? 'badge-success' : item.status === 'Rejected' ? 'badge-danger' : 'badge-warning'}`}>
                      {item.status}
                    </span>
                  </div>
                  <div className="program-body">
                    {user.role === 'Admin' && (
                      <p className="detail"><User size={14} /> <strong>Employee:</strong> {getEmployeeName(item.employeeId)}</p>
                    )}
                    <p className="detail"><Calendar size={14} /> {new Date(item.startDate).toLocaleDateString()} - {new Date(item.endDate).toLocaleDateString()}</p>
                    {item.education?.certificate && (
                      <span className="badge badge-outline mt-sm text-xs"><Award size={12} /> Certificate Course</span>
                    )}
                  </div>
                </div>
              ))}
              {educations.length === 0 && <div className="empty-state">No recorded educations.</div>}
            </div>
          </div>

        </div>
      )}

      {/* Record Internship Modal */}
      {isInternshipOpen && (
        <div className="modal-overlay">
          <div className="modal-card glass">
            <div className="modal-header">
              <h2>Record Internship Engagement</h2>
              <button className="icon-btn" onClick={() => setIsInternshipOpen(false)}><X size={20} /></button>
            </div>
            
            <form onSubmit={handleRecordInternship} className="modal-form">
              <div className="form-group">
                <label>Internship Name / Title</label>
                <input type="text" className="form-input" value={internName} onChange={e => setInternName(e.target.value)} required placeholder="E.g., Summer Frontend Internship" />
              </div>

              <div className="form-group">
                <label>Assign Intern (Employee)</label>
                <select className="form-input" value={internEmployeeId} onChange={e => setInternEmployeeId(e.target.value)} required>
                  <option value="">Select Employee...</option>
                  {employees.map(emp => (
                    <option key={emp.id} value={emp.id}>{emp.firstName} {emp.lastName} ({emp.role})</option>
                  ))}
                </select>
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label>Mentor First Name</label>
                  <input type="text" className="form-input" value={mentorFirstName} onChange={e => setMentorFirstName(e.target.value)} required placeholder="Alice" />
                </div>
                <div className="form-group">
                  <label>Mentor Last Name</label>
                  <input type="text" className="form-input" value={mentorLastName} onChange={e => setMentorLastName(e.target.value)} required placeholder="Williams" />
                </div>
              </div>

              <div className="form-group">
                <label>Mentor Position</label>
                <input type="text" className="form-input" value={mentorPosition} onChange={e => setMentorPosition(e.target.value)} required placeholder="Tech Lead / HR Specialist" />
              </div>

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

              <div className="modal-actions">
                <button type="button" className="btn-secondary" onClick={() => setIsInternshipOpen(false)}>Cancel</button>
                <button type="submit" className="btn-primary">Record</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Apply Education Modal */}
      {isEducationOpen && (
        <div className="modal-overlay">
          <div className="modal-card glass">
            <div className="modal-header">
              <h2>Apply for Education Course</h2>
              <button className="icon-btn" onClick={() => setIsEducationOpen(false)}><X size={20} /></button>
            </div>
            
            <form onSubmit={handleApplyEducation} className="modal-form">
              <div className="form-group">
                <label>Course Name / Program Title</label>
                <input type="text" className="form-input" value={eduName} onChange={e => setEduName(e.target.value)} required placeholder="E.g., Advanced React Patterns" />
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label>Start Date</label>
                  <input type="date" className="form-input" value={eduStartDate} onChange={e => setEduStartDate(e.target.value)} required />
                </div>
                <div className="form-group">
                  <label>End Date</label>
                  <input type="date" className="form-input" value={eduEndDate} onChange={e => setEduEndDate(e.target.value)} required />
                </div>
              </div>

              <div className="form-group flex-align-center" style={{paddingTop: '1rem'}}>
                <input type="checkbox" id="cert-edu" checked={certificate} onChange={e => setCertificate(e.target.checked)} />
                <label htmlFor="cert-edu" style={{marginLeft: '0.5rem'}}>Requires / Provides Certificate</label>
              </div>

              <div className="modal-actions">
                <button type="button" className="btn-secondary" onClick={() => setIsEducationOpen(false)}>Cancel</button>
                <button type="submit" className="btn-primary">Apply</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default Internships;
